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
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Runtime.Remoting;
using System.Web;
using System.Web.Http;
using Newtonsoft.Json;
using NUnit.Framework.Internal;
using System.Threading.Tasks;

namespace DOOFUS.Tests
{
    [TestFixture]
    public class SettingsTests
    {
        SettingsController _mockSettingsController;
        Mock<ISettingsRepository> _mockSettingsRepository;
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
            _mockSettingsRepository = new Mock<ISettingsRepository>();
            _mockSettingsController = new SettingsController(_mockSettingsRepository.Object);
            _mockSettingsController.Request = new HttpRequestMessage();
            _mockSettingsController.Configuration = new HttpConfiguration();

            testSetting = new Setting { Value = "100", SettingKey = "DisplayBrightness" };
            testSetting.Id = 27;
        }

        
       
        //note: will probably end up changing this all later and making a class layer in between Test and Controller 
        [Test]
        [AsyncStateMachineAttribute(typeof(Task))]
        public async Task TestPosts()
        {
            string jsonMessage;
            //
            //Test global setting, no override
            //
            //Get the test http response back
            var response = _mockSettingsController.PostGlobalSetting(testSetting, testSetting.SettingKey, false);
            testType ="PostGlobalSetting";

            //Get the response message as a stream and parse it into a string
            using (Stream responseStream = await response.Content.ReadAsStreamAsync())
            {
                jsonMessage = new StreamReader(responseStream).ReadToEnd();
            }
            //Create a Json object based off the string so we can access the individual setting variables
            TokenResponseModel tokenResponse = (TokenResponseModel) JsonConvert.DeserializeObject(jsonMessage, typeof(TokenResponseModel));
            //If the assert finishes without error, the setting we sent matches the setting we got back
            Assert.AreEqual(testSetting.Level, tokenResponse.Level, testType);
            Assert.AreEqual(testSetting.SettingKey, tokenResponse.SettingKey, testType);
            Assert.AreEqual(testSetting.Value, tokenResponse.Value, testType);

            //
            //Test global setting, with override
            //
            response = _mockSettingsController.PostGlobalSetting(testSetting, testSetting.SettingKey, true);
            testType = "PostGlobalSettingOverride";

            //Get the response message as a stream and parse it into a string
            using (Stream responseStream = await response.Content.ReadAsStreamAsync())
            {
                jsonMessage = new StreamReader(responseStream).ReadToEnd();
            }
            //Create a Json object based off the string so we can access the individual setting variables
            tokenResponse = (TokenResponseModel)JsonConvert.DeserializeObject(jsonMessage, typeof(TokenResponseModel));
            //If the assert finishes without error, the setting we sent matches the setting we got back
            Assert.AreEqual(testSetting.Level, tokenResponse.Level, testType);
            Assert.AreEqual(testSetting.SettingKey, tokenResponse.SettingKey, testType);
            Assert.AreEqual(testSetting.Value, tokenResponse.Value, testType);

            //
            //Test global setting, multiple customers with no override
            //

            response = _mockSettingsController.PostGlobalEntitySetting(testSetting, testSetting.SettingKey, customerids, false);
            testType = "PostGlobalEntitySetting";

            //Get the response message as a stream and parse it into a string
            using (Stream responseStream = await response.Content.ReadAsStreamAsync())
            {
                jsonMessage = new StreamReader(responseStream).ReadToEnd();
            }
            //Create a Json object based off the string so we can access the individual setting variables
            tokenResponse = (TokenResponseModel)JsonConvert.DeserializeObject(jsonMessage, typeof(TokenResponseModel));
            //If the assert finishes without error, the setting we sent matches the setting we got back
            Assert.AreEqual(testSetting.Level, tokenResponse.Level, testType);
            Assert.AreEqual(testSetting.SettingKey, tokenResponse.SettingKey, testType);
            Assert.AreEqual(testSetting.Value, tokenResponse.Value, testType);
            //for loop to check customer ids...

            ////
            ////Test posting global setting for multiple customers with override
            ////
            //response = _mockSettingsController.PostGlobalEntitySetting(testSetting, testSetting.SettingKey, customerids, true);
            //testType = "PostGlobalEntitySettingOverride";

            ////This should fail, hence why we test the status code
            //var responseString = response.StatusCode.ToString();
            //Assert.AreEqual(responseString, preconditionFailed);

            //
            //Test posting user setting with no override
            //
            response = _mockSettingsController.PostUserEntitySetting(testSetting, customerid, username, testSetting.SettingKey, false);
            testType = "PostGlobalEntitySettingOverride";

            //Get the response message as a stream and parse it into a string
            using (Stream responseStream = await response.Content.ReadAsStreamAsync())
            {
                jsonMessage = new StreamReader(responseStream).ReadToEnd();
            }
            //Create a Json object based off the string so we can access the individual setting variables
            tokenResponse = (TokenResponseModel)JsonConvert.DeserializeObject(jsonMessage, typeof(TokenResponseModel));
            //If the assert finishes without error, the setting we sent matches the setting we got back
            Assert.AreEqual(testSetting.Level, tokenResponse.Level, testType);
            Assert.AreEqual(testSetting.SettingKey, tokenResponse.SettingKey, testType);
            Assert.AreEqual(testSetting.Value, tokenResponse.Value, testType);
            Assert.AreEqual(customerid.ToString(), tokenResponse.CustomerId, testType);

            //
            //Test customer setting for one or more users no override
            //
            response = _mockSettingsController.PostCustomerSetting(testSetting, customerid, testSetting.SettingKey, usernames, false);
            testType = "PostCustomerSetting";

            //Get the response message as a stream and parse it into a string
            using (Stream responseStream = await response.Content.ReadAsStreamAsync())
            {
                jsonMessage = new StreamReader(responseStream).ReadToEnd();
            }
            //Create a Json object based off the string so we can access the individual setting variables
            tokenResponse = (TokenResponseModel)JsonConvert.DeserializeObject(jsonMessage, typeof(TokenResponseModel));
            //If the assert finishes without error, the setting we sent matches the setting we got back
            Assert.AreEqual(testSetting.Level, tokenResponse.Level, testType);
            Assert.AreEqual(testSetting.SettingKey, tokenResponse.SettingKey, testType);
            Assert.AreEqual(testSetting.Value, tokenResponse.Value, testType);
            Assert.AreEqual(customerid.ToString(), tokenResponse.CustomerId, testType);

            //
            //Test customer setting for one or more users with override
            //
            response = _mockSettingsController.PostCustomerSetting(testSetting, customerid, testSetting.SettingKey, usernames, true);
            testType = "PostCustomerSettingOverride";

            //Get the response message as a stream and parse it into a string
            using (Stream responseStream = await response.Content.ReadAsStreamAsync())
            {
                jsonMessage = new StreamReader(responseStream).ReadToEnd();
            }
            //Create a Json object based off the string so we can access the individual setting variables
            tokenResponse = (TokenResponseModel)JsonConvert.DeserializeObject(jsonMessage, typeof(TokenResponseModel));
            //If the assert finishes without error, the setting we sent matches the setting we got back
            Assert.AreEqual(testSetting.Level, tokenResponse.Level, testType);
            Assert.AreEqual(testSetting.SettingKey, tokenResponse.SettingKey, testType);
            Assert.AreEqual(testSetting.Value, tokenResponse.Value, testType);
            Assert.AreEqual(customerid.ToString(), tokenResponse.CustomerId, testType);
            //For loop to check usernames...

        }

