using System;
using SFuller.SharpGameLibs.Core.IOC;

namespace SFuller.SharpGameLibs.Core.Factory
{
    public interface IFactory : ISystem
    {
        T Get<T>() where T : class;
    }
}
