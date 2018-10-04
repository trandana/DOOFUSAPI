using DOOFUS.Models;
using DOOFUS.Models.Persistence;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Web.Http;
//using System.Web.Mvc;

namespace DOOFUS.Nhbnt.Web.Controllers
{
    public class SettingsController : ApiController
    {
        static readonly ISettingsRepository settingRepository = new SettingDataRepository();

        [Route("settings/global")]
        public IEnumerable<Setting> GetGlobalSettingData()
        {
            return settingRepository.GetAll();
        }

        [Route ("settings/customer/{customerId}")]
        public IEnumerable<Setting> GetCustomerSettingData(int customerId)
        {
            //return all customer settings based on customer id. (json structure)
        }

        [Route("settings/device/{customerId}/{deviceId}")]
        public IEnumerable<Setting> GetDeviceSettingData(int customerId, int deviceId)
        {
            //return all device settings based on customerid and device id (json structure)
        }

        [Route("settings/user/{customerId}/{userId}")]
        public IEnumerable<Setting> GetUserSettingData(int customerId, int userId)
        {
            //return all user settings based on customerid and userid (json structure) 
        }

        //Get setting
        [Route("settings/{id}")]
        public Setting GetSettingById(int id)
        {
            var setting = settingRepository.Get(id);

            if(setting == null)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);                
            }
            return setting;
        }

        //Get settings with specified key
        [Route("settings/{key}")]
        public void GetSettingsByKey(string key)
        {
           //return json structure of different settings values specified by key
        }

        //Delete a setting
        public void DeleteSetting(int id)
        {
            var setting = settingRepository.Get(id);

            if(setting == null)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }
            settingRepository.Delete(id);
        }

        //Post a setting
        
        public HttpResponseMessage PostSetting(Setting setting)
        {
            setting = settingRepository.Add(setting);

            var response = Request.CreateResponse<Setting>(HttpStatusCode.Created, setting);

            var uri = Url.Link("DefaultApi", new { id = setting.Id });
            response.Headers.Location = new Uri(uri);

            return response;
        }

        public void PutSetting(int id, Setting setting)
        {
            setting.Id = id;

            if (!settingRepository.Update(setting))
                throw new HttpResponseException(HttpStatusCode.NotFound);
        }


    }
}