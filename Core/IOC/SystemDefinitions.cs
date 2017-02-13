using System;

namespace SFuller.SharpGameLibs.Core.IOC
{

    public class SystemDefinition
    {
        public SystemDefinition(Type type, Func<ISystem> creator, bool isWeak = false)
        {
            Type = type;
            Creator = creator;
            IsWeak = isWeak;
        }

        public readonly Type Type;
        public readonly Func<ISystem> Creator;
        public readonly bool IsWeak;
    }
    
}
