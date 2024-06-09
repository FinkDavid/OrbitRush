using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PointIndicatorManager : MonoBehaviour
{
    [SerializeField]
    GameObject plusIndicator;

    [SerializeField]
    GameObject minusIndicator;

    public void SpawnPointIndicator(bool isPlusIndicator)
    {
        if(isPlusIndicator)
        {
            Instantiate(plusIndicator, transform);
        }
        else
        {
            Instantiate(minusIndicator, transform);
        }
    }
}
