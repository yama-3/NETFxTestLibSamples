using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MvcApplication1.Controllers
{
    public class ProductController : Controller
    {
        private IProductRepository _repository;

        public ProductController(IProductRepository repository)
        {
            _repository = repository;
        }
        //
        // GET: /Product/

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Search(string searchTerm)
        {
            return View("SearchResults", _repository.FindProducts(searchTerm));
        }
    }

    public interface IProductRepository
    {
        ICollection<Product> FindProducts(string searchTerm);
    }

    public class ProductRepository : IProductRepository
    {
        public ICollection<Product> FindProducts(string searchTerm)
        {
            throw new NotImplementedException();
        }
    }

    public class Product
    {
        
    }
}
