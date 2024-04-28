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
        Time.timeScale = 0f;
        sounds.PlayOneShot(click);
        panel.SetActive(true);
        Interface.SetActive(false);
    }

    // Update is called once per frame
    public void ClosePanel(GameObject panel)
    {
        Time.timeScale = 1f;
        sounds.PlayOneShot(click);
        panel.SetActive(false);
        Interface.SetActive(true);
    }
    public void OpenScene(string scene)
    {
        sounds.PlayOneShot(click);
        SceneManager.LoadScene(scene);
    }
    public void FlyLevel(int level)
    {
        float[] points_needed = {30f, 40f, 50f, 60f, 70f, 80f, 90f, 100f, 110f, 120f};
        int[] meteor_speed = {4,5,6,7,8,9,10,11,12,13};
        float[] meteor_frequency = {2f,1.75f,1.5f,1.25f,1f,0.75f,0.6f,0.5f,0.4f,0.3f};
        float[] bg_speed = {2f, 2.5f, 3f, 3.5f, 4f, 4.5f, 5f, 5.5f,6f, 6.5f};
        PlayerPrefs.SetFloat("Target score", points_needed[level]);
        PlayerPrefs.SetInt("Target speed", meteor_speed[level]);
        PlayerPrefs.SetFloat("Target frequency", meteor_frequency[level]);
        PlayerPrefs.SetFloat("Target scroll", bg_speed[level]);
    }
}
