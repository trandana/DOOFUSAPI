using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DOOFUS.Models.Persistence
{
    interface ISettingsRepository
    {
        Setting Get(int id);
        IEnumerable<Setting> GetAll();
        Setting Add(Setting setting);
        void Delete(int id);
        bool Update(Setting setting);
    }
}
