using System;
using System.Collections.Generic;
using SFuller.SharpGameLibs.Core.UI;
using SFuller.SharpGameLibs.Core.ViewManagement;
using SFuller.SharpGameLibs.Core.IOC;
using UnityEngine;

namespace SFuller.SharpGameLibs.Unity.UI
{
    [Dependencies(new Type[]
    {
        typeof(IViewManager)
    })]

    public sealed class UIManager : IUIManager, IInitializable, IDisposable
    {
        public void Init(IIOCProvider systems) {
            _views = systems.Get<IViewManager>();

            _canvasView = _views.Instantiate<ICanvasView>();
        }

        public void Dispose() {
            _views.Destroy(_canvasView);
        }

        public void SetHUD(IView view, int layer) {
            IView currentView;
            if (_huds.TryGetValue(layer, out currentView))
            {
                DisableView(currentView);
            }
            _huds[layer] = view;
            _canvasView.SetHUD(view, layer);
            EnableView(view);
        }

        private void DisableView(IView view) {
            if (view == null) {
                return;
            }

            MonoBehaviour behaviour = view as MonoBehaviour;
            if (behaviour == null) {
                return;
            }

            behaviour.gameObject.SetActive(false);
        }

        private void EnableView(IView view) {
            if (view == null) {
                return;
            }

            MonoBehaviour behaviour = view as MonoBehaviour;
            if (behaviour == null) {
                return;
            }

            behaviour.gameObject.SetActive(true);
        }


        private IViewManager _views;

        private ICanvasView _canvasView;
        private Dictionary<int, IView> _huds = new Dictionary<int, IView>();
    }
}
