using SFuller.SharpGameLibs.Core.ViewManagement;
using UnityEngine;

namespace SFuller.SharpGameLibs.Unity.UI
{
    public class CanvasView : MonoBehaviour, ICanvasView
    {
        public void SetHUD(IView view, int layer) {
            MonoBehaviour behaviour = view as MonoBehaviour;
            if (behaviour == null) {
                Debug.LogError("Given view cannot be used as a HUD");
                return;
            }

            var tf = behaviour.transform;
            tf.SetParent(CanvasTransform, false);
            tf.SetSiblingIndex(layer);
        }

        public Transform CanvasTransform;
    }
}
