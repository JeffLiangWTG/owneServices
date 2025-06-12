using System.Threading.Tasks;
using CargoWise.eHub.Products.Shared.RoutingRuleValidation.WebService.HealthCheck;
using NUnit.Framework;
using Rhino.Mocks;

namespace CargoWise.eHub.Products.Shared.RoutingRuleValidation.WebService.Tests
{
	[TestFixture]
	public class RoutingRuleValidationHealthCheckItemProviderTests
	{
		[Test]
		public async Task CheckHealthAsync_ShouldNotContainLFOrCRInErrors()
		{
			var mockProvider = MockRepository.GenerateMock<RoutingRuleValidationHealthCheckItemProvider>();
			mockProvider.Stub(p => p.ErrorsOnCreatingServiceInstance()).Return("Error with LF\n");
			mockProvider.Stub(p => p.ErrorsOnConnectingToDatabase()).Return("Error with CR\r");
			var provider = mockProvider;

			var checkItem = await provider.CheckHealthAsync().ConfigureAwait(false);

			Assert.IsFalse(checkItem.Description.Contains("\n"), "Description contains LF");
			Assert.IsFalse(checkItem.Description.Contains("\r"), "Description contains CR");
		}
	}
}
