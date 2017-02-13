using System;

namespace SFuller.SharpGameLibs.Core.IOC
{
    class SystemInfo
    {
        public SystemInfo(Type type, ISystem system)
        {
            Type = type;
            Dependencies = system.GetDependencies();
            System = system;
        }
        
        public readonly Type Type;
        public readonly Type[] Dependencies;
        public readonly ISystem System;
    }
}