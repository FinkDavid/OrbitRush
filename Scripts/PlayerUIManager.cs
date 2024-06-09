using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UIElements;

public class PlayerUIManager : MonoBehaviour
{
    [SerializeField]
    GameObject[] pointTexts = new GameObject[4];
    int parentPlayerID;
    [SerializeField]
    GameObject parentPlayer;

    public void InitiatePlayerUI()
    {
        parentPlayerID = parentPlayer.GetComponent<PlayerInformation>().playerID;
        pointTexts[parentPlayerID].SetActive(true);
    }

    public void UpdatePointsText(int points)
    {
        pointTexts[parentPlayerID].GetComponent<TextMeshProUGUI>().text = points.ToString();
    }
}
