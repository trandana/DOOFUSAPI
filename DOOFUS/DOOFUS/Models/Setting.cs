using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DOOFUS.Models
{
    public class Setting : SettingsBase
    {
        public virtual string Value { get; set; }
        public virtual string Level { get; set; }
    }
}