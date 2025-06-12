using System;

namespace CargoWise.eServices.Billing.Collector.eHub.WindowsService.IntegrationTests.DataModel.eHubTransactions
{
	class eHubMessageReferenceRegistry
	{
		public eHubMessageReferenceRegistry()
		{
			CR_PK = Guid.NewGuid();
		}

		public Guid CR_PK { get; set; }
		public Guid CR_CC_Client { get; set; }
		public string CR_ApplicationCode { get; set; }
		public string CR_MessageReference { get; set; }
		public string CR_Password { get; set; }
	}
}
