using System;
using System.Collections.Generic;

namespace SFuller.SharpGameLibs.Core.IOC
{
    public class SystemContext
    {
        public void Register<InterfaceT, ConcreteT>() 
            where InterfaceT : ISystem
            where ConcreteT : InterfaceT, new() 
        {
            m_Definitions.Add(new SystemDefinition(typeof(InterfaceT), MakeConcrete<ConcreteT>));    
        }

        public void Register<InterfaceT>(Func<InterfaceT> creatorFunc) where InterfaceT : ISystem
        {
            m_Definitions.Add(new SystemDefinition(typeof(InterfaceT), () => creatorFunc()));
        }

        public void RegisterWeak<InterfaceT>(Func<InterfaceT> creatorFunc) where InterfaceT : ISystem
        {
            m_Definitions.Add(new SystemDefinition (
                    typeof(InterfaceT),
                    () => creatorFunc(),
                    true
                )
            );
        }

        public IEnumerable<SystemDefinition> Definitions
        {
            get
            {
                return m_Definitions;
            }
        }

        public void RegisterWeak(SystemContainer systems) {
            var it = systems.Systems.GetEnumerator();
            while(it.MoveNext()) {
                KeyValuePair<Type, ISystem> kvp = it.Current;
                m_Definitions.Add(new SystemDefinition (
                    kvp.Key,
                    () => kvp.Value,
                    true
                ));
            }
        }

        private static ISystem MakeConcrete<IConcrete>() where IConcrete : ISystem, new()
        {
            return new IConcrete();
        }

        private readonly List<SystemDefinition> m_Definitions = new List<SystemDefinition>();
    }
}