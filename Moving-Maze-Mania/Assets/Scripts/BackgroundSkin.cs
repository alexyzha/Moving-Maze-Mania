using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BackgroundSkin : MonoBehaviour
{
    /* FIELDS */
    [SerializeField] Image BackgroundIcon;

    // Start is called before the first frame update
    void Start()
    {
        int b = PlayerPrefs.GetInt("BackgroundIcon",0);
        SetIcon(b);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void DecrementBI()
    {
        int b = PlayerPrefs.GetInt("BackgroundIcon");
        b = (b - 1 > -1) ? (b - 1) : 2;
        SetIcon(b);
    }

    public void IncrementBI()
    {
        int b = PlayerPrefs.GetInt("BackgroundIcon");
        b = (b + 1 > 2) ? 0 : (b + 1);
        SetIcon(b);
    }

    void SetIcon(int n)
    {
        PlayerPrefs.SetInt("BackgroundIcon",n);
        Image cur_img = BackgroundIcon.GetComponent<Image>();
        cur_img.sprite = Resources.Load<Sprite>(BASE + n.ToString());
    }

    private static readonly string BASE = "GameBkg/";
}