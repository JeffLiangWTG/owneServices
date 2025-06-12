using CargoWise.eHub.Products.GBCustoms.Core.InboundMessageWebService.Handlers;
using NUnit.Framework;

namespace CargoWise.eHub.Products.GBCustoms.Core.Tests.InboundMessageWebService
{
	[TestFixture]
	internal class ConfigurationTests
	{
		[Test]
		public void TestGetSettings()
		{
			var handlerEMCS = new TransportLayerHandler(ProviderType.EMCS);
			var handlerGVMS = new TransportLayerHandler(ProviderType.GVMS);

			Assert.AreEqual("2", handlerEMCS.GetSettings("GetRecipientsRetryAttempts"));
			Assert.AreEqual("4", handlerEMCS.GetSettings("GetRecipientsRetryWaitTime"));

			Assert.AreEqual("1", handlerGVMS.GetSettings("GetRecipientsRetryAttempts"));
			Assert.AreEqual("3", handlerGVMS.GetSettings("GetRecipientsRetryWaitTime"));
		}
	}
}
