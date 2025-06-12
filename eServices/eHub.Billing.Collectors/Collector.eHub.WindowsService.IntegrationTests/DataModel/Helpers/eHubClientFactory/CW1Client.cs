using CargoWise.eServices.Billing.Collector.eHub.WindowsService.IntegrationTests.DataModel.eHubTransactions;

namespace CargoWise.eServices.Billing.Collector.eHub.WindowsService.IntegrationTests.DataModel.Helpers.eHubClientBuilders
{
	class CW1Client : eHubClient
	{
		public CW1Client(string id) : base(id)
		{
			this.CC_OwnerCategory = "Client";
			this.CC_SystemCategory = "Enterprise";
		}
	}
}
