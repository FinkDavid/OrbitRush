using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
public class PlanetOrbit : MonoBehaviour
{
    public const float MinOrbitSpeed = 1f;
    public const float MaxOrbitSpeed = 3f;
    
    [Serializable]
    public class OrbitObject
    {
        public Transform Transform;
        public float Distance;
        public float InitialAngle;
        public float TimeInOrbit;
        public PlayerMovement PlayerMovement;
        public Vector3? TangentPoint;
        public bool OrbitTransitionStarted;
        public bool OrbitTransitionFinished;
        public float Direction = -1;
        
        public OrbitObject(Transform transform, float distance, float angle)
        {
            Transform = transform;
            Distance = distance;
            InitialAngle = angle;
            TimeInOrbit = 0;
        }
        
        public void Deconstruct(out Transform transform, out float distance) => (transform, distance) = (Transform, Distance);
    }
    
    [OnValueChanged(nameof(ApplyOrbitRadiusToSphereCollider))]
    [SerializeField] private float orbitRadius = 300;
    [SerializeField] private float gravityFactor = 70;
    [SerializeField] private float orbitSpeedFactor = 0.5f;
    [SerializeField] private float speedFactor = 300f;
    [ShowInInspector, DisableInEditorMode] private List<OrbitObject> _objectsInOrbit = new ();
    
    private readonly HashSet<int> _objectsInOrbitLookup = new();
    private SphereCollider _sphereCollider;
    private const float OrbitTransitionRadiusAddition = 200;

    public float ScaleIncludedOrbitRadius => GetRadiusScaleFactor() * orbitRadius;

    // From Unitys Shpere Collider Tool: https://github.com/Unity-Technologies/UnityCsReference/blob/master/Modules/PhysicsEditor/SphereColliderEditor.cs
    public float GetRadiusScaleFactor()
    {
        float result = 0;
        Vector3 globalScale = transform.lossyScale;
            
        for (int axis = 0; axis < 3; ++axis)
        {
            result = Mathf.Max(result, Mathf.Abs(globalScale[axis]));
        }
            
        return result;
    }
    
    private void ApplyOrbitRadiusToSphereCollider()
    {
        SphereCollider collider = _sphereCollider;
        
        if (!_sphereCollider)
        {
            if (!TryGetComponent<SphereCollider>(out collider))
            {
                Debug.LogError("PlanetOrbit Script has no SphereCollider attached on its GameObject.");
                return;
            }
        }
        
        collider.radius = orbitRadius + OrbitTransitionRadiusAddition;
    }
    
    private void Start()
    {
        _sphereCollider = GetComponent<SphereCollider>();
        _sphereCollider.radius = orbitRadius + OrbitTransitionRadiusAddition;
        // achtung scaling wird bereits in collider miteinbegriffen
    }

    private void Update()
    {
        Vector3 orbitPosition = transform.position;
        
        for (int i = 0; i < _objectsInOrbit.Count; i++)
        {
            var orbitObject = _objectsInOrbit[i];
            
            if (orbitObject.TangentPoint != null && !orbitObject.OrbitTransitionStarted && !orbitObject.OrbitTransitionFinished)
            {
                orbitObject.OrbitTransitionStarted = true;

                // var movementTween = orbitObject.Transform
                //     .DOMove(orbitObject.TangentPoint.Value, 1f)
                //     .SetEase(Ease.Linear)
                //     .OnComplete(() =>
                //     {
                //         orbitObject.OrbitTransitionFinished = true;
                //     })
                //     .OnUpdate(() =>
                //     {
                //         Vector3 object2Center = (transform.position - orbitObject.Transform.position).normalized;
                //         Vector3 objectSide = orbitObject.Direction < 0 ? -orbitObject.Transform.right : orbitObject.Transform.right;
                //         float angle = Vector3.SignedAngle(object2Center, objectSide, Vector3.up);
                //         orbitObject.Transform.Rotate(transform.up, -angle);
                //     });
                
                // StartCoroutine(EnterOrbitRoutine(orbitObject, 2f));
                orbitObject.OrbitTransitionFinished = true;
            }
            else if (orbitObject.TangentPoint == null ||
                     (orbitObject.TangentPoint != null && orbitObject.OrbitTransitionFinished))
            {
                _objectsInOrbit[i] = UpdateOrbitObject(orbitObject, orbitPosition);
            }
        }
    }

