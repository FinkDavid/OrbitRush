using System;
using System.Collections;
using Cinemachine;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerMovement))]
[RequireComponent(typeof(PlayerInformation))]
public class AstroidShaker : MonoBehaviour
{
    [System.Serializable]
    public class ShakeConfiguration
    {
        public float duration = .2f;
        public float strength = 20;
        public int vibrato = 15;
        public float randomness = 90;
        public ShakeRandomnessMode randomnessMode = ShakeRandomnessMode.Harmonic;
    }
    
    [System.Serializable]
    public class RumbleConfiguration
    {
        public float lowFrequency = 0.25f;
        public float highFrequency = 0.75f;
    }

    [SerializeField] private bool useControllerRumble = true;
    [InfoBox("Is the percentage of the MaxSpeed of set in PlayerMovement")]
    [SerializeField] private float rumbleDistanceThreshold = 400;
    [SerializeField, Required] private Transform gameObjectToShake;
    [SerializeField] private ShakeConfiguration shakeConfiguration;
    [SerializeField] private RumbleConfiguration rumbleConfiguration;
    [SerializeField] private OrbitAcceptor orbitAcceptor;

    private bool _doShake = false;
    private Coroutine _coroutine;

    private GameManager _gameManager;
    private PlayerMovement _playerMovement;
    private PlayerInformation _playerInformation;

    //Camera shake stuff
    public CinemachineVirtualCamera spaceMovementCamera;
    CinemachineBasicMultiChannelPerlin spaceMovementCameraPerlin;

    private void Start()
    {
        _gameManager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>();
        _playerMovement = GetComponent<PlayerMovement>();
        _playerInformation = GetComponent<PlayerInformation>();
        spaceMovementCameraPerlin = spaceMovementCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
    }

    private void Update()
    {
        if (orbitAcceptor.IsInOrbit)
        {
            var orbit = orbitAcceptor.CurrentOrbit.GetOrbitObjectData(orbitAcceptor.transform);
            
            var planetCollisionCheck = orbitAcceptor.CurrentOrbit.transform.GetChild(1).GetComponent<SphereCollider>();
            
            _doShake = orbit.Distance - planetCollisionCheck.radius < rumbleDistanceThreshold;
        }
        else
        {
            _doShake = false;
        }
        
        if (_doShake && _coroutine == null)
        {
            _coroutine = StartCoroutine(ShakeRoutine());
            
            if (useControllerRumble && Gamepad.all.Count > 0)
            {
                _gameManager.RumbleDeviceIfPossible(_playerInformation.playerID, rumbleConfiguration.lowFrequency, rumbleConfiguration.highFrequency);
            }
        }
        
        if (!_doShake && _coroutine != null)
        {
            StopCoroutine(_coroutine);
            _coroutine = null;
            
            if (useControllerRumble && Gamepad.all.Count > 0)
            {
                _gameManager.ResetHapticsIfPossible(_playerInformation.playerID);
            }
        }
    }
    
    private IEnumerator ShakeRoutine()
    {
        while (true)
        {
            if (_doShake)
            {
                yield return gameObjectToShake
                    .DOShakePosition(
                        shakeConfiguration.duration,
                        shakeConfiguration.strength,
                        shakeConfiguration.vibrato,
                        randomness: shakeConfiguration.randomness, fadeOut: false,
                        randomnessMode: shakeConfiguration.randomnessMode)
                    .SetEase(Ease.Linear)
                    .WaitForCompletion();
            }
        }
    }

    public void StartShakingCamera(float intensity, float frequency)
    {
        spaceMovementCameraPerlin.m_AmplitudeGain = intensity;
        spaceMovementCameraPerlin.m_FrequencyGain = frequency;
    }

    public void StopShakingCamera()
    {
        spaceMovementCameraPerlin.m_AmplitudeGain = 0;
        spaceMovementCameraPerlin.m_FrequencyGain = 0;
    }
}