using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Drawing;
using System.Threading;
using System.Xml.Serialization;
using Unity.Collections;
using Unity.Mathematics;
using Unity.PlasticSCM.Editor.WebApi;
using Unity.VisualScripting;
using UnityEditor;
using UnityEditor.U2D.Animation;
using UnityEngine;
using UnityEngine.UI;

public class Pair
{
    public Pair()
    {
        this.x = 0;
        this.y = 0;
    }

    public Pair(int x, int y)
    {
        this.x = x;
        this.y = y;
    }

    public int x { get; set; }
    public int y { get; set; }
};

public class MazeModule : MonoBehaviour
{

    /************************** ALL FIELDS BELOW **************************/

    [SerializeField] GameObject DefaultPrefab;
    [SerializeField] GameObject ObjectPrefab;
    [SerializeField] GameObject OPPrefab;
    
    /************************** ALL GAME LOOP BELOW **************************/

    OP testopclass;
    GameObject OPObj;

    void Start()
    {
        // RNG and seeding need to be init first
        SEED = PlayerPrefs.GetInt("Seed");
        rand = SEED > 0 ? new System.Random(SEED) : new System.Random();
        // Maze node and actual x/y values
        x = PlayerPrefs.GetInt("Width");
        y = PlayerPrefs.GetInt("Height");
        MAZE_WIDTH = x*2+1;
        MAZE_HEIGHT = y*2+1;
        
        // NEED GOOD PLAYER SPAWN (AND MAZE END) ALGO
        player_x = 1;
        player_y = 1;
        // ^^^^^^

        // Origin for shifts spawns around the middle of the maze
        origin_x = rand.Next(x/4,x*3/4)*2+1;
        origin_y = rand.Next(y/4,y*3/4)*2+1;
        // Setting up variables for coins and shifts
        DO_COINS = PlayerPrefs.GetInt("Coins");
        coins_picked = 0;
        SHIFT_COUNT = PlayerPrefs.GetInt("Shifts");
        // Get bot speed
        BOT_SPEED = PlayerPrefs.GetFloat("BotSpeed");
        // Init all matricies
        MazeFrame = new byte[MAZE_WIDTH,MAZE_HEIGHT];
        Tiles = new GameObject[MAZE_WIDTH,MAZE_HEIGHT];
        NPObjects = new GameObject[MAZE_WIDTH,MAZE_HEIGHT];
        NPObjectData = new char[MAZE_WIDTH,MAZE_HEIGHT];
        // Generate maze and set tiles, maze data stored in MazeFrame
        GenerateMaze();
        InitTree();
        SetTiles();
        // Spawn player sprite
        Vector3 PlayerPos = new(player_x*2.08f,player_y*2.08f,0);
        Player = Instantiate(ObjectPrefab,PlayerPos,Quaternion.identity);
        // If coins are allowed, set coins
        if(DO_COINS > 0)
        {
            SetCoins();
        }

        // TESTING BOTS        
        OPObj = Instantiate(OPPrefab,PlayerPos,Quaternion.identity);
        testopclass = new OP(MazeFrame,1,1,COIN_0_LOC,OPObj);

        // Init camera as follow
        Vector3 CameraPosition = new(MAZE_WIDTH*2.08f/2-1.04f,MAZE_HEIGHT*2.08f/2-1.04f,-10);
        Camera.main.gameObject.transform.position = CameraPosition;
        Camera.main.orthographicSize = Math.Max(MAZE_HEIGHT,MAZE_WIDTH)+(7.0f*Math.Max(y,x)/50.0f);
        Camera.main.orthographicSize /= ZOOM_FACTOR;
    }

