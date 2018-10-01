using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DOOFUS.Models
{
    public class Setting
    {
        public int Id { get; set; }

        public string Key { get; set; }
        
        public int CustomerId { get; set; }

        public string UserName { get; set; }

        //Can be used once DateTime is defined
        /*
        public DateTime StartEffectiveDate { get; set; }

        public DateTime EndEffectiveDate { get; set; }

        public DateTime CreatedTimeStamp { get; set; }

        public DateTime LastModifiedTimeStamp { get; set; }
        */

        public string LastModifiedBy { get; set; }

        public int LastModifiedById { get; set; }
    }
}
 
 
 