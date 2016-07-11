using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RhinoMocksTests
{
    class FooService
    {
        private IFooRepository _repository;

        public FooService(IFooRepository repository)
        {
            _repository = repository;
        }

        public void LookUpFoo(Foo foo)
        {
            _repository.GetFooByName(foo.Name);
        }
    }

    public interface IFooRepository
    {
        Foo GetFooByName(string name);
    }

    public class Foo
    {
        public string Name { get; set; }
    }

    public interface IFoo
    {
        int ID { get; }
    }
}
