using CargoWise.Billing.Service;

namespace CargoWise.eServices.Billing.WcfService.Tests
{
	using System;
	using System.Collections.Generic;
	using System.Threading.Tasks;
	using CargoWise.eServices.Monitoring.HealthCheck.API;
	using CargoWise.eServices.Monitoring.HealthCheck.API.Test;
	using Moq;
	using Moq.Protected;
	using NUnit.Framework;

	class BillingServiceHealthCheckItemProviderTest : HealthCheckItemProviderBaseClass<BillingServiceHealthCheckItemProvider>
	{
		[Test]
		public async Task TestBillingWebServiceHealthCheckItemProvider()
		{
			var mockProvider = new Mock<BillingServiceHealthCheckItemProvider>();
			mockProvider.CallBase = true;
			mockProvider.Protected().Setup("AddTransaction", ItExpr.IsAny<BillingTransaction>()).Verifiable();
			var provider = mockProvider.Object;

			Assert.That(provider.Name, Is.EqualTo("BillingWebService"));

			var checkItem = await provider.CheckHealthAsync().ConfigureAwait(false);
			Assert.That(checkItem.Status, Is.EqualTo(HealthCheckStatus.OK));
			Assert.That(checkItem.Description, Is.EqualTo("Service is alive."));
		}


		[Test]
		public async Task TestBillingWebServiceHealthCheckItemProviderThrowException()
		{
			var mockProvider = new Mock<BillingServiceHealthCheckItemProvider>();
			mockProvider.CallBase = true;
			mockProvider.Protected().Setup("AddTransaction", ItExpr.IsAny<BillingTransaction>()).Throws(new InvalidOperationException("Invalid Operation."));
			var provider = mockProvider.Object;

			Assert.That(provider.Name, Is.EqualTo("BillingWebService"));

			var checkItem = await provider.CheckHealthAsync().ConfigureAwait(false);
			Assert.That(checkItem.Status, Is.EqualTo(HealthCheckStatus.Error));
			Assert.That(checkItem.Description, Is.EqualTo("An exception 'System.InvalidOperationException' was thrown during health check. Message: Invalid Operation."));
		}

		protected override List<string> GetAllKindsOfDescriptions()
		{
			var descriptions = new List<string>();

			var mockProvider = new Mock<BillingServiceHealthCheckItemProvider>();
			mockProvider.CallBase = true;
			var provider = mockProvider.Object;

			mockProvider.Protected().Setup("AddTransaction", ItExpr.IsAny<BillingTransaction>()).Verifiable();
			var checkItem = provider.CheckHealthAsync().Result;
			descriptions.Add(checkItem.Description);

			mockProvider.Protected().Setup("AddTransaction", ItExpr.IsAny<BillingTransaction>()).Throws(new InvalidOperationException());
			checkItem = provider.CheckHealthAsync().Result;
			descriptions.Add(checkItem.Description);

			return descriptions;
		}
	}
}
