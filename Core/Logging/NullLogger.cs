using System;
using SFuller.SharpGameLibs.Core.IOC;

namespace SFuller.SharpGameLibs.Core.Logging
{
    public class NullLogger : ILogger {

        public Type[] GetDependencies(){
            return null;
        }

        public void Init(SystemContainer systems) {
        }

        public void Shutdown() {
        }

        public void LogError(string message) { }
        public void LogWarning(string message) { }

    }
}