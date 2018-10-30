using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DOOFUS.Models.Persistence
{
    interface ISettingsRepository
    {
        Setting Add(Setting setting);
        void Delete(int id);
        Setting Get(int id);
        bool SaveOrUpdate(Setting setting);        
        bool Update(Setting setting);
        IEnumerable<Setting> GetUserSettings(int CustomerId, string UserName);
        Setting GetUserSetting(int customerid, string key, string username);
        IEnumerable<Setting> GetDeviceSettings(int CustomerId, int DeviceId);
        Setting GetDeviceSetting(int CustomerId, string key, int DeviceId);
        IEnumerable<Setting> GetCustomerSettings(int CustomerId);
        Setting GetCustomerSetting(int CustomerId, string key);
        Setting GetGlobalSetting(string key);
        Setting GetGlobalSetting(int id,string key);
        IEnumerable<Setting> GetGlobalSetting();
        IEnumerable<Setting> GetAll();
        
        
    }
}
