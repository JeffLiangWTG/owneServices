using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using static Enterprise.Customs.PL.Business.Constants;

namespace Enterprise.Customs.PL.Business.Declaration.Testing;

sealed class ExportPreviousDocumentValidationTest : BusinessObjectValidationTestCase
{
	public void TestCheckCSI_DateOfExpiry()
	{
		var declaration = GetExportDeclaration();
		var invoice = declaration.Invoices.AddNew();
		var invoiceLine = invoice.InvoiceLines.AddNew();

		var declarationPreviousDocument = declaration.PreviousDocuments.AddNew();
		var invoicePreviousDocument = invoice.PreviousDocuments.AddNew();
		var invoiceLinePreviousDocument = invoiceLine.PreviousDocuments.AddNew();

		CombineAssertions("Export Previous Document CSI_DateOfIssue is not in use", () =>
		{
			AssertNoNotifications("declarationPreviousDocument - empty Declaration", declarationPreviousDocument.CSI_DateOfIssueInfo);
			AssertNoNotifications("invoicePreviousDocument - empty Declaration", invoicePreviousDocument.CSI_DateOfIssueInfo);
			AssertNoNotifications("invoiceLinePreviousDocument - empty Declaration", invoiceLinePreviousDocument.CSI_DateOfIssueInfo);

			declarationPreviousDocument.CSI_DateOfIssue = ZDateTime.Empty;
			invoicePreviousDocument.CSI_DateOfIssue = ZDateTime.Empty;
			invoiceLinePreviousDocument.CSI_DateOfIssue = ZDateTime.Empty;
			AssertNoNotifications("declarationPreviousDocument - empty DateOfIssue", declarationPreviousDocument.CSI_DateOfIssueInfo);
			AssertNoNotifications("invoicePreviousDocument - empty DateOfIssue", invoicePreviousDocument.CSI_DateOfIssueInfo);
			AssertNoNotifications("invoiceLinePreviousDocument - empty DateOfIssue", invoiceLinePreviousDocument.CSI_DateOfIssueInfo);

			declarationPreviousDocument.CSI_Code = "asd";
			declarationPreviousDocument.Validation.ValidateCSI_DateOfIssue();
			AssertNoNotifications("declarationPreviousDocument - empty DateOfIssue", declarationPreviousDocument.CSI_DateOfIssueInfo);

			declarationPreviousDocument.CSI_ReferenceNumber = "123456789";
			declarationPreviousDocument.Validation.ValidateCSI_DateOfIssue();
			AssertNoNotifications("declarationPreviousDocument - empty DateOfIssue", declarationPreviousDocument.CSI_DateOfIssueInfo);
		});
	}

	public void TestCheckCSI_LineNo_Mandatory()
	{
		var declaration = Factory.New<JobDeclaration>();
		var previousDocumentDeclaration = declaration.PreviousDocuments.AddNew();
		var invoice = declaration.Invoices.AddNew();
		var previousDocumentInvoice = invoice.PreviousDocuments.AddNew();
		var invoiceLine = invoice.InvoiceLines.AddNew();
		var previousDocumentLine = invoiceLine.PreviousDocuments.AddNew();
		CombineAssertions(() =>
		{
			AssertLineNumberMandatory("Declaration", previousDocumentDeclaration, expectedIsMandatory: false);
			AssertLineNumberMandatory("Invoice header", previousDocumentInvoice, expectedIsMandatory: false);
			AssertLineNumberMandatory("Invoice line", previousDocumentLine, expectedIsMandatory: true);
		});

		void AssertLineNumberMandatory(string description, PreviousDocument previousDocument, bool expectedIsMandatory)
		{
			const string messageError = "You have not entered a Line No. (Goods Item Number).";
			const string testSpecialProcedureCode = "MRN";

			var propertyInfo = previousDocument.CSI_LineNoInfo;

			previousDocument.CSI_LineNo = ZInt.Zero;
			previousDocument.CSI_Code = "123";
			AssertNoMessageError($"{description} - when LineNo=0 and CSI_Code is NOT special procedure code:", propertyInfo, messageError);

			previousDocument.CSI_Code = testSpecialProcedureCode;
			if (expectedIsMandatory)
			{
				AssertHasMessageError($"{description} - when LineNo=0 and CSI_Code is special procedure code:", propertyInfo, messageError);
			}
			else
			{
				AssertNoMessageError($"{description} - when LineNo=0 and CSI_Code is special procedure code:", propertyInfo, messageError);
			}

			previousDocument.CSI_LineNo = 1;
			AssertNoMessageError($"{description} - when CSI_Code is special procedure code but LineNo<>0:", propertyInfo, messageError);
		}
	}

