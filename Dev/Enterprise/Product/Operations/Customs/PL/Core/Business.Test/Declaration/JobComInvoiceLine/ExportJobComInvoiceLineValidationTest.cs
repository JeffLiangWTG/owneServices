using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.Testing;
using Enterprise.Customs.PL.Business.Testing;
using Enterprise.Customs.Universal.Testing;
using static Enterprise.Customs.PL.Business.Constants;

namespace Enterprise.Customs.PL.Business.Declaration.Testing;

sealed class ExportJobComInvoiceLineValidationTest : BusinessObjectValidationTestCase
{
	public void TestCheckProcedureCodeBase() => CombineAssertions(() =>
	{
		invoiceLine.ProcedureCodeBase = "10";
		AssertNoMessageErrorContaining("Non-empty Procedure Code", invoiceLine.ProcedureCodeBaseInfo, MandatoryValidation.YouHaveNotEntered);

		invoiceLine.ProcedureCodeBase = ZString.Empty;
		AssertHasMessageErrorContaining("Empty Procedure Code", invoiceLine.ProcedureCodeBaseInfo, MandatoryValidation.YouHaveNotEntered);
	});

	public void TestCheckPreviousProcedureCode() => CombineAssertions(() =>
	{
		invoiceLine.PreviousProcedureCode = "10";
		AssertNoMessageErrorContaining("Non-empty Previous Procedure Code", invoiceLine.PreviousProcedureCodeInfo, MandatoryValidation.YouHaveNotEntered);

		invoiceLine.PreviousProcedureCode = ZString.Empty;
		AssertHasMessageErrorContaining("Empty Previous Procedure Code", invoiceLine.PreviousProcedureCodeInfo, MandatoryValidation.YouHaveNotEntered);
	});

	public void TestCheckJI_CountryOfOrigin() => CombineAssertions(() =>
	{
		const string messageError = "You have not entered a Country/Region of Origin.";

		invoiceLine.JI_CountryOfOrigin = string.Empty;
		AssertNoMessageErrorContaining("Empty declaration", invoiceLine.JI_CountryOfOriginInfo, messageError);

		invoiceLine.JI_Procedure = "55";
		invoiceLine.Declaration.ZG_SpecificCircumstanceIndicator = SpecificCircumstanceIndicator.Codes.RoadModeOfTransport;
		invoiceLine.Validation.ValidateJI_CountryOfOrigin();
		AssertNoMessageError("ZG_SpecificCircumstanceIndicator is not AuthorizedEconomicOperators", invoiceLine.JI_CountryOfOriginInfo, messageError);

		invoiceLine.JI_Procedure = "76";
		invoiceLine.Validation.ValidateJI_CountryOfOrigin();
		AssertHasMessageErrorContaining("JI_Procedure starts with 76", invoiceLine.JI_CountryOfOriginInfo, messageError);

		invoiceLine.JI_Procedure = "55";
		invoiceLine.Declaration.ZG_SpecificCircumstanceIndicator = SpecificCircumstanceIndicator.Codes.AuthorizedEconomicOperators;
		invoiceLine.Validation.ValidateJI_CountryOfOrigin();
		AssertHasMessageErrorContaining("ZG_SpecificCircumstanceIndicator is AuthorizedEconomicOperators", invoiceLine.JI_CountryOfOriginInfo, messageError);

		invoiceLine.JI_CountryOfOrigin = "a";
		AssertNoMessageError("not empty country of Origin", invoiceLine.JI_CountryOfOriginInfo, messageError);
	});

