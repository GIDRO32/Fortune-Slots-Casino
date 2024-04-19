using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PanelHandler : MonoBehaviour
{
    public GameObject Interface;
    public GameObject[] panels;
    public AudioSource sounds;
    public AudioClip click;

    void Start()
    {
        for(int i = 0; i < panels.Length; i++)
        {
            panels[i].SetActive(false);
        }
    }
    // Start is called before the first frame update
    public void OpenPanel(GameObject panel)
    {
        sounds.PlayOneShot(click);
        panel.SetActive(true);
        Interface.SetActive(false);
    }

    // Update is called once per frame
    public void ClosePanel(GameObject panel)
    {
        sounds.PlayOneShot(click);
        panel.SetActive(false);
        Interface.SetActive(true);
    }
    public void OpenScene(string scene)
    {
        sounds.PlayOneShot(click);
        SceneManager.LoadScene(scene);
    }
}
