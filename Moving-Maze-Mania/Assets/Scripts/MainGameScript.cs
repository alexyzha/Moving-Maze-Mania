using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainGameScript : MonoBehaviour
{
    /* FIELDS */
    [SerializeField] GameObject BKG;

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

    private static readonly string BKG_BASE = "GameBkg/";
}
