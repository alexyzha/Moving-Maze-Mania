using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainGameScript : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Debug.Log($"Width: {PlayerPrefs.GetInt("Width")}");
        Debug.Log($"Height: {PlayerPrefs.GetInt("Height")}");
        Debug.Log($"Shifts: {PlayerPrefs.GetInt("Shifts")}");
        Debug.Log($"Coins: {PlayerPrefs.GetInt("Coins")}");
        Debug.Log($"Seed: {PlayerPrefs.GetInt("Seed")}");
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
