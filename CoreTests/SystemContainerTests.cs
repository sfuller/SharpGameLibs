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
            Assert.IsFalse(container.Init());
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
            Assert.IsFalse(container.Init());
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
            Assert.IsTrue(container.Init());

            system1.ReceivedWithAnyArgs(1).Init(null);
            system2.ReceivedWithAnyArgs(1).Init(null);
            system3.ReceivedWithAnyArgs(1).Init(null);
            system4.ReceivedWithAnyArgs(1).Init(null);
        }

    }
}