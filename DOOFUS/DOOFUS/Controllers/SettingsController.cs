using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace DOOFUS.Controllers
{
    public class SettingsController : Controller
    {
        // GET: Settings
        public ActionResult Index()
        {
            return View();
        }

        // GET api/values
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET api/values/5
        public string Get(int id)
        {
            return "value";
        }

        
    }
}