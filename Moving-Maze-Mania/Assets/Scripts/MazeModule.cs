using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Xml.Serialization;
using Unity.Collections;
using Unity.Mathematics;
using Unity.PlasticSCM.Editor.WebApi;
using Unity.VisualScripting;
using UnityEngine;

public class MazeModule : MonoBehaviour
{
    [SerializeField] GameObject DefaultPrefab;
    public GameObject[,] Tiles;
    MazeHandler Control;
    private int GEN_WIDTH;
    private int GEN_HEIGHT;
    private int MAZE_WIDTH;
    private int MAZE_HEIGHT;

    private int SHIFT_COUNT;
    private int DO_COINS;
    private int SEED;


    // Start is called before the first frame update
    void Start()
    {
        GEN_WIDTH = PlayerPrefs.GetInt("Width");
        GEN_HEIGHT = PlayerPrefs.GetInt("Height");
        MAZE_WIDTH = GEN_WIDTH*2+1;
        MAZE_HEIGHT = GEN_HEIGHT*2+1;
        SHIFT_COUNT = PlayerPrefs.GetInt("Shfits");
        DO_COINS = PlayerPrefs.GetInt("Coins");
        SEED = PlayerPrefs.GetInt("Seed");
        Tiles = new GameObject[MAZE_WIDTH,MAZE_HEIGHT];
        // instantiate all maze tile game objects
        for(int i = 0; i < MAZE_WIDTH; ++i)
        {
            for(int j = 0; j < MAZE_HEIGHT; ++j)
            {
                Vector3 Position = new(i*2.08f,j*2.08f,0);
                Tiles[i,j] = Instantiate(DefaultPrefab,Position,Quaternion.identity);
            }
        }
        Control = new(GEN_WIDTH,GEN_HEIGHT,SEED,Tiles);
        // change camera position and size
        Vector3 CameraPosition = new(MAZE_WIDTH*2.08f/2-1.04f,MAZE_HEIGHT*2.08f/2-1.04f,-10);
        Camera.main.gameObject.transform.position = CameraPosition;
        Camera.main.orthographicSize = Math.Max(MAZE_HEIGHT,MAZE_WIDTH)+(7.0f*Math.Max(GEN_HEIGHT,GEN_WIDTH)/50.0f);
    }

    // Update is called once per frame
    void Update()
    {
        Control.Shift();
    }
}

public class Pair {
    public Pair() {
        this.x = 0;
        this.y = 0;
    }

    public Pair(int x, int y) {
        this.x = x;
        this.y = y;
    }

    public int x { get; set; }
    public int y { get; set; }
};

public class MazeHandler
{
    public byte[,] maze_frame;
    public GameObject[,] Tiles;
    private readonly System.Random rand;
    private readonly int x;
    private readonly int y;
    private readonly int boundary_x;
    private readonly int boundary_y;
    private int origin_x;
    private int origin_y;
    private readonly string WALL_LOC = "Tiles/Wall";
    private readonly string FLOOR_LOC = "Tiles/Floor";
    private const byte EMPTY = (byte)0x00;
    private const byte OBJECT = (byte)0x10;
    private const byte IN_WALK = (byte)0x20;
    private const byte IN_MAZE = (byte)0x40;
    private const byte WALL = (byte)0x80;
    
    // POINTER TO PARENT, BELOW ARE BITMASKS

    private const byte PARENTS = (byte)0x0f;
    private const byte NEG_Y = (byte)0x01;
    private const byte NEG_X = (byte)0x02;
    private const byte POS_Y = (byte)0x04;
    private const byte POS_X = (byte)0x08;
    private static readonly byte[] DIR_MASK = new byte[4] {NEG_Y,NEG_X,POS_Y,POS_X};
    private static readonly Pair[] DIRECTIONS = new Pair[4] {new(0,-2),new(-2,0),new(0,2),new(2,0)};

    public MazeHandler(int x_, int y_, int seed_, GameObject[,] Tiles_)
    {
        Tiles = Tiles_;
        maze_frame = new byte[x_*2+1,y_*2+1];
        rand = new System.Random(seed_);
        x = x_;
        y = y_;
        boundary_x = x*2+1;
        boundary_y = y*2+1;
        GenerateMaze();
        InitTree();
        SetTiles();
    }

    public void Shift()
    {
        int next_dir = GetNextIndex(origin_x,origin_y);
        Pair next = new(origin_x+DIRECTIONS[next_dir].x,origin_y+DIRECTIONS[next_dir].y);
        // since next is becoming the new origin, remove its parents first
        // if unable to be removed, end of iteration. return
        for(int i = 0; i < 4; ++i) {
            if((maze_frame[next.x,next.y] & (1<<i)) > 0)
            {
                // check if removing parent affects objects in middle
                Pair next_mid = GetMid(next.x,next.y,next.x+DIRECTIONS[i].x,next.y+DIRECTIONS[i].y);
                if((maze_frame[next_mid.x,next_mid.y] & OBJECT) > 0)
                {
                    return;
                }
                else
                {
                    maze_frame[next_mid.x,next_mid.y] = WALL;
                    SetRendererSprite(next_mid.x,next_mid.y,WALL_LOC);
                    maze_frame[next.x,next.y] &= (byte)(PARENTS ^ DIR_MASK[i]);
                }
            }
        }   
        Pair mid = GetMid(next,origin_x,origin_y);
        maze_frame[mid.x,mid.y] = EMPTY;
        SetRendererSprite(mid.x,mid.y,FLOOR_LOC);
        maze_frame[origin_x,origin_y] |= DIR_MASK[next_dir];
        origin_x = next.x;
        origin_y = next.y;
    }

