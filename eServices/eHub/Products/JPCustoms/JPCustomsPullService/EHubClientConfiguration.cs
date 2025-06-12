using CargoWise.eHub.Products.JPCustoms.Client;

namespace CargoWise.eHub.Products.JPCustoms.PullService
{
	public class EHubClientConfiguration : IEHubClientConfiguration
	{
		public string EndpointConfigurationName
		{
			get
			{
				return "JPCustomsReplyService";
			}
		}
	}
}