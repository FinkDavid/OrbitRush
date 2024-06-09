using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public bool testSetup;
    public string gameSceneName = "MainScene"; //Name of the scene where the players should get spawned
    public int playerCount;
    public GameObject[] playerReferences;
    public Material[] playerOutlineColors = new Material[4];
    public List<InputDevice> inputDevices = new List<InputDevice>();
    public GameObject playerPrefab;
    Canvas canvas;

    private RespawnManager _respawnManager = null;

    public Vector3[] temporarySpawnPoints = new Vector3[4];
    
    /// <summary>
    /// Gets set by <see cref="CanvasMainMenuManager" /> but is very unflexible and probalby bugprone!!!!!/>
    /// </summary>
    public Dictionary<int, InputDevice> players = new();

    private (PlayerMovement Movement, Camera Camera)[] _playerData;
    
    void Start()
    {
        if(GameObject.FindGameObjectWithTag("GameManager") == this.gameObject)
        {
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            //Debug.LogError("There are more than one GameManager in the scene!");
            Destroy(gameObject);
        }
        gameObject.GetComponent<PlayerInputManager>().playerPrefab = playerPrefab;
        
        if (testSetup)
        {
            // we simulate the process of the mainmenu scene
            foreach (var device in InputSystem.devices)
            {
                if (device is Gamepad gamepad)
                {
                    inputDevices.Add(gamepad);
                }
            }
            
            players = inputDevices
                .Select((device, index) => (device, index))
                .ToDictionary(keySelector: x => x.index, elementSelector: x => x.device);
            
            SetupMainGame();
        }
    }

    private void Update()
    {
        if (SceneManager.GetActiveScene().name != gameSceneName) return;
        
        const int layerOffset = 10;
        const int dangerOffset = 1;
        // const int killableOffset = 2;
        
        for (int i = 0; i < _playerData.Length; i++)
        {
            float playerImpact = _playerData[i].Movement.GetImpactFactor();
            
            for (int otherIdx = 0; otherIdx < _playerData.Length; otherIdx++)
            {
                if (i == otherIdx)
                {
                    continue;
                }
                
                float othersImpact = _playerData[otherIdx].Movement.GetImpactFactor();

                if (playerImpact < othersImpact)
                {
                    _playerData[i].Camera.cullingMask |= 1 << (otherIdx * 2 + layerOffset + dangerOffset);
                    // deactivate other layer
                    // _playerData[i].Camera.cullingMask &= ~(1 << (otherIdx * 2 + layerOffset + killableOffset));
                }
                else if (playerImpact > othersImpact)
                {
                    // deactivate other layer
                    _playerData[i].Camera.cullingMask &= ~(1 << (otherIdx * 2 + layerOffset + dangerOffset));
                    // _playerData[i].Camera.cullingMask |= 1 << (otherIdx * 2 + layerOffset + killableOffset);
                }
                else
                {
                    // reset both layers with xor
                    _playerData[i].Camera.cullingMask &= ~(1 << (otherIdx * 2 + layerOffset + dangerOffset));
                    // _playerData[i].Camera.cullingMask &= ~(1 << (otherIdx * 2 + layerOffset + killableOffset));
                }
            }
        }
    }
    
    public GameObject GetPlayerWithID(int playerID)
    {
        return playerReferences[playerID];
    }

    void OnEnable()
    {
        if (!testSetup)
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
    }
    
    private IEnumerator DelayedSpawn()
    {
        yield return new WaitForEndOfFrame();
        for(int i = 0; i < playerReferences.Length; i++)
        {
            _respawnManager.SpawnPlayerOnAvailableOrbit(playerReferences[i]);
        }
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if(scene.name == gameSceneName && inputDevices.Count >= 2) 
		{
            SetupMainGame();
		}
    }

    private void SetupMainGame()
    {
        _respawnManager = GameObject.FindGameObjectWithTag("RespawnManager").GetComponent<RespawnManager>();

        if (_respawnManager == null)
        {
            Debug.LogError("RespawnManager not found!");
        }

        canvas = GameObject.FindGameObjectWithTag("IngameCanvas").GetComponent<Canvas>();

        if (players.Count == 0)
        {
            Debug.LogWarning($"player id to inputdevice map is not set =>" +
                             $" maybe due to the implementation in {nameof(CanvasMainMenuManager)}");
        }
        
		for(int i = 0; i < inputDevices.Count; i++)
        {
            Debug.Log("Joining Player with id: " + i + " and input device: " + inputDevices[i]);
            gameObject.GetComponent<PlayerInputManager>().JoinPlayer(i, i, null, inputDevices[i]);
            playerReferences = GameObject.FindGameObjectsWithTag("Player");
            playerReferences[playerReferences.Length - 1].transform.position = temporarySpawnPoints[i];
        }

        playerCount = playerReferences.Length;

        for(int i = 0; i < playerReferences.Length; i++)
        {
            //Set Outline Material to the correct color
            Renderer renderer = playerReferences[i].GetComponent<PlayerInformation>().playerModel.GetComponent<Renderer>();
            Material[] materials = renderer.materials;
            Array.Resize(ref materials, materials.Length + 1);
            materials[materials.Length - 1] = playerOutlineColors[i];
            renderer.materials = materials;

            for(int x = 0; x < playerReferences.Length; x++)
            {
                PlayerInformation playerInformation = playerReferences[x].GetComponent<PlayerInformation>();
                playerInformation.playerID = x;
                playerInformation.playerCamera.cullingMask |= 1 << (x + 6);
                playerReferences[x].GetComponent<KillIndicatorManager>().SetRenderLayer(playerId: x);
                
                //Loop over all other players and add an indicator to him
                for(int j = 0; j < playerReferences.Length; j++)
                {
                    if(x != j)
                    {
                        playerReferences[x].GetComponent<IndicatorManager>().AddIndicator(j, playerReferences[j].transform);
                        playerReferences[x].GetComponent<DashIndicatorManager>().AddIndicator(j, playerReferences[j].transform);
                    }
                }
                
                ScoreManager.Instance.AddPlayer();
            }

            PlayerInformation playerInfo = playerReferences[i].GetComponent<PlayerInformation>();
            playerInfo.switchCameraOnOrbitEnter.InitiateCamera();
            playerInfo.playerUIManager.InitiatePlayerUI();
            canvas.GetComponent<IngameUIManager>().ChangeOverlay(playerCount);
        }
        ApplySplitscreenRatios();

        _playerData = playerReferences
            .Select(p => (p.GetComponent<PlayerMovement>(), p.GetComponent<PlayerInformation>().playerCamera))
            .ToArray();
        
        StartCoroutine(DelayedSpawn());
    }

    Dictionary<int, List<(Vector2 pos, Vector2 size)>> splitscreenRatios = new Dictionary<int, List<(Vector2 pos, Vector2 size)>>
    {
        { 
            2, new List<(Vector2 pos, Vector2 size)>
            {
                (new Vector2(0.0f, 0.0f), new Vector2(0.5f, 1f)),
                (new Vector2(0.5f, 0.0f), new Vector2(0.5f, 1f))
            }
        },
        { 
            3, new List<(Vector2 pos, Vector2 size)>
            {
                (new Vector2(0.0f, 0.5f), new Vector2(0.5f, 0.5f)),
                (new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f)),
                (new Vector2(0.0f, 0.0f), new Vector2(0.5f, 0.5f))
            }
        },
        { 
            4, new List<(Vector2 pos, Vector2 size)>
            {
                (new Vector2(0.0f, 0.5f), new Vector2(0.5f, 0.5f)),
                (new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f)),
                (new Vector2(0.0f, 0.0f), new Vector2(0.5f, 0.5f)),
                (new Vector2(0.5f, 0.0f), new Vector2(0.5f, 0.5f))
            }
        }
    };

    void ApplySplitscreenRatios()
    {
        GameObject[] playerRoots = GameObject.FindGameObjectsWithTag("PlayerRoot");
        for(int i = 0; i < playerRoots.Length; i++)
        {
            float posX = splitscreenRatios[playerCount][i].pos.x;
            float posY = splitscreenRatios[playerCount][i].pos.y;
            float width = splitscreenRatios[playerCount][i].size.x;
            float height = splitscreenRatios[playerCount][i].size.y;
            playerRoots[i].transform.GetChild(0).gameObject.GetComponent<Camera>().rect = new Rect(posX, posY, width, height);
        }
    }

    private void OnApplicationQuit()
    {
        ResetControllerHapticsForAllControllers();
    }
    
    #region <<Controller Related Stuff>>

    public void ResetControllerHapticsForAllControllers()
    {
        foreach (var inputDevice in inputDevices)
        {
            if (inputDevice is Gamepad gamepad)
            {
                gamepad.ResetHaptics();
            }
        }
    }

    public void RumbleDeviceIfPossible(int playerId, float lowFrequency, float highFrequency)
    {
        if (players[playerId] is Gamepad gamepad)
        {
            gamepad.SetMotorSpeeds(lowFrequency, highFrequency);
        }
    }

    public void ResetHapticsIfPossible(int playerId)
    {
        if (players[playerId] is Gamepad gamepad)
        {
            gamepad.ResetHaptics();
        }
    }
    
    #endregion
}