using DOOFUS.Models;
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

        public const string GLOBAL = "Global";
        public const string CUSTOMER = "Customer";
        public const string USER = "User";
        public const string DEVICE = "Device";

        //GET a global setting
        [Route("settings/global")]
        public IEnumerable<Setting> GetGlobalSettingData()
        {
            return settingRepository.GetAll().Where(d => d.Level == "Global");
        }

        //Get all settings for specific customer
        [Route("settings/customer/{customerId}")]
        public IEnumerable<Setting> GetCustomerSettingData(int customerId)
        {
            return settingRepository.GetAll().Where(d => d.CustomerId == customerId && d.Level == CUSTOMER);
        }

        //Get all settings for a specific device
        [Route("settings/device/{customerId}/{deviceId}")]
        public IEnumerable<Setting> GetDeviceSettingData(int customerId, int deviceId)
        {
            return settingRepository.GetAll().Where(d => d.CustomerId == customerId && d.DeviceId == deviceId && d.Level == DEVICE);
        }

        //Get all settigns for a specific username
        [Route("settings/user/{customerId}/{userName}")]
        public IEnumerable<Setting> GetUserSettingData(int customerId, string userName)
        {
            return settingRepository.GetAll().Where(d => d.CustomerId == customerId && d.UserName == userName && d.Level == DEVICE);
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

        //Post a setting (global) 
        [Route("settings/global")]
        public HttpResponseMessage PostGlobalSetting(Setting setting)
        {
            setting.Level = GLOBAL;
            setting.LastModifiedBy = GLOBAL;
            setting.LastModifiedTimeStamp = DateTime.UtcNow;
            setting = settingRepository.Add(setting);

            var response = Request.CreateResponse<Setting>(HttpStatusCode.Created, setting);

            var uri = Url.Link("Global", new { id = setting.Id });
            response.Headers.Location = new Uri(uri); 

            return response;
        }

        //Post a global setting with option to override lower levels
        [Route("settings/global/{key}/{overrideLower=true}")]
        public HttpResponseMessage PostGlobalSettingOverride(Setting setting)
        {
            setting.Level = GLOBAL;
            setting.LastModifiedBy = GLOBAL;
            setting.LastModifiedTimeStamp = DateTime.UtcNow;
            
            //Get list of existing settings which match the incoming one, if any
            var SettingList = settingRepository.GetAll()
               .Where(c => c.SettingKey == setting.SettingKey).ToList();

            if (SettingList.Count() < 1) //no existing setting found, just add new
            {
                settingRepository.Add(setting);
            }
            //existing setting(s) found, lets override them and add the new one
            else
            {
                foreach (var c in SettingList)
                {
                    settingRepository.Delete(c.Id);
                    settingRepository.Add(setting);
                }
            }           

            var response = Request.CreateResponse<Setting>(HttpStatusCode.Created, setting);

            var uri = Url.Link("GlobalOverride", new { id = setting.Id });
            response.Headers.Location = new Uri(uri);

            return response;
        }

        
        //Post a setting (global) to multiple customers - with override
        //settings/global/{key}?customerids=1,2,50, etc
        [Route("settings/global/{key}/{overrideLower}")]
        public HttpResponseMessage PostGlobalEntitySetting(Setting setting, string customerids, string key, bool overrideLower)
        {
            setting.Level = GLOBAL;
            setting.LastModifiedBy = GLOBAL;
            setting.LastModifiedTimeStamp = DateTime.UtcNow;

            //separate customer id string into individual ints
            string[] separated = customerids.Split( ',' );
            int parsed = 0;

            //override was specified so override lower levels
            if (overrideLower)
            {
                for(int i = 0; i < separated.Count(); i++)
                {                   
                    Int32.TryParse(separated.ElementAt(i), out parsed);
                    
                    //Get list of existing settings for customer and lower levels which match the incoming one, if any
                    var SettingList = settingRepository.GetAll()
                        .Where(c => c.SettingKey == key && c.CustomerId == parsed).ToList();

                    foreach (var c in SettingList)
                    {
                        //delete the existing entries
                        settingRepository.Delete(c.Id);
                        //add new entries
                        settingRepository.Add(setting);
                    }                    
                }                
            }

            //Override was not specified, don't override lower levels
            else
            {
                foreach (var id in separated)
                {
                    Int32.TryParse(id, out int x);
                    setting.CustomerId = x;
                    settingRepository.Add(setting);
                }
            }            

            var response = Request.CreateResponse<Setting>(HttpStatusCode.Created, setting);

            var uri = Url.Link("GlobalEntity", new { id = setting.Id });
            response.Headers.Location = new Uri(uri);

            return response;
        }

        //Post a global setting - Specific customer id with option to override lower levels
        [Route("settings/global/{customerid}/{key}/{overrideLower=true}")]
        public HttpResponseMessage PostGlobalEntitySettingOverride(Setting setting, int customerid, string key)
        {
            setting.Level = GLOBAL;
            setting.LastModifiedBy = GLOBAL;
            setting.LastModifiedTimeStamp = DateTime.UtcNow;

            //Get list of existing settings which match the incoming one, if any
            var SettingList = settingRepository.GetAll()
               .Where(c => c.SettingKey == key && c.CustomerId == customerid).ToList();

            if (SettingList.Count() > 0)
            {
                foreach (var c in SettingList)
                {
                    settingRepository.Delete(c.Id);
                    settingRepository.Add(setting);
                }
            }            

            var response = Request.CreateResponse<Setting>(HttpStatusCode.Created, setting);

            var uri = Url.Link("GlobalEntityOverride", new { id = setting.Id });
            response.Headers.Location = new Uri(uri);

            return response;
        }

        //
        //POST Customer Level
        //

        //WARNING: This doesn't appear correct. We want customer level access to be able to create settings 
        //for all customers?? Thats what it says in the spec, but must be a mistake

        //Post a setting for a all customers, do not override lower levels
        [Route("settings/customer/{customerid}/{key}")]
        public HttpResponseMessage PostCustomerSetting(Setting setting, string key)
        {
            setting.Level = CUSTOMER;
            setting.LastModifiedBy = CUSTOMER;
            setting.LastModifiedTimeStamp = DateTime.UtcNow;

            //Get all setting matching this key at customer level
            var SettingList = settingRepository.GetAll()
              .Where(c => c.SettingKey == key && c.Level == CUSTOMER).ToList();

            //Setting exists. Delete it before adding new
            if(SettingList.Count() > 0)
            {
                foreach (var c in SettingList)
                {
                    settingRepository.Delete(c.Id);
                    settingRepository.Add(setting);
                }                
            }
            //Setting does not exist. Just add new
            else
            {
                settingRepository.Add(setting);
            }

            var response = Request.CreateResponse<Setting>(HttpStatusCode.Created, setting);

            var uri = Url.Link("Customer", new { id = setting.Id });
            response.Headers.Location = new Uri(uri);

            return response;
        }

        //Post a setting for every customer and override each customer's lower levels
        [Route("settings/customer/{key}/{overrideLower=true}")]
        public HttpResponseMessage PostCustomerSettingOverride(Setting setting, string key, bool overrideLower)
        {
            //Customer level override
            setting.Level = CUSTOMER;
            setting.LastModifiedBy = GLOBAL;
            setting.LastModifiedTimeStamp = DateTime.UtcNow;

            //Get all settings matching this key at Customer level or lower
            var SettingList = settingRepository.GetAll()
              .Where(c => c.SettingKey == key && c.Level == CUSTOMER).ToList(); 
            
            //Setting exists. Delete it before adding new
            if (SettingList.Count() > 0)
            {
                foreach (var c in SettingList)
                {                    
                    settingRepository.Delete(c.Id);
                    settingRepository.Add(setting);
                }                
            }
            //Setting does not exist. Just add new
            else
            {
                settingRepository.Add(setting);
            }

            setting.Level = USER;
            SettingList = settingRepository.GetAll()
              .Where(c => c.SettingKey == key && c.Level == USER).ToList();

            //we could do this all in one for loop, but maybe we want to keep some of the
            //existing table values? like created time stamp? etc etc 
            //Setting exists. Delete it before adding new
            if (SettingList.Count() > 0)
            {
                foreach (var c in SettingList)
                {                   
                    setting.DeviceId = c.DeviceId;
                    setting.UserName = c.UserName;                    
                    settingRepository.Delete(c.Id);
                    settingRepository.Add(setting);
                }
            }
            //Setting does not exist. Just add new
            else
            {
                settingRepository.Add(setting);
            }

            //Device level override
            setting.Level = DEVICE;

            SettingList = settingRepository.GetAll()
              .Where(c => c.SettingKey == key && c.Level == DEVICE).ToList();

            //Setting exists. Delete it before adding new
            if (SettingList.Count() > 0)
            {
                foreach (var c in SettingList)
                {
                    setting.DeviceId = c.DeviceId;
                    setting.UserName = c.UserName;
                    settingRepository.Delete(c.Id);
                    settingRepository.Add(setting);
                }
            }
            //Setting does not exist. Just add new
            else
            {
                settingRepository.Add(setting);
            }

            var response = Request.CreateResponse<Setting>(HttpStatusCode.Created, setting);

            var uri = Url.Link("CustomerOverride", new { id = setting.Id });
            response.Headers.Location = new Uri(uri);

            return response;
        }

        //Post a setting for a specific customer - do not override lower levels
        [Route("settings/customer/{customerid}/{key}")]
        public HttpResponseMessage PostCustomerEntitySetting(Setting setting, int customerid, string key)
        {
            //Customer level
            setting.Level = CUSTOMER;
            setting.CustomerId = customerid;            
            setting.LastModifiedById = customerid;
            setting.LastModifiedTimeStamp = DateTime.UtcNow;

            //Get all settings matching this key at Customer level with this customer id
            var SettingList = settingRepository.GetAll()
              .Where(c => c.SettingKey == key && c.Level == CUSTOMER && c.CustomerId == customerid).ToList();

            //Setting exists. Delete it before adding new
            if (SettingList.Count() > 0)
            {
                foreach (var c in SettingList)
                {
                    settingRepository.Delete(c.Id);
                    settingRepository.Add(setting);
                }
            }
            //Setting does not exist. Just add new
            else
            {
                settingRepository.Add(setting);
            }

            var response = Request.CreateResponse<Setting>(HttpStatusCode.Created, setting);
            
            var uri = Url.Link("CustomerEntity", new { id = setting.Id });
            response.Headers.Location = new Uri(uri);

            return response;
        }

        //Post a setting for specific customer and override lower levels
        [Route("settings/customer/{customerid}/{key}/{overrideLower=true}")]
        public HttpResponseMessage PostCustomerEntitySettingOverride(Setting setting, int customerid, string key)
        {
            //Customer level
            setting.Level = CUSTOMER;                      
            setting.LastModifiedById = customerid;
            setting.LastModifiedTimeStamp = DateTime.UtcNow;

            //Override customer level
            //Get all settings for this customer and this customer's lower levels which match this key
            var SettingList = settingRepository.GetAll()
              .Where(c => c.SettingKey == key && c.CustomerId == customerid && c.Level == CUSTOMER).ToList();
           
            //Override customer level
            if (SettingList.Count() > 0)
            {
                foreach (var c in SettingList)
                {
                    if (!settingRepository.SaveOrUpdate(setting))
                    {                        
                        settingRepository.Add(setting);
                    }
                }
            }

            //override user level           
            SettingList = settingRepository.GetAll()
              .Where(c => c.SettingKey == key && c.CustomerId == customerid && c.Level == USER).ToList();

            //we could do this all in one for loop, but maybe we want to keep some of the

            //existing table values? like created time stamp? etc etc 
            //Override user level and add entry
            if (SettingList.Count() > 0)
            {
                foreach (var c in SettingList)
                {
                    setting.DeviceId = c.DeviceId;
                    setting.UserName = c.UserName;                    

                    if (!settingRepository.SaveOrUpdate(setting))
                    {                        
                        settingRepository.Add(setting);
                    }
                }
            }

            //override device level           
            SettingList = settingRepository.GetAll()
              .Where(c => c.SettingKey == key && c.CustomerId == customerid && c.Level == DEVICE).ToList();

            //Override device level and add entry
            if (SettingList.Count() > 0)
            {
                foreach (var c in SettingList)
                {
                    setting.DeviceId = c.DeviceId;                    

                    if (!settingRepository.SaveOrUpdate(setting))
                    {
                        settingRepository.Add(setting);
                    }
                }
            }

            var response = Request.CreateResponse<Setting>(HttpStatusCode.Created, setting);

            var uri = Url.Link("CustomerEntityOverride", new { id = setting.Id });
            response.Headers.Location = new Uri(uri);

            return response;
        }

        //
        //POST Device Level
        //

        //Post device settings for a list of specified device ids
        //settings/device/{customerid}/{key}?deviceids=1,2,50, etc
        [Route("settings/device/{customerid}/{key}")]
        public HttpResponseMessage PostDeviceSetting(Setting setting, int customerid, string key, string deviceids)
        {
            setting.Level = DEVICE;
            setting.CustomerId = customerid;
            setting.SettingKey = key;           

            //separate device id string into individual strings
            var separated = deviceids.Split(',');
            int dID;

            //Post setting for each device
            foreach (var c in separated)
            {
                Int32.TryParse(c, out dID);
                setting.DeviceId = dID;
                settingRepository.Add(setting);
            }

            var response = Request.CreateResponse<Setting>(HttpStatusCode.Created, setting);

            var uri = Url.Link("DeviceOverride", new { id = setting.Id });
            response.Headers.Location = new Uri(uri);

            return response;
        }       

        //Post a device setting for specific device for one customer
        [Route("settings/device/{customerid}/{deviceid}/{key}")]
        public HttpResponseMessage PostDeviceEntitySetting(Setting setting, int customerid, int deviceid, string key)
        {            
            setting.DeviceId = deviceid;                      
            setting.LastModifiedById = customerid;
            setting.LastModifiedTimeStamp = DateTime.UtcNow;

            setting = settingRepository.Add(setting);

            var response = Request.CreateResponse<Setting>(HttpStatusCode.Created, setting);

            var uri = Url.Link("DeviceEntity", new { id = setting.Id });
            response.Headers.Location = new Uri(uri);

            return response;
        }
        
        //
        //POST User Level
        //

        //Post a user setting 
        [Route("settings/user/{customerid}/{username}/{key}")]
        public HttpResponseMessage PostUserSetting(Setting setting, string customerid, string username, string key)
        {
            setting.Level = USER;
            setting.SettingKey = key;
            setting.LastModifiedBy = username;
            setting.LastModifiedTimeStamp = DateTime.UtcNow;
            setting = settingRepository.Add(setting);

            var response = Request.CreateResponse<Setting>(HttpStatusCode.Created, setting);

            var uri = Url.Link("User", new { id = setting.Id });
            response.Headers.Location = new Uri(uri);

            return response;
        }

        //Post a user setting for specific user and override lower
        [Route("settings/user/{customerid}/{username}/{key}")]
        public HttpResponseMessage PostUserEntitySetting(Setting setting, int customerid, string username, string key)
        {
            setting.Level = USER;
            setting.SettingKey = key;
            setting.UserName = username;
            setting.LastModifiedBy = username;
            setting.LastModifiedTimeStamp = DateTime.UtcNow;
            setting = settingRepository.Add(setting);

            //override setting for all devices used by this user           
            var SettingList = settingRepository.GetAll()
              .Where(c => c.SettingKey == key && c.CustomerId == customerid && (c.Level == USER || c.Level == DEVICE) && c.UserName == username).ToList();
            
            //Override user level and device level 
            if (SettingList.Count() > 0)
            {
                foreach (var c in SettingList)
                {
                    setting.DeviceId = c.DeviceId;

                    if (!settingRepository.SaveOrUpdate(setting))
                    {
                        settingRepository.Add(setting);
                    }
                }
            }

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
            if (!settingRepository.SaveOrUpdate(currentSetting))
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
        [Route("settings/global/{key}/{overrideLower=true}")]
        public HttpResponseMessage PutGlobalSettingOverride(string key, Setting setting, bool overrideLower)
        {
            //get all global settings with same key
            var SettingsList = settingRepository.GetAll().Where(x => x.SettingKey == key).ToList();

            if (SettingsList == null)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }

            if (!overrideLower) //if overrideLower is false, update all settings in list of current setting
            {
                for (int i = 0; i < SettingsList.Count(); i++)
                {
                    SettingsList[i].SettingKey = setting.SettingKey;
                    SettingsList[i].CustomerId = setting.CustomerId;
                    SettingsList[i].DeviceId = setting.DeviceId;
                    SettingsList[i].UserName = setting.UserName;
                    SettingsList[i].StartEffectiveDate = setting.StartEffectiveDate;
                    SettingsList[i].EndEffectiveDate = setting.EndEffectiveDate;
                    SettingsList[i].CreatedTimeStamp = setting.CreatedTimeStamp;
                    SettingsList[i].LastModifiedTimeStamp = setting.LastModifiedTimeStamp;
                    SettingsList[i].LastModifiedBy = setting.LastModifiedBy;
                    SettingsList[i].LastModifiedById = setting.LastModifiedById;
                    SettingsList[i].Value = setting.Value;
                }
            }
           /* else //if overrideLower is true, override device customer, and user settings associated with the specified key
            {
                
            }*/

            //update settings
            for (int j = 0; j < SettingsList.Count(); j++)
            {
                if (!settingRepository.SaveOrUpdate(SettingsList[j]))
                {
                    throw new HttpResponseException(HttpStatusCode.NotFound);
                }
            }
            var response = Request.CreateResponse<Setting>(HttpStatusCode.Created, setting);
            return response;
        }

        //Put setting - Specific Customer (global)
        [Route("settings/global/{customerId}/{key}/{overrideLower}")]
        public HttpResponseMessage PutGlobalEntitySetting(string customerId, string key, Setting setting, bool overrideLower)
        {            
            //Using first item in list because only one item should return from this query
            var currentSetting = settingRepository.GetAll().Where(c => c.CustomerId == customerId && c.SettingKey == key && c.Level == CUSTOMER).ToList()[0];

            //If entry doesn't exist, create one
            if (currentSetting != null)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }
            else
            {

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
            else //if overrideLower is true, override lower levels
            {
                //we could do this all in one for loop, but maybe we want to keep some of the
                //existing table values? like created time stamp? etc etc 
                var SettingList = settingRepository.GetAll().Where(c => c.CustomerId == customerId && c.SettingKey == key && c.Level == USER).ToList();

                //Override user level
                if (SettingList.Count() > 0)
                {
                    foreach (var c in SettingList)
                    {
                        if (!settingRepository.Update(setting))
                        {
                            settingRepository.Add(setting);
                        }
                    }
                }

                SettingList = settingRepository.GetAll().Where(c => c.CustomerId == customerId && c.SettingKey == key && c.Level == DEVICE).ToList();

                //Override device level
                if (SettingList.Count() > 0)
                {
                    foreach (var c in SettingList)
                    {
                        if (!settingRepository.Update(setting))
                        {
                            settingRepository.Add(setting);
                        }
                    }
                }
            }           


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
        [Route("settings/customer/{customerid}/{key}")]
        public void PutCustomerSetting(int id, Setting setting)
        {
            setting.Id = id;

            if (!settingRepository.Update(setting))
                throw new HttpResponseException(HttpStatusCode.NotFound);
        }

        //Put setting (customer) and override lower
        [Route("settings/customer/{key}/{customerid}/{overrideLower=true}")]
        public void PutCustomerSettingOverride(int id, Setting setting)
        {
            setting.Id = id;

            if (!settingRepository.Update(setting))
                throw new HttpResponseException(HttpStatusCode.NotFound);
        }

        //Put setting - Specific Entity (customer)
        [Route("settings/customer/{customerid}/{key}")]
        public void PutCustomerEntitySetting(int id, Setting setting)
        {
            setting.Id = id;

            if (!settingRepository.Update(setting))
                throw new HttpResponseException(HttpStatusCode.NotFound);
        }                

        //Put setting - Specific Entity (customer) and override lower
        [Route("settings/customer/{customerid}/{key}/{overrideLower=true}")]
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
        [Route("settings/global/{key}")]
        public HttpResponseMessage DeleteGlobalSetting(String key)
        {
            var setting = settingRepository.GetGlobalSetting(key);
            if (setting == null)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }
            
            settingRepository.Delete(setting.Id);
            var response = Request.CreateResponse<Setting>(HttpStatusCode.OK, setting);

            return response;
        }

        //Delete a setting at global level and delete all lower levels also
        [Route("settings/global/{key}/{overrideLower=true}")]
        public void DeleteGlobalSettingOverride(string key)
        {
            var setting = settingRepository.GetGlobalSetting(key);

            if (setting == null)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }
            settingRepository.Delete(setting.Id);

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

        //Delete a setting at customer level for list of devices
        //settings/customer/{customerid}/{key}?username=jody,victor,etc
        [Route("settings/customer/{customerid}/{key}")]
        public HttpResponseMessage DeleteCustomerSetting(string key, string deviceids)
        {
            //Get list of existing settings which match the incoming one, if any, and have the specified deviceids
            var SettingList = settingRepository.GetAll()
               .Where(c => c.SettingKey == key && c.Level == CUSTOMER && c.DeviceId == deviceids).ToList();

            Setting setting = null;

            if (SettingList == null)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }

            if (SettingList.Count() > 0)
            {
                foreach (var c in SettingList)
                {
                    setting = c;
                    settingRepository.Delete(c.Id);                   
                }
            }           

            //perhaps we can change this to a long list reponse later?
            var response = Request.CreateResponse<Setting>(HttpStatusCode.OK, setting);          

            return response;
        }

        //Delete a setting at customer level for list of users
        [Route("settings/customer/{customerid}/{key}")]
        public HttpResponseMessage DeleteUserSettings(string key, int customerid, string usernames)
        {
            var separated = usernames.Split(',');
            Setting temp = null;

            if (separated.Count() > 0)
            {
                foreach (var user in separated)
                {
                    if ((temp = settingRepository.GetUserSetting(customerid, key, user)) == null)
                    {
                        continue;
                    }
                    else
                    {
                        settingRepository.Delete(temp.Id);
                    }                                  
                }
            }            
            
            var response = Request.CreateResponse<Setting>(HttpStatusCode.OK, setting);

            settingRepository.Delete(temp.Id);

            return response;
        }

        //Delete a setting at customer level and override all lower levels also
        [Route("settings/customer/{key}/{overrideLower=true}")]
        public HttpResponseMessage DeleteCustomerSettingOverride(String key)
        {
            var setting = settingRepository.GetCustomerSetting(key);

            if (setting == null)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }
            settingRepository.Delete(setting.Id);

            //subject to change, override lower
            var SettingList = settingRepository.GetAll()
                .Where(c => c.CustomerId == setting.CustomerId && c.SettingKey == key).ToList();
            if (SettingList.Count!=0)
            {
                foreach (var c in SettingList)
                {
                    settingRepository.Delete(c.Id);
                }
            }
            var response = Request.CreateResponse<Setting>(HttpStatusCode.OK, setting);

            return response;
        }

        //Delete a setting at customer level for specific user
        [Route("settings/customer/{customerid}/{username}/{key}")]
        public HttpResponseMessage DeleteCustomerEntitySetting(string customerid, string UserName, string key)
        {
            var setting = settingRepository.GetUserSetting(customerid, key, UserName);

            if (setting == null)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }
            settingRepository.Delete(setting.Id);
            var response = Request.CreateResponse<Setting>(HttpStatusCode.OK, setting);

            return response;
        }

        //Delete a setting at customer level for specific device
        [Route("settings/customer/{customerid}/{deviceid}/{key}")]
        public HttpResponseMessage DeleteCustomerEntitySetting(string customerid, string key, int deviceId)
        {
            var setting = settingRepository.GetDeviceSetting(customerid, key, deviceId);

            if (setting == null)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }
            settingRepository.Delete(setting.Id);
            var response = Request.CreateResponse<Setting>(HttpStatusCode.OK, setting);

            return response;
        }

        //Delete a setting at customer level for specific entity id and override all lower levels also
        [Route("settings/customer/{entityId}/{key}/{overrideLower=true}")]
        public HttpResponseMessage DeleteCustomerEntitySettingOverride(int entityId,string key)
        {
            var setting = settingRepository.GetCustomerSetting(entityId,key);

            if (setting == null)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }
            settingRepository.Delete(setting.Id);
            //subject to change, override lower
            var SettingList = settingRepository.GetAll()
                .Where(c => c.CustomerId == setting.CustomerId && c.SettingKey == key).ToList();
            if (SettingList.Count != 0)
            {
                foreach (var c in SettingList)
                {
                    settingRepository.Delete(c.Id);
                }
            }
            var response = Request.CreateResponse<Setting>(HttpStatusCode.OK, setting);

            return response;
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
        [Route("settings/device/{CustomerId}/{deviceId}/{entityId}/{key}")]
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
        [Route("settings/user/{CustomerId}/{userId}")]
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
        [Route("settings/user/{CustomerId}/{deviceId}/{entityId}/{key}")]
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