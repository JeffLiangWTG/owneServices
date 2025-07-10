using CargoWise.Types;

namespace Enterprise.Customs.PL.Business.Declaration.Testing;

sealed class ExportSupportingDocumentValidationTest : SupportingDocumentValidationTest
{
	public void TestCheckCSI_Value()
	{
		const string errorMessage = "Value cannot be zero.";
		var declaration = GetExportDeclaration();
		var suppDoc = declaration.SupportingDocuments.AddNew();
		CombineAssertions(() =>
		{
			suppDoc.Validation.ValidateCSI_Value();
			AssertNoMessageError("CSI_RX_NKCurrency is empty", suppDoc.CSI_ValueInfo, errorMessage);
			suppDoc.CSI_RX_NKCurrency = "PLN";
			suppDoc.Validation.ValidateCSI_Value();
			AssertHasMessageError("CSI_RX_NKCurrency is entered, but CSI_Value is zero", suppDoc.CSI_ValueInfo, errorMessage);
			suppDoc.CSI_Value = 10;
			AssertNoMessageError("Valid", suppDoc.CSI_ValueInfo, errorMessage);
		});
	}

	public void TestCheckCSI_Value_NotNegative()
	{
		const string negativeErrorMessage = "Value cannot be negative.";
		var declaration = GetExportDeclaration();
		var suppDoc = declaration.SupportingDocuments.AddNew();
		CombineAssertions(() =>
		{
			suppDoc.CSI_Value = -10;
			AssertHasMessageError("Negative", suppDoc.CSI_ValueInfo, negativeErrorMessage);
			suppDoc.CSI_Value = 10;
			AssertNoMessageError("Valid", suppDoc.CSI_ValueInfo, negativeErrorMessage);
		});
	}

	public void TestCheckCSI_ValueIsValidMoney()
	{
		const string errorMessage = "The number 10,000,000,000,000,000 is too large, the value's range of Value is between -922337203685477.5808 and 922337203685477.5807.";
		var declaration = GetExportDeclaration();
		var suppDoc = declaration.SupportingDocuments.AddNew();
		CombineAssertions(() =>
		{
			suppDoc.CSI_Value = 10000000000000000M;
			suppDoc.Validation.ValidateCSI_Value();
			AssertHasError("Too large", suppDoc.CSI_ValueInfo, errorMessage);
			suppDoc.CSI_Value = 124312.51M;
			suppDoc.Validation.ValidateCSI_Value();
			AssertNoError("Valid", suppDoc.CSI_ValueInfo, errorMessage);
		});
	}

	public void TestCSI_RX_NKCurrency()
	{
		const string errorMessage = "You have not entered a Currency.";
		var declaration = GetExportDeclaration();
		var suppDoc = declaration.SupportingDocuments.AddNew();
		CombineAssertions(() =>
		{
			suppDoc.Validation.ValidateCSI_RX_NKCurrency();
			AssertNoMessageError("CSI_Value is zero", suppDoc.CSI_RX_NKCurrencyInfo, errorMessage);
			suppDoc.CSI_Value = 1;
			suppDoc.Validation.ValidateCSI_RX_NKCurrency();
			AssertHasMessageError("CSI_RX_NKCurrency is entered, but CSI_Value is zero", suppDoc.CSI_RX_NKCurrencyInfo, errorMessage);
			suppDoc.CSI_RX_NKCurrency = "PLN";
			AssertNoMessageError("Valid", suppDoc.CSI_RX_NKCurrencyInfo, errorMessage);
		});
	}

	public void TestCheckRuleR303()
	{
		var declaration = GetExportDeclaration();
		var instruction = declaration.CustomsEntryInstructions.AddNew();
		var invoice = declaration.Invoices.AddNew();
		var line = invoice.InvoiceLines.AddNew();

		var decSuppDoc = declaration.SupportingDocuments.AddNew();
		AssertCheckRuleR303(instruction, decSuppDoc);

		var invoiceSupportingDocument = invoice.SupportingDocuments.AddNew();
		AssertCheckRuleR303(instruction, invoiceSupportingDocument);

		var lineSupportingDocument = line.SupportingDocuments.AddNew();
		AssertCheckRuleR303(instruction, lineSupportingDocument);
	}

