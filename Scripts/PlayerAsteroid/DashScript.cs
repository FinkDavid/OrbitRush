using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

public class DashScript : MonoBehaviour
{
    private const float FOV = 60; // 60° of player camera / 2
    public List<GameObject> PlayersToDash;
    [SerializeField] private PlayerMovement playerMovement;
    public AnimationCurve dashCurve;
    [SerializeField] private OrbitAcceptor orbitAcceptor;
    [SerializeField] private DashIndicatorManager dashIndicatorManager;
    float dashCooldown = 1.0f;
    float currentCooldown = 0.0f;
    bool dashPressed = false;
    private Dictionary<GameObject, PlayerInformation> _playerInformationMap = new();
    
    void Update()
    {
        if(orbitAcceptor.IsInOrbit)
        {
            var copy = PlayersToDash.ToArray();

            foreach(var player in copy)
            {
                Debug.Log("Removing player cause we entered orbit");
                RemovePlayerFromDashableList(player);
            }
        }

        if(dashPressed)
        {
            currentCooldown += Time.deltaTime;

            if(currentCooldown >= dashCooldown)
            {
                dashPressed = false;
                currentCooldown = dashCooldown;
            }
        }
    }


    void OnTriggerStay(Collider other)
    {
        if(orbitAcceptor.IsInOrbit) return;
        
        if(!other.CompareTag("Player")) return;
        
        //TODO: Check if player is dead

        if(IsPlayerInFOV(other))
        {
            AddPlayerToDashableList(other.gameObject);
        }

        if(PlayersToDash.Contains(other.gameObject) && !IsPlayerInFOV(other))
        {
            RemovePlayerFromDashableList(other.gameObject);
        }
    }
    
    private void OnTriggerExit(Collider other)
    {
        if(!other.CompareTag("Player")) return;

        if (!PlayersToDash.Contains(other.gameObject)) return;

        RemovePlayerFromDashableList(other.gameObject);
    }

    private void AddPlayerToDashableList(GameObject player)
    {
        if (PlayersToDash.Contains(player)) return;

        if(PlayersToDash.Count == 0 || Vector3.Distance(player.transform.position, playerMovement.transform.position) < Vector3.Distance(PlayersToDash.First().transform.position, playerMovement.transform.position))
        {
            var playerInformation = player.GetComponent<PlayerInformation>();
            playerInformation.playerDeath += OnPlayerDeath;
            
            if(PlayersToDash.Count > 0)
            {
                RemovePlayerFromDashableList(PlayersToDash.First());
            }

            PlayersToDash.Add(player);
            _playerInformationMap.Add(player, playerInformation);
            dashIndicatorManager.dashIndicators[playerInformation.playerID].gameObject.SetActive(true);
        }
    }

    private void OnPlayerDeath(PlayerInformation player)
    {
        RemovePlayerFromDashableList(player.gameObject);
    }
    
    private void RemovePlayerFromDashableList(GameObject player)
    {
        if (!PlayersToDash.Contains(player)) return;
        
        Debug.Log("REMOVING PLAYER");
        PlayersToDash.Remove(player);
        dashIndicatorManager.dashIndicators[_playerInformationMap[player].playerID].gameObject.SetActive(false);
        _playerInformationMap[player].playerDeath -= OnPlayerDeath;
        _playerInformationMap.Remove(player);
    }
    
    private bool IsPlayerInFOV(Collider other)
    {
        Vector3 direction = other.transform.position - transform.position;
        
        float angle = Vector3.Angle(transform.forward, direction);
        //Debug.Log(angle);

        if(angle >= FOV) return false;

        return true;
    }

    // Input System Callback
    public void Dash(InputAction.CallbackContext context)
    {
        if(!context.started) return;

        if(orbitAcceptor.IsInOrbit) return;
        
        if(PlayersToDash.Count == 0) return;
        
        if(!dashPressed)
        {
            StartCoroutine(DoDash(PlayersToDash[0]));
            dashPressed = true;
        }
    }

    private IEnumerator DoDash(GameObject other)
    {
        playerMovement.canMove = false;
        
        Vector3 startPosition = playerMovement.transform.position;
        Vector3 endPosition = other.transform.position;
        
        float elapsedTime = 0f;
        float duration = 0.5f;
        
        //TODO: shaker?
        
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / duration;
            float smoothT = dashCurve.Evaluate(t);

            playerMovement.transform.position = Vector3.Lerp(startPosition, endPosition, smoothT);

            yield return null;
        }

        playerMovement.transform.position = endPosition;
        playerMovement.canMove = true;
    }
}
