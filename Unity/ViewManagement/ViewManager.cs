using System;
using System.Collections.Generic;
using SFuller.SharpGameLibs.Core.IOC;
using SFuller.SharpGameLibs.Core.Logging;
using SFuller.SharpGameLibs.Core.ViewManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SFuller.SharpGameLibs.Unity.ViewManagement
{
    class Binding {
        public List<BindingTarget> Targets = new List<BindingTarget>();
    }

    public class ViewManager : IViewManager, IInitializable
    {
        public ViewManager(ViewRegistry registry) {
            _registry = registry;
        }

        public Type[] GetDependencies() {
            return null;
        }

        public void Init(IIOCProvider systems) {
            _logger = systems.Get<Core.Logging.ILogger>();
            if (_logger == null) {
                _logger = new NullLogger();
            }

            foreach (TypeBinding bindingData in _registry.Bindings) {
                Type type = Type.GetType(bindingData.TypeName);
                if (type == null) {
                    _logger.LogError(string.Format(
                        "Could not get type for qualified name '{0}'",
                        bindingData.TypeName
                    ));
                    continue;
                }
                var binding = new Binding();
                binding.Targets.AddRange(bindingData.Targets);
                _bindings.Add(type, binding);
            }

            _scene = SceneManager.CreateScene("Managed_by_View_Manager");
        }

        public T Instantiate<T>() where T : IView {
            Binding binding = _bindings[typeof(T)];
            BindingTarget target = binding.Targets[0];
            GameObject obj = InstantiatePrefab(target.Prefab);
            return GetComponent<T>(obj);
        }

        public T Instantiate<T>(uint tag) where T : IView {
            Binding binding = _bindings[typeof(T)];
            BindingTarget target = binding.Targets.Find(x => x.Tag == tag);
            GameObject obj = InstantiatePrefab(target.Prefab);
            return GetComponent<T>(obj);
        }

        public void Destroy<T>(T view) where T : IView {
            MonoBehaviour behaviour = view as MonoBehaviour;
            if (behaviour != null) {
                GameObject.Destroy(behaviour.gameObject);
            }
        }

        private GameObject InstantiatePrefab(GameObject prefab) {
            GameObject obj = GameObject.Instantiate(prefab);
            SceneManager.MoveGameObjectToScene(obj, _scene);
            return obj;
        }


        private T GetComponent<T>(GameObject obj) where T : IView {
            T view = obj.GetComponent<T>();
            if (view == null) {
                _logger.LogWarning(string.Format(
                    "No component implementing {0} found on game object {1}",
                    typeof(T).FullName, obj.name
                ));
            }
            return view;
        }

        private readonly ViewRegistry _registry;
        private Core.Logging.ILogger _logger;
        private readonly Dictionary<Type, Binding> _bindings = new Dictionary<Type, Binding>();
        private Scene _scene;
    }

}