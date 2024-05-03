using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Leaderboards : MonoBehaviour
{
       private float spaceship_highscore;
    public Text fly_record;
    void Start()
    {
        spaceship_highscore = PlayerPrefs.GetFloat("Fly Record", spaceship_highscore);
    }

    // Update is called once per frame
    void Update()
    {
        fly_record.text = "Highscore:\n"+spaceship_highscore.ToString("F0");
    }
    public void ResetScores()
    {
        spaceship_highscore = 0;
        PlayerPrefs.SetFloat("Fly Record", spaceship_highscore);
    }
}
