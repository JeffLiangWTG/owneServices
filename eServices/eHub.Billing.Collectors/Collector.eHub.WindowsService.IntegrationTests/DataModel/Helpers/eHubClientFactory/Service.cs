using CargoWise.eServices.Billing.Collector.eHub.WindowsService.IntegrationTests.DataModel.eHubTransactions;

namespace CargoWise.eServices.Billing.Collector.eHub.WindowsService.IntegrationTests.DataModel.Helpers.eHubClientBuilders
{
	class Service : eHubClient
	{
		public Service(string id) : base(id)
		{
			this.CC_OwnerCategory = "Service";
			this.CC_SystemCategory = "Third Party";
		}
	}
}
