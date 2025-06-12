using System;
using System.IO;
using System.Reflection;
using CargoWise.eHub.BizTalkAdapters.FtpEx.Admin;
using Microsoft.BizTalk.Adapter.Framework;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CargoWise.eHub.BizTalkAdapters.Tests
{
    [TestClass()]
    public class FtpExAdapterManagementTest : TestBase
    {
        [TestMethod]
  		[TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
        public void FtpExAdapterManagement_GetSchemeTest()
        {
            var target = new FtpExAdapterManagement();
            string expected = "ftpex";

            string actual = target.Scheme();

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
  		[TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
        public void FtpExAdapterManagement_GetConfigSchemaTest()
        {
            var target = new FtpExAdapterManagement();
            Assembly ass = target.GetType().Assembly;
            string expected, actual;

            expected = new StreamReader(ass.GetManifestResourceStream("CargoWise.eHub.BizTalkAdapters.FtpEx.Admin.FtpExTransmitHandler.xsd")).ReadToEnd();
            actual = target.GetConfigSchema(ConfigType.TransmitHandler);
            Assert.AreEqual(expected, actual);
            expected = new StreamReader(ass.GetManifestResourceStream("CargoWise.eHub.BizTalkAdapters.FtpEx.Admin.FtpExTransmitLocation.xsd")).ReadToEnd();
            actual = target.GetConfigSchema(ConfigType.TransmitLocation);
            Assert.AreEqual(expected, actual);
            expected = new StreamReader(ass.GetManifestResourceStream("CargoWise.eHub.BizTalkAdapters.FtpEx.Admin.FtpExReceiveHandler.xsd")).ReadToEnd();
            actual = target.GetConfigSchema(ConfigType.ReceiveHandler);
            Assert.AreEqual(expected, actual);
            expected = new StreamReader(ass.GetManifestResourceStream("CargoWise.eHub.BizTalkAdapters.FtpEx.Admin.FtpExReceiveLocation.xsd")).ReadToEnd();
            actual = target.GetConfigSchema(ConfigType.ReceiveLocation);
            Assert.AreEqual(expected, actual);
            new ConfigType();
            expected = String.Empty;
            actual = target.GetConfigSchema((ConfigType)4);
            Assert.AreEqual(expected, actual);
        }
    }
}
