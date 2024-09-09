using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class RunNewGame : MonoBehaviour
{
    [SerializeField] public Slider X_Slider;
    [SerializeField] private Slider Y_Slider;
    [SerializeField] private Slider Shift_Slider;
    [SerializeField] private Toggle Coin_Toggle;
    [SerializeField] private TMP_InputField Seed;
    // Start is called before the first frame update
    void Start()
    {
        X_Slider.maxValue = 50;
        X_Slider.minValue = 10;
        Y_Slider.maxValue = 50;
        Y_Slider.minValue = 10;
        Shift_Slider.maxValue = 100;
        Shift_Slider.minValue = 0;
        Coin_Toggle.isOn = false;
        X_Slider.value = 10;
        Y_Slider.value = 10;
        Shift_Slider.value = 0;
        Seed.characterLimit = 100;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void RunNewGameWithConfig() {
        PlayerPrefs.SetInt("Width",(int)X_Slider.value);
        PlayerPrefs.SetInt("Height",(int)Y_Slider.value);
        PlayerPrefs.SetInt("Shifts",(int)Shift_Slider.value);
        PlayerPrefs.SetInt("Coins",Coin_Toggle.isOn ? 1 : 0);
        PlayerPrefs.SetInt("Seed",Hash(Seed.text));
        SceneManager.LoadScene(sceneName: "CurGame");
    }

    int Hash(string str)
    {
        if(str.Length == 0)
        {
            return 0;
        } 
        long ret = 0;
        foreach(char c in str)
        {
            ret *= 10007;
            ret += c;
            ret %= (long)0x7fffffff;
        }
        return (int)ret;
    }
}
