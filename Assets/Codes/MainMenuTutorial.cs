using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuTutorial : MonoBehaviour
{
    private int fisrtTime = 0;
    public GameObject tutorial;
    public GameObject menu;
    void Start()
    {
        fisrtTime = PlayerPrefs.GetInt("Tutor", fisrtTime);
        if(fisrtTime == 0)
        {
            tutorial.SetActive(true);
            menu.SetActive(false);
            PlayerPrefs.SetInt("Tutor", 1);
        }
        else
        {
            tutorial.SetActive(false);
            menu.SetActive(true);
        }
    }
}
