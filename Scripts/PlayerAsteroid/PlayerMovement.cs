using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(OrbitAcceptor))]
[RequireComponent(typeof(PlayerInformation))]
[RequireComponent(typeof(AstroidShaker))]
public class PlayerMovement : MonoBehaviour
{
    [Serializable]
    private struct JumpConfiguration
    {
        public float JumpDuration;
        public DG.Tweening.Ease EaseType;
        [ShowIf("@EaseType == Ease.Unset")] public AnimationCurve TweenCurve;
        
        public JumpConfiguration(float jumpDuration = 0.5f)
        {
            JumpDuration = jumpDuration;
            EaseType = Ease.Linear;
            TweenCurve = AnimationCurve.Constant(0, 1, 0);
        }
    }
    
    //Movement-Based variables
    public const float MinSpeed = 1000;
    public const float MaxSpeed = 3000;
    [SerializeField, Range(MinSpeed, MaxSpeed)] private float speed = 1000.0f;
    [SerializeField] private float speedDecreaseFactor = 250;
    [SerializeField] private float steeringFactor = 5f;
    [SerializeField] private JumpConfiguration jumpConfiguration = new JumpConfiguration();
    public bool canMove = true;


    //Playermodel-Based variables
    public const float MinMass = 10f;
    public const float MaxMass = 14f;
    [SerializeField] public float mass = 10.0f;
    [InfoBox("The player model is the object that will be scaled when the player changes mass.")]
    [SerializeField] private GameObject playerModel;
    public float initialColliderRadius = 150.0f;

    
    //Information about the status of the player
    private Vector3 _currentMovementInput;
    private OrbitAcceptor _orbitAcceptor;
    private bool _isJumping = false;


    //Important references
    SphereCollider playerCollider;
    GameManager gameManager;
    private PlayerInformation _playerInformation;
    private AstroidShaker _astroidShaker;
    
    public float Speed
    {
        get => speed;
        set
        {
            speed = value;

            if (speed > MaxSpeed)
            {
                speed = MaxSpeed;
            }
            else if (speed < MinSpeed)
            {
                speed = MinSpeed;
            }
            
        }
    }
    
    private void Start()
    {
        _orbitAcceptor = GetComponent<OrbitAcceptor>();
        gameManager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>();
        playerCollider = GetComponent<SphereCollider>();
        _playerInformation = GetComponent<PlayerInformation>();
        _astroidShaker = GetComponent<AstroidShaker>();
    }
    
    private void Update()
    {
        // Reset Rotation input when entering orbit to avoid applying rotation in ApplyMovement
        // when leaving the orbit
        if (_orbitAcceptor.IsInOrbit && _currentMovementInput.x != 0)
        {
            _currentMovementInput = Vector3.zero;
        }
        
        if(canMove)
        {
            ApplyMovement();
        }
    }

    public void ApplyMovement()
    {
        if (_orbitAcceptor.IsInOrbit || _isJumping) return;

        Vector3 movement = new Vector3(0.0f, 0.0f, speed) * Time.deltaTime;
        Vector3 rotation = new Vector3(0.0f, _currentMovementInput.x * steeringFactor, 0.0f) * Time.deltaTime;
        
        transform.Rotate(rotation);
        transform.Translate(movement);
        Speed -= speedDecreaseFactor * Time.deltaTime;
    }
    
    public void Move(InputAction.CallbackContext context)
    {
        // is enabled while isJumping to allow player to orient himself after the jump
        if (_orbitAcceptor.IsInOrbit) return;
        
        _currentMovementInput.x = context.ReadValue<Vector2>().x;
    }

