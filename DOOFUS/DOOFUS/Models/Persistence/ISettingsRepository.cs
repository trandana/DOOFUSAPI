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
        Setting GetUserSetting(int CustomerId, string Username);
        Setting GetUserSetting(int CustomerId, int DeviceId, int EntityId);
        Setting GetDeviceSetting(int CustomerId, int DeviceId, int EntityId);
        Setting GetDeviceSetting(int CustomerId, string key);
        Setting GetCustomerSetting(string key);
        Setting GetCustomerSetting(int EntityId, string key);
        Setting GetGlobalSetting(String key);
        IEnumerable<Setting> GetAll();
        Setting Add(Setting setting);
        void Delete(int id);
        bool Update(Setting setting);
    }
}
