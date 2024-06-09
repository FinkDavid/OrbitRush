using System;
using Sirenix.OdinInspector;
using UnityEngine;

public class OrbitAcceptor : MonoBehaviour
{
    [ShowInInspector] private PlanetOrbit _currentOrbit = null;
    [SerializeField] public AudioSource boostSound;

    public bool IsInOrbit => _currentOrbit != null;

    public PlanetOrbit CurrentOrbit
    {
        get => _currentOrbit;
        set
        {
            if (_currentOrbit == value) return;
                
            _currentOrbit = value;

            if (_currentOrbit != null)
            {
                OnEnterOrbit?.Invoke(_currentOrbit);
            }
        }
    }
        
    public event Action<PlanetOrbit> OnEnterOrbit;
    public event Action<PlanetOrbit> OnExitOrbit;
        
    public void FleeFromOrbit()
    {
        if (!IsInOrbit) return;

        var planetOrbit = _currentOrbit;
        _currentOrbit.ReleaseOrbitObject(this);
        OnExitOrbit?.Invoke(planetOrbit);
        boostSound.Play();
    }
        
#if UNITY_EDITOR
    public void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, transform.position + transform.forward * 100);
    }
#endif
}