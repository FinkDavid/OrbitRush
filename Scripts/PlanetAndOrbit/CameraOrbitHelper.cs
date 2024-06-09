using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraOrbitHelper : MonoBehaviour
{
    [SerializeField] public OrbitAcceptor _orbitAcceptor;
    [SerializeField] private PlayerInformation playerInformation;
    private Vector3 _currentMovementInput;
    private PlanetOrbit.OrbitObject _currentOrbitData;
    private float _currentOrbitAngle;
    
    public void SetOrbitData(PlanetOrbit.OrbitObject orbitData)
    {
        _currentOrbitData = orbitData;
        _currentOrbitAngle = _currentOrbitData.InitialAngle;
    }

    public void ResetOrbitData()
    {
        _currentOrbitData = null;
        _currentOrbitAngle = 0;
    }
    
    public void MoveCamera(InputAction.CallbackContext context)
    {
        if (!_orbitAcceptor.IsInOrbit) return;
        _currentMovementInput.x = context.ReadValue<Vector2>().x;
    }

    private void Update()
    {
        // if player dies and still holds camera control jouystick we have to reset the movement input
        if (playerInformation.IsDead)
        {
            _currentMovementInput = new Vector3();
        }
        
        if (!_orbitAcceptor.IsInOrbit) return;
        
        float speed = 160 * Mathf.Deg2Rad * Time.deltaTime;
        
        _currentOrbitAngle += -_currentOrbitData.Direction * _currentMovementInput.x * speed;
        transform.position = _orbitAcceptor.CurrentOrbit
            .GetPositionInOrbit(_currentOrbitAngle, directionSide: _currentOrbitData.Direction);
        transform.Rotate(transform.up, _orbitAcceptor.CurrentOrbit
            .GetRotationAngleToAlignSideToPlanet(transform, directionSide: _currentOrbitData.Direction)
        );
    }
}
