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

        //Session factory
        private static void CreateSessionFactory()
        {
            string connectionString = "Server=EC2AMAZ-TFIDP2I; Database=TestDB; User Id=sa; Password=d00f20!8;";

            _sessionFactory = Fluently.Configure().Database(MsSqlConfiguration.MsSql2008.ConnectionString(connectionString).ShowSql).Mappings(m => m.FluentMappings.AddFromAssemblyOf<Setting>())
               .ExposeConfiguration(cfg => new SchemaExport(cfg).Create(false, false)).BuildSessionFactory();
        }
        
    }
}