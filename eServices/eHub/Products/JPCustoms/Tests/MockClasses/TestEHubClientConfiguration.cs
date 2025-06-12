using CargoWise.eHub.Products.JPCustoms.Client;

namespace CargoWise.eHub.Products.JPCustoms.Tests.MockClasses
{
	public class TestEHubClientConfiguration : IEHubClientConfiguration
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
