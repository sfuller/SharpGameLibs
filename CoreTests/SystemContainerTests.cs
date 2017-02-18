using System;
using NUnit.Framework;
using NSubstitute;
using SFuller.SharpGameLibs.Core.IOC;


namespace SFuller.SharpGameLibs.CoreTests
{
    [TestFixture]
    public class SystemContainerTests
    {
        public interface ITestSystem1 : ISystem {}
        public interface ITestSystem2 : ISystem {}
        public interface ITestSystem3 : ISystem {}
        public interface ITestSystem4 : ISystem {}

        public interface ITestInitializable1 : IInitializable { }

        // TODO: Better way to test these, maybe SystemContainer can use an
        // attribute provider that we can mock?
        [Dependencies(new Type[] { typeof(ITestSystem1) })]
        public class TestInitializableNeedingSystem1 : ITestInitializable1
        {
            public void Init(SystemContainer systems)
            {
            }
        }

        public class TestInitializable : ITestInitializable1
        {
            public void Init(SystemContainer systems)
            {
            }
        }



        [Test]
        public void TestCircularDependency()
        {
            ITestSystem1 system1 = Substitute.For<ITestSystem1>();
            ITestSystem2 system2 = Substitute.For<ITestSystem2>();

            system1.GetDependencies().Returns(new Type[]{ typeof(ITestSystem2) });
            system2.GetDependencies().Returns(new Type[]{ typeof(ITestSystem1) });
            
            var context = new SystemContext();
            context.Register<ITestSystem1>(() => system1);
            context.Register<ITestSystem2>(() => system2);
            var container = new SystemContainer();
            container.SetContext(context);
            Assert.IsTrue(container.Init().Status == ContainerInitStatus.CircularDependency);
        }

        [Test]
        public void TestTriangularDependency()
        {
            ITestSystem1 system1 = Substitute.For<ITestSystem1>();
            ITestSystem2 system2 = Substitute.For<ITestSystem2>();
            ITestSystem3 system3 = Substitute.For<ITestSystem3>();

            system1.GetDependencies().Returns(new Type[] { typeof(ITestSystem2) });
            system2.GetDependencies().Returns(new Type[] { typeof(ITestSystem3) });
            system3.GetDependencies().Returns(new Type[] { typeof(ITestSystem1) });

            var context = new SystemContext();
            context.Register<ITestSystem1>(() => system1);
            context.Register<ITestSystem2>(() => system2);
            context.Register<ITestSystem3>(() => system3);
            var container = new SystemContainer();
            container.SetContext(context);
            Assert.IsTrue(container.Init().Status == ContainerInitStatus.CircularDependency);
        }

        [Test]
        public void TestMissingDependency()
        {
            ITestSystem1 system1 = Substitute.For<ITestSystem1>();
            ITestSystem3 system3 = Substitute.For<ITestSystem3>();

            system1.GetDependencies();
            system3.GetDependencies().Returns(new Type[] { typeof(ITestSystem2) });

            var context = new SystemContext();
            context.Register<ITestSystem1>(() => system1);
            context.Register<ITestSystem3>(() => system3);
            var container = new SystemContainer();
            container.SetContext(context);
            Assert.IsTrue(container.Init().Status == ContainerInitStatus.MissingDependencies);
        }

        [Test]
        public void TestSystemsNotDuplicated()
        {
            ITestSystem1 system1 = Substitute.For<ITestSystem1>();
            ITestSystem2 system2 = Substitute.For<ITestSystem2>();
            ITestSystem3 system3 = Substitute.For<ITestSystem3>();
            ITestSystem4 system4 = Substitute.For<ITestSystem4>();

            system1.GetDependencies();
            system2.GetDependencies().Returns(new Type[]{ typeof(ITestSystem1) });
            system3.GetDependencies().Returns(new Type[]{ typeof(ITestSystem1), typeof(ITestSystem2) });
            system4.GetDependencies().Returns(new Type[]{ typeof(ITestSystem2), typeof(ITestSystem3) });

            var context = new SystemContext();
            context.Register<ITestSystem1>(() => system1);
            context.Register<ITestSystem2>(() => system2);
            context.Register<ITestSystem3>(() => system3);
            context.Register<ITestSystem4>(() => system4);
            var container = new SystemContainer();
            container.SetContext(context);
            Assert.IsTrue(container.Init().Status == ContainerInitStatus.Ok);

            system1.ReceivedWithAnyArgs(1).Init(null);
            system2.ReceivedWithAnyArgs(1).Init(null);
            system3.ReceivedWithAnyArgs(1).Init(null);
            system4.ReceivedWithAnyArgs(1).Init(null);
        }

        [Test]
        public void TestSystemsAndFactories()
        {
            ITestSystem1 system1 = Substitute.For<ITestSystem1>();
            ITestSystem3 system3 = Substitute.For<ITestSystem3>();

            system1.GetDependencies();
            system3.GetDependencies().Returns(new Type[]{ typeof(ITestInitializable1) });

            var context = new SystemContext();
            context.Register<ITestSystem1>(() => system1);
            context.RegisterFactory<ITestInitializable1, TestInitializableNeedingSystem1>();
            context.Register<ITestSystem3>(() => system3);
            var container = new SystemContainer();
            container.SetContext(context);
            Assert.IsTrue(container.Init().Status == ContainerInitStatus.Ok);
        }

        [Test]
        public void TestFactoryCreatsObject()
        {
            var context = new SystemContext();
            context.RegisterFactory<ITestInitializable1, TestInitializable>();
            var container = new SystemContainer();
            container.SetContext(context);
            Assert.IsTrue(container.Init().Status == ContainerInitStatus.Ok);
            Assert.IsNotNull(container.Get<ITestInitializable1>());
        }

    }
}