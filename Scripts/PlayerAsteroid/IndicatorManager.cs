using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IndicatorManager : MonoBehaviour
{
    [SerializeField]
    GameObject[] indicators = new GameObject[4];
    GameManager gameManager;
    int layerOffset = 6;

    void Awake()
    {
        gameManager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>();
    }

    public void AddIndicator(int playerID, Transform target)
    {
        indicators[playerID].SetActive(true);
        indicators[playerID].GetComponent<Indicator>().SetTarget(target);

        //Add the indicator to this players specific layer (for each child model) so other cameras dont render it
        int thisPlayersID = GetComponent<PlayerInformation>().playerID;
        GameObject indicator = indicators[playerID];
        
        for(int i = 0; i < indicator.transform.childCount; i++)
        {
            indicator.transform.GetChild(i).gameObject.layer = thisPlayersID + layerOffset;
        }
    }

    public void SetIndicatorsActive(bool active)
    {
        for(int i = 0; i < gameManager.playerReferences.Length; i++)
        {
            if(i != GetComponent<PlayerInformation>().playerID)
            {
                indicators[i].SetActive(active);
            }
        }
    }
}
