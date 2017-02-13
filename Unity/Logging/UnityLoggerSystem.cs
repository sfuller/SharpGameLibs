using System;
using SFuller.SharpGameLibs.Core;
using SFuller.SharpGameLibs.Core.IOC;

namespace SFuller.SharpGameLibs.Unity.Logging
{
    public class UnityLoggerSystem : UnityLogger, ILoggerSystem
    {
        public Type[] GetDependencies() {
            return null;
        }

        public void Init(SystemContainer container) { }
        public void Shutdown() { }
    }
}
