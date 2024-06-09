using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PointIndicator : MonoBehaviour
{
    [SerializeField] float animationTime = 1.0f;
    [SerializeField] Vector2 startPosition;
    [SerializeField] Vector2 endPosition;
    float currentTime = 0.0f;
    
    public AnimationCurve animationCurve;
    RectTransform rectTransform;

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    void Update()
    {
        currentTime += Time.deltaTime;
        float t = currentTime / animationTime;
        float smoothT = animationCurve.Evaluate(t);

        rectTransform.anchoredPosition = Vector2.Lerp(startPosition, endPosition, smoothT);

        if(currentTime >= animationTime)
        {
            Destroy(gameObject);
        }
    }
}
