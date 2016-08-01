using System;
using System.Linq;
using FakeItEasy;
using Xunit;

namespace FakeItEasySamples
{
    public class UnitTest1
    {
        [Fact]
        public void TestMethod1()
        {
            var foo = A.Fake<IFoo>();
            Assert.Equal(default(int), foo.MethodA(1));
            A.CallTo(() => foo.MethodA(1)).Returns(10);
            Assert.Equal(10, foo.MethodA(1));

            var foos = A.CollectionOfFake<IFoo>(10);
            Assert.Equal(10, foos.Count);
        }
    }

    public interface IFoo
    {
        int MethodA(int val1);
    }
}
