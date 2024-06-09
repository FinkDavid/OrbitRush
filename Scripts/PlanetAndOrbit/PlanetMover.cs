using System;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

public class PlanetMover : MonoBehaviour
{
    [SerializeField, Required] private Transform centralObject;
    [SerializeField] private float speed = 3;
    
    private float _distance;
    private float _initialAngle;
    
    private void Start()
    {
        _distance = (centralObject.position - transform.position).magnitude;
        _initialAngle = (Vector3.SignedAngle(transform.position - centralObject.position, centralObject.forward, Vector3.up) + 360) * Mathf.Deg2Rad;
        //_initialAngle = Vector3.Angle(transform.position - centralObject.position, centralObject.forward) * Mathf.Deg2Rad;
    }

    private void Update()
    {
        float x = _distance * Mathf.Sin(_initialAngle + Time.timeSinceLevelLoad * speed);
        float z = _distance * Mathf.Cos(_initialAngle + Time.timeSinceLevelLoad * speed);
        transform.position = centralObject.position + new Vector3(x, 0, z);
    }
    
    #if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        float distance = (centralObject.position - transform.position).magnitude;
        Handles.DrawWireDisc(centralObject.position, Vector3.up, distance);
    }
    #endif
}