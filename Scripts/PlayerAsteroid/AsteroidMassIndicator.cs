using System;
using UnityEngine;

public class AsteroidMassIndicator : MonoBehaviour
{
    private Material material;
    public PlayerMovement playerMovement;
    
    // Start is called before the first frame update
    void Start()
    {
        material = GetComponent<Renderer>().material;
    }

    // Update is called once per frame
    void Update()
    {
        float speedRemapped = playerMovement.mass.Remap(PlayerMovement.MinMass, PlayerMovement.MaxMass, 0, 0.8f);
        material.SetFloat("_Metallic", speedRemapped);
        material.SetFloat("_Smoothness", speedRemapped);
    }
}
