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
        public event Action<object, EventArgs> Ready;

        public void Init(IIOCProvider systems) {
            _views = systems.Get<IViewManager>();

            var resourceGroup = new ResourceGroup();
            resourceGroup.Add<ICanvasView>();
            _viewResourceHandle = _views.AddResourceGroup(resourceGroup);
        }

        public void Load() {
            _viewResourceHandle.Loaded += HandleResourcesLoaded;
            _viewResourceHandle.Load();
        }

        public void Dispose() {
            _views.Destroy(_canvasView);
            _viewResourceHandle.Unload();
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

        private void HandleResourcesLoaded(object sender, EventArgs args) {
            _viewResourceHandle.Loaded -= HandleResourcesLoaded;
            _canvasView = _views.Instantiate<ICanvasView>();
            HandleReady();
        }

        private void HandleReady() {
            var handler = Ready;
            if (handler != null) {
                handler(this, EventArgs.Empty);
            }
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

        private IResourceGroupHandle _viewResourceHandle;
        private ICanvasView _canvasView;
        private Dictionary<int, IView> _huds = new Dictionary<int, IView>();
    }
}
