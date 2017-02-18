using System;
using System.Collections.Generic;

namespace SFuller.SharpGameLibs.Core.IOC
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple=false, Inherited=true)]
    public class DependenciesAttribute : Attribute
    {
        public DependenciesAttribute(Type[] dependencies)
        {
            Dependencies = dependencies;
        }

        public readonly Type[] Dependencies;
    }
}