	public void TestCheckRuleR0031E() => CombineAssertions(() =>
	{
		const string messageError = "(R0031E) Supporting Document C710 or 4DK3 is required";

		Factory.AddCodeToCusMap_EUNAU((CusAuthorizationUsageType.C601, null), (CusAuthorizationUsageType.C019, null));
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		invoiceLine.JI_CEI = entryInstruction.PK;
		var declarationSupportingDocument = declaration.SupportingDocuments.AddNew();
		var invoiceSupportingDocument = invoice.SupportingDocuments.AddNew();
		var invoiceLineSupportingDocument = invoiceLine.SupportingDocuments.AddNew();
		var cusAuthorizationUsage = entryInstruction.CusAuthorizationUsages.AddNew();

		AssertNoRowMessageError("Empty declaration", invoiceLine, messageError);

		cusAuthorizationUsage.AGC_Code = CusAuthorizationUsageType.C019;
		invoiceLine.Validation.ValidateAll();
		AssertNoRowMessageError("AGC_Code is C019", invoiceLine, messageError);
		cusAuthorizationUsage.AGC_Code = CusAuthorizationUsageType.C601;
		AssertNoRowMessageError("AGC_Code is C601", invoiceLine, messageError);
		entryInstruction.CEI_Procedure = ProcedureCodes._11;
		invoiceLine.Validation.ValidateAll();
		AssertHasRowMessageError("CEI_Procedure is 11 and AGC_Code is C601", invoiceLine, messageError);
		entryInstruction.CEI_Procedure = ProcedureCodes._21;
		invoiceLine.Validation.ValidateAll();
		AssertNoRowMessageError("CEI_Procedure is 21 and AGC_Code is C601", invoiceLine, messageError);

		cusAuthorizationUsage.AGC_Code = CusAuthorizationUsageType.C019;
		invoiceLine.Validation.ValidateAll();
		AssertHasRowMessageError("CEI_Procedure is 21 and AGC_Code is C019", invoiceLine, messageError);
		entryInstruction.CEI_Procedure = ProcedureCodes._11;
		invoiceLine.Validation.ValidateAll();
		AssertNoRowMessageError("CEI_Procedure is 11 and AGC_Code is C019", invoiceLine, messageError);

		declarationSupportingDocument.CSI_Code = SupportingDocumentCodes.C710;
		invoiceLine.Validation.ValidateAll();
		AssertNoRowMessageError("declaration SupportingDocument is C710", invoiceLine, messageError);

		declarationSupportingDocument.CSI_Code = ZString.Empty;
		invoiceSupportingDocument.CSI_Code = SupportingDocumentCodes.C710;
		invoiceLine.Validation.ValidateAll();
		AssertNoRowMessageError("invoice SupportingDocument is C710", invoiceLine, messageError);

		invoiceSupportingDocument.CSI_Code = ZString.Empty;
		invoiceLineSupportingDocument.CSI_Code = SupportingDocumentCodes.C710;
		invoiceLine.Validation.ValidateAll();
		AssertNoRowMessageError("invoiceLine SupportingDocument is C710", invoiceLine, messageError);
	});

	public void TestCheckRuleC0871() => CombineAssertions(() =>
	{
		const string messageError = "(C0871) A Country of Origin is required for a procedure details code starting with letter 'E'";
		var additionalProcedureCode = invoiceLine.AdditionalProcedureCodes.AddNew();

		invoiceLine.Validation.ValidateJI_CountryOfOrigin();
		AssertNoMessageError("Empty declaration", invoiceLine.JI_CountryOfOriginInfo, messageError);

		additionalProcedureCode.CY_Code = "A00";
		invoiceLine.Validation.ValidateJI_CountryOfOrigin();
		AssertNoMessageError("Concession code is A00", invoiceLine.JI_CountryOfOriginInfo, messageError);

		additionalProcedureCode.CY_Code = "A0E";
		invoiceLine.Validation.ValidateJI_CountryOfOrigin();
		AssertNoMessageError("Concession code does not start with E", invoiceLine.JI_CountryOfOriginInfo, messageError);

		additionalProcedureCode.CY_Code = "E00";
		invoiceLine.Validation.ValidateJI_CountryOfOrigin();
		AssertHasMessageError("Concession code starts with E", invoiceLine.JI_CountryOfOriginInfo, messageError);

		invoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.Poland;
		AssertNoMessageError("JI_CountryOfOrigin is not empty", invoiceLine.JI_CountryOfOriginInfo, messageError);
	});

