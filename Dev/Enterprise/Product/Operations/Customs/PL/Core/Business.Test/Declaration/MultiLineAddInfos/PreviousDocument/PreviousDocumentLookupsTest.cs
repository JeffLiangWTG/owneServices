using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal.Testing;

namespace Enterprise.Customs.PL.Business.Declaration.Testing;

sealed class PreviousDocumentLookupsTest : BusinessObjectLookupsTestCase
{
	public void TestSubTypeList()
	{
		var declaration = Factory.New<JobDeclaration>();
		var previousDocument = declaration.Invoices.AddNew().PreviousDocuments.AddNew();

		CombineAssertions(() =>
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			var codesAsString = previousDocument.Lookups.SubTypeList.CodesAsString;
			AssertEquals("Import", "ECO, STD", codesAsString);

			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			codesAsString = previousDocument.Lookups.SubTypeList.CodesAsString;
			AssertNotEquals("Export", "ECO, STD", codesAsString);
		});
	}

	public void TestUnitOfQuantityList()
	{
		var helper = new UniversalReferenceTestDataHelper(Factory);
		helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Poland, "Poland");
		helper.CreateNewOrGetExistingCusCodeType(UniversalReferenceConstants.RefCusCodeListType.Codes.ExportCustomsDeclarationUnitsOfQuantity, "UoQ");
		helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Poland, UniversalReferenceConstants.RefCusCodeListType.Codes.ExportCustomsDeclarationUnitsOfQuantity, "123", "Description 1", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Poland, UniversalReferenceConstants.RefCusCodeListType.Codes.ExportCustomsDeclarationUnitsOfQuantity, "321", "Description 2", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Poland, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsUQ, "444", "Description 3", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		Factory.Save();

		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
		var invoice = declaration.Invoices.AddNew();
		var previousDocument = invoice.PreviousDocuments.AddNew();
		var unitOfQuantityList = previousDocument.Lookups.UnitOfQuantityList;
		CombineAssertions(() =>
		{
			AssertEquals("Contains code 123", true, unitOfQuantityList.GetAllCodes().Contains("123"));
			AssertEquals("Contains code 321", true, unitOfQuantityList.GetAllCodes().Contains("321"));
			AssertEquals("Description for code 321", "Description 2", unitOfQuantityList.GetDescriptionFromCode("321"));

			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			unitOfQuantityList = previousDocument.Lookups.UnitOfQuantityList;
			AssertEquals("Contains code 444", true, unitOfQuantityList.GetAllCodes().Contains("444"));
			AssertEquals("Description for code 444", "Description 3", unitOfQuantityList.GetDescriptionFromCode("444"));
		});
	}

	public void TestPackTypeList()
	{
		var helper = new UniversalReferenceTestDataHelper(Factory);
		helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Poland, "Poland");
		helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes, "UoQ2");
		helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Poland, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes, "123", "Description 1", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Poland, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes, "321", "Description 2", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		Factory.Save();

		var declaration = Factory.New<JobDeclaration>();
		var invoice = declaration.Invoices.AddNew();
		var previousDocument = invoice.PreviousDocuments.AddNew();
		var packTypeList = previousDocument.Lookups.PackTypeList;
		CombineAssertions(() =>
		{
			AssertEquals("Contains code 123", true, packTypeList.GetAllCodes().Contains("123"));
			AssertEquals("Contains code 321", true, packTypeList.GetAllCodes().Contains("321"));
			AssertEquals("Description", "Description 2", packTypeList.GetDescriptionFromCode("321"));
		});
	}

	public void TestCodeList_Export()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
		var invoice = declaration.Invoices.AddNew();
		var previousDocument = invoice.PreviousDocuments.AddNew();
		AssertType<StandardExportDocumentCodeList>(previousDocument.Lookups.CodeList);
	}

	public void TestCodeList_Export_InvoiceLine()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
		var invoice = declaration.Invoices.AddNew();
		var invoiceLine = invoice.InvoiceLines.AddNew();
		var previousDocument = invoiceLine.PreviousDocuments.AddNew();
		AssertType<ExportPreviousDocumentCodeList>(previousDocument.Lookups.CodeList);
	}

	public void TestCodeList_Import()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		var invoice = declaration.Invoices.AddNew();
		var previousDocument = invoice.PreviousDocuments.AddNew();
		AssertType<ImportPreviousDocumentCodeList>(previousDocument.Lookups.CodeList);
	}
}
