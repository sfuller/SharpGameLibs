using System;

namespace SFuller.SharpGameLibs.Core.IOC
{
    public interface IDependencyProvider {

        /// <summary>
        /// Get the dependencies of the given type.
        /// </summary>
        Type[] Get(Type type);

    }
}
