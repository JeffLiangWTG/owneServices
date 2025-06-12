using System;
using System.Collections.Generic;
using CargoWise.eHub.Portal.Models.eHubTransactions;
using CargoWise.eHub.Portal.Models.Extensions;

namespace CargoWise.eHub.Portal.Models.View
{
	public class ClientEditView
	{
		public eHubClient Client { get; set; }
		public string AirServiceProviderName { get; set; }

		public ClientEditView(IeHubTransactionsContext context, Guid clientId)
		{
			Client = context.GetClient(clientId);
			AirServiceProvider = context.GetAllAirServiceProvider();
		}
		
		public IEnumerable<eHubClient> AirServiceProvider { get; set; }
	}
}