using CargoWise.EntityFramework.Testing;
using CustomsConstants = Enterprise.Core.Constants.Customs.Universal;

namespace Enterprise.Customs.Universal.Testing
{
	public class RefCusCodeTypeListTest : TestCaseWithFactory
	{
		public void TestGetListByCountry()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var codeType1 = helper.CreateCusCodeType(CustomsConstants.RefCusCodeListTypes.Codes.Facilities, "FAC DESC");
			codeType1.ZZK_ZZZ_NKDataGrouping = Core.Constants.CountryCodes.Ethiopia;
			codeType1.ZZK_IsReadonly = false;
			var codeType2 = helper.CreateCusCodeType(CustomsConstants.RefCusCodeListTypes.Codes.ExciseProductCodes, "EPC DESC");
			codeType2.ZZK_ZZZ_NKDataGrouping = Core.Constants.CountryCodes.Ethiopia;
			codeType2.ZZK_IsReadonly = true;
			var codeType3 = helper.CreateCusCodeType(CustomsConstants.RefCusCodeListTypes.Codes.Port, "PORT DESC");
			codeType3.ZZK_ZZZ_NKDataGrouping = Core.Constants.CountryCodes.Ethiopia;
			codeType3.ZZK_IsReadonly = false;
			var codeType4 = helper.CreateCusCodeType(CustomsConstants.RefCusCodeListTypes.Codes.CustomsOffice, "CUSOF DESC");
			codeType4.ZZK_ZZZ_NKDataGrouping = Core.Constants.CountryCodes.Ethiopia;
			codeType4.ZZK_IsReadonly = false;
			var codeType5 = helper.CreateCusCodeType(CustomsConstants.RefCusCodeListTypes.Codes.CustomsUQ, "CUSUQ DESC");
			codeType5.ZZK_ZZZ_NKDataGrouping = Core.Constants.CountryCodes.Ethiopia;
			codeType5.ZZK_IsReadonly = false;
			Factory.Save();
			var list1 = RefCusCodeTypeList.GetListByCountry(Factory, "ET", false);
			AssertContainsExactElementsInAnyOrder(new string[] { "CUSOF", "CUSUQ", "EPC", "FAC", "PORT" }, list1.GetAllCodes());
			var list2 = RefCusCodeTypeList.GetListByCountry(Factory, "ET", true);
			AssertContainsExactElementsInAnyOrder(new string[] { "CUSOF", "CUSUQ", "FAC", "PORT" }, list2.GetAllCodes());
			AssertEquals("FAC DESC", list2.GetDescriptionFromCode("FAC"));
		}

		public void TestGetListByCountryWithCusCodeTypeLanguage()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var languageCode = TranslationHelper.GetCurrentLanguageCode();
			helper.CreateOrGetLanguage(languageCode, "Test");
			helper.CreateOrGetLanguage(languageCode + "M", "Test");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Brazil, Core.Constants.CountryCodes.Brazil);

			var refCusCodeType1 = helper.CreateCusCodeType("CUSOF", "Customs Office", Core.Constants.CountryCodes.Brazil);
			var refCusCodeTypeLanguage1 = helper.CreateCusCodeTypeLanguage(refCusCodeType1, languageCode, "BR Customs Office Description");
			var refCusCodeType2 = helper.CreateCusCodeType("ADDCD", "Additional Codes", Core.Constants.CountryCodes.Brazil);
			helper.CreateCusCodeTypeLanguage(refCusCodeType2, languageCode + "M", "BRM Additional Codes Description");
			var refCusCodeType3 = helper.CreateCusCodeType("CURR", "Currency", Core.Constants.CountryCodes.Brazil);

			Factory.Save();

			var list = RefCusCodeTypeList.GetListByCountry(Factory, Core.Constants.CountryCodes.Brazil, false);
			AssertEquals(3, list.Count);
			AssertEquals(refCusCodeTypeLanguage1.ZXI_Description, list.GetDescriptionFromCode("CUSOF"));
			AssertEquals(refCusCodeType2.ZZK_Description, list.GetDescriptionFromCode("ADDCD"));
			AssertEquals(refCusCodeType3.ZZK_Description, list.GetDescriptionFromCode("CURR"));
		}

		public void TestGetListByCountry_IncludeParentDataGrouping()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var parentDataGrouping = helper.CreateNewOrGetExistingDataGrouping(CustomsConstants.RefDataGrouping.Codes.EuropeanUnionEUN, "European Union");
			const string germanyCountryCode = Core.Constants.CountryCodes.Germany;
			helper.CreateNewOrGetExistingDataGrouping(germanyCountryCode, "Germany", parentDataGrouping);
			var codeType1 = helper.CreateCusCodeType(CustomsConstants.RefCusCodeListTypes.Codes.ValuationMethod, "VALMT DESC");
			codeType1.ZZK_ZZZ_NKDataGrouping = CustomsConstants.RefDataGrouping.Codes.EuropeanUnionEUN;
			Factory.Save();
			var list1 = RefCusCodeTypeList.GetListByCountry(Factory, germanyCountryCode, false, true);
			AssertCollectionContains("Parent DataGrouping code should be included.", "VALMT", list1.GetAllCodes());
		}
	}
}
