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
        Mock<SettingsController> mockSettingsController;
        Mock<SettingDataRepository> mockSettingsRepository;
        Mock<Setting> mockSetting;        

        //Test conditions
        string failed;
        string testType;

        int deviceid = 51;
        int customerid = 1;
        string customerids = "1,2,3";
        string deviceids = "50,12,42";
        string usernames = "Jody, Victor, Phum, Alex";

        
        [SetUp]
        public void Setup()
        {
            mockSettingsController = new Mock<SettingsController>();
            mockSettingsRepository = new Mock<SettingDataRepository>();
            mockSetting = new Mock<Setting>();            

            mockSetting.Object.Value = "100";
            mockSetting.Object.SettingKey = "DisplayBrightness";             
        }
       
        //note: will probably end up changing this all later and making a class layer in between Test and Controller 
        [Test]
        public void TestPosts()
        {
            //
            //Test global setting, no override
            //
            var response = mockSettingsController.Object.PostGlobalSetting(mockSetting.Object, mockSetting.Object.SettingKey, false);
            testType ="PostGlobalSetting";

            //Using Newtonsoft JSON library to parse the JSON response so we can compare
            var responseString = response.Content.ToString();            
            dynamic jsonObject = JObject.Parse(responseString);

            isEqual(mockSetting.Object.Level, (string)jsonObject.SelectToken("Level"), testType);
            isEqual(mockSetting.Object.Value, (string)jsonObject.SelectToken("Value"), testType);
            isEqual(mockSetting.Object.SettingKey, (string)jsonObject.SelectToken("SettingKey"), testType);

            //
            //Test global setting, with override
            //
            response = mockSettingsController.Object.PostGlobalSetting(mockSetting.Object, mockSetting.Object.SettingKey, true);            
            testType = "PostGlobalSettingOverride";

            //Using Newtonsoft JSON library to parse the JSON response so we can compare
            responseString = response.Content.ToString();
            jsonObject = JObject.Parse(responseString);

            isEqual(mockSetting.Object.Level, (string)jsonObject.SelectToken("Level"), testType);
            isEqual(mockSetting.Object.Value, (string)jsonObject.SelectToken("Value"), testType);
            isEqual(mockSetting.Object.SettingKey, (string)jsonObject.SelectToken("SettingKey"), testType);

            //
            //Test customer setting for one or more users no override
            //
            response = mockSettingsController.Object.PostCustomerSetting(mockSetting.Object, customerid, mockSetting.Object.SettingKey, usernames, false);
            testType = "PostCustomerSetting";
            
            responseString = response.Content.ToString();
            jsonObject = JObject.Parse(responseString);

            isEqual(mockSetting.Object.Level, (string)jsonObject.SelectToken("Level"), testType);
            isEqual(mockSetting.Object.Value, (string)jsonObject.SelectToken("Value"), testType);
            isEqual(mockSetting.Object.SettingKey, (string)jsonObject.SelectToken("SettingKey"), testType);

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
                mockSettingsController.Object.PostGlobalSetting(mockSetting.Object, mockSetting.Object.SettingKey,
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