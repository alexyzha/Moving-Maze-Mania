using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SettingsControl : MonoBehaviour
{
    /* FIELDS */
    [SerializeField] private Slider CamZoom;
    [SerializeField] private Slider CamFollow;

    // Start is called before the first frame update
    void Start()
    {
        CamZoom.maxValue = 40;
        CamZoom.minValue = 20;
        CamFollow.maxValue = 10;
        CamFollow.minValue = 2;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void GoToTitle()
    {
        SceneManager.LoadScene(sceneName: "Title");
        PlayerPrefs.SetFloat("CamZoom",CamZoom.value);
        PlayerPrefs.SetFloat("CamFollow",CamFollow.value);
    }
}
