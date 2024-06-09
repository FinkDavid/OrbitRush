using UnityEngine;

public class IngameUIManager : MonoBehaviour
{
    [SerializeField]
    GameObject[] overlays = new GameObject[4];

    public void ChangeOverlay(int playerCount)
    {
        foreach(var overlay in overlays)
        {
            overlay.SetActive(false);
        }
        overlays[playerCount - 1].SetActive(true);
    }
}
