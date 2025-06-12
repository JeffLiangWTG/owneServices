using System.ServiceModel;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CargoWise.eHub.Share.eHubServices.Tests.eHubSender
{
	[TestClass]
	public class CiscoSendToServiceTests : BaseTest
	{
		[TestMethod]
		public void ResolveEnpointCiscoUrlProd()
		{
			var service = new TestSendToService(Logger, "Sender1", "TRXELPELP_CIS", "");
			service.LoadContextTest();
			var address = service.GetEndpointAddressTest();
			Assert.AreEqual("https://wsgx.cisco.com/cvcm/msf/lss/service", address.Uri.AbsoluteUri);
		}

		[TestMethod]
		public void ResolveEnpointCiscoUrlTest()
		{
			var service = new TestSendToService(Logger, "Sender1", "TRXELPTST_CIS", "");
			service.LoadContextTest();
			var address = service.GetEndpointAddressTest();
			Assert.AreEqual("https://wsgx-test.cisco.com/cvcm/msf/lss/service", address.Uri.AbsoluteUri);
		}

		[TestMethod]
		public void ResolveEnpointeLMSUrlTest()
		{
			var service = new TestSendToService(Logger, "Sender1", "APOMELTSH_ELM", "");
			service.LoadContextTest();
			var address = service.GetEndpointAddressTest();
			Assert.AreEqual("https://exttrnapps.npe.auspost.com.au/elmsWs/MailingStatementService", address.Uri.AbsoluteUri);
		}

		[TestMethod]
		[ExpectedException(typeof(EndpointNotFoundException), "Can not resolve Endpoint url by recepientId")]
		public void ResolveEnpointUrlForWrongRecepient()
		{
			var service = new TestSendToService(Logger, "Sender1", "Bla", "");
			service.LoadContextTest();
			var address = service.GetEndpointAddressTest();
		}

		[TestMethod]
		public void ResolveEnpointUSDISUrlTest()
		{
			{
				var service = new TestSendToService(Logger, "Sender1", "USDIS", "");
				service.LoadContextTest();
				var address = service.GetEndpointAddressTest();
				Assert.AreEqual("https://prodserver:2933/", address.Uri.AbsoluteUri);
			}
			{
				var service = new TestSendToService(Logger, "Sender1", "USDISTest", "");
				service.LoadContextTest();
				var address = service.GetEndpointAddressTest();
				Assert.AreEqual("https://testserver:2933/", address.Uri.AbsoluteUri);
			}
		}

		[TestMethod]
		public void ResolveEnpointOceanInsightsUrlTest()
		{
			var service = new TestSendToService(Logger, "Sender1", "OceanInsights", "");
			service.LoadContextTest();
			var address = service.GetEndpointAddressTest();
			Assert.AreEqual("http://capi.ocean-insights.com/containertracking/v2/", address.Uri.AbsoluteUri);
		}

	}
}