        [Test]
        [AsyncStateMachineAttribute(typeof(Task))]
        public async Task TestPuts()
        {
            _mockSettingsRepository.Setup(repo => repo.Get(It.IsAny<int>())).Returns(testSetting);

            var initialPostResponse = _mockSettingsController.PostUserEntitySetting(testSetting, customerid, username, testSetting.SettingKey, false);
            string jsonMessage; //variable to hold json response
            //Get the response message as a stream and parse it into a string
            using (Stream responseStream = await initialPostResponse.Content.ReadAsStreamAsync())
            {
                jsonMessage = new StreamReader(responseStream).ReadToEnd();
            }

            //Create a Json object based off the string so we can access the individual setting variables

            TokenResponseModel tokenResponse = (TokenResponseModel)JsonConvert.DeserializeObject(jsonMessage, typeof(TokenResponseModel));
            
            Setting updateSetting = new Setting { Value = "Updated", LastModifiedById = 1234 }; //values to update with

            //Test Put setting (global) by id
            //PutGlobalSettingById(int id, Setting setting)

            Assert.IsNotNull(initialPostResponse);

            int id = tokenResponse.Id; //id of global setting to update

            var response = _mockSettingsController.PutGlobalSettingById(id, updateSetting);
            testType = "PutSettingById";

            //Get the response message as a stream and parse it into a string
            using (Stream responseStream = await response.Content.ReadAsStreamAsync())
            {
                jsonMessage = new StreamReader(responseStream).ReadToEnd();
            }

            //Create a Json object based off the string so we can access the individual setting variables
            tokenResponse = (TokenResponseModel)JsonConvert.DeserializeObject(jsonMessage, typeof(TokenResponseModel));

            //Check if update succeeded
            Assert.AreEqual(updateSetting.Value, tokenResponse.Value, testType);
            Assert.AreEqual(updateSetting.LastModifiedById, tokenResponse.LastModifiedById, testType);
            Assert.AreEqual(updateSetting.EndEffectiveDate, tokenResponse.EndEffectiveDate, testType);

            /*


            //Put global with option to override
            //PutGlobalSetting(string key, Setting setting, bool overrideLower = false)

            //Overrride lower = false
            string key = "newGlobalSetting";
            response = _mockSettingsController.PutGlobalSetting(key, updateSetting, false);
            testType = "PutGlobalSetting";

            responseString = response.Content.ToString();
            jsonObject = JObject.Parse(responseString);

            statusCode = response.StatusCode;

            Assert.AreEqual(updateSetting.Value, (string)jsonObject.SelectToken("Value"), testType);
            Assert.AreEqual(updateSetting.LastModifiedById, (int)jsonObject.SelectToken("LastModifiedById"), testType);
            Assert.AreEqual(updateSetting.EndEffectiveDate, (DateTime)jsonObject.SelectToken("EndEffectiveDate"), testType);

            //override lower = true
            response = _mockSettingsController.PutGlobalSetting(key, updateSetting, true);
            testType = "PutGlobalSettingOverrideLower";




            //Put setting (customer) with option to override lower
            //PutCustomerSetting(string key, Setting setting, bool overrideLower = false)

            //overrideLower = false
            key = "newCustomerSetting";
            response = _mockSettingsController.PutCustomerSetting(key, updateSetting, false);
            testType = "PutGlobalSetting";

            //override lower = true
            key = "newCustomerSetting";
            response = _mockSettingsController.PutCustomerSetting(key, updateSetting, true);
            testType = "PutGlobalSettingOverrideLower";




            //Put mutliple settings (customer) with option to override lower
            //PutCustomerSettingMultiple(string key, Setting setting, string customerIds, bool overrideLower = false)

            key = "multipleCustomers";

            //override lower = false
            response = _mockSettingsController.PutCustomerSettingMultiple(key, updateSetting, customerids, false);
            testType = "PutCustomerSettingMultiple";

            //overrride lower = true
            response = _mockSettingsController.PutCustomerSettingMultiple(key, updateSetting, customerids, true);
            testType = "PutCustomerSettingMultipleOverrideLower";




            //Put setting - Specific Entity (customer) and override lower
            //PutCustomerEntitySetting(int entityId, string key, Setting setting, bool overrideLower = false)

            int entityId = 123;
            key = "customerEntitySetting";

            //override lower = false
            response = _mockSettingsController.PutCustomerEntitySetting(entityId, key, updateSetting, false);
            testType = "PutCustomerEntitySetting";

            //overrride lower = true
            response = _mockSettingsController.PutCustomerEntitySetting(entityId, key, updateSetting, true);
            testType = "PutCustomerEntitySettingOverrideLower";




            //Put setting for device
            //PutDeviceSetting(int customerId, string key, Setting setting)
            int customerId = 111;
            key = "deviceSetting";

            response = _mockSettingsController.PutDeviceSetting(customerId, key, updateSetting);
            testType = "PutDeviceSetting";
            


            //Put setting for multiple devices
            //PutDeviceSettingMultiple(int customerId, string key, string deviceIds, Setting setting)

            customerId = 123;
            key = "multipleDeviceSetting";

            response = _mockSettingsController.PutDeviceSettingMultiple(customerId, key, deviceids, updateSetting);
            testType = "PutDeviceSettingMultiple";




            //Put setting for device - Specific device
            //PutDeviceEntitySetting(int customerId, int entityId, string key, Setting setting)
            key = "deviceEntitySetting";
            entityId = 99;

            response = _mockSettingsController.PutDeviceEntitySetting(customerId, entityId, key, updateSetting);
            testType = "PutDeviceEntitySetting";




            //Put setting for user 
            //PutUserSetting(int customerId, string key, Setting setting)
            response = _mockSettingsController.PutUserSetting(customerId, key, updateSetting);
            testType = "PutUserSetting";




            //Put setting for user - Specific entity (username)
            //PutUserEntitySetting(int customerId, string entityId, string key, Setting setting)
            var entityid = "user";
            response = _mockSettingsController.PutUserEntitySetting(customerId, entityid, key, updateSetting);
            testType = "PutUserEntitySetting";




            //put setting for multiple users
            //PutUserSettingMultiple(int customerId, string key, string usernames, Setting setting)
            response = _mockSettingsController.PutUserEntitySetting(customerId, key, usernames, updateSetting);
            testType = "PutUserEntitySetting";

    */

        }

