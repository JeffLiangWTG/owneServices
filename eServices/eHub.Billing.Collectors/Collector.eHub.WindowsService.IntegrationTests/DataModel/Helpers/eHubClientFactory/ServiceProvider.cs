using CargoWise.eServices.Billing.Collector.eHub.WindowsService.IntegrationTests.DataModel.eHubTransactions;

namespace CargoWise.eServices.Billing.Collector.eHub.WindowsService.IntegrationTests.DataModel.Helpers.eHubClientBuilders
{
	class ServiceProvider : eHubClient
	{
		public ServiceProvider(string id) : base(id)
		{
			this.CC_OwnerCategory = "Service Provider";
			this.CC_SystemCategory = "Third Party";
		}
	}
}