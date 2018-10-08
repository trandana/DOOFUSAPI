using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DOOFUS.Models
{
    //This abstract class is overridden in Setting
    public class SettingsBase
    {
        public virtual int Id { get; set; }

        public virtual string Key { get; set; }
        
        public virtual int CustomerId { get; set; }

        public virtual int DeviceId { get; set; }

        public virtual string UserName { get; set; }
        
        public virtual DateTime StartEffectiveDate { get; set; }

        public virtual DateTime EndEffectiveDate { get; set; }

        public virtual DateTime CreatedTimeStamp { get; set; }

        public virtual DateTime LastModifiedTimeStamp { get; set; }        

        public virtual string LastModifiedBy { get; set; }

        public virtual int LastModifiedById { get; set; }
    }
}
 
 
 