/*
 * The controller for the HOME page. Easy peasy.
 * Developed by Jason Scott and Tiaan Naude under TapX (pty) Ltd.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CellularAutomata.Controllers
{
    public class HomeController : Controller
    {
        //
        // GET: /Home/

        public ActionResult Index()
        {
            return View();
        }

    }
}
