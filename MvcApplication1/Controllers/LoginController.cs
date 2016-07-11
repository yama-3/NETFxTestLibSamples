using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MvcApplication1.Models;

namespace MvcApplication1.Controllers
{
    public class LoginController : Controller
    {
        private IUserRepository _repository;
        private ISmsSender _sender;

        private LoginController()
        {
        }

        public LoginController(IUserRepository repository, ISmsSender sender)
        {
            _repository = repository;
            _sender = sender;
        }

        //
        // GET: /Login/

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult ForgotMyPassword(string name)
        {
            var user = _repository.GetUserByName(name);
            user.HashedPassword = "new password";
            _repository.Save(user);
            return View();
        }
    }
}
