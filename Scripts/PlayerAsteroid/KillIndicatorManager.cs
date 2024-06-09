using Sirenix.OdinInspector;
using UnityEngine;

public class KillIndicatorManager : MonoBehaviour
{
      [SerializeField, Required] private GameObject killableIndicator;
      [SerializeField, Required] private GameObject dangerIndicator;

      public void SetRenderLayer(int playerId)
      {
          int layer;
          layer = LayerMask.NameToLayer($"Kill_Indicator{playerId}killable");
          killableIndicator.layer = layer;
          
          foreach (Transform child in killableIndicator.transform)
          {
              child.gameObject.layer = layer;
          }
          
          layer = LayerMask.NameToLayer($"Kill_Indicator{playerId}danger");
          dangerIndicator.layer = layer;
          
          foreach (Transform child in dangerIndicator.transform)
          {
              child.gameObject.layer = layer;
          }
      }

      public void SetActivation(bool active)
      {
          killableIndicator.SetActive(active);
          dangerIndicator.SetActive(active);
      }
}