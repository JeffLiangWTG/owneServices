using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;

namespace Enterprise.Customs.PL.Business.Declaration.Testing;

sealed class StandardExportDocumentCodeListTest : TestCaseWithFactory
{
	public void TestConstructor()
	{
		var helper = new UniversalReferenceTestDataHelper(Factory);
		var euGrouping = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "Europe");
		helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Poland, "Poland", euGrouping);
		helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.PreviousDocumentOfExportDirection, "1");
		helper.CreateNewOrGetExistingCusCodeType(UniversalReferenceConstants.RefCusCodeListType.Codes.ExportPreviousDocumentSpecialProcedures, "2");
		helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Poland, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.PreviousDocumentOfExportDirection, "123", "Description 1", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Poland, UniversalReferenceConstants.RefCusCodeListType.Codes.ExportPreviousDocumentSpecialProcedures, "321", "Description 2", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		helper.CreateNewOrGetExistingCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.PreviousDocumentOfExportDirection, "222", "Description 1", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		helper.CreateNewOrGetExistingCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, UniversalReferenceConstants.RefCusCodeListType.Codes.ExportPreviousDocumentSpecialProcedures, "333", "Description 2", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		Factory.Save();

		var codeList = new StandardExportDocumentCodeList(Factory);
		CombineAssertions(() =>
		{
			Assert("Contains code 123", codeList.GetAllCodes().Contains("123"));
			Assert("Not contains special procedure code 321", !codeList.GetAllCodes().Contains("321"));
			Assert("Not contains EUN code 222", !codeList.GetAllCodes().Contains("222"));
			Assert("Not contains EUN code 333", !codeList.GetAllCodes().Contains("333"));
			AssertEquals("Description", "Description 1", codeList.GetDescriptionFromCode("123"));
			AssertEquals("Description should be null", null, codeList.GetDescriptionFromCode("321"));
		});
	}
}
