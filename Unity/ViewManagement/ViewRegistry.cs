using System;
using System.Collections.Generic;
using UnityEngine;

namespace SFuller.SharpGameLibs.Unity.ViewManagement
{
    [Serializable]
    public class BindingTarget {
        public int Tag;
        public GameObject Prefab;
    }

    [Serializable]
    public class TypeBinding {
        public string TypeName;
        public List<BindingTarget> Targets = new List<BindingTarget>();
    }

    [CreateAssetMenu()]
    public class ViewRegistry : ScriptableObject {
        public List<TypeBinding> Bindings = new List<TypeBinding>();
    }
}