	public void TestR0025E() => CombineAssertions(() =>
	{
		const string messageError = "(R0025E) For the requested Procedure Code, a Previous document with one of the codes 'MRN','CLE','SDE','OGL','ZZZ' is required.";

		Factory.AddCodeToCusMap_EUNAU((CusAuthorizationUsageType.N990, customsCode: null), (CusAuthorizationUsageType.D019, customsCode: null));
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		invoiceLine.JI_CEI = entryInstruction.PK;
		var authorisationUsage = entryInstruction.CusAuthorizationUsages.AddNew();
		var declarationPreviousDocument = declaration.PreviousDocuments.AddNew();
		var invoicePreviousDocument = invoice.PreviousDocuments.AddNew();
		var invoiceLinePreviousDocument = invoiceLine.PreviousDocuments.AddNew();

		R0025ECheck(false, invoiceLine, ProcedureCodes._10, ProcedureCodes._10, declarationPreviousDocument, ZString.Empty, ZString.Empty);
		R0025ECheck(false, invoiceLine, ProcedureCodes._10, ProcedureCodes._10, invoicePreviousDocument, ZString.Empty, ZString.Empty);
		R0025ECheck(false, invoiceLine, ProcedureCodes._10, ProcedureCodes._10, invoiceLinePreviousDocument, ZString.Empty, ZString.Empty);
		R0025ECheck(false, invoiceLine, ProcedureCodes._10, ProcedureCodes._10, invoiceLinePreviousDocument, "ABC", ZString.Empty);
		R0025ECheck(false, invoiceLine, ProcedureCodes._10, ProcedureCodes._10, invoiceLinePreviousDocument, "ASD", "QWE");
		R0025ECheck(false, invoiceLine, ProcedureCodes._10, ProcedureCodes._10, invoiceLinePreviousDocument, ZString.Empty, "ZXC");

		R0025ECheck(false, invoiceLine, ProcedureCodes._10, ProcedureCodes._44, declarationPreviousDocument, PreviousDocumentCodes.ZZZ, "ASD");
		R0025ECheck(false, invoiceLine, ProcedureCodes._10, ProcedureCodes._44, invoicePreviousDocument, PreviousDocumentCodes.ZZZ, "ASD");
		R0025ECheck(false, invoiceLine, ProcedureCodes._10, ProcedureCodes._44, invoiceLinePreviousDocument, PreviousDocumentCodes.ZZZ, "ASD");
		R0025ECheck(false, invoiceLine, ProcedureCodes._10, ProcedureCodes._44, invoiceLinePreviousDocument, PreviousDocumentCodes.CLE, "ASD");
		R0025ECheck(false, invoiceLine, ProcedureCodes._10, ProcedureCodes._44, invoiceLinePreviousDocument, PreviousDocumentCodes.SDE, "ASD");
		R0025ECheck(false, invoiceLine, ProcedureCodes._10, ProcedureCodes._44, invoiceLinePreviousDocument, PreviousDocumentCodes.OGL, "ASD");
		R0025ECheck(false, invoiceLine, ProcedureCodes._10, ProcedureCodes._44, invoiceLinePreviousDocument, PreviousDocumentCodes.MRN, "ASD");
		R0025ECheck(false, invoiceLine, ProcedureCodes._10, ProcedureCodes._44, invoiceLinePreviousDocument, PreviousDocumentCodes.ZZZ, ZString.Empty);

		R0025ECheck(true, invoiceLine, ProcedureCodes._10, ProcedureCodes._44, declarationPreviousDocument, ZString.Empty, "ZXC");
		R0025ECheck(true, invoiceLine, ProcedureCodes._10, ProcedureCodes._44, invoicePreviousDocument, ZString.Empty, "ZXC");
		R0025ECheck(true, invoiceLine, ProcedureCodes._10, ProcedureCodes._44, invoiceLinePreviousDocument, ZString.Empty, "ZXC");
		R0025ECheck(true, invoiceLine, ProcedureCodes._10, ProcedureCodes._44, invoiceLinePreviousDocument, ZString.Empty, ZString.Empty);
		R0025ECheck(true, invoiceLine, ProcedureCodes._10, ProcedureCodes._44, invoiceLinePreviousDocument, "VCC", ZString.Empty);
		R0025ECheck(true, invoiceLine, ProcedureCodes._10, ProcedureCodes._41, invoiceLinePreviousDocument, ZString.Empty, ZString.Empty);
		R0025ECheck(true, invoiceLine, ProcedureCodes._21, ProcedureCodes._41, invoiceLinePreviousDocument, ZString.Empty, ZString.Empty);
		R0025ECheck(true, invoiceLine, ProcedureCodes._21, ProcedureCodes._51, invoiceLinePreviousDocument, ZString.Empty, ZString.Empty);
		R0025ECheck(true, invoiceLine, ProcedureCodes._21, ProcedureCodes._48, invoiceLinePreviousDocument, ZString.Empty, ZString.Empty);
		R0025ECheck(true, invoiceLine, ProcedureCodes._21, ProcedureCodes._46, invoiceLinePreviousDocument, ZString.Empty, ZString.Empty);
		R0025ECheck(true, invoiceLine, ProcedureCodes._21, ProcedureCodes._54, invoiceLinePreviousDocument, ZString.Empty, ZString.Empty);
		R0025ECheck(true, invoiceLine, ProcedureCodes._21, ProcedureCodes._91, invoiceLinePreviousDocument, ZString.Empty, ZString.Empty);
		R0025ECheck(true, invoiceLine, ProcedureCodes._31, ProcedureCodes._78, invoiceLinePreviousDocument, ZString.Empty, ZString.Empty);
		R0025ECheck(true, invoiceLine, ProcedureCodes._31, ProcedureCodes._51, invoiceLinePreviousDocument, ZString.Empty, ZString.Empty);
		R0025ECheck(true, invoiceLine, ProcedureCodes._31, ProcedureCodes._54, invoiceLinePreviousDocument, ZString.Empty, ZString.Empty);
		R0025ECheck(true, invoiceLine, ProcedureCodes._31, ProcedureCodes._53, invoiceLinePreviousDocument, ZString.Empty, ZString.Empty);
		R0025ECheck(true, invoiceLine, ProcedureCodes._31, ProcedureCodes._53, invoiceLinePreviousDocument, ZString.Empty, ZString.Empty);
		R0025ECheck(true, invoiceLine, ProcedureCodes._31, ProcedureCodes._71, invoiceLinePreviousDocument, ZString.Empty, ZString.Empty);
		R0025ECheck(true, invoiceLine, ProcedureCodes._31, ProcedureCodes._91, invoiceLinePreviousDocument, ZString.Empty, ZString.Empty);

		R0025ECheck(false, invoiceLine, ProcedureCodes._10, ProcedureCodes._40, declarationPreviousDocument, PreviousDocumentCodes.ZZZ, CusAuthorizationUsageType.N990);
		R0025ECheck(false, invoiceLine, ProcedureCodes._10, ProcedureCodes._40, invoicePreviousDocument, PreviousDocumentCodes.ZZZ, CusAuthorizationUsageType.N990);
		R0025ECheck(false, invoiceLine, ProcedureCodes._10, ProcedureCodes._40, invoiceLinePreviousDocument, PreviousDocumentCodes.ZZZ, CusAuthorizationUsageType.N990);

		R0025ECheck(true, invoiceLine, ProcedureCodes._10, ProcedureCodes._40, invoiceLinePreviousDocument, "ASD", CusAuthorizationUsageType.N990);
		R0025ECheck(true, invoiceLine, ProcedureCodes._10, ProcedureCodes._40, invoiceLinePreviousDocument, ZString.Empty, CusAuthorizationUsageType.N990);
		R0025ECheck(true, invoiceLine, ProcedureCodes._10, ProcedureCodes._40, invoiceLinePreviousDocument, ZString.Empty, CusAuthorizationUsageType.D019);
		R0025ECheck(true, invoiceLine, ProcedureCodes._21, ProcedureCodes._40, invoiceLinePreviousDocument, ZString.Empty, CusAuthorizationUsageType.N990);
		R0025ECheck(true, invoiceLine, ProcedureCodes._21, ProcedureCodes._40, invoiceLinePreviousDocument, ZString.Empty, CusAuthorizationUsageType.D019);
		R0025ECheck(true, invoiceLine, ProcedureCodes._21, ProcedureCodes._44, invoiceLinePreviousDocument, ZString.Empty, CusAuthorizationUsageType.N990);
		R0025ECheck(true, invoiceLine, ProcedureCodes._21, ProcedureCodes._44, invoiceLinePreviousDocument, ZString.Empty, CusAuthorizationUsageType.D019);

		R0025ECheck(false, invoiceLine, ProcedureCodes._10, ProcedureCodes._40, invoiceLinePreviousDocument, ZString.Empty, "ASD");
		R0025ECheck(false, invoiceLine, ProcedureCodes._21, ProcedureCodes._40, invoiceLinePreviousDocument, ZString.Empty, "ASD");
		R0025ECheck(false, invoiceLine, ProcedureCodes._21, ProcedureCodes._44, invoiceLinePreviousDocument, ZString.Empty, "ASD");
		return;

		void R0025ECheck(bool shouldHaveMessageError, JobComInvoiceLine invoiceLine, ZString procedureCode, ZString previousProcedureCode
			, PreviousDocument previousDocument, ZString previousDocumentCode
			, ZString authorisationUsageCode)
		{
			invoiceLinePreviousDocument.CSI_Code = ZString.Empty;
			declarationPreviousDocument.CSI_Code = ZString.Empty;
			invoicePreviousDocument.CSI_Code = ZString.Empty;

			invoiceLine.ProcedureCodeBase = procedureCode;
			invoiceLine.PreviousProcedureCode = previousProcedureCode;
			authorisationUsage.AGC_Code = authorisationUsageCode;
			previousDocument.CSI_Code = previousDocumentCode;

			if (shouldHaveMessageError)
			{
				invoiceLine.PLValidationOrNull?.ValidateProcedureCodeBase();
				AssertHasMessageErrorContaining($"Procedure:[{procedureCode}{previousProcedureCode}] PreviousDocumentCode:[{previousDocument.CSI_Code}] AuthorisationUsageCode:[{authorisationUsage.AGC_Code}] should have message error."
												, invoiceLine.ProcedureCodeBaseInfo, messageError);

				previousDocument.CSI_Code = PreviousDocumentCodes.ZZZ;
			}

			invoiceLine.PLValidationOrNull?.ValidateProcedureCodeBase();
			AssertNoMessageErrorContaining($"Procedure:[{procedureCode}{previousProcedureCode}] PreviousDocumentCode:[{previousDocument.CSI_Code}] AuthorisationUsageCode:[{authorisationUsage.AGC_Code}] should not have message error."
											, invoiceLine.ProcedureCodeBaseInfo, messageError);
		}
	});