    private IEnumerator EnterOrbitRoutine(OrbitObject orbitObject, float duration)
    {
        if (!orbitObject.OrbitTransitionStarted) yield break;
        
        float time = 0;
        var targetPosition = orbitObject.Transform.position + orbitObject.TangentPoint.Value - orbitObject.Transform.position;
        var distance = orbitObject.TangentPoint.Value - orbitObject.Transform.position;
        float distanceLength = distance.magnitude;
        float timeUnit = (distanceLength / (duration * 1000));
        
        while (time < duration)
        {
            orbitObject.Transform.position = Vector3.Lerp(orbitObject.Transform.position,
                targetPosition, timeUnit * Time.deltaTime);
           
            Vector3 object2Center = (transform.position - orbitObject.Transform.position).normalized;
            Vector3 objectSide = orbitObject.Direction < 0 ? -orbitObject.Transform.right : orbitObject.Transform.right;
            float angle = Vector3.SignedAngle(object2Center, objectSide, Vector3.up);
            orbitObject.Transform.Rotate(transform.up, -angle);
            time += Time.deltaTime;
            yield return null;
        }

        orbitObject.OrbitTransitionFinished = true;
    }

    public Vector3 GetPositionInOrbit(float orbitAngle, float? distance = null, float directionSide = -1)
    {
        distance ??= ScaleIncludedOrbitRadius;
        Vector3 orbitPosition = this.transform.position;
        float x = distance.Value * directionSide * Mathf.Sin(orbitAngle);
        float z = distance.Value * Mathf.Cos(orbitAngle);
        return orbitPosition + new Vector3(x, 0, z);
    }

    public float GetRotationAngleToAlignSideToPlanet(Transform objectTransform, float directionSide = -1)
    {
        Vector3 object2Center = (transform.position - objectTransform.position).normalized;
        Vector3 objectSide = directionSide < 0 ? -objectTransform.right : objectTransform.right;
        float angle = Vector3.SignedAngle(object2Center, objectSide, Vector3.up);
        return -angle;      // probably the minus angle
    }
    
    public OrbitObject GetOrbitObjectData(Transform orbitObject)
    {
        return _objectsInOrbit.Find(oo => oo.Transform == orbitObject);
    }
    
    private OrbitObject UpdateOrbitObject(OrbitObject orbitObject, Vector3 orbitPosition)
    {
        // rotate player to align it with the planet's surface
        // Vector3 object2Center = (transform.position - orbitObject.Transform.position).normalized;
        // Vector3 objectSide = orbitObject.Direction < 0 ? -orbitObject.Transform.right : orbitObject.Transform.right;
        // float angle = Vector3.SignedAngle(object2Center, objectSide, Vector3.up);
        // orbitObject.Transform.Rotate(transform.up, -angle);
        
        float modelRotateAngle = GetRotationAngleToAlignSideToPlanet(orbitObject.Transform, orbitObject.Direction);
        orbitObject.Transform.Rotate(transform.up, modelRotateAngle);
        
        // move the player around the orbit of the planet
        orbitObject.TimeInOrbit += Time.deltaTime;

        float localOrbitSpeedFactor = orbitSpeedFactor;

        if (orbitObject.PlayerMovement)
        {
            localOrbitSpeedFactor = orbitObject.PlayerMovement.Speed.Remap(PlayerMovement.MinSpeed, PlayerMovement.MaxSpeed, MinOrbitSpeed,
                MaxOrbitSpeed);
            orbitObject.PlayerMovement.Speed += speedFactor * Time.deltaTime;
        }
        
        float orbitAngle = orbitObject.InitialAngle + localOrbitSpeedFactor * orbitObject.TimeInOrbit;
        // orbitObject.Direction stands either for "-Mathf.Sin(orbitAngle)" or "+Mathf.Sin(orbitAngle)"
        // float x = orbitObject.Distance * orbitObject.Direction * Mathf.Sin(orbitAngle);
        // float z = orbitObject.Distance * Mathf.Cos(orbitAngle);
        // orbitObject.Transform.position = orbitPosition + new Vector3(x, 0, z);

        // abstracted version of the implementation above
        orbitObject.Transform.position = GetPositionInOrbit(orbitAngle, orbitObject.Distance, orbitObject.Direction);
        
        // orbitObject.PreviousAngle = orbitAngle;
        // simulate the gravity force, that drags the player to the planet over time
        orbitObject.Distance -= gravityFactor * Time.deltaTime;
        
        return orbitObject;
    }
    
