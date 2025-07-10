using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.PL.Business.Declaration.Testing;

class ImportPreviousDocumentCodeListTest : TestCaseWithFactory
{
	public void TestConstructor()
	{
		var helper = new UniversalReferenceTestDataHelper(Factory);
		var parentDataGrouping = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.EuropeanUnion, "EU");
		helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Poland, "Poland", parentDataGrouping);
		helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.PreviousDocumentOfImportDirection, "Import");
		helper.CreateNewOrGetExistingCusCodeType(UniversalReferenceConstants.RefCusCodeListType.Codes.ExportPreviousDocumentSpecialProcedures, "Special Export");
		helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.PreviousDocumentOfExportDirection, "Export");
		helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Poland, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.PreviousDocumentOfExportDirection, "123", "Description 1", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Poland, UniversalReferenceConstants.RefCusCodeListType.Codes.ExportPreviousDocumentSpecialProcedures, "321", "Description 2", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Poland, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.PreviousDocumentOfImportDirection, "555", "Description 3", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.EuropeanUnion, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.PreviousDocumentOfExportDirection, "999", "EU Description 1", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.EuropeanUnion, UniversalReferenceConstants.RefCusCodeListType.Codes.ExportPreviousDocumentSpecialProcedures, "888", "EU Description 2", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.EuropeanUnion, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.PreviousDocumentOfImportDirection, "777", "EU Description 3", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		Factory.Save();

		var codeList = new ImportPreviousDocumentCodeList(Factory);
		CombineAssertions(() =>
		{
			var codeDescriptionPairList = (CodeDescriptionPairList)codeList;
			AssertEquals("Should not contain code 321", false, codeDescriptionPairList.GetAllCodes().Contains("123"));
			AssertEquals("Should not contain code 321", false, codeDescriptionPairList.GetAllCodes().Contains("321"));
			AssertEquals("Should not contain EUN code 777", false, codeDescriptionPairList.GetAllCodes().Contains("777"));
			AssertEquals("Should not contain EUN code 888", false, codeDescriptionPairList.GetAllCodes().Contains("888"));
			AssertEquals("Should not contain EUN code 999", false, codeDescriptionPairList.GetAllCodes().Contains("999"));
			AssertEquals("Contains code 555", true, codeDescriptionPairList.GetAllCodes().Contains("555"));
			AssertEquals("Description", "Description 3", codeDescriptionPairList.GetDescriptionFromCode("555"));
		});
	}
}
