using System;

namespace SFuller.SharpGameLibs.Core.IOC
{
    class DependencyProvider : IDependencyProvider
    {
        public Type[] Get(Type type) {
            object[] attributes = type.GetCustomAttributes(typeof(DependenciesAttribute), true);
            if (attributes.Length > 0) {
                DependenciesAttribute attribute = (DependenciesAttribute)attributes[0];
                return attribute.Dependencies;
            }
            return null;
        }
    }
}
