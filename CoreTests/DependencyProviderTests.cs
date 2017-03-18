using NUnit.Framework;
using NSubstitute;
using System;
using SFuller.SharpGameLibs.Core.IOC;

namespace SFuller.SharpGameLibs.CoreTests
{
    [TestFixture]
    public class DependencyProviderTests
    {
        public interface ITestSystem1 { }
        public interface ITestSystem2 { }
        public interface ITestSystem3 { }
        public interface ITestSystem4 { }

        private DependencyProvider _provider;

        [SetUp]
        public void Setup()
        {
            _provider = new DependencyProvider();
        }


        [Dependencies(new Type[] {
            typeof(ITestSystem1)
        })]
        public class GenericConcrete<T>
        {
        }

        [Test]
        public void TestGenericDependencies()
        {
            Assert.AreEqual(new Type[] { typeof(ITestSystem1) }, _provider.Get(typeof(GenericConcrete<int>)));
        }

    }
}
