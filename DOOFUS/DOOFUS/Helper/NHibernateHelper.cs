using DOOFUS.Models;
using FluentNHibernate.Cfg;
using FluentNHibernate.Cfg.Db;
using NHibernate;
using NHibernate.Linq;
using NHibernate.Tool.hbm2ddl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DOOFUS.Helper
{
    public class NHibernateHelper
    {
        private static ISessionFactory _sessionFactory;

        public static ISession OpenSession()
        {
            return SessionFactory.OpenSession();
        }

        private static ISessionFactory SessionFactory
        {
            get
            {
                if (_sessionFactory == null)
                    _sessionFactory = CreateSessionFactory();

                return _sessionFactory;
            }
        }

        //Session factory
        private static ISessionFactory CreateSessionFactory()
        {
            return Fluently.Configure().Database(MsSqlConfiguration.MsSql2008.ConnectionString(c => c.FromConnectionStringWithKey("DOOFUSDatabase"))).Mappings(m => m.FluentMappings.AddFromAssemblyOf<Setting>())
                .ExposeConfiguration(cfg => new SchemaExport(cfg).Create(true, true)).BuildSessionFactory();           
        }
        
    }
}