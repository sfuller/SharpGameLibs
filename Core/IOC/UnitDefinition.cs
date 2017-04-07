using System;
using System.Collections.Generic;
using System.Text;

namespace SFuller.SharpGameLibs.Core.IOC
{
    public class UnitDefinition
    {
        public UnitDefinition(
            Type[] interfaceTypes,
            Type concreteType,
            Func<object> factory,
            BindingMode mode
        ) {
            InterfaceTypes = interfaceTypes;
            ConcreteType = concreteType;
            Factory = factory;
            Mode = mode;
        }

        public override string ToString() {
            if (InterfaceTypes == null || InterfaceTypes.Length < 1) {
                return "[]";
            }

            if (InterfaceTypes.Length == 1) {
                return InterfaceTypes[0].ToString();
            }

            var builder = new StringBuilder();
            builder.Append("[");
            for (int i = 0, ilen = InterfaceTypes.Length; i < ilen; ++i) {
                builder.Append(InterfaceTypes[i]);
                if (i < ilen - 1) {
                    builder.Append(", ");
                }
            }
            builder.Append("]");
            return builder.ToString();
        }

        public readonly Type[] InterfaceTypes;

        /// <summary>
        /// Note: Only set when binding mode is factory.
        /// </summary>
        public readonly Type ConcreteType;

        public readonly Func<object> Factory;
        public readonly BindingMode Mode;
    }    
}
