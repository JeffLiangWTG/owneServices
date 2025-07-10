using CargoWise.EntityFramework.Testing;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class UNDGSubstanceADRLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestExceptedQuantityList()
		{
			var item = Factory.New<UNDGSubstanceADR>();
			var lookup = new UNDGSubstanceADRLookups(item);

			AssertEquals("Expecting 6", 6, lookup.ExceptedQuantityList.Count);
			Assert(lookup.ExceptedQuantityList.ContainsCode(UNDGSubstanceLookups.ExceptedQuantity.Code.E0));
			Assert(lookup.ExceptedQuantityList.ContainsCode(UNDGSubstanceLookups.ExceptedQuantity.Code.E1));
			Assert(lookup.ExceptedQuantityList.ContainsCode(UNDGSubstanceLookups.ExceptedQuantity.Code.E2));
			Assert(lookup.ExceptedQuantityList.ContainsCode(UNDGSubstanceLookups.ExceptedQuantity.Code.E3));
			Assert(lookup.ExceptedQuantityList.ContainsCode(UNDGSubstanceLookups.ExceptedQuantity.Code.E4));
			Assert(lookup.ExceptedQuantityList.ContainsCode(UNDGSubstanceLookups.ExceptedQuantity.Code.E5));
		}

		public void TestPackingGroupList()
		{
			var item = Factory.New<UNDGSubstanceADR>();
			var lookup = new UNDGSubstanceADRLookups(item);

			AssertEquals("Expecting 3", 3, lookup.PackingGroupList.Count);
			AssertEquals(UNDGSubstanceLookups.PackingGroupTypes.HighDanger, lookup.PackingGroupList.GetDescriptionFromCode(UNDGSubstanceLookups.PackingGroupTypes.HighDangerCode));
			AssertEquals(UNDGSubstanceLookups.PackingGroupTypes.MediumDanger, lookup.PackingGroupList.GetDescriptionFromCode(UNDGSubstanceLookups.PackingGroupTypes.MediumDangerCode));
			AssertEquals(UNDGSubstanceLookups.PackingGroupTypes.LowDanger, lookup.PackingGroupList.GetDescriptionFromCode(UNDGSubstanceLookups.PackingGroupTypes.LowDangerCode));
		}
	}
}
