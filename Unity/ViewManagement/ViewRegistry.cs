using System;
using System.Collections.Generic;
using UnityEngine;

namespace SFuller.SharpGameLibs.Unity.ViewManagement
{
    public enum TargetType {
        PreloadedPrefab,
        Resource
        // TODO: Support asset bundles
    }

    [Serializable]
    public class BindingTarget {
        public int Tag;
        public TargetType Type;
        public GameObject Prefab;
        public string ResourcePath;
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