    private void InitTree()
    {
        // set origin 
        origin_x = rand.Next(0,x)*2+1;
        origin_y = rand.Next(0,y)*2+1;
        // bfs to set pointers
        Queue<Pair> q = new();
        q.Enqueue(new Pair(origin_x,origin_y));
        while(q.Count > 0)
        {
            Pair cur = q.Dequeue();
            maze_frame[cur.x,cur.y] ^= IN_MAZE;
            for(int i = 0; i < 4; ++i)
            {
                int next_x = cur.x+DIRECTIONS[i].x;
                int next_y = cur.y+DIRECTIONS[i].y;
                Pair mid = GetMid(cur,next_x,next_y);
                if(!IsValid(next_x,next_y) || ((maze_frame[next_x,next_y] & IN_MAZE) == 0) || ((maze_frame[mid.x,mid.y] & WALL) > 0))
                {
                    continue;
                }
                // get mask for parent of next
                byte parent = DIR_MASK[(i+2)%4];
                maze_frame[next_x,next_y] |= parent;
                q.Enqueue(new Pair(next_x,next_y));
            }
        }
    }

    /*
        0b x000 xxxx
        masking 0x40 in maze
                0x20 in walk
    */
    private void GenerateMaze()
    {
        for(int i = 0; i < boundary_x; ++i)
        {
            for(int j = 0; j < boundary_y; ++j)
            {
                maze_frame[i,j] = (i&j&1) == 1 ? EMPTY : WALL;
            }
        }   
        int starting_x = rand.Next(0,x)*2+1;
        int starting_y = rand.Next(0,y)*2+1;
        int tile_count = x*y-1;
        maze_frame[starting_x,starting_y] = IN_MAZE;
        while(tile_count > 0)
        {
            tile_count -= RandomWalk();
        }
    }

    private int RandomWalk()
    {
        // init
        Stack<Pair> current_walk = new();
        int current_x = rand.Next(0,x)*2+1;
        int current_y = rand.Next(0,y)*2+1;
        while(maze_frame[current_x,current_y] != EMPTY)
        {
            current_x = rand.Next(0,x)*2+1;
            current_y = rand.Next(0,y)*2+1;
        }
        current_walk.Push(new Pair(current_x,current_y));
        // walk
        while(maze_frame[current_x,current_y] != IN_MAZE)
        {
            maze_frame[current_x,current_y] = IN_WALK;
            Pair next = GetNext(current_x,current_y);
            current_x = next.x;
            current_y = next.y;
            if(maze_frame[current_x,current_y] == IN_WALK)
            {
                while(!IsEqual(next,current_walk.Peek()))
                {
                    Pair top = current_walk.Pop();
                    maze_frame[top.x,top.y] = EMPTY;
                }
            }
            else
            {
                current_walk.Push(new Pair(current_x,current_y));
            }
        }
        // clean path
        int ret = current_walk.Count;
        while(current_walk.Count > 0)
        {
            Pair next = current_walk.Pop();
            maze_frame[next.x,next.y] = IN_MAZE;
            Pair mid = GetMid(next,current_x,current_y);
            maze_frame[mid.x,mid.y] = IN_MAZE;
            current_x = next.x;
            current_y = next.y;
        }
        return ret-1;
    }

    private void SetTiles()
    {
        for(int i = 0; i < boundary_x; ++i)
        {
            for(int j = 0; j < boundary_y; ++j)
            {
                SetRendererSprite(i,j,(maze_frame[i,j] == WALL ? WALL_LOC : FLOOR_LOC));
            }        
        }
    }
    
    /************************** ALL UTIL BELOW **************************/
    
    public void PrintMaze()
    {
        Debug.Log("\x1B[32mMaze:\n");
        for(int i = 0; i < boundary_x; ++i)
        {
            for(int j = 0; j < boundary_y; ++j)
            {
                Debug.Log(maze_frame[i,j] == 0x80 ? "0 " : "  ");
            }
            Debug.Log("\n");
        }
    }

    private Pair GetNext(int current_x, int current_y)
    {
        Pair next = new(-1,-1);
        while(!IsValid(next))
        {
            int dir = rand.Next(0,4);
            next.x = current_x+DIRECTIONS[dir].x;
            next.y = current_y+DIRECTIONS[dir].y;
        }
        return next;
    }

    private int GetNextIndex(int current_x, int current_y)
    {
        Pair next = new(-1,-1);
        int dir = -1;
        while(!IsValid(next))
        {
            dir = rand.Next(0,4);
            next.x = current_x+DIRECTIONS[dir].x;
            next.y = current_y+DIRECTIONS[dir].y;
        }
        return dir;
    }

    private bool IsEqual(Pair a, Pair b)
    {
        return (a.x == b.x) && (a.y == b.y);
    }

    private bool IsValid(Pair current)
    {
        return (current.x > 0) && 
               (current.y > 0) &&
               (current.x < boundary_x-1) &&
               (current.y < boundary_y-1);
    }

    private bool IsValid(int x, int y)
    {
        return (x > 0) && 
               (y > 0) &&
               (x < boundary_x-1) &&
               (y < boundary_y-1);
    }

    private Pair GetMid(Pair a, int bx, int by)
    {
        int dx = (a.x-bx)/2;
        int dy = (a.y-by)/2;
        return new Pair(a.x-dx,a.y-dy);
    }

    private Pair GetMid(int ax, int ay, int bx, int by)
    {
        int dx = (ax-bx)/2;
        int dy = (ay-by)/2;
        return new Pair(ax-dx,ay-dy);
    }

    private void SetRendererSprite(int x, int y, string path)
    {
        SpriteRenderer CurrentRenderer = Tiles[x,y].GetComponent<SpriteRenderer>();
        CurrentRenderer.sprite = Resources.Load<Sprite>(path);
    }

}