        [Test]
        public void TestGets()
        {
            
        }    

        [Test]
        public void TestDeletes()
        {
            String Key = "Key";

            //Global delete
            testType = "DeleteSingleGlobalSettingWithFalse";
            _mockSettingsController.PostGlobalSetting(testSetting, testSetting.SettingKey,
                false);
            var response = _mockSettingsController.DeleteGlobalSettingOverride(testSetting.SettingKey);
            Assert.AreEqual(HttpStatusCode.OK,response.StatusCode,testType);

            testType = "DeleteSingleGlobalSettingWithFalse(Dont exist)";
            response = _mockSettingsController.DeleteGlobalSettingOverride(testSetting.SettingKey, false);
            Assert.AreEqual(HttpStatusCode.NotFound,response.StatusCode, testType);

            testType = "DeleteSingleGlobalSettingWithFalse(No key provided)";
            response = _mockSettingsController.DeleteGlobalSettingOverride("", false);
            Assert.AreEqual(HttpStatusCode.BadRequest,response.StatusCode, testType);

            testType = "DeleteMutipleGlobalSettingWithFalse";
            _mockSettingsController.PostGlobalSetting(testSetting, testSetting.SettingKey);
            _mockSettingsController.PostGlobalSetting(testSetting, Key);
            response = _mockSettingsController.DeleteGlobalSettingOverride(testSetting.SettingKey + "," + "Key", false);
            Assert.AreEqual(HttpStatusCode.OK,response.StatusCode,testType);

            testType = "DeleteSingleGlobalSettingWithTrue";
            _mockSettingsController.PostGlobalSetting(testSetting, testSetting.SettingKey);
            _mockSettingsController.PostCustomerSetting(testSetting, customerid, testSetting.SettingKey, username,
                false);
            response = _mockSettingsController.DeleteGlobalSettingOverride(testSetting.SettingKey, true);
            Assert.AreEqual(HttpStatusCode.OK,response.StatusCode,testType);

            testType = "DeleteMutipleGlobalSettingWithTrue";
            _mockSettingsController.PostGlobalSetting(testSetting, testSetting.SettingKey);
            _mockSettingsController.PostCustomerSetting(testSetting, customerid, testSetting.SettingKey, username,
                false);
            _mockSettingsController.PostGlobalSetting(testSetting, Key);
            _mockSettingsController.PostCustomerSetting(testSetting, customerid, Key, username,
                false);
            response = _mockSettingsController.DeleteGlobalSettingOverride(testSetting.SettingKey + "," + "Key", true);
            Assert.AreEqual(HttpStatusCode.OK,response.StatusCode,testType);


            //Customer delete, override all
            testType = "DeleteCustomerSettingwithFalse(with no id)";
            response = _mockSettingsController.DeleteCustomerEntitySettingOverride("", testSetting.SettingKey, false);
            Assert.AreEqual(HttpStatusCode.BadRequest,response.StatusCode);

            testType = "DeleteCustomerSettingWithFalse(with no key)";
            response = _mockSettingsController.DeleteCustomerEntitySettingOverride(customerid.ToString(), "", false);
            Assert.AreEqual(HttpStatusCode.BadRequest,response.StatusCode);

            testType = "DeleteSingleCustomerSetting with false";
            _mockSettingsController.PostCustomerSetting(testSetting, customerid, testSetting.SettingKey, username,
                false);
            response = _mockSettingsController.DeleteCustomerEntitySettingOverride(customerid.ToString(), testSetting.SettingKey,
                false);
            Assert.AreEqual(HttpStatusCode.OK,response.StatusCode);

            testType = "DeleteMutipleCustomerSetting(different key) with false";
            _mockSettingsController.PostCustomerSetting(testSetting, customerid, testSetting.SettingKey, username,
                false);
            _mockSettingsController.PostCustomerSetting(testSetting, customerid, Key, username,
                false);
            response = _mockSettingsController.DeleteCustomerEntitySettingOverride(customerid.ToString(),
                testSetting.SettingKey + "," + Key, false);
            Assert.AreEqual(HttpStatusCode.OK,response.StatusCode,testType);

            testType = "DeleteMutipleCustomerSetting(different Id) with false";
            _mockSettingsController.PostCustomerSetting(testSetting, customerid, testSetting.SettingKey, username,
                false);
            _mockSettingsController.PostCustomerSetting(testSetting, 2, testSetting.SettingKey, username,
                false);
            response = _mockSettingsController.DeleteCustomerEntitySettingOverride(
                2.ToString() + "," + customerid.ToString(), testSetting.SettingKey, false);
            Assert.AreEqual(HttpStatusCode.OK,response.StatusCode);

            testType = "DeleteMutipleCustomerSetting(different Id and keys) with false";
            _mockSettingsController.PostCustomerSetting(testSetting, customerid, testSetting.SettingKey, username,
                false);
            _mockSettingsController.PostCustomerSetting(testSetting, customerid, Key, username,
                false);

            _mockSettingsController.PostCustomerSetting(testSetting, 2, testSetting.SettingKey, username,
                false);
            _mockSettingsController.PostCustomerSetting(testSetting, 2, Key, username,
                false);
            response = _mockSettingsController.DeleteCustomerEntitySettingOverride(
                customerid.ToString() + "," + 2.ToString(), Key + testSetting.SettingKey, false);
            Assert.AreEqual(HttpStatusCode.OK,response.StatusCode,testType);

            testType = "DeleteMutipleSetting(different Id and Keys) with true";

            _mockSettingsController.PostCustomerSetting(testSetting, customerid, testSetting.SettingKey, username,
                false);
            _mockSettingsController.PostCustomerSetting(testSetting, customerid, Key, username,
                false);
            _mockSettingsController.PostDeviceSetting(testSetting, customerid, testSetting.SettingKey, deviceid.ToString());

            _mockSettingsController.PostCustomerSetting(testSetting, 2, testSetting.SettingKey, username,
                false);
            _mockSettingsController.PostCustomerSetting(testSetting, 2, Key, username,
                false);
            _mockSettingsController.PostUserEntitySetting(testSetting, 2, username, Key, false);

            Assert.AreEqual(HttpStatusCode.OK,response.StatusCode,testType);

            //customer delete, device override

            testType = "DeleteCostomerSetting, no setting to be found";
            response = _mockSettingsController.DeleteCustomerSettingDevices(Key, customerid, deviceid.ToString());
            Assert.AreEqual(HttpStatusCode.NotFound,response.StatusCode,testType);

            testType = "DeleteCustomerSetting, override 1 setting";
            _mockSettingsController.PostCustomerSetting(testSetting, customerid, testSetting.SettingKey, username,
                false);
            _mockSettingsController.PostDeviceSetting(testSetting, customerid, testSetting.SettingKey, deviceid.ToString());
            response = _mockSettingsController.DeleteCustomerSettingDevices(testSetting.SettingKey, customerid,
                deviceid.ToString());
            Assert.AreEqual(HttpStatusCode.OK,response.StatusCode,testType);

            testType = "DeleteCustomerSetting, override mutiple setting";

            _mockSettingsController.PostCustomerSetting(testSetting, customerid, testSetting.SettingKey, username,
                false);
            _mockSettingsController.PostDeviceSetting(testSetting, customerid, testSetting.SettingKey, deviceid.ToString());
            _mockSettingsController.PostDeviceSetting(testSetting, customerid, testSetting.SettingKey, 52.ToString());
            response = _mockSettingsController.DeleteCustomerSettingDevices(testSetting.SettingKey, customerid,
                deviceid.ToString()+","+52.ToString());
            Assert.AreEqual(HttpStatusCode.OK,response.StatusCode,testType);

            //customer delete, user override

            testType = "DeleteCustomerSetting, noSetting to be found";
            response = _mockSettingsController.DeleteCustomerSettingCustomers(customerid, Key, username);
            Assert.AreEqual(HttpStatusCode.NotFound,response.StatusCode);

            testType = "DeleteCustomerSetting, override 1 setting";
            _mockSettingsController.PostCustomerSetting(testSetting, customerid, testSetting.SettingKey, username,
                false);
            _mockSettingsController.PostUserEntitySetting(testSetting, customerid, username, testSetting.SettingKey,
                false);
            response=_mockSettingsController.DeleteCustomerSettingCustomers(customerid, testSetting.SettingKey, username);
            Assert.AreEqual(HttpStatusCode.OK,response.StatusCode,testType);

            testType = "DeleteCustomerSetting, override multiple setting";
            _mockSettingsController.PostCustomerSetting(testSetting, customerid, testSetting.SettingKey, username,
                false);
            _mockSettingsController.PostUserEntitySetting(testSetting, customerid, username, testSetting.SettingKey,
                false);
            _mockSettingsController.PostUserEntitySetting(testSetting, customerid, "Victor", testSetting.SettingKey,
                false);
            response = _mockSettingsController.DeleteCustomerSettingCustomers(customerid, testSetting.SettingKey,
                username + "," + "Victor");
            Assert.AreEqual(HttpStatusCode.OK,response.StatusCode,testType);
            
            //device level delete

            testType = "DeleteDeviceSetting, no setting to be found";
            response = _mockSettingsController.DeleteDeviceSetting(customerid, deviceid.ToString(), testSetting.SettingKey);
            Assert.AreEqual(HttpStatusCode.NotFound,response.StatusCode,testType);

            testType = "DeleteDeviceSetting, 1 setting";
            _mockSettingsController.PostDeviceSetting(testSetting, customerid, testSetting.SettingKey, deviceid.ToString());
            response = _mockSettingsController.DeleteDeviceSetting(customerid, deviceid.ToString(), testSetting.SettingKey);
            Assert.AreEqual(HttpStatusCode.OK,response.StatusCode,testType);

            testType = "DeleteDeviceSetting,multipleSetting";
            _mockSettingsController.PostDeviceSetting(testSetting, customerid, testSetting.SettingKey, deviceid.ToString());
            _mockSettingsController.PostDeviceSetting(testSetting, customerid, testSetting.SettingKey, 52.ToString());
            response = _mockSettingsController.DeleteDeviceSetting(customerid, deviceid + "," + 52.ToString(),
                testSetting.SettingKey);
            Assert.AreEqual(HttpStatusCode.OK,response.StatusCode,testType);

            //user level delete

            testType = "DeleteUserSetting, no setting to be found";
            response = _mockSettingsController.DeleteUserSetting(customerid, testSetting.SettingKey, username);
            Assert.AreEqual(HttpStatusCode.NotFound,response.StatusCode,testType);

            testType = "DeleteUserSetting, 1 setting";
            _mockSettingsController.PostUserEntitySetting(testSetting, customerid, username, testSetting.SettingKey,
                false);
            response = _mockSettingsController.DeleteUserSetting(customerid, testSetting.SettingKey, username);
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode, testType);

            testType = "DeleteUserSetting, multiple setting";
            _mockSettingsController.PostUserEntitySetting(testSetting, customerid, username, testSetting.SettingKey,
                false);
            _mockSettingsController.PostUserEntitySetting(testSetting, customerid, "Victor", testSetting.SettingKey,
                false);
            response = _mockSettingsController.DeleteUserSetting(customerid, testSetting.SettingKey, username+","+"Victor");
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode, testType);
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