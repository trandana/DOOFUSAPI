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
        IEnumerable<Setting> GetDeviceSettings(int CustomerId, int DeviceId);
        Setting GetDeviceSetting(int CustomerId, int DeviceId, string key);
        IEnumerable<Setting> GetCustomerSettings(string key);
        Setting GetCustomerSetting(int CustomerId, string key);
        Setting GetGlobalSetting(string key);
        IEnumerable<Setting> GetAll();
        Setting Get();
        
    }
}
