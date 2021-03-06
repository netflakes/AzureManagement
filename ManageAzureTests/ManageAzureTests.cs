﻿using System;
using Xunit;
using Shouldly;
using ManageAzure.Lib;
using ManageAzure.AzureModels;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Collections.Generic;
using System.Threading;
using System.Collections.Concurrent;
using System.Xml.Serialization;
using System.IO;
using AppConfiguration;
using AppLogging;
using AppDataExport;
using TinyIoC;

namespace ManageAzureTests
{
    public class ManageAzureTests
    {
        AzureManagementDownloader sutD;
        AzureManagementReporter sutR;
        TestData testData;

        [Fact]
        public void TestReading_AzureSettingsFile_ShouldRetunAvalidSubscriptionAndManagementCertificate() 
        {
            var expectedSubscriptionId = sutD.Configuration.SubscriptionId();
            var expectedCertificateExists = sutD.Configuration.Base64EncodedManagementCertificate().Length > 0;
            var actualSubscriptionId = testData.SubscriptionId;

            Should.Equals(expectedCertificateExists, true);
            Should.Equals(expectedSubscriptionId, actualSubscriptionId);
        }

        [Fact]
        public void Test_GetAllElasticRoleRdpFiles_ShouldReturnAListOfRdpFiles() 
        {
            var downloadPath = testData.DownloadResourcePath;
            var rdpFileList = sutD.GetAllElasticRoleRdpFiles();
            var expectedResult = sutD.DownloadRdpFiles(rdpFileList, downloadPath);
            Should.Equals(expectedResult, true);
        }

        [Fact]
        public void Test_GetAllVirtualMachineRdpFiles_ShouldReturnAListOfRdpFiles()
        {
            var downloadPath = testData.DownloadResourcePath;
            var rdpFileList = sutD.GetAllVirtualMachineRdpFiles();
            var expectedResult = sutD.DownloadRdpFiles(rdpFileList, downloadPath);
            Should.Equals(rdpFileList.Count, 1);
            Should.Equals(expectedResult, true);
        }

        [Fact]
        public void Test_GetAllVirtualMachineRdpFiles_ShouldReturnAListOfRdpFiles_ForAgivenCloudService()
        {
            var downloadPath = testData.DownloadResourcePath;

            foreach (var cloudService in testData._CloudServices._CloudServices) 
            {
                var rdpFileList = sutD.GetAllVirtualMachineRdpFilesForService(cloudService.CloudServiceName);
                var expectedResult = sutD.DownloadRdpFiles(rdpFileList, downloadPath);
                Should.Equals(expectedResult, true);
            }
        }


        [Fact]
        public void Test_GetAllCloudServiceNames_Should_Return_AlistOfCloudServices()
        {
            CloudServices services = sutR.GetAllCloudServices();

            foreach (var cloudService in testData._CloudServices._CloudServices)
            {
                Should.Equals(services.Contains(cloudService.CloudServiceName), true);
            }
        }

        [Fact]
        public void Test_GetAllVirtualMachineRoles_Should_Return_AlistOfVirtualMachines() 
        {
            VirtualMachines vms = sutR.GetAllVirtualMachineRoles();

            foreach (var vm in testData._VirtualMachines._VirtualMachineRoles) 
            {
                Should.Equals(vms.Contains(vm.VmName), true);
            }
        }

        [Fact]
        public void Test_GetAllWebRoles_Should_Return_AlistOfWebRoles() 
        {
            ComputeRoles roles = sutR.GetAllWebRoles();

            foreach (var role in testData._ComputeRoles._ComputeRoles) 
            {
                var expectedServiceName = role.ServiceName;
                var expectedInstanceName = role.InstanceName;

                Should.Equals(roles.Contains(expectedServiceName, expectedInstanceName), true);
            }
        }

        /// <summary>
        /// Any setup code should be going in here..
        /// </summary>
        public ManageAzureTests() 
        {
            var setupFile ="c:\\temp\\Azure_Publish_Settings\\AzureManagementTestConfig.xml";
            var costDataFile = "c:\\temp\\Azure_Publish_Settings\\AzureCosts.xml";
            testData = ReadTestData(setupFile);

            var settingsPath = testData.PublishSettingsFile;
            TestBootstrap.Register(settingsPath, costDataFile);

            sutD = TinyIoCContainer.Current.Resolve<AzureManagementDownloader>();
            sutR = TinyIoCContainer.Current.Resolve<AzureManagementReporter>();
        }

        public static class TestBootstrap
        {
            public static void Register(string settingsFile, string costDataFile)
            {
                IMlogger mLogger = new Mlogger();
                IAppConfiguration appConfig = new ApplicationConfiguration(settingsFile,costDataFile);
                IDataExporter dataExporter = new ConsoleWriter();
                TinyIoCContainer.Current.Register<IMlogger>(mLogger);
                TinyIoCContainer.Current.Register<IAppConfiguration>(appConfig);
                TinyIoCContainer.Current.Register<IDataExporter>(dataExporter);
            }
        }

        private TestData ReadTestData(string settingsFile)
        {

            TextReader reader = null;
            try
            {
                XmlSerializer deserializer = new XmlSerializer(typeof(TestData));
                reader = new StreamReader(@settingsFile);
                object obj = deserializer.Deserialize(reader);
                TestData XmlData = (TestData)obj;
                return XmlData;
            }
            catch (InvalidOperationException ioe)
            {
                var message = String.Format("Error Reading the Test Data File{0}", ioe);
                throw ioe;
            }
            finally
            {
                reader.Close();
            }
        }

    }

}
