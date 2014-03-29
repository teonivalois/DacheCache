using DacheCache.Sample.Web.Models;
using DacheCache.Sample.Web.Models.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace DacheCache.Sample.Web.Controllers {
    public class HomeController : Controller {
        public ActionResult Index() {
            using (AppDbContext ctx = new AppDbContext()) {
                List<Product> products = ctx.Products.ToList();
                foreach (var p in products) {
                    Category category = p.Category;
                }
            }
            return View();
        }

        public ActionResult About() {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact() {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}