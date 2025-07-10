using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Universal.Testing
{
	[TestedType(typeof(RefCusCodeListLanguage))]
	public class RefCusCodeListLanguageTest : EnterpriseBusinessObjectTestCase
	{
		protected override BusinessObject GetBusinessObjectForFetchForLoad()
		{
			return CreateNewCodeListLanguage(Factory);
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			return CreateNewCodeListLanguage(factory);
		}

		RefCusCodeListLanguage CreateNewCodeListLanguage(BusinessObjectFactory factory)
		{
			var helper = new UniversalReferenceTestDataHelper(factory);
			helper.CreateOrGetLanguage("ENG", "English");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN);
			var codeType = helper.CreateCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Facilities, "Facilities", Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN);
			var codeList = factory.New<RefCusCodeList>();
			codeList.ZZD_ZZK_NKCodeType = codeType.ZZK_CodeType;
			codeList.ZZD_ZZZ_NKDataGrouping = Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN;
			codeList.ZZD_StartDate = ZDateTime.Today;
			codeList.ZZD_EndDate = ZDateTime.Today.AddYears(1);
			codeList.ZZD_Code = "ZXZ";
			codeList.ZZD_Description = "Description";
			var language = factory.New<RefCusCodeListLanguage>();
			language.ZXA_ZZD_CodeList = codeList.PK;
			language.ZXA_ZX6_NKLanguage = "ENG";
			language.ZXA_Description = "Test Description";
			return language;
		}
	}
}