	public void TestCheckCSI_Quantity2()
	{
		const int maximumPackages = 99999999;
		const string messageError = "The number of packages is too large. 99999999 is the maximum allowed.";

		var declaration = GetExportDeclaration();
		var invoice = declaration.Invoices.AddNew();
		var invoiceLine = invoice.InvoiceLines.AddNew();
		var previousDocument = invoiceLine.PreviousDocuments.AddNew();

		CombineAssertions("Export Previous Document CSI_DateOfIssue is not in use", () =>
		{
			previousDocument.CSI_Quantity2 = maximumPackages;
			AssertNoMessageError("The number of packages qualifies the condition", previousDocument.CSI_Quantity2Info, messageError);

			previousDocument.CSI_Quantity2 = maximumPackages + 1;
			AssertHasMessageError("The number of packages exceeds the condition", previousDocument.CSI_Quantity2Info, messageError);
		});
	}

	public void TestCheckRuleR0024E()
	{
		var declaration = GetExportDeclaration();
		var invoice = declaration.Invoices.AddNew();
		var invoiceLine = invoice.InvoiceLines.AddNew();

		var invalidCases = new ZString[] {
			"20221231-A", "20220404-A23", "20220404-12A", "20221204-1A3"
			, "20221200-1", "20220004-1", "202A0404-1", "00221204-1"
			, "2021024-1", "2021204-1"
			, "221204-1", "021204-1", "221204-1" , "21204-1" };

		var validCases = new ZString[] { "20221231-1", "20220404-123", "19990101-1" };
		TestCaseCheckRuleR0024E(invalidCases, validCases, declaration.PreviousDocuments.AddNew(), nameof(JobDeclaration));
		TestCaseCheckRuleR0024E(invalidCases, validCases, invoice.PreviousDocuments.AddNew(), nameof(JobComInvoiceHeader));
		TestCaseCheckRuleR0024E(invalidCases, validCases, invoiceLine.PreviousDocuments.AddNew(), nameof(JobComInvoiceLine));
	}

