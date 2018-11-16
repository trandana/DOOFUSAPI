using DOOFUS.Models;
using DOOFUS.Models.Persistence;
using DOOFUS.Nhbnt.Web.Controllers;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;

namespace DOOFUS.Tests
{
    [TestFixture]
    public class SettingsTests
    {
        SettingsController settingsController;
        SettingDataRepository settingsRepository;
        Setting testSetting;

        //status codes
        int created;
        int ok;
        int customerid;
        int deviceid;
        string customerids;
        string usernames;
        string deviceids;

        [SetUp]
        public void Setup()
        {
            settingsController = new SettingsController();
            settingsRepository = new SettingDataRepository();
            testSetting = new Setting();   
            
            //Test values and status codes
            testSetting.Value = "100";
            testSetting.SettingKey = "DisplayBrightness";
            created = 201;
            ok = 200;
            customerid = 1;
            customerids = "1,2,3";
            usernames = "Jody,Victor,Phum,Alex";
            usernames = "Jody";
            deviceids = "1,2,3";
            deviceid = 1;

        }
       
        [Test]
        public void TestPosts()
        {   
            //Test posting a single global setting
            var response = settingsController.PostGlobalSetting(testSetting, testSetting.SettingKey);
            Assert.AreEqual(created, (int)response.StatusCode);

            //Test posting a single global setting and override lower 
            response = settingsController.PostGlobalSettingOverride(testSetting, testSetting.SettingKey, true);
            Assert.AreEqual(created, (int)response.StatusCode);

            //Testing posting a single global setting to specific customers only and do not override lower
            response = settingsController.PostGlobalEntitySetting(testSetting, testSetting.SettingKey, customerids);
            Assert.AreEqual(created, (int)response.StatusCode);

            //Testing posting a single global setting to specific customers only and overide lower
            response = settingsController.PostGlobalEntitySetting(testSetting, testSetting.SettingKey, customerids, true);
            Assert.AreEqual(created, (int)response.StatusCode);

            //Test settings with multiple device ids for a specific customer
            response = settingsController.PostCustomerSettingOverride(testSetting, customerid, testSetting.SettingKey, deviceids);
            Assert.AreEqual(created, (int)response.StatusCode);

            //Test settings with multiple usernames for a specific customer
            response = settingsController.PostCustomerSettingOverride(testSetting, customerid, testSetting.SettingKey, usernames);
            Assert.AreEqual(created, (int)response.StatusCode);

            //Test user setting for a particular user and override lower
            response = settingsController.PostUserEntitySetting(testSetting, customerid, testSetting.SettingKey, usernames, true);
            Assert.AreEqual(created, (int)response.StatusCode);

            //Test user setting for a particular user and do not override lower
            response = settingsController.PostUserEntitySetting(testSetting, customerid, testSetting.SettingKey, usernames);
            Assert.AreEqual(created, (int)response.StatusCode);

            //Test device setting for a particular user and override lower
            response = settingsController.PostDeviceSetting(testSetting, customerid, testSetting.SettingKey, deviceid);
            Assert.AreEqual(created, (int)response.StatusCode);
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

        }         
        
    }
}