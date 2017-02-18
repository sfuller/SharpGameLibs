using System;

namespace SFuller.SharpGameLibs.Core.IOC
{
    public interface ISystem : IInitializable
    {
        Type[] GetDependencies();
        void Shutdown();
    }

}
