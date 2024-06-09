using System.Collections.Generic;
using UnityEngine;

public static class ExtensionMethods
{
    /// <summary>
    /// Remaps the specified value from one range to another.
    /// </summary>
    public static float Remap(this float value, float from1, float to1, float from2, float to2)
    {
        return (value - from1) / (to1 - from1) * (to2 - from2) + from2;
    }

    /// <summary>
    /// Returns a 3-Color Gradient from a float [0,1].
    /// </summary>
    /// <param name="value">The value [0,1].</param>
    /// <returns>Gradient Color</returns>
    public static Color ToGradientColor(this float value, Color min, Color mid, Color high)
    {
        float val = Mathf.Max(Mathf.Min(1f, value), 0f);
        if (value <= 0.5f)
            return Color.Lerp(min, mid, val.Remap(0.0f, 0.5f, 0.0f, 1.0f));
        else
            return Color.Lerp(Color.yellow, Color.red, val.Remap(0.5f, 1.0f, 0.0f, 1.0f));
    }


    public static float SqrDistance(this Vector3 a, Vector3 b)
    {
        return (a - b).sqrMagnitude;
    }

    public static Vector3 ToVec3XZ(this Vector2 value)
    {
        return new Vector3(value.x, 0, value.y);
    }

    public static float SqrDistance(this Vector2 a, Vector2 b)
    {
        return (a - b).sqrMagnitude;
    }

    public static Vector2 ToVec2XZ(this Vector3 value)
    {
        return new Vector2(value.x, value.z);
    }



    public static Vector3 RotateAroundPivot(this Vector3 value, Vector3 pivot, Vector3 angles)
    {
        Vector3 dir = value - pivot; // get point direction relative to pivot
        dir = Quaternion.Euler(angles) * dir; // rotate it
        value = dir + pivot; // calculate rotated point
        return value;
    }

    /// <summary>
    /// Gets or add a component. Usage example:
    /// BoxCollider boxCollider = transform.GetOrAddComponent<BoxCollider>();
    /// </summary>
    static public T GetOrAddComponent<T>(this Component child) where T : Component
    {
        T result = child.GetComponent<T>();
        if (result == null)
        {
            result = child.gameObject.AddComponent<T>();
        }
        return result;
    }

    public static void Fisher_Yates_CardDeck_Shuffle<T>(this List<T> aList)
    {
        var n = aList.Count;
        for (var i = 0; i < n; i++)
        {
            var r = i + (int)(Random.value * (n - i));
            var myGo = aList[r];
            aList[r] = aList[i];
            aList[i] = myGo;
        }
        
    }

    public static void Fisher_Yates_CardDeck_Shuffle<T>(this T[] anArray)
    {
        var n = anArray.Length;
        for (var i = 0; i < n; i++)
        {
            var r = i + (int)(Random.value * (n - i));
            var myGo = anArray[r];
            anArray[r] = anArray[i];
            anArray[i] = myGo;
        }
    }

    public static Vector3 ScreenToWorldZero(this Vector3 screenPos)
    {
        var ray = Camera.main.ScreenPointToRay(screenPos);
        var hPlane = new Plane(-Vector3.forward, Vector3.zero);

        if (hPlane.Raycast(ray, out var distance))
            return ray.GetPoint(distance);

        return Vector3.zero;
    }
}