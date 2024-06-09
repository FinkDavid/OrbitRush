using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class DashIndicatorManager : MonoBehaviour
{
    [FormerlySerializedAs("indicators")] [SerializeField]
    public DashIndicator[] dashIndicators = new DashIndicator[4];

    [SerializeField] private PlayerInformation playerInformation;
    //GameManager gameManager;
    int layerOffset = 6;

    private void Start()
    {
        //gameManager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>();

        // playerInformation.playerDeath += myPlayer =>
        // {
        //     foreach (var dashIndicator in dashIndicators)
        //     {
        //         if (dashIndicator.gameObject.activeSelf)
        //         {
        //             dashIndicator.gameObject.SetActive(false);
        //         }
        //     }
        // };
    }
    
    public void AddIndicator(int playerID, Transform target)
    {
        dashIndicators[playerID].SetTarget(target);

        //Add the indicator to this players specific layer (for each child model) so other cameras dont render it
        int thisPlayersID = playerInformation.playerID;
        DashIndicator dashIndicator = dashIndicators[playerID];
        
        for(int i = 0; i < dashIndicator.transform.childCount; i++)
        {
            dashIndicator.transform.GetChild(i).gameObject.layer = thisPlayersID + layerOffset;
        }
    }
}
