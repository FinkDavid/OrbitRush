using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class AsteroidImpact : MonoBehaviour
{
    [SerializeField] public PlayerMovement playerMovement;
    [SerializeField] public AudioSource munchSound;

    private void OnTriggerEnter(Collider other)
    {
        if(!other.CompareTag("Asteroid")) return;
        
        playerMovement.AddMass(0.2f);
        
        Destroy(other.gameObject);
        
        munchSound.Play();
    }
}
