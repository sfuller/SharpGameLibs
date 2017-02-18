using System;
using System.Collections.Generic;
using SFuller.SharpGameLibs.Core.IOC;

namespace SFuller.SharpGameLibs.Core.Factory
{
    public class Factory : IFactory
    {
        public Type[] GetDependencies() {
            return null;
        }

        public void Init(SystemContainer container) {
        }

        public void Shutdown() {
        }

        public T Get<T>() where T : class
        {
            object obj;
            _factories.TryGetValue(typeof(T), out obj);
            Func<T> factory = obj as Func<T>;
            return factory();
        }

        public void Register<InterfaceT, ConcreteT>() 
            where ConcreteT : InterfaceT, new()
            where InterfaceT : class
        {
            Register<InterfaceT>(() => new ConcreteT());
        }

        public void Register<InterfaceT>(Func<InterfaceT> factory) where InterfaceT : class
        {
            _factories.Add(typeof(InterfaceT), factory);
        }

        private readonly Dictionary<Type, object> _factories =
            new Dictionary<Type, object>();
    }
}
