using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.NL.NCTS.Business.Testing;

sealed class NonPersistentNctsUnloadingRemarkLookupsTest : BusinessObjectLookupsTestCase
{
	public void TestCodeList()
	{
		var nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);
		var lookups = nctsHeader.ArrivalMovementHeader.UnloadingRemarkCollection.AddNew().Lookups;
		var list = lookups.CodeList;

		CombineAssertions(() =>
		{
			AssertEquals("List for Arrival - incidents", "I, O", list.CodesAsString);
			AssertSame("Cached", list, lookups.CodeList);
		});
	}
}
