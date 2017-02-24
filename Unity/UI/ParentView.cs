using System;
using SFuller.SharpGameLibs.Core.UI;
using SFuller.SharpGameLibs.Core.ViewManagement;
using UnityEngine;

namespace SFuller.SharpGameLibs.Unity.UI
{
    public class ParentView : MonoBehaviour, IParentView {

        public void AddChild(IView view) {
            MonoBehaviour behaviour = view as MonoBehaviour;
            if (behaviour == null)
            {
                return;
            }

            var tf = behaviour.transform;
            tf.SetParent(_tf, false);
            tf.SetAsLastSibling();
        }

        private void Awake()
        {
            _tf = transform;
        }

        private Transform _tf;
    }
}
