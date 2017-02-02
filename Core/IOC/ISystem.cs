using System;

namespace SFuller.SharpGameLibs.Core.IOC
{
    public interface ISystem
    {
        Type[] GetDependencies();
        void Init(SystemContainer container);
        void Shutdown();
    }

}
