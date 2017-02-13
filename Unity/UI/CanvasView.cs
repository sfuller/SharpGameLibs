using SFuller.SharpGameLibs.Core.ViewManagement;
using UnityEngine;

namespace SFuller.SharpGameLibs.Unity.UI
{
    public class CanvasView : MonoBehaviour, ICanvasView
    {
        public void SetHUD(IView view) {
            MonoBehaviour behaviour = view as MonoBehaviour;
            if (behaviour == null) {
                Debug.LogError("Given view cannot be used as a HUD");
                return;
            }

            behaviour.transform.SetParent(_canvasTransform, false);
        }

        [SerializeField] private Transform _canvasTransform;
    }
}
