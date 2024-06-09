using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class RoundTimer : MonoBehaviour
{
    [SerializeField]
    TextMeshProUGUI roundTimerText;
    public float roundTime = 120;
    float timer;
    public AudioSource timerSound;
    
    void Start()
    {
        timer = roundTime;
    }

    void Update()
    {
        timer -= Time.deltaTime;

        if(timer > 0)
        {
            if(timer < 10 && roundTimerText.text != FormatTime((int)Math.Ceiling(timer)))
            {
                roundTimerText.color = Color.red;
                timerSound.Play();
            }
            
            roundTimerText.text = FormatTime((int)Math.Ceiling(timer));
        }
        else
        {
            GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>().ResetControllerHapticsForAllControllers();
            SceneManager.LoadScene(2);
        }
    }

    public string FormatTime(int totalSeconds)
    {
        // Calculate minutes and seconds
        int minutes = totalSeconds / 60;
        int seconds = totalSeconds % 60;

        // Format the time as "MM:SS"
        string formattedTime = string.Format("{0:00}:{1:00}", minutes, seconds);

        return formattedTime;
    }
}
