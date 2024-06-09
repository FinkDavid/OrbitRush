using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScoreManager
{
    private static ScoreManager instance = null;

    private ScoreManager()
    {

    }

    public static ScoreManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new ScoreManager();
            }
            return instance;
        }
    }

    List<int> playerScores = new List<int>();

    public void AddPoints(int amount, int playerID)
    {
        playerScores[playerID] += amount;
        PlayerInformation playerInformation = GameObject.FindGameObjectWithTag("GameManager")
                                                        .GetComponent<GameManager>().playerReferences[playerID].GetComponent<PlayerInformation>();
        playerInformation.playerCanvas.GetComponent<PlayerUIManager>().UpdatePointsText(playerScores[playerID]);
        playerInformation.pointIndicatorManager.SpawnPointIndicator(true);
    }

    public void RemovePoints(int amount, int playerID)
    {
        PlayerInformation playerInformation = GameObject.FindGameObjectWithTag("GameManager")
                                                        .GetComponent<GameManager>().playerReferences[playerID].GetComponent<PlayerInformation>();
        if(playerScores[playerID] - amount < 0)
        {
            playerScores[playerID] = 0;
        }
        else
        {
            playerScores[playerID] -= amount;
            playerInformation.pointIndicatorManager.SpawnPointIndicator(false);
        }

        playerInformation.playerCanvas.GetComponent<PlayerUIManager>().UpdatePointsText(playerScores[playerID]);
    }

    public void Reset()
    {
        playerScores = new List<int>();
    }

    public int GetScore(int playerID)
    {
        return playerScores[playerID];
    }

    public void AddPlayer()
    {
        playerScores.Add(0);
    }
}
