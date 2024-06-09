using UnityEngine;

[RequireComponent(typeof(PlayerMovement))]
public class TrailColorChanger : MonoBehaviour
{
    [System.Serializable]
    public class SpeedStage
    {
        public float lowerSpeedLimitFactor = PlayerMovement.MinSpeed / PlayerMovement.MaxSpeed;
        public float upperSpeedLimitFactor = 1;
        public Color trailColor  = Color.white;
    }

    [SerializeField] private SpeedStage[] _speedStages;
    [SerializeField] private ParticleSystem _trailParticleSystem;

    private PlayerMovement _playerMovement;
    private SpeedStage _currentSpeedStage;
    
    private void Start()
    {
        _playerMovement = GetComponent<PlayerMovement>();
    }
    
    private void Update()
    {
        foreach (var speedStage in _speedStages)
        {
            if (_playerMovement.Speed >= PlayerMovement.MaxSpeed * speedStage.lowerSpeedLimitFactor
                && _playerMovement.Speed <= PlayerMovement.MaxSpeed * speedStage.upperSpeedLimitFactor
                && speedStage != _currentSpeedStage)
            {
                _currentSpeedStage = speedStage;        // for performance to not set the color every frame
                var main = _trailParticleSystem.main;
                main.startColor = speedStage.trailColor;
            }
        }
    }
}