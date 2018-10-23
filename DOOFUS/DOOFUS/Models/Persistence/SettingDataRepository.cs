using DOOFUS.Helper;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DOOFUS.Models.Persistence
{
    public class SettingDataRepository : ISettingsRepository
    {
        //Query Goes here
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
        
        public Setting Get(int id)
        {
            using (var session = NHibernateHelper.OpenSession())
                return session.Get<Setting>(id);
        }

        public Setting GetUserSetting(int CustomerId, string Username)
        {
            using (var session = NHibernateHelper.OpenSession())
                return session.Query<Setting>().Where(c => c.UserName == Username&&c.CustomerId== CustomerId).FirstOrDefault();
        }

        public Setting GetUserSetting(int CustomerId,int DeviceId, int EntityId)
        {
            using (var session = NHibernateHelper.OpenSession())
                return session.Query<Setting>()
                    .Where(c => c.Id == EntityId && c.CustomerId == CustomerId && c.DeviceId == DeviceId)
                    .FirstOrDefault();
        }

        public Setting GetDeviceSetting(int CustomerId, int DeviceId, int EntityId)
        {
            using (var session = NHibernateHelper.OpenSession())
                return session.Query<Setting>()
                    .Where(c => c.Id == EntityId && c.CustomerId == CustomerId && c.DeviceId == DeviceId)
                    .FirstOrDefault();
        }

        public Setting GetDeviceSetting(int CustomerId, string key)
        {
            using (var session = NHibernateHelper.OpenSession())
                return session.Query<Setting>().Where(c=>c.CustomerId==CustomerId&&c.SettingKey==key).FirstOrDefault();
        }

        public Setting GetCustomerSetting(string key)
        {
            using(var session = NHibernateHelper.OpenSession())
                return session.Query<Setting>().Where(c => c.SettingKey == key).FirstOrDefault();
        }

        public Setting GetCustomerSetting(int EntityId,string key)
        {
            using (var session = NHibernateHelper.OpenSession())
                return session.Query<Setting>().Where(c => c.SettingKey == key&&c.Id==EntityId).FirstOrDefault();
        }

        public Setting GetGlobalSetting(String key)
        {
            using (var session = NHibernateHelper.OpenSession())
                return session.Query<Setting>().Where(c => c.SettingKey == key).FirstOrDefault();
        }

        public IEnumerable<Setting> GetAll()
        {
            using (var session = NHibernateHelper.OpenSession())
                return session.Query<Setting>().ToList();
        }

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
    }
}