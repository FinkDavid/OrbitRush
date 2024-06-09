using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class BorderTeleporter : MonoBehaviour
{
    [SerializeField]
    public ParticleSystem teleportEffect;
    private ParticleSystem.MainModule teleportEffectMain;
    [SerializeField] public AudioSource teleportSound;

    private const float spawnOffset = 500;
    private const float teleportEffectStart = 1500;

    private void OnEnable()
    {
        teleportEffectMain = teleportEffect.main;
    }

    private void OnTriggerEnter(Collider other)
    {
        if(!other.gameObject.CompareTag("FXBorder"))
        {
            return;
        }
        
        teleportEffect.Play();
    }

    private void OnTriggerExit(Collider other)
    {
        if(!other.gameObject.CompareTag("FXBorder"))
        {
            return;
        }
        
        teleportEffect.Stop();
    }

    void OnTriggerStay(Collider other)
    {
        if (other.gameObject.CompareTag("BorderTop"))
        {
            transform.position = new Vector3(transform.position.x, transform.position.y, -transform.position.z + spawnOffset);
            teleportSound.Play();
        }
        
        if (other.gameObject.CompareTag("BorderBottom"))
        {            
            transform.position = new Vector3(transform.position.x, transform.position.y, -transform.position.z - spawnOffset);
            teleportSound.Play();
        }
        
        if (other.gameObject.CompareTag("BorderLeft"))
        {            
            transform.position = new Vector3(-transform.position.x - spawnOffset, transform.position.y, transform.position.z);
            teleportSound.Play();
        }
        
        if (other.gameObject.CompareTag("BorderRight"))
        {            
            transform.position = new Vector3(-transform.position.x + spawnOffset, transform.position.y, transform.position.z);
            teleportSound.Play();
        }
        
        if(other.gameObject.CompareTag("FXBorder"))
        {
            var border = other.gameObject.transform.parent.gameObject;
            var borderCollider = border.GetComponent<BoxCollider>();
        
            Vector3 closestPoint = borderCollider.ClosestPointOnBounds(transform.position);
            float distance = Vector3.Distance(closestPoint, transform.position);
            float alphaValue = Mathf.Clamp(1 - distance / teleportEffectStart, 0, 1);
            teleportEffectMain.startColor = new ParticleSystem.MinMaxGradient(new Color(1, 1, 1, alphaValue));
        }
    }
}
