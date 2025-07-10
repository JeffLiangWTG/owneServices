using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.PL.Business.Declaration.Testing;

class ImportCusEntryInstructionValidationTest : BusinessObjectValidationTestCase
{
	public void TestCheckRuleR986()
	{
		const string messageError = "(R986) One of C512, C513 Supporting document is required.";
		var (declaration, instruction) = GetImportInstruction();
		var subStyles = new[] { "C", "F", "Y" };
		foreach (var subStyle in subStyles)
		{
			instruction.CEI_SubStyle = subStyle;
			AssertHasMessageError(instruction.CEI_SubStyleInfo, messageError);
		}

		var invoice = declaration.Invoices.AddNew();
		var invoiceLine = invoice.InvoiceLines.AddNew();
		invoiceLine.JI_CEI = instruction.PK;
		foreach (var subStyle in subStyles)
		{
			instruction.CEI_SubStyle = subStyle;
			AssertHasMessageError(instruction.CEI_SubStyleInfo, messageError);
		}

		var suppDoc = invoice.SupportingDocuments.AddNew();
		var supportingDocumentCodeList = new[] { "C512", "C513" };
		foreach (var suppDocCode in supportingDocumentCodeList)
		{
			suppDoc.CSI_Code = suppDocCode;
			foreach (var subStyle in subStyles)
			{
				instruction.CEI_SubStyle = subStyle;
				AssertNoMessageError(instruction.CEI_SubStyleInfo, messageError);
			}
		}

		suppDoc.CSI_Code = ZString.Empty;
		instruction.Validation.ValidateCEI_SubStyle();
		AssertHasMessageError(instruction.CEI_SubStyleInfo, messageError);

		suppDoc = invoiceLine.SupportingDocuments.AddNew();
		foreach (var suppDocCode in supportingDocumentCodeList)
		{
			suppDoc.CSI_Code = suppDocCode;
			foreach (var subStyle in subStyles)
			{
				instruction.CEI_SubStyle = subStyle;
				AssertNoMessageError(instruction.CEI_SubStyleInfo, messageError);
			}
		}

		suppDoc.CSI_Code = ZString.Empty;
		instruction.Validation.ValidateCEI_SubStyle();
		AssertHasMessageError(instruction.CEI_SubStyleInfo, messageError);
	}

	public void TestCheckForRuleR267()
	{
		const string messageError = "(R267) – For the requested procedure code 42 or 63 the destination country code cannot be PL.";
		var (declaration, entryInstruction) = GetImportInstruction();
		CombineAssertions(() =>
		{
			declaration.JE_GoodsDestination = CountryCodes.Poland;
			entryInstruction.CEI_Procedure = "11";
			AssertNoMessageErrorContaining("Goods Destination is PL|Procedure code is not 42 or 63", entryInstruction.CEI_ProcedureInfo, messageError);

			entryInstruction.CEI_Procedure = Constants.ProcedureCodes._42;
			AssertHasMessageErrorContaining("Goods Destination is PL|Procedure code is 42", entryInstruction.CEI_ProcedureInfo, messageError);

			entryInstruction.CEI_Procedure = Constants.ProcedureCodes._63;
			AssertHasMessageErrorContaining("Goods Destination is PL|Procedure code is 63", entryInstruction.CEI_ProcedureInfo, messageError);

			declaration.JE_GoodsDestination = CountryCodes.Denmark;
			entryInstruction.Validation.ValidateCEI_Procedure();
			AssertNoMessageErrorContaining("Goods Destination is not PL", entryInstruction.CEI_ProcedureInfo, messageError);
		});
	}

	public void TestCheckJI_ValuationDateOverrideIsValidZDateTime()
	{
		var messageError = "All valuation dates for the Entry must be from the same month and year.";
		var (declaration, entryInstruction) = GetImportInstruction();
		var invoice = declaration.Invoices.AddNew();
		var invoiceLine1 = invoice.InvoiceLines.AddNew();
		invoiceLine1.JI_CEI = entryInstruction.PK;
		var invoiceLine2 = invoice.InvoiceLines.AddNew();
		invoiceLine2.JI_CEI = entryInstruction.PK;

		CombineAssertions(() =>
		{
			invoiceLine1.JI_ValuationDateOverride = new ZDateTime(2022, 03, 01);
			invoiceLine2.JI_ValuationDateOverride = new ZDateTime(2021, 01, 20);
			entryInstruction.Validation.ValidateAll();
			AssertHasRowMessageError("Invalid Date for one of Valuation Dates for Entry Instruction", entryInstruction, messageError);

			invoiceLine2.JI_ValuationDateOverride = new ZDateTime(2022, 03, 03);
			entryInstruction.Validation.ValidateAll();
			AssertNoRowMessageError("Valid Dates for all of Valuation Dates for Entry Instructions - ", entryInstruction, messageError);
		});
	}

