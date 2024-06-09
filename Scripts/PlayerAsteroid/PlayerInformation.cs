using System;
using TMPro;
using UnityEngine;

public class PlayerInformation : MonoBehaviour
{
    public int playerID;
    [SerializeField] private bool _isDead = false;
    public TextMeshProUGUI playerRespawnText;
    public Canvas playerCanvas;
    public Camera playerCamera;
    public GameObject playerModel;
    public SwitchCameraOnOrbitEnter switchCameraOnOrbitEnter;
    public PlayerUIManager playerUIManager;
    public PointIndicatorManager pointIndicatorManager;

    public event Action<PlayerInformation> playerDeath;
    
    public bool IsDead
    {
        get => _isDead;
        set
        {
            if (_isDead == value) return;
            
            _isDead = value;
            
            if (_isDead)
            {
                playerDeath?.Invoke(this);
            }
        }
    }
    
}
