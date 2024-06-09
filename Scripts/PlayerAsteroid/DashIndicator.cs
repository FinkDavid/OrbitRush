using UnityEngine;

public class DashIndicator : MonoBehaviour
{
    public Transform target;
    [SerializeField] private GameObject indicator;

    void Update()
    {
        transform.LookAt(target);
        
        var length = Vector3.Distance(transform.position, target.position);
        
        indicator.transform.localScale = new Vector3(200, 10, length);
        indicator.transform.localPosition = new Vector3(0, 0, length / 2);
    }

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }
}
