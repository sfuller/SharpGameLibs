using System;
using System.Collections.Generic;

namespace SFuller.SharpGameLibs.Core.IOC
{
    public class Context
    {
        public IEnumerable<UnitDefinition> Definitions {
            get {
                return _definitions;
            }
        }

        public void AddDefinition(UnitDefinition definition) {
            _definitions.Add(definition);
        }

        private readonly List<UnitDefinition> _definitions = new List<UnitDefinition>();
    }

    public class ContextBuilder
    {
        public IBindingSubject Bind<T>() {
            var binding = new Binding();
            _bindings.Add(binding);
            binding.InterfaceTypes.Add(typeof(T));
            return binding;
        }

        public Context Build() {
            var context = new Context();
            for (int i = 0, ilen = _bindings.Count; i < ilen; ++i) {
                Binding binding = _bindings[i];
                context.AddDefinition(
                    new UnitDefinition(
                        binding.InterfaceTypes.ToArray(),
                        binding.ConcreteType,
                        binding.Factory,
                        binding.Mode
                    )
                );
            }
            return context;
        }

        private readonly List<Binding> _bindings = new List<Binding>();
    }

    public interface IBindingSubject
    {
        IBindingSubject And<T>();
        void ToSystem<T>() where T : new();
        void ToSystem<T>(Func<T> factory);
        void ToSystem<T>(T instance);
        void ToFactory<T>() where T : new();
        void ToFactory<T>(Func<T> factory);
    }

    public class Binding : IBindingSubject
    {
        public IBindingSubject And<T>() {
            InterfaceTypes.Add(typeof(T));
            return this;
        }

        public void ToSystem<T>() where T : new() {
            VerifyType(typeof(T));
            Factory = MakeConcrete<T>;
            Mode = BindingMode.System;
        }

        public void ToSystem<T>(Func<T> factory) {
            VerifyType(typeof(T));
            Factory = () => factory();
            Mode = BindingMode.System;
        }

        public void ToSystem<T>(T instance) {
            VerifyType(typeof(T));
            Factory = () => instance;
            Mode = BindingMode.System;
        }

        public void ToFactory<T>() where T : new() {
            VerifyType(typeof(T));
            Factory = MakeConcrete<T>;
            Mode = BindingMode.Factory;
        }

        public void ToFactory<T>(Func<T> factory) {
            VerifyType(typeof(T));
            Factory = () => factory;
            Mode = BindingMode.Factory;
        }

        private void VerifyType(Type type) {
            for (int i = 0, ilen = InterfaceTypes.Count; i < ilen; ++i) {
                Type interfaceType = InterfaceTypes[i];
                if (!interfaceType.IsAssignableFrom(type)) {
                    throw new InvalidOperationException(string.Format("Cannot assign {0} to {1}.", type, interfaceType));
                }
            }
        }

        private static object MakeConcrete<IConcrete>() where IConcrete : new() {
            return new IConcrete();
        }

        public readonly List<Type> InterfaceTypes = new List<Type>();
        public Type ConcreteType;
        public Func<object> Factory;
        public BindingMode Mode;
    }

}