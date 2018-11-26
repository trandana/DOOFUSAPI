using DOOFUS.Models;
using DOOFUS.Models.Persistence;
using DOOFUS.Nhbnt.Web.Controllers;
using Moq;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;

namespace DOOFUS.Tests
{
    [TestFixture]
    public class SettingsTests
    {
        SettingsController mockSettingsController;
        Mock<ISettingsRepository> mockSettingsRepository;
        Setting testSetting;        

        //Test conditions
        private string failed;
        private string testType;

        //status codes
        private const string preconditionFailed = "412";

        private const int deviceid = 51;
        private const int customerid = 1;
        private const string customerids = "1,2,3";
        private const string deviceids = "50,12,42";
        private const  string usernames = "Jody, Victor, Phum, Alex";
        private const string username = "Jody";
        
        //Initialize mock repository and controller with a test setting
        [SetUp]
        public void Setup()
        {
            mockSettingsRepository = new Mock<ISettingsRepository>();
            mockSettingsController = new SettingsController(mockSettingsRepository.Object);

            testSetting = new Setting { Value = "100", SettingKey = "DisplayBrightness" };
        }
       
        //note: will probably end up changing this all later and making a class layer in between Test and Controller 
        [Test]
        public void TestPosts()
        {
            //
            //Test global setting, no override
            //
            var response = mockSettingsController.PostGlobalSetting(testSetting, testSetting.SettingKey, false);
            testType ="PostGlobalSetting";

            //Using Newtonsoft JSON library to parse the JSON response so we can compare
            var responseString = response.Content.ToString();            
            dynamic jsonObject = JObject.Parse(responseString);

            Assert.AreEqual(testSetting.Level, (string)jsonObject.SelectToken("Level"), testType);
            Assert.AreEqual(testSetting.Value, (string)jsonObject.SelectToken("Value"), testType);
            Assert.AreEqual(testSetting.SettingKey, (string)jsonObject.SelectToken("SettingKey"), testType);

            //
            //Test global setting, with override
            //
            response = mockSettingsController.PostGlobalSetting(testSetting, testSetting.SettingKey, true);            
            testType = "PostGlobalSettingOverride";

            //Using Newtonsoft JSON library to parse the JSON response so we can compare
            responseString = response.Content.ToString();
            jsonObject = JObject.Parse(responseString);

            Assert.AreEqual(testSetting.Level, (string)jsonObject.SelectToken("Level"), testType);
            Assert.AreEqual(testSetting.Value, (string)jsonObject.SelectToken("Value"), testType);
            Assert.AreEqual(testSetting.SettingKey, (string)jsonObject.SelectToken("SettingKey"), testType);

            //
            //Test global setting, multiple customers with no override
            //
            
            response = mockSettingsController.PostGlobalEntitySetting(testSetting, testSetting.SettingKey, customerids, false);
            testType = "PostGlobalEntitySetting";

            //Using Newtonsoft JSON library to parse the JSON response so we can compare
            responseString = response.Content.ToString();
            jsonObject = JObject.Parse(responseString);

            Assert.AreEqual(testSetting.Level, (string)jsonObject.SelectToken("Level"), testType);
            Assert.AreEqual(testSetting.Value, (string)jsonObject.SelectToken("Value"), testType);
            Assert.AreEqual(testSetting.SettingKey, (string)jsonObject.SelectToken("SettingKey"), testType);

            //
            //Test posting global setting for multiple customers with override
            //
            response = mockSettingsController.PostGlobalEntitySetting(testSetting, testSetting.SettingKey, customerids, true);
            testType = "PostGlobalEntitySettingOverride";

            //This should fail, hence why we test the status code
            responseString = response.StatusCode.ToString();
            Assert.AreEqual(responseString, preconditionFailed);

            //
            //Test posting user setting with no override
            //
            response = mockSettingsController.PostUserEntitySetting(testSetting, customerid, username, testSetting.SettingKey, false);
            testType = "PostGlobalEntitySettingOverride";
            
            responseString = response.Content.ToString();
            jsonObject = JObject.Parse(responseString);

            Assert.AreEqual(testSetting.Level, (string)jsonObject.SelectToken("Level"), testType);
            Assert.AreEqual(testSetting.Value, (string)jsonObject.SelectToken("Value"), testType);
            Assert.AreEqual(testSetting.SettingKey, (string)jsonObject.SelectToken("SettingKey"), testType);

            //
            //Test customer setting for one or more users no override
            //
            response = mockSettingsController.PostCustomerSetting(testSetting, customerid, testSetting.SettingKey, usernames, false);
            testType = "PostCustomerSetting";
            
            responseString = response.Content.ToString();
            jsonObject = JObject.Parse(responseString);

            Assert.AreEqual(testSetting.Level, (string)jsonObject.SelectToken("Level"), testType);
            Assert.AreEqual(testSetting.Value, (string)jsonObject.SelectToken("Value"), testType);
            Assert.AreEqual(testSetting.SettingKey, (string)jsonObject.SelectToken("SettingKey"), testType);
            //
            //Test customer setting for one or more users with override
            //
            response = mockSettingsController.PostCustomerSetting(testSetting, customerid, testSetting.SettingKey, usernames, true);
            testType = "PostCustomerSettingOverride";

            responseString = response.Content.ToString();
            jsonObject = JObject.Parse(responseString);

            Assert.AreEqual(testSetting.Level, (string)jsonObject.SelectToken("Level"), testType);
            Assert.AreEqual(testSetting.Value, (string)jsonObject.SelectToken("Value"), testType);
            Assert.AreEqual(testSetting.SettingKey, (string)jsonObject.SelectToken("SettingKey"), testType);

        }

