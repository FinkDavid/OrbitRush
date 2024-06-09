using System;
using System.Collections;
using DG.Tweening;
using UnityEngine;

public class AstroidRotator : MonoBehaviour
{
    private Coroutine _rotationRoutine;

    private void OnEnable()
    {
        if (_rotationRoutine != null)
        {
            StopCoroutine(_rotationRoutine);
        }
        
        _rotationRoutine = StartCoroutine(EndlessRotationRoutine());
    }

    private void OnDisable()
    {
        if (_rotationRoutine != null)
        {
            StopCoroutine(_rotationRoutine);
            _rotationRoutine = null;
        }
    }

    private IEnumerator EndlessRotationRoutine()
    {
        while (true)
        {
            var rotation = new Vector3(
                UnityEngine.Random.Range(90, 180),
                0,
                0
            );
            
            yield return transform
                .DOLocalRotate(rotation, 1, RotateMode.WorldAxisAdd)
                .SetEase(Ease.Linear)
                .WaitForCompletion();

            // war irgendein cooler effekt
            // yield return transform
            //     .DOShakeRotation(1, 60, 1, 30)
            //     .SetEase(Ease.Linear)
            //     .WaitForCompletion();
        }
    }
}