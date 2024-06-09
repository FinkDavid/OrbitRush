using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CanvasMainMenuManager : MonoBehaviour
{
    public GameObject controlsScreen;
    public Button startGameButton;
    public Button controlsButton;
    public Button exitControlsButton;

    public GameObject joinScreen;
    public GameObject[] playerCards = new GameObject[4];
    public List<InputDevice> joinedPlayers = new List<InputDevice>();
    public GameObject startText;
    int playerCount = 0;
    bool loadedMainScene = false;

    void Update()
    {
        if(joinScreen.activeSelf)
        {
            foreach (var device in InputSystem.devices)
            {
                if (device is Gamepad gamepad && gamepad.buttonWest.isPressed)
                {
                    InputDevice xPressedDevice = device;
                    if(!joinedPlayers.Contains(xPressedDevice))
                    {
                        joinedPlayers.Add(xPressedDevice);
                        playerCards[playerCount].transform.GetChild(0).gameObject.SetActive(false);
                        playerCards[playerCount].transform.GetChild(1).gameObject.SetActive(true);
                        playerCount++;
                    }
                }
            }

            if(joinedPlayers.Count >= 2)
            {
                startText.SetActive(true);
            }
        }
    }

    public void OnStartGame(InputAction.CallbackContext context)
    {
        if(joinedPlayers.Count >= 2 && !loadedMainScene)
        {
            var gameManager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>();
            gameManager.inputDevices = joinedPlayers;
            gameManager.players = joinedPlayers
                .Select((device, index) => (device, index))
                .ToDictionary(keySelector: x => x.index, elementSelector: x => x.device);

            loadedMainScene = true;
            SceneManager.LoadScene("MainScene");
        }
    }

    public void OnBackButton(InputAction.CallbackContext context)
    {
        if(joinScreen.activeSelf && context.started)
        {
            joinScreen.SetActive(false);

            for(int i = 0; i < 3;i++)
            { 
                playerCards[i].transform.GetChild(0).gameObject.SetActive(true);
                playerCards[i].transform.GetChild(1).gameObject.SetActive(false);
            }
            joinedPlayers.Clear();
            playerCount = 0;
            startText.SetActive(false);
        }
    }
    
    public void EnableJoinScreen()
    {
        joinScreen.SetActive(true);
    }

    public void ShowControlsScreen()
    {
        controlsScreen.SetActive(true);
        startGameButton.gameObject.SetActive(false);
        controlsButton.gameObject.SetActive(false);
        GameObject.Find("EventSystem").GetComponent<EventSystem>().SetSelectedGameObject(exitControlsButton.gameObject);
    }

    public void HideControlsScreen()
    {
        controlsScreen.SetActive(false);
        startGameButton.gameObject.SetActive(true);
        controlsButton.gameObject.SetActive(true);
        GameObject.Find("EventSystem").GetComponent<EventSystem>().SetSelectedGameObject(startGameButton.gameObject);
    }
}
