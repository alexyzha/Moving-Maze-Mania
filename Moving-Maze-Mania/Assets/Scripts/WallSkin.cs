using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WallSkin : MonoBehaviour
{
    /* FIELDS */
    [SerializeField] Image WallIcon;

    // Start is called before the first frame update
    void Start()
    {
        int w = PlayerPrefs.GetInt("WallIcon",0);
        SetIcon(w);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void DecrementWI()
    {
        int w = PlayerPrefs.GetInt("WallIcon");
        w = (w - 1 > -1) ? (w - 1) : 2;
        SetIcon(w);
    }

    public void IncrementWI()
    {
        int w = PlayerPrefs.GetInt("WallIcon");
        w = (w + 1 > 2) ? 0 : (w + 1);
        SetIcon(w);
    }

    void SetIcon(int n)
    {
        PlayerPrefs.SetInt("WallIcon",n);
        Image cur_img = WallIcon.GetComponent<Image>();
        cur_img.sprite = Resources.Load<Sprite>(BASE + n.ToString());
    }

    private static readonly string BASE = "Tiles/Wall/";
}
