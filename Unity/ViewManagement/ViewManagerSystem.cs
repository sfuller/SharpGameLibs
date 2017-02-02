using System;
using SFuller.SharpGameLibs.Core;
using SFuller.SharpGameLibs.Core.IOC;
using SFuller.SharpGameLibs.Core.ViewManagement;

namespace SFuller.SharpGameLibs.Unity.ViewManagement
{
    /// <summary>
    /// A simple system for ViewManager. You can make your own if you want to.
    /// </summary>
    public class ViewManagerSystem : IViewManagerSystem
    {
        public ViewManagerSystem(ViewRegistry registry) {
            _registry = registry;
        }

        public Type[] GetDependencies() {
            return null;
        }

        public void Init(SystemContainer container) {
            ILogger logger = container.Get<ILoggerSystem>();
            _manager.Setup(_registry, logger);
        }

        public void Shutdown() {
        }

        public T Instantiate<T>() where T : IView {
            return _manager.Instantiate<T>();
        }

        public T Instantiate<T>(uint tag) where T : IView {
            return _manager.Instantiate<T>(tag);
        }

        public void Destroy<T>(T view) where T : IView {
            _manager.Destroy(view);
        }

        private readonly ViewManager _manager = new ViewManager();
        private readonly ViewRegistry _registry;
    }
}