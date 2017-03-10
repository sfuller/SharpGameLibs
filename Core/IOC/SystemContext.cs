using System;
using System.Collections.Generic;

namespace SFuller.SharpGameLibs.Core.IOC
{
    public class SystemContext
    {
        public IEnumerable<UnitDefinition> Definitions {
            get {
                return m_Definitions;
            }
        }

        public void Register<InterfaceT, ConcreteT>() 
            where ConcreteT : InterfaceT, new() 
        {
            m_Definitions.Add(
                new UnitDefinition(
                    typeof(InterfaceT),
                    null,
                    MakeConcrete<ConcreteT>,
                    BindingMode.System
                )
            );    
        }

        public void Register<InterfaceT>(Func<InterfaceT> creatorFunc)  {
            m_Definitions.Add(
                new UnitDefinition(
                    typeof(InterfaceT),
                    null,
                    () => creatorFunc(),
                    BindingMode.System
                )
            );
        }

        public void Register<InterfaceT>(InterfaceT instance) {
            Register(() => instance);
        }

        public void RegisterWeak<InterfaceT>(Func<InterfaceT> creatorFunc) {
            m_Definitions.Add(
                new UnitDefinition(
                    typeof(InterfaceT),
                    null,
                    () => creatorFunc(),
                    BindingMode.WeakSystem
                )
            );
        }

        public void RegisterFactory<InterfaceT, ConcreteT>()
            where ConcreteT : InterfaceT, new()
        {
            m_Definitions.Add(
                new UnitDefinition(
                    typeof(InterfaceT),
                    typeof(ConcreteT),
                    MakeConcrete<ConcreteT>,
                    BindingMode.Factory
                )
            );
        }

        public void RegisterFactory<InterfaceT, ConcreteT>(Func<ConcreteT> creatorFunc)
            where ConcreteT : InterfaceT
        {
            m_Definitions.Add(
                new UnitDefinition(
                    typeof(InterfaceT),
                    typeof(ConcreteT),
                    () => creatorFunc(),
                    BindingMode.Factory
                )
            );
        }

        public void AddDefinition(UnitDefinition definition) {
            m_Definitions.Add(definition);
        }

        private static object MakeConcrete<IConcrete>() where IConcrete : new()
        {
            return new IConcrete();
        }

        private readonly List<UnitDefinition> m_Definitions = new List<UnitDefinition>();
    }
}