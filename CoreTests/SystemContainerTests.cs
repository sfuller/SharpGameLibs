using System;
using NUnit.Framework;
using NSubstitute;
using SFuller.SharpGameLibs.Core.IOC;


namespace SFuller.SharpGameLibs.CoreTests
{
    [TestFixture]
    public class SystemContainerTests
    {
        public interface ITestSystem1 {}
        public interface ITestSystem2 {}
        public interface ITestSystem3 {}
        public interface ITestSystem4 {}

        private ITestSystem1 _system1;
        private ITestSystem2 _system2;
        private ITestSystem3 _system3;
        private ITestSystem4 _system4;
        private IDependencyProvider _depends;
        private SystemContext _context;
        private SystemContainer _container;

        [SetUp]
        public void Setup()
        {
            _system1 = Substitute.For<ITestSystem1>();
            _system2 = Substitute.For<ITestSystem2>();
            _system3 = Substitute.For<ITestSystem3>();
            _system4 = Substitute.For<ITestSystem4>();
            _depends = Substitute.For<IDependencyProvider>();
            _context = new SystemContext();
            _container = new SystemContainer(_depends);
            _container.SetContext(_context);
        }

        [Test]
        public void TestCircularDependency()
        {
            _depends.Get(_system1.GetType()).Returns(new Type[] { typeof(ITestSystem2) });
            _depends.Get(_system2.GetType()).Returns(new Type[] { typeof(ITestSystem1) });

            _context.Register(_system1);
            _context.Register(_system2);
            Assert.IsTrue(_container.Init().Status == ContainerInitStatus.CircularDependency);
        }

        [Test]
        public void TestTriangularDependency()
        {
            _depends.Get(_system1.GetType()).Returns(new Type[] { typeof(ITestSystem2) });
            _depends.Get(_system2.GetType()).Returns(new Type[] { typeof(ITestSystem3) });
            _depends.Get(_system3.GetType()).Returns(new Type[] { typeof(ITestSystem1) });

            _context.Register(_system1);
            _context.Register(_system2);
            _context.Register(_system3);
            Assert.IsTrue(_container.Init().Status == ContainerInitStatus.CircularDependency);
        }

        [Test]
        public void TestMissingDependency()
        {
            _depends.Get(_system1.GetType()).Returns((Type[])null);
            _depends.Get(_system3.GetType()).Returns(new Type[] { typeof(ITestSystem2) });

            _context.Register(_system1);
            _context.Register(_system3);
            Assert.IsTrue(_container.Init().Status == ContainerInitStatus.MissingDependencies);
        }

        [Test]
        public void TestSystemsNotDuplicated()
        {
            _system1 = Substitute.For<ITestSystem1, IInitializable>();
            _system2 = Substitute.For<ITestSystem2, IInitializable>();
            _system3 = Substitute.For<ITestSystem3, IInitializable>();
            _system4 = Substitute.For<ITestSystem4, IInitializable>();

            _depends.Get(_system1.GetType()).Returns((Type[])null);
            _depends.Get(_system2.GetType()).Returns(new Type[] { typeof(ITestSystem1) });
            _depends.Get(_system3.GetType()).Returns(new Type[] { typeof(ITestSystem1), typeof(ITestSystem2) });
            _depends.Get(_system4.GetType()).Returns(new Type[] { typeof(ITestSystem2), typeof(ITestSystem3) });

            _context.Register(_system1);
            _context.Register(_system2);
            _context.Register(_system3);
            _context.Register(_system4);
            Assert.IsTrue(_container.Init().Status == ContainerInitStatus.Ok);

            ((IInitializable)_system1.ReceivedWithAnyArgs(1)).Init(null);
            ((IInitializable)_system2.ReceivedWithAnyArgs(1)).Init(null);
            ((IInitializable)_system3.ReceivedWithAnyArgs(1)).Init(null);
            ((IInitializable)_system4.ReceivedWithAnyArgs(1)).Init(null);
        }

        [Test]
        public void TestSystemsAndFactories()
        {
            _depends.Get(_system1.GetType()).Returns((Type[])null);
            _depends.Get(_system2.GetType()).Returns(new Type[] { typeof(ITestSystem1) });
            _depends.Get(_system3.GetType()).Returns(new Type[] { typeof(ITestSystem2) });

            _context.Register(_system1);
            _context.AddDefinition(new UnitDefinition(typeof(ITestSystem2), _system2.GetType(), () => _system2, BindingMode.Factory));
            _context.Register(_system3);
            Assert.IsTrue(_container.Init().Status == ContainerInitStatus.Ok);
        }

        [Test]
        public void TestWeakSystemsNotReInitialized()
        {
            ITestSystem1 system1 = Substitute.For<ITestSystem1, IInitializable>();
            _context.Register(system1);
            Assert.IsTrue(_container.Init().Status == ContainerInitStatus.Ok);
            var container2 = new SystemContainer(_depends);
            var context2 = new SystemContext();
            _container.RegisterToContextAsWeak(context2);
            container2.SetContext(context2);
            Assert.IsTrue(container2.Init().Status == ContainerInitStatus.Ok);
            ((IInitializable)system1).ReceivedWithAnyArgs(1).Init(null);
        }

        [Test]
        public void TestFactoriesFromDerivedContext()
        {
            ITestSystem1 system1 = Substitute.For<ITestSystem1, IInitializable>();
            _context.AddDefinition(new UnitDefinition(typeof(ITestSystem1), system1.GetType(), () => system1, BindingMode.Factory));
            Assert.IsTrue(_container.Init().Status == ContainerInitStatus.Ok);
            var container2 = new SystemContainer(_depends);
            var context2 = new SystemContext();
            _container.RegisterToContextAsWeak(context2);
            container2.SetContext(context2);
            Assert.IsTrue(container2.Init().Status == ContainerInitStatus.Ok);
            Assert.IsNotNull(container2.Get<ITestSystem1>());
        }

        public enum TestEnum
        {
            Option1,
            Option2,
            Option3
        }

        [Test]
        public void TestEnumSystemBinding()
        {
            _context.Register<TestEnum>(TestEnum.Option2);
            Assert.IsTrue(_container.Init().Status == ContainerInitStatus.Ok);
            Assert.AreEqual(TestEnum.Option2, _container.Get<TestEnum>());
        }

        [Test]
        public void TestEnumFactoryBinding()
        {
            _context.AddDefinition(new UnitDefinition(typeof(TestEnum), typeof(TestEnum), () => TestEnum.Option2, BindingMode.Factory));
            Assert.IsTrue(_container.Init().Status == ContainerInitStatus.Ok);
            Assert.AreEqual(TestEnum.Option2, _container.Get<TestEnum>());
        }

        public interface IGenericSystem<T> {}

        [Test]
        public void TestMultipleGenericBindings()
        {
            IGenericSystem<int> system1 = Substitute.For<IGenericSystem<int>>();
            IGenericSystem<float> system2 = Substitute.For<IGenericSystem<float>>();
            _context.Register<IGenericSystem<int>>(system1);
            _context.Register<IGenericSystem<float>>(system2);
            Assert.IsTrue(_container.Init().Status == ContainerInitStatus.Ok);
            Assert.AreEqual(system1, _container.Get<IGenericSystem<int>>());
            Assert.AreEqual(system2, _container.Get<IGenericSystem<float>>());
        }

    }
}