	public void TestCheckRuleR625()
	{
		var messageError = "(R625) Additional Information code 00100 or Supporting Document/Authorization code C601 is required for requested procedure.";
		var (declaration, entryInstruction) = GetImportInstruction();
		var invoice = declaration.Invoices.AddNew();
		var invoiceLine1 = invoice.InvoiceLines.AddNew();
		invoiceLine1.JI_CEI = entryInstruction.PK;
		var additionalDocument1 = invoiceLine1.AdditionalInfos.AddNew();
		var supportingDocument1 = invoiceLine1.SupportingDocuments.AddNew();

		CombineAssertions(() =>
		{
			entryInstruction.Validation.ValidateAll();
			AssertNoRowMessageError("Defaults", entryInstruction, messageError);

			entryInstruction.CEI_Procedure = Constants.ProcedureCodes._51;
			entryInstruction.Validation.ValidateAll();
			AssertHasRowMessageError("Missing Supporting Document C601 and Additional Information 00100", entryInstruction, messageError);

			entryInstruction.CEI_Procedure = Constants.ProcedureCodes._53;
			entryInstruction.Validation.ValidateAll();
			AssertNoRowMessageError("CEI_Procedure is not 51", entryInstruction, messageError);

			entryInstruction.CEI_Procedure = Constants.ProcedureCodes._51;
			additionalDocument1.CSI_Code = Constants.AdditionalInfoCodes._00100;
			entryInstruction.Validation.ValidateAll();
			AssertNoRowMessageError("Additional Information Code is 00100", entryInstruction, messageError);

			additionalDocument1.CSI_Code = ZString.Empty;
			supportingDocument1.CSI_Code = Constants.SupportingDocumentCodes.C601;
			entryInstruction.Validation.ValidateAll();
			AssertNoRowMessageError("Supporting Document Code is C601", entryInstruction, messageError);

			var invoiceLine2 = invoice.InvoiceLines.AddNew();
			invoiceLine2.JI_CEI = entryInstruction.PK;
			entryInstruction.Validation.ValidateAll();
			AssertHasRowMessageError("Missing Supporting Document C601 and Additional Information 00100 for 2nd invoice Line", entryInstruction, messageError);

			var supportingDocument2 = declaration.SupportingDocuments.AddNew();
			supportingDocument2.CSI_Code = Constants.SupportingDocumentCodes.C601;
			entryInstruction.Validation.ValidateAll();
			AssertNoRowMessageError("Supporting Document Code C601 exists for both invoice lines on declaration level", entryInstruction, messageError);

			supportingDocument2.CSI_Code = ZString.Empty;
			var additionalDocument2 = invoice.AdditionalInfos.AddNew();
			additionalDocument2.CSI_Code = Constants.AdditionalInfoCodes._00100;
			entryInstruction.Validation.ValidateAll();
			AssertNoRowMessageError("Additional Information Code 00100 exists for both invoice lines", entryInstruction, messageError);
		});
	}

