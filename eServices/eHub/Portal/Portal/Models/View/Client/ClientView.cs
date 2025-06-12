using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using CargoWise.eHub.Portal.Models.eHubTransactions;

namespace CargoWise.eHub.Portal.Models.View
{
	[Serializable]
	public class ClientView
	{
		public eHubClient Client { get; set; }

		public ClientView(eHubClient client)
		{
			Client = client;
		}

		public string DistributionZoneName { get; set; }
		public string AirServiceProviderName { get; set; }
		public IEnumerable<eHubZone> DistributionZone { get; set; }
		public IEnumerable<eHubClient> AirServiceProvider { get; set; }
	}
}