﻿using SFuller.SharpGameLibs.Core.IOC;
using SFuller.SharpGameLibs.Core.Update;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SFuller.SharpGameLibs.Unity.Update
{
    public class UpdateManager : IUpdateManager
    {
        private enum RegistrationActionType {
            Add,
            Remove
        }

        private struct RegistrationAction {
            public RegistrationActionType Type;
            public IUpdatable Updatable;
        }

        public Type[] GetDependencies() {
            return null;
        }

        public void Init(SystemContainer container) {
            _obj = new GameObject("UpdateManager");
            UpdateBehaviour behaviour = _obj.AddComponent<UpdateBehaviour>();
            behaviour.StartCoroutine(UpdateCoroutine());
        }

        public void Shutdown() {
            _actions.Clear();
            _items.Clear();
            GameObject.Destroy(_obj);
        }

        public void Register(IUpdatable updatable) {
            _actions.Add(new RegistrationAction() {
                Type = RegistrationActionType.Add,
                Updatable = updatable
            });
        }

        public void Unregister(IUpdatable updatable) {
            _actions.Add(new RegistrationAction() {
                Type = RegistrationActionType.Remove,
                Updatable = updatable
            });
        }

        private IEnumerator UpdateCoroutine() {
            for (;;) {
                Update();
                yield return null;
            }
        }

        private void Update() {
            for (int i = 0, ilen = _actions.Count; i < ilen; ++i) {
                RegistrationAction action = _actions[i];
                if (action.Type == RegistrationActionType.Add) {
                    _items.Add(action.Updatable);
                }
                else {
                    _items.Remove(action.Updatable);
                }
            }
            _actions.Clear();

            for (int i = 0, ilen = _items.Count; i < ilen; ++i) {
                IUpdatable item = _items[i];
                item.Update(Time.deltaTime);
            }
        }

        private GameObject _obj;
        private readonly List<RegistrationAction> _actions = new List<RegistrationAction>();
        private readonly List<IUpdatable> _items = new List<IUpdatable>(4);
    }
}
