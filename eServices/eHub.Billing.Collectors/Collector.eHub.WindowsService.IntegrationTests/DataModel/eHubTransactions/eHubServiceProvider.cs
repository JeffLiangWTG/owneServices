using System;

namespace CargoWise.eServices.Billing.Collector.eHub.WindowsService.IntegrationTests.DataModel.eHubTransactions
{
	class eHubServiceProvider
	{
		public eHubServiceProvider(Guid service, Guid provider, Guid? sp_RR = null)
		{
			SP_PK = new Guid();
			SP_RR = sp_RR;
			SP_CC_Service = service;
			SP_CC_Provider = provider;
		}

		public Guid SP_PK { get; set; }
		public Guid? SP_RR { get; set; }
		public Guid SP_CC_Provider { get; set; }
		public Guid SP_CC_Service { get; set; }
	}
}
