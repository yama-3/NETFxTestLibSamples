using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Contexts;
using System.Security.AccessControl;
using System.Security.Cryptography.X509Certificates;
using System.Security.Principal;
using System.Text;
using System.Threading;
using Microsoft.SqlServer.Server;
using NSubstitute;
using Xunit;

namespace xUnitSamples
{
    public class XUnitSample
    {
        [Fact]
        public void PartialSubsAndTestSpies()
        {
            var reader = Substitute.ForPartsOf<SummingReader>();
            reader.ReadFile(Arg.Is("foo.txt")).Returns("1,2,3,4,5");
            var result1 = reader.Read("foo.txt");
            Assert.Equal(15, result1);

            reader.When(x => x.ReadFile("foo.txt")).DoNotCallBase();
            reader.ReadFile("foo.txt").Returns("1,2,3,4,5");
            var result2 = reader.Read("foo.txt");
            Assert.Equal(15, result2);
        }

        public class SummingReader
        {
            public virtual int Read(string path)
            {
                var s = ReadFile(path);
                return s.Split(',').Select(int.Parse).Sum();
            }
            public virtual string ReadFile(string path)
            {
                return "the result of reading the file here";
            }
        }

        [Fact]
        public void CheckingCallOrder()
        {
            var connection = Substitute.For<IConnection>();
            var command = Substitute.For<ICommand>();
            var subject = new Controller(connection, command);
            subject.DoStuff();
            Received.InOrder(() =>
            {
                connection.Open();
                command.Run(connection);
                connection.Close();
            });

            var connection2 = Substitute.For<IConnection>();
            connection2.SomethingHappend += () => { };
            connection2.Open();
            Received.InOrder(() =>
            {
                connection2.SomethingHappend += Arg.Any<Action>();
                connection2.Open();
            });
        }
        public interface IConnection
        {
            void Open();
            void Close();
            event Action SomethingHappend;
        }
        public class Controller
        {
            private IConnection connection;
            private ICommand command;
            public Controller(IConnection connection, ICommand command)
            {
                this.connection = connection;
                this.command = command;
            }

            public void DoStuff()
            {
                if (connection != null && command != null)
                {
                    connection.Open();
                    command.Run(connection);
                    connection.Close();
                }
            }
        }

        [Fact]
        public void ActionsWithArgumentMatchers()
        {
            // arrange
            var cart = Substitute.For<ICart>();
            var events = Substitute.For<IEvents>();
            var processor = Substitute.For<IOrderProcessor>();
            cart.OrderId = 3;
            processor.Processorder(3, Arg.Invoke(true));
            // act
            var command = new OrderPlacedCommand(processor, events);
            command.Execute(cart);
            // assert
            events.Received().RaiseOrderProcessed(3);

            var calcurator = Substitute.For<ICalculator>();
            var arguemtnUsed = 0;
            calcurator.Multiply(Arg.Any<int>(), Arg.Do<int>(x => arguemtnUsed = x));
            calcurator.Multiply(123, 42);
            Assert.Equal(42, arguemtnUsed);

            var firstArgsBeingMultiplied = new List<int>();
            calcurator.Multiply(Arg.Do<int>(x => firstArgsBeingMultiplied.Add(x)), 10);
            calcurator.Multiply(2, 10);
            calcurator.Multiply(5, 10);
            calcurator.Multiply(7, 4567);
            Assert.Equal(new[] { 2, 5 }, firstArgsBeingMultiplied);

            var numberOfCallsWhereFirstArgIsLessThan0 = 0;
            calcurator.Multiply(Arg.Is<int>(x => x < 0), Arg.Do<int>(x => numberOfCallsWhereFirstArgIsLessThan0++)).Returns(123);
            var results = new[]
            {
                calcurator.Multiply(-4, 3),
                calcurator.Multiply(-27, 88),
                calcurator.Multiply(-7, 8),
                calcurator.Multiply(123, 2)
            };
            Assert.Equal(3, numberOfCallsWhereFirstArgIsLessThan0);
            Assert.Equal(new[] { 123, 123, 123, 0 }, results);
        }

        public interface IOrderProcessor
        {
            void Processorder(int orderId, Action<bool> orderProcessed);
        }
        public interface IEvents
        {
            void RaiseOrderProcessed(int orderId);
        }
        public interface ICart
        {
            int OrderId { get; set; }
        }
        public class OrderPlacedCommand
        {
            private IOrderProcessor orderProcessor;
            private IEvents events;

            public OrderPlacedCommand(IOrderProcessor orderProcessor, IEvents events)
            {
                this.orderProcessor = orderProcessor;
                this.events = events;
            }