	public void TestCaseCheckRuleR0024E(ZString[] invalidCases, ZString[] validCases, PreviousDocument previousDocument, string objectName)
	{
		const string messageError = "(R0024E) Invalid Reference number format, expected YYYYMMDD-n";

		CombineAssertions($"BO object {objectName} Previous Document", () =>
		{
			AssertNoMessageError("Empty declaration"
				, previousDocument.CSI_ReferenceNumberInfo, messageError);

			previousDocument.CSI_Code = "asd";
			previousDocument.Validation.ValidateCSI_ReferenceNumber();
			AssertNoMessageError("Previous Document Code is not CLE"
				, previousDocument.CSI_ReferenceNumberInfo, messageError);

			previousDocument.CSI_Code = PreviousDocumentCodes.CLE;
			previousDocument.Validation.ValidateCSI_ReferenceNumber();
			AssertHasMessageError("Previous Document Code is CLE with empty ReferenceNumber"
				, previousDocument.CSI_ReferenceNumberInfo, messageError);

			foreach (var invalidCase in invalidCases)
			{
				previousDocument.CSI_Code = "asd";
				AssertNoExceptionThrown($"invalid case {invalidCase}"
					, () => previousDocument.CSI_ReferenceNumber = invalidCase);
				AssertNoMessageError($"Previous Document Code is not CLE with invalid case {previousDocument.CSI_ReferenceNumber}"
					, previousDocument.CSI_ReferenceNumberInfo, messageError);

				previousDocument.CSI_Code = PreviousDocumentCodes.CLE;
				previousDocument.Validation.ValidateCSI_ReferenceNumber();
				AssertHasMessageError($"Previous Document Code is CLE with invlid case {previousDocument.CSI_ReferenceNumber}"
					, previousDocument.CSI_ReferenceNumberInfo, messageError);
			}

			foreach (var validCase in validCases)
			{
				previousDocument.CSI_Code = "asd";
				AssertNoExceptionThrown($"valid case {validCase}"
					, () => previousDocument.CSI_ReferenceNumber = validCase);

				AssertNoMessageError($"Previous Document Code is not CLE with valid case {validCase}"
					, previousDocument.CSI_ReferenceNumberInfo, messageError);

				previousDocument.CSI_Code = PreviousDocumentCodes.CLE;
				previousDocument.Validation.ValidateCSI_ReferenceNumber();
				AssertNoMessageError($"Previous Document Code is CLE with valid case {validCase}"
					, previousDocument.CSI_ReferenceNumberInfo, messageError);
			}
		});
	}

	public void TestCheckRuleR0022E()
	{
		const string messageError = "(R0022E) Reference number must be 21 characters long";
		var declaration = GetExportDeclaration();
		var previousDocument = declaration.PreviousDocuments.AddNew();

		CombineAssertions(() =>
		{
			AssertNoMessageError("Empty declaration", previousDocument.CSI_ReferenceNumberInfo, messageError);

			previousDocument.CSI_Code = PreviousDocumentCodes.AAD;
			previousDocument.Validation.ValidateCSI_ReferenceNumber();
			AssertHasMessageError("CSI_Code AAD CSI_ReferenceNumber is empty", previousDocument.CSI_ReferenceNumberInfo, messageError);

			previousDocument.CSI_ReferenceNumber = "0123456789qwertyuiopas";
			AssertHasMessageError("CSI_ReferenceNumber > 21 characters", previousDocument.CSI_ReferenceNumberInfo, messageError);

			previousDocument.CSI_Code = "asd";
			previousDocument.Validation.ValidateCSI_ReferenceNumber();
			AssertNoMessageError("CSI_Code 'asd' CSI_ReferenceNumber > 21 characters", previousDocument.CSI_ReferenceNumberInfo, messageError);

			previousDocument.CSI_Code = PreviousDocumentCodes.AAD;
			previousDocument.CSI_ReferenceNumber = "0123456789qwertyuiop";
			AssertHasMessageError("CSI_ReferenceNumber < 21 characters", previousDocument.CSI_ReferenceNumberInfo, messageError);

			previousDocument.CSI_Code = "asd";
			previousDocument.Validation.ValidateCSI_ReferenceNumber();
			AssertNoMessageError("CSI_Code 'asd' CSI_ReferenceNumber < 21 characters", previousDocument.CSI_ReferenceNumberInfo, messageError);

			previousDocument.CSI_Code = PreviousDocumentCodes.AAD;
			previousDocument.CSI_ReferenceNumber = "0123456789qwertyuiopa";
			AssertNoMessageError("CSI_ReferenceNumber 21 characters", previousDocument.CSI_ReferenceNumberInfo, messageError);
		});
	}

