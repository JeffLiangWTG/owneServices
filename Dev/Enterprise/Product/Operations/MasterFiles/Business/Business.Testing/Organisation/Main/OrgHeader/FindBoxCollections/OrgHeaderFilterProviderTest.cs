using CargoWise.EntityFramework.Testing;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class OrgHeaderFilterProviderTest : TestCaseWithFactory
	{
		public void TestGetCTOFilter()
		{
			AssertEquals("LiteralTextADO", "OH_IsMiscFreightServices = 1 and (OH_IsAirCTO = 1 or OH_IsSeaCTO = 1)", new OrgHeaderFilterProvider().GetCTOFilter().LiteralTextADO);
		}

		public void TestGetDepotFilter()
		{
			AssertEquals("LiteralTextADO", "OH_IsMiscFreightServices = 1 and (OH_IsPackDepot = 1 or OH_IsUnpackDepot = 1 or OH_IsRoadFreightDepot = 1 or OH_IsRailHead = 1)", new OrgHeaderFilterProvider().GetDepotFilter().LiteralTextADO);
		}

		public void TestGetWarehouseFilter()
		{
			AssertEquals("LiteralTextADO", "OH_IsWarehouseClient = 1", new OrgHeaderFilterProvider().GetWarehouseFilter().LiteralTextADO);
		}

		public void TestGetCTOOrDepotOrWarehouseFilter()
		{
			AssertEquals("LiteralTextADO", "((OH_IsMiscFreightServices = 1 and (OH_IsAirCTO = 1 or OH_IsSeaCTO = 1)) or (OH_IsMiscFreightServices = 1 and (OH_IsPackDepot = 1 or OH_IsUnpackDepot = 1 or OH_IsRoadFreightDepot = 1 or OH_IsRailHead = 1))) or OH_IsWarehouseClient = 1", new OrgHeaderFilterProvider().GetCTOOrDepotOrWarehouseFilter().LiteralTextADO);
		}
	}
}
