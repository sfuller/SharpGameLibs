using System;

namespace SFuller.SharpGameLibs.Core.IOC
{

    public enum BindingMode
    {
        System,
        WeakSystem,
        Factory
    }

    public class UnitDefinition
    {
        public UnitDefinition(Type type, Func<ISystem> creator, BindingMode mode = BindingMode.System)
        {
            Type = type;
            Creator = creator;
            Mode = mode;
        }

        public UnitDefinition(Type type, Type concreteType) {
            Type = type;
            ConcreteType = concreteType;
            Mode = BindingMode.Factory;
        }

        public readonly Type Type;
        public readonly Func<ISystem> Creator;
        public readonly Type ConcreteType;
        public BindingMode Mode;
    }
    
}
