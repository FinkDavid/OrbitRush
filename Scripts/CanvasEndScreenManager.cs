using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine.SceneManagement;

public class CanvasEndScreenManager : MonoBehaviour
{
    public GameObject[] playerDisplays = new GameObject[4];
    GameManager gameManager;

    public TextMeshProUGUI winnerText;

    void Start()
    {
        gameManager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>();

        Dictionary<int, int> playerScores = new Dictionary<int, int>();

        for(int i = 0; i < gameManager.playerReferences.Length; i++)
        {
            playerDisplays[i].SetActive(true);
            int score = ScoreManager.Instance.GetScore(i);
            playerScores.Add(i, score);
            playerDisplays[i].transform.GetChild(0).GetChild(0).GetComponent<TextMeshProUGUI>().text = score.ToString();
        }

        var sortedDict = playerScores.OrderByDescending(x => x.Value).ToList();

        string winText = "";

        if(sortedDict.Count < 1)
        {
            winText = "RED WON!";
            winnerText.text = winText;
            return;
        }

        if(sortedDict[0].Value == sortedDict[1].Value)
        {
            winText = "ITS A DRAW!";
        }
        else
        {
            switch(sortedDict[0].Key)
            {
                case 0: winText = "RED WON!"; break;
                case 1: winText = "GREEN WON!"; break;
                case 2: winText = "YELLOW WON!"; break;
                case 3: winText = "BLUE WON!"; break;
            }
        }

        winnerText.text = winText;
    }

    public void RestartGame()
    {
        ScoreManager.Instance.Reset();
        gameManager.playerReferences = new GameObject[4];
        SceneManager.LoadScene("MainMenu");
    }
}