            public void Execute(ICart cart)
            {
                orderProcessor.Processorder(
                    cart.OrderId,
                    wasOk =>
                    {
                        if (wasOk)
                        {
                            events.RaiseOrderProcessed(cart.OrderId);
                        }
                    });
            }
        }

        [Fact]
        public void SettingOutAndRefArgs()
        {
            var value = "";
            var lookup = Substitute.For<ILookup>();
            lookup.TryLookup("hello", out value).Returns(x =>
            {
                x[1] = "world!";
                return true;
            });
            var result = lookup.TryLookup("hello", out value);
            Assert.True(result);
            Assert.Equal(value, "world!");
        }
        public interface ILookup
        {
            bool TryLookup(string key, out string value);
        }
        [Fact]
        public void AutoAndRecursiveMocsk()
        {
            var factory = Substitute.For<INumberParserFactory>();
            //var parser = Substitute.For<INumberParser>();
            //factory.Create(',').Returns(parser);
            //parser.Parse("an expression").Returns(new[] { 1, 2, 3 });
            factory.Create(',').Parse("an expression").Returns(new[] { 1, 2, 3 });
            Assert.Equal(new[] { 1, 2, 3 }, factory.Create(',').Parse("an expression"));

            var firstCall = factory.Create(',');
            var secondCall = factory.Create(',');
            var thirdCallWithDiffArg = factory.Create('x');
            Assert.Same(firstCall, secondCall);
            Assert.NotSame(firstCall, thirdCallWithDiffArg);

            var context = Substitute.For<IContext>();
            context.CurrentRequest.Identity.Name.Returns("My pet fish Eric");
            Assert.Equal("My pet fish Eric", context.CurrentRequest.Identity.Name);

            var identity = Substitute.For<IIdentity>();
            Assert.Equal(string.Empty, identity.Name);
            Assert.Equal(0, identity.Roles().Length);
        }

        public interface IContext
        {
            IRequest CurrentRequest { get; }
        }
        public interface IRequest
        {
            IIdentity Identity { get; }
            IIdentity NewIdentity(string name);
        }
        public interface IIdentity
        {
            string Name { get; }
            string[] Roles();
        }
        public interface INumberParser
        {
            IEnumerable<int> Parse(string expression);
        }
        public interface INumberParserFactory
        {
            INumberParser Create(char delimiter);
        }

        [Fact]
        public void RaisingEvents()
        {
            var wasCalled = false;
            var engine = Substitute.For<IEngine>();
            engine.Idling += (sender, args) => wasCalled = true;
            engine.Idling += Raise.Event();
            Assert.True(wasCalled);

            var numberOfEvents = 0;
            engine.LowFuelWarning += (sender, args) => numberOfEvents++;
            engine.LowFuelWarning += Raise.EventWith(new LowFuelWarningEventArgs(10));
            engine.LowFuelWarning += Raise.EventWith(new object(), new LowFuelWarningEventArgs(10));

            var sub = Substitute.For<INotifyPropertyChanged>();
            bool wasCalled2 = false;
            sub.PropertyChanged += (sender, args) => wasCalled2 = true;
            sub.PropertyChanged += Raise.Event<PropertyChangedEventHandler>(this, new PropertyChangedEventArgs("test"));
            Assert.True(wasCalled2);

            int revvedAt = 0;
            engine.RevvedAt += rpm => revvedAt = rpm;
            engine.RevvedAt += Raise.Event<Action<int>>(123);
            Assert.Equal(123, revvedAt);
        }

        public interface IEngine
        {
            event EventHandler Idling;
            event EventHandler<LowFuelWarningEventArgs> LowFuelWarning;
            event Action<int> RevvedAt;
        }

        public class LowFuelWarningEventArgs : EventArgs
        {
            public int PercentLeft { get; private set; }

            public LowFuelWarningEventArgs(int percentLeft)
            {
                PercentLeft = percentLeft;
            }
        }

        [Fact]
        public void ThrowingExceptions()
        {
            var calculator = Substitute.For<ICalculator>();
            calculator.Add(-1, -1).Returns(x => { throw new Exception(); });
            calculator.When(x => x.Add(-2, -2)).Do(x => { throw new Exception(); });
            Assert.Throws<Exception>(() => calculator.Add(-1, -1));
            Assert.Throws<Exception>(() => calculator.Add(-2, -2));
        }

