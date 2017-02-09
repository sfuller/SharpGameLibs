using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SFuller.SharpGameLibs.Core.ViewManagement;
using UnityEditor;
using UnityEngine;

namespace SFuller.SharpGameLibs.Unity.ViewManagement {

    [CustomEditor(typeof(ViewRegistry))]
    public class ViewRegistryEditor : Editor {

        private class TagTypeData {
            public readonly string[] DisplayNames;
            public readonly List<int> Values = new List<int>();

            public TagTypeData(Type type) {
                var displayNames = new List<string>(4);
                Array values = Enum.GetValues(type);
                foreach(object obj in values) {
                    int val = unchecked((int)obj);
                    Values.Add(val);
                    displayNames.Add(Enum.GetName(type, obj));
                }
                DisplayNames = displayNames.ToArray();
            }
        }

        private class BindingView {
            public bool IsFoldOpen;
            public int TypeIndex;
            public TypeBinding Binding;
            public List<TargetView> TargetViews = new List<TargetView>();
        }

        private class TargetView {
            public int TagIndex;
            public BindingTarget Target;
        }

        public ViewRegistryEditor() {
            _viewInterfaceType = typeof(IView);
            _typeDisplayNames.Add("None");

            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (Assembly assembly in assemblies) {
                AddTypesFromAssembly(assembly);
            }
            _typeDisplayNamesArray = _typeDisplayNames.ToArray();
        }

        public override void OnInspectorGUI() {
            serializedObject.Update();
            
            ViewRegistry registry = (ViewRegistry)target;
            Setup(registry);

            if (_types.Count < 1) {
                EditorGUILayout.LabelField(
                    "No IView interfaces found."
                );
                return;
            } 

            foreach (BindingView view in _editorViews) {
                DrawBinding(view);
            }

            List<TypeBinding> bindings = registry.Bindings;
            
            if (GUILayout.Button("+")) {
                AddNewBinding(bindings);
            }
            
            if (_bindingToRemove != null) {
                int bindingIndex = _editorViews.IndexOf(_bindingToRemove);
                BindingView view = _editorViews[bindingIndex];
                bindings.Remove(view.Binding);
                _editorViews.RemoveAt(bindingIndex);
                _bindingToRemove = null;
            }

            serializedObject.ApplyModifiedProperties();

            // TODO: Only set dirty when data is changed.
            EditorUtility.SetDirty(registry);
        }

        private void AddNewBinding(List<TypeBinding> bindings) {
            var binding = new TypeBinding();
            var view = new BindingView() {
                Binding = binding
            };
            bindings.Add(binding);
            _editorViews.Add(view);
        }

        private void DrawBinding(BindingView view) {
            TypeBinding binding = view.Binding;

            // Fold
            EditorGUILayout.BeginHorizontal();
            bool isFoldOpen = view.IsFoldOpen;
            string foldName = _typeDisplayNamesArray[view.TypeIndex];
            isFoldOpen = EditorGUILayout.Foldout(isFoldOpen, foldName);
            view.IsFoldOpen = isFoldOpen;
            
            // Add button
            if (isFoldOpen) {
                if (GUILayout.Button("Add Target")) {
                    var target = new BindingTarget();
                    binding.Targets.Add(target);
                    view.TargetViews.Add(new TargetView(){ Target = target });
                }
            }
            
            if (GUILayout.Button("-", GUILayout.MaxWidth(32))) {
                _bindingToRemove = view;
            }
            
            EditorGUILayout.EndHorizontal();
            
            // Fold Contents
            if (isFoldOpen) {

                // Type selector
                int oldSelectedIndex = view.TypeIndex;
                int selectedIndex = EditorGUILayout.Popup(
                    oldSelectedIndex,
                    _typeDisplayNamesArray
                );
                Type bindingType = GetTypeFromIndex(selectedIndex);
                if (selectedIndex != oldSelectedIndex) {
                    if (bindingType != null) {
                        binding.TypeName = bindingType.AssemblyQualifiedName;
                    }
                    view.TypeIndex = selectedIndex;
                }

                ++EditorGUI.indentLevel;
                if (bindingType != null) {
                    foreach (TargetView targetView in view.TargetViews) {
                        DrawTarget(targetView, bindingType);
                    }
                }

                if (_targetToRemove != null) {
                    binding.Targets.Remove(_targetToRemove.Target);
                    view.TargetViews.Remove(_targetToRemove);
                    _targetToRemove = null;
                }
                --EditorGUI.indentLevel;
                EditorGUILayout.Space();
            }
        }

