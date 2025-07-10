using CargoWise.EntityFramework.Testing;
using Enterprise.Core;

namespace Enterprise.MasterFiles.Business.Testing
{
	public class RefFacilityLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestFacilityTypes()
		{
			var refFacility = Factory.New<RefFacility>();
			var lookups = refFacility.Lookups;
			AssertEquals(3, lookups.FacilityTypes.Count);
			Assert(lookups.FacilityTypes.ContainsCode(Constants.FacilityType.Code.Terminal));
			Assert(lookups.FacilityTypes.ContainsCode(Constants.FacilityType.Code.ContainerYard));
			Assert(lookups.FacilityTypes.ContainsCode(Constants.FacilityType.Code.TransitWarehouse));
		}

		public void TestRefFacilityLocalCodes()
		{
			var refFacility = Factory.NewWithValidTestData<RefFacility>();
			refFacility.RFT_Code = "00000000001";
			var refFacilityLocalCode1 = Factory.NewWithValidTestData<RefFacilityLocalCode>();
			refFacilityLocalCode1.RFL_RFT_NKFacilityCode = "00000000001";
			refFacilityLocalCode1.RFL_Usage = "AAA";
			var refFacilityLocalCode2 = Factory.NewWithValidTestData<RefFacilityLocalCode>();
			refFacilityLocalCode2.RFL_RFT_NKFacilityCode = "00000000001";
			refFacilityLocalCode2.RFL_Usage = "BBB";
			var refFacilityLocalCode3 = Factory.NewWithValidTestData<RefFacilityLocalCode>();
			refFacilityLocalCode3.RFL_RFT_NKFacilityCode = "00000000002";
			refFacilityLocalCode3.RFL_Usage = "CCC";
			Factory.Save();

			var lookups = refFacility.Lookups;
			AssertEquals(2, lookups.RefFacilityLocalCodeList.Count);
			AssertCollectionContains(refFacilityLocalCode1, lookups.RefFacilityLocalCodeList);
			AssertCollectionContains(refFacilityLocalCode2, lookups.RefFacilityLocalCodeList);
			AssertCollectionNotContains(refFacilityLocalCode3, lookups.RefFacilityLocalCodeList);
		}

		public void TestRefFacilities()
		{
			var refFacility = Factory.NewWithValidTestData<RefFacility>();
			refFacility.RFT_Code = "00000000001";
			var lookups = refFacility.Lookups;
			AssertEquals(1, lookups.RefFacilities.Count);

			Assert(lookups.RefFacilities.Contains(refFacility));
		}
	}
}
