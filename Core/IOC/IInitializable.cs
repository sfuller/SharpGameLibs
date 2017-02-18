using System;

namespace SFuller.SharpGameLibs.Core.IOC
{
    public interface IInitializable
    {
        void Init(SystemContainer systems);
    }
}
