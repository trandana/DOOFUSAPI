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
        public const string EXISTING_ENTRY = "The setting already exists!";

        //GET a global setting
        //**
        [Route("settings/global")]
        public GetResponse GetGlobalSettingData()
        {
            var response = new GetResponse();
            response.Level = GLOBAL;
            response.Settings = settingRepository.GetGlobalSetting();
            return response;
        }

        //Get all settings for specific customer
        //**
        [Route("settings/customer/{customerId}")]
        public GetResponse GetCustomerSettingData(int customerId)
        {
            var response = new GetResponse();
            response.Level = CUSTOMER;
            response.Settings = settingRepository.GetCustomerSettings(customerId);
            return response;
        }

        //Get all settings for a specific device
        [Route("settings/device/{customerId}/{deviceId}")]
        public GetResponse GetDeviceSettingData(int customerId, int deviceId)
        {
            var response = new GetResponse();
            response.Level = DEVICE;
            response.Settings = settingRepository.GetDeviceSettings(customerId, deviceId);
            return response;
        }

        //Get all settigns for a specific username
        [Route("settings/user/{customerId}/{userName}")]
        public GetResponse GetUserSettingData(int customerId, string userName)
        {
            var response = new GetResponse();
            response.Level = USER;
            response.Settings = settingRepository.GetUserSettings(customerId, userName);
            return response;
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
        //**
        [Route("settings/global/{key}")]
        public HttpResponseMessage PostGlobalSetting(Setting setting)
        {
            setting.Level = GLOBAL;
            setting.LastModifiedBy = GLOBAL;
            setting.LastModifiedTimeStamp = DateTime.UtcNow;
            setting.CreatedTimeStamp = DateTime.UtcNow;

            if (!settingRepository.DoesSettingExist(setting))
            {
                setting = settingRepository.Add(setting);

                var response = Request.CreateResponse<Setting>(HttpStatusCode.Created, setting);

                var uri = Url.Link("Global", new { id = setting.Id });
               // response.Headers.Location = new Uri(uri);

                return response;
            }
            else
            {
                var uri = Url.Link("Global", new { id = setting.Id });
                var response = Request.CreateResponse<Setting>(HttpStatusCode.PreconditionFailed, setting);
                response.Content = new StringContent(EXISTING_ENTRY);
                //response.Headers.Location = new Uri(uri);

                return response;                
            }

        }

        //Post a global setting with option to override lower levels
        [Route("settings/global/{key}/{overrideLower?}")]
        public HttpResponseMessage PostGlobalSettingOverride(Setting setting, string key, bool overrideLower = false)
        {
            if (!settingRepository.DoesSettingExist(setting)) //check if setting already exists
            {
                setting.Level = GLOBAL;
                setting.LastModifiedBy = GLOBAL;
                setting.LastModifiedTimeStamp = DateTime.UtcNow;
                setting.CreatedTimeStamp = DateTime.UtcNow;

                //Get list of existing settings which match the incoming one, if any
                var SettingList = settingRepository.GetAll()
                    .Where(c => c.SettingKey == setting.SettingKey).ToList();

                if (SettingList.Count() < 1) //no existing setting found, just add new
                {
                    settingRepository.Add(setting);
                }

                if(overrideLower)  //existing setting(s) found, lets override them and add the new one
                {
                    foreach (var c in SettingList)
                    {
                        setting.Id = c.Id;
                        setting.Level = c.Level;
                        settingRepository.SaveOrUpdate(setting);
                    }
                }               

                var response = Request.CreateResponse<Setting>(HttpStatusCode.Created, setting);

                var uri = Url.Link("GlobalOverride", new { id = setting.Id });
                response.Headers.Location = new Uri(uri);

                return response;
            }
            else //setting already exists, POST failed
            {
                var uri = Url.Link("Global", new { id = setting.Id });
                var response = Request.CreateResponse<Setting>(HttpStatusCode.PreconditionFailed, setting);
                response.Content = new StringContent(EXISTING_ENTRY);
                response.Headers.Location = new Uri(uri);

                return response;
            }
        }

        
        //Post a setting (global) to multiple customers - with override
        //settings/global/{key}?customerids=1,2,50, etc
        [Route("settings/global/{key}/{customerids}/{overrideLower?}")]
        public HttpResponseMessage PostGlobalEntitySetting(Setting setting, string key, string customerids, bool overrideLower = false)
        {
            if (!settingRepository.DoesSettingExist(setting)) //check if setting already exists
            {
                setting.Level = GLOBAL;
                setting.LastModifiedBy = GLOBAL;
                setting.LastModifiedTimeStamp = DateTime.UtcNow;
                setting.CreatedTimeStamp = DateTime.UtcNow;

                //separate customer id string into individual ints
                string[] separated = customerids.Split(',');
                int parsed = 0;

                //override was specified so override lower levels
                if (overrideLower)
                {
                    for (int i = 0; i < separated.Count(); i++)
                    {
                        Int32.TryParse(separated.ElementAt(i), out parsed);

                        //Get list of existing settings for customer and lower levels which match the incoming one, if any
                        var SettingList = settingRepository.GetAll()
                            .Where(c => c.SettingKey == key && c.CustomerId == parsed).ToList();

                        foreach (var c in SettingList)
                        {
                            setting.Level = c.Level;
                            setting.Id = c.Id;                            
                            //add new entries                            
                            settingRepository.SaveOrUpdate(setting);
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

                        //wont override existing setting if it already exists
                        if (!settingRepository.DoesSettingExist(setting))
                        {
                            settingRepository.Add(setting);
                        }                        
                    }
                }

                var response = Request.CreateResponse<Setting>(HttpStatusCode.Created, setting);

                var uri = Url.Link("GlobalEntity", new { id = setting.Id });
                response.Headers.Location = new Uri(uri);

                return response;
            }
            else
            {
                var uri = Url.Link("Global", new { id = setting.Id });
                var response = Request.CreateResponse<Setting>(HttpStatusCode.PreconditionFailed, setting);
                response.Content = new StringContent(EXISTING_ENTRY);
                response.Headers.Location = new Uri(uri);

                return response;
            }
        }

        //Post a global setting - Specific customer id with option to override lower levels
        [Route("settings/global/{customerid}/{key}/{overrideLower?}")]
        public HttpResponseMessage PostGlobalEntitySettingOverride(Setting setting, int customerid, string key, bool overrideLower = false)
        {
            if (!settingRepository.DoesSettingExist(setting)) //check if setting already exists
            {
                setting.Level = GLOBAL;
                setting.LastModifiedBy = GLOBAL;
                setting.LastModifiedTimeStamp = DateTime.UtcNow;
                setting.CreatedTimeStamp = DateTime.UtcNow;

                //Get list of existing settings which match the incoming one, if any
                var SettingList = settingRepository.GetAll()
                   .Where(c => c.SettingKey == key && c.CustomerId == customerid).ToList();

                if(overrideLower)
                {
                    if (SettingList.Count() > 0)
                    {
                        foreach (var c in SettingList)
                        {
                            setting.Level = c.Level;
                            setting.Id = c.Id;
                            settingRepository.Update(setting);
                        }
                    }
                }
                else //no override, just add
                {                    
                    settingRepository.Add(setting);                    
                }
                

                var response = Request.CreateResponse<Setting>(HttpStatusCode.Created, setting);

                var uri = Url.Link("GlobalEntityOverride", new { id = setting.Id });
                response.Headers.Location = new Uri(uri);

                return response;
            }
            else
            {
                var uri = Url.Link("Global", new { id = setting.Id });
                var response = Request.CreateResponse<Setting>(HttpStatusCode.PreconditionFailed, setting);
                response.Content = new StringContent(EXISTING_ENTRY);
                response.Headers.Location = new Uri(uri);

                return response;
            }
        }

        //
        //POST Customer Level
        //

        //WARNING: This doesn't appear correct. We want customer level access to be able to create settings 
        //for all customers?? Thats what it says in the spec, but must be a mistake

        //Post a setting for a all customers, do not override lower levels
       /* [Route("settings/customer/{customerid}/{key}")]
        public HttpResponseMessage PostCustomerSetting(Setting setting, string key)
        {
            if (!settingRepository.DoesSettingExist(setting)) //check if setting already exists
            {
                setting.Level = CUSTOMER;
                setting.LastModifiedBy = CUSTOMER;
                setting.LastModifiedTimeStamp = DateTime.UtcNow;
                setting.CreatedTimeStamp = DateTime.UtcNow;

                //Get all setting matching this key at customer level
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

                var response = Request.CreateResponse<Setting>(HttpStatusCode.Created, setting);

                var uri = Url.Link("Customer", new { id = setting.Id });
                response.Headers.Location = new Uri(uri);

                return response;
            }
            else
            {
                var uri = Url.Link("Customer", new { id = setting.Id });
                var response = Request.CreateResponse<Setting>(HttpStatusCode.PreconditionFailed, setting);
                response.Content = new StringContent(EXISTING_ENTRY);
                response.Headers.Location = new Uri(uri);

                return response;
            }
        } */

        //Post a setting for each specified user at a specific customer and override each customer's lower levels
       [Route("settings/customer/{customerid}/{key}/{usernames}/{overrideLower?}")]
        public HttpResponseMessage PostCustomerSettingOverride(Setting setting, int customerid, string key, string usernames, bool overrideLower = false)
        {
            //Customer level override
            setting.Level = CUSTOMER;
            setting.CustomerId = customerid;
            setting.LastModifiedById = customerid;
            setting.LastModifiedTimeStamp = DateTime.UtcNow;
            setting.CreatedTimeStamp = DateTime.UtcNow;

            if (!settingRepository.DoesSettingExist(setting)) //check if setting already exists. 
            {
                settingRepository.Add(setting); //add the customer level first
                string[] separated = usernames.Split(',');

                //override was specified, so we override this setting for each user and for each device in the lower levels
                if (overrideLower)
                {
                    setting.Level = USER;

                    //add or override this setting for each user specified in the CSV list
                    for (int i = 0; i < separated.Count(); i++)
                    {
                        var ExistingSetting = settingRepository.GetUserSetting(customerid, key, separated.ElementAt(i));

                        setting.DeviceId = ExistingSetting.DeviceId;
                        setting.UserName = ExistingSetting.UserName;
                        settingRepository.SaveOrUpdate(setting);
                    }

                    //User level overwritten, now lets overwrite device level
                    var SettingList = settingRepository.GetAll()
                     .Where(c => c.SettingKey == key && c.Level == DEVICE && c.CustomerId == customerid).ToList();

                    setting.Level = DEVICE;

                    foreach (var c in SettingList)
                    {
                        setting.DeviceId = c.DeviceId;
                        settingRepository.SaveOrUpdate(setting);
                    }
                }
                 //override wasn't specified, we are done here                
                var response = Request.CreateResponse<Setting>(HttpStatusCode.Created, setting);

                var uri = Url.Link("CustomerOverride", new { id = setting.Id });
                response.Headers.Location = new Uri(uri);

                return response;                
            }
            else //The setting already exists at customer level 
            {
                var uri = Url.Link("Customer", new { id = setting.Id });
                var response = Request.CreateResponse<Setting>(HttpStatusCode.PreconditionFailed, setting);
                response.Content = new StringContent(EXISTING_ENTRY);
                response.Headers.Location = new Uri(uri);

                return response;
            }
        }

        //Post a setting for a specific customer 
        //**
        [Route("settings/customer/{customerid}/{key}")]
        public HttpResponseMessage PostCustomerEntitySetting(Setting setting, int customerid, string key)
        {
            //Customer level
            setting.Level = CUSTOMER;
            setting.CustomerId = customerid;
            setting.LastModifiedById = customerid;
            setting.LastModifiedTimeStamp = DateTime.UtcNow;
            setting.CreatedTimeStamp = DateTime.UtcNow;

            if (!settingRepository.DoesSettingExist(setting)) //check if setting already exists
            {
                settingRepository.Add(setting);                  

                var response = Request.CreateResponse<Setting>(HttpStatusCode.Created, setting);

                var uri = Url.Link("CustomerEntity", new { id = setting.Id });
               // response.Headers.Location = new Uri(uri);

                return response;
            }
            else //setting already exists, use PUT instead
            {
                var uri = Url.Link("Customer", new { id = setting.Id });
                var response = Request.CreateResponse<Setting>(HttpStatusCode.PreconditionFailed, setting);
                response.Content = new StringContent(EXISTING_ENTRY);
               // response.Headers.Location = new Uri(uri);

                return response;
            }
        }

        //Post a setting for specific customer and override lower levels
        [Route("settings/customer/{customerid}/{key}/{overrideLower?}")]
        public HttpResponseMessage PostCustomerEntitySettingOverride(Setting setting, int customerid, string key, bool overrideLower = false)
        {
            if (!settingRepository.DoesSettingExist(setting)) //check if setting already exists
            {
                //Customer level
                setting.Level = CUSTOMER;
                setting.LastModifiedById = customerid;
                setting.LastModifiedTimeStamp = DateTime.UtcNow;
                setting.CreatedTimeStamp = DateTime.UtcNow;

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
            else
            {
                var uri = Url.Link("Customer", new { id = setting.Id });
                var response = Request.CreateResponse<Setting>(HttpStatusCode.PreconditionFailed, setting);
                response.Content = new StringContent(EXISTING_ENTRY);
                response.Headers.Location = new Uri(uri);

                return response;
            }
        }

        //
        //POST Device Level
        //

        //Post device settings for a list of specified device ids
        //settings/device/{customerid}/{key}?deviceids=1,2,50, etc
        [Route("settings/device/{customerid}/{key}")]
        public HttpResponseMessage PostDeviceSetting(Setting setting, int customerid, string key, string deviceids)
        {
            if (!settingRepository.DoesSettingExist(setting)) //check if setting already exists
            {
                setting.Level = DEVICE;
                setting.CustomerId = customerid;
                setting.SettingKey = key;
                setting.CreatedTimeStamp = DateTime.UtcNow;

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
            else
            {
                var uri = Url.Link("Device", new { id = setting.Id });
                var response = Request.CreateResponse<Setting>(HttpStatusCode.PreconditionFailed, setting);
                response.Content = new StringContent(EXISTING_ENTRY);
                response.Headers.Location = new Uri(uri);

                return response;
            }
        }       

        //Post a device setting for specific device for one customer
        [Route("settings/device/{customerid}/{deviceid}/{key}")]
        public HttpResponseMessage PostDeviceEntitySetting(Setting setting, int customerid, int deviceid, string key)
        {
            if (!settingRepository.DoesSettingExist(setting)) //check if setting already exists
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
            else
            {
                var uri = Url.Link("Device", new { id = setting.Id });
                var response = Request.CreateResponse<Setting>(HttpStatusCode.PreconditionFailed, setting);
                response.Content = new StringContent(EXISTING_ENTRY);
                response.Headers.Location = new Uri(uri);

                return response;
            }
        }
        
        //
        //POST User Level
        //

        //Post a user setting 
        [Route("settings/user/{customerid}/{username}/{key}")]
        public HttpResponseMessage PostUserSetting(Setting setting, string customerid, string username, string key)
        {            
            if (!settingRepository.DoesSettingExist(setting)) //check if setting already exists
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
            else
            {
                var uri = Url.Link("User", new { id = setting.Id });
                var response = Request.CreateResponse<Setting>(HttpStatusCode.PreconditionFailed, setting);
                response.Content = new StringContent(EXISTING_ENTRY);
                response.Headers.Location = new Uri(uri);

                return response;
            }
        }

        //Post a user setting for specific user and override lower
        [Route("settings/user/{customerid}/{username}/{key}")]
        public HttpResponseMessage PostUserEntitySetting(Setting setting, int customerid, string username, string key)
        {
            if (!settingRepository.DoesSettingExist(setting)) //check if setting already exists
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
            else
            {
                var uri = Url.Link("User", new { id = setting.Id });
                var response = Request.CreateResponse<Setting>(HttpStatusCode.PreconditionFailed, setting);
                response.Content = new StringContent(EXISTING_ENTRY);
                response.Headers.Location = new Uri(uri);

                return response;
            }
        }


        //
        // PUT
        //

        //
        //PUT Global Level
        //

        //Put setting (global) by id
        [Route("settings/global/{id}")]
        public HttpResponseMessage PutGlobalSetting(int id, Setting setting)
        {
            //get setting to update by id
            var currentSetting = settingRepository.Get(id);

            //if setting does not exist, add setting to DB
            if (currentSetting == null && !settingRepository.DoesSettingExist(setting))
            {
                setting.Level = GLOBAL;
                setting.CreatedTimeStamp = DateTime.UtcNow;
                settingRepository.Add(setting);
                var createdResponse = Request.CreateResponse<Setting>(HttpStatusCode.Created, setting);
                return createdResponse;
            }

            //replace current setting values with new setting values
            currentSetting.StartEffectiveDate = setting.StartEffectiveDate;
            currentSetting.EndEffectiveDate = null;
            currentSetting.LastModifiedTimeStamp = DateTime.UtcNow;
            currentSetting.LastModifiedBy = setting.LastModifiedBy;
            currentSetting.LastModifiedById = setting.LastModifiedById;
            currentSetting.Value = setting.Value;

            //update setting
            if (!settingRepository.Update(currentSetting))
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }

            var updatedResponse = Request.CreateResponse<Setting>(HttpStatusCode.Created, currentSetting);
            return updatedResponse;

        }

        //Put global with option to override
        //settings/global/{key}/{overrideLower?}?customerIds="1,4,7,etc."
        [Route("settings/global/{key}/{overrideLower?}")]
        public HttpResponseMessage PutGlobalSettingOverride(string key, Setting setting, string customerIds, bool overrideLower = false)
        {
            if (!overrideLower)
            {
                var currentSetting = settingRepository.GetAll().Where(x => x.SettingKey == key && x.Level == GLOBAL).ToList().First<Setting>();

                //if setting does not exist, add setting to DB
                if (currentSetting == null && !settingRepository.DoesSettingExist(setting))
                {
                    setting.SettingKey = key;
                    setting.Level = GLOBAL;
                    setting.CreatedTimeStamp = DateTime.UtcNow;
                    settingRepository.Add(setting);
                    var createdResponse = Request.CreateResponse<Setting>(HttpStatusCode.Created, setting);
                    return createdResponse;
                }

                //replace current setting values with new values
                currentSetting.StartEffectiveDate = setting.StartEffectiveDate;
                currentSetting.EndEffectiveDate = null;
                currentSetting.LastModifiedTimeStamp = DateTime.UtcNow;
                currentSetting.LastModifiedBy = setting.LastModifiedBy;
                currentSetting.LastModifiedById = setting.LastModifiedById;
                currentSetting.Value = setting.Value;

                //try updating setting
                if (!settingRepository.Update(currentSetting))
                {
                    throw new HttpResponseException(HttpStatusCode.NotFound);
                }

            }
            else
            {
                //get list of all settings with specified key
                var listOfCurrentSettings = settingRepository.GetAll()
                    .Where(x => x.SettingKey == key &&
                    x.Level == GLOBAL || x.Level == CUSTOMER || x.Level == DEVICE || x.Level == USER).ToList();

                /*//if settings were not found, create them?
                if (listOfCurrentSettings.Count == 0)
                {

                }*/

                //update values of setting objects in list
                for (int i = 0; i < listOfCurrentSettings.Count(); i++)
                {
                    listOfCurrentSettings[i].StartEffectiveDate = setting.StartEffectiveDate;
                    listOfCurrentSettings[i].EndEffectiveDate = null;
                    listOfCurrentSettings[i].LastModifiedTimeStamp = DateTime.UtcNow;
                    listOfCurrentSettings[i].LastModifiedBy = setting.LastModifiedBy;
                    listOfCurrentSettings[i].LastModifiedById = setting.LastModifiedById;
                    listOfCurrentSettings[i].Value = setting.Value;
                }

                //update settings
                for (int j = 0; j < listOfCurrentSettings.Count(); j++)
                {
                    if (!settingRepository.Update(listOfCurrentSettings[j]))
                    {
                        throw new HttpResponseException(HttpStatusCode.NotFound);
                    }
                }
            }

            //Create HTTP response
            var updatedResponse = Request.CreateResponse<Setting>(HttpStatusCode.Created, setting);
            return updatedResponse;
        }

        //Put setting - Specific Entity (global)
        //**
        [Route("settings/global/{entityId}/{key}/{overrideLower?}")]
        public HttpResponseMessage PutGlobalEntitySetting(int entityId, string key, Setting setting, bool overrideLower = false)
        {
            if (!overrideLower) //if overrideLower is false, update specified setting with new setting values
            {
                var currentSetting = settingRepository.GetAll().Where(x => x.CustomerId == entityId 
                && x.SettingKey == key && x.Level == GLOBAL).ToList().First<Setting>();

                //if setting does not exist, add setting to DB
                if (currentSetting == null && !settingRepository.DoesSettingExist(setting))
                {
                    setting.Level = GLOBAL;
                    setting.CustomerId = entityId;
                    setting.CreatedTimeStamp = DateTime.UtcNow;
                    setting.SettingKey = key;
                    settingRepository.Add(setting);
                    var createdResponse = Request.CreateResponse<Setting>(HttpStatusCode.Created, setting);
                    return createdResponse;
                }

                //replace current setting values with new setting values
                currentSetting.StartEffectiveDate = setting.StartEffectiveDate;
                currentSetting.EndEffectiveDate = null;
                currentSetting.LastModifiedTimeStamp = DateTime.UtcNow;
                currentSetting.LastModifiedBy = setting.LastModifiedBy;
                currentSetting.LastModifiedById = setting.LastModifiedById;
                currentSetting.Value = setting.Value;

                //update setting
                if (!settingRepository.Update(currentSetting))
                {
                    throw new HttpResponseException(HttpStatusCode.NotFound);
                }
            }
            else //if overrideLower is true, override lower levels
            {
                //get list of all setting with specified key
                var listOfCurrentSettings = settingRepository.GetAll()
                    .Where(x => x.SettingKey == key && x.CustomerId == entityId &&
                    x.Level == GLOBAL || x.Level == CUSTOMER || x.Level == DEVICE || x.Level == USER).ToList();

                /*//if settings were not found, create them?
                if (listOfCurrentSettings.Count == 0)
                {

                }*/

                //update values of setting objects in list
                for (int i = 0; i < listOfCurrentSettings.Count(); i++)
                {
                    listOfCurrentSettings[i].StartEffectiveDate = setting.StartEffectiveDate;
                    listOfCurrentSettings[i].EndEffectiveDate = null;
                    listOfCurrentSettings[i].LastModifiedTimeStamp = DateTime.UtcNow;
                    listOfCurrentSettings[i].LastModifiedBy = setting.LastModifiedBy;
                    listOfCurrentSettings[i].LastModifiedById = setting.LastModifiedById;
                    listOfCurrentSettings[i].Value = setting.Value;
                }

                //update settings
                for (int j = 0; j < listOfCurrentSettings.Count(); j++)
                {
                    if (!settingRepository.Update(listOfCurrentSettings[j]))
                    {
                        throw new HttpResponseException(HttpStatusCode.NotFound);
                    }
                }
            }

            var updatedResponse = Request.CreateResponse<Setting>(HttpStatusCode.Created, setting);
            return updatedResponse;

        }

        //
        //PUT Customer Level
        //

        //Put setting (customer) with option to override lower
        //add csv attribute
        [Route("settings/customer/{key}/{overrideLower?}")]
        public HttpResponseMessage PutCustomerSettingOverride(string key, Setting setting, bool overrideLower = false)
        {
            if (!overrideLower)
            {
                var currentSetting = settingRepository.GetAll().Where(x => x.SettingKey == key && x.Level == CUSTOMER).ToList().First<Setting>();

                //if setting does not exist, add setting to DB
                if (currentSetting == null && !settingRepository.DoesSettingExist(setting))
                {
                    setting.SettingKey = key;
                    setting.Level = CUSTOMER;
                    setting.CreatedTimeStamp = DateTime.UtcNow;
                    settingRepository.Add(setting);
                    var createdResponse = Request.CreateResponse<Setting>(HttpStatusCode.Created, setting);
                    return createdResponse;
                }

                currentSetting.StartEffectiveDate = setting.StartEffectiveDate;
                currentSetting.EndEffectiveDate = null;
                currentSetting.LastModifiedTimeStamp = DateTime.UtcNow;
                currentSetting.LastModifiedBy = setting.LastModifiedBy;
                currentSetting.LastModifiedById = setting.LastModifiedById;
                currentSetting.Value = setting.Value;

                if (!settingRepository.Update(currentSetting))
                {
                    throw new HttpResponseException(HttpStatusCode.NotFound);
                }

            }
            else
            {
                //get list of all setting with specified key
                var listOfCurrentSettings = settingRepository.GetAll()
                    .Where(x => x.SettingKey == key &&
                    x.Level == CUSTOMER || x.Level == DEVICE || x.Level == USER).ToList();

                /*//if settings were not found, create them?
                if (listOfCurrentSettings.Count == 0)
                {

                }*/

                //update values of setting objects in list
                for (int i = 0; i < listOfCurrentSettings.Count(); i++)
                {
                    listOfCurrentSettings[i].StartEffectiveDate = setting.StartEffectiveDate;
                    listOfCurrentSettings[i].EndEffectiveDate = null;
                    listOfCurrentSettings[i].LastModifiedTimeStamp = DateTime.UtcNow;
                    listOfCurrentSettings[i].LastModifiedBy = setting.LastModifiedBy;
                    listOfCurrentSettings[i].LastModifiedById = setting.LastModifiedById;
                    listOfCurrentSettings[i].Value = setting.Value;
                }

                //update settings
                for (int j = 0; j < listOfCurrentSettings.Count(); j++)
                {
                    if (!settingRepository.Update(listOfCurrentSettings[j]))
                    {
                        throw new HttpResponseException(HttpStatusCode.NotFound);
                    }
                }

            }

            //Create HTTP response
            var response = Request.CreateResponse<Setting>(HttpStatusCode.Created, setting);
            return response;
        }

        //Put setting - Specific Entity (customer) and override lower
        //**
        [Route("settings/customer/{entityId}/{key}/{overrideLower?}")]
        public HttpResponseMessage PutCustomerEntitySettingOverride(int entityId, string key, Setting setting, bool overrideLower = false)
        {
            if (!overrideLower)
            {
                var currentSetting = settingRepository.GetAll().Where(x => x.CustomerId == entityId && x.SettingKey == key
                && x.Level == CUSTOMER).First<Setting>();

                //if setting does not exist, add setting to DB
                if (currentSetting == null && !settingRepository.DoesSettingExist(setting))
                {
                    setting.DeviceId = entityId;
                    setting.SettingKey = key;
                    setting.Level = CUSTOMER;
                    setting.CreatedTimeStamp = DateTime.UtcNow;
                    settingRepository.Add(setting);
                    var createdResponse = Request.CreateResponse<Setting>(HttpStatusCode.Created, setting);
                    return createdResponse;
                }

                currentSetting.StartEffectiveDate = setting.StartEffectiveDate;
                currentSetting.EndEffectiveDate = null;
                currentSetting.LastModifiedTimeStamp = DateTime.UtcNow;
                currentSetting.LastModifiedBy = setting.LastModifiedBy;
                currentSetting.LastModifiedById = setting.LastModifiedById;
                currentSetting.Value = setting.Value;

                if (!settingRepository.Update(currentSetting))
                {
                    throw new HttpResponseException(HttpStatusCode.NotFound);
                }
            }
            else
            {
                var currentSettingsList = settingRepository.GetAll().Where(x => x.DeviceId == entityId && x.SettingKey == key
                && x.Level == CUSTOMER || x.Level == DEVICE || x.Level == USER).ToList();

                /*//if settings were not found, create them?
                if (listOfCurrentSettings.Count == 0)
                {

                }*/

                for (int i = 0; i < currentSettingsList.Count; i++)
                {
                    currentSettingsList[i].StartEffectiveDate = setting.StartEffectiveDate;
                    currentSettingsList[i].EndEffectiveDate = null;
                    currentSettingsList[i].LastModifiedTimeStamp = DateTime.UtcNow;
                    currentSettingsList[i].LastModifiedBy = setting.LastModifiedBy;
                    currentSettingsList[i].LastModifiedById = setting.LastModifiedById;
                    currentSettingsList[i].Value = setting.Value;
                }

                //update settings
                for (int j = 0; j < currentSettingsList.Count(); j++)
                {
                    if (!settingRepository.Update(currentSettingsList[j]))
                    {
                        throw new HttpResponseException(HttpStatusCode.NotFound);
                    }
                }
            }

            var response = Request.CreateResponse<Setting>(HttpStatusCode.Created, setting);
            return response;
        }

        //
        //PUT Device Level      
        //

        //Put setting for device with option to override lower levels
        //settings/device/{customerId}/{key}?deviceIds = "111,234,678,etc."
        [Route("settings/device/{customerId}/{key}")]
        public HttpResponseMessage PutDeviceSettingOverride(int customerId, string key, string deviceIds, Setting setting)
        {   
            var currentSetting = settingRepository.GetAll().Where(x => x.SettingKey == key
            && x.CustomerId == customerId && x.Level == DEVICE).First<Setting>();

            //if setting does not exist, add setting to DB
            if (currentSetting == null && !settingRepository.DoesSettingExist(setting))
            {
                setting.CustomerId = customerId;
                setting.Level = DEVICE;
                setting.SettingKey = key;
                setting.CreatedTimeStamp = DateTime.UtcNow;
                settingRepository.Add(setting);
                var createdResponse = Request.CreateResponse<Setting>(HttpStatusCode.Created, setting);
                return createdResponse;
            }

            currentSetting.StartEffectiveDate = setting.StartEffectiveDate;
            currentSetting.EndEffectiveDate = null;
            currentSetting.LastModifiedTimeStamp = DateTime.UtcNow;
            currentSetting.LastModifiedBy = setting.LastModifiedBy;
            currentSetting.LastModifiedById = setting.LastModifiedById;
            currentSetting.Value = setting.Value;

            if (!settingRepository.Update(currentSetting))
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }

            //Create HTTP response
            var updatedResponse = Request.CreateResponse<Setting>(HttpStatusCode.Created, currentSetting);
            return updatedResponse;


        }

        //Put setting for device - Specific entity 
        [Route("settings/device/{customerId}/{entityId}/{key}")]
        public HttpResponseMessage PutDeviceEntitySettingOverride(int customerId, string entityId, string key, Setting setting, bool overrideLower = false)
        {
            var currentSetting = settingRepository.GetAll().Where(x => x.CustomerId == customerId
            && x.UserName == entityId && x.SettingKey == key && x.Level == DEVICE).First<Setting>();

            //if setting does not exist, add setting to DB
            if (currentSetting == null && !settingRepository.DoesSettingExist(setting))
            {
                setting.CustomerId = customerId;
                setting.Level = DEVICE;
                setting.UserName = entityId;
                setting.SettingKey = key;
                setting.CreatedTimeStamp = DateTime.UtcNow;
                settingRepository.Add(setting);
                var createdResponse = Request.CreateResponse<Setting>(HttpStatusCode.Created, setting);
                return createdResponse;
            }

            currentSetting.StartEffectiveDate = setting.StartEffectiveDate;
            currentSetting.EndEffectiveDate = null;
            currentSetting.LastModifiedTimeStamp = DateTime.UtcNow;
            currentSetting.LastModifiedBy = setting.LastModifiedBy;
            currentSetting.LastModifiedById = setting.LastModifiedById;
            currentSetting.Value = setting.Value;

            if (!settingRepository.Update(currentSetting))
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }

            //Create HTTP response
            var updatedResponse = Request.CreateResponse<Setting>(HttpStatusCode.Created, currentSetting);
            return updatedResponse;
        }


        //
        //PUT User Level
        //

        //Put setting for user 
        //settings/user/{customerId}/{key}?usernames = "jody,victor,phum,alex"
        [Route("settings/user/{customerId}/{key}")]
        public HttpResponseMessage PutUserSetting(int customerId, string key, string usernames, Setting setting)
        {
            var currentSetting = settingRepository.GetAll().Where(x => x.CustomerId == customerId 
            && x.SettingKey == key && x.Level == USER).First<Setting>();

            //if setting does not exist, add setting to DB
            if (currentSetting == null && !settingRepository.DoesSettingExist(setting))
            {
                setting.CustomerId = customerId;
                setting.Level = USER;
                setting.CreatedTimeStamp = DateTime.UtcNow;
                setting.SettingKey = key;
                settingRepository.Add(setting);
                var createdResponse = Request.CreateResponse<Setting>(HttpStatusCode.Created, setting);
                return createdResponse;
            }

            currentSetting.StartEffectiveDate = setting.StartEffectiveDate;
            currentSetting.EndEffectiveDate = null;
            currentSetting.LastModifiedTimeStamp = DateTime.UtcNow;
            currentSetting.LastModifiedBy = setting.LastModifiedBy;
            currentSetting.LastModifiedById = setting.LastModifiedById;
            currentSetting.Value = setting.Value;

            if (!settingRepository.Update(currentSetting))
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }

            //Create HTTP response
            var updatedResponse = Request.CreateResponse<Setting>(HttpStatusCode.Created, currentSetting);
            return updatedResponse;
        }

        //Put setting for user - Specific entity 
        [Route("settings/user/{customerId}/{entityId}/{key}")]
        public HttpResponseMessage PutUserEntitySetting(int customerId, string entityId, string key, Setting setting)
        {
            var currentSetting = settingRepository.GetAll().Where(x => x.CustomerId == customerId
            && x.UserName == entityId && x.SettingKey == key && x.Level == USER).First<Setting>();

            //if setting does not exist, add setting to DB
            if (currentSetting == null && !settingRepository.DoesSettingExist(setting))
            {
                setting.UserName = entityId;
                setting.CustomerId = customerId;
                setting.Level = USER;
                setting.SettingKey = key;
                settingRepository.Add(setting);
                var createdResponse = Request.CreateResponse<Setting>(HttpStatusCode.Created, setting);
                return createdResponse;
            }

            currentSetting.StartEffectiveDate = setting.StartEffectiveDate;
            currentSetting.EndEffectiveDate = null;
            currentSetting.LastModifiedTimeStamp = DateTime.UtcNow;
            currentSetting.LastModifiedBy = setting.LastModifiedBy;
            currentSetting.LastModifiedById = setting.LastModifiedById;
            currentSetting.Value = setting.Value;

            if (!settingRepository.Update(currentSetting))
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }

            //Create HTTP response
            var updatedResponse = Request.CreateResponse<Setting>(HttpStatusCode.Created, currentSetting);
            return updatedResponse;
        }


        //
        //DELETE
        //

        //
        //DELETE Global Level
        //

        //Delete a setting global level
        //**
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
        }//tested

        //Delete a setting at global level and delete all lower levels also

        [Route("settings/global/{key}/{overrideLower:bool}")]
        public HttpResponseMessage DeleteGlobalSettingOverride(string key,bool overrideLower)
        {
            if (!overrideLower)
            {
                var response = Request.CreateResponse(HttpStatusCode.Accepted, "Override lower is false");
                return response;
            }
            var setting = settingRepository.GetGlobalSetting(key);

            if (setting == null)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }
            settingRepository.Delete(setting.Id);

            var SettingsList = settingRepository.GetAll().Where(c=>c.SettingKey==key&&(c.Level==USER||c.Level==CUSTOMER||c.Level==DEVICE)).ToList();
            if (SettingsList.Count == 0)
            {
                var response = Request.CreateResponse(HttpStatusCode.Accepted, "No lower Level values found");
                return response;
            }

            foreach (var c in SettingsList)
            {
                settingRepository.Delete(c.Id);
            }

            var response2 = Request.CreateResponse(HttpStatusCode.Accepted, "Lower level deleted");
            return response2;

        }//tested

        

        //Delete a setting at global level for specific entity id
        //Assumed entityID as ID for each row in the table 
        [Route("settings/global/{key}")]
        public HttpResponseMessage DeleteGlobalEntitySetting( String key)
        {
            var setting = settingRepository.GetGlobalSetting( key);

            if (setting == null)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }
            settingRepository.Delete(setting.Id);

            var response = Request.CreateResponse<Setting>(HttpStatusCode.OK, setting);
            return response;
        }//tested

        //Delete a setting at global level for specific entity and override all lower levels also
        [Route("settings/global/{key}/{overrideLower=true}")]
        public HttpResponseMessage DeleteGlobalEntitySettingOverride( String key)
        {
            var setting = settingRepository.GetGlobalSetting( key);

            if (setting == null)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }
            settingRepository.Delete(setting.Id);
            
            var SettingList = settingRepository.GetAll()
                .Where(c => c.SettingKey==key&&(c.Level == CUSTOMER || c.Level == DEVICE || c.Level == USER)).ToList();
            if (SettingList.Count==0)
            {
                var response = Request.CreateResponse(HttpStatusCode.Accepted, "No lower Level values found");
                return response;
            }
            
            foreach (var c in SettingList)
            {
                    settingRepository.Delete(c.Id);
            }

            var response2 = Request.CreateResponse(HttpStatusCode.Accepted, "Job Done");
            return response2;

        }//tested

        //
        //DELETE Customer Level
        //

        //Delete a setting at customer level for list of devices
        //settings/customer/{customerid}/{key}?username=jody,victor,etc
        [Route("settings/customer/{customerid}/{key}/device/{deviceids}")]
        public HttpResponseMessage DeleteCustomerSetting(string key, int customerid, string deviceids)
        {
            

            //separate device id string into individual strings
            var separated = deviceids.Split(',');
            

            //Post setting for each device
            //Convert device id's from CSV list to integers, one at a time and delete their corresponding entries
            foreach (var i in separated)
            {
                Int32.TryParse(i, out int dID); 
                var SettingList = settingRepository.GetAll()
                    .Where(c => c.SettingKey == key && c.DeviceId == dID && c.CustomerId == customerid&&c.Level==DEVICE);

                if(settingRepository == null)
                {
                     throw new HttpResponseException(HttpStatusCode.NotFound);
                }
                else
                {
                    foreach (var set in SettingList)
                    {
                        settingRepository.Delete(set.Id);
                    }                   
                }                
            } 
            
            //perhaps we can change this to a long list reponse later?
            var response = Request.CreateResponse(HttpStatusCode.OK, "List of settings deleted");          

            return response;
        }//tested

        //Delete a setting at customer level for list of users
        [Route("settings/customer/{customerid}/{key}/user/{usernames}")]
        public HttpResponseMessage DeleteUserSettings(string key, int customerid, string usernames)
        {
            var separated = usernames.Split(',');
            Setting setting = null;

            if (separated.Count() > 0)
            {
                foreach (var user in separated)
                {
                    if ((setting = settingRepository.GetUserSetting(customerid, key, user)) == null||setting.Level!=USER)
                    {
                        continue;
                    }
                    else
                    {
                        settingRepository.Delete(setting.Id);
                    }                                  
                }
            }            
            
            var response = Request.CreateResponse(HttpStatusCode.OK, "Deleted for multiple user");

            

            return response;
        }//tested

        

        //Delete a setting at customer level and override all lower levels also
        /*
        [Route("settings/customer/{customerid}/{key}/{overrideLower:bool}")]
        public HttpResponseMessage DeleteCustomerSettingOverride(int customerid, string key,bool overrideLower)
        {
            if (!overrideLower)
            {
                return Request.CreateResponse(HttpStatusCode.OK, "OverrideLower if false");
            }

            var setting = settingRepository.GetCustomerSetting(customerid, key);

            if (setting == null)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }
            settingRepository.Delete(setting.Id);

            //subject to change, override lower
            var SettingList = settingRepository.GetAll()
                .Where(c => c.CustomerId == setting.CustomerId && c.SettingKey == key&&(c.Level == USER || c.Level == DEVICE)).ToList();
            if (SettingList.Count!=0)
            {
                foreach (var c in SettingList)
                {
                    settingRepository.Delete(c.Id);
                }
            }
            var response = Request.CreateResponse<Setting>(HttpStatusCode.OK, setting);

            return response;
        }//test here
        */


        //Delete a setting at customer level for specific user
        [Route("settings/customer/{customerid}/{username}/{key}")]
        public HttpResponseMessage DeleteCustomerEntitySetting(int customerid, string UserName, string key)
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
        public HttpResponseMessage DeleteCustomerEntitySetting(int customerid, string key, int deviceId)
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

        //Delete a setting at customer level for specific entity id and override all lower levels also if true
        //**
        [Route("settings/customer/{entityId}/{key}/{overrideLower:bool?}")]
        public HttpResponseMessage DeleteCustomerEntitySettingOverride(int entityId,string key,bool overrideLower=false)
        {
            var setting = settingRepository.GetCustomerSetting(entityId,key);

            if (setting == null)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }
            settingRepository.Delete(setting.Id);
            //subject to change, override lower
            if (overrideLower)
            {
                var SettingList = settingRepository.GetAll()
                    .Where(c => c.CustomerId == setting.CustomerId && c.SettingKey == key && (c.Level == USER || c.Level == DEVICE)).ToList();
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
            else
            {
                var response = Request.CreateResponse<Setting>(HttpStatusCode.OK, setting);

                return response;
            }


        }

        //
        //DELETE Device Level
        //

        //Delete a setting at device level
        [Route("settings/device/{CustomerId}/{key}/{deviceIds}")]
        public HttpResponseMessage DeleteDeviceSetting(int customerid, String deviceids, string key)
        {
            var divided = deviceids.Split(',');
            foreach (var deviceIdString in divided)
            {
                int deviceId=Int32.Parse(deviceIdString);
                try
                {
                    var setting = settingRepository.GetDeviceSetting(customerid, key, deviceId);
                    if (setting == null)
                    {
                        throw new HttpResponseException(HttpStatusCode.NotFound);
                    }
                    settingRepository.Delete(setting.Id);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    throw;
                }
            }
            var response = Request.CreateResponse<String>(HttpStatusCode.OK, deviceids);
            return response;
        }

        //Delete a setting at device level for specific entity id 
        [Route("settings/device/{CustomerId}/{deviceId}/{key}")]
        public HttpResponseMessage DeleteDeviceEntitySetting(int CustomerId, string key, int deviceId )
        {
            var setting = settingRepository.GetDeviceSetting(CustomerId, key, deviceId);

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

        //Delete mutiple setting at user level with usernames
        //Assume usernames is string and divided by ',', ie: jody,alex
        [Route("settings/user/{CustomerId}/{key}/{usernames}")]
        public HttpResponseMessage DeleteUserSetting(int CustomerId, string key, string usernames)
        {
            var divided = usernames.Split(',');
            String LastUsername;
            
            foreach (var username in divided)
            {
                LastUsername = username;
                try
                {

                    var setting = settingRepository.GetUserSetting(CustomerId, key, username);
                    if (setting == null)
                    {
                        throw new HttpResponseException(HttpStatusCode.NotFound);
                    }
                    settingRepository.Delete(setting.Id);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    throw;
                }
            }
            var response = Request.CreateResponse<String>(HttpStatusCode.OK, "DELETED");
            return response;//delete the setting and return it
        }

        //Delete a setting at user level for specific username   
        [Route("settings/user/{CustomerId}/{username}/{key}")]
        public HttpResponseMessage DeleteUserEntitySetting(int customerid, string key, string username)
        {
            var setting = settingRepository.GetUserSetting(customerid, key, username);

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