using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using static Enterprise.Core.Constants.Customs.Universal;

namespace Enterprise.Customs.ZA.Manifest.Business.Testing
{
	class AsycudaPackLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestPackUQList()
		{
			var helper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(RefCusCodeListTypes.Codes.UnitedNationsPackageTypes, "UNPKG");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.UnitedNationsRecommendations, RefCusCodeListTypes.Codes.UnitedNationsPackageTypes, "Test", "Test", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.UnitedNationsRecommendations);
			Factory.Save();
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var pack = bill.Packs.AddNew();
			var list = pack.Lookups.PackUQList;
			AssertNotEquals(typeof(RefPackTypeCollection), list.GetType());
			Assert(list.ContainsCode("Test"));
		}
	}
}
