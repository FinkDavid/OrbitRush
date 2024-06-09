using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class BorderWarpManager : MonoBehaviour
{
    public List<GameObject> blackHoles = new List<GameObject>();
    GameManager gameManager;
    public AnimationCurve absorptionCurve;
    public float absorptionDuration = 2.0f;

    void Start()
    {
        gameManager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>();
    }

    void OnTriggerExit(Collider other)
    {
        if(other.CompareTag("Player"))
        {
            int playerId = other.gameObject.GetComponent<PlayerInformation>().playerID;

            Vector3 playerPosition = gameManager.playerReferences[playerId].transform.position;

            GameObject nearestBlackHole = new GameObject();
            float minDistance = float.MaxValue;

            foreach(var blackHole in blackHoles)
            {
                float distance = Vector3.Distance(blackHole.transform.position, playerPosition);
                if(distance < minDistance)
                {
                    minDistance = distance;
                    nearestBlackHole = blackHole;
                }
            }
            
            //Found nearest blackhole, suck player into it
            gameManager.playerReferences[playerId].GetComponent<PlayerMovement>().AbsorbPlayer(nearestBlackHole, absorptionDuration, absorptionCurve);
        }
    }

    #if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Handles.color = Color.magenta;
        Handles.DrawWireDisc(transform.position, transform.up, GetComponent<SphereCollider>().radius, 3);
    }
    #endif
}
