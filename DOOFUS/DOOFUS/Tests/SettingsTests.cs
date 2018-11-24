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