//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoItemPackagingAddInfoLookups
//
//    This class should be used for overriding collections in AutoItemPackagingAddInfoLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using CargoWise.Types;

namespace Enterprise.Customs.NZ.Business.Declaration.Testing
{
	using CargoWise.EntityFramework.Testing;
	using Enterprise.Customs.Universal.Testing;

	internal class ItemPackagingAddInfoLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestPackageUQList()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes, "UN Package List");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.UnitedNationsRecommendations, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes,
				"AAA", "AAAAA", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.UnitedNationsRecommendations, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes,
				"BG", "BG DESC", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.UnitedNationsRecommendations, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes,
				"BBB", "BBB DESC", new ZDateTime(2012, 3, 3), ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.UnitedNationsRecommendations);

			Factory.Save();

			Assert("BG", Lookups.PackageUQList.ContainsCode("BG"));
			Assert("BAG should not appear as it is not in the list", !Lookups.PackageUQList.ContainsCode("BAG"));
			Assert("BBB should not appear as it is too new", !Lookups.PackageUQList.ContainsCode("BBB"));
			Assert("AAA", Lookups.PackageUQList.ContainsCode("AAA"));
		}

		ItemPackagingAddInfoLookups Lookups
		{
			get { return lookups ?? (lookups = new ItemPackagingAddInfoLookups(ItemPackagingAddInfo)); }
		}
		ItemPackagingAddInfoLookups lookups;

		ItemPackagingAddInfo ItemPackagingAddInfo
		{
			get
			{
				if (fItemPackagingAddInfo == null)
				{
					var itemPackaging = Factory.New<ItemPackaging>();
					fItemPackagingAddInfo = new ItemPackagingAddInfo(itemPackaging.B7_AddInfoDataInfo);
				}
				return fItemPackagingAddInfo;
			}
		}
		ItemPackagingAddInfo fItemPackagingAddInfo;
	}
}
