﻿using DOOFUS.Models;
using DOOFUS.Models.Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
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

        [Route("settings/customer/{customerId}")]
        public IEnumerable<Setting> GetCustomerSettingData(int customerId)
        {
            return settingRepository.GetAll().Where(d => d.CustomerId == customerId);
        }

        [Route("settings/device/{customerId}/{deviceId}")]
        public IEnumerable<Setting> GetDeviceSettingData(int customerId, int deviceId)
        {
            return settingRepository.GetAll().Where(d => d.CustomerId == customerId && d.DeviceId == deviceId);
        }

        [Route("settings/user/{customerId}/{userName}")]
        public IEnumerable<Setting> GetUserSettingData(int customerId, string userName)
        {
            return settingRepository.GetAll().Where(d => d.CustomerId == customerId && d.UserName == userName);
        }

        //Get setting
        [Route("settings/{id}")]
        public Setting GetSettingById(int id)
        {
            var setting = settingRepository.Get(id);

            if (setting == null)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }
            return setting;
        }

        /*
         * This function interferes with the GET function above.
         * 
        //Get settings with specified key
        [Route("settings/{key}")]
        public IEnumerable<Setting> GetSettingsByKey(string key)
        {
            return settingRepository.GetAll().Where(d => d.SettingKey == key);
        }*/



        //
        // POST
        //  


        //
        //POST Global Level
        //       

        //Post a setting (global) with option to override lower levels
        [Route("settings/global")]
        public HttpResponseMessage PostGlobalSetting(Setting setting)
        {
            setting = settingRepository.Add(setting);

            var response = Request.CreateResponse<Setting>(HttpStatusCode.Created, setting);

            var uri = Url.Link("DefaultApi", new { id = setting.Id });
            //response.Headers.Location = new Uri(uri); 

            return response;
        }

        //Post a global setting with option to override lower levels
        [Route("settings/global/{key}/{overrideLower=true}")]
        public HttpResponseMessage PostGlobalSettingOverride(Setting setting)
        {
            setting = settingRepository.Add(setting);

            var response = Request.CreateResponse<Setting>(HttpStatusCode.Created, setting);

            var uri = Url.Link("GlobalOverride", new { id = setting.Id });
            response.Headers.Location = new Uri(uri);

            return response;
        }

        //Post a setting (global), no override 
        [Route("settings/global/{entity id}/{key}")]
        public HttpResponseMessage PostGlobalEntitySetting(Setting setting)
        {
            setting = settingRepository.Add(setting);

            var response = Request.CreateResponse<Setting>(HttpStatusCode.Created, setting);

            var uri = Url.Link("GlobalEntity", new { id = setting.Id });
            response.Headers.Location = new Uri(uri);

            return response;
        }

        //Post a global setting - Specific entity id with option to override 
        [Route("settings/global/{entity id}/{key}/{overrideLower=true}")]
        public HttpResponseMessage PostGlobalEntitySettingOverride(Setting setting)
        {
            setting = settingRepository.Add(setting);

            var response = Request.CreateResponse<Setting>(HttpStatusCode.Created, setting);

            var uri = Url.Link("GlobalEntityOverride", new { id = setting.Id });
            response.Headers.Location = new Uri(uri);

            return response;
        }

        //
        //POST Customer Level
        //

        //Post a setting (customer) 
        [Route("settings/customer/{key}")]
        public HttpResponseMessage PostCustomerSetting(Setting setting)
        {
            setting = settingRepository.Add(setting);

            var response = Request.CreateResponse<Setting>(HttpStatusCode.Created, setting);

            var uri = Url.Link("Customer", new { id = setting.Id });
            response.Headers.Location = new Uri(uri);

            return response;
        }

        //Post a setting (customer) with option to override lower levels
        [Route("settings/customer/{key}/{overrideLower=true}")]
        public HttpResponseMessage PostCustomerSettingOverride(Setting setting)
        {
            setting = settingRepository.Add(setting);

            var response = Request.CreateResponse<Setting>(HttpStatusCode.Created, setting);

            var uri = Url.Link("CustomerOverride", new { id = setting.Id });
            response.Headers.Location = new Uri(uri);

            return response;
        }

        //Post a customer setting - Specific entity id 
        [Route("settings/customer/{entity id}/{key}")]
        public HttpResponseMessage PostCustomerEntitySetting(Setting setting)
        {
            setting = settingRepository.Add(setting);

            var response = Request.CreateResponse<Setting>(HttpStatusCode.Created, setting);

            var uri = Url.Link("CustomerEntity", new { id = setting.Id });
            response.Headers.Location = new Uri(uri);

            return response;
        }

        //Post a customer setting - Specific entity id with option to override lower levels
        [Route("settings/customer/{entity id}/{key}/{overrideLower=true}")]
        public HttpResponseMessage PostCustomerEntitySettingOverride(Setting setting)
        {
            setting = settingRepository.Add(setting);

            var response = Request.CreateResponse<Setting>(HttpStatusCode.Created, setting);

            var uri = Url.Link("CustomerEntityOverride", new { id = setting.Id });
            response.Headers.Location = new Uri(uri);

            return response;
        }

        //
        //POST Device Level
        //

        //Post a device setting 
        [Route("settings/device/{customer id}/{key}")]
        public HttpResponseMessage PostDeviceSetting(Setting setting)
        {
            setting = settingRepository.Add(setting);

            var response = Request.CreateResponse<Setting>(HttpStatusCode.Created, setting);

            var uri = Url.Link("DeviceOverride", new { id = setting.Id });
            response.Headers.Location = new Uri(uri);

            return response;
        }

        //Post a device setting and override lower levels
        [Route("settings/device/{customer id}/{key}/{overrideLower=true}")]
        public HttpResponseMessage PostDeviceSettingOverride(Setting setting)
        {
            setting = settingRepository.Add(setting);

            var response = Request.CreateResponse<Setting>(HttpStatusCode.Created, setting);

            var uri = Url.Link("DeviceOveride", new { id = setting.Id });
            response.Headers.Location = new Uri(uri);

            return response;
        }

        //Post a device setting for specific entity
        [Route("settings/device/{entity id}/{customer id}/{key}")]
        public HttpResponseMessage PostDeviceEntitySetting(Setting setting)
        {
            setting = settingRepository.Add(setting);

            var response = Request.CreateResponse<Setting>(HttpStatusCode.Created, setting);

            var uri = Url.Link("DeviceEntity", new { id = setting.Id });
            response.Headers.Location = new Uri(uri);

            return response;
        }

        //Post a device setting for specific entity
        [Route("settings/device/{entity id}/{customer id}/{key}/{overrideLower=true}")]
        public HttpResponseMessage PostDeviceEntitySettingOverride(Setting setting)
        {
            setting = settingRepository.Add(setting);

            var response = Request.CreateResponse<Setting>(HttpStatusCode.Created, setting);

            var uri = Url.Link("DeviceEntityOverride", new { id = setting.Id });
            response.Headers.Location = new Uri(uri);

            return response;
        }

        //
        //POST User Level
        //

        //Post a user setting 
        [Route("settings/user/{entity id}/{customer id}/{key}")]
        public HttpResponseMessage PostUserSetting(Setting setting)
        {
            setting = settingRepository.Add(setting);

            var response = Request.CreateResponse<Setting>(HttpStatusCode.Created, setting);

            var uri = Url.Link("User", new { id = setting.Id });
            response.Headers.Location = new Uri(uri);

            return response;
        }

        //Post a user setting for specific entity
        [Route("settings/user/{entity id}/{customer id}/{key}")]
        public HttpResponseMessage PostUserEntitySetting(Setting setting)
        {
            setting = settingRepository.Add(setting);

            var response = Request.CreateResponse<Setting>(HttpStatusCode.Created, setting);

            var uri = Url.Link("UserEntity", new { id = setting.Id });
            response.Headers.Location = new Uri(uri);

            return response;
        }


        //
        // PUT
        //

        //
        //PUT Global Level
        //

        //Put setting (global)
        [Route("settings/global/{id}")]
        public HttpResponseMessage PutGlobalSetting(int id, Setting setting)
        {
            //get setting to update by 
            var currentSetting = settingRepository.Get(id);
            if (currentSetting == null)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }

            //replace current setting values with new setting values
            currentSetting.SettingKey = setting.SettingKey;
            currentSetting.CustomerId = setting.CustomerId;
            currentSetting.DeviceId = setting.DeviceId;
            currentSetting.UserName = setting.UserName;
            currentSetting.StartEffectiveDate = setting.StartEffectiveDate;
            currentSetting.EndEffectiveDate = setting.EndEffectiveDate;
            currentSetting.CreatedTimeStamp = setting.CreatedTimeStamp;
            currentSetting.LastModifiedTimeStamp = setting.LastModifiedTimeStamp;
            currentSetting.LastModifiedBy = setting.LastModifiedBy;
            currentSetting.LastModifiedById = setting.LastModifiedById;
            currentSetting.Value = setting.Value;

            //update setting
            if (!settingRepository.Update(currentSetting))
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }
            else
            {
                var response = Request.CreateResponse<Setting>(HttpStatusCode.Created, setting);
                return response;
            }
        }

        //Put global with option to override 
        [Route("settings/global/{key}/{overrideLower?}")]
        public HttpResponseMessage PutGlobalSettingOverride(string key, Setting setting, bool overrideLower = false)
        {
            //get all global settings with same key
            var listOfCurrentSettings = settingRepository.GetAll().Where(x => x.SettingKey == key).ToList();

            if (listOfCurrentSettings == null)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }

            if (!overrideLower) //if overrideLower is false, update all settings in list of current setting
            {
                for (int i = 0; i < listOfCurrentSettings.Count(); i++)
                {
                    listOfCurrentSettings[i].SettingKey = setting.SettingKey;
                    listOfCurrentSettings[i].CustomerId = setting.CustomerId;
                    listOfCurrentSettings[i].DeviceId = setting.DeviceId;
                    listOfCurrentSettings[i].UserName = setting.UserName;
                    listOfCurrentSettings[i].StartEffectiveDate = setting.StartEffectiveDate;
                    listOfCurrentSettings[i].EndEffectiveDate = setting.EndEffectiveDate;
                    listOfCurrentSettings[i].CreatedTimeStamp = setting.CreatedTimeStamp;
                    listOfCurrentSettings[i].LastModifiedTimeStamp = setting.LastModifiedTimeStamp;
                    listOfCurrentSettings[i].LastModifiedBy = setting.LastModifiedBy;
                    listOfCurrentSettings[i].LastModifiedById = setting.LastModifiedById;
                    listOfCurrentSettings[i].Value = setting.Value;
                }
            }
           /* else //if overrideLower is true, override device customer, and user settings associated with the specified key
            {
                
            }*/

            //update settings
            for (int j = 0; j < listOfCurrentSettings.Count(); j++)
            {
                if (!settingRepository.Update(listOfCurrentSettings[j]))
                {
                    throw new HttpResponseException(HttpStatusCode.NotFound);
                }
            }
            var response = Request.CreateResponse<Setting>(HttpStatusCode.Created, setting);
            return response;
        }

        //Put setting - Specific Entity (global)
        [Route("settings/global/{entityId}/{key}/{overrideLower?}")]
        public HttpResponseMessage PutGlobalEntitySetting(int entityId, string key, Setting setting, bool overrideLower= false)
        {
            //Using first item in list because only one item should return from this query
            var currentSetting = settingRepository.GetAll().Where(x => x.DeviceId == entityId && x.SettingKey == key).ToList()[0];

            if (currentSetting == null)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }

            if (!overrideLower) //if overrideLower is false, update specified setting with new setting values
            {
                //replace current setting values with new setting values
                currentSetting.SettingKey = setting.SettingKey;
                currentSetting.CustomerId = setting.CustomerId;
                currentSetting.DeviceId = setting.DeviceId;
                currentSetting.UserName = setting.UserName;
                currentSetting.StartEffectiveDate = setting.StartEffectiveDate;
                currentSetting.EndEffectiveDate = setting.EndEffectiveDate;
                currentSetting.CreatedTimeStamp = setting.CreatedTimeStamp;
                currentSetting.LastModifiedTimeStamp = setting.LastModifiedTimeStamp;
                currentSetting.LastModifiedBy = setting.LastModifiedBy;
                currentSetting.LastModifiedById = setting.LastModifiedById;
                currentSetting.Value = setting.Value;
            }
            /*else //if overrideLower is true, override lower levels
            {
                
            }*/


            //update setting
            if (!settingRepository.Update(currentSetting))
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }
            else
            {
                var response = Request.CreateResponse<Setting>(HttpStatusCode.Created, setting);
                return response;
            }
        }

        //
        //PUT Customer Level
        //

        //Put setting (customer)
        [Route("settings/customer/{key}")]
        public void PutCustomerSetting(int id, Setting setting)
        {
            setting.Id = id;

            if (!settingRepository.Update(setting))
                throw new HttpResponseException(HttpStatusCode.NotFound);
        }

        //Put setting (customer) and override lower
        [Route("settings/customer/{key}/{overrideLower=true}")]
        public void PutCustomerSettingOverride(int id, Setting setting)
        {
            setting.Id = id;

            if (!settingRepository.Update(setting))
                throw new HttpResponseException(HttpStatusCode.NotFound);
        }

        //Put setting - Specific Entity (customer)
        [Route("settings/customer/{entity id}/{key}")]
        public void PutCustomerEntitySetting(int id, Setting setting)
        {
            setting.Id = id;

            if (!settingRepository.Update(setting))
                throw new HttpResponseException(HttpStatusCode.NotFound);
        }                

        //Put setting - Specific Entity (customer) and override lower
        [Route("settings/customer/{entity id}/{key}/{overrideLower=true}")]
        public void PutCustomerEntitySettingOverride(int id, Setting setting)
        {
            setting.Id = id;

            if (!settingRepository.Update(setting))
                throw new HttpResponseException(HttpStatusCode.NotFound);
        }

        //
        //PUT Device Level      
        //

        //Put setting for device
        [Route("settings/device/{customer id}/{key}")]
        public void PutDeviceSetting(int id, Setting setting)
        {
            setting.Id = id;

            if (!settingRepository.Update(setting))
                throw new HttpResponseException(HttpStatusCode.NotFound);
        }

        //Put setting for device and override lower levels
        [Route("settings/device/{customer id}/{key}/{overrideLower=true}")]
        public void PutDeviceSettingOverride(int id, Setting setting)
        {
            setting.Id = id;

            if (!settingRepository.Update(setting))
                throw new HttpResponseException(HttpStatusCode.NotFound);
        }

        //Put setting for device - Specific entity 
        [Route("settings/device/{customer id}/{entity id}/{key}")]
        public void PutDeviceEntitySetting(int id, Setting setting)
        {
            setting.Id = id;

            if (!settingRepository.Update(setting))
                throw new HttpResponseException(HttpStatusCode.NotFound);
        }

        //Put setting for device - Specific entity and override lower levels
        [Route("settings/device/{customer id}/{entity id}/{key}/{overrideLower=true}")]
        public void PutDeviceEntitySettingOverride(int id, Setting setting)
        {
            setting.Id = id;

            if (!settingRepository.Update(setting))
                throw new HttpResponseException(HttpStatusCode.NotFound);
        }


        //
        //PUT User Level
        //

        //Put setting for user 
        [Route("settings/user/{customer id}/{key}")]
        public void PutUserSetting(int id, Setting setting)
        {
            setting.Id = id;

            if (!settingRepository.Update(setting))
                throw new HttpResponseException(HttpStatusCode.NotFound);
        }
        
        //Put setting for user - Specific entity 
        [Route("settings/user/{customer id}/{entity id}/{key}")]
        public void PutUserEntitySetting(int id, Setting setting)
        {
            setting.Id = id;

            if (!settingRepository.Update(setting))
                throw new HttpResponseException(HttpStatusCode.NotFound);
        }     

       
        //
        //DELETE
        //

        //
        //DELETE Global Level
        //

        //Delete a setting global level
        [Route("settings/global/{id}")]
        public void DeleteGlobalSetting(int id)
        {
            var setting = settingRepository.Get(id);
            if (setting == null)//have to insert into table after api is running to work, other than that, works as intended.
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }
            
            settingRepository.Delete(id);
        }

        //Delete a setting at global level and delete all lower levels also
        [Route("settings/global/{key}/{overrideLower=true}")]
        public void DeleteGlobalSettingOverride(int id)
        {
            var setting = settingRepository.Get(id);

            if (setting == null)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }
            settingRepository.Delete(id);
        }

        //Delete a setting at global level for specific entity id 
        [Route("settings/global/{entity id}/{key}")]
        public void DeleteGlobalEntitySetting(int id)
        {
            var setting = settingRepository.Get(id);

            if (setting == null)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }
            settingRepository.Delete(id);
        }

        //Delete a setting at global level for specific entity and override all lower levels also
        [Route("settings/global/{entity id}/{key}/{overrideLower=true}")]
        public void DeleteGlobalEntitySettingOverride(int id)
        {
            var setting = settingRepository.Get(id);

            if (setting == null)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }
            settingRepository.Delete(id);
        }
        
        //
        //DELETE Customer Level
        //

        //Delete a setting at customer level
        [Route("settings/customer/{key}")]
        public HttpResponseMessage DeleteCustomerSetting(string key)
        {
            var setting = settingRepository.GetCustomerSetting(key);

            if (setting == null)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }
            var response = Request.CreateResponse<Setting>(HttpStatusCode.OK, setting);

            settingRepository.Delete(setting.Id);

            return response;
        }

        //Delete a setting at customer level and override all lower levels also
        [Route("settings/customer/{key}/{overrideLower=true}")]
        public void DeleteCustomerSettingOverride(String key)
        {
            var setting = settingRepository.GetCustomerSetting(key);

            if (setting == null)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }
            settingRepository.Delete(setting.Id);

            var SettingList = settingRepository.GetAll();

        }

        //Delete a setting at customer level for specific entity id
        [Route("settings/customer/{entity id}/{key}")]
        public void DeleteCustomerEntitySetting(int id)
        {
            var setting = settingRepository.Get(id);

            if (setting == null)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }
            settingRepository.Delete(id);
        }

        //Delete a setting at customer level for specific entity id and override all lower levels also
        [Route("settings/customer/{entity id}/{key}/{overrideLower=true}")]
        public void DeleteCustomerEntitySettingOverride(int id)
        {
            var setting = settingRepository.Get(id);

            if (setting == null)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }
            settingRepository.Delete(id);
        }

        //
        //DELETE Device Level
        //

        //Delete a setting at device level
        [Route("settings/device/{CustomerId}/{key}")]
        public HttpResponseMessage DeleteDeviceSetting(int CustomerId,string key)
        {
            var setting = settingRepository.GetDeviceSetting(CustomerId,key);

            if (setting == null)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }
            var response = Request.CreateResponse<Setting>(HttpStatusCode.OK, setting);

            settingRepository.Delete(setting.Id);

            return response;
        }

        //Delete a setting at device level for specific entity id 
        [Route("settings/device/{CustomerId}/{deviceId}/{entityId}")]
        public HttpResponseMessage DeleteDeviceEntitySetting(int CustomerId,int deviceId,int entityId)
        {
            var setting = settingRepository.GetDeviceSetting(CustomerId,deviceId,entityId);

            if (setting == null)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }
            var response = Request.CreateResponse<Setting>(HttpStatusCode.OK, setting);

            settingRepository.Delete(setting.Id);

            return response;
        }

        //
        //DELETE User Level
        //

        //Delete a setting at user level
        //done
        [Route("settings/user/{CustomerId}/{Username}")]
        public HttpResponseMessage DeleteUserSetting(string Username,int CustomerId)
        {
            var setting = settingRepository.GetUserSetting(CustomerId,Username);
            
            if (setting == null)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }
            var response = Request.CreateResponse<Setting>(HttpStatusCode.OK,setting);

            settingRepository.Delete(setting.Id);

            return response;//delete the setting and return it
        }

        //Delete a setting at user level for specific entity id
        //done
        [Route("settings/user/{CustomerId}/{deviceId}/{entityId}")]
        public HttpResponseMessage DeleteUserEntitySetting(int CustomerId,int DeviceId,int id)
        {
            var setting = settingRepository.GetUserSetting(CustomerId, DeviceId, id);

            if (setting == null)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }
            var response = Request.CreateResponse<Setting>(HttpStatusCode.OK, setting);
            settingRepository.Delete(setting.Id);
            return response;
        }

    }
}