	public void TestCheckRuleR628()
	{
		var messageError = "(R628) Additional Information code 00100 or Supporting Document/Authorization code C516 is required for requested procedure.";
		var (declaration, entryInstruction) = GetImportInstruction();
		var invoice = declaration.Invoices.AddNew();
		var invoiceLine1 = invoice.InvoiceLines.AddNew();
		invoiceLine1.JI_CEI = entryInstruction.PK;
		var additionalDocument1 = invoiceLine1.AdditionalInfos.AddNew();
		var supportingDocument1 = invoiceLine1.SupportingDocuments.AddNew();

		CombineAssertions(() =>
		{
			entryInstruction.Validation.ValidateAll();
			AssertNoRowMessageError("Defaults", entryInstruction, messageError);

			entryInstruction.CEI_Procedure = Constants.ProcedureCodes._53;
			entryInstruction.Validation.ValidateAll();
			AssertHasRowMessageError("Missing Supporting Document C601 and Additional Information 00100", entryInstruction, messageError);

			entryInstruction.CEI_Procedure = Constants.ProcedureCodes._51;
			entryInstruction.Validation.ValidateAll();
			AssertNoRowMessageError("CEI_Procedure is not 53", entryInstruction, messageError);

			entryInstruction.CEI_Procedure = Constants.ProcedureCodes._53;
			additionalDocument1.CSI_Code = Constants.AdditionalInfoCodes._00100;
			entryInstruction.Validation.ValidateAll();
			AssertNoRowMessageError("Additional Information Code is 00100", entryInstruction, messageError);

			additionalDocument1.CSI_Code = ZString.Empty;
			supportingDocument1.CSI_Code = Constants.SupportingDocumentCodes.C516;
			entryInstruction.Validation.ValidateAll();
			AssertNoRowMessageError("Supporting Document Code is C516", entryInstruction, messageError);

			var invoiceLine2 = invoice.InvoiceLines.AddNew();
			invoiceLine2.JI_CEI = entryInstruction.PK;
			entryInstruction.Validation.ValidateAll();
			AssertHasRowMessageError("Missing Supporting Document C516 and Additional Information 00100 for 2nd invoice Line", entryInstruction, messageError);

			var supportingDocument2 = declaration.SupportingDocuments.AddNew();
			supportingDocument2.CSI_Code = Constants.SupportingDocumentCodes.C516;
			entryInstruction.Validation.ValidateAll();
			AssertNoRowMessageError("Supporting Document Code C516 exists for both invoice lines on declaration level", entryInstruction, messageError);

			supportingDocument2.CSI_Code = ZString.Empty;
			var additionalDocument2 = invoice.AdditionalInfos.AddNew();
			additionalDocument2.CSI_Code = Constants.AdditionalInfoCodes._00100;
			entryInstruction.Validation.ValidateAll();
			AssertNoRowMessageError("Additional Information Code 00100 exists for both invoice lines", entryInstruction, messageError);
		});
	}

	public void TestCheckRuleR631()
	{
		var messageError = "(R631) Additional Information code 00100 or Supporting Document/Authorization code C019 is required for requested procedure.";
		var (declaration, entryInstruction) = GetImportInstruction();
		var invoice = declaration.Invoices.AddNew();
		var invoiceLine1 = invoice.InvoiceLines.AddNew();
		invoiceLine1.JI_CEI = entryInstruction.PK;
		var additionalDocument1 = invoiceLine1.AdditionalInfos.AddNew();
		var supportingDocument1 = invoiceLine1.SupportingDocuments.AddNew();

		CombineAssertions(() =>
		{
			entryInstruction.Validation.ValidateAll();
			AssertNoRowMessageError("Defaults", entryInstruction, messageError);

			entryInstruction.CEI_Procedure = Constants.ProcedureCodes._48;
			entryInstruction.Validation.ValidateAll();
			AssertHasRowMessageError("Missing Supporting Document C601 and Additional Information 00100", entryInstruction, messageError);

			entryInstruction.CEI_Procedure = Constants.ProcedureCodes._51;
			entryInstruction.Validation.ValidateAll();
			AssertNoRowMessageError("CEI_Procedure is not 48", entryInstruction, messageError);

			entryInstruction.CEI_Procedure = Constants.ProcedureCodes._48;
			additionalDocument1.CSI_Code = Constants.AdditionalInfoCodes._00100;
			entryInstruction.Validation.ValidateAll();
			AssertNoRowMessageError("Additional Information Code is 00100", entryInstruction, messageError);

			additionalDocument1.CSI_Code = ZString.Empty;
			supportingDocument1.CSI_Code = Constants.SupportingDocumentCodes.C019;
			entryInstruction.Validation.ValidateAll();
			AssertNoRowMessageError("Supporting Document Code is C019", entryInstruction, messageError);

			var invoiceLine2 = invoice.InvoiceLines.AddNew();
			invoiceLine2.JI_CEI = entryInstruction.PK;
			entryInstruction.Validation.ValidateAll();
			AssertHasRowMessageError("Missing Supporting Document C019 and Additional Information 00100 for 2nd invoice Line", entryInstruction, messageError);

			var supportingDocument2 = declaration.SupportingDocuments.AddNew();
			supportingDocument2.CSI_Code = Constants.SupportingDocumentCodes.C019;
			entryInstruction.Validation.ValidateAll();
			AssertNoRowMessageError("Supporting Document Code C019 exists for both invoice lines on declaration level", entryInstruction, messageError);

			supportingDocument2.CSI_Code = ZString.Empty;
			var additionalDocument2 = invoice.AdditionalInfos.AddNew();
			additionalDocument2.CSI_Code = Constants.AdditionalInfoCodes._00100;
			entryInstruction.Validation.ValidateAll();
			AssertNoRowMessageError("Additional Information Code 00100 exists for both invoice lines", entryInstruction, messageError);
		});
	}

