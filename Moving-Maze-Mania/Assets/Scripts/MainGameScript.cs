using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainGameScript : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Debug.Log(PlayerPrefs.GetInt("Width"));
        Debug.Log(PlayerPrefs.GetInt("Height"));
        Debug.Log(PlayerPrefs.GetInt("Shifts"));
        Debug.Log(PlayerPrefs.GetInt("Coins"));
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
