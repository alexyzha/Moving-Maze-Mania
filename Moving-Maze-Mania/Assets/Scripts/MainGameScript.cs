using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainGameScript : MonoBehaviour
{
    /* FIELDS */
    [SerializeField] GameObject BKG;
    [SerializeField] Button CameraButton;

    // Start is called before the first frame update
    void Start()
    {
        int b = PlayerPrefs.GetInt("BackgroundIcon",0);
        SpriteRenderer cur_img = BKG.GetComponent<SpriteRenderer>();
        cur_img.sprite = Resources.Load<Sprite>(BKG_BASE + b.ToString() + "B");
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SwitchCamIcon()
    {
        Image cur_img = CameraButton.GetComponent<Image>();
        cur_img.sprite = Resources.Load<Sprite>(ZOOMED_IN ? ZOOM_IN_LOC : ZOOM_OUT_LOC);
        ZOOMED_IN = !ZOOMED_IN;
    }   

    private bool ZOOMED_IN = true;
    private static readonly string BKG_BASE = "GameBkg/";
    private static readonly string ZOOM_OUT_LOC = "Buttons/ZoomOut";
    private static readonly string ZOOM_IN_LOC = "Buttons/ZoomIn";
}