        private void DrawTarget(TargetView view, Type viewType) {
            EditorGUILayout.BeginHorizontal();
            
            BindingTarget target = view.Target;

            // Tag Selector
            TagTypeData tagData;
            if (_tagTypes.TryGetValue(viewType, out tagData)) {
                int previouslySelected = view.TagIndex;
                int selectedIndex = EditorGUILayout.Popup(
                    previouslySelected,
                    tagData.DisplayNames
                );
                if (selectedIndex != previouslySelected) {
                    view.TagIndex = selectedIndex;
                    target.Tag = tagData.Values[selectedIndex];
                }
            }
            
            // Prefab field
            target.Prefab = EditorGUILayout.ObjectField(
                target.Prefab, typeof(GameObject),
                allowSceneObjects: false
            ) as GameObject;

            // Remove button            
            if (GUILayout.Button("-", GUILayout.MaxWidth(32))) {
                _targetToRemove = view;
            }

            EditorGUILayout.EndHorizontal();
        }

        private void Setup(ViewRegistry view) {
            if (_view == view) {
                return;
            }

            foreach (TypeBinding binding in view.Bindings) {
                int typeIndex = GetIndexFromQualifiedTypeName(binding.TypeName);
                Type bindingType = GetTypeFromIndex(typeIndex);
                var bindingView = new BindingView() {
                    Binding = binding,
                    TypeIndex = typeIndex
                };

                TagTypeData tagData = null;
                if (bindingType != null) {
                    _tagTypes.TryGetValue(bindingType, out tagData);
                }

                foreach (BindingTarget target in binding.Targets) {
                    int tagIndex = 0;
                    if (tagData != null) {
                        tagIndex = tagData.Values.IndexOf(target.Tag);
                    }
                    var targetView = new TargetView() {
                        Target = target,
                        TagIndex = tagIndex
                    };
                    bindingView.TargetViews.Add(targetView);
                }
                _editorViews.Add(bindingView);
            }
            _view = view;
        }

        private int GetIndexFromQualifiedTypeName(string name) {
            for (int i = 0, ilen = _types.Count; i < ilen; ++i) {
                Type type = _types[i];
                if (name == type.AssemblyQualifiedName) {
                    return i + 1;
                }
            }
            return 0;
        }

        private Type GetTypeFromIndex(int index) {
            if (index < 1) {
                return null;
            }
            return _types[index - 1];
        }

        private void AddTypesFromAssembly(Assembly assembly) {
            Type[] types = assembly.GetTypes();
            foreach (Type type in types) {
                AddViewInterfaceType(type);
                AddViewTagsType(type);
            }
        }

        private void AddViewInterfaceType(Type type) {
            if (!type.IsInterface) {
                return;
            }
            Type[] interfaces = type.GetInterfaces();
            if (interfaces.Contains(_viewInterfaceType)) {
                _types.Add(type);
                _typeDisplayNames.Add(type.FullName);
            }
        }

        private void AddViewTagsType(Type type) {
            object[] attributes = type.GetCustomAttributes(typeof(ViewTagsAttribute), false);
            foreach(object obj in attributes) {
                ViewTagsAttribute attr = obj as ViewTagsAttribute;
                if (attr == null) {
                    continue;
                }
                _tagTypes.Add(attr.ViewType, new TagTypeData(type));
            }
        }

        private readonly Type _viewInterfaceType;
        private readonly List<Type> _types = new List<Type>();
        private readonly List<string> _typeDisplayNames = new List<string>();
        private readonly string[] _typeDisplayNamesArray;
        private readonly Dictionary<Type, TagTypeData> _tagTypes = new Dictionary<Type, TagTypeData>();
        
        private ViewRegistry _view;
        private readonly List<BindingView> _editorViews = new List<BindingView>(4);
        private BindingView _bindingToRemove;
        private TargetView _targetToRemove;

    }
}
