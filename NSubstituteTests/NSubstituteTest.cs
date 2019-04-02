using System;
using System.Collections;
using System.Collections.Generic;
using NSubstitute;
using Xunit;
using Xunit.Extensions;

namespace NSubstituteTests
{
    public class NSubstituteTest
    {
        [Fact]
        public void TestMethod1()
        {
            var calculator = Substitute.For<ICalculator>();

            calculator.Add(1, 2).Returns(3);
            Assert.Equal(3, calculator.Add(1, 2));

            calculator.Received().Add(1, Arg.Any<int>());
            calculator.Received().Add(Arg.Any<int>(), 2);
            calculator.DidNotReceive().Add(2, 2);

            calculator.Mode.Returns("DEC");
            Assert.Equal("DEC", calculator.Mode);

            calculator.Mode = "HEX";
            Assert.Equal("HEX", calculator.Mode);

            calculator.Add(10, -5);
            calculator.Received().Add(10, Arg.Any<int>());
            calculator.Received().Add(10, Arg.Is<int>(_ => _ < 0));

            calculator.Add(Arg.Any<int>(), Arg.Any<int>()).Returns(x => (int)x[0] + (int)x[1]);
            Assert.Equal(15, calculator.Add(5, 10));

            calculator.Mode.Returns("HEX", "DEC", "BIN");
            Assert.Equal("HEX", calculator.Mode);
            Assert.Equal("DEC", calculator.Mode);
            Assert.Equal("BIN", calculator.Mode);

            bool eventWasRaised = false;
            calculator.PoweringUp += (sender, args) => eventWasRaised = true;
            calculator.PoweringUp += Raise.Event();
            Assert.True(eventWasRaised);
        }

        [Fact]
        public void create_substitute()
        {
            var substituteInterface = Substitute.For<ISomeInterface>();
            var substituteClass = Substitute.For<SomeClassWithCtorArgs>(5, "hello, world");
        }

        [Fact]
        public void substituting_for_multiple_interface()
        {
            var command = Substitute.For<CommandRunner.ICommand, IDisposable>();
            var runner = new CommandRunner(command);
            runner.RunCommand();
            command.Received().Execute();
            ((IDisposable)command).Received().Dispose();

            //var substitute = Substitute.For(
            //    new[] { typeof(CommandRunner.ICommand), typeof(ISomeInterface), typeof(SomeClassWithCtorArgs) },
            //    new object[] { 5, "hello, world" });
            //var substitute = Substitute.For(new[] { typeof(ISomeInterface) }, new object[] { });
            var substitute = new SomeClass();
            Assert.IsType<SomeClass>(substitute);
            Assert.IsAssignableFrom<ISomeInterface>(substitute);

            //Assert.IsType<CommandRunner.ICommand>(substitute);
            //Assert.IsType<ISomeInterface>(substitute);
            //Assert.IsType<SomeClassWithCtorArgs>(substitute);
        }

        [Fact]
        public void substituting_for_delegates()
        {
            var func = Substitute.For<Func<string>>();
            func().Returns("hello");
            Assert.Equal("hello", func());
        }

        [Fact]
        public void return_for_specific_args()
        {
            var calculator = Substitute.For<ICalculator>();
            calculator.Add(Arg.Any<int>(), 5).Returns(10);
            Assert.Equal(10, calculator.Add(123, 5));
            Assert.Equal(10, calculator.Add(-9, 5));
            Assert.NotEqual(10, calculator.Add(-9, -9));

            calculator.Add(1, Arg.Is<int>(_ => _ < 0)).Returns(345);
            Assert.Equal(345, calculator.Add(1, -2));
            Assert.NotEqual(345, calculator.Add(1, 2));

            calculator.Add(Arg.Is(0), Arg.Is(0)).Returns(99);
            Assert.Equal(99, calculator.Add(0, 0));
        }

        [Fact]
        public void return_for_any_args()
        {
            var calculator = Substitute.For<ICalculator>();
            calculator.Add(1, 2).ReturnsForAnyArgs(100);
            calculator.Add(1, 3).Returns(50);
            Assert.Equal(calculator.Add(1, 2), 100);
            Assert.Equal(calculator.Add(1, 3), 50);
            Assert.Equal(calculator.Add(-7, 15), 100);
        }

        [Fact]
        public void class1_method1_positive()
        {
            var if1 = Substitute.For<Interface1>();
            var if2 = Substitute.For<Interface2>();

            var target = new Class1(if1, if2);
            target.Method1(1);

            if1.Received().Method1();
            if2.DidNotReceive().Method1();
        }

        [Fact]
        public void class_method2_exception()
        {
            var if1 = Substitute.For<Interface1>();
            var if2 = Substitute.For<Interface2>();

            var target = new Class1(if1, if2);

            var e = Record.Exception(() => target.Method2());

            Assert.IsType<ArgumentException>(e);
        }

        [Theory]
        [InlineData("あいうえお", 10, false)]
        [InlineData("かきくけこ", 11, true)]
        [InlineData("さしすせそ", 12, true)]
        public void class1_method1_negative(string val1, int val2, bool val3)
        {
            Assert.Equal("あいうえお", val1);
            Assert.Equal(10, val2);
            Assert.False(val3);
        }

        [Theory]
        [MemberData("Prop1")]
        public void class1_method1_a(string val1, int val2, bool val3)
        {
            Assert.Equal("あいうえお", val1);
            Assert.Equal(10, val2);
            Assert.False(val3);
        }

        public static IEnumerable<object[]> Prop1
        {
            get
            {
                yield return new object[] { "a", 1, true };
                yield return new object[] { "b", 2, false };
                yield return new object[] { "c", 3, true };
            }
        }

        [Fact]
        public void Test1()
        {
            var mock = Substitute.For<Hoge>();
            mock.M1().Returns("b");
            Assert.Equal("b", mock.M1());

            var obj = new Hoge();
            Assert.Equal("a", obj.M1());
        }
    }

    public class Hoge
    {
        public virtual string M1()
        {
            return "a";
        }
    }

    public class CommandRunner
    {
        private readonly ICommand _command;
        public CommandRunner(ICommand command)
        {
            _command = command;
        }
        public void RunCommand()
        {
            using (_command)
            {
                _command.Execute();
            }
        }
        public interface ICommand : IDisposable
        {
            void Execute();
        }
    }

    public class SomeClassWithCtorArgs
    {
        public SomeClassWithCtorArgs(int id, string message)
        {
        }
    }

    public interface ISomeInterface
    {
    }

    public class SomeClass : ISomeInterface
    {
    }

    public interface ICalculator
    {
        int Add(int val1, int val2);
        string Mode { get; set; }
        event EventHandler PoweringUp;
    }

    public class Class1
    {
        public Interface1 _if1;
        public Interface2 _if2;

        private Class1()
        {
        }


        public Class1(Interface1 if1, Interface2 if2)
        {
            _if1 = if1;
            _if2 = if2;
        }

        public void Method1(int arg1)
        {
            if (arg1 > 0)
            {
                _if1.Method1();
            }
            else
            {
                _if2.Method1();
            }
        }

        public void Method2()
        {
            throw new ArgumentException();
        }
    }

    public interface Interface1
    {
        int Method1();
    }

    public interface Interface2
    {
        int Method1();
    }
}
