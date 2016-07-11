using System;
using MvcApplication1.Controllers;
using MvcApplication1.Models;
using NUnit.Framework;
using Rhino.Mocks;

namespace RhinoMocksTests
{
    [TestFixture]
    public class RhinoMocksTest
    {
        [Test]
        public void When_user_forgot_password_should_save_user()
        {
            var stubUserRepository = MockRepository.GenerateStub<IUserRepository>();
            var stubbedSmsSender = MockRepository.GenerateStub<ISmsSender>();
            var theUser = new User { HashedPassword = "this is not hashed password" };
            stubUserRepository.Stub(_ => _.GetUserByName("ayende")).Return(theUser);

            var controllerUnderTest = new LoginController(stubUserRepository, stubbedSmsSender);
            controllerUnderTest.ForgotMyPassword("ayende");
            stubUserRepository.AssertWasCalled(_ => _.Save(theUser));
        }


        [Test]
        public void How_to_Assert_that_a_method_calls_an_expected_method_with_value()
        {
            var foo = new Foo { Name = "rhino-mocks" };
            var mockFooRepository = MockRepository.GenerateStub<IFooRepository>();
            var fooService = new FooService(mockFooRepository);

            fooService.LookUpFoo(foo);

            mockFooRepository.AssertWasCalled(x => x.GetFooByName(Arg<string>.Matches(y => y.Equals(foo.Name))));
        }

        [Test]
        public void How_to_Stub_out_your_own_value_of_a_ReadOnlyProperty()
        {
            var foo = MockRepository.GenerateStub<IFoo>();
            foo.Stub(x => x.ID).Return(123);
            var id = foo.ID;
            Assert.That(id, Is.EqualTo(123));
        }

        [Test]
        public void Enum_mask()
        {
            Hoge h = Hoge.Hoge1 | Hoge.Hoge2;
            Assert.AreEqual((int)h, 3);

        }
    }

    enum Hoge
    {
        Hoge1 = 1,
        Hoge2 = 2,
        Hoge4 = 4,
        Hoge8 = 8
    }
}
