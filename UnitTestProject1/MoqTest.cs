using System;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using MvcApplication1.Controllers;

namespace UnitTestProject1
{
    [TestClass]
    public class MoqTest
    {
        [TestMethod]
        public void TestMethod1()
        {
            var controller = new HomeController();

            var server = new Mock<HttpServerUtilityBase>(MockBehavior.Loose);
            var response = new Mock<HttpResponseBase>(MockBehavior.Strict);

            var request = new Mock<HttpRequestBase>(MockBehavior.Strict);
            request.Setup(_ => _.UserHostAddress).Returns("127.0.0.1");

            var session = new Mock<HttpSessionStateBase>();
            session.Setup(_ => _.SessionID).Returns(Guid.NewGuid().ToString());

            var context = new Mock<HttpContextBase>();
            context.SetupGet(_ => _.Request).Returns(request.Object);
            context.SetupGet(_ => _.Response).Returns(response.Object);
            context.SetupGet(_ => _.Server).Returns(server.Object);
            context.SetupGet(_ => _.Session).Returns(session.Object);

            controller.ControllerContext = new ControllerContext(context.Object, new RouteData(), controller);
        }

        [TestMethod]
        public void IFoo_DoSomething()
        {
            var mock = new Mock<IFoo>();
            mock.Setup(foo => foo.DoSomething("ping")).Returns(true);
            
            var outString = "ack";
            mock.Setup(foo => foo.TryParse("ping", out outString)).Returns(true);

            var instance = new Bar();
            mock.Setup(foo => foo.Submit(ref instance)).Returns(true);

            mock.Setup(x => x.Method1(It.IsAny<string>()))
                .Returns((string s) => s.ToLower());
            mock.Setup(foo => foo.Method1("reset")).Throws<InvalidOperationException>();
            mock.Setup(foo => foo.Method1("")).Throws(new ArgumentException("command"));

            var count = 10;
            mock.Setup(foo => foo.GetCount()).Returns(() => count);

            var calls = 0;
            //mock.Setup(foo => foo.GetCountThing())
            //    .Returns(() => calls)
            //    .Callback(() => calls++);
            mock.Setup(foo => foo.GetCountThing())
                .Returns(() => calls);
            Debug.WriteLine(mock.Object.GetCountThing());
        }

        [TestMethod]
        public void mock_matching_arguments()
        {
            var mock = new Mock<IFoo>();
            mock.Setup(foo => foo.DoSomething(It.IsAny<string>())).Returns(true);
            mock.Setup(foo => foo.Add(It.Is<int>(i => i%2 == 0))).Returns(true);
            mock.Setup(foo => foo.Add(It.IsInRange<int>(0, 10, Range.Inclusive))).Returns(true);
            mock.Setup(foo => foo.DoSomething(It.IsRegex("[a-d]+", RegexOptions.IgnoreCase))).Returns(false);
        }

        [TestMethod]
        public void Test1()
        {
            var mock = new Mock<Bar>();
            mock.Setup(_ => _.Method1()).Returns("b");
            Assert.AreEqual("b", mock.Object.Method1());

            var obj = new Bar();
            Assert.AreEqual("a", obj.Method1());
        }
    }

    internal interface IFoo
    {
        bool DoSomething(string command);
        string Method1(string str);
        bool TryParse(string command, out string message);
        bool Submit(ref Bar bar);
        int GetCount();
        int GetCountThing();
        bool Add(int value);
    }

    class Bar
    {
        public virtual string Method1()
        {
            return "a";
        }
    }
}
