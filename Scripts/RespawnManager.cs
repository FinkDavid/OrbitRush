using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class RespawnManager : MonoBehaviour
{
    // public List<(bool isLocked, Vector3 position)> respawnPoints = new List<(bool isLocked, Vector3 position)>();
    
    [SerializeField] private int respawnTimer = 3;

    [SerializeField] private List<PlanetOrbit> planetOrbits = new List<PlanetOrbit>();

    GameManager gameManager;

    public void Start()
    {
        gameManager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>();

        if (planetOrbits.Count == 0)
        {
            var planetOrbitComponents = GameObject.FindGameObjectsWithTag("Planet")
                .Select(orbit => orbit.GetComponent<PlanetOrbit>())
                .Where(orbit => orbit != null);
            
            planetOrbits.AddRange(planetOrbitComponents);
        }
    }

    public void RespawnPlayer(int playerID)
    {
        StartCoroutine(RespawnAfterSeconds(playerID));
    }

    private IEnumerator RespawnAfterSeconds(int playerID)
    {
        GameObject player = gameManager.GetPlayerWithID(playerID);
        PlayerMovement playerMovement = player.GetComponent<PlayerMovement>();
        PlayerInformation playerInformation = player.GetComponent<PlayerInformation>();

        playerInformation.IsDead = true;
        playerMovement.SetPlayerAlive(false);
        TextMeshProUGUI respawnText = playerInformation.playerRespawnText;
        respawnText.gameObject.SetActive(true);

        float timer = respawnTimer;
        while(timer > 0)
        {
            timer -= Time.deltaTime;
            respawnText.text = Math.Ceiling(timer).ToString();
            yield return null;
        }
        respawnText.gameObject.SetActive(false);

        playerMovement.GetComponent<OrbitAcceptor>().FleeFromOrbit();
        playerMovement.ResetMass();
        playerMovement.ResetSpeed();
        playerInformation.IsDead = false;
        
        playerMovement.SetPlayerAlive(true);
        PlanetOrbit orbit = SpawnPlayerOnAvailableOrbit(player);
        Debug.Log("Respawning player: " + playerID + " at: " + orbit.gameObject.name);
    }

    internal PlanetOrbit SpawnPlayerOnAvailableOrbit(GameObject player)
    {
        int randomIndex = 0;
        bool respawnSuccess = false;
        
        do
        {
            randomIndex = UnityEngine.Random.Range(0, planetOrbits.Count);
            respawnSuccess = planetOrbits[randomIndex].TrySpawnPlayer(player);
        }
        while(!respawnSuccess);
        
        return planetOrbits[randomIndex];
    }
}
