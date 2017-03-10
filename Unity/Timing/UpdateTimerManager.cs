using System;
using System.Collections.Generic;
using SFuller.SharpGameLibs.Core.IOC;
using SFuller.SharpGameLibs.Core.Timing;
using SFuller.SharpGameLibs.Core.Update;
using UnityEngine;

namespace SFuller.SharpGameLibs.Unity.Timing
{
    [Dependencies(new Type[] {
        typeof(IUpdateManager)
    })]

    public sealed class UpdateTimerManager : ITimerManager, IUpdatable, IDisposable
    {
        public void Init(IIOCProvider container)
        {
            _updates = container.Get<IUpdateManager>();
            _updates.Register(this);
        }

        public void Dispose()
        {
            _updates.Unregister(this);
            _times.Clear();
        }

        public float SetTimer(float duration, TimerCallback callback)
        {
            float signature = Time.time + duration;
            _times.Add(signature, callback);
            return signature;
        }

        public void Update(float timestep)
        {
            var timers = new List<KeyValuePair<float, TimerCallback>>();
            float currentTime = Time.time;
            foreach (var pair in _times)
            {
                if (pair.Key > currentTime)
                {
                    break;
                }
                timers.Add(pair);
            }
            foreach (var pair in timers)
            {
                _times.Remove(pair.Key);
            }
            foreach (var pair in timers)
            {
                pair.Value(pair.Key);
            }
        }

        private IUpdateManager _updates;
        private SortedDictionary<float, TimerCallback> _times =
            new SortedDictionary<float, TimerCallback>();
    }
}
