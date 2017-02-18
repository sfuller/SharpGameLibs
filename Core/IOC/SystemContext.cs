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
            where InterfaceT : ISystem
            where ConcreteT : InterfaceT, new() 
        {
            m_Definitions.Add(new UnitDefinition(typeof(InterfaceT), MakeConcrete<ConcreteT>));    
        }

        public void Register<InterfaceT>(Func<InterfaceT> creatorFunc) 
            where InterfaceT : ISystem
        {
            m_Definitions.Add(new UnitDefinition(typeof(InterfaceT), () => creatorFunc()));
        }

        public void RegisterWeak<InterfaceT>(Func<InterfaceT> creatorFunc)
            where InterfaceT : ISystem
        {
            m_Definitions.Add(
                new UnitDefinition (
                    typeof(InterfaceT),
                    () => creatorFunc(),
                    BindingMode.WeakSystem
                )
            );
        }

        public void RegisterFactory<InterfaceT, ConcreteT>()
            where InterfaceT : IInitializable
            where ConcreteT : InterfaceT, new()
        {
            m_Definitions.Add(
                new UnitDefinition(
                    typeof(InterfaceT),
                    typeof(ConcreteT)
                )
            );
        }

        public void AddDefinition(UnitDefinition definition) {
            m_Definitions.Add(definition);
        }

        //public void RegisterWeak(SystemContainer systems) {
        //    var it = systems.Systems.GetEnumerator();
        //    while(it.MoveNext()) {
        //        KeyValuePair<Type, ISystem> kvp = it.Current;
        //        m_Definitions.Add(new SystemDefinition (
        //            kvp.Key,
        //            () => kvp.Value,
        //            BindingMode.WeakSystem
        //        ));
        //    }
        //}

        private static ISystem MakeConcrete<IConcrete>() where IConcrete : ISystem, new()
        {
            return new IConcrete();
        }

        private readonly List<UnitDefinition> m_Definitions = new List<UnitDefinition>();
    }
}