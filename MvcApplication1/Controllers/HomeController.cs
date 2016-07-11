using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MvcApplication1.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            ViewBag.Message = "ASP.NET MVC アプリケーションを簡単に始めるには、このテンプレートを変更してください。";

            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "アプリケーション説明ページ。";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "連絡先ページ。";

            return View();
        }
    }
}