    void Update()
    {
        // All player movement controls
        if(Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
        {
            MovePlayer(0);
        }
        else if(Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
        {
            MovePlayer(1);
        }
        else if(Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
        {
            MovePlayer(2);
        }
        else if(Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
        {
            MovePlayer(3);
        }
        // Swap camera perspective hotkey
        if(Input.GetKeyDown(KeyCode.R))
        {
            ToggleCamera();
        }
        // Move camera if set to follow
        if (CameraFollow)
        {
            Vector3 cameraPos = Camera.main.gameObject.transform.position;
            cameraPos += (Player.transform.position - Camera.main.gameObject.transform.position) * Time.deltaTime * FOLLOW_SPEED;
            cameraPos.z = -10;
            Camera.main.gameObject.transform.position = cameraPos;
        }
        
        // Move bots (TESTING)
        curr_cooldown += Time.deltaTime*BOT_SPEED;
        if (curr_cooldown > BOT_COOLDOWN) 
        {
            curr_cooldown = 0;
            testopclass.MoveTowards(19,19);
        }
    }

    /************************** ALL GAME CONTROL BELOW **************************/

    public void ToggleCamera() 
    {
        if (CameraFollow) 
        {
            CameraFollow = false;
            Camera.main.gameObject.transform.position = new(MAZE_WIDTH*2.08f/2-1.04f,MAZE_HEIGHT*2.08f/2-1.04f,-10);
            Camera.main.orthographicSize *= ZOOM_FACTOR;
        } 
        else 
        {
            CameraFollow = true;
            Camera.main.orthographicSize /= ZOOM_FACTOR;
            Vector3 cameraPos = Player.transform.position;
            cameraPos.z = -10;
            Camera.main.gameObject.transform.position = cameraPos;
        }
    }

    private bool MovePlayer(int dir)
    {
        // Check direction validity (WALL + OOB)
        int nx = player_x+PLAYER_DIR[dir].x;
        int ny = player_y+PLAYER_DIR[dir].y;
        if(!IsValid(nx,ny) || MazeFrame[nx,ny] == WALL)
        {
            return false;
        }
        // Remove object flag from current tile
        MazeFrame[player_x,player_y] &= NOT_OBJECT;
        player_x = nx;
        player_y = ny;
        // Set object flag in new tile
        MazeFrame[player_x,player_y] |= OBJECT;
        Player.transform.position += VEC_DIR[dir];
        // Collecting coins
        if(NPObjectData[player_x,player_y] == IS_COIN)
        {
            ++coins_picked;
            GameObject.Destroy(NPObjects[player_x,player_y]);
            NPObjectData[player_x,player_y] = OBJ_EMPTY;
        }
        // Shift after every move
        for(int c = 0; c < SHIFT_COUNT; ++c)
        {
            Shift();
        }
        return true;
    }

    /************************** ALL GAME UTIL BELOW **************************/
    
    private void Shift()
    {
        int next_dir = GetNextIndex(origin_x,origin_y);
        Pair next = new(origin_x+DIRECTIONS[next_dir].x,origin_y+DIRECTIONS[next_dir].y);
        // Since next is becoming the new origin, remove its parents first
        // If unable to be removed, end of iteration. Continue to next shift (return)
        for(int i = 0; i < 4; ++i) {
            if((MazeFrame[next.x,next.y] & (1<<i)) > 0)
            {
                // Check if removing parent affects objects in middle
                Pair next_mid = GetMid(next.x,next.y,next.x+DIRECTIONS[i].x,next.y+DIRECTIONS[i].y);
                if((MazeFrame[next_mid.x,next_mid.y] & OBJECT) > 0)
                {
                    return;
                }
                // If it doesn't, then close off the tile between cur and next with a wall
                else
                {
                    MazeFrame[next_mid.x,next_mid.y] = WALL;
                    SetRendererSprite(next_mid.x,next_mid.y,WALL_LOC);
                    MazeFrame[next.x,next.y] &= (byte)(PARENTS ^ DIR_MASK[i]);
                }
            }
        }   
        // Tile between origin and next is set to empty, as current origin will point to next
        Pair mid = GetMid(next,origin_x,origin_y);
        MazeFrame[mid.x,mid.y] = EMPTY;
        SetRendererSprite(mid.x,mid.y,FLOOR_LOC);
        // Mask for pointer on current origin, then change origin coords to next
        MazeFrame[origin_x,origin_y] |= DIR_MASK[next_dir];
        origin_x = next.x;
        origin_y = next.y;
    }
    
    private void SetRendererSprite(int x, int y, string path)
    {
        SpriteRenderer CurrentRenderer = Tiles[x,y].GetComponent<SpriteRenderer>();
        CurrentRenderer.sprite = Resources.Load<Sprite>(path);
    }

    /************************** ALL MAZE GEN BELOW **************************/

    private void GenerateMaze()
    {
        // Init with only nodes empty
        for(int i = 0; i < MAZE_WIDTH; ++i)
        {
            for(int j = 0; j < MAZE_HEIGHT; ++j)
            {
                MazeFrame[i,j] = (i&j&1) == 1 ? EMPTY : WALL;
            }
        }   
        // Random starting point
        int starting_x = rand.Next(0,x)*2+1;
        int starting_y = rand.Next(0,y)*2+1;
        int tile_count = x*y-1;
        MazeFrame[starting_x,starting_y] = IN_MAZE;
        // While nodes are left out:
        while(tile_count > 0)
        {
            tile_count -= RandomWalk();
        }
    }

    private int RandomWalk()
    {
        // Init
        Stack<Pair> current_walk = new();
        int current_x = rand.Next(0,x)*2+1;
        int current_y = rand.Next(0,y)*2+1;
        while(MazeFrame[current_x,current_y] != EMPTY)
        {
            current_x = rand.Next(0,x)*2+1;
            current_y = rand.Next(0,y)*2+1;
        }
        current_walk.Push(new Pair(current_x,current_y));
        // Walk
        while(MazeFrame[current_x,current_y] != IN_MAZE)
        {
            MazeFrame[current_x,current_y] = IN_WALK;
            Pair next = GetNext(current_x,current_y);
            current_x = next.x;
            current_y = next.y;
            if(MazeFrame[current_x,current_y] == IN_WALK)
            {
                while(!IsEqual(next,current_walk.Peek()))
                {
                    Pair top = current_walk.Pop();
                    MazeFrame[top.x,top.y] = EMPTY;
                }
            }
            else
            {
                current_walk.Push(new Pair(current_x,current_y));
            }
        }
        // Clean path
        int ret = current_walk.Count;
        while(current_walk.Count > 0)
        {
            Pair next = current_walk.Pop();
            MazeFrame[next.x,next.y] = IN_MAZE;
            Pair mid = GetMid(next,current_x,current_y);
            MazeFrame[mid.x,mid.y] = IN_MAZE;
            current_x = next.x;
            current_y = next.y;
        }
        return ret-1;
    }

    private void InitTree()
    {
        // BFS to set pointers
        Queue<Pair> q = new();
        q.Enqueue(new Pair(origin_x,origin_y));
        while(q.Count > 0)
        {
            Pair cur = q.Dequeue();
            // Mask for whether or not tile is seen in BFS
            MazeFrame[cur.x,cur.y] ^= IN_MAZE;
            for(int i = 0; i < 4; ++i)
            {
                int next_x = cur.x+DIRECTIONS[i].x;
                int next_y = cur.y+DIRECTIONS[i].y;
                Pair mid = GetMid(cur,next_x,next_y);
                if(!IsValid(next_x,next_y) || ((MazeFrame[next_x,next_y] & IN_MAZE) == 0) || ((MazeFrame[mid.x,mid.y] & WALL) > 0))
                {
                    continue;
                }
                // Get mask for parent of next
                byte parent = DIR_MASK[(i+2)%4];
                MazeFrame[next_x,next_y] |= parent;
                q.Enqueue(new Pair(next_x,next_y));
            }
        }
    }

    private void SetTiles()
    {
        for(int i = 0; i < MAZE_WIDTH; ++i)
        {
            for(int j = 0; j < MAZE_HEIGHT; ++j)
            {
                Vector3 Position = new(2.08f*i,2.08f*j,0);
                Tiles[i,j] = Instantiate(DefaultPrefab,Position,Quaternion.identity);
                SetRendererSprite(i,j,MazeFrame[i,j] == WALL ? WALL_LOC : FLOOR_LOC);
            }        
        }
    }

    private void SetCoins()
    {
        int count = Math.Min(MAZE_WIDTH,MAZE_HEIGHT);
        for(int i = 0; i < count; ++i)
        {
            int nx = rand.Next(0,x)*2+1;
            int ny = rand.Next(0,y)*2+1;
            while(NPObjectData[nx,ny] != 0 || (MazeFrame[nx,ny] & OBJECT) > 0) 
            {
                nx = rand.Next(0,x)*2+1;
                ny = rand.Next(0,y)*2+1;
            }
            NPObjectData[nx,ny] = IS_COIN;
            Vector3 Position = new(nx*2.08f,ny*2.08f,0);
            NPObjects[nx,ny] = Instantiate(ObjectPrefab,Position,Quaternion.identity);
            SpriteRenderer CurrentRenderer = NPObjects[nx,ny].GetComponent<SpriteRenderer>();
            CurrentRenderer.sprite = Resources.Load<Sprite>(COIN_0_LOC);
        }
    }
    
    /************************** ALL MAZE UTIL BELOW **************************/

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
               (current.x < MAZE_WIDTH-1) &&
               (current.y < MAZE_HEIGHT-1);
    }

    private bool IsValid(int x, int y)
    {
        return (x > 0) && 
               (y > 0) &&
               (x < MAZE_WIDTH-1) &&
               (y < MAZE_HEIGHT-1);
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

    /************************** ALL VARS BELOW **************************/

    private int SEED;
    private System.Random rand;
    private byte[,] MazeFrame;
    private GameObject Player;
    private GameObject[,] Tiles;
    private GameObject[,] NPObjects;
    private char[,] NPObjectData;
    private int x;
    private int y;
    private int MAZE_WIDTH;
    private int MAZE_HEIGHT;
    private int origin_x;
    private int origin_y;
    private int player_x;
    private int player_y;
    private int DO_COINS;
    private int coins_picked;
    private float curr_cooldown = 0.0f;
    private int SHIFT_COUNT;
    private float BOT_SPEED;
    private float BOT_COOLDOWN = 1.0f;
    private bool CameraFollow = true;
    private int ZOOM_FACTOR = 3;
    private float FOLLOW_SPEED = 2;

    /************************** ALL CONST/MASKS BELOW **************************/

    private const byte EMPTY = 0x00;
    private const byte OBJECT = 0x10;
    private const byte NOT_OBJECT = 0xef;
    private const byte IN_WALK = 0x20;
    private const byte IN_MAZE = 0x40;
    private const byte WALL = 0x80;
    private const byte PARENTS = 0x0f;
    private const byte NEG_Y = 0x01;
    private const byte NEG_X = 0x02;
    private const byte POS_Y = 0x04;
    private const byte POS_X = 0x08;
    private const char IS_COIN = (char)0x0001;
    private const char OBJ_EMPTY = (char)0x0000;
    private static readonly string COIN_0_LOC = "Items/Coin/0";
    private static readonly string WALL_LOC = "Tiles/Wall";
    private static readonly string FLOOR_LOC = "Tiles/Floor";
    private static readonly byte[] DIR_MASK = new byte[4] {NEG_Y,NEG_X,POS_Y,POS_X};
    private static readonly Pair[] DIRECTIONS = new Pair[4] {new(0,-2),new(-2,0),new(0,2),new(2,0)};
    private static readonly Pair[] PLAYER_DIR =  new Pair[4] {new(0,1),new(-1,0),new(0,-1),new(1,0)};
    private static readonly Vector3[] VEC_DIR = new Vector3[4] {new(0,2.08f,0),new(-2.08f,0,0),new(0,-2.08f,0),new(2.08f,0,0)};

}

public class OP {

    /************************** CONSTRUCTOR **************************/

    public OP(byte[,] MF, int x_, int y_, string skin, GameObject OP) {
        MazeFrame = MF;
        x = x_;
        y = y_;
        MAZE_WIDTH = MazeFrame.GetLength(0);
        MAZE_HEIGHT = MazeFrame.GetLength(1);
        SKIN = skin;
        MazeFrame[x,y] |= OBJECT;
        Vector3 SpawnPos = new(x*2.08f,y*2.08f,0);
        OPObject = OP;
    }

    /************************** ALL FUNCTIONS BELOW **************************/

    public void MoveTowards(int tx, int ty) {
        bool[,] vis = new bool[MAZE_WIDTH,MAZE_HEIGHT];
        vis[x,y] = true;
        // Find which direction target is in (only 1 dir since tree)
        for(int i = 0; i < 4; ++i) {
            int nx = x+OP_DIR[i].x;
            int ny = y+OP_DIR[i].y;
            if(DFS(tx,ty,nx,ny,vis))
            {
                MazeFrame[x,y] &= NOT_OBJECT;
                x = nx;
                y = ny;
                MazeFrame[x,y] |= OBJECT;
                OPObject.transform.position += VEC_DIR[i];    
                break;
            }
        }
    }

    private bool DFS(int tx, int ty, int cx, int cy, bool[,] vis) {
        if(!IsValid(cx,cy) || vis[cx,cy] || MazeFrame[cx,cy] == WALL)
        {
            return false;
        }
        if(cx == tx && cy == ty)
        {
            return true;
        }
        vis[cx,cy] = true;
        bool ret = false;
        for(int i = 0; i < 4; ++i)
        {
            int nx = cx+OP_DIR[i].x;
            int ny = cy+OP_DIR[i].y;
            ret = ret || DFS(tx,ty,nx,ny,vis);
        }
        return ret;
    }

    /************************** ALL UTIL BELOW **************************/

    private bool IsValid(int x, int y)
    {
        return (x > 0) && 
               (y > 0) &&
               (x < MAZE_WIDTH-1) &&
               (y < MAZE_HEIGHT-1);
    }

    /************************** ALL VARS BELOW **************************/

    private byte[,] MazeFrame;
    private GameObject OPObject;
    private int x;
    private int y;
    private int MAZE_WIDTH;
    private int MAZE_HEIGHT;
    private const byte WALL = 0x80;
    private const byte OBJECT = 0x10;
    private const byte NOT_OBJECT = 0xef;
    private readonly string SKIN;
    private static readonly Pair[] OP_DIR =  new Pair[4] {new(0,1),new(-1,0),new(0,-1),new(1,0)};
    private static readonly Vector3[] VEC_DIR = new Vector3[4] {new(0,2.08f,0),new(-2.08f,0,0),new(0,-2.08f,0),new(2.08f,0,0)};
}