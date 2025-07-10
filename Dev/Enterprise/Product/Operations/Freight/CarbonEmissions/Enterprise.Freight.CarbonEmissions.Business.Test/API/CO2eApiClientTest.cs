using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Integration.Licensing;
using Moq;
using static Enterprise.Integration.Forwarding;

namespace Enterprise.Freight.CarbonEmissions.Business.Testing
{
	sealed class CO2eApiClientTest : TestCaseWithFactory
	{
		public void TestGetRequestEndpoint()
		{
			var productRegistrationMock = new Mock<IProductRegistration>();
			var productRegistrationKeyMock = new Mock<IProductRegistrationKey>();
			productRegistrationKeyMock.Setup(x => x.EnterpriseCode).Returns("EnterpriseCode");
			productRegistrationKeyMock.Setup(x => x.ServerCode).Returns("ServerCode");
			productRegistrationMock.Setup(x => x.Key).Returns(productRegistrationKeyMock.Object);

			var bizo = Factory.New<IForwardingShipment>();
			bizo.JS_UniqueConsignRef = "JOB011/A";
			using (ObjectFactory.Substitute(productRegistrationMock.Object))
			{
				var uri = new CO2eApiClient().GetRequestEndpoint(bizo as BusinessObject);
				AssertEquals("v1/clients/EnterpriseCodeServerCode/ForwardingShipment/JOB011%2FA/emission", uri);
			}
		}
	}
}
