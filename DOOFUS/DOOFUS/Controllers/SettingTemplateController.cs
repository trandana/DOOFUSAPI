using DOOFUS.Models;
using DOOFUS.Models.Persistence;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Mvc;

namespace DOOFUS.Nhbnt.Web.Controllers
{

    //POST/PUT/DELETE api/settingstemplate/{global or customer}/{setting key}?ids={csv list of entity ids}&overrideLower={optional true or false, default false} 
    //POST/PUT/DELETE api/settingstemplate/{global or customer}/{entity id}/{setting key}? overrideLower={true or false, optional parameter}
    //POST/PUT/DELETE api/settingstemplate/{device or user}/{customer id}/{setting key}?ids= {csv list of entity ids} 
    //POST/PUT/DELETE api/settingstemplate/{device or user}/{customer id}/{entity id}/{setting key}
    public class SettingTemplateController : ApiController
    {
        static readonly ISettingsRepository settingRepository = new SettingDataRepository();

        public IEnumerable<Setting> GetSettingData()
        {
            return settingRepository.GetAll();
        }

        //Get setting
        public Setting GetSettingById(int id, string key)
        {
            var setting = settingRepository.Get(id);

            if(setting == null)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);                
            }
            return setting;
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