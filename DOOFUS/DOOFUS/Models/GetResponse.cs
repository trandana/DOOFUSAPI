using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DOOFUS.Models
{
    public class GetResponse
    {
        public virtual string Id { get; set; }
        public virtual string Level { get; set; }
        public virtual IEnumerable<Setting> Settings { get; set; }
    }
}