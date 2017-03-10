using System;
using SFuller.SharpGameLibs.Core.IOC;

namespace SFuller.SharpGameLibs.Core.Logging
{
    public class NullLogger : ILogger {
        public void LogError(string message) { }
        public void LogWarning(string message) { }
    }
}