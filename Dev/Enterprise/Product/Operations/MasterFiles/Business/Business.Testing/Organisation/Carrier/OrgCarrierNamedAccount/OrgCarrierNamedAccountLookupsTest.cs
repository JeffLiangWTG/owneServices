using System.Collections.Generic;
using System.Threading;
using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using Enterprise.Integration;
using Enterprise.Integration.Rating;
using Moq;
using WiseRates.Api.Client;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class OrgCarrierNamedAccountLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestForeignNames()
		{
			var wiseRatesClientMock = new Mock<IWiseRatesClient>();
			wiseRatesClientMock
				.Setup(m => m.GetNamedAccounts(It.IsAny<string>()))
				.Returns(new HashSet<string>()
				{
					"Nike Inc.",
					"Nike",
					"Test Account",
					"Test Account 2",
				});

			var wiseRatesClientFactoryMock = new Mock<IWiseRatesClientFactory>();
			wiseRatesClientFactoryMock
				.Setup(f => f.TryCreate(
					It.IsAny<string>(),
					It.IsAny<int>(),
					It.IsAny<CancellationToken>(),
					It.IsAny<ILogger>()))
				.Returns((wiseRatesClientMock.Object, string.Empty));

			using (ObjectFactory.Substitute(wiseRatesClientFactoryMock.Object))
			{
				var orgCarrierNamedAccount = Factory.NewWithValidTestData<OrgCarrierNamedAccount>();
				var lookups = new OrgCarrierNamedAccountLookups(orgCarrierNamedAccount);
				AssertContainsExactElementsInAnyOrder(["Nike", "Test Account", "Nike Inc.", "Test Account 2"], lookups.ForeignNames.GetAllCodes());
			}
		}
	}
}
