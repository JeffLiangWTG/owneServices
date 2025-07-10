using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;

namespace Enterprise.Customs.PL.Business.Declaration.Testing;

class PreviousDocumentValidationTest : BusinessObjectValidationTestCase
{
	public void TestCheckCSI_ReferenceNumber()
	{
		var messageError = "You have not entered a Reference.";
		var previousDocument = GetPreviousDocument();
		var propertyInfo = previousDocument.CSI_ReferenceNumberInfo;
		CombineAssertions(() =>
		{
			previousDocument.Validation.ValidateCSI_ReferenceNumber();
			AssertHasMessageError("Empty", propertyInfo, messageError);
			previousDocument.CSI_ReferenceNumber = "123";
			previousDocument.Validation.ValidateCSI_ReferenceNumber();
			AssertNoMessageError("Not Empty", propertyInfo, messageError);
		});
	}

	public void TestCheckCSI_Quantity()
	{
		var messageError = "Quantity should be an integer value.";
		var previousDocument = GetPreviousDocument();
		var propertyInfo = previousDocument.CSI_QuantityInfo;
		CombineAssertions(() =>
		{
			previousDocument.CSI_UnitOfQuantity = Constants.SupportingDocumentUnitOfQuantityCodes.NumberOfCells;
			previousDocument.Validation.ValidateCSI_Quantity();
			AssertNoMessageError("Empty quantity", propertyInfo, messageError);
			previousDocument.CSI_Quantity = 123M;
			previousDocument.Validation.ValidateCSI_Quantity();
			AssertNoMessageError("Integer quantity", propertyInfo, messageError);
			previousDocument.CSI_UnitOfQuantity = "ABC";
			previousDocument.CSI_Quantity = 123.41M;
			previousDocument.Validation.ValidateCSI_Quantity();
			AssertNoMessageError("UnitOfQuantity doesn't require integer", propertyInfo, messageError);
			previousDocument.CSI_UnitOfQuantity = Constants.SupportingDocumentUnitOfQuantityCodes.NumberOfCells;
			previousDocument.Validation.ValidateCSI_Quantity();
			AssertHasMessageError("Message error", propertyInfo, messageError);
		});
	}

	public void TestCheckCSI_UnitOfQuantity()
	{
		const string codeType = UniversalReferenceConstants.RefCusCodeListType.Codes.ExportCustomsDeclarationUnitsOfQuantity;
		const string countryCodePL = Core.Constants.CountryCodes.Poland;

		var helper = new UniversalReferenceTestDataHelper(Factory);
		helper.CreateNewOrGetExistingDataGrouping(countryCodePL, "Poland");
		helper.CreateNewOrGetExistingCusCodeType(codeType, "TestUQ", countryCodePL);
		helper.CreateNewOrGetExistingCusCodeList(countryCodePL, codeType, "ABC", "Test ABC", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		Factory.Save();

		var messageErrorNotEntered = "You have not entered a Unit Of Quantity.";
		var messageErrorNotInTheList = "The code you have selected is not in the list.";
		var previousDocument = GetPreviousDocument();
		var propertyInfo = previousDocument.CSI_UnitOfQuantityInfo;
		CombineAssertions(() =>
		{
			previousDocument.Validation.ValidateCSI_UnitOfQuantity();
			AssertNoMessageError("Empty UoQ and Empty Quantity", propertyInfo, messageErrorNotEntered);
			AssertNoMessageError("Empty UoQ and Empty Quantity", propertyInfo, messageErrorNotInTheList);

			previousDocument.CSI_Quantity = 1;
			previousDocument.Validation.ValidateCSI_UnitOfQuantity();
			AssertHasMessageError("Empty UoQ and Not Empty Quantity", propertyInfo, messageErrorNotEntered);

			previousDocument.CSI_UnitOfQuantity = "A";
			previousDocument.Validation.ValidateCSI_UnitOfQuantity();
			AssertNoMessageError("Not Empty UoQ and Not Empty Quantity", propertyInfo, messageErrorNotEntered);
			AssertHasMessageError("UoQ is not in the list", propertyInfo, messageErrorNotInTheList);

			previousDocument.CSI_UnitOfQuantity = "ABC";
			previousDocument.Validation.ValidateCSI_UnitOfQuantity();
			AssertNoMessageError("UoQ is in the list", propertyInfo, messageErrorNotInTheList);
		});
	}

	public void TestCheckCSI_LineNo()
	{
		var helper = new UniversalReferenceTestDataHelper(Factory);
		helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Poland, "Poland");
		helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes, "1");
		helper.CreateNewOrGetExistingCusCodeType(UniversalReferenceConstants.RefCusCodeListType.Codes.ExportPreviousDocumentSpecialProcedures, "2");
		var cusCodeList1 = helper.CreateCusCodeListWithAttributeNames(Core.Constants.CountryCodes.Poland,
			Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.PreviousDocumentOfExportDirection, "123", "Description 1",
			ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue,
			new[] { new KeyValuePair<string, string>(Constants.RefCusCodeListAttributeName.ItemNumber, "Item Number") });
		var cusCodeList2 = helper.CreateCusCodeListWithAttributeNames(Core.Constants.CountryCodes.Poland,
			UniversalReferenceConstants.RefCusCodeListType.Codes.ExportPreviousDocumentSpecialProcedures, "321", "Description 2",
			ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue,
			new[] { new KeyValuePair<string, string>(Constants.RefCusCodeListAttributeName.ItemNumber, "Item Number") });

