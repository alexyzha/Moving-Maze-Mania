using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class RunNewGame : MonoBehaviour
{
    [SerializeField] public Slider XSlider;
    [SerializeField] private Slider YSlider;
    [SerializeField] private Slider ShiftSlider;
    [SerializeField] private Slider BotSlider;
    [SerializeField] private Toggle CoinToggle;
    [SerializeField] private TMP_InputField Seed;
    // Start is called before the first frame update
    void Start()
    {
        XSlider.maxValue = 50;
        XSlider.minValue = 10;
        YSlider.maxValue = 50;
        YSlider.minValue = 10;
        ShiftSlider.maxValue = 100;
        ShiftSlider.minValue = 0;
        BotSlider.maxValue = 10;
        BotSlider.minValue = 1;
        ShiftSlider.minValue = 0;
        CoinToggle.isOn = false;
        XSlider.value = 10;
        YSlider.value = 10;
        ShiftSlider.value = 0;
        Seed.characterLimit = 100;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void RunNewGameWithConfig() {
        PlayerPrefs.SetInt("Width",(int)XSlider.value);
        PlayerPrefs.SetInt("Height",(int)YSlider.value);
        PlayerPrefs.SetInt("Shifts",(int)ShiftSlider.value);
        PlayerPrefs.SetFloat("BotSpeed",BotSlider.value);
        PlayerPrefs.SetInt("Coins",CoinToggle.isOn ? 1 : 0);
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
