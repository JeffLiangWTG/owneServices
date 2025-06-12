using System;
using System.Xml;
using CargoWise.eHub.BizTalkAdapters.Transferrer.Adapter;
using Microsoft.BizTalk.Adapter.Framework;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CargoWise.eHub.BizTalkAdapters.Tests.Transferrer.Adapter
{
	[TestClass]
	public class AdapterManagementTests
	{
        [TestMethod]
  		[TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TransferrerCore_AdapterManagement_GetSchema()
		{
			string fileLocation;
			var testAdapterManagement = new TestAdapterManagement();
			var result = testAdapterManagement.GetSchema("uri", "ns", out fileLocation);

			Assert.AreEqual(String.Empty, fileLocation);
			Assert.AreEqual(Result.Continue, result);
		}

        [TestMethod]
  		[TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TransferrerCore_AdapterManagement_ValidationConfiguration()
		{
			string config = "<config />";
			var testAdapterManagement = new TestAdapterManagement();
			Assert.AreEqual(config, testAdapterManagement.ValidateConfiguration(ConfigType.TransmitHandler, config));
			Assert.AreEqual(config, testAdapterManagement.ValidateConfiguration(ConfigType.TransmitLocation, config));
			Assert.AreEqual(config, testAdapterManagement.ValidateConfiguration(ConfigType.ReceiveHandler, config));
			Assert.AreEqual(config, testAdapterManagement.ValidateConfiguration(ConfigType.ReceiveLocation, config));
		}

        [TestMethod]
  		[TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TransferrerCore_AdapterManagement_SetNamedUri()
		{
			var configXml = new XmlDocument();
			configXml.LoadXml("<Config><Name>NAME</Name></Config>");

			var testAdapterManagement = new TestAdapterManagement();

			testAdapterManagement.SetNamedUri(configXml);
			Assert.AreEqual("<Config><Name>NAME</Name><uri>scheme://name/</uri></Config>", configXml.OuterXml);

			configXml["Config"]["Name"].InnerText = "CHANGED";

			testAdapterManagement.SetNamedUri(configXml);
			Assert.AreEqual("<Config><Name>CHANGED</Name><uri>scheme://changed/</uri></Config>", configXml.OuterXml);
		}
	}

	class TestAdapterManagement : AdapterManagement
	{
		protected override string Scheme() { return "scheme"; }
		public override string GetConfigSchema(Microsoft.BizTalk.Adapter.Framework.ConfigType configType) { throw new NotImplementedException(); }
		public new void SetNamedUri(XmlDocument configXml) { base.SetNamedUri(configXml); }
	}
}
