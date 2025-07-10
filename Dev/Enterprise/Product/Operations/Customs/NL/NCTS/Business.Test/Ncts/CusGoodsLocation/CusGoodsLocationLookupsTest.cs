using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.NL.NCTS.Business.Testing;

sealed class CusGoodsLocationLookupsTest : BusinessObjectLookupsTestCase
{
	public void TestQualifierList_ParentIsArrivalIncident()
	{
		var nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);
		nctsHeader.BH_ExportFlag = "Y";
		var lookups = nctsHeader.EnRouteIncidents.AddNew().GoodsLocation.Lookups;
		var list = lookups.QualifierList;

		CombineAssertions(() =>
		{
			AssertEquals("List for Arrival - incidents", "U, W, Z", list.CodesAsString);
			AssertSame("Cached", list, lookups.QualifierList);
		});
	}

	public void TestQualifierList_ParentIsNotArrivalIncident()
	{
		var nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
		var lookups = nctsHeader.MovementHeader.GoodsLocation.Lookups;
		var list = lookups.QualifierList;

		CombineAssertions(() =>
		{
			AssertEquals("List for MovementHeader", "T", list.CodesAsString);
			AssertSame("Cached", list, lookups.QualifierList);
		});
	}

	public void TestTypeList_ParentIsDepartureMovementHeader()
	{
		var nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
		var lookups = nctsHeader.MovementHeader.GoodsLocation.Lookups;
		var list = lookups.TypeList;

		CombineAssertions(() =>
		{
			AssertEquals("List for Departure", "A, B", list.CodesAsString);
			AssertSame("Cached", list, lookups.TypeList);
		});
	}

	public void TestTypeList_ParentIsNotDepartureMovementHeader()
	{
		var nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);
		var lookups = nctsHeader.ArrivalMovementHeader.GoodsLocation.Lookups;
		var list = lookups.TypeList;

		CombineAssertions(() =>
		{
			AssertEquals("List for Arrival", "A, B, C, D", list.CodesAsString);
			AssertSame("Cached", list, lookups.TypeList);
		});
	}
}
