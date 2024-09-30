using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FloorSkin : MonoBehaviour
{
    /* FIELDS */
    [SerializeField] Image FloorIcon;

    // Start is called before the first frame update
    void Start()
    {
        int f = PlayerPrefs.GetInt("FloorIcon",0);
        SetIcon(f);    
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void DecrementFI()
    {
        int f = PlayerPrefs.GetInt("FloorIcon");
        f = (f - 1 > -1) ? (f - 1) : 2;
        SetIcon(f);
    }

    public void IncrementFI()
    {
        int f = PlayerPrefs.GetInt("FloorIcon");
        f = (f + 1 > 2) ? 0 : (f + 1);
        SetIcon(f);
    }

    void SetIcon(int n)
    {
        PlayerPrefs.SetInt("FloorIcon",n);
        Image cur_img = FloorIcon.GetComponent<Image>();
        cur_img.sprite = Resources.Load<Sprite>(BASE + n.ToString());
    }

    private static readonly string BASE = "Tiles/Floor/";
}
