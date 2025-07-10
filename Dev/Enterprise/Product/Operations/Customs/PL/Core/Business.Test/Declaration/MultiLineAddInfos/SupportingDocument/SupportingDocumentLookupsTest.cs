using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.PL.Business.Declaration.Testing;

public class SupportingDocumentLookupsTest : BusinessObjectLookupsTestCase
{
	public void TestCustomsUQList()
	{
		var dec = Factory.New<JobDeclaration>();
		var supportingDocumentForLine = dec.Invoices.AddNew().InvoiceLines.AddNew().SupportingDocuments.AddNew();

		var testCases = new[]
		{
			new { declarationType = "IMP", codeType = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsUQ, cusCodes = new string[] { "TESTA", "TESTB" } },
			new { declarationType = "EXP", codeType = UniversalReferenceConstants.RefCusCodeListType.Codes.ExportCustomsDeclarationUnitsOfQuantity, cusCodes = new string[] { "TESTC", "TESTD" } },
		};

		CombineAssertions(() =>
		{
			foreach (var item in testCases)
			{
				InitializeListOfCusCodesForCodeType(item.codeType, item.cusCodes);
				dec.JE_MessageType = item.declarationType;
				var uqList = supportingDocumentForLine.Lookups.UnitOfQuantityList;
				AssertContainsExactElementsInAnyOrder($"{item.declarationType} - UQ list comes from ZZ db.", item.cusCodes, uqList.GetAllCodes());
			}
		});
	}

	void InitializeListOfCusCodesForCodeType(string cusCodeType, string[] codesToAdd)
	{
		const string countryCodePL = Core.Constants.CountryCodes.Poland;

		var helper = new UniversalReferenceTestDataHelper(Factory);
		helper.CreateNewOrGetExistingDataGrouping(countryCodePL, "Poland");
		helper.CreateNewOrGetExistingCusCodeType(cusCodeType, $"Test code type {cusCodeType}", countryCodePL);
		foreach (var code in codesToAdd)
		{
			helper.CreateNewOrGetExistingCusCodeList(countryCodePL, cusCodeType, code, $"Test code {code}", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		}
		Factory.Save();
	}

	public void TestCodeList_Export()
	{
		AssertCodeList(MessageTypeList.Codes.Export, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfExportDirection);
	}

	public void TestCodeList_Import()
	{
		AssertCodeList(MessageTypeList.Codes.Import, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfImportDirection);
	}

	void AssertCodeList(string messageType, string codeType)
	{
		var helper = new UniversalReferenceTestDataHelper(Factory);
		var eun = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "European Union");
		helper.CreateNewOrGetExistingDataGrouping(GlbCompany.CurrentCompany.Country.Code, GlbCompany.CurrentCompany.Country.RN_Desc, eun);
		var attributeNameValuePairs = new Dictionary<string, string[]> { { "Level", new[] { "ITEM" } } };
		helper.CreateCusCodeListsForMultipleTypesWithAttributes(GlbCompany.CurrentCompany.Country.Code,
			new[] { codeType }, "1111", "Description 1", attributeNameValuePairs, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		helper.CreateCusCodeListsForMultipleTypesWithAttributes(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN,
			new[] { codeType }, "2222", "Description 2", attributeNameValuePairs, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		Factory.Save();

		var declaration = Factory.New<JobDeclaration>();
		var invoiceHeader = declaration.Invoices.AddNew();
		var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
		var supportingDocument = invoiceLine.SupportingDocuments.AddNew();

		declaration.JE_MessageType = messageType;
		var collection = (ZZRefCusCodeListCombinedCollection)supportingDocument.Lookups.CodeList;
		collection.Load();
		CombineAssertions(() =>
		{
			AssertEquals("Contains only one element", 1, collection.Count);
			AssertEquals("Contains current country code", "1111", collection[0].ZZD_Code);
		});
	}
}
