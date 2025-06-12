using CargoWise.eServices.Billing.Collector.eHub.WindowsService.IntegrationTests.DataModel.eHubTransactions;
using CargoWise.eServices.Billing.Collector.eHub.WindowsService.IntegrationTests.DataModel.Helpers.eHubClientBuilders;
using System;

namespace CargoWise.eServices.Billing.Collector.eHub.WindowsService.IntegrationTests.DataModel.Helpers.eHubClientFactory
{
	public class eHubClientFactory
	{
		public static eHubClient CreateeHubClient(string clientType, string clientID)
		{
			switch (clientType)
			{
				case (eHubClientType.CW1Client):
					return new CW1Client(clientID);
				case (eHubClientType.Service):
					return new Service(clientID);
				case (eHubClientType.ServiceProvider):
					return new ServiceProvider(clientID);
				case (eHubClientType.ThirdPartyClient):
					return new ThirdPartyClient(clientID);
				case (eHubClientType.XhClient):
					return new XhClient(clientID);
				default:
					throw new Exception("Please specify a client type.");
			}
		}
	}

	public static class eHubClientType
	{
		public const string CW1Client = "CW1Client";
		public const string Service = "Service";
		public const string ServiceProvider = "ServiceProvider";
		public const string ThirdPartyClient = "ThirdPartyClient";
		public const string XhClient = "XhClient";
	}
}
