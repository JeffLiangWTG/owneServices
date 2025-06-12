using CargoWise.eServices.Billing.Collector.eHub.WindowsService.IntegrationTests.DataModel.eHubTransactions;

namespace CargoWise.eServices.Billing.Collector.eHub.WindowsService.IntegrationTests.DataModel.Helpers.eHubClientBuilders
{
	class ThirdPartyClient : eHubClient
	{
		public ThirdPartyClient(string id) : base(id)
		{
			this.CC_OwnerCategory = "Client";
			this.CC_SystemCategory = "Third Party";
		}
	}
}