	public void TestCheckRuleR0219() => CombineAssertions(() =>
	{
		const string messageError = "(R0219) You have not selected any packing information.";

		using var testContext = new FunctionalityTestContext();
		testContext.SetAESTransitionPeriod(true);
		invoiceLine.Validation.ValidateAll();
		AssertNoRowMessageError("R0219 should be disabled when in transition period", invoiceLine, messageError);

		testContext.SetAESTransitionPeriod(false);
		invoiceLine.Validation.ValidateAll();
		AssertHasRowMessageError("Empty line", invoiceLine, messageError);

		var package = declaration.Packages.AddNew();
		var pivotPackage = invoiceLine.PackagesForInvoiceLinesForBindingOnly.AddNew();
		pivotPackage.PackagePk = package.PK;
		invoiceLine.Validation.ValidateAll();
		AssertHasRowMessageError("Package is not linked", invoiceLine, messageError);

		pivotPackage.IsLinked = true;
		invoiceLine.Validation.ValidateAll();
		AssertNoRowMessageError("At least 1 Package is linked", invoiceLine, messageError);
	});

	protected override void SetUp()
	{
		declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
		invoice = declaration.Invoices.AddNew();
		invoiceLine = invoice.InvoiceLines.AddNew();
	}

	JobDeclaration declaration;
	JobComInvoiceHeader invoice;
	JobComInvoiceLine invoiceLine;
}
