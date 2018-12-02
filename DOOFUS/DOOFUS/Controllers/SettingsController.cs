using DOOFUS.Models;
using DOOFUS.Models.Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Web.Http;
using Microsoft.Ajax.Utilities;
using NUnit.Framework;

//using System.Web.Mvc;

namespace DOOFUS.Nhbnt.Web.Controllers
{
    public class SettingsController : ApiController
    {
        public readonly ISettingsRepository settingRepository = new SettingDataRepository();

        public SettingsController() { } //Default constructor
        
        public SettingsController(ISettingsRepository testSettingRepository)
        {
            settingRepository = testSettingRepository;
        }

        public const string GLOBAL = "Global";
        public const string CUSTOMER = "Customer";
        public const string USER = "User";
        public const string DEVICE = "Device";
        public const string EXISTING_ENTRY = "The setting already exists!";
        public const string SUCCESS = "Transaction completed successfully";

        public Setting lastSettingUsed;

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
            IEnumerable<Setting> customer = settingRepository.GetCustomerSettings(customerId);

            if(customer.Count() == 0)
            {
                response.Settings = customer;
                return response;
            }

            IEnumerable<Setting> global = settingRepository.GetGlobalSetting();
            response.Settings = overrideSetting(global, customer);

            return response;
        }

        //Get all settings for a specific device
        [Route("settings/device/{customerId}/{deviceId}")]
        public GetResponse GetDeviceSettingData(int customerId, int deviceId)
        {
            var response = new GetResponse();
            response.Level = DEVICE;
            IEnumerable<Setting> device = settingRepository.GetDeviceSettings(customerId, deviceId);

            if (device.Count() == 0)
            {
                response.Settings = device;
                return response;
            }

            IEnumerable<Setting> global = settingRepository.GetGlobalSetting();
            IEnumerable<Setting> customer = settingRepository.GetCustomerSettings(customerId);

            IEnumerable<Setting> overridedCustomer = overrideSetting(global, customer);
            response.Settings = overrideSetting(overridedCustomer, device);

            return response;
        }