	public void TestCheckCUDExchangeRatesArePresent()
	{
		PopulateExchangeRateData();
		var (_, entryInstruction) = GetImportInstruction();

		CombineAssertions(() =>
		{
			entryInstruction.Validation.ValidateCEI_DateForDuty();
			AssertNoNotifications("Empty Date for Duty", entryInstruction.CEI_DateForDutyInfo);

			entryInstruction.CEI_DateForDuty = ZDateTime.Today;
			AssertNoNotifications("Date with a defined exchange rate", entryInstruction.CEI_DateForDutyInfo);

			entryInstruction.CEI_DateForDuty = ZDateTime.Today.AddDays(1);
			AssertHasMessageError("Date with no defined exchange rate", entryInstruction.CEI_DateForDutyInfo, $"The EUR (CUD type) exchange rate for {entryInstruction.CEI_DateForDuty.ToShortDateString()} is missing. No duty / taxes calculation can be done.");
		});
	}

	public void TestCheckRuleR1628()
	{
		var messageError = "[R1628] Authorization owner information is required.";
		var r1628SupportingDocuments = new string[] { Constants.SupportingDocumentCodes.C019, Constants.SupportingDocumentCodes.C504
			, Constants.SupportingDocumentCodes.C512, Constants.SupportingDocumentCodes.C513, Constants.SupportingDocumentCodes.C514
			, Constants.SupportingDocumentCodes.C515, Constants.SupportingDocumentCodes.C516, Constants.SupportingDocumentCodes.C601
			, Constants.SupportingDocumentCodes.C626, Constants.SupportingDocumentCodes.C627, Constants.SupportingDocumentCodes.N990 };

		var (declaration, entryInstruction) = GetImportInstruction();
		var supportingDocumentDeclaration = declaration.SupportingDocuments.AddNew();
		var invoice = declaration.Invoices.AddNew();
		var supportingDocumentInvoice = invoice.SupportingDocuments.AddNew();
		var invoiceLine = invoice.InvoiceLines.AddNew();
		var supportingDocumentInvoiceLine = invoiceLine.SupportingDocuments.AddNew();
		invoiceLine.JI_CEI = entryInstruction.PK;

		CombineAssertions(() =>
		{
			AssertR1628ForDocumentCode(string.Empty, false);

			foreach (var supportingDocument in r1628SupportingDocuments)
			{
				entryInstruction.CusAuthorizationUsages.AddNew();
				AssertR1628ForDocumentCode(supportingDocument, false);
				entryInstruction.CusAuthorizationUsages.Clear();
				AssertR1628ForDocumentCode(supportingDocument, true);
			}

			AssertR1628ForDocumentCode("A000", false);
		});

		void AssertR1628ForDocumentCode(string value, bool messageErrorShouldExist)
		{
			AssertR1628(value, true, false, false, messageErrorShouldExist);
			AssertR1628(value, false, true, false, messageErrorShouldExist);
			AssertR1628(value, false, false, true, messageErrorShouldExist);
		}

		void AssertR1628(string value, bool declarationIncluded, bool invoiceIncluded, bool invoiceLineIncluded, bool messageErrorShouldExist)
		{
			supportingDocumentDeclaration.CSI_Code = declarationIncluded ? value : string.Empty;
			supportingDocumentInvoice.CSI_Code = invoiceIncluded ? value : string.Empty;
			supportingDocumentInvoiceLine.CSI_Code = invoiceLineIncluded ? value : string.Empty;

			entryInstruction.Validation.ValidateAll();
			if (messageErrorShouldExist)
			{
				AssertHasRowMessageError($"{value} - [{declarationIncluded}] [{invoiceIncluded}] [{invoiceLineIncluded}]", entryInstruction, messageError);
			}
			else
			{
				AssertNoRowMessageError($"{value} - [{declarationIncluded}] [{invoiceIncluded}] [{invoiceLineIncluded}]", entryInstruction, messageError);
			}
		}
	}

	void PopulateExchangeRateData()
	{
		RefExchangeRate exchangeRate = RefExchangeRate.New(Factory);
		exchangeRate.RE_StartDate = ZDateTime.Today;
		exchangeRate.RE_ExpiryDate = ZDateTime.Today;
		exchangeRate.RE_ExRateType = Core.Constants.ExchangeRateTypes.Code.CustomsMeasureEURExRate;
		exchangeRate.RE_SellRate = 3.3333m;
		exchangeRate.RE_RX_NKExCurrency = CurrencyCodes.EuropeanUnion;
		exchangeRate.RE_GC = GlbCompany.CurrentCompany.PK;

		Factory.Save();
	}

	(JobDeclaration, CusEntryInstruction) GetImportInstruction()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		return (declaration, entryInstruction);
	}
}