    public void ReleaseOrbitObject(OrbitAcceptor orbitAcceptor)
    {
        int instanceID = orbitAcceptor.gameObject.GetInstanceID();

        if (_objectsInOrbitLookup.Contains(instanceID))
        {
            orbitAcceptor.CurrentOrbit = null;
            _objectsInOrbitLookup.Remove(instanceID);
            _objectsInOrbit.Remove(_objectsInOrbit.Find(oo => oo.Transform == orbitAcceptor.transform));
        }
    }

    public bool TrySpawnPlayer(GameObject player)
    {
        // TODO: what if there are other orbit objects types than player?????
        if (_objectsInOrbit.Count > 0) return false;

        AddObjectToOrbit(player);
        //Insantly update the players position so he doesnt die again to the sun in case the OnTriggerEnter gets called befor this
        _objectsInOrbit[_objectsInOrbit.Count - 1] = UpdateOrbitObject(_objectsInOrbit.Last(), transform.position);
        return true;
    }

    private void AddObjectToOrbit(GameObject objectToAdd, bool withOrbitTransition = false)
    {
        int instanceId = objectToAdd.GetInstanceID();
        OrbitObject orbitObject = null;

        if (!_objectsInOrbitLookup.Contains(instanceId) && objectToAdd.TryGetComponent<OrbitAcceptor>(out var orbitAcceptor))
        {
            // if player is already in a orbit do nothing
            if (orbitAcceptor.IsInOrbit)
            {
                return;
            }
            
            Transform orbitTransform = this.transform;
            Vector3? tangentPoint = null;
            Vector3 objectEnterPosition = objectToAdd.transform.position;
            
            float directionAngle = Vector3.SignedAngle(
                orbitTransform.position - orbitAcceptor.transform.position,
                orbitAcceptor.transform.forward,
                Vector3.up);

            float directionSide = directionAngle > 0 ? -1 : 1;
            
            // if (withOrbitTransition)
            // {
            //     tangentPoint = CalculateTangentPoint(objectToAdd.transform.position, directionSide);
            //     
            //     if (tangentPoint != null)
            //     {
            //         objectEnterPosition = tangentPoint.Value;
            //     }
            // }
            
            float angle = Vector3.SignedAngle(objectEnterPosition - orbitTransform.position, orbitTransform.forward, Vector3.up);
            orbitObject = new OrbitObject(objectToAdd.transform, distance: ScaleIncludedOrbitRadius, -directionSide * Mathf.Deg2Rad * angle);
            orbitObject.TangentPoint = tangentPoint;

            orbitObject.Direction = directionSide;
            
            if (objectToAdd.CompareTag("Player"))
            {
                orbitObject.PlayerMovement = objectToAdd.GetComponent<PlayerMovement>();
            }
            
            _objectsInOrbitLookup.Add(instanceId);
            _objectsInOrbit.Add(orbitObject);
            
            // des muas am ende sei, weil sonst callback hell into the dark
            orbitAcceptor.CurrentOrbit = this;
        }
    }
    
    private void OnTriggerEnter(Collider other)
    {
        AddObjectToOrbit(other.gameObject, withOrbitTransition: true);
    }

    private Vector3? CalculateTangentPoint(Vector3 objectPosition, float enterSide)
    {
        Vector3 direction = objectPosition - transform.position;
        float distance = direction.magnitude;
        // enterSide defines if it means "-Vector3.Cross" or "+Vector3.Cross"
        // it has the opposite impact for the orbit calculation, therefore -enterSide is used to make it compatible with Math.Sin calculations
        Vector3 tangent = -enterSide * ScaleIncludedOrbitRadius * Vector3.Cross(direction, Vector3.up).normalized;
        
        // If the point is inside or on the sphere
        if (distance <= ScaleIncludedOrbitRadius)
        {
            Debug.LogWarning("Object is already inside a orbit position.");
            return null;
        }
        
        // Calculate the tangent point, thats the orbit enter target
        Vector3 tangentPoint = transform.position + tangent;
        return tangentPoint;
    }
    
#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Handles.color = Color.yellow;
        Handles.DrawWireDisc(transform.position, transform.up, ScaleIncludedOrbitRadius);
        Handles.color = Color.magenta;
        Handles.DrawWireDisc(transform.position, transform.up, ScaleIncludedOrbitRadius + GetRadiusScaleFactor() * OrbitTransitionRadiusAddition);
        
        // draw tangentPoint
        foreach (var orbitObject in _objectsInOrbit)
        {
            if (orbitObject.TangentPoint != null)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawLine(transform.position, orbitObject.TangentPoint.Value);
            }
        }
    }
#endif
}
