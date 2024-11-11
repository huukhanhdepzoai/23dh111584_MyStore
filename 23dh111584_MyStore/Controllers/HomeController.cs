using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using _23dh111584_MyStore.Models.ViewModels;
using _23dh111584_MyStore.Models;

namespace _23dh111584_MyStore.Controllers
{
    public class HomeController : Controller
    {
        // GET: Home
        public ActionResult Index()
        {
            return View();
        }
    }
}