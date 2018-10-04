using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using FluentNHibernate.Mapping;

namespace DOOFUS.Models
{
    public class SettingsDataMap : ClassMap<Setting>
    {
        public SettingsDataMap()
        {
            Table("SettingsData");

            Id(x => x.Id, "Id").GeneratedBy.Identity().UnsavedValue(0);

            Map(x => x.Key);
            Map(x => x.CustomerId);
            Map(x => x.DeviceId);
            Map(x => x.UserName);
            Map(x => x.StartEffectiveDate);
            Map(x => x.EndEffectiveDate);
            Map(x => x.CreatedTimeStamp);
            Map(x => x.LastModifiedTimeStamp);
            Map(x => x.LastModifiedBy);
            Map(x => x.LastModifiedById);
            Map(x => x.Value);
        }
    }
}