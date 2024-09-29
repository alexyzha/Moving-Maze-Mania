using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class PlayerSkin : MonoBehaviour
{
    /* FIELDS */
    [SerializeField] Image PlayerIcon;

    // Start is called before the first frame update
    void Start()
    {
        int p = PlayerPrefs.GetInt("PlayerIcon",5);
        SetIcon(p);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void DecrementPI()
    {
        int p = PlayerPrefs.GetInt("PlayerIcon");
        p = (p - 1 > -1) ? (p - 1) : 17;
        SetIcon(p);
    }

    public void IncrementPI()
    {
        int p = PlayerPrefs.GetInt("PlayerIcon");
        p = (p + 1 > 17) ? 0 : (p + 1);
        SetIcon(p);
    }

    void SetIcon(int n)
    {
        PlayerPrefs.SetInt("PlayerIcon",n);
        Image cur_img = PlayerIcon.GetComponent<Image>();
        cur_img.sprite = Resources.Load<Sprite>(BASE + n.ToString());
    }

    private static readonly string BASE = "Player/";
}
