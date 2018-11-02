using DOOFUS.Helper;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DOOFUS.Models.Persistence
{
    public class SettingDataRepository : ISettingsRepository
    {
        public const string GLOBAL = "Global";
        public const string CUSTOMER = "Customer";
        public const string USER = "User";
        public const string DEVICE = "Device";

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

        //Update a setting if it exists or save a setting if it doesnt exist
        public bool SaveOrUpdate(Setting setting)
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

        //Only update a setting if it already exists
        public bool Update(Setting setting)
        {
            using (var session = NHibernateHelper.OpenSession())
            {
                using (var transaction = session.BeginTransaction())
                {
                    session.Update(setting);
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

        //Check if setting already exists
        public bool DoesSettingExist(Setting setting)
        {
            using (var session = NHibernateHelper.OpenSession())
                if (session.Query<Setting>()
                        .Where(c => c.SettingKey == setting.SettingKey && c.Level == setting.Level).FirstOrDefault() != null)
                {
                    return true;
                }
                else
                {
                    return false;
                }                
        }

        //Get a list of all user settings for a given username
        public IEnumerable<Setting> GetUserSettings(int CustomerId, string UserName)
        {
            using (var session = NHibernateHelper.OpenSession())
                return session.Query<Setting>()
                    .Where(c => c.CustomerId == CustomerId && c.UserName == UserName && c.Level == DEVICE).ToList();
        }

        

        public Setting GetUserSetting(int CustomerId, string key, string UserName)
        {
            using (var session = NHibernateHelper.OpenSession())
                return session.Query<Setting>()
                    .Where(c => c.SettingKey == key && c.UserName == UserName && c.CustomerId == CustomerId).FirstOrDefault();
        }

       
        public IEnumerable<Setting> GetDeviceSettings(int CustomerId, int DeviceId)
        {
            using (var session = NHibernateHelper.OpenSession())
                return session.Query<Setting>()
                    .Where(c => c.CustomerId == CustomerId && c.DeviceId == DeviceId && c.Level == DEVICE).ToList();                    
        }

        //Whats the setting for a specific settingkey for a specific device, at a given customer?
        public Setting GetDeviceSetting(int CustomerId, string key, int DeviceId)
        {
            using (var session = NHibernateHelper.OpenSession())
                return session.Query<Setting>()
                    .Where(c=> c.CustomerId == CustomerId && c.SettingKey == key && c.DeviceId == DeviceId).FirstOrDefault();
        }

        //Get all customer level settings matching specific setting key
        public IEnumerable<Setting> GetCustomerSettings(int CustomerId)
        {
            using(var session = NHibernateHelper.OpenSession())
                return session.Query<Setting>().Where(c => c.CustomerId == CustomerId && c.Level == CUSTOMER).ToList();
        }        

        //Get specific customer setting which matches this key
        public Setting GetCustomerSetting(int CustomerId, string key)
        {
            using (var session = NHibernateHelper.OpenSession())
                return session.Query<Setting>().Where(c => c.SettingKey == key && c.SettingKey == key && c.Level == CUSTOMER).FirstOrDefault();
        }

        public Setting GetGlobalSetting(string key)
        {
            using (var session = NHibernateHelper.OpenSession())
                return session.Query<Setting>().Where(c => c.SettingKey == key).FirstOrDefault();
        }
        public Setting GetGlobalSetting(int Id,string key)
        {
            using (var session = NHibernateHelper.OpenSession())
                return session.Query<Setting>().Where(c => c.SettingKey == key&&c.Id==Id).FirstOrDefault();
        }

        public IEnumerable<Setting> GetGlobalSetting()
        {
            using (var session = NHibernateHelper.OpenSession())
                return session.Query<Setting>().Where(d => d.Level == GLOBAL).ToList();
        }

        public IEnumerable<Setting> GetAll()
        {
            using (var session = NHibernateHelper.OpenSession())
                return session.Query<Setting>().ToList();
        }



    }
}