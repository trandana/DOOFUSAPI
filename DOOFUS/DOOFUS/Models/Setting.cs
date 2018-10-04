using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DOOFUS.Models
{
    public abstract class Setting : SettingsBase
    {
        public string Value { get; set; }
    }
}