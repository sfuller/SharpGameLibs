using System;

namespace SFuller.SharpGameLibs.IOC
{
    public interface ISystem
    {
        Type[] GetDependencies();
        void Init(SystemContainer container);
        void Shutdown();
    }

}
