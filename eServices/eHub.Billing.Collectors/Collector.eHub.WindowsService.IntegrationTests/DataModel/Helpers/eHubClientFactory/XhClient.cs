using CargoWise.eServices.Billing.Collector.eHub.WindowsService.IntegrationTests.DataModel.eHubTransactions;

namespace CargoWise.eServices.Billing.Collector.eHub.WindowsService.IntegrationTests.DataModel.Helpers.eHubClientBuilders
{
	class XhClient : eHubClient
	{
		public XhClient(string id) : base(id)
		{
			this.CC_OwnerCategory = "Client";
			this.CC_SystemCategory = "XH";
		}
	}
}
