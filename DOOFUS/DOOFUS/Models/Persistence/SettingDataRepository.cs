using DOOFUS.Helper;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DOOFUS.Models.Persistence
{
    public class SettingDataRepository : ISettingsRepository
    {
        //Add setting
        public Setting Add(Setting setting)
        {
            using (var session = NHibernateHelper.OpenSession())
            {
                using (var transaction = session.BeginTransaction())
                {
                    session.Save(setting);
                    transaction.Commit();
                }
                return setting;
            }
        }
        
        //Delete specific setting
        public void Delete(int id)
        {
            var setting = Get(id);

            using (var session = NHibernateHelper.OpenSession())
            {
                using (var transaction = session.BeginTransaction())
                {
                    session.Delete(setting);
                    transaction.Commit();
                }
            }
        }

        //Update a setting
        public bool Update(Setting setting)
        {
            using (var session = NHibernateHelper.OpenSession())
            {
                using (var transaction = session.BeginTransaction())
                {
                    session.SaveOrUpdate(setting);
                    try
                    {
                        transaction.Commit();
                    }
                    catch (Exception)
                    {

                        throw;
                    }

                }
                return true;
            }
        }

        //Get a specified setting
        public Setting Get(int id)
        {
            using (var session = NHibernateHelper.OpenSession())
                return session.Get<Setting>(id);
        }

        //Get a list of all user settings for a given username
        public IEnumerable<Setting> GetUserSettings(int CustomerId, string UserName)
        {
            using (var session = NHibernateHelper.OpenSession())
                return session.Query<Setting>()
                    .Where(c => c.Id == EntityId && c.UserName == UserName && c.CustomerId == CustomerId).ToList();
        }      

        public IEnumerable<Setting> GetDeviceSettings(int CustomerId, int DeviceId)
        {
            using (var session = NHibernateHelper.OpenSession())
                return session.Query<Setting>()
                    .Where(c => c.DeviceId == DeviceId && c.CustomerId == CustomerId).ToList();                    
        }

        //Whats the setting for a specific settingkey for a specific device, at a given customer?
        public Setting GetDeviceSetting(int CustomerId, int DeviceId, string key)
        {
            using (var session = NHibernateHelper.OpenSession())
                return session.Query<Setting>()
                    .Where(c=> c.CustomerId == CustomerId && c.SettingKey == key && c.DeviceId == DeviceId).FirstOrDefault();
        }

        //Get all customer level settings matching specific setting key
        public IEnumerable<Setting> GetCustomerSettings(string key)
        {
            using(var session = NHibernateHelper.OpenSession())
                return session.Query<Setting>().Where(c => c.SettingKey == key && c.Level == "Customer").ToList();
        }

        public Setting GetCustomerSetting(int CustomerId, string key)
        {
            using (var session = NHibernateHelper.OpenSession())
                return session.Query<Setting>().Where(c => c.SettingKey == key&&c.Id==EntityId).FirstOrDefault();
        }

        public Setting GetGlobalSetting(string key)
        {
            using (var session = NHibernateHelper.OpenSession())
                return session.Query<Setting>().Where(c => c.SettingKey == key).FirstOrDefault();
        }

        public IEnumerable<Setting> GetAll()
        {
            using (var session = NHibernateHelper.OpenSession())
                return session.Query<Setting>().ToList();
        }

        
    }
}