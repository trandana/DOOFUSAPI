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