	void AssertCheckRuleR303(CusEntryInstruction entry, SupportingDocument suppDoc)
	{
		const string messageErrorA = "(R303) Supporting document code C512 can only be used for Entry Sub Style C,F or Y.";
		const string messageErrorB = "(R303) Supporting document code C513 can only be used for Entry Sub Style A,C,D,F or Y.";
		const string messageErrorC = "(R303) Supporting document code C514 can only be used for Entry Sub Style A,C,D,F,Y or Z.";

		var acceptedByC512CEI_SubStyle = new ZString[] { "C", "F", "Y" };
		var acceptedByC513CEI_SubStyle = new ZString[] { "A", "C", "D", "F", "Y" };
		var acceptedByC514CEI_SubStyle = new ZString[] { "A", "C", "D", "F", "Y", "Z" };

		suppDoc.CSI_Code = "C511";
		AssertNoMessageError(suppDoc.CSI_CodeInfo, messageErrorA);
		AssertNoMessageError(suppDoc.CSI_CodeInfo, messageErrorB);
		AssertNoMessageError(suppDoc.CSI_CodeInfo, messageErrorC);

		entry.CEI_SubStyle = "A";
		suppDoc.CSI_Code = "C512";
		AssertHasMessageError(suppDoc.CSI_CodeInfo, messageErrorA);
		AssertNoMessageError(suppDoc.CSI_CodeInfo, messageErrorB);
		AssertNoMessageError(suppDoc.CSI_CodeInfo, messageErrorC);

		suppDoc.CSI_Code = "C512";
		foreach (var item in acceptedByC512CEI_SubStyle)
		{
			entry.CEI_SubStyle = item;
			suppDoc.Validation.ValidateCSI_Code();
			AssertNoMessageError(suppDoc.CSI_CodeInfo, messageErrorA);
			AssertNoMessageError(suppDoc.CSI_CodeInfo, messageErrorB);
			AssertNoMessageError(suppDoc.CSI_CodeInfo, messageErrorC);
		}

		suppDoc.CSI_Code = "C513";
		entry.CEI_SubStyle = "B";
		suppDoc.Validation.ValidateCSI_Code();
		AssertNoMessageError(suppDoc.CSI_CodeInfo, messageErrorA);
		AssertHasMessageError(suppDoc.CSI_CodeInfo, messageErrorB);
		AssertNoMessageError(suppDoc.CSI_CodeInfo, messageErrorC);
		foreach (var item in acceptedByC513CEI_SubStyle)
		{
			entry.CEI_SubStyle = item;
			suppDoc.Validation.ValidateCSI_Code();
			AssertNoMessageError(suppDoc.CSI_CodeInfo, messageErrorA);
			AssertNoMessageError(suppDoc.CSI_CodeInfo, messageErrorB);
			AssertNoMessageError(suppDoc.CSI_CodeInfo, messageErrorC);
		}

		suppDoc.CSI_Code = "C514";
		entry.CEI_SubStyle = "B";
		suppDoc.Validation.ValidateCSI_Code();
		AssertNoMessageError(suppDoc.CSI_CodeInfo, messageErrorA);
		AssertNoMessageError(suppDoc.CSI_CodeInfo, messageErrorB);
		AssertHasMessageError(suppDoc.CSI_CodeInfo, messageErrorC);
		foreach (var item in acceptedByC514CEI_SubStyle)
		{
			entry.CEI_SubStyle = item;
			suppDoc.Validation.ValidateCSI_Code();
			AssertNoMessageError(suppDoc.CSI_CodeInfo, messageErrorA);
			AssertNoMessageError(suppDoc.CSI_CodeInfo, messageErrorB);
			AssertNoMessageError(suppDoc.CSI_CodeInfo, messageErrorC);
		}
	}

	JobDeclaration GetExportDeclaration()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
		return declaration;
	}
}
