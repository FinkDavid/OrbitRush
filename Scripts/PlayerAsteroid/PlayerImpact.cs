using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

public class PlayerImpact : MonoBehaviour
{
    PlayerMovement playerMovement;
    RespawnManager respawnManager;
    
    [SerializeField] public ParticleSystem explosionParticles;
    [SerializeField] public AudioSource explosionSound;
    
    void Start()
    {
        playerMovement = GetComponent<PlayerMovement>();
        respawnManager = GameObject.FindGameObjectWithTag("RespawnManager").GetComponent<RespawnManager>();
    }

    void OnCollisionEnter(Collision collision)
    {
        if(!collision.collider.gameObject.CompareTag("Player"))
        {
            return;
        }

        PlayerMovement other = collision.gameObject.GetComponent<PlayerMovement>();
        if(!other.GetComponent<PlayerInformation>().IsDead && !gameObject.GetComponent<PlayerInformation>().IsDead)
        {
            if(other.GetImpactFactor() > playerMovement.GetImpactFactor())
            {
                other.ChangeSpeed(0.75f);
                other.ChangeMass(0.75f);
                explosionParticles.Play();
                explosionSound.Play();

                respawnManager.RespawnPlayer(GetComponent<PlayerInformation>().playerID);

                ScoreManager.Instance.AddPoints(1, other.GetComponent<PlayerInformation>().playerID);
            }
            else
            {
                playerMovement.ChangeSpeed(0.75f);
                playerMovement.ChangeMass(0.75f);

                //Respawn other player
                other.gameObject.GetComponent<PlayerImpact>().explosionParticles.Play();
                other.gameObject.GetComponent<PlayerImpact>().explosionSound.Play();

                respawnManager.RespawnPlayer(other.GetComponent<PlayerInformation>().playerID);
                
                ScoreManager.Instance.AddPoints(1, playerMovement.GetComponent<PlayerInformation>().playerID);
            }
        }
    }
}
