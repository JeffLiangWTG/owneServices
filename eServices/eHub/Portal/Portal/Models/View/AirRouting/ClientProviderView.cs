using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using CargoWise.eHub.Portal.Models.Extensions;
using CargoWise.eHub.Portal.Models.eHubTransactions;

namespace CargoWise.eHub.Portal.Models.View
{
	public class ClientProviderView
	{
		public Guid ServiceProviderId { get; set; }
		public string ServiceProviderName { get; set; }

		public ClientProviderView(IeHubTransactionsContext context, Guid? serviceProviderId)
		{
			if (serviceProviderId != null)
			{
				ServiceProviderId = serviceProviderId.Value;
				var client = context.GetClient(serviceProviderId);
				ServiceProviderName = client.CC_ID;
			}
		}

	}
}