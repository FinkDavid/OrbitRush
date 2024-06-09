using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlanetCollisionCheck : MonoBehaviour
{
    void OnTriggerEnter(Collider collider)
    {
        if(collider.gameObject.CompareTag("Player") && collider.gameObject.GetComponent<PlayerInformation>().IsDead == false)
        {
            PlayerInformation playerInformation = collider.gameObject.GetComponent<PlayerInformation>();
            ScoreManager.Instance.RemovePoints(1, playerInformation.playerID);
            GameObject.FindGameObjectWithTag("RespawnManager").GetComponent<RespawnManager>().RespawnPlayer(playerInformation.playerID);
        }
    }
}
