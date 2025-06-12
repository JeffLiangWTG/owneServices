using System.Collections.Generic;
using CargoWise.eHub.Portal.Models.eHubTransactions;

namespace CargoWise.eHub.Portal.Models.View
{
	public class ClientPIMAView
	{
		public eHubClient Client { get; set; }
		public List<ServiceProviderPIMAView> ServiceProviderPIMAView;
		public List<ServiceProviderPIMAView> ServiceProviderPIMAViewTest;
		public string Env { get; set; }
	}
}