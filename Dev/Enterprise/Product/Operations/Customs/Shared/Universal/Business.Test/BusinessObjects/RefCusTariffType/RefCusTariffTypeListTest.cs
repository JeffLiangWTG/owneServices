using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.Universal.Testing
{
	class RefCusTariffTypeListTest : TestCaseWithFactory
	{
		public void TestGetList()
		{
			var tariffType1 = CreateTariffType(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "BOB");
			var tariffType2 = CreateTariffType(Core.Constants.CountryCodes.Eritrea, "B1B");
			var tariffType3 = CreateTariffType(Core.Constants.CountryCodes.Estonia, "BOB");
			var tariffType4 = CreateTariffType(Core.Constants.CountryCodes.Eritrea, "B2B");
			Factory.Save();
			var list1 = RefCusTariffTypeList.GetCachedList(Factory, Core.Constants.CountryCodes.Estonia);
			var list2 = RefCusTariffTypeList.GetCachedList(Factory, Core.Constants.CountryCodes.Estonia);
			AssertEquals("Is cached", list2, list1);
			AssertEquals("list1.Count", 1, list1.Count);
			AssertEquals("description", Core.Constants.CountryCodes.Estonia + "BOB DESC", list1.GetDescriptionFromCode("BOB"));
			var list3 = RefCusTariffTypeList.GetCachedList(Factory, Core.Constants.CountryCodes.Eritrea);
			var list4 = RefCusTariffTypeList.GetCachedList(Factory, Core.Constants.CountryCodes.Eritrea);
			AssertNotEquals("Is not same", list1, list3);
			AssertEquals("Is cached", list4, list3);
			AssertEquals("list1.Count", 3, list3.Count);
			AssertEquals("description", Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN + "BOB DESC", list3.GetDescriptionFromCode("BOB"));
			AssertEquals("description", Core.Constants.CountryCodes.Eritrea + "B1B DESC", list3.GetDescriptionFromCode("B1B"));
			AssertEquals("description", Core.Constants.CountryCodes.Eritrea + "B2B DESC", list3.GetDescriptionFromCode("B2B"));
			list1 = RefCusTariffTypeList.GetCachedList(Factory, "!#");
			list2 = RefCusTariffTypeList.GetCachedList(Factory, "!#");
			AssertEquals("Is cached", list2, list1);
			AssertEquals("list1.Count", 0, list1.Count);
		}

		public void TestGetTariffTypesForCountry()
		{
			var tariffType1 = CreateTariffType(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "BOB");
			var tariffType2 = CreateTariffType(Core.Constants.CountryCodes.Eritrea, "B1B");
			var tariffType3 = CreateTariffType(Core.Constants.CountryCodes.Estonia, "BOB");
			var tariffType4 = CreateTariffType(Core.Constants.CountryCodes.Eritrea, "B2B");
			Factory.Save();
			var list1 = RefCusTariffTypeList.GetTariffTypesForCountry(Factory, Core.Constants.CountryCodes.Estonia);
			AssertEquals("list1.Count", 1, list1.Length);
			var list2 = RefCusTariffTypeList.GetTariffTypesForCountry(Factory, Core.Constants.CountryCodes.Eritrea);
			AssertEquals("list2.Count", 3, list2.Length);
			var list3 = RefCusTariffTypeList.GetTariffTypesForCountry(Factory, "!#");
			AssertEquals("list3.Count", 0, list3.Length);
		}

		RefCusTariffType CreateTariffType(ZString country, ZString type, string desc = null)
		{
			var tariffType = Factory.New<RefCusTariffType>();
			tariffType.ZZI_TariffType = type;
			tariffType.ZZI_Description = country + type + " DESC";
			tariffType.ZZI_ZZZ_NKDataGrouping = country;
			return tariffType;
		}

		protected override void SetUp()
		{
			base.SetUp();
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var eunGrouping = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Eritrea, parent: eunGrouping);
		}
	}
}
