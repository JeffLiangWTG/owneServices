using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using RefCusCodeListTypes = Enterprise.Core.Constants.Customs.Universal.RefCusCodeListTypes;

namespace Enterprise.Customs.TR.Manifest.Business.Testing
{
	public class AsycudaPackLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestPackUQList()
		{
			var helper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(RefCusCodeListTypes.Codes.PackageTypes, "PKG");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Turkey, RefCusCodeListTypes.Codes.PackageTypes, "YGT", "YGT", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Turkey);
			Factory.Save();
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var pack = bill.Packs.AddNew();
			var list = pack.Lookups.PackUQList;
			Assert(list.ContainsCode("YGT"));
		}
	}
}
