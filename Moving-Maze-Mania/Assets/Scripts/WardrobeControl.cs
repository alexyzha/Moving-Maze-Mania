using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class WardrobeControl : MonoBehaviour
{
    /* FIELDS */
    [SerializeField] TMP_Text TotalScore;

    // Start is called before the first frame update
    void Start()
    {
        TotalScore.text = $"Total Score: {PlayerPrefs.GetFloat("TotalScore",0.0f)}";
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void GoToTitle()
    {
        SceneManager.LoadScene(sceneName: "Title");
    }
}
