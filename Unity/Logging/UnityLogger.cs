using System;
using SFuller.SharpGameLibs.Core;
using UnityEngine;

namespace SFuller.SharpGameLibs.Unity.Logging
{
    public class UnityLogger : Core.ILogger
    {
        public void LogError(string message) {
            Debug.LogError(message);
        }

        public void LogWarning(string message) {
            Debug.LogWarning(message);
        }
    }
}
