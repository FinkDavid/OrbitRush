using Cinemachine;
using Sirenix.OdinInspector;
using UnityEngine;

[RequireComponent(typeof(OrbitAcceptor))]
public class SwitchCameraOnOrbitEnter : MonoBehaviour
{
    [SerializeField] private OrbitAcceptor orbitAcceptor;
    [SerializeField, Required] private CameraOrbitHelper orbitHelper;
    [SerializeField] private CinemachineVirtualCamera spaceMovementCamera;
    [SerializeField] private CinemachineVirtualCamera orbitMovementCamera;
    [SerializeField] private PlayerInformation playerInformation;
    
    public void InitiateCamera()
    {
        int layer = LayerMask.NameToLayer($"IndicatorLayer{playerInformation.playerID}");
        playerInformation.playerCamera.gameObject.layer = layer;
        spaceMovementCamera.gameObject.layer = layer;
        orbitMovementCamera.gameObject.layer = layer;
        
        orbitAcceptor.OnEnterOrbit += planetOrbit =>
        {
            // swap algorithm by deconstruction feature
            (spaceMovementCamera.Priority, orbitMovementCamera.Priority) = (orbitMovementCamera.Priority, spaceMovementCamera.Priority);

            Vector3 enterPosition = transform.position;
            // target should be the player GameObject? :D 
            PlanetOrbit.OrbitObject orbitData = planetOrbit.GetOrbitObjectData(orbitAcceptor.transform);
            orbitHelper.transform.position = enterPosition;
            orbitHelper.SetOrbitData(orbitData);
        };
		
        orbitAcceptor.OnExitOrbit += _ =>
        {
            (spaceMovementCamera.Priority, orbitMovementCamera.Priority) = (orbitMovementCamera.Priority, spaceMovementCamera.Priority);
            orbitHelper.ResetOrbitData();
        };
    }
}