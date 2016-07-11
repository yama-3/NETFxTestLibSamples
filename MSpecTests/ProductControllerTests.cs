using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using Machine.Specifications;
using MvcApplication1.Controllers;
using NSubstitute;

namespace MSpecTests
{
    [Subject("Product Search")]
    public class when_product_search_page_requested
    {
        private static ProductController _controller;
        private static ActionResult _result;
        private static IProductRepository _repository;

        private Establish context = () =>
            {
                _repository = Substitute.For<IProductRepository>();
                _controller = new ProductController(_repository);
            };

        private Because of = () => { _result = _controller.Search(string.Empty); };

        It should_return_product_search_page = () => { _result.is_a_view_and().ViewName.Equals("SearchResults"); };
    }

    [Subject("Product Search")]
    public class when_asked_for_products_matching_search_term : concern_for_product_controller
    {
        private static ActionResult _result;

        private Because of = () => { _result = _controller.Search("test"); };

        private It should_retrieve_a_list_of_products_with_titles_containing_the_search_term
            = () => _productRepository.Received(1).FindProducts("test");


        private static ICollection<Product> _products;

        private Establish context = () =>
            {
                _products = new List<Product>();
                _productRepository.FindProducts("test").Returns(_products);
            };

        private It should_return_the_list_of_products_to_the_user
            = () => _result.is_a_view_and().ViewData.Model.Equals(_products);

        private It should_return_the_search_results_page_to_the_user
            = () => _result.is_a_view_and().ViewName.Equals("SearchResults");
    }
        
    [Subject("Product Search")]
    public class when_empty_search_term_entered : concern_for_product_controller
    {
        private It should_return_an_error_message;
    }

    public class concern_for_product_controller
    {
        protected static ProductController _controller;
        protected static IProductRepository _productRepository;

        private Establish context = () =>
            {
                _productRepository = Substitute.For<IProductRepository>();
                _controller = new ProductController(_productRepository);
            };
    }
}


public static class TestExtensions
{
    public static ViewResult is_a_view_and(this ActionResult result)
    {
        return result as ViewResult;
    }
}