        //Get all settigns for a specific username
        [Route("settings/user/{customerId}/{userName}")]
        public GetResponse GetUserSettingData(int customerId, string userName)
        {
            var response = new GetResponse();
            response.Level = USER;
            IEnumerable<Setting> user = settingRepository.GetUserSettings(customerId, userName);

            if (user.Count() == 0)
            {
                response.Settings = user;
                return response;
            }

            IEnumerable<Setting> global = settingRepository.GetGlobalSetting();
            IEnumerable<Setting> customer = settingRepository.GetCustomerSettings(customerId);

            IEnumerable<Setting> overridedCustomer = overrideSetting(global, customer);
            response.Settings = overrideSetting(overridedCustomer, user);

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

        public IEnumerable<Setting> overrideSetting(IEnumerable<Setting> settings, IEnumerable<Setting> overrideSettings)
        {
            foreach (var overrideSetting in overrideSettings)
            {
                if (settings.FirstOrDefault(setting => setting.SettingKey.Equals(overrideSetting.SettingKey)) == null)
                {
                    settings = settings.Concat(new[] { overrideSetting });
                }
                else
                {
                    settings = settings.Select(setting => setting.SettingKey == overrideSetting.SettingKey ? overrideSetting : setting).ToArray();
                }
            }

            return settings;
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

        /// <summary>
        /// Post a setting (global) 
        /// </summary>
        /// <param name="key">Setting Key</param>
        /// <param name="overrideLower">example: true</param>
        /// <returns>return 200 if all good, 400 if key is empty, or 412 if setting already exists</returns>
        //Post a global setting with option to override lower levels
        //**
      
        [Route("settings/global/{key}/{overrideLower:bool?}")]
        public HttpResponseMessage PostGlobalSetting(Setting setting, string key, bool overrideLower = false)
        {
            setting.Level = GLOBAL;
            setting.SettingKey = key;
            setting.LastModifiedBy = GLOBAL;
            setting.LastModifiedTimeStamp = DateTime.UtcNow;
            setting.CreatedTimeStamp = DateTime.UtcNow;
            setting.StartEffectiveDate = DateTime.UtcNow;

            lastSettingUsed = setting;

            if (!settingRepository.DoesSettingExistAtLevel(setting)) //check if setting already exists
            {
                //Get list of existing settings which match the incoming one, if any
                var SettingList = settingRepository.GetAll()
                    .Where(c => c.SettingKey == setting.SettingKey).ToList();

                if (overrideLower)  //existing setting(s) found, lets override them and add the new one
                {
                    foreach (var c in SettingList)
                    {
                        if (c != null)
                        {
                            settingRepository.Delete(c.Id);
                        }
                    }
                }

                //add the new setting
                settingRepository.Add(setting);

                //return response
                var response = Request.CreateResponse<Setting>(HttpStatusCode.Created, setting);
               // response.Content = new StringContent(SUCCESS, Encoding.UTF8, "application/json");

                return response;
            }
            else //setting already exists, POST failed
            {
                //return response
                var response = Request.CreateResponse<Setting>(HttpStatusCode.PreconditionFailed, setting);
                response.Content = new StringContent(EXISTING_ENTRY);               

                return response;
            }
        }

        /// <summary>
        /// Post a setting (global) 
        /// </summary>
        /// <param name="key">Setting Key</param>
        /// <param name="overrideLower">example: true</param>
        /// /// <param name="customerids">example: 1,2,3</param>
        /// <returns>return 200 if all good, 400 if key is empty, or 412 if setting already exists</returns>
        ///Post a setting (global) to multiple customers - with option to override
        ///settings/global/{key}?customerids=1,2,50, etc
        //**

        [Route("settings/global/{key}/{customerids}/{overrideLower:bool?}")]
        public HttpResponseMessage PostGlobalEntitySetting(Setting setting, string key, string customerids, bool overrideLower = false)
        {
            setting.Level = GLOBAL;
            setting.LastModifiedBy = GLOBAL;
            setting.SettingKey = key;
            setting.LastModifiedTimeStamp = DateTime.UtcNow;
            setting.CreatedTimeStamp = DateTime.UtcNow;
            setting.StartEffectiveDate = DateTime.UtcNow;

            if (!settingRepository.DoesSettingExistAtLevel(setting)) //check if setting already exists
            {
                //separate customer id string into individual ints
                string[] separated = customerids.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
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
                            if (c != null)
                            {
                                settingRepository.Delete(c.Id); //delete the old ones
                            }
                        }
                        //assign a customer id to the setting in each iteration before saving the new setting
                        setting.CustomerId = parsed;
                        settingRepository.Add(setting);
                    }

                }
                //Override was not specified, don't override lower levels
                else
                {
                    int x = 0;
                    //grab each customer id, put it into the setting and save the new setting for each customer
                    foreach (var id in separated)
                    {
                        if (Int32.TryParse(id, out x))
                        {
                            setting.CustomerId = x;
                            settingRepository.Add(setting);
                        }
                    }
                }
                //return success
                var response = Request.CreateResponse<Setting>(HttpStatusCode.Created, setting);
                //response.Content = new StringContent(SUCCESS, Encoding.UTF8, "application/json");

                return response;
            }
            //return failed
            else
            {
                var response = Request.CreateResponse<Setting>(HttpStatusCode.PreconditionFailed, setting);
                response.Content = new StringContent(EXISTING_ENTRY);

                return response;
            }
        }

        //
        //POST Customer Level
        //

        /// <summary>
        /// Post a setting (global) 
        /// </summary>
        /// <param name="customerid">Which Customer is it For</param>
        /// <param name="overrideLower">example: true</param>
        /// /// <param name="customerids">example: 1,2,3</param>
        /// <returns>return 200 if all good, 400 if key is empty, or 412 if setting already exists</returns>
        ///Post a setting for one or more users or devices (entityid can be either) and optionally override lower levels
        ///If device id's were sent, override lower does nothing since we are already at the lowest level
        ///settings/customer/{customerid}/{key}{entityids}/{overrideLower:bool?}
        //**
        [Route("settings/customer/{customerid}/{key}/{entityids}/{overrideLower:bool?}")]
        public HttpResponseMessage PostCustomerSetting(Setting setting, int customerid, string key, string entityids, bool overrideLower = false)
        {
            //Set setting parameters
            setting.Level = CUSTOMER;
            setting.SettingKey = key;
            setting.CustomerId = customerid;
            setting.LastModifiedById = customerid;
            setting.LastModifiedTimeStamp = DateTime.UtcNow;
            setting.CreatedTimeStamp = DateTime.UtcNow;
            setting.StartEffectiveDate = DateTime.UtcNow;

            if (!settingRepository.DoesSettingExist(setting)) //check if setting already exists. 
            {
                settingRepository.Add(setting); //add the customer level first
                string[] separated = entityids.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

                //check if entity id's which were sent are device id's or usernames
                //by parsing the CSV list. If the parse fails we know its a username
                //No need to check for override if its a device, since device is already lowest level
                int parsed = 0;
                if (int.TryParse(separated.ElementAt(0), out parsed))
                {
                     setting.Level = DEVICE;
                    
                     //For each device, add the setting by parsing the device list into individual ints
                     for (int i = 0; i < separated.Count(); i++)
                     {
                         Int32.TryParse(separated.ElementAt(i), out parsed);
                         setting.DeviceId = parsed;
                         settingRepository.Add(setting);
                     }        
                    //return success response
                    var response = Request.CreateResponse<Setting>(HttpStatusCode.Created, setting);

                    return response;
                }
                else //couldn't assign int, must be usernames
                {
                    //override was specified, so we override this setting for each user and for each device in the lower levels
                    if (overrideLower)
                    {
                        setting.Level = USER;

                        //override this setting for each user specified in the CSV list
                        for (int i = 0; i < separated.Count(); i++)
                        {
                            var ExistingSetting = settingRepository.GetUserSetting(customerid, key, separated.ElementAt(i));

                            if (ExistingSetting != null)
                            {
                                settingRepository.Delete(ExistingSetting.Id);
                            }
                            
                        }

                        //User level overwritten, now lets overwrite device level too
                        var SettingList = settingRepository.GetAll()
                         .Where(c => c.SettingKey == key && c.Level == DEVICE && c.CustomerId == customerid).ToList();

                        setting.Level = DEVICE;

                        foreach (var c in SettingList)
                        {
                            if (c != null)
                            {
                                settingRepository.Delete(c.Id);
                            }
                        }
                    }
                    else
                    {
                        setting.Level = USER;

                        //add setting for each user specified in the CSV list
                        for (int i = 0; i < separated.Count(); i++)
                        {
                            setting.UserName = separated.ElementAt(i);
                            settingRepository.Add(setting);
                        }
                    }
                    //override wasn't specified, we are done here                
                    var response = Request.CreateResponse<Setting>(HttpStatusCode.Created, setting);
                    //response.Content = new StringContent(SUCCESS, Encoding.UTF8, "application/json");

                    return response;
                }
            }
            else //The setting already exists at customer level 
            {                
                var response = Request.CreateResponse<Setting>(HttpStatusCode.PreconditionFailed, setting);
                response.Content = new StringContent(EXISTING_ENTRY);               

                return response;
            }
        }
        //
        //POST User Level
        //  
        /// <summary>
        /// Post a setting (global) 
        /// </summary>
        /// <param name="customerid">Which Customer is it For</param>
        /// <param name="username">Which user this is for. Example: Jason</param>
        /// <param name="key">Setting Key</param>
        /// <param name="overrideLower">If you want to override lower levels. Example: True</param>
        /// <returns>return 200 if all good, 400 if key is empty, or 412 if setting already exists</returns>
        ///Post a user setting for specific user with option to  override lower
        //**

        [Route("settings/user/{customerid}/{username}/{key}/{overrideLower:bool?}")]
        public HttpResponseMessage PostUserEntitySetting(Setting setting, int customerid, string username, string key, bool overrideLower = false)
        {
            setting.Level = USER;
            setting.CustomerId = customerid;
            setting.SettingKey = key;
            setting.UserName = username;
            setting.LastModifiedBy = username;
            setting.LastModifiedTimeStamp = DateTime.UtcNow;
            setting.CreatedTimeStamp = DateTime.UtcNow;
            setting.StartEffectiveDate = DateTime.UtcNow;

            if (!settingRepository.DoesSettingExistAtLevel(setting)) //check if setting already exists
            {                
                //should override lower levels
                if (overrideLower)
                {
                    //override setting for all devices used by this user           
                    var SettingList = settingRepository.GetAll()
                      .Where(c => c.SettingKey == key && c.CustomerId == customerid && c.UserName == username && (c.Level == USER || c.Level == DEVICE)).ToList();

                    //Override user level and device level 
                    if (SettingList.Count() > 0)
                    {
                        foreach (var c in SettingList)
                        {
                            if (c != null)
                            {
                                settingRepository.Delete(c.Id);
                            }
                        }
                    }
                }
                else //don't override, just update this level
                {
                    settingRepository.Add(setting);
                }

                var response = Request.CreateResponse<Setting>(HttpStatusCode.Created, setting);
               // response.Content = new StringContent(SUCCESS, Encoding.UTF8, "application/json");

                return response;
            }
            else //setting exists already at this level
            {                
                var response = Request.CreateResponse<Setting>(HttpStatusCode.PreconditionFailed, setting);
                response.Content = new StringContent(EXISTING_ENTRY);

                return response;
            }
        }

        //
        //POST Device Level
        //
        
        /// <summary>
        /// Post a setting (global) 
        /// </summary>
        /// <param name="customerid">Which Customer is it For</param>
        /// <param name="key">Setting Key</param>
        /// <param name="deviceids">Can submit one or multiple device ids separated by commas. Example: 1,2,3 </param>
        /// <returns>Post device settings for one or more specific device ids
        ///settings/device/{customerid}/{key}?deviceids=1,2,50, etc
        //**
        //
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

            if (!settingRepository.DoesSettingExistAtLevel(setting)) //check if setting already exists
            {
                //Split CSV list, discard empty entries
                string[] separated = deviceids.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);                
                
                int parsed = 0;
                foreach (var c in separated)
                {
                    int.TryParse(c, out parsed);
                    setting.DeviceId = parsed;
                    settingRepository.Add(setting);
                }             

                var response = Request.CreateResponse<Setting>(HttpStatusCode.Created, setting);
                //response.Content = new StringContent(SUCCESS, Encoding.UTF8, "application/json");

                return response;
            }
            else
            {               
                var response = Request.CreateResponse<Setting>(HttpStatusCode.PreconditionFailed, setting);
                response.Content = new StringContent(EXISTING_ENTRY);

                return response;
            }
        }

        //
        // PUT
        //


        //function that is used throughout PUT calls to update a setting object before 
        //updating the database.
        public Setting UpdateSetting(Setting current, Setting setting)
        {
            if (setting.CustomerId != current.CustomerId && setting.CustomerId != null)
            {
                current.CustomerId = setting.CustomerId;
            }

            if (setting.DeviceId != current.DeviceId && setting.DeviceId != null)
            {
                current.DeviceId = setting.DeviceId;
            }

            if (setting.UserName != current.UserName && setting.UserName != null)
            {
                current.UserName = setting.UserName;
            }

            if (setting.StartEffectiveDate != current.StartEffectiveDate && setting.StartEffectiveDate != null)
            {
                current.StartEffectiveDate = setting.StartEffectiveDate;
            }

            if (setting.LastModifiedBy != current.LastModifiedBy && setting.LastModifiedBy != null)
            {
                current.LastModifiedBy = setting.LastModifiedBy;
            }

            if (setting.LastModifiedById != current.LastModifiedById && setting.LastModifiedById != null)
            {
                current.LastModifiedById = setting.LastModifiedById;
            }

            if (setting.Value != current.Value && setting.Value != null)
            {
                current.Value = setting.Value;
            }

            current.EndEffectiveDate = null;
            current.LastModifiedTimeStamp = DateTime.UtcNow;

            return current;
        }

        //
        //PUT Global Level
        //

        /// <summary>
        /// Update setting by setting ID. Works with any level and can update any setting by ID.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="setting"></param>
        /// <returns>Returns 200 on succesful update and returns 400 on unsuccessful update. Returns 404 if setting not found.</returns>

        //Put setting by id
        [Route("settings/global/{id}")]
        public HttpResponseMessage PutGlobalSettingById(int id, Setting setting)
        {
            var currentSetting = new Setting();
            //get setting to update by id
            try
            {
                currentSetting = settingRepository.Get(id);
            }
            catch (NullReferenceException)
            {
                var notFoundResponse = Request.CreateResponse(HttpStatusCode.NotFound, "Setting not found");
                return notFoundResponse;
            }
            Assert.IsNotNull(currentSetting);
            //replace current setting values with new setting values
            currentSetting = UpdateSetting(currentSetting, setting);

            //update setting
            if (!settingRepository.Update(currentSetting))
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, "Update error");
            }

            var updatedResponse = Request.CreateResponse(HttpStatusCode.OK, currentSetting);
            return updatedResponse;
        }

        /// <summary>
        /// Update global setting by setting key with option to override lower settings.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="setting"></param>
        /// <param name="overrideLower"></param>
        /// <returns>Returns 200 on succesful update and returns 400 on unsuccessful update. Returns 404 if setting not found.</returns>

        //Put global with option to override
        [Route("settings/global/{key}/{overrideLower?}")]
        public HttpResponseMessage PutGlobalSetting(string key, Setting setting, bool overrideLower = false)
        {
            var currentSetting = new Setting();
            
            try
            {
                currentSetting = currentSetting = settingRepository.GetAll().Where(x => x.SettingKey == key && x.Level == GLOBAL).ToList().FirstOrDefault<Setting>();
            }
            catch (NullReferenceException)
            {
                var notFoundResponse = Request.CreateResponse(HttpStatusCode.NotFound, "Setting not found");
                return notFoundResponse;
            }

            //replace current setting values with new values if applicable
            currentSetting = UpdateSetting(currentSetting, setting);

            //try updating setting
            if (!settingRepository.Update(currentSetting))
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, "Update error");
            }

            if (overrideLower)
            {
                //get list of all lower level settings with specified key
                var listOfCurrentSettings = settingRepository.GetAll()
                    .Where(x => x.SettingKey == key &&
                    x.Level == CUSTOMER || x.Level == DEVICE || x.Level == USER).ToList();

                if (listOfCurrentSettings.Contains(null))
                {
                    var notFoundResponse = Request.CreateResponse(HttpStatusCode.OK, "Updated setting. Unable to find lower level settings to overrride.");
                    return notFoundResponse;
                }

                //Delete settings in list
                for (int i = 0; i < listOfCurrentSettings.Count(); i++)
                {
                    settingRepository.Delete(listOfCurrentSettings[i].Id);
                }

                //Create HTTP response for overriding lower
                var overrideResponse = Request.CreateResponse(HttpStatusCode.OK, currentSetting, "Override successful");
                overrideResponse.Content = new StringContent(SUCCESS, Encoding.UTF8, "application/json");
                return overrideResponse;
            }
            //Create HTTP response
            var updatedResponse = Request.CreateResponse(HttpStatusCode.OK, currentSetting);
            updatedResponse.Content = new StringContent(SUCCESS, Encoding.UTF8, "application/json");
            return updatedResponse;
        }

        //
        //PUT Customer Level
        //

        /// <summary>
        /// Update customer setting by setting key with option to override lower settings.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="setting"></param>
        /// <param name="overrideLower"></param>
        /// <returns>Returns 200 on succesful update and returns 400 on unsuccessful update.</returns>
        //Put setting (customer) with option to override lower
        [Route("settings/customer/{key}/{overrideLower?}")]
        public HttpResponseMessage PutCustomerSetting(string key, Setting setting, bool overrideLower = false)
        {
            var currentSetting = settingRepository.GetAll().Where(x => x.SettingKey == key && x.Level == CUSTOMER).ToList().FirstOrDefault<Setting>();

            if (currentSetting == null)
            {
                var notFoundResponse = Request.CreateResponse(HttpStatusCode.BadRequest);
                return notFoundResponse;
            }

            //replace current setting values with new values if applicable
            currentSetting = UpdateSetting(currentSetting, setting);

            if (!settingRepository.Update(currentSetting))
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, "Update error");
            }

            if (overrideLower)
            {
                //get list of all setting with specified key
                var listOfCurrentSettings = settingRepository.GetAll()
                    .Where(x => x.SettingKey == key &&
                    x.Level == DEVICE || x.Level == USER).ToList();

                if (listOfCurrentSettings.Contains(null))
                {
                    var notFoundResponse = Request.CreateResponse(HttpStatusCode.OK, "Updated setting. Unable to find lower level settings to delete.");
                    return notFoundResponse;
                }

                //Delete settings in list
                for (int i = 0; i < listOfCurrentSettings.Count(); i++)
                {
                    settingRepository.Delete(listOfCurrentSettings[i].Id);
                }

                //Create HTTP response for overriding lower
                var overrideResponse = Request.CreateResponse(HttpStatusCode.OK, currentSetting, "Override successful");
                overrideResponse.Content = new StringContent(SUCCESS, Encoding.UTF8, "application/json");
                return overrideResponse;
            }

            //Create HTTP response
            var updatedResponse = Request.CreateResponse(HttpStatusCode.OK, currentSetting);
            updatedResponse.Content = new StringContent(SUCCESS, Encoding.UTF8, "application/json");
            return updatedResponse;
        }

        /// <summary>
        /// Update multiple customer settings by setting key with option to override lower settings.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="setting"></param>
        /// <param name="customerIds"></param>
        /// <param name="overrideLower"></param>
        /// <returns>Returns 200 on successful update and returns 400 on unsuccessful update.</returns>
        //Put multiple settings (customer) with option to override lower
        //Can update multiple settings by customerIds (csv list of ids)
        [Route("settings/customer/{key}/{customerIds}/{overrideLower?}")]
        public HttpResponseMessage PutCustomerSettingMultiple(string key, Setting setting, string customerIds, bool overrideLower = false)
        {
            string[] separated = customerIds.Split(','); //hold seperated customer IDs
            int[] cIds = new int[separated.Count()]; //place parsed ids in this array

            for (int i = 0; i < separated.Count(); i++) //convert strings to ints and place in array
            {
                cIds[i] = Convert.ToInt32(separated[i]);
            }

            var currentSettings = new List<Setting>(); //new list to store settings


            //add each setting  to array that is specific to the customer Ids entered
            for (int j = 0; j < separated.Count(); j++)
            {
                currentSettings.Add(settingRepository.GetAll().Where(x => x.SettingKey == key
                && x.Level == CUSTOMER && x.CustomerId == cIds[j]).ToList().FirstOrDefault<Setting>());
            }

            if (currentSettings.Contains(null))
            {
                var notFoundResponse = Request.CreateResponse(HttpStatusCode.BadRequest, "Setting not found");
                return notFoundResponse;
            }

            for (int k = 0; k < currentSettings.Count(); k++)
            {
                //replace current setting values with new setting values
                currentSettings[k] = UpdateSetting(currentSettings[k], setting);

                if (!settingRepository.Update(currentSettings[k]))
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "Update error");
                }

            }

            if (overrideLower)
            {
                var currentSettingsToOverride = new List<Setting>(); //new list to store settings

                //add lower level settings to currentSettings list
                for (int j = 0; j < separated.Count(); j++)
                {
                    currentSettingsToOverride.Add(settingRepository.GetAll().Where(x => x.SettingKey == key &&
                    x.Level == DEVICE && x.CustomerId == cIds[j]).ToList().FirstOrDefault<Setting>());

                    currentSettingsToOverride.Add(settingRepository.GetAll().Where(x => x.SettingKey == key &&
                    x.Level == USER && x.CustomerId == cIds[j]).ToList().FirstOrDefault<Setting>());
                }

                if (currentSettingsToOverride.Contains(null))
                {
                    var notFoundResponse = Request.CreateResponse(HttpStatusCode.OK, "Updated setting. Unable to find lower level settings to delete.");
                    return notFoundResponse;
                }

                //delete setting objects in list from db
                for (int k = 0; k < currentSettingsToOverride.Count(); k++)
                {
                    settingRepository.Delete(currentSettingsToOverride[k].Id);
                }

                //Create HTTP response for overriding lower
                var overrideResponse = Request.CreateResponse<List<Setting>>(HttpStatusCode.OK, currentSettings, "Override successful");
                overrideResponse.Content = new StringContent(SUCCESS, Encoding.UTF8, "application/json");
                return overrideResponse;

            }

            //Create HTTP response
            var updatedResponse = Request.CreateResponse<List<Setting>>(HttpStatusCode.OK, currentSettings);
            updatedResponse.Content = new StringContent(SUCCESS, Encoding.UTF8, "application/json");
            return updatedResponse;
        }


        /// <summary>
        /// Update customer setting by entity ID (customer ID), and setting key with option to override lower settings.
        /// </summary>
        /// <param name="entityId"></param>
        /// <param name="key"></param>
        /// <param name="setting"></param>
        /// <param name="overrideLower"></param>
        /// <returns>Returns 200 on succesful update and returns 400 on unsuccessful update.</returns>
        //Put setting - Specific Entity (customer) and override lower
        [Route("settings/customer/{entityId}/{key}/{overrideLower?}")]
        public HttpResponseMessage PutCustomerEntitySetting(int entityId, string key, Setting setting, bool overrideLower = false)
        {
            var currentSetting = settingRepository.GetAll().Where(x => x.CustomerId == entityId && x.SettingKey == key
            && x.Level == CUSTOMER).FirstOrDefault<Setting>();

            if (currentSetting == null)
            {
                var notFoundResponse = Request.CreateResponse(HttpStatusCode.BadRequest);
                return notFoundResponse;
            }

            //replace current setting values with new setting values
            currentSetting = UpdateSetting(currentSetting, setting);

            if (!settingRepository.Update(currentSetting))
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, "Update error");
            }

            if (overrideLower)
            {
                var listOfCurrentSettings = settingRepository.GetAll().Where(x => x.DeviceId == entityId && x.SettingKey == key
                && x.Level == DEVICE || x.Level == USER).ToList();

                if (listOfCurrentSettings.Count() == 0)
                {
                    var notFoundResponse = Request.CreateResponse(HttpStatusCode.BadRequest);
                    return notFoundResponse;
                }

                for (int i = 0; i < listOfCurrentSettings.Count; i++)
                {
                    settingRepository.Delete(listOfCurrentSettings[i].Id);
                }

                //Create HTTP response for overriding lower
                var overrideResponse = Request.CreateResponse(HttpStatusCode.OK, currentSetting, "Override successful");
                overrideResponse.Content = new StringContent(SUCCESS, Encoding.UTF8, "application/json");
                return overrideResponse;

            }
            var response = Request.CreateResponse(HttpStatusCode.OK, currentSetting);
            response.Content = new StringContent(SUCCESS, Encoding.UTF8, "application/json");
            return response;
        }

        //
        //PUT Device Level      
        //

        /// <summary>
        /// Update device setting by customer ID and setting key.
        /// </summary>
        /// <param name="customerId"></param>
        /// <param name="key"></param>
        /// <param name="setting"></param>
        /// <returns>Returns 200 on succesful update and returns 400 on unsuccessful update.</returns>
        //Put setting for device
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

            //replace current setting values with new setting values
            currentSetting = UpdateSetting(currentSetting, setting);

            if (!settingRepository.Update(currentSetting))
            {
                throw new HttpResponseException(HttpStatusCode.BadRequest);
            }

            //Create HTTP response
            var updatedResponse = Request.CreateResponse<Setting>(HttpStatusCode.OK, currentSetting);
            updatedResponse.Content = new StringContent(SUCCESS, Encoding.UTF8, "application/json");
            return updatedResponse;
        }

        /// <summary>
        /// Update multiple device settings by customer ID and setting key.
        /// </summary>
        /// <param name="customerId"></param>
        /// <param name="key"></param>
        /// <param name="deviceIds">Example: 123,456 </param>
        /// <param name="setting"></param>
        /// <returns>Returns 200 on succesful update and returns 400 on unsuccessful update.</returns>
        //Put setting for multiple devices
        [Route("settings/device/{customerId}/{key}/{deviceIds}")]
        public HttpResponseMessage PutDeviceSettingMultiple(int customerId, string key, string deviceIds, Setting setting)
        {
            string[] separated = deviceIds.Split(','); //hold seperated customer IDs
            int[] dIds = new int[separated.Count()]; //place parsed ids in this array

            for (int i = 0; i < separated.Count(); i++) //convert strings to ints and place in array
            {
                dIds[i] = Convert.ToInt32(separated[i]);
            }

            var currentSettings = new List<Setting>(); //new list to store settings

            for (int i = 0; i < dIds.Count(); i++) //place all settings to modify in a list
            {
                var settingToAdd = settingRepository.GetAll().Where(x => x.SettingKey == key
                && x.Level == DEVICE && x.DeviceId == dIds[i]).ToList().FirstOrDefault<Setting>();
                currentSettings.Add(settingToAdd);
            }

            if (currentSettings.Count() == 0)
            {
                var notFoundResponse = Request.CreateResponse(HttpStatusCode.BadRequest);
                return notFoundResponse;
            }

            for (int j = 0; j < currentSettings.Count(); j++) //go through each setting in list and perform nessesary changes
            {
                //replace current setting values with new setting values
                currentSettings[j] = UpdateSetting(currentSettings[j], setting);

                if (!settingRepository.Update(currentSettings[j]))
                {
                    throw new HttpResponseException(HttpStatusCode.NotFound);
                }

            }

            //Create HTTP response
            var updatedResponse = Request.CreateResponse<List<Setting>>(HttpStatusCode.OK, currentSettings);
            updatedResponse.Content = new StringContent(SUCCESS, Encoding.UTF8, "application/json");
            return updatedResponse;
        }

        /// <summary>
        /// Update device setting by customer ID, entity ID (device ID), and setting key.
        /// </summary>
        /// <param name="customerId"></param>
        /// <param name="entityId"></param>
        /// <param name="key"></param>
        /// <param name="setting"></param>
        /// <returns>Returns 200 on succesful update and returns 400 on unsuccessful update.</returns>
        //Put setting for device - Specific device
        [Route("settings/device/{customerId}/{entityId}/{key}")]
        public HttpResponseMessage PutDeviceEntitySetting(int customerId, int entityId, string key, Setting setting)
        {
            var currentSetting = settingRepository.GetAll().Where(x => x.CustomerId == customerId
            && x.DeviceId == entityId && x.SettingKey == key && x.Level == DEVICE).FirstOrDefault<Setting>();

            if (currentSetting == null)
            {
                var notFoundResponse = Request.CreateResponse(HttpStatusCode.BadRequest);
                return notFoundResponse;
            }

            //replace current setting values with new setting values
            currentSetting = UpdateSetting(currentSetting, setting);

            if (!settingRepository.Update(currentSetting))
            {
                throw new HttpResponseException(HttpStatusCode.BadRequest);
            }

            //Create HTTP response
            var updatedResponse = Request.CreateResponse<Setting>(HttpStatusCode.OK, currentSetting);
            updatedResponse.Content = new StringContent(SUCCESS, Encoding.UTF8, "application/json");
            return updatedResponse;
        }


        //
        //PUT User Level
        //

        /// <summary>
        /// Update setting at user level for specific customer ID and setting key.
        /// </summary>
        /// <param name="customerId"></param>
        /// <param name="key"></param>
        /// <param name="setting"></param>
        /// <returns>Returns 200 on succesful update and returns 400 on unsuccessful update.</returns>
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

            //replace current setting values with new setting values
            currentSetting = UpdateSetting(currentSetting, setting);

            if (!settingRepository.Update(currentSetting))
            {
                throw new HttpResponseException(HttpStatusCode.BadRequest);
            }

            //Create HTTP response
            var updatedResponse = Request.CreateResponse<Setting>(HttpStatusCode.OK, currentSetting);
            updatedResponse.Content = new StringContent(SUCCESS, Encoding.UTF8, "application/json");
            return updatedResponse;
        }

        /// <summary>
        /// Update setting at user level for specific customer ID, entity ID (username), and setting key
        /// </summary>
        /// <param name="customerId"></param>
        /// <param name="entityId"></param>
        /// <param name="key"></param>
        /// <param name="setting"></param>
        /// <returns>Returns 200 on succesful update and returns 400 on unsuccessful update.</returns>

        //Put setting for user - Specific entity (username)
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

            //replace current setting values with new setting values
            currentSetting = UpdateSetting(currentSetting, setting);

            if (!settingRepository.Update(currentSetting))
            {
                throw new HttpResponseException(HttpStatusCode.BadRequest);
            }

            //Create HTTP response
            var updatedResponse = Request.CreateResponse<Setting>(HttpStatusCode.OK, currentSetting);
            updatedResponse.Content = new StringContent(SUCCESS, Encoding.UTF8, "application/json");
            return updatedResponse;
        }

        /// <summary>
        /// Update settings at user level for multiple users by customer ID and setting key.
        /// </summary>
        /// <param name="customerId"></param>
        /// <param name="key"></param>
        /// <param name="usernames">Example: user1, user2, user3</param>
        /// <param name="setting"></param>
        /// <returns>Returns 200 on succesful update and returns 400 on unsuccessful update.</returns>

        //put setting for multiple users

        //NOTE: the route for this function call has been adjusted to "users" rather than "user". This call collides with the call above.
        [Route("settings/users/{customerId}/{key}/{usernames}")]
        public HttpResponseMessage PutUserSettingMultiple(int customerId, string key, string usernames, Setting setting)
        {
            string[] separated = usernames.Split(','); //hold seperated usernames

            var currentSettings = new List<Setting>(); //new list to store settings

            for (int i = 0; i < separated.Count(); i++) //place all settings to modify in a list
            {
                var settingToAdd = settingRepository.GetAll().Where(x => x.SettingKey == key
                && x.Level == USER && x.UserName == separated[i]).ToList().FirstOrDefault<Setting>();

                currentSettings.Add(settingToAdd);
            }

            if (currentSettings.Contains(null))
            {
                var notFoundResponse = Request.CreateResponse(HttpStatusCode.BadRequest);
                return notFoundResponse;
            }

            for (int j = 0; j < currentSettings.Count(); j++) //go through each setting in list and perform nessesary changes
            {
                //replace current setting values with new setting values
                currentSettings[j] = UpdateSetting(currentSettings[j], setting);

                if (!settingRepository.Update(currentSettings[j]))
                {
                    var updateErrorResponse = Request.CreateResponse(HttpStatusCode.BadRequest, "Update error.");
                    return updateErrorResponse;
                }

            }

            //Create HTTP response
            var updatedResponse = Request.CreateResponse<List<Setting>>(HttpStatusCode.OK, currentSettings);
            updatedResponse.Content = new StringContent(SUCCESS, Encoding.UTF8, "application/json");
            return updatedResponse;
        }
        //
        //DELETE
        //

        //
        //DELETE Global Level
        //
        /// <summary>
        /// To delete a single or mutiple settings at gloabal level. with options to override lower level
        /// </summary>
        /// <param name="keys">Example:key1,key2</param>
        /// <param name="overrideLower">example:true</param>
        /// <returns>return 200 if pass, 400 if empty keys, 404 if no setting was found</returns>
        /// 
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

        //
        //DELETE Customer Level
        //

        /// <summary>
        /// Delete a setting at customer level for list of ids and keys and override all lower levels also if true
        /// </summary>
        /// <param name="entityIds">CustomerIds: 1,2,3</param>
        /// <param name="keys">Keys: key1,key2</param>
        /// <param name="overrideLower">example: true</param>
        /// <returns>return 200 if all good, 400 if customer or key is empty</returns>
        //Delete a setting at customer level for list of ids and keys and override all lower levels also if true
        //**

        [Route("settings/customer/{entityIds}/{keys}/{overrideLower:bool?}")]
        public HttpResponseMessage DeleteCustomerEntitySettingOverride(string entityIds, string keys, bool overrideLower = false)
        {
            //converting string to int list
            //filter out strings which can not be converted
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
            //else only delete customer level settings
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

            var response = Request.CreateResponse(HttpStatusCode.OK);
            response.Content = new StringContent(SUCCESS, Encoding.UTF8, "application/json");

            return Request.CreateResponse(HttpStatusCode.OK);

        }


        //Delete a setting at customer level override a list of device level settings
        /// <summary>
        /// Delete settings at customer level and override a list of device level settings
        /// </summary>
        /// <param name="key">keys to override: key1,key2</param>
        /// <param name="customerid">customerids: 1</param>
        /// <param name="deviceids">deviceids to override: 1,2</param>
        /// <returns>return 200 if all good, 404 if no corresponding setting was found </returns>
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
            int dID = 0;
            foreach (var i in separated)
            {
                Int32.TryParse(i, out dID); 
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
        /// <summary>
        /// Delete a setting at customer level override a list of device level settings
        /// </summary>
        /// <param name="customerid">customerId: 1</param>
        /// <param name="key">key for that setting: key</param>
        /// <param name="userids">userIds to override: 1</param>
        /// <returns>return 200 if all good, 400 if corresponding setting do not exist</returns>
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

        //
        //DELETE Device Level
        //
        /// <summary>
        /// delete settings at device level
        /// </summary>
        /// <param name="customerid">customerId to delete: 1</param>
        /// <param name="deviceids">deviceIds to delete: 1,2,3</param>
        /// <param name="key">key to delete</param>
        /// <returns>return 200 if all is good, 400 if a deviceId can not be found</returns>
        //Delete settings at device level
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
            response.Content = new StringContent(SUCCESS, Encoding.UTF8, "application/json");
            return response;
        }


        //
        //DELETE User Level
        //

        //Delete mutiple setting at user level with usernames
        //Assume usernames is string and divided by ',', ie: jody,alex
        /// <summary>
        /// delete settings at user level
        /// </summary>
        /// <param name="CustomerId">customerId to delete: 1</param>
        /// <param name="key">key to delete: key </param>
        /// <param name="usernames">usernames to delete: jody,alex</param>
        /// <returns>returns 200 if all is good, 400 if a username can not be founds</returns>
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
            
            var response = Request.CreateResponse(HttpStatusCode.OK, "DELETED");
            response.Content = new StringContent(SUCCESS, Encoding.UTF8, "application/json");
            return response;//delete the setting and return it
        }

    }
}