	public void TestCheckRuleR0032E()
	{
		const string messageError = "(R0032E) A supporting document C710 or 4DK3 is required for each Entry Line";
		var declaration = GetExportDeclaration();
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		var previousDocument = declaration.PreviousDocuments.AddNew();
		var supportingDocument = declaration.SupportingDocuments.AddNew();
		var invoice = declaration.Invoices.AddNew();
		var invoiceLine = invoice.InvoiceLines.AddNew();

		CombineAssertions(() =>
		{
			previousDocument.Validation.ValidateCSI_ReferenceNumber();
			AssertNoMessageError("Empty declaration", previousDocument.CSI_ReferenceNumberInfo, messageError);

			previousDocument.CSI_Code = PreviousDocumentCodes.ZZZ;
			previousDocument.Validation.ValidateCSI_ReferenceNumber();
			AssertNoMessageError("Previous Document code is ZZZ", previousDocument.CSI_ReferenceNumberInfo, messageError);

			previousDocument.CSI_ReferenceNumber = "00DE00";
			AssertNoMessageError("CSI_ReferenceNumber 00DE00", previousDocument.CSI_ReferenceNumberInfo, messageError);

			entryInstruction.CEI_Procedure = ProcedureCodes._31;
			previousDocument.Validation.ValidateCSI_ReferenceNumber();
			AssertNoMessageError("Procedure code is 31", previousDocument.CSI_ReferenceNumberInfo, messageError);

			invoiceLine.PreviousProcedureCode = ProcedureCodes._51;
			previousDocument.Validation.ValidateCSI_ReferenceNumber();
			AssertHasMessageError("Previous Procedure code is 51", previousDocument.CSI_ReferenceNumberInfo, messageError);

			invoiceLine.PreviousProcedureCode = ProcedureCodes._54;
			previousDocument.Validation.ValidateCSI_ReferenceNumber();
			AssertHasMessageError("Previous Procedure code is 53", previousDocument.CSI_ReferenceNumberInfo, messageError);

			invoiceLine.PreviousProcedureCode = ProcedureCodes._21;
			previousDocument.Validation.ValidateCSI_ReferenceNumber();
			AssertNoMessageError("Previous Procedure code is 21", previousDocument.CSI_ReferenceNumberInfo, messageError);

			invoiceLine.PreviousProcedureCode = ProcedureCodes._51;
			previousDocument.CSI_ReferenceNumber = "00PL00";
			AssertNoMessageError("CSI_ReferenceNumber 00PL00", previousDocument.CSI_ReferenceNumberInfo, messageError);

			entryInstruction.CEI_Procedure = ProcedureCodes._40;
			previousDocument.CSI_ReferenceNumber = "00DE00";
			AssertNoMessageError("Procedure code is 40", previousDocument.CSI_ReferenceNumberInfo, messageError);

			entryInstruction.CEI_Procedure = ProcedureCodes._31;
			previousDocument.CSI_Code = "asd";
			previousDocument.Validation.ValidateCSI_ReferenceNumber();
			AssertNoMessageError("Previous Document code is not ZZZ", previousDocument.CSI_ReferenceNumberInfo, messageError);

			previousDocument.CSI_Code = PreviousDocumentCodes.ZZZ;
			supportingDocument.CSI_Code = SupportingDocumentCodes._4DK3;
			previousDocument.Validation.ValidateCSI_ReferenceNumber();
			AssertNoMessageError("Supporting Document code is 4DK3", previousDocument.CSI_ReferenceNumberInfo, messageError);

			supportingDocument.CSI_Code = SupportingDocumentCodes.C710;
			previousDocument.Validation.ValidateCSI_ReferenceNumber();
			AssertNoMessageError("Supporting Document code is C710", previousDocument.CSI_ReferenceNumberInfo, messageError);

			supportingDocument.CSI_Code = SupportingDocumentCodes.C513;
			previousDocument.Validation.ValidateCSI_ReferenceNumber();
			AssertHasMessageError("Supporting Document code is C513", previousDocument.CSI_ReferenceNumberInfo, messageError);
		});
	}

	JobDeclaration GetExportDeclaration()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
		return declaration;
	}
}
