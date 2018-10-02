using DOOFUS.Models;
using System.Web.Mvc;

namespace DOOFUS.Controllers
{
    public class SettingsController : Controller
    {
        // GET: Settings
        public Setting[] Get()
        {
            return new Setting[]
            {
                new Setting
                {
                    Id = 0,
                    Key = "test",
                    CustomerId = 0,
                    UserName = "Test",
                    LastModifiedBy = "JC",
                    LastModifiedById = 0
                }
            };
         }        
    }
}