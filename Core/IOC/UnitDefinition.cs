using System;

namespace SFuller.SharpGameLibs.Core.IOC
{
    public class UnitDefinition
    {
        public UnitDefinition(
            Type interfaceType,
            Type concreteType,
            Func<object> factory,
            BindingMode mode
        ) {
            InterfaceType = interfaceType;
            ConcreteType = concreteType;
            Factory = factory;
            Mode = mode;
        }

        public readonly Type InterfaceType;

        /// <summary>
        /// Note: Only set when binding mode is factory.
        /// </summary>
        public readonly Type ConcreteType;

        public readonly Func<object> Factory;
        public readonly BindingMode Mode;
    }    
}
