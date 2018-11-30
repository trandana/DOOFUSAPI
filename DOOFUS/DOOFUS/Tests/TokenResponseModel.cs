using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Newtonsoft.Json;

namespace DOOFUS.Tests
{
    public class TokenResponseModel
    {
        [JsonProperty("Value")]
        public string Value { get; set; }

        [JsonProperty("SettingKey")]
        public string SettingKey { get; set; }

        [JsonProperty("CustomerId")]
        public string CustomerId { get; set; }

        [JsonProperty("DeviceId")]
        public string DeviceId { get; set; }

        [JsonProperty("Level")]
        public string Level { get; set; }

        [JsonProperty("UserName")]
        public string UserName { get; set; }

        [JsonProperty("StartEffectiveDate")]
        public string StartEffectiveDate { get; set; }

        [JsonProperty("EndEffectiveDate")]
        public string EndEffectiveDate { get; set; }

        [JsonProperty("CreatedTimeStamp")]
        public string CreatedTimeStamp { get; set; }

        [JsonProperty("LastModifiedTimeStamp")]
        public string LastModifiedTimeStamp { get; set; }

        [JsonProperty("LastModifiedBy")]
        public string LastModifiedBy { get; set; }

        [JsonProperty("LastModifiedById")]
        public string LastModifiedById { get; set; }

       
    }
}