using System;
using SFuller.SharpGameLibs.Core;
using SFuller.SharpGameLibs.Core.IOC;
using UnityEngine;

namespace SFuller.SharpGameLibs.Unity.Logging
{
    public class UnityLogger : Core.Logging.ILogger
    {
        public Type[] GetDependencies() {
            return null;
        }

        public void Init(SystemContainer systems) {
        }

        public void Shutdown() {
        }

        public void LogError(string message) {
            Debug.LogError(message);
        }

        public void LogWarning(string message) {
            Debug.LogWarning(message);
        }

    }
}