		helper.CreateCusCodeListAttribute(cusCodeList1.PK, Constants.RefCusCodeListAttributeName.ItemNumber, "E");
		helper.CreateCusCodeListAttribute(cusCodeList2.PK, Constants.RefCusCodeListAttributeName.ItemNumber, "R");
		var messageError = "Line No. cannot be zero.";
		var previousDocument = GetPreviousDocument();
		var propertyInfo = previousDocument.CSI_LineNoInfo;
		CombineAssertions(() =>
		{
			previousDocument.CSI_Code = "123";
			previousDocument.Validation.ValidateCSI_LineNo();
			AssertNoMessageError("Not mandatory", propertyInfo, messageError);
			previousDocument.CSI_Code = "321";
			previousDocument.Validation.ValidateCSI_LineNo();
			AssertHasMessageError("Mandatory and empty", propertyInfo, messageError);
			previousDocument.CSI_LineNo = 3;
			previousDocument.Validation.ValidateCSI_LineNo();
			AssertNoMessageError("Mandatory and not empty", propertyInfo, messageError);
		});
	}

	public void TestCheckRuleR1044()
	{
		var messageError = "(R1044) For Previous Document type 355 and 337 it’s required to enter Line no.";
		var previousDocument = GetPreviousDocument();
		CombineAssertions(() =>
		{
			previousDocument.CSI_Code = Constants.PreviousDocumentCodes.OGL;
			previousDocument.CSI_LineNo = 1;
			AssertNoMessageError("No message error when CSI_LineNo is not empty and Previous Document type is not 355 or 337", previousDocument.CSI_LineNoInfo, messageError);
			previousDocument.CSI_LineNo = 0;
			AssertNoMessageError("No message error when CSI_LineNo is empty and Previous Document type is not 355 or 337", previousDocument.CSI_LineNoInfo, messageError);
			previousDocument.CSI_Code = Constants.PreviousDocumentCodes._337;
			previousDocument.Validation.ValidateCSI_LineNo();
			AssertHasMessageError("It should show a message error when CSI_LineNo is empty and Previous Document type is 337", previousDocument.CSI_LineNoInfo, messageError);
			previousDocument.CSI_Code = Constants.PreviousDocumentCodes._355;
			previousDocument.Validation.ValidateCSI_LineNo();
			AssertHasMessageError("It should show a message error when CSI_LineNo is empty and Previous Document type is 355", previousDocument.CSI_LineNoInfo, messageError);
		});
	}

	PreviousDocument GetPreviousDocument()
	{
		var declaration = Factory.New<JobDeclaration>();
		var invoiceHeader = declaration.Invoices.AddNew();
		var previousDocument = invoiceHeader.PreviousDocuments.AddNew();
		return previousDocument;
	}
}
