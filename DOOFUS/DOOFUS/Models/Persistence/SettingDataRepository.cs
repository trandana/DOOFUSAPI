using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DOOFUS.Models.Persistence
{
    public class SettingDataRepository : ISettingsRepository
    {
        //Query Goes here
        public Setting Add(Setting setting)
        {
            throw new NotImplementedException();
        }
        
        public void Delete(int id)
        {
            throw new NotImplementedException();
        }        
        
        public Setting Get(int id)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<Setting> GetAll()
        {
            throw new NotImplementedException();
        }

        public bool Update(Setting setting)
        {
            throw new NotImplementedException();
        }
    }
}