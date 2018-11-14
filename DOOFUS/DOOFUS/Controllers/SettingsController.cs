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

        //TESTED
        //Post a setting (global) 
        //**
        [Route("settings/global/{key}")]
        public HttpResponseMessage PostGlobalSetting(Setting setting, string key)
        {
            setting.Level = GLOBAL;
            setting.SettingKey = key;
            setting.LastModifiedBy = GLOBAL;
            setting.LastModifiedTimeStamp = DateTime.UtcNow;
            setting.CreatedTimeStamp = DateTime.UtcNow;
            setting.StartEffectiveDate = DateTime.UtcNow;

            if (!settingRepository.DoesSettingExist(setting))
            {
                setting = settingRepository.Add(setting);

                var response = Request.CreateResponse<Setting>(HttpStatusCode.Created, setting);
                var uri = Url.Link("Global", new { id = setting.Id });              

                return response;
            }
            else
            {
                var uri = Url.Link("Global", new { id = setting.Id });
                var response = Request.CreateResponse<Setting>(HttpStatusCode.PreconditionFailed, setting);
                response.Content = new StringContent(EXISTING_ENTRY);
                

                return response;                
            }

        }

        //Non override tested and working. Override still needs testing
        //Post a global setting with option to override lower levels
        [Route("settings/global/{key}/{overrideLower:bool?}")]
        public HttpResponseMessage PostGlobalSettingOverride(Setting setting, string key, bool overrideLower = false)
        {
            setting.Level = GLOBAL;
            setting.SettingKey = key;
            setting.LastModifiedBy = GLOBAL;
            setting.LastModifiedTimeStamp = DateTime.UtcNow;
            setting.CreatedTimeStamp = DateTime.UtcNow;
            setting.StartEffectiveDate = DateTime.UtcNow;

            if (!settingRepository.DoesSettingExistAtLevel(setting)) //check if setting already exists
            {
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
                var uri = Url.Link("Global", new { id = setting.Id });

                return response;
            }
            else //setting already exists, POST failed
            {
                var uri = Url.Link("Global", new { id = setting.Id });
                var response = Request.CreateResponse<Setting>(HttpStatusCode.PreconditionFailed, setting);
                response.Content = new StringContent(EXISTING_ENTRY);                

                return response;
            }
        }

        //Non override tested and working. Override still needs testing
        //Post a setting (global) to multiple customers - with option to override
        //settings/global/{key}?customerids=1,2,50, etc
        [Route("settings/global/{key}/{customerids}/{overrideLower:bool?}")]
        public HttpResponseMessage PostGlobalEntitySetting(Setting setting, string key, string customerids, bool overrideLower = false)
        {
            setting.Level = GLOBAL;
            setting.LastModifiedBy = GLOBAL;
            setting.SettingKey = key;
            setting.LastModifiedTimeStamp = DateTime.UtcNow;
            setting.CreatedTimeStamp = DateTime.UtcNow;
            setting.StartEffectiveDate = DateTime.UtcNow;

            if (!settingRepository.DoesSettingExist(setting)) //check if setting already exists
            {
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
                        
                        settingRepository.Add(setting);                                               
                    }
                }

                var response = Request.CreateResponse<Setting>(HttpStatusCode.Created, setting);
                var uri = Url.Link("GlobalEntity", new { id = setting.Id });
                //response.Headers.Location = new Uri(uri);

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

        ////Post a global setting - Specific customer id with option to override lower levels
        //[Route("settings/global/{key}/{customerid}/{overrideLower:bool?}")]
        //public HttpResponseMessage PostGlobalEntitySettingOverride(Setting setting,  string key, int customerid, bool overrideLower = false)
        //{
        //    if (!settingRepository.DoesSettingExist(setting)) //check if setting already exists
        //    {
        //        setting.Level = GLOBAL;
        //        setting.SettingKey = key;
        //        setting.LastModifiedBy = GLOBAL;
        //        setting.LastModifiedTimeStamp = DateTime.UtcNow;
        //        setting.CreatedTimeStamp = DateTime.UtcNow;
        //        setting.StartEffectiveDate = DateTime.UtcNow;

        //        //Get list of existing settings which match the incoming one, if any
        //        var SettingList = settingRepository.GetAll()
        //           .Where(c => c.SettingKey == key && c.CustomerId == customerid).ToList();

        //        if(overrideLower)
        //        {
        //            if (SettingList.Count() > 0)
        //            {
        //                foreach (var c in SettingList)
        //                {
        //                    setting.Level = c.Level;
        //                    setting.Id = c.Id;
        //                    settingRepository.Update(setting);
        //                }
        //            }
        //        }
        //        else //no override, just add
        //        {                    
        //            settingRepository.Add(setting);                    
        //        }
                

        //        var response = Request.CreateResponse<Setting>(HttpStatusCode.Created, setting);

        //        var uri = Url.Link("GlobalEntityOverride", new { id = setting.Id });
        //        response.Headers.Location = new Uri(uri);

        //        return response;
        //    }
        //    else
        //    {
        //        var uri = Url.Link("Global", new { id = setting.Id });
        //        var response = Request.CreateResponse<Setting>(HttpStatusCode.PreconditionFailed, setting);
        //        response.Content = new StringContent(EXISTING_ENTRY);
        //        response.Headers.Location = new Uri(uri);

        //        return response;
        //    }
        //}

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

       
        //Post a setting for one or more users or devices (entityid) and optionally override lower levels
        //If device id's were sent, override lower does nothing since we are already at the lowest level
       [Route("settings/customer/{customerid}/{key}/{entityids}/{overrideLower:bool?}")]
        public HttpResponseMessage PostCustomerSettingOverride(Setting setting, int customerid, string key, string entityids, bool overrideLower = false)
        {
            //Set setting parameters
            setting.Level = CUSTOMER;
            setting.SettingKey = key;
            setting.CustomerId = customerid;
            setting.LastModifiedById = customerid;
            setting.LastModifiedTimeStamp = DateTime.UtcNow;
            setting.CreatedTimeStamp = DateTime.UtcNow;
            setting.StartEffectiveDate = DateTime.UtcNow;

            if (!settingRepository.DoesSettingExistAtLevel(setting)) //check if setting already exists. 
            {
                settingRepository.Add(setting); //add the customer level first
                string[] separated = entityids.Split(',');

                //check if entity id's which were sent are device id's or usernames
                int parsed = 0;
                if(int.TryParse(separated.ElementAt(0), out parsed))
                {
                    setting.Level = DEVICE;
                    //For each device, add the setting by parsing the device list into individual ints
                    for (int i = 0; i < separated.Count(); i++)
                    {
                        Int32.TryParse(separated.ElementAt(i), out parsed);
                        setting.DeviceId = parsed;                        
                        settingRepository.Add(setting);
                    }
                    var response = Request.CreateResponse<Setting>(HttpStatusCode.Created, setting);
                    var uri = Url.Link("CustomerEntityOverride", new { id = setting.Id });

                    return response;
                }
                else //couldn't assign int, must be usernames
                {
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
                    else
                    {
                        setting.Level = USER;

                        //add or override this setting for each user specified in the CSV list
                        for (int i = 0; i < separated.Count(); i++)
                        {
                            setting.UserName = separated.ElementAt(i);
                            settingRepository.Add(setting);
                        }
                    }
                    //override wasn't specified, we are done here                
                    var response = Request.CreateResponse<Setting>(HttpStatusCode.Created, setting);
                    var uri = Url.Link("CustomerOverride", new { id = setting.Id });                    

                    return response;
                }              
            }
            else //The setting already exists at customer level 
            {
                var uri = Url.Link("Customer", new { id = setting.Id });
                var response = Request.CreateResponse<Setting>(HttpStatusCode.PreconditionFailed, setting);
                response.Content = new StringContent(EXISTING_ENTRY);               

                return response;
            }
        }     

        
        //
        //POST User Level
        //      

        //Post a user setting for specific user with option to  override lower
        [Route("settings/user/{customerid}/{username}/{key}/{overrideLower:bool?}")]
        public HttpResponseMessage PostUserEntitySetting(Setting setting, int customerid, string username, string key, bool overrideLower = false)
        {
            setting.Level = USER;
            setting.SettingKey = key;
            setting.UserName = username;
            setting.LastModifiedBy = username;
            setting.LastModifiedTimeStamp = DateTime.UtcNow;
            setting.CreatedTimeStamp = DateTime.UtcNow;
            setting.StartEffectiveDate = DateTime.UtcNow;

            if (!settingRepository.DoesSettingExist(setting)) //check if setting already exists
            {
                setting = settingRepository.Add(setting);

                //should override lower levels
                if(overrideLower)
                {
                    //override setting for all devices used by this user           
                    var SettingList = settingRepository.GetAll()
                      .Where(c => c.SettingKey == key && c.CustomerId == customerid && c.UserName == username && (c.Level == USER || c.Level == DEVICE)).ToList();

                    //Override user level and device level 
                    if (SettingList.Count() > 0)
                    {
                        foreach (var c in SettingList)
                        {
                            setting.DeviceId = c.DeviceId;

                            settingRepository.SaveOrUpdate(setting);
                        }
                    }
                }
                else //don't override, just update this level
                {                   
                    settingRepository.Add(setting);
                }                

                var response = Request.CreateResponse<Setting>(HttpStatusCode.Created, setting);
                var uri = Url.Link("UserEntity", new { id = setting.Id });                

                return response;
            }
            else //setting exists already at this level
            {
                var uri = Url.Link("User", new { id = setting.Id });
                var response = Request.CreateResponse<Setting>(HttpStatusCode.PreconditionFailed, setting);
                response.Content = new StringContent(EXISTING_ENTRY);                

                return response;
            }
        }

        //
        //POST Device Level
        //

        //Post device settings for one or more specific device ids
        //settings/device/{customerid}/{key}?deviceids=1,2,50, etc
        [Route("settings/device/{customerid}/{key}/{deviceids}")]
        public HttpResponseMessage PostDeviceSetting(Setting setting, int customerid, string key, string deviceids)
        {
            setting.Level = DEVICE;
            setting.CustomerId = customerid;
            setting.SettingKey = key;
            setting.LastModifiedById = customerid;
            setting.LastModifiedTimeStamp = DateTime.UtcNow;
            setting.CreatedTimeStamp = DateTime.UtcNow;
            setting.StartEffectiveDate = DateTime.UtcNow;

            if (!settingRepository.DoesSettingExist(setting)) //check if setting already exists
            {
                //separate device id string into individual strings
                var separated = deviceids.Split(',');
                int parsedDeviceId;

                //Post setting for each device
                foreach (var c in separated)
                {
                    Int32.TryParse(c, out parsedDeviceId);
                    setting.DeviceId = parsedDeviceId;
                    settingRepository.Add(setting);
                }

                var response = Request.CreateResponse<Setting>(HttpStatusCode.Created, setting);
                var uri = Url.Link("DeviceOverride", new { id = setting.Id });                

                return response;
            }
            else
            {
                var uri = Url.Link("Device", new { id = setting.Id });
                var response = Request.CreateResponse<Setting>(HttpStatusCode.PreconditionFailed, setting);
                response.Content = new StringContent(EXISTING_ENTRY);                

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
        public HttpResponseMessage PutGlobalSettingById(int id, Setting setting)
        {
            //get setting to update by id
            var currentSetting = settingRepository.Get(id);

            if(currentSetting == null)
            {
                var notFoundResponse = Request.CreateResponse(HttpStatusCode.BadRequest);
                return notFoundResponse;
            }

            //replace current setting values with new setting values
            //if a value is different in new setting object when compared to the current setting values, adjust accordingly
            if (setting.CustomerId != currentSetting.CustomerId && setting.CustomerId != null)
            {
                currentSetting.CustomerId = setting.CustomerId;
            }

            if (setting.DeviceId != currentSetting.DeviceId && setting.DeviceId != null)
            {
                currentSetting.DeviceId = setting.DeviceId;
            }

            if (setting.UserName != currentSetting.UserName && setting.UserName != null)
            {
                currentSetting.UserName = setting.UserName;
            }

            if (setting.StartEffectiveDate != currentSetting.StartEffectiveDate && setting.StartEffectiveDate != null)
            {
                currentSetting.StartEffectiveDate = setting.StartEffectiveDate;
            }

            if (setting.LastModifiedBy != currentSetting.LastModifiedBy && setting.LastModifiedBy != null)
            {
                currentSetting.LastModifiedBy = setting.LastModifiedBy;
            }

            if (setting.LastModifiedById != currentSetting.LastModifiedById && setting.LastModifiedById != null)
            {
                currentSetting.LastModifiedById = setting.LastModifiedById;
            }

            if (setting.Value != currentSetting.Value && setting.Value != null)
            {
                currentSetting.Value = setting.Value;
            }

            currentSetting.EndEffectiveDate = null;
            currentSetting.LastModifiedTimeStamp = DateTime.UtcNow;

            //update setting
            if (!settingRepository.Update(currentSetting))
            {
                var notFoundResponse = Request.CreateResponse(HttpStatusCode.BadRequest);
                return notFoundResponse;
            }

            var updatedResponse = Request.CreateResponse<Setting>(HttpStatusCode.OK, currentSetting);
            return updatedResponse;
        }

        //Put global with option to override
        [Route("settings/global/{key}/{overrideLower?}")]
        public HttpResponseMessage PutGlobalSetting(string key, Setting setting, bool overrideLower = false)
        {
            if (!overrideLower)
            {
                
                var currentSetting = settingRepository.GetAll().Where(x => x.SettingKey == key && x.Level == GLOBAL).ToList().FirstOrDefault<Setting>();

                if (currentSetting == null)
                {
                    var notFoundResponse = Request.CreateResponse(HttpStatusCode.BadRequest);
                    return notFoundResponse;
                }

                //replace current setting values with new values if applicable
                if (setting.CustomerId != currentSetting.CustomerId && setting.CustomerId != null)
                {
                    currentSetting.CustomerId = setting.CustomerId;
                }

                if (setting.DeviceId != currentSetting.DeviceId && setting.DeviceId != null)
                {
                    currentSetting.DeviceId = setting.DeviceId;
                }

                if (setting.UserName != currentSetting.UserName && setting.UserName != null)
                {
                    currentSetting.UserName = setting.UserName;
                }

                if (setting.StartEffectiveDate != currentSetting.StartEffectiveDate && setting.StartEffectiveDate != null)
                {
                    currentSetting.StartEffectiveDate = setting.StartEffectiveDate;
                }

                if (setting.LastModifiedBy != currentSetting.LastModifiedBy && setting.LastModifiedBy != null)
                {
                    currentSetting.LastModifiedBy = setting.LastModifiedBy;
                }

                if (setting.LastModifiedById != currentSetting.LastModifiedById && setting.LastModifiedById != null)
                {
                    currentSetting.LastModifiedById = setting.LastModifiedById;
                }

                if (setting.Value != currentSetting.Value && setting.Value != null)
                {
                    currentSetting.Value = setting.Value;
                }

                currentSetting.EndEffectiveDate = null;
                currentSetting.LastModifiedTimeStamp = DateTime.UtcNow;

                //try updating setting
                if (!settingRepository.Update(currentSetting))
                {
                    throw new HttpResponseException(HttpStatusCode.BadRequest);
                }

                //Create HTTP response
                var updatedResponse = Request.CreateResponse<Setting>(HttpStatusCode.OK, currentSetting);
                return updatedResponse;

            }
            else
            {
                //get list of all settings with specified key
                var listOfCurrentSettings = settingRepository.GetAll()
                    .Where(x => x.SettingKey == key &&
                    x.Level == GLOBAL || x.Level == CUSTOMER || x.Level == DEVICE || x.Level == USER).ToList();

                //update values of setting objects in list
                for (int i = 0; i < listOfCurrentSettings.Count(); i++)
                {
                    if (setting.CustomerId != listOfCurrentSettings[i].CustomerId && setting.CustomerId != null)
                    {
                        listOfCurrentSettings[i].CustomerId = setting.CustomerId;
                    }

                    if (setting.DeviceId != listOfCurrentSettings[i].DeviceId && setting.DeviceId != null)
                    {
                        listOfCurrentSettings[i].DeviceId = setting.DeviceId;
                    }

                    if (setting.UserName != listOfCurrentSettings[i].UserName && setting.UserName != null)
                    {
                        listOfCurrentSettings[i].UserName = setting.UserName;
                    }

                    if (setting.StartEffectiveDate != listOfCurrentSettings[i].StartEffectiveDate && setting.StartEffectiveDate != null)
                    {
                        listOfCurrentSettings[i].StartEffectiveDate = setting.StartEffectiveDate;
                    }

                    if (setting.LastModifiedBy != listOfCurrentSettings[i].LastModifiedBy && setting.LastModifiedBy != null)
                    {
                        listOfCurrentSettings[i].LastModifiedBy = setting.LastModifiedBy;
                    }

                    if (setting.LastModifiedById != listOfCurrentSettings[i].LastModifiedById && setting.LastModifiedById != null)
                    {
                        listOfCurrentSettings[i].LastModifiedById = setting.LastModifiedById;
                    }

                    if (setting.Value != listOfCurrentSettings[i].Value && setting.Value != null)
                    {
                        listOfCurrentSettings[i].Value = setting.Value;
                    }

                    listOfCurrentSettings[i].EndEffectiveDate = null;
                    listOfCurrentSettings[i].LastModifiedTimeStamp = DateTime.UtcNow;
                }

                //update settings
                for (int j = 0; j < listOfCurrentSettings.Count(); j++)
                {
                    if (!settingRepository.Update(listOfCurrentSettings[j]))
                    {
                        throw new HttpResponseException(HttpStatusCode.NotFound);
                    }
                }
                //Create HTTP response
                var updatedResponse = Request.CreateResponse<List<Setting>>(HttpStatusCode.OK, listOfCurrentSettings);
                return updatedResponse;
            }

           
        }

        //Put setting - Specific Entity (global)
        [Route("settings/global/{entityId}/{key}/{overrideLower?}")]
        public HttpResponseMessage PutGlobalEntitySetting(int entityId, string key, Setting setting, bool overrideLower = false)
        {
            if (!overrideLower) //if overrideLower is false, update specified setting with new setting values
            {
                var currentSetting = settingRepository.GetAll().Where(x => x.CustomerId == entityId 
                && x.SettingKey == key && x.Level == GLOBAL).ToList().FirstOrDefault<Setting>();

                if (currentSetting == null)
                {
                    var notFoundResponse = Request.CreateResponse(HttpStatusCode.BadRequest);
                    return notFoundResponse;
                }

                //replace current setting values with new setting values
                if (setting.CustomerId != currentSetting.CustomerId && setting.CustomerId != null)
                {
                    currentSetting.CustomerId = setting.CustomerId;
                }

                if (setting.DeviceId != currentSetting.DeviceId && setting.DeviceId != null)
                {
                    currentSetting.DeviceId = setting.DeviceId;
                }

                if (setting.UserName != currentSetting.UserName && setting.UserName != null)
                {
                    currentSetting.UserName = setting.UserName;
                }

                if (setting.StartEffectiveDate != currentSetting.StartEffectiveDate && setting.StartEffectiveDate != null)
                {
                    currentSetting.StartEffectiveDate = setting.StartEffectiveDate;
                }

                if (setting.LastModifiedBy != currentSetting.LastModifiedBy && setting.LastModifiedBy != null)
                {
                    currentSetting.LastModifiedBy = setting.LastModifiedBy;
                }

                if (setting.LastModifiedById != currentSetting.LastModifiedById && setting.LastModifiedById != null)
                {
                    currentSetting.LastModifiedById = setting.LastModifiedById;
                }

                if (setting.Value != currentSetting.Value && setting.Value != null)
                {
                    currentSetting.Value = setting.Value;
                }

                currentSetting.EndEffectiveDate = null;
                currentSetting.LastModifiedTimeStamp = DateTime.UtcNow;


                //update setting
                if (!settingRepository.Update(currentSetting))
                {
                    throw new HttpResponseException(HttpStatusCode.BadRequest);
                }
            }
            else //if overrideLower is true, override lower levels
            {
                //get list of all setting with specified key and all levels
                var listOfCurrentSettings = settingRepository.GetAll()
                    .Where(x => x.SettingKey == key && x.CustomerId == entityId &&
                    x.Level == GLOBAL || x.Level == CUSTOMER || x.Level == DEVICE || x.Level == USER).ToList();

                if(listOfCurrentSettings.Count == 0)
                {
                    var notFoundResponse = Request.CreateResponse(HttpStatusCode.BadRequest);
                    return notFoundResponse;
                }

                //update values of setting objects in list
                for (int i = 0; i < listOfCurrentSettings.Count(); i++)
                {
                    if (setting.CustomerId != listOfCurrentSettings[i].CustomerId && setting.CustomerId != null)
                    {
                        listOfCurrentSettings[i].CustomerId = setting.CustomerId;
                    }

                    if (setting.DeviceId != listOfCurrentSettings[i].DeviceId && setting.DeviceId != null)
                    {
                        listOfCurrentSettings[i].DeviceId = setting.DeviceId;
                    }

                    if (setting.UserName != listOfCurrentSettings[i].UserName && setting.UserName != null)
                    {
                        listOfCurrentSettings[i].UserName = setting.UserName;
                    }

                    if (setting.StartEffectiveDate != listOfCurrentSettings[i].StartEffectiveDate && setting.StartEffectiveDate != null)
                    {
                        listOfCurrentSettings[i].StartEffectiveDate = setting.StartEffectiveDate;
                    }

                    if (setting.LastModifiedBy != listOfCurrentSettings[i].LastModifiedBy && setting.LastModifiedBy != null)
                    {
                        listOfCurrentSettings[i].LastModifiedBy = setting.LastModifiedBy;
                    }

                    if (setting.LastModifiedById != listOfCurrentSettings[i].LastModifiedById && setting.LastModifiedById != null)
                    {
                        listOfCurrentSettings[i].LastModifiedById = setting.LastModifiedById;
                    }

                    if (setting.Value != listOfCurrentSettings[i].Value && setting.Value != null)
                    {
                        listOfCurrentSettings[i].Value = setting.Value;
                    }

                    listOfCurrentSettings[i].EndEffectiveDate = null;
                    listOfCurrentSettings[i].LastModifiedTimeStamp = DateTime.UtcNow;
                }

                //update settings
                for (int j = 0; j < listOfCurrentSettings.Count(); j++)
                {
                    if (!settingRepository.Update(listOfCurrentSettings[j]))
                    {
                        throw new HttpResponseException(HttpStatusCode.BadRequest);
                    }
                }
            }

            var updatedResponse = Request.CreateResponse<Setting>(HttpStatusCode.OK, setting);
            return updatedResponse;

        }

        //
        //PUT Customer Level
        //

        //Put setting (customer) with option to override lower
        [Route("settings/customer/{key}/{overrideLower?}")]
        public HttpResponseMessage PutCustomerSetting(string key, Setting setting, bool overrideLower = false)
        {
            if (!overrideLower)
            {
                var currentSetting = settingRepository.GetAll().Where(x => x.SettingKey == key && x.Level == CUSTOMER).ToList().FirstOrDefault<Setting>();

                if (currentSetting == null)
                {
                    var notFoundResponse = Request.CreateResponse(HttpStatusCode.BadRequest);
                    return notFoundResponse;
                }

                //replace current setting values with new values if applicable
                if (setting.CustomerId != currentSetting.CustomerId && setting.CustomerId != null)
                {
                    currentSetting.CustomerId = setting.CustomerId;
                }

                if (setting.DeviceId != currentSetting.DeviceId && setting.DeviceId != null)
                {
                    currentSetting.DeviceId = setting.DeviceId;
                }

                if (setting.UserName != currentSetting.UserName && setting.UserName != null)
                {
                    currentSetting.UserName = setting.UserName;
                }

                if (setting.StartEffectiveDate != currentSetting.StartEffectiveDate && setting.StartEffectiveDate != null)
                {
                    currentSetting.StartEffectiveDate = setting.StartEffectiveDate;
                }

                if (setting.LastModifiedBy != currentSetting.LastModifiedBy && setting.LastModifiedBy != null)
                {
                    currentSetting.LastModifiedBy = setting.LastModifiedBy;
                }

                if (setting.LastModifiedById != currentSetting.LastModifiedById && setting.LastModifiedById != null)
                {
                    currentSetting.LastModifiedById = setting.LastModifiedById;
                }

                if (setting.Value != currentSetting.Value && setting.Value != null)
                {
                    currentSetting.Value = setting.Value;
                }

                currentSetting.EndEffectiveDate = null;
                currentSetting.LastModifiedTimeStamp = DateTime.UtcNow;

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

                if (listOfCurrentSettings.Count == 0)
                {
                    var notFoundResponse = Request.CreateResponse(HttpStatusCode.BadRequest);
                    return notFoundResponse;
                }

                //update values of setting objects in list
                for (int i = 0; i < listOfCurrentSettings.Count(); i++)
                {
                    if (setting.CustomerId != listOfCurrentSettings[i].CustomerId && setting.CustomerId != null)
                    {
                        listOfCurrentSettings[i].CustomerId = setting.CustomerId;
                    }

                    if (setting.DeviceId != listOfCurrentSettings[i].DeviceId && setting.DeviceId != null) 
                    {
                        listOfCurrentSettings[i].DeviceId = setting.DeviceId;
                    }

                    if (setting.UserName != listOfCurrentSettings[i].UserName && setting.UserName != null)
                    {
                        listOfCurrentSettings[i].UserName = setting.UserName;
                    }

                    if (setting.StartEffectiveDate != listOfCurrentSettings[i].StartEffectiveDate && setting.StartEffectiveDate != null)
                    {
                        listOfCurrentSettings[i].StartEffectiveDate = setting.StartEffectiveDate;
                    }

                    if (setting.LastModifiedBy != listOfCurrentSettings[i].LastModifiedBy && setting.LastModifiedBy != null)
                    {
                        listOfCurrentSettings[i].LastModifiedBy = setting.LastModifiedBy;
                    }

                    if (setting.LastModifiedById != listOfCurrentSettings[i].LastModifiedById && setting.LastModifiedById != null)
                    {
                        listOfCurrentSettings[i].LastModifiedById = setting.LastModifiedById;
                    }

                    if (setting.Value != listOfCurrentSettings[i].Value && setting.Value != null)
                    {
                        listOfCurrentSettings[i].Value = setting.Value;
                    }

                    listOfCurrentSettings[i].EndEffectiveDate = null;
                    listOfCurrentSettings[i].LastModifiedTimeStamp = DateTime.UtcNow;
                }

                //update settings
                for (int j = 0; j < listOfCurrentSettings.Count(); j++)
                {
                    if (!settingRepository.Update(listOfCurrentSettings[j]))
                    {
                        throw new HttpResponseException(HttpStatusCode.BadRequest);
                    }
                }

            }

            //Create HTTP response
            var response = Request.CreateResponse<Setting>(HttpStatusCode.OK, setting);
            return response;
        }

        //Put setting (customer) with option to override lower
        //Can update multiple customerIds
        //settings/customer/{key}/{overrideLower?}?customerIds="123,234,345"
        [Route("settings/customer/{key}/{overrideLower?}")]
        public HttpResponseMessage PutCustomerSetting(string key, Setting setting, string customerIds, bool overrideLower = false)
        {
            string[] separated = customerIds.Split(','); //hold seperated customer IDs
            int[] cIds = new int[separated.Count()]; //place parsed ids in this array

            for(int i = 0; i < separated.Count(); i++) //convert strings to ints and place in array
            {
                cIds[i] = Convert.ToInt32(separated[i]);
            }

            var currentSettings = new List<Setting>(); //new list to store settings

            if (!overrideLower)
            {
                //add each setting  to array that is specific to the customer Ids entered
                for(int j = 0; j < separated.Count(); j++)
                {
                    currentSettings.Add(settingRepository.GetAll().Where(x => x.SettingKey == key 
                    && x.Level == CUSTOMER && x.CustomerId == cIds[j]).ToList().FirstOrDefault<Setting>());
                }

                if (currentSettings.Count() == 0)
                {
                    var notFoundResponse = Request.CreateResponse(HttpStatusCode.BadRequest);
                    return notFoundResponse;
                }

                for (int k = 0; k < currentSettings.Count(); k++)
                {
                    //replace current setting values with new values if applicable
                    if (setting.CustomerId != currentSettings[k].CustomerId && setting.CustomerId != null)
                    {
                        currentSettings[k].CustomerId = setting.CustomerId;
                    }

                    if (setting.DeviceId != currentSettings[k].DeviceId && setting.DeviceId != null)
                    {
                        currentSettings[k].DeviceId = setting.DeviceId;
                    }

                    if (setting.UserName != currentSettings[k].UserName && setting.UserName != null)
                    {
                        currentSettings[k].UserName = setting.UserName;
                    }

                    if (setting.StartEffectiveDate != currentSettings[k].StartEffectiveDate && setting.StartEffectiveDate != null)
                    {
                        currentSettings[k].StartEffectiveDate = setting.StartEffectiveDate;
                    }

                    if (setting.LastModifiedBy != currentSettings[k].LastModifiedBy && setting.LastModifiedBy != null)
                    {
                        currentSettings[k].LastModifiedBy = setting.LastModifiedBy;
                    }

                    if (setting.LastModifiedById != currentSettings[k].LastModifiedById && setting.LastModifiedById != null)
                    {
                        currentSettings[k].LastModifiedById = setting.LastModifiedById;
                    }

                    if (setting.Value != currentSettings[k].Value && setting.Value != null)
                    {
                        currentSettings[k].Value = setting.Value;
                    }

                    currentSettings[k].EndEffectiveDate = null;
                    currentSettings[k].LastModifiedTimeStamp = DateTime.UtcNow;

                    if (!settingRepository.Update(currentSettings[k]))
                    {
                        throw new HttpResponseException(HttpStatusCode.BadRequest);
                    }
                }

            }
            else
            {
                //add lower level settings to currentSettings list
                for (int j = 0; j < separated.Count(); j++)
                {
                    currentSettings.Add(settingRepository.GetAll().Where(x => x.SettingKey == key &&
                    x.Level == CUSTOMER && x.CustomerId == cIds[j]).ToList().FirstOrDefault<Setting>());

                    currentSettings.Add(settingRepository.GetAll().Where(x => x.SettingKey == key &&
                    x.Level == DEVICE && x.CustomerId == cIds[j]).ToList().FirstOrDefault<Setting>());

                    currentSettings.Add(settingRepository.GetAll().Where(x => x.SettingKey == key &&
                    x.Level == USER && x.CustomerId == cIds[j]).ToList().FirstOrDefault<Setting>());
                }


                if (currentSettings.Count() == 0)
                {
                    var notFoundResponse = Request.CreateResponse(HttpStatusCode.BadRequest);
                    return notFoundResponse;
                }

                //update values of setting objects in list
                for (int k = 0; k < currentSettings.Count(); k++)
                {
                    if (setting.CustomerId != currentSettings[k].CustomerId && setting.CustomerId != null)
                    {
                        currentSettings[k].CustomerId = setting.CustomerId;
                    }

                    if (setting.DeviceId != currentSettings[k].DeviceId && setting.DeviceId != null)
                    {
                        currentSettings[k].DeviceId = setting.DeviceId;
                    }

                    if (setting.UserName != currentSettings[k].UserName && setting.UserName != null)
                    {
                        currentSettings[k].UserName = setting.UserName;
                    }

                    if (setting.StartEffectiveDate != currentSettings[k].StartEffectiveDate && setting.StartEffectiveDate != null)
                    {
                        currentSettings[k].StartEffectiveDate = setting.StartEffectiveDate;
                    }

                    if (setting.LastModifiedBy != currentSettings[k].LastModifiedBy && setting.LastModifiedBy != null)
                    {
                        currentSettings[k].LastModifiedBy = setting.LastModifiedBy;
                    }

                    if (setting.LastModifiedById != currentSettings[k].LastModifiedById && setting.LastModifiedById != null)
                    {
                        currentSettings[k].LastModifiedById = setting.LastModifiedById;
                    }

                    if (setting.Value != currentSettings[k].Value && setting.Value != null)
                    {
                        currentSettings[k].Value = setting.Value;
                    }

                    currentSettings[k].EndEffectiveDate = null;
                    currentSettings[k].LastModifiedTimeStamp = DateTime.UtcNow;
                }

                //update settings
                for (int j = 0; j < currentSettings.Count(); j++)
                {
                    if (!settingRepository.Update(currentSettings[j]))
                    {
                        throw new HttpResponseException(HttpStatusCode.NotFound);
                    }
                }

            }

            //Create HTTP response
            var response = Request.CreateResponse<Setting>(HttpStatusCode.OK, setting);
            return response;
        }



        //Put setting - Specific Entity (customer) and override lower
        [Route("settings/customer/{entityId}/{key}/{overrideLower?}")]
        public HttpResponseMessage PutCustomerEntitySetting(int entityId, string key, Setting setting, bool overrideLower = false)
        {
            if (!overrideLower)
            {
                var currentSetting = settingRepository.GetAll().Where(x => x.CustomerId == entityId && x.SettingKey == key
                && x.Level == CUSTOMER).FirstOrDefault<Setting>();

                if (currentSetting == null)
                {
                    var notFoundResponse = Request.CreateResponse(HttpStatusCode.BadRequest);
                    return notFoundResponse;
                }

                if (setting.CustomerId != currentSetting.CustomerId && setting.CustomerId != null)
                {
                    currentSetting.CustomerId = setting.CustomerId;
                }

                if (setting.DeviceId != currentSetting.DeviceId && setting.DeviceId != null)
                {
                    currentSetting.DeviceId = setting.DeviceId;
                }

                if (setting.UserName != currentSetting.UserName && setting.UserName != null)
                {
                    currentSetting.UserName = setting.UserName;
                }

                if (setting.StartEffectiveDate != currentSetting.StartEffectiveDate && setting.StartEffectiveDate != null)
                {
                    currentSetting.StartEffectiveDate = setting.StartEffectiveDate;
                }

                if (setting.LastModifiedBy != currentSetting.LastModifiedBy && setting.LastModifiedBy != null)
                {
                    currentSetting.LastModifiedBy = setting.LastModifiedBy;
                }

                if (setting.LastModifiedById != currentSetting.LastModifiedById && setting.LastModifiedById != null)
                {
                    currentSetting.LastModifiedById = setting.LastModifiedById;
                }

                if (setting.Value != currentSetting.Value && setting.Value != null)
                {
                    currentSetting.Value = setting.Value;
                }

                currentSetting.EndEffectiveDate = null;
                currentSetting.LastModifiedTimeStamp = DateTime.UtcNow;

                if (!settingRepository.Update(currentSetting))
                {
                    throw new HttpResponseException(HttpStatusCode.BadRequest);
                }
            }
            else
            {
                var listOfCurrentSettings = settingRepository.GetAll().Where(x => x.DeviceId == entityId && x.SettingKey == key
                && x.Level == CUSTOMER || x.Level == DEVICE || x.Level == USER).ToList();

                if (listOfCurrentSettings.Count() == 0)
                {
                    var notFoundResponse = Request.CreateResponse(HttpStatusCode.BadRequest);
                    return notFoundResponse;
                }

                for (int i = 0; i < listOfCurrentSettings.Count; i++)
                {
                    if (setting.CustomerId != listOfCurrentSettings[i].CustomerId && setting.CustomerId != null)
                    {
                        listOfCurrentSettings[i].CustomerId = setting.CustomerId;
                    }

                    if (setting.DeviceId != listOfCurrentSettings[i].DeviceId && setting.DeviceId != null)
                    {
                        listOfCurrentSettings[i].DeviceId = setting.DeviceId;
                    }

                    if (setting.UserName != listOfCurrentSettings[i].UserName && setting.UserName != null)
                    {
                        listOfCurrentSettings[i].UserName = setting.UserName;
                    }

                    if (setting.StartEffectiveDate != listOfCurrentSettings[i].StartEffectiveDate && setting.StartEffectiveDate != null)
                    {
                        listOfCurrentSettings[i].StartEffectiveDate = setting.StartEffectiveDate;
                    }

                    if (setting.LastModifiedBy != listOfCurrentSettings[i].LastModifiedBy && setting.LastModifiedBy != null)
                    {
                        listOfCurrentSettings[i].LastModifiedBy = setting.LastModifiedBy;
                    }

                    if (setting.LastModifiedById != listOfCurrentSettings[i].LastModifiedById && setting.LastModifiedById != null)
                    {
                        listOfCurrentSettings[i].LastModifiedById = setting.LastModifiedById;
                    }

                    if (setting.Value != listOfCurrentSettings[i].Value && setting.Value != null)
                    {
                        listOfCurrentSettings[i].Value = setting.Value;
                    }

                    listOfCurrentSettings[i].EndEffectiveDate = null;
                    listOfCurrentSettings[i].LastModifiedTimeStamp = DateTime.UtcNow;
                }

                //update settings
                for (int j = 0; j < listOfCurrentSettings.Count(); j++)
                {
                    if (!settingRepository.Update(listOfCurrentSettings[j]))
                    {
                        throw new HttpResponseException(HttpStatusCode.BadRequest);
                    }
                }
            }

            var response = Request.CreateResponse<Setting>(HttpStatusCode.OK, setting);
            return response;
        }

        //
        //PUT Device Level      
        //

        //Put setting for device with option to override lower levels
        [Route("settings/device/{customerId}/{key}")]
        public HttpResponseMessage PutDeviceSetting(int customerId, string key, Setting setting)
        {   
            var currentSetting = settingRepository.GetAll().Where(x => x.SettingKey == key
            && x.CustomerId == customerId && x.Level == DEVICE).FirstOrDefault<Setting>();

            if (currentSetting == null)
            {
                var notFoundResponse = Request.CreateResponse(HttpStatusCode.BadRequest);
                return notFoundResponse;
            }

            if (setting.CustomerId != currentSetting.CustomerId && setting.CustomerId != null)
            {
                currentSetting.CustomerId = setting.CustomerId;
            }

            if (setting.DeviceId != currentSetting.DeviceId && setting.DeviceId != null)
            {
                currentSetting.DeviceId = setting.DeviceId;
            }

            if (setting.UserName != currentSetting.UserName && setting.UserName != null)
            {
                currentSetting.UserName = setting.UserName;
            }

            if (setting.StartEffectiveDate != currentSetting.StartEffectiveDate && setting.StartEffectiveDate != null)
            {
                currentSetting.StartEffectiveDate = setting.StartEffectiveDate;
            }

            if (setting.LastModifiedBy != currentSetting.LastModifiedBy && setting.LastModifiedBy != null)
            {
                currentSetting.LastModifiedBy = setting.LastModifiedBy;
            }

            if (setting.LastModifiedById != currentSetting.LastModifiedById && setting.LastModifiedById != null)
            {
                currentSetting.LastModifiedById = setting.LastModifiedById;
            }

            if (setting.Value != currentSetting.Value && setting.Value != null)
            {
                currentSetting.Value = setting.Value;
            }

            currentSetting.EndEffectiveDate = null;
            currentSetting.LastModifiedTimeStamp = DateTime.UtcNow;

            if (!settingRepository.Update(currentSetting))
            {
                throw new HttpResponseException(HttpStatusCode.BadRequest);
            }

            //Create HTTP response
            var updatedResponse = Request.CreateResponse<Setting>(HttpStatusCode.OK, currentSetting);
            return updatedResponse;
        }

        //settings/device/{customerId}/{key}?deviceIds = "111,234,678,etc."
        [Route("settings/device/{customerId}/{key}")]
        public HttpResponseMessage PutDeviceSetting(int customerId, string key, string deviceIds, Setting setting)
        {
            string[] separated = deviceIds.Split(','); //hold seperated customer IDs
            int[] dIds = new int[separated.Count()]; //place parsed ids in this array

            for (int i = 0; i < separated.Count(); i++) //convert strings to ints and place in array
            {
                dIds[i] = Convert.ToInt32(separated[i]);
            }

            var currentSettings = new List<Setting>(); //new list to store settings

            for(int i = 0; i < dIds.Count(); i++) //place all settings to modify in a list
            {
                currentSettings.Add(settingRepository.GetAll().Where(x => x.SettingKey == key
                && x.Level == CUSTOMER && x.CustomerId == dIds[i]).ToList().FirstOrDefault<Setting>());
            }

            if(currentSettings.Count() == 0)
            {
                var notFoundResponse = Request.CreateResponse(HttpStatusCode.BadRequest);
                return notFoundResponse;
            }

            for(int j = 0; j < currentSettings.Count(); j++) //go through each setting in list and perform nessesary changes
            {
                if (setting.CustomerId != currentSettings[j].CustomerId && setting.CustomerId != null)
                {
                    currentSettings[j].CustomerId = setting.CustomerId;
                }

                if (setting.DeviceId != currentSettings[j].DeviceId && setting.DeviceId != null)
                {
                    currentSettings[j].DeviceId = setting.DeviceId;
                }

                if (setting.UserName != currentSettings[j].UserName && setting.UserName != null)
                {
                    currentSettings[j].UserName = setting.UserName;
                }

                if (setting.StartEffectiveDate != currentSettings[j].StartEffectiveDate && setting.StartEffectiveDate != null)
                {
                    currentSettings[j].StartEffectiveDate = setting.StartEffectiveDate;
                }

                if (setting.LastModifiedBy != currentSettings[j].LastModifiedBy && setting.LastModifiedBy != null)
                {
                    currentSettings[j].LastModifiedBy = setting.LastModifiedBy;
                }

                if (setting.LastModifiedById != currentSettings[j].LastModifiedById && setting.LastModifiedById != null)
                {
                    currentSettings[j].LastModifiedById = setting.LastModifiedById;
                }

                if (setting.Value != currentSettings[j].Value && setting.Value != null)
                {
                    currentSettings[j].Value = setting.Value;
                }

                currentSettings[j].EndEffectiveDate = null;
                currentSettings[j].LastModifiedTimeStamp = DateTime.UtcNow;

                if (!settingRepository.Update(currentSettings[j]))
                {
                    throw new HttpResponseException(HttpStatusCode.NotFound);
                }

            }

            //Create HTTP response
            var updatedResponse = Request.CreateResponse<Setting>(HttpStatusCode.Created, setting);
            return updatedResponse;
        }

        //Put setting for device - Specific entity 
        [Route("settings/device/{customerId}/{entityId}/{key}")]
        public HttpResponseMessage PutDeviceEntitySetting(int customerId, string entityId, string key, Setting setting, bool overrideLower = false)
        {
            var currentSetting = settingRepository.GetAll().Where(x => x.CustomerId == customerId
            && x.UserName == entityId && x.SettingKey == key && x.Level == DEVICE).FirstOrDefault<Setting>();

            if (currentSetting == null)
            {
                var notFoundResponse = Request.CreateResponse(HttpStatusCode.BadRequest);
                return notFoundResponse;
            }

            if (setting.CustomerId != currentSetting.CustomerId && setting.CustomerId != null)
            {
                currentSetting.CustomerId = setting.CustomerId;
            }

            if (setting.DeviceId != currentSetting.DeviceId && setting.DeviceId != null)
            {
                currentSetting.DeviceId = setting.DeviceId;
            }

            if (setting.UserName != currentSetting.UserName && setting.UserName != null)
            {
                currentSetting.UserName = setting.UserName;
            }

            if (setting.StartEffectiveDate != currentSetting.StartEffectiveDate && setting.StartEffectiveDate != null)
            {
                currentSetting.StartEffectiveDate = setting.StartEffectiveDate;
            }

            if (setting.LastModifiedBy != currentSetting.LastModifiedBy && setting.LastModifiedBy != null)
            {
                currentSetting.LastModifiedBy = setting.LastModifiedBy;
            }

            if (setting.LastModifiedById != currentSetting.LastModifiedById && setting.LastModifiedById != null)
            {
                currentSetting.LastModifiedById = setting.LastModifiedById;
            }

            if (setting.Value != currentSetting.Value && setting.Value != null)
            {
                currentSetting.Value = setting.Value;
            }

            currentSetting.EndEffectiveDate = null;
            currentSetting.LastModifiedTimeStamp = DateTime.UtcNow;

            if (!settingRepository.Update(currentSetting))
            {
                throw new HttpResponseException(HttpStatusCode.BadRequest);
            }

            //Create HTTP response
            var updatedResponse = Request.CreateResponse<Setting>(HttpStatusCode.OK, currentSetting);
            return updatedResponse;
        }


        //
        //PUT User Level
        //

        //Put setting for user 
        [Route("settings/user/{customerId}/{key}")]
        public HttpResponseMessage PutUserSetting(int customerId, string key, Setting setting)
        {
            var currentSetting = settingRepository.GetAll().Where(x => x.CustomerId == customerId 
            && x.SettingKey == key && x.Level == USER).FirstOrDefault<Setting>();

            if (currentSetting == null)
            {
                var notFoundResponse = Request.CreateResponse(HttpStatusCode.BadRequest);
                return notFoundResponse;
            }

            if (setting.CustomerId != currentSetting.CustomerId && setting.CustomerId != null)
            {
                currentSetting.CustomerId = setting.CustomerId;
            }

            if (setting.DeviceId != currentSetting.DeviceId && setting.DeviceId != null)
            {
                currentSetting.DeviceId = setting.DeviceId;
            }

            if (setting.UserName != currentSetting.UserName && setting.UserName != null)
            {
                currentSetting.UserName = setting.UserName;
            }

            if (setting.StartEffectiveDate != currentSetting.StartEffectiveDate && setting.StartEffectiveDate != null)
            {
                currentSetting.StartEffectiveDate = setting.StartEffectiveDate;
            }

            if (setting.LastModifiedBy != currentSetting.LastModifiedBy && setting.LastModifiedBy != null)
            {
                currentSetting.LastModifiedBy = setting.LastModifiedBy;
            }

            if (setting.LastModifiedById != currentSetting.LastModifiedById && setting.LastModifiedById != null)
            {
                currentSetting.LastModifiedById = setting.LastModifiedById;
            }

            if (setting.Value != currentSetting.Value && setting.Value != null)
            {
                currentSetting.Value = setting.Value;
            }

            currentSetting.EndEffectiveDate = null;
            currentSetting.LastModifiedTimeStamp = DateTime.UtcNow;

            if (!settingRepository.Update(currentSetting))
            {
                throw new HttpResponseException(HttpStatusCode.BadRequest);
            }

            //Create HTTP response
            var updatedResponse = Request.CreateResponse<Setting>(HttpStatusCode.OK, currentSetting);
            return updatedResponse;
        }

        //put setting for user
        //settings/user/{customerId}/{key}?usernames = "jody,victor,phum,alex"
        [Route("settings/user/{customerId}/{key}")]
        public HttpResponseMessage PutUserSetting(int customerId, string key, string usernames, Setting setting)
        {
            var currentSetting = settingRepository.GetAll().Where(x => x.CustomerId == customerId
            && x.SettingKey == key && x.Level == USER).FirstOrDefault<Setting>();

            if (currentSetting == null)
            {
                var notFoundResponse = Request.CreateResponse(HttpStatusCode.BadRequest);
                return notFoundResponse;
            }

            if (setting.CustomerId != currentSetting.CustomerId && setting.CustomerId != null)
            {
                currentSetting.CustomerId = setting.CustomerId;
            }

            if (setting.DeviceId != currentSetting.DeviceId && setting.DeviceId != null)
            {
                currentSetting.DeviceId = setting.DeviceId;
            }

            if (setting.UserName != currentSetting.UserName && setting.UserName != null)
            {
                currentSetting.UserName = setting.UserName;
            }

            if (setting.StartEffectiveDate != currentSetting.StartEffectiveDate && setting.StartEffectiveDate != null)
            {
                currentSetting.StartEffectiveDate = setting.StartEffectiveDate;
            }

            if (setting.LastModifiedBy != currentSetting.LastModifiedBy && setting.LastModifiedBy != null)
            {
                currentSetting.LastModifiedBy = setting.LastModifiedBy;
            }

            if (setting.LastModifiedById != currentSetting.LastModifiedById && setting.LastModifiedById != null)
            {
                currentSetting.LastModifiedById = setting.LastModifiedById;
            }

            if (setting.Value != currentSetting.Value && setting.Value != null)
            {
                currentSetting.Value = setting.Value;
            }

            currentSetting.EndEffectiveDate = null;
            currentSetting.LastModifiedTimeStamp = DateTime.UtcNow;

            if (!settingRepository.Update(currentSetting))
            {
                throw new HttpResponseException(HttpStatusCode.BadRequest);
            }

            //Create HTTP response
            var updatedResponse = Request.CreateResponse<Setting>(HttpStatusCode.OK, currentSetting);
            return updatedResponse;
        }

        //Put setting for user - Specific entity 
        [Route("settings/user/{customerId}/{entityId}/{key}")]
        public HttpResponseMessage PutUserEntitySetting(int customerId, string entityId, string key, Setting setting)
        {
            var currentSetting = settingRepository.GetAll().Where(x => x.CustomerId == customerId
            && x.UserName == entityId && x.SettingKey == key && x.Level == USER).FirstOrDefault<Setting>();

            if (currentSetting == null)
            {
                var notFoundResponse = Request.CreateResponse(HttpStatusCode.BadRequest);
                return notFoundResponse;
            }

            if (setting.CustomerId != currentSetting.CustomerId && setting.CustomerId != null)
            {
                currentSetting.CustomerId = setting.CustomerId;
            }

            if (setting.DeviceId != currentSetting.DeviceId && setting.DeviceId != null)
            {
                currentSetting.DeviceId = setting.DeviceId;
            }

            if (setting.UserName != currentSetting.UserName && setting.UserName != null)
            {
                currentSetting.UserName = setting.UserName;
            }

            if (setting.StartEffectiveDate != currentSetting.StartEffectiveDate && setting.StartEffectiveDate != null)
            {
                currentSetting.StartEffectiveDate = setting.StartEffectiveDate;
            }

            if (setting.LastModifiedBy != currentSetting.LastModifiedBy && setting.LastModifiedBy != null)
            {
                currentSetting.LastModifiedBy = setting.LastModifiedBy;
            }

            if (setting.LastModifiedById != currentSetting.LastModifiedById && setting.LastModifiedById != null)
            {
                currentSetting.LastModifiedById = setting.LastModifiedById;
            }

            if (setting.Value != currentSetting.Value && setting.Value != null)
            {
                currentSetting.Value = setting.Value;
            }

            currentSetting.EndEffectiveDate = null;
            currentSetting.LastModifiedTimeStamp = DateTime.UtcNow;

            if (!settingRepository.Update(currentSetting))
            {
                throw new HttpResponseException(HttpStatusCode.BadRequest);
            }

            //Create HTTP response
            var updatedResponse = Request.CreateResponse<Setting>(HttpStatusCode.OK, currentSetting);
            return updatedResponse;
        }


        //
        //DELETE
        //

        //
        //DELETE Global Level
        //

        //Delete a setting at global level with option to delete all lower levels also

        [Route("settings/global/{keys}/{overrideLower:bool?}")]
        public HttpResponseMessage DeleteGlobalSettingOverride(string keys,bool overrideLower=false)
        {
            var SeperatedKeys = keys.Split(',').ToList();

            if (SeperatedKeys.Count==0)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, "Key can not be empty");
            }

            if (!overrideLower)
            {
                foreach (var key in SeperatedKeys)
                {
                    var setting = settingRepository.GetGlobalSetting(key);
                    if (setting == null)
                    {
                        return Request.CreateResponse(HttpStatusCode.NotFound,"Key: "+key+" Not found");
                    }

                    settingRepository.Delete(setting.Id);

                }
                return Request.CreateResponse(HttpStatusCode.OK);
            }
            else
            {
                foreach (var key in SeperatedKeys)
                {
                    var setting = settingRepository.GetGlobalSetting(key);
                    if (setting == null)
                    {
                        return Request.CreateResponse(HttpStatusCode.NotFound, "Key: " + key + " Not found");
                    }

                    settingRepository.Delete(setting.Id);

                    var SettingsList = settingRepository.GetAll().Where(c => c.SettingKey == key && (c.Level == USER || c.Level == CUSTOMER || c.Level == DEVICE)).ToList();
                    if (SettingsList.Count == 0)
                    {
                        var response = Request.CreateResponse(HttpStatusCode.OK);
                        return response;
                    }
                    foreach (var c in SettingsList)
                    {
                        settingRepository.Delete(c.Id);
                    }
                }
                return Request.CreateResponse(HttpStatusCode.OK);
            }
        }//tested

        //Assumes don't need this
        //Delete a setting at global level for specific entity and override all lower levels also
        /*
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
        */
        //
        //DELETE Customer Level
        //

        //Delete a setting at customer level forlist of ids and keys and override all lower levels also if true
        //**

        [Route("settings/customer/{entityIds}/{keys}/{overrideLower:bool?}")]
        public HttpResponseMessage DeleteCustomerEntitySettingOverride(string entityIds, string keys, bool overrideLower = false)
        {
            //converting string to int list
            var ListCustomers = entityIds.Split(',').ToList();
            List<int> CustomerIdS = ListCustomers
                .Select(s => Int32.TryParse(s, out int n) ? n : (int?)null)
                .Where(n => n.HasValue)
                .Select(n => n.Value)
                .ToList();
            //Split key list 
            var KeysList = keys.Split(',');

            //checking parameters
            if(CustomerIdS.Count==0||KeysList.Length==0)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, "Customer ID and Key can not be empty");
            }

            if (overrideLower)
            {
                
                foreach (var c in CustomerIdS)
                {
                    foreach (var k in KeysList)
                    {
                        //Delete corrsponding setting 
                        var Setting = settingRepository.GetCustomerSetting(c, k);
                        if (Setting!=null)
                        {
                            settingRepository.Delete(Setting.Id);
                        }
                        //overriding lower level setting
                        var SettingList = settingRepository.GetAll()
                            .Where(s => s.CustomerId == c && s.SettingKey == k && (s.Level == USER || s.Level == DEVICE)).ToList();
                        if (SettingList.Count != 0)
                        {
                            foreach (var s in SettingList)
                            {
                                settingRepository.Delete(s.Id);
                            }
                        }
                    }
                    
                }
            }
            else
            {
                foreach (var c in CustomerIdS)
                {
                    foreach (var k in KeysList)
                    {
                        var Setting = settingRepository.GetCustomerSetting(c, k);
                        if (Setting != null)
                        {
                            settingRepository.Delete(Setting.Id);
                        }
                    }
                }
            }

            return Request.CreateResponse(HttpStatusCode.OK);

        }

        /*
        //Delete a setting at customer level for list of customers
        [Route("settings/customer/{customerids}/{key}/user/{usernames}")]
        public HttpResponseMessage DeleteUserSettings(string key, int customerid, string usernames)
        {
            var setting = settingRepository.GetCustomerSetting(customerid, key);
            if (setting == null)
            {
                return Request.CreateResponse(HttpStatusCode.NotFound);
            }

            var separated = usernames.Split(',');
            Setting setting_ = null;

            if (separated.Count() > 0)
            {
                foreach (var user in separated)
                {
                    if ((setting_ = settingRepository.GetUserSetting(customerid, key, user)) == null || setting.Level != USER)
                    {
                        continue;
                    }
                    else
                    {
                        settingRepository.Delete(setting.Id);
                    }
                }
            }

            var response = Request.CreateResponse(HttpStatusCode.OK);



            return response;
        }//tested
        */
        /*
        //Delete a setting at customer level for specific user
        [Route("settings/customer/{customerid}/{username}/{key}")]
        public HttpResponseMessage DeleteCustomerEntitySetting(int customerid, string UserName, string key)
        {
            var setting = settingRepository.GetUserSetting(customerid, key, UserName);

            if (setting == null)
            {
                return Request.CreateResponse(HttpStatusCode.NotFound);
            }
            settingRepository.Delete(setting.Id);
            return Request.CreateResponse(HttpStatusCode.OK);
        }
        */

        //Delete a setting at customer level override a list of device level settings
        //settings/customer/{customerid}/{key}?deviceids=1,2,etc
        [Route("settings/customer/{customerid}/{key}/device/{deviceids}")]
        public HttpResponseMessage DeleteCustomerSettingDevices(string key, int customerid, string deviceids)
        {

            var setting = settingRepository.GetCustomerSetting(customerid, key);
            if (setting==null)
            {
                return Request.CreateResponse(HttpStatusCode.NotFound);
            }

            //separate device id string into individual strings
            var separated = deviceids.Split(',');
            

            //Post setting for each device
            //Convert device id's from CSV list to integers, one at a time and delete their corresponding entries
            foreach (var i in separated)
            {
                Int32.TryParse(i, out int dID); 
                var SettingList = settingRepository.GetAll()
                    .Where(c => c.SettingKey == key && c.DeviceId == dID && c.CustomerId == customerid&&c.Level==DEVICE);

                if(settingRepository != null)
                {
                    foreach (var set in SettingList)
                    {
                        settingRepository.Delete(set.Id);
                    }
                    
                }
            }
            return Request.CreateResponse(HttpStatusCode.OK);

        }//tested


        //Delete a setting at customer level override a list of device level settings
        //settings/customer/{customerid}/{key}?customerids=1,2,etc
        [Route("settings/customer/{customerid}/{key}/user/{userids}")]
        public HttpResponseMessage DeleteCustomerSettingCustomers(int customerid,string key,string userids)
        {
            var setting = settingRepository.GetCustomerSetting(customerid, key);
            if (setting == null)
            {
                return Request.CreateResponse(HttpStatusCode.NotFound);
            }
            var separated = userids.Split(',');
            foreach (var i in separated)
            {
                var SettingList = settingRepository.GetAll()
                    .Where(c => c.SettingKey == key && c.UserName == i && c.CustomerId == customerid && c.Level == USER);
                if (settingRepository != null)
                {
                    foreach (var set in SettingList)
                    {
                        settingRepository.Delete(set.Id);
                    }

                }
            }
            return Request.CreateResponse(HttpStatusCode.OK);
        }


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


        /*
        //Not needed? according to Victor
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
        */



        //
        //DELETE Device Level
        //


        
        //Delete a setting at device level
        [Route("settings/device/{CustomerId}/{key}/{deviceIds}")]
        public HttpResponseMessage DeleteDeviceSetting(int customerid, string deviceids, string key)
        {

            var divided = deviceids.Split(',');

            foreach (var deviceIdString in divided)
            {
                int deviceId=Int32.Parse(deviceIdString);
              
                var setting = settingRepository.GetDeviceSetting(customerid, key, deviceId);
                if (setting != null)
                {
                    settingRepository.Delete(setting.Id);
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.NotFound, "ID: " + deviceIdString + " Not found");
                }
            }
            var response = Request.CreateResponse(HttpStatusCode.OK);
            return response;
        }

        //Delete a setting at device level for specific entity id 
        [Route("settings/device/{CustomerId}/{deviceId}/{key}")]
        public HttpResponseMessage DeleteDeviceEntitySetting(int CustomerId, string key, int deviceId )
        {
            var setting = settingRepository.GetDeviceSetting(CustomerId, key, deviceId);

            if (setting == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK);
            }

            settingRepository.Delete(setting.Id);

            var response = Request.CreateResponse(HttpStatusCode.OK);
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
            
            foreach (var username in divided)
            {
                var setting = settingRepository.GetUserSetting(CustomerId, key, username);
                if (setting == null)
                {
                    return Request.CreateResponse(HttpStatusCode.NotFound);
                }
                settingRepository.Delete(setting.Id);
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