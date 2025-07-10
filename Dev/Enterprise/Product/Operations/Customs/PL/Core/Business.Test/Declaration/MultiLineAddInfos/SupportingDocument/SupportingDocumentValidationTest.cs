using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;

namespace Enterprise.Customs.PL.Business.Declaration.Testing;

abstract class SupportingDocumentValidationTest : TestCaseWithFactory
{
	public void TestCheckCSI_UnitOfQuantity()
	{
		var helper = new UniversalReferenceTestDataHelper(Factory);
		helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Poland, "Poland");
		helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsUQ, "CustomsUQ");
		helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Poland, UniversalReferenceConstants.RefCusCodeListType.Codes.ExportCustomsDeclarationUnitsOfQuantity, "tstC", "test c", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		Factory.Save();
		var declaration = Factory.New<JobDeclaration>();
		var suppDoc = declaration.Invoices.AddNew().InvoiceLines.AddNew().SupportingDocuments.AddNew();

		CombineAssertions(() =>
		{
			AssertNoNotifications("Empty supporting ddocument", suppDoc.CSI_UnitOfQuantityInfo);

			suppDoc.CSI_Quantity = 1.0;
			suppDoc.Validation.ValidateCSI_UnitOfQuantity();
			AssertHasMessageErrorContaining("CSI_UnitOfQuantity is empty with not empty CSI_Quantity", suppDoc.CSI_UnitOfQuantityInfo, MandatoryValidation.YouHaveNotEntered);

			suppDoc.CSI_UnitOfQuantity = "a";
			AssertHasMessageErrorContaining("CSI_UnitOfQuantity is invalid", suppDoc.CSI_UnitOfQuantityInfo, ListValidation.InvalidCodeMessageError.ToString());

			suppDoc.CSI_UnitOfQuantity = "tstC";
			AssertNoNotifications("valid CSI_UnitOfQuantity", suppDoc.CSI_UnitOfQuantityInfo);
		});
	}

	public void TestCheckCSI_Quantity()
	{
		var declaration = Factory.New<JobDeclaration>();
		var suppDoc = declaration.Invoices.AddNew().InvoiceLines.AddNew().SupportingDocuments.AddNew();

		CombineAssertions(() =>
		{
			AssertNoNotifications("Empty supporting ddocument", suppDoc.CSI_QuantityInfo);

			suppDoc.CSI_UnitOfQuantity = "a";
			suppDoc.Validation.ValidateCSI_Quantity();
			AssertHasMessageErrorContaining("CSI_Quantity is empty", suppDoc.CSI_QuantityInfo, MandatoryValidation.ValueCannotBeZero);

			suppDoc.CSI_Quantity = 1.0;
			AssertNoNotifications("CSI_Quantity is not empty", suppDoc.CSI_QuantityInfo);
		});
	}

	public void TestCheckCSI_ReferenceNumber()
	{
		var declaration = Factory.New<JobDeclaration>();
		var suppDoc = declaration.SupportingDocuments.AddNew();

		CombineAssertions(() =>
		{
			suppDoc.CSI_ReferenceNumber = "a";
			AssertNoMessageErrorContaining("Not empty Reference Number", suppDoc.CSI_ReferenceNumberInfo, MandatoryValidation.YouHaveNotEntered);

			suppDoc.CSI_ReferenceNumber = ZString.Empty;
			AssertHasMessageErrorContaining("Empty Reference Number", suppDoc.CSI_ReferenceNumberInfo, MandatoryValidation.YouHaveNotEntered);
		});
	}

	public void TestDocumentUniqueness()
	{
		void setupDocumentProperties(
			SupportingDocument document,
			string code = "N001",
			string refNumber = "12345",
			short itemNumber = 123,
			string unitOfQuantity = "KG",
			string currency = "PLN",
			string description = "ABCDEF")
		{
			document.CSI_ReferenceNumber = refNumber;
			document.CSI_ItemNumber = itemNumber;
			document.CSI_UnitOfQuantity = unitOfQuantity;
			document.CSI_RX_NKCurrency = currency;
			document.CSI_AdditionalDescription = description;
			document.CSI_Code = code;
		}

		var invoiceLine = Factory.New<JobDeclaration>().Invoices.AddNew().JobComInvoiceLines.AddNew();

		const string expectedErrorMessage = "A row with that document type and reference already exists on this invoice line.";
		CombineAssertions(() =>
		{
			var document1 = invoiceLine.SupportingDocuments.AddNew();
			setupDocumentProperties(document1);
			AssertNoMessageError("Single", document1.CSI_CodeInfo, expectedErrorMessage);

			var document2 = invoiceLine.SupportingDocuments.AddNew();
			setupDocumentProperties(document2);
			AssertHasMessageError("Duplicate", document2.CSI_CodeInfo, expectedErrorMessage);

			setupDocumentProperties(document2, code: "N002");
			AssertNoMessageError("Different Code", document2.CSI_CodeInfo, expectedErrorMessage);

			setupDocumentProperties(document2, refNumber: "54321");
			AssertNoMessageError("Different Reference Number", document2.CSI_CodeInfo, expectedErrorMessage);

			setupDocumentProperties(document2, itemNumber: 456);
			AssertNoMessageError("Different Item Number", document2.CSI_CodeInfo, expectedErrorMessage);

			setupDocumentProperties(document2, unitOfQuantity: "TN");
			AssertNoMessageError("Different Unit Of Quantity", document2.CSI_CodeInfo, expectedErrorMessage);

			setupDocumentProperties(document2, currency: "EUR");
			AssertNoMessageError("Different Currency", document2.CSI_CodeInfo, expectedErrorMessage);

			setupDocumentProperties(document2, description: "FEDCBA");
			AssertNoMessageError("Different Additional Description", document2.CSI_CodeInfo, expectedErrorMessage);
		});
	}
}