    public void Jump(InputAction.CallbackContext context)
    {
        if (!_orbitAcceptor.IsInOrbit) return;
        
        _isJumping = true;
        _orbitAcceptor.FleeFromOrbit();
        // jump force is always the same as the speed to make a smooth transition from jump to movement 
        Vector3 targetPosition = transform.position + transform.forward * speed;
        var tweener = transform.DOMove(targetPosition, jumpConfiguration.JumpDuration);
        tweener.onComplete += () =>
        {
            _isJumping = false;
        };

        if (jumpConfiguration.EaseType == Ease.Unset)
        {
            tweener.SetEase(jumpConfiguration.TweenCurve);
        }
        else
        {
            tweener.SetEase(jumpConfiguration.EaseType);
        }
    }
    
    public void ChangeMass(float factor)
    {
        mass *= factor;
        playerModel.transform.localScale = new Vector3(mass, mass, mass);
        //Change sphere collider radius based on factor as well
        playerCollider.radius = playerCollider.radius * factor;
    }

    public void ResetMass()
    {
        mass = MinMass;
        playerModel.transform.localScale = new Vector3(MinMass, MinMass, MinMass);
        playerCollider.radius = initialColliderRadius;
    }

    public void ResetSpeed()
    {
        Speed = MinMass;
    }
    
    public bool AddMass(float amount)
    {
        if(mass >= MaxMass) return false;
        
        mass += amount;
        playerModel.transform.localScale = new Vector3(mass, mass, mass);
        playerCollider.radius = playerCollider.radius + (initialColliderRadius / MinMass * amount);
        
        return true;        
    }

    public void ChangeSpeed(float factor)
    {
        speed *= factor;
    }

    public float GetImpactFactor()
    {
        return Math.Abs(speed * (mass * 3));
    }

    public void SetPlayerAlive(bool isAlive)
    {
        playerModel.SetActive(isAlive);

        //In case he dies while being locked in an orbit, first remove him from orbit so he can properly respawn
        // if(!isAlive)
        // {
        //     GetComponent<OrbitAcceptor>().FleeFromOrbit();
        // }

        GetComponent<IndicatorManager>().SetIndicatorsActive(isAlive);
        GetComponent<KillIndicatorManager>().SetActivation(isAlive);
        //GetComponent<PlayerMovement>().enabled = isAlive;
        this.enabled = isAlive;
        playerCollider.enabled = isAlive;
        GetComponent<PlayerImpact>().enabled = isAlive;
    }

    public void AbsorbPlayer(GameObject blackHole, float duration, AnimationCurve absorptionCurve)
    {
        StartCoroutine(Absorb(blackHole, duration, absorptionCurve));
    }

    public IEnumerator Absorb(GameObject blackHole, float duration, AnimationCurve absorptionCurve)
    {
        Vector3 startPosition = transform.position;
        Vector3 endPosition = blackHole.transform.position;
        float elapsedTime = 0f;
        canMove = false;

        gameManager.RumbleDeviceIfPossible(_playerInformation.playerID, 0.25f, 0.75f);
        _astroidShaker.StartShakingCamera(4f, 15f);
        
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / duration;
            float smoothT = absorptionCurve.Evaluate(t);

            transform.position = Vector3.Lerp(startPosition, endPosition, smoothT);

            yield return null;
        }

        transform.position = endPosition;
        transform.LookAt(new Vector3(0.0f, 0.0f, 0.0f));
        StartCoroutine(SendPlayerOutOfBlackhole(blackHole, duration, absorptionCurve));
    }

    public IEnumerator SendPlayerOutOfBlackhole(GameObject blackHole, float duration, AnimationCurve absorptionCurve)
    {
        Vector3 startPosition = blackHole.transform.position;
        Vector3 endPosition = blackHole.transform.position * 0.5f;
        float elapsedTime = 0f;

        bool stopCameraShake = false;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / duration;
            float smoothT = absorptionCurve.Evaluate(t);

            transform.position = Vector3.Lerp(startPosition, endPosition, smoothT);

            if(elapsedTime > duration / 4 && !stopCameraShake)
            {
                stopCameraShake = true;
                _astroidShaker.StopShakingCamera();
                gameManager.ResetHapticsIfPossible(_playerInformation.playerID);
            }

            yield return null;
        }

        transform.position = endPosition;
        canMove = true;
    }
}
