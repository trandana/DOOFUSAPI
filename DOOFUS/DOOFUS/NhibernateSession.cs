using NHibernate;
using NHibernate.Cfg;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;


//Manages the Nhibernate session
//Defines the Nhibernate settings and config paths
namespace DOOFUS.Nhbnt.Web
{
    public class NhibernateSession
    {
        public static ISession OpenSession()
        {
            var configuration = new Configuration();
            var configurationPath = HttpContext.Current.Server.MapPath(@"~\Models\hibernate.cfg.xml");
            configuration.Configure(configurationPath);
            var settingConfigurationFile = HttpContext.Current.Server.MapPath(@"~\Mappings\Setting.hbm.xml");
            configuration.AddFile(settingConfigurationFile);
            ISessionFactory sessionFactory = configuration.BuildSessionFactory();
            return sessionFactory.OpenSession();
        }
    }
}