using System.Collections;
using System.Collections.Generic;
using UnityEditor.SearchService;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class StartGame : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void NewGame()
    {
        SceneManager.LoadScene(sceneName: "NG_Config");
    }

    public void GoToSettings()
    {
        SceneManager.LoadScene(sceneName: "Settings");
    }

    public void GoToWardrobe()
    {
        SceneManager.LoadScene(sceneName: "Wardrobe");
    }

    public void GoToCredits()
    {
        SceneManager.LoadScene(sceneName: "Credits");
    }
    public void GoToInfo()
    {
        SceneManager.LoadScene(sceneName: "Info");
    }

}
