using System.Threading;
using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using Enterprise.Integration;
using Enterprise.Integration.Rating;
using Enterprise.ZArchitecture.Core;
using Moq;
using WiseRates.Api.Client;
using WiseRates.Api.Model;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class RefCommodityCodeLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestRH_UniversalCommodityGroup_List()
		{
			var commodity = Factory.NewWithValidTestData<RefCommodityCode>();
			var lookups = commodity.Lookups;

			var universalCommodityGroupList = new[]
			{
				new RefCommodityGroup { Code = "GENL", Description = "General" },
				new RefCommodityGroup { Code = "HAZD", Description = "Hazardous" },
				new RefCommodityGroup { Code = "PERS", Description = "Perisable" },
				new RefCommodityGroup { Code = "TIMB", Description = "Timber" },
				new RefCommodityGroup { Code = "FLAM", Description = "Flammable" },
				new RefCommodityGroup { Code = "CNVT", Description = "Container Vent Required" },
			};

			UntranslatableCodeDescriptionPairList actualUniversalCommodityGroupList = null;

			var wiseRatesClientMock = new Mock<IWiseRatesClient>();
			wiseRatesClientMock
				.Setup(m => m.GetCommodityGroups(It.IsAny<string>()))
				.Returns(universalCommodityGroupList);

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
				actualUniversalCommodityGroupList = lookups.RH_UniversalCommodityGroup_List;
			}

			CombineAssertions("Should just contain 1 commodity because HAZD, PERS, TIMB, FLAM and CNVT are listed as checkbox", () =>
			{
				AssertEquals(1, actualUniversalCommodityGroupList.Count);
				Assert(actualUniversalCommodityGroupList.ContainsCode("GENL"));
			});

			wiseRatesClientMock.VerifyAll();
		}
	}
}