        [Fact]
        public void CallbacksVoidCallsAndWhenDo()
        {
            var calculator = Substitute.For<ICalculator>();
            var counter = 0;
            calculator.Add(0, 0).ReturnsForAnyArgs(x => 0).AndDoes(x => counter++);
            calculator.Add(7, 3);
            calculator.Add(2, 2);
            calculator.Add(11, -3);
            Assert.Equal(3, counter);

            var counter2 = 0;
            var foo = Substitute.For<IFoo>();
            foo.When(x => x.SayHello("World")).Do(x => counter2++);
            foo.SayHello("World");
            foo.SayHello("World");
            Assert.Equal(2, counter2);

            var counter3 = 0;
            calculator.Add(1, 2).Returns(3);
            calculator.When(x => x.Add(Arg.Any<int>(), Arg.Any<int>())).Do(x => counter3++);
            var result = calculator.Add(1, 2);
            Assert.Equal(3, result);
            Assert.Equal(1, counter3);
        }

        [Fact]
        public void ArgumentMatches()
        {
        }

        [Fact]
        public void ClearingReceivedCalls()
        {
            var command = Substitute.For<ICommand>();
            var runner = new OnceOffCommandRunner(command);

            runner.Run();
            command.Received().Execute();

            command.ClearReceivedCalls();

            runner.Run();
            command.DidNotReceive().Execute();
        }

        public class OnceOffCommandRunner
        {
            private ICommand command;

            public OnceOffCommandRunner(ICommand command)
            {
                this.command = command;
            }

            public void Run()
            {
                if (command == null)
                {
                    return;
                }
                command.Execute();
                command = null;
            }
        }

        [Fact]
        public void CheckingReceivedCalls()
        {
            var command1 = Substitute.For<ICommand>();
            var something1 = new SomethingThatNeedsACommand(command1);
            something1.DoSomething();
            command1.Received().Execute();

            var command2 = Substitute.For<ICommand>();
            var something2 = new SomethingThatNeedsACommand(command2);
            something2.DontDoAnything();
            command2.DidNotReceive().Execute();

            var command3 = Substitute.For<ICommand>();
            var repeater = new CommandRepeater(command3, 3);
            repeater.Execute();
            command3.Received(3).Execute();

            var calculator = Substitute.For<ICalculator>();
            calculator.Add(1, 2);
            calculator.Add(-100, 100);
            calculator.Received().Add(Arg.Any<int>(), 2);
            calculator.Received().Add(Arg.Is<int>(x => x < 0), 100);
            calculator.DidNotReceive().Add(Arg.Any<int>(), Arg.Is<int>(x => x >= 500));

            calculator.Add(1, 3);
            calculator.ReceivedWithAnyArgs().Add(1, 1);
            calculator.DidNotReceiveWithAnyArgs().Subtract(0, 0);

            var mode = calculator.Mode;
            calculator.Mode = "TEST";
            var temp = calculator.Received().Mode;
            calculator.Received().Mode = "TEST";

            var dictionary = Substitute.For<IDictionary<string, int>>();
            dictionary["test"] = 1;
            dictionary.Received()["test"] = 1;
            dictionary.Received()["test"] = Arg.Is<int>(x => x < 5);

            var command4 = Substitute.For<ICommand>();
            var watcher4 = new CommandWatcher(command4);
            command4.Executed += Raise.Event();
            Assert.True(watcher4.DidStuff);

            var command5 = Substitute.For<ICommand>();
            var watcher5 = new CommandWatcher(command5);
            command5.Received().Executed += watcher5.OnExecuted;
            command5.Received().Executed += Arg.Any<EventHandler>();
        }

        public class CommandWatcher
        {
            private ICommand command;

            public CommandWatcher(ICommand command)
            {
                command.Executed += OnExecuted;
            }
            public bool DidStuff { get; set; }

            public void OnExecuted(object o, EventArgs e)
            {
                DidStuff = true;
            }
        }

        public class CommandRepeater
        {
            private ICommand command;
            private int numberOfTimesToCall;

            public CommandRepeater(ICommand command, int numberOfTimesToCall)
            {
                this.command = command;
                this.numberOfTimesToCall = numberOfTimesToCall;
            }

            public void Execute()
            {
                for (var i = 0; i < numberOfTimesToCall; i++) { command.Execute(); }
            }
        }

        public interface ICommand
        {
            void Execute();
            event EventHandler Executed;
            void Run(IConnection con);
        }

        public class SomethingThatNeedsACommand
        {
            private ICommand command;

            public SomethingThatNeedsACommand(ICommand command)
            {
                this.command = command;
            }

            public void DoSomething()
            {
                command.Execute();
            }

            public void DontDoAnything() { }
        }

        [Fact]
        public void ReplacingReturnValues()
        {
            var calculator = Substitute.For<ICalculator>();
            calculator.Mode.Returns("DEC,HEX,OCT");
            calculator.Mode.Returns(x => "???");
            calculator.Mode.Returns("HEX");
            calculator.Mode.Returns("BIN");
            Assert.Equal("BIN", calculator.Mode);
        }

