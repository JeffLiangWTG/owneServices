using CargoWise.EntityFramework;
using NUnit.Framework;
using static Enterprise.Freight.Integration.Forwarding;

namespace Enterprise.Freight.Business.Testing
{
	sealed class CommonContainerLookupsTest : TestCase
	{
		public void TestGetContainerModesList()
		{
			var list = CommonContainerLookups.GetContainerModesList("AIR");
			Assert("AIR contains ULD", list.ContainsCode("ULD"));
			AssertEquals("1 modes for AIR", 1, list.Count);

			list = CommonContainerLookups.GetContainerModesList("ROA");
			Assert("ROA contains LTL", list.ContainsCode("LTL"));
			Assert("ROA contains FTL", list.ContainsCode("FTL"));
			Assert("ROA contains FCL", list.ContainsCode("FCL"));
			Assert("ROA contains LCL", list.ContainsCode("LCL"));
			Assert("ROA contains BCN", list.ContainsCode("BCN"));
			Assert("ROA contains SCN", list.ContainsCode("SCN"));
			Assert("ROA contains GRP", list.ContainsCode("GRP"));
			AssertEquals("7 modes for ROA", 7, list.Count);

			list = CommonContainerLookups.GetContainerModesList("SEA");
			Assert("SEA contains LCL", list.ContainsCode("LCL"));
			Assert("SEA contains FCL", list.ContainsCode("FCL"));
			Assert("SEA contains GRP", list.ContainsCode("GRP"));
			Assert("SEA contains BCN", list.ContainsCode("BCN"));
			Assert("SEA contains SCN", list.ContainsCode("SCN"));
			Assert("SEA contains BBK", list.ContainsCode("BBK"));
			Assert("SEA contains ROR", list.ContainsCode("ROR"));
			AssertEquals("7 modes for SEA", 7, list.Count);

			list = CommonContainerLookups.GetContainerModesList("RAI");
			Assert("RAI contains LCL", list.ContainsCode("LCL"));
			Assert("RAI contains FCL", list.ContainsCode("FCL"));
			Assert("RAI contains GRP", list.ContainsCode("GRP"));
			Assert("RAI contains BCN", list.ContainsCode("BCN"));
			Assert("RAI contains SCN", list.ContainsCode("SCN"));
			Assert("RAI contains BBK", list.ContainsCode("BBK"));
			Assert("SEA contains ROR", list.ContainsCode("ROR"));
			AssertEquals("7 modes for RAI", 7, list.Count);
		}

		public void TestGrossWeightVerificationTypeList()
		{
			var expectedAllowsRTL = new[] { "NON", "PKG", "CNT", "WTA", "RTL", "NRQ" };
			var expectedListRestOfWorld = new[] { "NON", "PKG", "CNT", "WTA", "NRQ" };

			var factory = new BusinessObjectFactory();
			var container = factory.New<CommonContainer>();
			AssertContainsExactElementsInAnyOrder(expectedListRestOfWorld, container.Lookups.GrossWeightVerificationTypeList.GetAllCodes());

			var consol = factory.New<CommonConsol>();
			container.JC_JK = consol.PK;

			consol.Transports[0].JW_RL_NKLoadPort = "USCHI";
			AssertContainsExactElementsInAnyOrder(expectedAllowsRTL, container.Lookups.GrossWeightVerificationTypeList.GetAllCodes());

			consol.Transports[0].JW_RL_NKLoadPort = "AUBNE";
			AssertContainsExactElementsInAnyOrder(expectedListRestOfWorld, container.Lookups.GrossWeightVerificationTypeList.GetAllCodes());

			consol.JK_RL_NKLoadPort = "USNYC";
			AssertContainsExactElementsInAnyOrder(expectedAllowsRTL, container.Lookups.GrossWeightVerificationTypeList.GetAllCodes());

			container = factory.New<CommonContainer>();

			var shipment = factory.New<CommonShipment>();
			container.JC_JS_FCLBookingOnlyLink = shipment.PK;

			var transport = shipment.Transports.AddNew();
			transport.JW_RL_NKLoadPort = "AUBNE";
			AssertContainsExactElementsInAnyOrder(expectedListRestOfWorld, container.Lookups.GrossWeightVerificationTypeList.GetAllCodes());

			transport.JW_RL_NKLoadPort = "USBAL";
			AssertContainsExactElementsInAnyOrder(expectedAllowsRTL, container.Lookups.GrossWeightVerificationTypeList.GetAllCodes());

			transport.JW_RL_NKLoadPort = "AUMEL";
			AssertContainsExactElementsInAnyOrder(expectedListRestOfWorld, container.Lookups.GrossWeightVerificationTypeList.GetAllCodes());
		}

		public void TestRelatedContainerLoadListCollection()
		{
			var factory = new BusinessObjectFactory();
			var container = factory.New<CommonContainer>();
			var containerLoadList = factory.New<ICYContainerLoadList>();
			var containerLoadPlan = factory.New<ICFSContainerLoadList>();
			var list = container.Lookups.RelatedContainerLoadListCollection;
			Assert("Contains container load list", list.Contains(containerLoadList));
			Assert("Not contains container load plan", !list.Contains(containerLoadPlan));
		}

		public void TestRelatedContainerLoadPlanCollection()
		{
			var factory = new BusinessObjectFactory();
			var container = factory.New<CommonContainer>();
			var containerLoadList = factory.New<ICYContainerLoadList>();
			var containerLoadPlan = factory.New<ICFSContainerLoadList>();
			var list = container.Lookups.RelatedContainerLoadPlanCollection;
			Assert("Contains container load plan", list.Contains(containerLoadPlan));
			Assert("Not contains container load list", !list.Contains(containerLoadList));
		}
	}
}
