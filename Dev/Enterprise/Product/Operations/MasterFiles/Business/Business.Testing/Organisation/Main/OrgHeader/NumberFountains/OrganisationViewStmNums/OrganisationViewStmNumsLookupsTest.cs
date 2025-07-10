using CargoWise.EntityFramework.Testing;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class OrganisationViewStmNumsLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TesTypeList()
		{
			var stmNums = Factory.New<OrganisationViewStmNums>();
			var lookups = stmNums.Lookups;
			var list = lookups.TypeList;
			var list2 = lookups.TypeList;

			AssertEquals("Should be cached", list, list2);
			AssertEquals(5, list.Count);
			Assert(list.ContainsCode(OrgConstants.NumberFountains.Code.SSCCBarCodeNumbers));
			Assert(list.ContainsCode(OrgConstants.NumberFountains.Code.FTZAdmissionControlNumber));
			Assert(list.ContainsCode(OrgConstants.NumberFountains.Code.FTZAdmissionControlNumberForWarehouse));
			Assert(list.ContainsCode(OrgConstants.NumberFountains.Code.ForwardAirBillNumbers));
			Assert(list.ContainsCode(OrgConstants.NumberFountains.Code.TransportReferenceNumbers));
		}
	}
}
