using DOOFUS.Models;
using System.Web.Mvc;

namespace DOOFUS.Controllers
{
    public class SettingsController : Controller
    {
        // GET: Settings
        public SettingsBase[] Get()
        {
            return new SettingsBase[]
            {
                new SettingsBase
                { 
                   
                }
            };
         }        
    }
}