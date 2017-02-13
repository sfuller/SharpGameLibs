using System;
using SFuller.SharpGameLibs.Core.UI;
using SFuller.SharpGameLibs.Core.ViewManagement;
using SFuller.SharpGameLibs.Core.IOC;
using UnityEngine;

namespace SFuller.SharpGameLibs.Unity.UI
{
    public class HUDManager : IHUDManager
    {
        public Type[] GetDependencies() {
            return new Type[] {
                typeof(IViewManagerSystem)
            };
        }

        public void Init(SystemContainer systems) {
            _views = systems.Get<IViewManagerSystem>();

            _canvasView = _views.Instantiate<ICanvasView>();
        }

        public void Shutdown() {
            _views.Destroy(_canvasView);
        }

        public void SetHUD(IView view) {
            DisableView(_currentHUD);
            _canvasView.SetHUD(view);
            _currentHUD = view;
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

        private void EnableView(IView view)
        {
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

        private IView _currentHUD;
    }
}