        [Fact]
        public void MultipleReturnValues()
        {
            var calculator = Substitute.For<ICalculator>();
            calculator.Mode.Returns("DEC", "HEX", "BIN");
            Assert.Equal("DEC", calculator.Mode);
            Assert.Equal("HEX", calculator.Mode);
            Assert.Equal("BIN", calculator.Mode);

            calculator.Mode.Returns(x => "DEC", x => "HEX", x => { throw new Exception(); });
            Assert.Equal("DEC", calculator.Mode);
            Assert.Equal("HEX", calculator.Mode);
            Assert.Throws<Exception>(() => { var result = calculator.Mode; });
        }

        [Fact]
        public void ReturnFromAFunction()
        {
            var calculator = Substitute.For<ICalculator>();
            calculator.Add(Arg.Any<int>(), Arg.Any<int>()).Returns(_ => (int)_[0] + (int)_[1]);
            Assert.Equal(2, calculator.Add(1, 1));
            Assert.Equal(50, calculator.Add(20, 30));
            Assert.Equal(9275, calculator.Add(-73, 9348));

            var foo = Substitute.For<IFoo>();
            foo.Bar(0, "").ReturnsForAnyArgs(x => "Hello " + x.Arg<string>());
            Assert.Equal("Hello World", foo.Bar(1, "World"));

            var counter1 = 0;
            calculator.Add(0, 0).ReturnsForAnyArgs(x =>
            {
                counter1++;
                return 0;
            });
            calculator.Add(7, 3);
            calculator.Add(2, 2);
            calculator.Add(11, -3);
            Assert.Equal(3, counter1);

            var counter2 = 0;
            calculator.Add(0, 0).ReturnsForAnyArgs(x => 0).AndDoes(x => counter2++);
            calculator.Add(7, 3);
            calculator.Add(2, 2);
            Assert.Equal(2, counter2);
        }

        public interface IFoo
        {
            string Bar(int a, string b);
            string SayHello(string to);
        }

        [Fact]
        public void ReturnForAnyArgs()
        {
            var calculator = Substitute.For<ICalculator>();
            calculator.Add(1, 2).ReturnsForAnyArgs(100);
            Assert.Equal(100, calculator.Add(1, 2));
            Assert.Equal(100, calculator.Add(-7, 15));
        }

        [Fact]
        public void ReturnForSpecificArgs()
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
        public void SettingAReturnValue()
        {
            var calculator = Substitute.For<ICalculator>();
            calculator.Add(1, 2).Returns(3);
            Assert.Equal(calculator.Add(1, 2), 3);
            Assert.Equal(calculator.Add(1, 2), 3);
            Assert.NotEqual(calculator.Add(3, 6), 3);

            calculator.Mode.Returns("DEC");
            Assert.Equal(calculator.Mode, "DEC");
            calculator.Mode = "HEX";
            Assert.Equal(calculator.Mode, "HEX");
        }

        [Fact]
        public void CreatingSubsitute()
        {
            var command = Substitute.For<ICommand, IDisposable>();
            var runner = new CommandRunner(command);
            runner.RunCommand();
            command.Received().Execute();
            ((IDisposable)command).Received().Dispose();

            var substitute = Substitute.For(
                new[] { typeof(ICommand), typeof(ISomeInterface), typeof(SomeClassWithCtorArgs) },
                new object[] { 5, "hello world" }
            );
            Assert.IsAssignableFrom<ICommand>(substitute);
            Assert.IsAssignableFrom<ISomeInterface>(substitute);
            Assert.IsAssignableFrom<SomeClassWithCtorArgs>(substitute);

            var func = Substitute.For<Func<string>>();
            func().Returns("hello");
            Assert.Equal("hello", func());
        }
        public class SomeClassWithCtorArgs
        {
            public SomeClassWithCtorArgs(int number, string message) { }
        }
        public interface ISomeInterface { }

        public class CommandRunner
        {
            private object _command;

            public CommandRunner(object command)
            {
                _command = command;
            }

            public void RunCommand()
            {
                (_command as ICommand).Execute();
                (_command as IDisposable).Dispose();
            }
        }

        [Fact]
        public void Test()
        {
            var calculator = Substitute.For<ICalculator>();
            calculator.Add(1, 2).Returns(3);
            Assert.Equal(3, calculator.Add(1, 2));

            calculator.Received().Add(1, Arg.Any<int>());
            calculator.DidNotReceive().Add(2, 2);
            calculator.PoweringUp += Raise.Event();
        }
    }

    public interface ICalculator
    {
        int Add(int left, int right);
        event EventHandler PoweringUp;
        string Mode { get; set; }
        int Subtract(int left, int right);
        int Multiply(int left, int right);
    }
}