        [Test]
        public void TestPuts()
        {
            Setting updateSetting = new Setting { Value = "Updated Value", LastModifiedById = 12345, EndEffectiveDate = null };

            //Test Put setting (global) by id
            //PutGlobalSettingById(int id, Setting setting)

            int id = 7; //id of global setting to update
            var response = mockSettingsController.PutGlobalSettingById(id, updateSetting);
            testType = "PutSettingById";

            //Using Newtonsoft JSON library to parse the JSON response so we can compare
            var responseString = response.Content.ToString();
            dynamic jsonObject = JObject.Parse(responseString);

            HttpStatusCode statusCode = response.StatusCode;

            Assert.AreEqual(HttpStatusCode.OK, statusCode);
            //Check if update succeeded
            Assert.AreEqual(updateSetting.Value, (string)jsonObject.SelectToken("Value"), testType);
            Assert.AreEqual(updateSetting.LastModifiedById, (int)jsonObject.SelectToken("LastModifiedById"), testType);
            Assert.AreEqual(updateSetting.EndEffectiveDate, (DateTime)jsonObject.SelectToken("EndEffectiveDate"), testType);

            


            //Put global with option to override
            //PutGlobalSetting(string key, Setting setting, bool overrideLower = false)

            //Overrride lower = false
            string key = "newGlobalSetting";
            response = mockSettingsController.PutGlobalSetting(key, updateSetting, false);
            testType = "PutGlobalSetting";

            responseString = response.Content.ToString();
            jsonObject = JObject.Parse(responseString);

            statusCode = response.StatusCode;

            Assert.AreEqual(updateSetting.Value, (string)jsonObject.SelectToken("Value"), testType);
            Assert.AreEqual(updateSetting.LastModifiedById, (int)jsonObject.SelectToken("LastModifiedById"), testType);
            Assert.AreEqual(updateSetting.EndEffectiveDate, (DateTime)jsonObject.SelectToken("EndEffectiveDate"), testType);

            //override lower = true
            response = mockSettingsController.PutGlobalSetting(key, updateSetting, true);
            testType = "PutGlobalSettingOverrideLower";




            //Put setting (customer) with option to override lower
            //PutCustomerSetting(string key, Setting setting, bool overrideLower = false)

            //overrideLower = false
            key = "newCustomerSetting";
            response = mockSettingsController.PutCustomerSetting(key, updateSetting, false);
            testType = "PutGlobalSetting";

            //override lower = true
            key = "newCustomerSetting";
            response = mockSettingsController.PutCustomerSetting(key, updateSetting, true);
            testType = "PutGlobalSettingOverrideLower";




            //Put mutliple settings (customer) with option to override lower
            //PutCustomerSettingMultiple(string key, Setting setting, string customerIds, bool overrideLower = false)

            key = "multipleCustomers";

            //override lower = false
            response = mockSettingsController.PutCustomerSettingMultiple(key, updateSetting, customerids, false);
            testType = "PutCustomerSettingMultiple";

            //overrride lower = true
            response = mockSettingsController.PutCustomerSettingMultiple(key, updateSetting, customerids, true);
            testType = "PutCustomerSettingMultipleOverrideLower";




            //Put setting - Specific Entity (customer) and override lower
            //PutCustomerEntitySetting(int entityId, string key, Setting setting, bool overrideLower = false)

            int entityId = 123;
            key = "customerEntitySetting";

            //override lower = false
            response = mockSettingsController.PutCustomerEntitySetting(entityId, key, updateSetting, false);
            testType = "PutCustomerEntitySetting";

            //overrride lower = true
            response = mockSettingsController.PutCustomerEntitySetting(entityId, key, updateSetting, true);
            testType = "PutCustomerEntitySettingOverrideLower";




            //Put setting for device
            //PutDeviceSetting(int customerId, string key, Setting setting)
            int customerId = 111;
            key = "deviceSetting";

            response = mockSettingsController.PutDeviceSetting(customerId, key, updateSetting);
            testType = "PutDeviceSetting";
            


            //Put setting for multiple devices
            //PutDeviceSettingMultiple(int customerId, string key, string deviceIds, Setting setting)

            customerId = 123;
            key = "multipleDeviceSetting";

            response = mockSettingsController.PutDeviceSettingMultiple(customerId, key, deviceids, updateSetting);
            testType = "PutDeviceSettingMultiple";




            //Put setting for device - Specific device
            //PutDeviceEntitySetting(int customerId, int entityId, string key, Setting setting)
            key = "deviceEntitySetting";
            entityId = 99;

            response = mockSettingsController.PutDeviceEntitySetting(customerId, entityId, key, updateSetting);
            testType = "PutDeviceEntitySetting";




            //Put setting for user 
            //PutUserSetting(int customerId, string key, Setting setting)
            response = mockSettingsController.PutUserSetting(customerId, key, updateSetting);
            testType = "PutUserSetting";




            //Put setting for user - Specific entity (username)
            //PutUserEntitySetting(int customerId, string entityId, string key, Setting setting)
            var entityid = "user";
            response = mockSettingsController.PutUserEntitySetting(customerId, entityid, key, updateSetting);
            testType = "PutUserEntitySetting";




            //put setting for multiple users
            //PutUserSettingMultiple(int customerId, string key, string usernames, Setting setting)
            response = mockSettingsController.PutUserEntitySetting(customerId, key, usernames, updateSetting);
            testType = "PutUserEntitySetting";
            
        }

        [Test]
        public void TestGets()
        {
            
        }    

        [Test]
        public void TestDeletes()
        {
            var response =
                mockSettingsController.PostGlobalSetting(testSetting, testSetting.SettingKey,
                    false);
            testType = "DeleteGlobalSetting";
            if (response.StatusCode!=HttpStatusCode.Created)
            {
            }
        }         

        //Check if strings are equal
        public bool isEqual( string expectedResult, string actualResult, string testType)
        {
            try
            {
                Assert.AreEqual(expectedResult, actualResult);
            }
            catch (AssertionException e)
            {                
                failed += expectedResult + " was not equal to: " + actualResult + " in " + testType;
                return false;
            }
            return true;
        }
        
    }
}