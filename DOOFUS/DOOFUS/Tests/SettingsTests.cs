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
using System.Web.Http.Routing;
using NHibernate.Criterion;

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
        private const string usernames = "Jody,Victor,Phum,Alex";
        private const string username = "Jody";
        private readonly string[] cidsList = { "1", "2", "3" };
        private readonly string[] didsList = { "50", "12", "42" };
        private readonly string[] usernamesList = { "Jody", "Victor", "Phum", "Alex" };

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
            Assert.AreEqual(customerids.ElementAt(customerids.Length -1).ToString().Replace("\"",""),
                tokenResponse.CustomerId.Replace('"','\''), testType); //Trim the quotes off before comparison


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
            Assert.AreEqual(username, tokenResponse.UserName, testType);

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
            Assert.AreEqual(usernamesList.Last(), tokenResponse.UserName, testType); 

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
            Assert.AreEqual(usernamesList.Last(),tokenResponse.UserName, testType);

            //
            //Test customer setting for one or more users with no override
            //
            response = _mockSettingsController.PostUserEntitySetting(testSetting, customerid, username, testSetting.SettingKey, false);
            testType = "PostUserEntitySetting";

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
            Assert.AreEqual(username, tokenResponse.UserName); 

            //
            //Test customer setting for one or more users with  override
            //
            response = _mockSettingsController.PostUserEntitySetting(testSetting, customerid, username, testSetting.SettingKey, true);
            testType = "PostUserEntitySettingOverride";

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
            Assert.AreEqual(username, tokenResponse.UserName);

            //
            //Test customer setting for one or more devices 
            //
            response = _mockSettingsController.PostDeviceSetting(testSetting, customerid, testSetting.SettingKey, deviceids);
            testType = "PostDeviceSetting";

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
            Assert.AreEqual(didsList.Last(), tokenResponse.DeviceId);

        }


        [Test]
        [AsyncStateMachineAttribute(typeof(Task))]
        public async Task TestPutGlobalSettingById()
        {
            //Test for PutGlobalSettingById(int id, Setting setting)

            //first post a setting to update
            testSetting.CustomerId = customerid;
            testSetting.UserName = username;
            _mockSettingsRepository.Setup(repo => repo.Get(It.IsAny<int>())).Returns(testSetting);
            _mockSettingsRepository.Setup(repo => repo.Update(It.IsAny<Setting>())).Returns(true);
            

            Setting updateSetting = new Setting { Value = "Updated", LastModifiedById = 1234 }; //values to update with

            //Test Put setting (global) by id
            //PutGlobalSettingById(int id, Setting setting)

            int id = testSetting.Id; //id of global setting to update

            var response = _mockSettingsController.PutGlobalSettingById(id, updateSetting);
            testType = "PutSettingById";
            string jsonMessage;

            //Get the response message as a stream and parse it into a string
            using (Stream responseStream = await response.Content.ReadAsStreamAsync())
            {
                jsonMessage = new StreamReader(responseStream).ReadToEnd();
            }

            //Create a Json object based off the string so we can access the individual setting variables
            TokenResponseModel tokenResponse = (TokenResponseModel)JsonConvert.DeserializeObject(jsonMessage, typeof(TokenResponseModel));

            //Check if update succeeded
            Assert.AreEqual(updateSetting.Value, tokenResponse.Value, testType);
            int lastModId = Int32.Parse(tokenResponse.LastModifiedById);
            Assert.AreEqual(updateSetting.LastModifiedById, lastModId, testType);
            Assert.AreEqual(updateSetting.EndEffectiveDate, tokenResponse.EndEffectiveDate, testType);
        }

        [Test]
        [AsyncStateMachineAttribute(typeof(Task))]
        public async Task TestPutGlobalSetting()
        {
            //Test for PutGlobalSetting(string key, Setting setting, bool overrideLower)

            //setup repo
            //List of settings used for testing OverrideLower
            Setting override1 = new Setting { Value = "100", SettingKey = "DisplayBrightness" };
            Setting override2 = new Setting { Value = "100", SettingKey = "DisplayBrightness" };
            Setting override3 = new Setting { Value = "100", SettingKey = "DisplayBrightness" };
            List<Setting> overrideLower = new List<Setting> { override1, override2, override3 };

            testSetting.CustomerId = customerid;
            testSetting.UserName = username;
            _mockSettingsRepository.Setup(repo => repo.GetAll()).Returns(Enumerable.Repeat(testSetting, 1));
            _mockSettingsRepository.Setup(repo => repo.Update(It.IsAny<Setting>())).Returns(true);

            _mockSettingsRepository.Setup(repo => repo.OverrideTest()).Returns(overrideLower);

            Setting updateSetting = new Setting { Value = "Updated", LastModifiedById = 1234 }; //values to update with

            //Put global with option to override
            //PutGlobalSetting(string key, Setting setting, bool overrideLower = false)

            //Overrride lower = false
            string settingKey = testSetting.SettingKey;
            var response = _mockSettingsController.PutGlobalSetting(settingKey, updateSetting, false);
            testType = "PutGlobalSetting";
            string jsonMessage;

            //Get the response message as a stream and parse it into a string
            using (Stream responseStream = await response.Content.ReadAsStreamAsync())
            {
                jsonMessage = new StreamReader(responseStream).ReadToEnd();
            }

            var jsonObject = JsonConvert.DeserializeObject<Setting>(jsonMessage); //deserialize json to object

            //check if update successful 
            Assert.AreEqual(response.StatusCode, HttpStatusCode.OK);
            Assert.AreEqual(updateSetting.Value, jsonObject.Value);
            Assert.AreEqual(updateSetting.LastModifiedById, jsonObject.LastModifiedById);
            Assert.AreEqual(updateSetting.EndEffectiveDate, jsonObject.EndEffectiveDate);

            //override lower = true
            response = _mockSettingsController.PutGlobalSetting(settingKey, updateSetting, true);
            testType = "PutGlobalSettingOverrideLower";

            Assert.AreEqual(response.StatusCode, HttpStatusCode.OK);

        }

        [Test]
        [AsyncStateMachineAttribute(typeof(Task))]
        public async Task TestPutCustomerSetting()
        {
            //Test for PutCustomerSetting(string key, Setting setting, bool overrideLower)

            //setup repo
            //List of settings used for testing OverrideLower
            Setting override1 = new Setting { Value = "100", SettingKey = "DisplayBrightness" };
            Setting override2 = new Setting { Value = "100", SettingKey = "DisplayBrightness" };
            Setting override3 = new Setting { Value = "100", SettingKey = "DisplayBrightness" };
            List<Setting> overrideLower = new List<Setting> { override1, override2, override3 };

            testSetting.CustomerId = customerid;
            testSetting.UserName = username;
            _mockSettingsRepository.Setup(repo => repo.GetAll()).Returns(Enumerable.Repeat(testSetting, 1));
            _mockSettingsRepository.Setup(repo => repo.Update(It.IsAny<Setting>())).Returns(true);

            _mockSettingsRepository.Setup(repo => repo.OverrideTest()).Returns(overrideLower);


            //Put global with option to override
            //PutCustomerSetting(string key, Setting setting, bool overrideLower = false)
            Setting updateSetting = new Setting { Value = "Updated", LastModifiedById = 1234 }; //values to update with
                                                                                                //Overrride lower = false
            string settingKey = testSetting.SettingKey;
            var response = _mockSettingsController.PutCustomerSetting(settingKey, updateSetting, false);
            testType = "PutCustomerSetting";
            string jsonMessage;

            Assert.IsNotNull(response);
            Assert.AreEqual(response.StatusCode, HttpStatusCode.OK);

            //Get the response message as a stream and parse it into a string
            using (Stream responseStream = await response.Content.ReadAsStreamAsync())
            {
                jsonMessage = new StreamReader(responseStream).ReadToEnd();
            }


            var jsonObject = JsonConvert.DeserializeObject<Setting>(jsonMessage); //deserialize json to object

            //check if update successful 

            Assert.AreEqual(updateSetting.Value, jsonObject.Value);
            Assert.AreEqual(updateSetting.LastModifiedById, jsonObject.LastModifiedById);
            Assert.AreEqual(updateSetting.EndEffectiveDate, jsonObject.EndEffectiveDate);

            //override lower = true
            response = _mockSettingsController.PutCustomerSetting(settingKey, updateSetting, true);
            testType = "PutGlobalSettingOverrideLower";

            Assert.AreEqual(response.StatusCode, HttpStatusCode.OK);

        }

        [Test]
        [AsyncStateMachineAttribute(typeof(Task))]
        public async Task TestPutCustomerSettingMultiple()
        {

            //Put mutliple settings (customer) with option to override lower
            //PutCustomerSettingMultiple(string key, Setting setting, string customerIds, bool overrideLower = false)

            //setup repo
            //List of settings used for testing OverrideLower
            Setting override1 = new Setting { Value = "101", SettingKey = "DisplayBrightness1" };
            Setting override2 = new Setting { Value = "102", SettingKey = "DisplayBrightness2" };
            Setting override3 = new Setting { Value = "103", SettingKey = "DisplayBrightness3" };
            List<Setting> overrideLower = new List<Setting> { override1, override2, override3 };

            string cusIds = "111,222";
            int cus1 = 111;
            int cus2 = 222;
            testSetting.CustomerId = customerid;
            testSetting.UserName = username;
            Setting setting1 = new Setting { SettingKey = testSetting.SettingKey, CustomerId = cus1, Value = "val1", LastModifiedById = 1234 };
            Setting setting2 = new Setting { SettingKey = "setting", CustomerId = cus2, Value = "val2", LastModifiedById = 1234 };
            List<Setting> list = new List<Setting> { setting1, setting2 };
            _mockSettingsRepository.Setup(repo => repo.GetAll()).Returns(list);
            _mockSettingsRepository.Setup(repo => repo.Update(It.IsAny<Setting>())).Returns(true);
            _mockSettingsRepository.Setup(repo => repo.OverrideTest()).Returns(overrideLower);



            Setting updateSetting = new Setting { Value = "Updated", LastModifiedById = 1234 }; //values to update with

            //Put mutliple settings (customer) with option to override lower
            //PutCustomerSettingMultiple(string key, Setting setting, string customerIds, bool overrideLower = false)

            //Overrride lower = false
            string settingKey = testSetting.SettingKey;
            var response = _mockSettingsController.PutCustomerSettingMultiple(settingKey, updateSetting, cusIds, false);
            testType = "PutCustomerSettingMultiple";
            string jsonMessage;

            Assert.IsNotNull(response);
            Assert.AreEqual(response.StatusCode, HttpStatusCode.OK);

            //Get the response message as a stream and parse it into a string
            using (Stream responseStream = await response.Content.ReadAsStreamAsync())
            {
                jsonMessage = new StreamReader(responseStream).ReadToEnd();
            }

            var jsonObject = JsonConvert.DeserializeObject<List<Setting>>(jsonMessage); //deserialize json to object

            //check if update successful 

            Assert.AreEqual(updateSetting.Value, jsonObject[0].Value);
            Assert.AreEqual(updateSetting.LastModifiedById, jsonObject[0].LastModifiedById);
            Assert.AreEqual(updateSetting.EndEffectiveDate, jsonObject[0].EndEffectiveDate);

            Assert.AreEqual(updateSetting.Value, jsonObject[1].Value);
            Assert.AreEqual(updateSetting.LastModifiedById, jsonObject[1].LastModifiedById);
            Assert.AreEqual(updateSetting.EndEffectiveDate, jsonObject[1].EndEffectiveDate);

            //override lower = true
            response = _mockSettingsController.PutCustomerSettingMultiple(settingKey, updateSetting, cusIds, true);
            testType = "PutGlobalSettingOverrideLower";

            Assert.AreEqual(response.StatusCode, HttpStatusCode.OK);

        }

        [Test]
        [AsyncStateMachineAttribute(typeof(Task))]
        public async Task TestPutCustomerEntitySetting()
        {

            //Put setting - Specific Entity (customer) and override lower
            //PutCustomerEntitySetting(int entityId, string key, Setting setting, bool overrideLower = false)


            //setup repo
            //List of settings used for testing OverrideLower
            Setting override1 = new Setting { Value = "101", SettingKey = "DisplayBrightness1" };
            Setting override2 = new Setting { Value = "102", SettingKey = "DisplayBrightness2" };
            Setting override3 = new Setting { Value = "103", SettingKey = "DisplayBrightness3" };
            List<Setting> overrideLower = new List<Setting> { override1, override2, override3 };

            testSetting.CustomerId = customerid;
            testSetting.UserName = username;
            _mockSettingsRepository.Setup(repo => repo.GetAll()).Returns(Enumerable.Repeat(testSetting, 1));
            _mockSettingsRepository.Setup(repo => repo.Update(It.IsAny<Setting>())).Returns(true);
            _mockSettingsRepository.Setup(repo => repo.OverrideTest()).Returns(overrideLower);


            Setting updateSetting = new Setting { Value = "Updated", LastModifiedById = 1234 }; //values to update with

            //Put global with option to override
            //PutCustomerEntitySetting(int entityId, string key, Setting setting, bool overrideLower = false)

            //Overrride lower = false
            string settingKey = testSetting.SettingKey;
            var response = _mockSettingsController.PutCustomerEntitySetting(customerid, settingKey, updateSetting, false);
            testType = "PutCustomerEntitySetting";
            string jsonMessage;

            Assert.IsNotNull(response);
            Assert.AreEqual(response.StatusCode, HttpStatusCode.OK);

            //Get the response message as a stream and parse it into a string
            using (Stream responseStream = await response.Content.ReadAsStreamAsync())
            {
                jsonMessage = new StreamReader(responseStream).ReadToEnd();
            }

            var jsonObject = JsonConvert.DeserializeObject<Setting>(jsonMessage); //deserialize json to object

            //check if update successful 

            Assert.AreEqual(updateSetting.Value, jsonObject.Value);
            Assert.AreEqual(updateSetting.LastModifiedById, jsonObject.LastModifiedById);
            Assert.AreEqual(updateSetting.EndEffectiveDate, jsonObject.EndEffectiveDate);

            //override lower = true
            response = _mockSettingsController.PutCustomerEntitySetting(customerid, settingKey, updateSetting, true);
            testType = "PutCustomerEntitySettingOverrideLower";

            Assert.AreEqual(response.StatusCode, HttpStatusCode.OK);
        }

        [Test]
        [AsyncStateMachineAttribute(typeof(Task))]
        public async Task TestPutDeviceSetting()
        {

            // PutDeviceSetting(int customerId, string key, Setting setting)

            //setup repo
            testSetting.CustomerId = customerid;
            testSetting.UserName = username;
            _mockSettingsRepository.Setup(repo => repo.GetAll()).Returns(Enumerable.Repeat(testSetting, 1));
            _mockSettingsRepository.Setup(repo => repo.Update(It.IsAny<Setting>())).Returns(true);


            Setting updateSetting = new Setting { Value = "Updated", LastModifiedById = 1234 }; //values to update with

            //Put global with option to override
            //PutDeviceSetting(int customerId, string key, Setting setting)

            //Overrride lower = false
            string settingKey = testSetting.SettingKey;
            var response = _mockSettingsController.PutDeviceSetting(customerid, settingKey, updateSetting);
            testType = "PutDeviceSetting";
            string jsonMessage;

            Assert.IsNotNull(response);
            Assert.AreEqual(response.StatusCode, HttpStatusCode.OK);

            //Get the response message as a stream and parse it into a string
            using (Stream responseStream = await response.Content.ReadAsStreamAsync())
            {
                jsonMessage = new StreamReader(responseStream).ReadToEnd();
            }

            var jsonObject = JsonConvert.DeserializeObject<Setting>(jsonMessage); //deserialize json to object

            //check if update successful 

            Assert.AreEqual(updateSetting.Value, jsonObject.Value);
            Assert.AreEqual(updateSetting.LastModifiedById, jsonObject.LastModifiedById);
            Assert.AreEqual(updateSetting.EndEffectiveDate, jsonObject.EndEffectiveDate);

        }

        [Test]
        [AsyncStateMachineAttribute(typeof(Task))]
        public async Task TestPutDeviceSettingMultiple()
        {
            //PutDeviceSettingMultiple(int customerId, string key, string deviceIds, Setting setting)


            //setup repo
            string devIds = "111,222";
            int dev1 = 111;
            int dev2 = 222;
            testSetting.CustomerId = customerid;
            testSetting.UserName = username;
            Setting setting1 = new Setting { CustomerId = customerid, SettingKey = testSetting.SettingKey, DeviceId = dev1, Value = "val1", LastModifiedById = 1234 };
            Setting setting2 = new Setting { CustomerId = customerid, SettingKey = "setting", DeviceId = dev2, Value = "val2", LastModifiedById = 1234 };
            List<Setting> list = new List<Setting> { setting1, setting2 };
            _mockSettingsRepository.Setup(repo => repo.GetAll()).Returns(list);
            _mockSettingsRepository.Setup(repo => repo.Update(It.IsAny<Setting>())).Returns(true);

            Setting updateSetting = new Setting { Value = "Updated", LastModifiedById = 12345 }; //values to update with

            //Put global with option to override
            //PutDeviceSettingMultiple(int customerId, string key, string deviceIds, Setting setting)

            //Overrride lower = false
            string settingKey = testSetting.SettingKey;
            var response = _mockSettingsController.PutDeviceSettingMultiple(customerid, settingKey, devIds, updateSetting);
            testType = "PutCustomerSettingMultiple";
            string jsonMessage;

            Assert.IsNotNull(response);
            Assert.AreEqual(response.StatusCode, HttpStatusCode.OK);

            //Get the response message as a stream and parse it into a string
            using (Stream responseStream = await response.Content.ReadAsStreamAsync())
            {
                jsonMessage = new StreamReader(responseStream).ReadToEnd();
            }

            var jsonObject = JsonConvert.DeserializeObject<List<Setting>>(jsonMessage); //deserialize json to object

            //check if update successful 

            Assert.AreEqual(updateSetting.Value, jsonObject[0].Value);
            Assert.AreEqual(updateSetting.LastModifiedById, jsonObject[0].LastModifiedById);
            Assert.AreEqual(updateSetting.EndEffectiveDate, jsonObject[0].EndEffectiveDate);

            Assert.AreEqual(updateSetting.Value, jsonObject[1].Value);
            Assert.AreEqual(updateSetting.LastModifiedById, jsonObject[1].LastModifiedById);
            Assert.AreEqual(updateSetting.EndEffectiveDate, jsonObject[1].EndEffectiveDate);


        }

        [Test]
        [AsyncStateMachineAttribute(typeof(Task))]
        public async Task TestPutDeviceEntitySetting()
        {

            // PutDeviceEntitySetting(int customerId, int entityId, string key, Setting setting)

            //setup repo
            testSetting.CustomerId = customerid;
            testSetting.UserName = username;
            testSetting.DeviceId = 123;
            _mockSettingsRepository.Setup(repo => repo.GetAll()).Returns(Enumerable.Repeat(testSetting, 1));
            _mockSettingsRepository.Setup(repo => repo.Update(It.IsAny<Setting>())).Returns(true);


            Setting updateSetting = new Setting { Value = "Updated", LastModifiedById = 1234 }; //values to update with

            //Put global with option to override
            //PutDeviceEntitySetting(int customerId, int entityId, string key, Setting setting)

            //Overrride lower = false
            string settingKey = testSetting.SettingKey;
            var response = _mockSettingsController.PutDeviceEntitySetting(customerid, 123, settingKey, updateSetting);
            testType = "PutDeviceEntitySetting";
            string jsonMessage;

            Assert.IsNotNull(response);
            Assert.AreEqual(response.StatusCode, HttpStatusCode.OK);

            //Get the response message as a stream and parse it into a string
            using (Stream responseStream = await response.Content.ReadAsStreamAsync())
            {
                jsonMessage = new StreamReader(responseStream).ReadToEnd();
            }

            var jsonObject = JsonConvert.DeserializeObject<Setting>(jsonMessage); //deserialize json to object

            //check if update successful 

            Assert.AreEqual(updateSetting.Value, jsonObject.Value);
            Assert.AreEqual(updateSetting.LastModifiedById, jsonObject.LastModifiedById);
            Assert.AreEqual(updateSetting.EndEffectiveDate, jsonObject.EndEffectiveDate);

        }

        [Test]
        [AsyncStateMachineAttribute(typeof(Task))]
        public async Task TestPutUserSetting()
        {
            //PutUserSetting(int customerId, string key, Setting setting)

            //setup repo
            testSetting.CustomerId = customerid;
            testSetting.UserName = username;
            _mockSettingsRepository.Setup(repo => repo.GetAll()).Returns(Enumerable.Repeat(testSetting, 1));
            _mockSettingsRepository.Setup(repo => repo.Update(It.IsAny<Setting>())).Returns(true);


            Setting updateSetting = new Setting { Value = "Updated", LastModifiedById = 1234 }; //values to update with

            //Put global with option to override
            //PutUserSetting(int customerId, string key, Setting setting)

            //Overrride lower = false
            string settingKey = testSetting.SettingKey;
            var response = _mockSettingsController.PutUserSetting(customerid, settingKey, updateSetting);
            testType = "PutUserSetting";
            string jsonMessage;

            Assert.IsNotNull(response);
            Assert.AreEqual(response.StatusCode, HttpStatusCode.OK);

            //Get the response message as a stream and parse it into a string
            using (Stream responseStream = await response.Content.ReadAsStreamAsync())
            {
                jsonMessage = new StreamReader(responseStream).ReadToEnd();
            }

            var jsonObject = JsonConvert.DeserializeObject<Setting>(jsonMessage); //deserialize json to object

            //check if update successful 

            Assert.AreEqual(updateSetting.Value, jsonObject.Value);
            Assert.AreEqual(updateSetting.LastModifiedById, jsonObject.LastModifiedById);
            Assert.AreEqual(updateSetting.EndEffectiveDate, jsonObject.EndEffectiveDate);

        }

        [Test]
        [AsyncStateMachineAttribute(typeof(Task))]
        public async Task TestPutUserEntitySetting()
        {
            //PutUserEntitySetting(int customerId, string entityId, string key, Setting setting)

            //setup repo
            testSetting.CustomerId = customerid;
            testSetting.UserName = username;
            _mockSettingsRepository.Setup(repo => repo.GetAll()).Returns(Enumerable.Repeat(testSetting, 1));
            _mockSettingsRepository.Setup(repo => repo.Update(It.IsAny<Setting>())).Returns(true);


            Setting updateSetting = new Setting { Value = "Updated", LastModifiedById = 1234 }; //values to update with

            //Put global with option to override
            //PutUserEntitySetting(int customerId, string entityId, string key, Setting setting)

            //Overrride lower = false
            string settingKey = testSetting.SettingKey;
            var response = _mockSettingsController.PutUserEntitySetting(customerid, username, settingKey, updateSetting);
            testType = "PutUserEntitySetting";
            string jsonMessage;

            Assert.IsNotNull(response);
            Assert.AreEqual(response.StatusCode, HttpStatusCode.OK);

            //Get the response message as a stream and parse it into a string
            using (Stream responseStream = await response.Content.ReadAsStreamAsync())
            {
                jsonMessage = new StreamReader(responseStream).ReadToEnd();
            }

            var jsonObject = JsonConvert.DeserializeObject<Setting>(jsonMessage); //deserialize json to object

            //check if update successful 

            Assert.AreEqual(updateSetting.Value, jsonObject.Value);
            Assert.AreEqual(updateSetting.LastModifiedById, jsonObject.LastModifiedById);
            Assert.AreEqual(updateSetting.EndEffectiveDate, jsonObject.EndEffectiveDate);
        }

        [Test]
        [AsyncStateMachineAttribute(typeof(Task))]
        public async Task TestPutUserSettingMultiple()
        {
            //PutUserSettingMultiple(int customerId, string key, string usernames, Setting setting)
            //setup repo
            string newUsers = "joe,jack";
            string joe = "joe";
            string jack = "jack";
            testSetting.CustomerId = customerid;
            testSetting.UserName = username;
            Setting setting1 = new Setting { CustomerId = customerid, SettingKey = testSetting.SettingKey, UserName = joe, Value = "val1", LastModifiedById = 1234 };
            Setting setting2 = new Setting { CustomerId = customerid, SettingKey = "setting", UserName = jack, Value = "val2", LastModifiedById = 1234 };
            List<Setting> list = new List<Setting> { setting1, setting2 };
            _mockSettingsRepository.Setup(repo => repo.GetAll()).Returns(list);
            _mockSettingsRepository.Setup(repo => repo.Update(It.IsAny<Setting>())).Returns(true);

            Setting updateSetting = new Setting { Value = "Updated", LastModifiedById = 12345 }; //values to update with

            //Put global with option to override
            //PutUserSettingMultiple(int customerId, string key, string usernames, Setting setting)

            //Overrride lower = false
            string settingKey = testSetting.SettingKey;
            var response = _mockSettingsController.PutUserSettingMultiple(customerid, settingKey, newUsers, updateSetting);
            testType = "PutUserSettingMultiple";
            string jsonMessage;

            Assert.IsNotNull(response);
            Assert.AreEqual(response.StatusCode, HttpStatusCode.OK);

            //Get the response message as a stream and parse it into a string
            using (Stream responseStream = await response.Content.ReadAsStreamAsync())
            {
                jsonMessage = new StreamReader(responseStream).ReadToEnd();
            }

            var jsonObject = JsonConvert.DeserializeObject<List<Setting>>(jsonMessage); //deserialize json to object

            //check if update successful 

            Assert.AreEqual(updateSetting.Value, jsonObject[0].Value);
            Assert.AreEqual(updateSetting.LastModifiedById, jsonObject[0].LastModifiedById);
            Assert.AreEqual(updateSetting.EndEffectiveDate, jsonObject[0].EndEffectiveDate);

            Assert.AreEqual(updateSetting.Value, jsonObject[1].Value);
            Assert.AreEqual(updateSetting.LastModifiedById, jsonObject[1].LastModifiedById);
            Assert.AreEqual(updateSetting.EndEffectiveDate, jsonObject[1].EndEffectiveDate);
        }

        //
        [Test]
        [Category("TestGet")]
        public void TestGetGlobalSettingData()
        {
            // Arrange
            testType = "GetGlobalSetting";
            _mockSettingsRepository.Setup(repo => repo.GetGlobalSetting()).Returns(Enumerable.Repeat(testSetting, 1));

            // Act
            var response = _mockSettingsController.GetGlobalSettingData();

            // Assert
            Assert.AreEqual(response.Level, "Global", testType);
            Assert.AreEqual(response.Settings.First().Level, testSetting.Level, testType);
            Assert.AreEqual(response.Settings.First().Value, testSetting.Value, testType);
            Assert.AreEqual(response.Settings.First().SettingKey, testSetting.SettingKey, testType);
        }

        [Test]
        [Category("TestGet")]
        public void TestGetCustomerSettingData()
        {
            // Case: there is no customer id in database
            // Arrange
            testType = "GetCustomerSetting";

            // Act
            var response = _mockSettingsController.GetCustomerSettingData(customerid);

            // Assert
            Assert.AreEqual(response.Level, "Customer", testType);
            Assert.AreEqual(response.Settings.Count(), 0, testType);

            // Case: there is a customer id 
            // Arrange
            testType = "GetCustomerSetting";
            testSetting.CustomerId = customerid;
            _mockSettingsRepository.Setup(repo => repo.GetCustomerSettings(customerid)).Returns(Enumerable.Repeat(testSetting, 1));

            // Act
            response = _mockSettingsController.GetCustomerSettingData(customerid);

            // Assert
            Assert.AreEqual(response.Level, "Customer", testType);
            Assert.AreEqual(response.Settings.First().Level, testSetting.Level, testType);
            Assert.AreEqual(response.Settings.First().Value, testSetting.Value, testType);
            Assert.AreEqual(response.Settings.First().SettingKey, testSetting.SettingKey, testType);
        }


        [Test]
        [Category("TestGet")]
        public void TestGetDeviceSettingData()
        {
            // Case: there is no device id in database
            // Arrange
            testType = "GetDeviceSetting";

            // Act
            var response = _mockSettingsController.GetDeviceSettingData(customerid, deviceid);

            // Assert
            Assert.AreEqual(response.Level, "Device", testType);
            Assert.AreEqual(response.Settings.Count(), 0, testType);

            // Case: there is a device id 
            // Arrange
            testType = "GetDeviceSetting";
            testSetting.CustomerId = customerid;
            testSetting.DeviceId = deviceid;
            _mockSettingsRepository.Setup(repo => repo.GetDeviceSettings(customerid, deviceid)).Returns(Enumerable.Repeat(testSetting, 1));

            // Act
            response = _mockSettingsController.GetDeviceSettingData(customerid, deviceid);

            // Assert
            Assert.AreEqual(response.Level, "Device", testType);
            Assert.AreEqual(response.Settings.First().Level, testSetting.Level, testType);
            Assert.AreEqual(response.Settings.First().Value, testSetting.Value, testType);
            Assert.AreEqual(response.Settings.First().SettingKey, testSetting.SettingKey, testType);
        }

        [Test]
        [Category("TestGet")]
        public void TestGetUserSettingData()
        {
            // Case: there is no username in database
            // Arrange
            testType = "GetUserSetting";

            // Act
            var response = _mockSettingsController.GetUserSettingData(customerid, username);

            // Assert
            Assert.AreEqual(response.Level, "User", testType);
            Assert.AreEqual(response.Settings.Count(), 0, testType);

            // Case: there is a username in database
            // Arrange
            testType = "GetUserSetting";
            testSetting.CustomerId = customerid;
            testSetting.UserName = username;
            _mockSettingsRepository.Setup(repo => repo.GetUserSettings(customerid, username)).Returns(Enumerable.Repeat(testSetting, 1));

            // Act
            response = _mockSettingsController.GetUserSettingData(customerid, username);

            // Assert
            Assert.AreEqual(response.Level, "User", testType);
            Assert.AreEqual(response.Settings.First().Level, testSetting.Level, testType);
            Assert.AreEqual(response.Settings.First().Value, testSetting.Value, testType);
            Assert.AreEqual(response.Settings.First().SettingKey, testSetting.SettingKey, testType);
        }

        [Test]
        public void TestDeletes()
        {
            String Key = "Key";
        
            var message=new HttpResponseMessage(HttpStatusCode.OK);
            Setting nullSetting = null;
            //Global delete
            testType = "DeleteSingleGlobalSettingWithFalse";
            _mockSettingsRepository.Setup(repo => repo.GetGlobalSetting(testSetting.SettingKey)).Returns(testSetting);
            var response = _mockSettingsController.DeleteGlobalSettingOverride(testSetting.SettingKey,false);
            Assert.AreEqual(HttpStatusCode.OK,response.StatusCode,testType);

            testType = "DeleteSingleGlobalSettingWithFalse(Dont exist)";
            _mockSettingsRepository.Setup(repo => repo.GetGlobalSetting(testSetting.SettingKey))
                .Returns(nullSetting);
            response = _mockSettingsController.DeleteGlobalSettingOverride(testSetting.SettingKey, false);
            Assert.AreEqual(HttpStatusCode.NotFound,response.StatusCode, testType);
            /*
            testType = "DeleteSingleGlobalSettingWithFalse(No key provided)";
            response = _mockSettingsController.DeleteGlobalSettingOverride(" ", false);
            Assert.AreEqual(HttpStatusCode.BadRequest,response.StatusCode, testType);*/

            testType = "DeleteMutipleGlobalSettingWithFalse";
            _mockSettingsRepository.Setup(repo => repo.GetGlobalSetting(testSetting.SettingKey)).Returns(testSetting);
            _mockSettingsRepository.Setup(repo => repo.GetGlobalSetting("Key")).Returns(testSetting);
            response = _mockSettingsController.DeleteGlobalSettingOverride(testSetting.SettingKey + "," + "Key", false);
            Assert.AreEqual(HttpStatusCode.OK,response.StatusCode,testType);

            testType = "DeleteSingleGlobalSettingWithTrue";
            response = _mockSettingsController.DeleteGlobalSettingOverride(testSetting.SettingKey, true);
            Assert.AreEqual(HttpStatusCode.OK,response.StatusCode,testType);

            testType = "DeleteMutipleGlobalSettingWithTrue";
            
            response = _mockSettingsController.DeleteGlobalSettingOverride(testSetting.SettingKey + "," + "Key", true);
            Assert.AreEqual(HttpStatusCode.OK,response.StatusCode,testType);


            //Customer delete, override all
            testType = "DeleteCustomerSettingwithFalse(with no id)";
            response = _mockSettingsController.DeleteCustomerEntitySettingOverride("", testSetting.SettingKey, false);
            Assert.AreEqual(HttpStatusCode.BadRequest,response.StatusCode);
            /*
            testType = "DeleteCustomerSettingWithFalse(with no key)";
            response = _mockSettingsController.DeleteCustomerEntitySettingOverride(customerid.ToString(), "", false);
            Assert.AreEqual(HttpStatusCode.BadRequest,response.StatusCode);*/

            testType = "DeleteSingleCustomerSetting with false";
            response = _mockSettingsController.DeleteCustomerEntitySettingOverride(customerid.ToString(), testSetting.SettingKey,
                false);
            Assert.AreEqual(HttpStatusCode.OK,response.StatusCode);

            testType = "DeleteMutipleCustomerSetting(different key) with false";
            response = _mockSettingsController.DeleteCustomerEntitySettingOverride(customerid.ToString(),
                testSetting.SettingKey + "," + Key, false);
            Assert.AreEqual(HttpStatusCode.OK,response.StatusCode,testType);

            testType = "DeleteMutipleCustomerSetting(different Id) with false";
            response = _mockSettingsController.DeleteCustomerEntitySettingOverride(
                2.ToString() + "," + customerid.ToString(), testSetting.SettingKey, false);
            Assert.AreEqual(HttpStatusCode.OK,response.StatusCode);

            testType = "DeleteMutipleCustomerSetting(different Id and keys) with false";
            response = _mockSettingsController.DeleteCustomerEntitySettingOverride(
                customerid.ToString() + "," + 2.ToString(), Key + testSetting.SettingKey, false);
            Assert.AreEqual(HttpStatusCode.OK,response.StatusCode,testType);

            testType = "DeleteMutipleSetting(different Id and Keys) with true";
            response = _mockSettingsController.DeleteCustomerEntitySettingOverride(
                customerid.ToString() + "," + 2.ToString(), Key + testSetting.SettingKey, true);
            Assert.AreEqual(HttpStatusCode.OK,response.StatusCode,testType);

            //customer delete, device override

            testType = "DeleteCostomerSetting, no setting to be found";
            response = _mockSettingsController.DeleteCustomerSettingDevices(Key, customerid, deviceid.ToString());
            Assert.AreEqual(HttpStatusCode.NotFound,response.StatusCode,testType);

            testType = "DeleteCustomerSetting, override 1 setting";
            _mockSettingsRepository.Setup(repo => repo.GetCustomerSetting(customerid, testSetting.SettingKey))
                .Returns(testSetting);
            response = _mockSettingsController.DeleteCustomerSettingDevices(testSetting.SettingKey, customerid,
                deviceid.ToString());
            Assert.AreEqual(HttpStatusCode.OK,response.StatusCode,testType);

            testType = "DeleteCustomerSetting, override mutiple setting";

            
            response = _mockSettingsController.DeleteCustomerSettingDevices(testSetting.SettingKey, customerid,
                deviceid.ToString()+","+52.ToString());
            Assert.AreEqual(HttpStatusCode.OK,response.StatusCode,testType);

            //customer delete, user override

            
            testType = "DeleteCustomerSetting, noSetting to be found";
            _mockSettingsRepository.Setup(repo => repo.GetCustomerSetting(customerid, testSetting.SettingKey))
                .Returns(nullSetting);
            response = _mockSettingsController.DeleteCustomerSettingCustomers(customerid, Key, username);
            Assert.AreEqual(HttpStatusCode.NotFound,response.StatusCode);

            testType = "DeleteCustomerSetting, override 1 setting";
            _mockSettingsRepository.Setup(repo => repo.GetCustomerSetting(customerid, testSetting.SettingKey))
                .Returns(testSetting);
            response=_mockSettingsController.DeleteCustomerSettingCustomers(customerid, testSetting.SettingKey, username);
            Assert.AreEqual(HttpStatusCode.OK,response.StatusCode,testType);

            testType = "DeleteCustomerSetting, override multiple setting";
            response = _mockSettingsController.DeleteCustomerSettingCustomers(customerid, testSetting.SettingKey,
                username + "," + "Victor");
            Assert.AreEqual(HttpStatusCode.OK,response.StatusCode,testType);
            
            //device level delete

            testType = "DeleteDeviceSetting, no setting to be found";
            response = _mockSettingsController.DeleteDeviceSetting(customerid, deviceid.ToString(), testSetting.SettingKey);
            Assert.AreEqual(HttpStatusCode.NotFound,response.StatusCode,testType);

            testType = "DeleteDeviceSetting, 1 setting";
            _mockSettingsRepository.Setup(repo => repo.GetDeviceSetting(customerid, testSetting.SettingKey, deviceid))
                .Returns(testSetting);
            response = _mockSettingsController.DeleteDeviceSetting(customerid, deviceid.ToString(), testSetting.SettingKey);
            Assert.AreEqual(HttpStatusCode.OK,response.StatusCode,testType);

            testType = "DeleteDeviceSetting,multipleSetting";
            _mockSettingsRepository.Setup(repo => repo.GetDeviceSetting(customerid, testSetting.SettingKey, 52))
                .Returns(testSetting);
            response = _mockSettingsController.DeleteDeviceSetting(customerid, deviceid + "," + 52.ToString(),
                testSetting.SettingKey);
            Assert.AreEqual(HttpStatusCode.OK,response.StatusCode,testType);

            //user level delete

            testType = "DeleteUserSetting, no setting to be found";
            response = _mockSettingsController.DeleteUserSetting(customerid, testSetting.SettingKey, username);
            Assert.AreEqual(HttpStatusCode.NotFound,response.StatusCode,testType);

            testType = "DeleteUserSetting, 1 setting";
            _mockSettingsRepository.Setup(repo => repo.GetUserSetting(customerid, testSetting.SettingKey, username))
                .Returns(testSetting);
            response = _mockSettingsController.DeleteUserSetting(customerid, testSetting.SettingKey, username);
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode, testType);

            testType = "DeleteUserSetting, multiple setting";
            _mockSettingsRepository.Setup(repo => repo.GetUserSetting(customerid, testSetting.SettingKey, "Victor"))
                .Returns(testSetting);
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