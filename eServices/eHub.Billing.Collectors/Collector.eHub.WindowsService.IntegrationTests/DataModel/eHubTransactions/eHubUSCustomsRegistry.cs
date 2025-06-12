using System;

namespace CargoWise.eServices.Billing.Collector.eHub.WindowsService.IntegrationTests.DataModel.eHubTransactions
{
	class eHubUSCustomsRegistry
	{
		public eHubUSCustomsRegistry()
		{
			ER_PK = Guid.NewGuid();
		}

		public Guid ER_PK { get; set; }
		public Guid ER_CC_Client { get; set; }
		public string ER_ApplicationCode { get; set; }
		public string ER_Name { get; set; }
		public string ER_Value { get; set; }
		public bool ER_IsProduction { get; set; }
	}
}