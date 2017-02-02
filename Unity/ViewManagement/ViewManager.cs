using System;
using System.Collections.Generic;
using SFuller.SharpGameLibs.Core;
using SFuller.SharpGameLibs.Core.ViewManagement;
using UnityEngine;

namespace SFuller.SharpGameLibs.Unity.ViewManagement
{
    class Binding {
        public List<BindingTarget> Targets = new List<BindingTarget>();
    }

    public class ViewManager : IViewManager
    {
        public void Setup(ViewRegistry registry, Core.ILogger logger) {
            if (logger == null) {
                logger = new NullLogger();
            }
            _logger = logger;

            foreach (TypeBinding bindingData in registry.Bindings) {
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
        }

        public T Instantiate<T>() where T : IView {
            Binding binding = _bindings[typeof(T)];
            BindingTarget target = binding.Targets[0];
            GameObject obj = GameObject.Instantiate(target.Prefab);
            return GetComponent<T>(obj);
        }

        public T Instantiate<T>(uint tag) where T : IView {
            Binding binding = _bindings[typeof(T)];
            BindingTarget target = binding.Targets.Find(x => x.Tag == tag);
            GameObject obj = GameObject.Instantiate(target.Prefab);
            return GetComponent<T>(obj);
        }

        public void Destroy<T>(T view) where T : IView {
            MonoBehaviour behaviour = view as MonoBehaviour;
            GameObject.Destroy(behaviour.gameObject);
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

        private Core.ILogger _logger;
        private readonly Dictionary<Type, Binding> _bindings = new Dictionary<Type, Binding>();        
    }

}