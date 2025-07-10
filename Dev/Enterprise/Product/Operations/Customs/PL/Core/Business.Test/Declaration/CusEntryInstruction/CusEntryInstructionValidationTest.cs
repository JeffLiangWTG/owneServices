using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common.PL;
using Enterprise.Customs.EU.Business.Testing;
using Enterprise.Customs.PL.Business.Testing;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Moq;
using Moq.Protected;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.PL.Business.Declaration.Testing;

sealed class CusEntryInstructionValidationTest : BusinessObjectValidationTestCase
{
	public void TestValidateNoDuplicateInstructions()
	{
		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		var testInstruction1 = GetEntryInstruction();
		var testInstruction2 = GetEntryInstruction();

		testInstruction1.CEI_SubStyle = "A";
		testInstruction2.CEI_SubStyle = "A";
		testInstruction1.CEI_Description = "ABC";
		testInstruction2.CEI_Description = "ABC";
		testInstruction1.CEI_Procedure = "40";
		testInstruction2.CEI_Procedure = "40";
		var duplicateEntryMessage = "This Entry Instruction already exists in the instructions list";
		testInstruction2.Validation.ValidateAll();
		AssertHasRowWarningContaining(testInstruction2, duplicateEntryMessage);
		testInstruction2.CEI_SubStyle = "B";
		testInstruction2.Validation.ValidateAll();
		AssertNoRowWarningContaining(testInstruction2, duplicateEntryMessage);
	}

	public void TestCheckMaximumNumberOfPackagesWithSignlePackage()
	{
		const int maximumPackages = 99999999;
		const string message = "The number of packages is too large. 99999999 is the maximum allowed for an entry.";

		var package = declaration.Packages.AddNew();
		var invoice = declaration.Invoices.AddNew();
		var invoiceLine = invoice.InvoiceLines.AddNew();
		var invoiceLinePackage = invoiceLine.PackagesForInvoiceLinesForBindingOnly.AddNew();
		var instruction = GetEntryInstruction();
		invoiceLine.JI_CEI = instruction.PK;

		CombineAssertions(() =>
		{
			invoiceLinePackage.Package = package;
			invoiceLinePackage.IsLinked = true;
			invoiceLinePackage.PackQty = maximumPackages;
			instruction.Validation.ValidateAll();
			AssertNoRowMessageError("The number of package qualifies the condition", instruction, message);

			invoiceLinePackage.PackQty = maximumPackages + 1;
			instruction.Validation.ValidateAll();
			AssertHasRowMessageError("The number of package exceeds the maximum restriction", instruction, message);
		});
	}

	public void TestValidateMaximumNumberOfPackagesWithMultiplePackages()
	{
		const int maximumPackages = 99999999;
		const string message = "The number of packages is too large. 99999999 is the maximum allowed for an entry.";

		var package1 = declaration.Packages.AddNew();
		var package2 = declaration.Packages.AddNew();
		var invoice = declaration.Invoices.AddNew();
		var invoiceLine1 = invoice.InvoiceLines.AddNew();
		var invoiceLine2 = invoice.InvoiceLines.AddNew();
		var invoiceLinePackage1 = invoiceLine1.PackagesForInvoiceLinesForBindingOnly.AddNew();
		var invoiceLinePackage2 = invoiceLine2.PackagesForInvoiceLinesForBindingOnly.AddNew();
		var instruction = GetEntryInstruction();
		invoiceLine1.JI_CEI = instruction.PK;
		invoiceLine2.JI_CEI = instruction.PK;

		CombineAssertions(() =>
		{
			invoiceLinePackage1.Package = package1;
			invoiceLinePackage1.IsLinked = true;
			invoiceLinePackage1.PackQty = maximumPackages;
			instruction.Validation.ValidateAll();
			AssertNoRowMessageError("The number of packages qualifies the condition", instruction, message);

			invoiceLinePackage2.Package = package2;
			invoiceLinePackage2.IsLinked = true;
			invoiceLinePackage2.PackQty = maximumPackages;
			instruction.Validation.ValidateAll();
			AssertHasRowMessageError("The number of packages exceeds the maximum restriction", instruction, message);
		});
	}

	public void TestCheckCEI_SubStyle() => CombineAssertions(() =>
	{
		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

		var testInstruction = GetEntryInstruction();
		testInstruction.Validation.ValidateCEI_SubStyle();
		AssertHasMessageErrorContaining("CEI_SubStyle is mandatory.", testInstruction.CEI_SubStyleInfo, MandatoryValidation.YouHaveNotEntered);

		testInstruction.CEI_SubStyle = "1";
		testInstruction.Validation.ValidateCEI_SubStyle();
		AssertHasMessageError("CEI_SubStyle has unexpected value.", testInstruction.CEI_SubStyleInfo, ListValidation.InvalidCodeMessageError);

		testInstruction.CEI_SubStyle = "Z";
		testInstruction.Validation.ValidateCEI_SubStyle();
		AssertNoNotifications("CEI_SubStyle has valid value.", testInstruction.CEI_SubStyleInfo);
	});

	public void TestCheckCEI_SubStyle_ExitSummary() => CombineAssertions(() =>
	{
		var testInstruction = GetEntryInstruction();

		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		testInstruction.CEI_SubStyle = "1";
		AssertHasMessageError("List Validation is enabled for non-EXS.", testInstruction.CEI_SubStyleInfo, ListValidation.InvalidCodeMessageError);

		declaration.JE_MessageType = PLJobMessageTypeList.Codes.ExitSummary;
		testInstruction.Validation.ValidateCEI_SubStyle();
		AssertNoMessageError("List Validation is disabled for EXS.", testInstruction.CEI_SubStyleInfo, ListValidation.InvalidCodeMessageError);

		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		testInstruction.CEI_SubStyle = ZString.Empty;
		AssertHasMessageErrorContaining("CEI_SubStyle is mandatory for non-EXS.", testInstruction.CEI_SubStyleInfo, MandatoryValidation.YouHaveNotEntered);

		declaration.JE_MessageType = PLJobMessageTypeList.Codes.ExitSummary;
		testInstruction.Validation.ValidateCEI_SubStyle();
		AssertNoMessageError("CEI_SubStyle is not mandatory for EXS.", testInstruction.CEI_SubStyleInfo, MandatoryValidation.YouHaveNotEntered);
	});

	public void TestCheckCEI_Procedure_ExitSummary() => CombineAssertions(() =>
	{
		var testInstruction = GetEntryInstruction();

		declaration.JE_MessageType = PLJobMessageTypeList.Codes.Import;
		testInstruction.CEI_Procedure = "1";
		AssertHasMessageError("List Validation is enabled for non-EXS.", testInstruction.CEI_ProcedureInfo, ListValidation.InvalidCodeMessageError);

		declaration.JE_MessageType = PLJobMessageTypeList.Codes.ExitSummary;
		testInstruction.Validation.ValidateCEI_Procedure();
		AssertNoMessageError("List Validation is disabled for EXS.", testInstruction.CEI_ProcedureInfo, ListValidation.InvalidCodeMessageError);

		declaration.JE_MessageType = PLJobMessageTypeList.Codes.Import;
		testInstruction.CEI_Procedure = ZString.Empty;
		AssertHasMessageErrorContaining("CEI_Procedure is mandatory for non-EXS.", testInstruction.CEI_ProcedureInfo, MandatoryValidation.YouHaveNotEntered);

		declaration.JE_MessageType = PLJobMessageTypeList.Codes.ExitSummary;
		testInstruction.Validation.ValidateCEI_Procedure();
		AssertNoMessageError("CEI_Procedure is not mandatory for EXS.", testInstruction.CEI_ProcedureInfo, MandatoryValidation.YouHaveNotEntered);
	});

	public void TestCheckCEI_DateForDuty_NotEntered()
	{
		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		var messageError = "You have not entered a Declaration Date.";

		var entryInstruction = GetEntryInstruction();

		CombineAssertions(() =>
		{
			entryInstruction.CEI_DateForDuty = ZDateTime.Empty;
			AssertHasMessageErrorContaining("Date for duty is empty", entryInstruction.CEI_DateForDutyInfo, messageError);

			entryInstruction.CEI_DateForDuty = ZDateTime.Today;
			AssertNoMessageErrorContaining("Date for duty is not empty", entryInstruction.CEI_DateForDutyInfo, messageError);
		});
	}

	public void TestCheckCEI_DateForDuty_DateIsOlder()
	{
		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		var warningMessage = "Date is older than current date.";

		var entryInstruction = GetEntryInstruction();

		CombineAssertions(() =>
		{
			entryInstruction.CEI_DateForDuty = ZDateTime.Today.AddDays(-2);
			AssertHasWarningContaining("Date in the past", entryInstruction.CEI_DateForDutyInfo, warningMessage);

			entryInstruction.CEI_DateForDuty = ZDateTime.Today.AddDays(2);
			AssertNoWarningContaining("Date in the future", entryInstruction.CEI_DateForDutyInfo, warningMessage);

			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_CEI_Instruction = entryInstruction.PK;
			AssertNotNull(entryInstruction.EntryHeader);

			entryHeader.EntryNumber = "Test";
			entryInstruction.CEI_DateForDuty = ZDateTime.Today.AddDays(-2);
			AssertNoWarningContaining("EntryNumber is not empty", entryInstruction.CEI_DateForDutyInfo, warningMessage);

			entryHeader.EntryNumber = ZString.Empty;
			entryInstruction.Validation.ValidateCEI_DateForDuty();
			AssertHasWarningContaining("EntryNumber is empty", entryInstruction.CEI_DateForDutyInfo, warningMessage);
		});
	}

	public void TestCheckCEI_Procedure()
	{
		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		var entryInstruction = GetEntryInstruction();

		var helper = new UniversalReferenceTestDataHelper(Factory);
		helper.CreateOrFindExistingRefCusProcedure(CountryCodes.Poland, "", "11", "", "", "PLI1", "IMP", "");
		helper.CreateOrFindExistingRefCusProcedure(CountryCodes.Poland, "", "21", "", "", "PLE1", "EXP", "");
		Factory.Save();

		entryInstruction.CEI_Procedure = ZString.Empty;
		ValidationTestHelper.AssertYouHaveNotEnteredMessageError(entryInstruction.CEI_ProcedureInfo);

		declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
		ValidationTestHelper.AssertYouHaveNotEnteredMessageError(entryInstruction.CEI_ProcedureInfo);

		ValidationTestHelper.AssertInvalidCodeMessageError(entryInstruction.CEI_ProcedureInfo, "33", "21");

		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		ValidationTestHelper.AssertInvalidCodeMessageError(entryInstruction.CEI_ProcedureInfo, "33", "11");
	}

	public void TestCheckCEI_OA_Warehouse_EmailLength()
	{
		const string messageError = "The organization email address is longer than 35 characters.";
		var org = Factory.NewWithValidTestData<OrgHeader>();
		var entryInstruction = GetEntryInstruction();

		CombineAssertions(() =>
		{
			var orgAddress = org.Addresses.AddNew();
			orgAddress.OA_Email = "May the force be with you@Star Wars.New Hope";
			entryInstruction.CEI_OA_Warehouse = orgAddress.PK;
			AssertHasMessageError("It should show a message error if email address length is longer than 35 characters:", entryInstruction.CEI_OA_WarehouseInfo, messageError);

			orgAddress.OA_Email = "May the force @be wi.th you";
			entryInstruction.CEI_OA_Warehouse = orgAddress.PK;
			AssertNoMessageError("No message error when the email address length is less or equal than 35 characters:", entryInstruction.CEI_OA_WarehouseInfo, messageError);
		});
	}

	public void TestValidateFiscalReferences_CanBeEntered()
	{
		var invoiceLineConfigurationMock = new Mock<InstructionConfiguration>();
		invoiceLineConfigurationMock.CallBase = true;
		invoiceLineConfigurationMock.Protected()
			.Setup<ZBool>("FiscalReferencesSupportCore", ItExpr.IsAny<BusinessObject>())
			.Returns(true);

		using (ConfigurationTestHelper.TemporarilySetConfiguration_Declaration(Factory, "GetNewInstructionConfiguration", invoiceLineConfigurationMock.Object, null))
		{
			var declaration = Factory.New<JobDeclaration>();
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_CEI = instruction.PK;
			invoiceLine.JI_Procedure = "40";
			instruction.FiscalReferences.AddNew();
			instruction.Validation.ValidateFiscalReferences();
			AssertEquals("There is no notification when enter a Fiscal Representation and no invoice with CPC starts with 42 or 63", 0, instruction.Notifications.Count());
		}
	}

	public void TestCheckRuleR266()
	{
		const string messageError = "(R266) For the requested procedure code 42 and 63, supporting document \'Y044\' is required.";
		var instruction = declaration.CustomsEntryInstructions.AddNew();
		var invoice = declaration.Invoices.AddNew();
		var invoiceLine = invoice.InvoiceLines.AddNew();
		invoiceLine.JI_CEI = instruction.PK;
		var supportingDocumentDeclaration = declaration.SupportingDocuments.AddNew();
		var supportingDocumentInvoice = invoice.SupportingDocuments.AddNew();
		supportingDocumentInvoice.CSI_Code = Constants.SupportingDocumentCodes.C019;
		var supportingDocumentInvoiceLine = invoiceLine.SupportingDocuments.AddNew();
		supportingDocumentInvoiceLine.CSI_Code = Constants.SupportingDocumentCodes.C019;
		instruction.CEI_Procedure = "40";

		CombineAssertions(() =>
		{
			AssertNoMessageError("No message error when procedure is not 42 or 63 and not exist supporting document for code Y044", instruction.CEI_ProcedureInfo, messageError);
			instruction.CEI_Procedure = "42";
			AssertHasMessageError("It should show a message error when procedure is 42 and not exist supporting document for code Y044", instruction.CEI_ProcedureInfo, messageError);
			instruction.CEI_Procedure = "63";
			AssertHasMessageError("It should show a message error when procedure is 63 and not exist supporting document for code Y044", instruction.CEI_ProcedureInfo, messageError);
			supportingDocumentInvoice.CSI_Code = Constants.SupportingDocumentCodes.Y044;
			instruction.Validation.ValidateCEI_Procedure();
			AssertNoMessageError("No message error when procedure is 42 or 63 and invoice header supporting document code is Y044", instruction.CEI_ProcedureInfo, messageError);
			supportingDocumentInvoice.CSI_Code = Constants.SupportingDocumentCodes.C019;
			supportingDocumentDeclaration.CSI_Code = Constants.SupportingDocumentCodes.Y044;
			instruction.Validation.ValidateCEI_Procedure();
			AssertNoMessageError("No message error when procedure is 42 or 63 and declaration supporting document code is Y044", instruction.CEI_ProcedureInfo, messageError);
			supportingDocumentDeclaration.CSI_Code = Constants.SupportingDocumentCodes.C019;
			supportingDocumentInvoiceLine.CSI_Code = Constants.SupportingDocumentCodes.Y044;
			instruction.Validation.ValidateCEI_Procedure();
			AssertNoMessageError("No message error when procedure is 42 or 63 and invoice line supporting document code is Y044", instruction.CEI_ProcedureInfo, messageError);
			var invoiceLine2 = invoice.InvoiceLines.AddNew();
			var supportingDocumentInvoiceLine2 = invoiceLine2.SupportingDocuments.AddNew();
			supportingDocumentInvoiceLine2.CSI_Code = Constants.SupportingDocumentCodes.C019;
			instruction.Validation.ValidateCEI_Procedure();
			AssertHasMessageError("It should show a message error when procedure is 63 and any invoice line not exist supporting document for code Y044", instruction.CEI_ProcedureInfo, messageError);

			supportingDocumentInvoiceLine2 = invoiceLine2.SupportingDocuments.AddNew();
			supportingDocumentInvoiceLine2.CSI_Code = Constants.SupportingDocumentCodes.C019;
			instruction.Validation.ValidateCEI_Procedure();
			AssertHasMessageError("It should show a message error when procedure is 63 and any invoice line not exist supporting document for code Y044 when more than one supporting document", instruction.CEI_ProcedureInfo, messageError);
			supportingDocumentInvoiceLine2.CSI_Code = Constants.SupportingDocumentCodes.Y044;
			instruction.Validation.ValidateCEI_Procedure();
			AssertNoMessageError("No message error when procedure is 42 or 63 and invoice line supporting document code has Y044 when more than one supporting document", instruction.CEI_ProcedureInfo, messageError);

			supportingDocumentInvoiceLine2.CSI_Code = Constants.SupportingDocumentCodes.C019;
			var supportingDocumentInvoice2 = invoice.SupportingDocuments.AddNew();
			supportingDocumentInvoice2.CSI_Code = Constants.SupportingDocumentCodes.C019;
			instruction.Validation.ValidateCEI_Procedure();
			AssertHasMessageError("It should show a message error when procedure is 63 and any invoice not exist supporting document for code Y044 when more than one supporting document", instruction.CEI_ProcedureInfo, messageError);
			supportingDocumentInvoice2.CSI_Code = Constants.SupportingDocumentCodes.Y044;
			instruction.Validation.ValidateCEI_Procedure();
			AssertNoMessageError("No message error when procedure is 42 or 63 and invoice supporting document code has Y044 when more than one supporting document", instruction.CEI_ProcedureInfo, messageError);

			supportingDocumentInvoice2.CSI_Code = Constants.SupportingDocumentCodes.C019;
			var supportingDocumentdeclaration2 = declaration.SupportingDocuments.AddNew();
			supportingDocumentdeclaration2.CSI_Code = Constants.SupportingDocumentCodes.C019;
			instruction.Validation.ValidateCEI_Procedure();
			AssertHasMessageError("It should show a message error when procedure is 63 and any declaration not exist supporting document for code Y044 when more than one supporting document", instruction.CEI_ProcedureInfo, messageError);
			supportingDocumentdeclaration2.CSI_Code = Constants.SupportingDocumentCodes.Y044;
			instruction.Validation.ValidateCEI_Procedure();
			AssertNoMessageError("No message error when procedure is 42 or 63 and declaration supporting document code has Y044 when more than one supporting document", instruction.CEI_ProcedureInfo, messageError);
		});
	}

	public void TestCheckCEI_OA_Warehouse()
	{
		var org = Factory.NewWithValidTestData<OrgHeader>();
		var orgAddress = org.Addresses.AddNew();
		var orgCusCode = orgAddress.CustomsCodes.AddNew();
		var entryInstruction = GetEntryInstruction();
		entryInstruction.CEI_OA_Warehouse = orgAddress.PK;

		CombineAssertions(() =>
		{
			orgCusCode.OK_RN_NKCodeCountry = CountryCodes.Poland;
			orgCusCode.OK_CodeType = OrgCusCode.CodeTypes.TerminalControlledPremisesID;
			orgCusCode.OK_CustomsRegNo = "123456789";
			AssertHasMessageError(entryInstruction.CEI_OA_WarehouseInfo, "A warehouse authorization number(country PL type CPW) is missing for selected address");

			orgCusCode.OK_CodeType = OrgCusCode.CodeTypes.WarehouseControlledPremisesID;
			entryInstruction.Validation.ValidateCEI_OA_Warehouse();
			AssertNoMessageError(entryInstruction.CEI_OA_WarehouseInfo, "A warehouse authorization number(country PL type CPW) is missing for selected address");
		});
	}

	public void TestCheckCEI_OA_Warehouse2()
	{
		var org = Factory.NewWithValidTestData<OrgHeader>();
		var orgAddress = org.Addresses.AddNew();
		var orgCusCode = orgAddress.CustomsCodes.AddNew();
		var entryInstruction = GetEntryInstruction();
		entryInstruction.CEI_OA_Warehouse2 = orgAddress.PK;

		CombineAssertions(() =>
		{
			orgCusCode.OK_RN_NKCodeCountry = CountryCodes.Poland;
			orgCusCode.OK_CodeType = OrgCusCode.CodeTypes.TerminalControlledPremisesID;
			orgCusCode.OK_CustomsRegNo = "123456789";
			AssertHasMessageError(entryInstruction.CEI_OA_Warehouse2Info, "A warehouse authorization number(country PL type CPW) is missing for selected address");

			orgCusCode.OK_CodeType = OrgCusCode.CodeTypes.WarehouseControlledPremisesID;
			entryInstruction.Validation.ValidateCEI_OA_Warehouse2();
			AssertNoMessageError(entryInstruction.CEI_OA_Warehouse2Info, "A warehouse authorization number(country PL type CPW) is missing for selected address");
		});
	}

	public void TestCheckRuleR408()
	{
		var message = "(R408) - For custom procedure '71' guarantee is not used";
		var entryInstruction = GetEntryInstruction();
		entryInstruction.CEI_Procedure = Constants.ProcedureCodes._71;
		CombineAssertions(() =>
		{
			entryInstruction.Validation.ValidateAll();
			AssertNoRowMessageError("No GuaranteeBondDetails and 71 procedure", entryInstruction, message);
			entryInstruction.Guarantees.AddNew();
			entryInstruction.Validation.ValidateAll();
			AssertHasRowMessageError("GuaranteeBondDetails and 71 procedure", entryInstruction, message);
			entryInstruction.CEI_Procedure = Constants.ProcedureCodes._11;
			entryInstruction.Validation.ValidateAll();
			AssertNoRowMessageError("GuaranteeBondDetails and 11 procedure", entryInstruction, message);
		});
	}

	public void TestCheckRuleE1301()
	{
		var message = "You have entered different valuation method codes for the Entry (E1301-not allowed in transition period)";
		var entryInstruction = GetEntryInstruction();

		var invoice1 = declaration.Invoices.AddNew();
		var invoiceLine1 = invoice1.InvoiceLines.AddNew();
		invoiceLine1.JI_CEI = entryInstruction.PK;

		var invoice2 = declaration.Invoices.AddNew();
		var invoiceLine2 = invoice2.InvoiceLines.AddNew();
		invoiceLine2.JI_CEI = entryInstruction.PK;

		CombineAssertions(() =>
		{
			using (declaration.TemporarilySetIsAESTransitionPeriod(true))
			{
				invoice1.JZ_ValuationCode = "A";
				invoice2.JZ_ValuationCode = "B";
				entryInstruction.Validation.ValidateAll();
				AssertHasRowMessageError("\u00D7 unique code", entryInstruction, message);

				invoice2.JZ_ValuationCode = "A";
				entryInstruction.Validation.ValidateAll();
				AssertNoRowMessageError("\u2713 unique code", entryInstruction, message);
			}
			using (declaration.TemporarilySetIsAESTransitionPeriod(false))
			{
				invoice1.JZ_ValuationCode = "A";
				invoice2.JZ_ValuationCode = "B";
				entryInstruction.Validation.ValidateAll();
				AssertNoRowMessageError("After the transition period", entryInstruction, message);
			}
		});
	}

	public void TestCheckRuleR286()
	{
		var message = "(R286/R842) - Guarantee is required";
		var entryInstruction = GetEntryInstruction();
		CombineAssertions(() =>
		{
			AssertEquals("No guarantees", 0, entryInstruction.Guarantees.Count);

			entryInstruction.Validation.ValidateAll();
			AssertNoRowMessageError("No entry fees", entryInstruction, message);

			var fee = GetEntryLineFee(entryInstruction);
			fee.CF_ChargeType = EU.Business.UniversalReferenceConstants.RefCusRateCodes.CustomsDutyOnIndustrialProducts;
			fee.CF_MethodOfPayment = PLMethodOfPaymentList.Codes.E;

			entryInstruction.CEI_Procedure = Constants.ProcedureCodes._11;
			entryInstruction.Validation.ValidateAll();
			AssertHasRowMessageError("R286", entryInstruction, message);

			entryInstruction.CEI_Procedure = Constants.ProcedureCodes._71;
			entryInstruction.Validation.ValidateAll();
			AssertNoRowMessageError("R286 - no error because 71 procedure", entryInstruction, message);

			entryInstruction.CEI_Procedure = Constants.ProcedureCodes._21;
			fee.CF_ChargeType = EU.Business.UniversalReferenceConstants.RefCusRateCodes.Vat;
			AssertNoRowMessageError("R286 - no error as no fee with Charge Type A00", entryInstruction, message);

			fee.CF_ChargeType = EU.Business.UniversalReferenceConstants.RefCusRateCodes.CustomsDutyOnIndustrialProducts;
			fee.CF_MethodOfPayment = PLMethodOfPaymentList.Codes.G;
			AssertNoRowMessageError("R286 - no error as no fee with MoP E", entryInstruction, message);

			fee.CF_MethodOfPayment = PLMethodOfPaymentList.Codes.E;
			entryInstruction.Guarantees.AddNew();
			entryInstruction.Validation.ValidateAll();
			AssertNoRowMessageError("R286 - no error as GuaranteeBondDetails exist", entryInstruction, message);
		});
	}

	public void TestCheckRuleR842()
	{
		var message = "(R286/R842) - Guarantee is required";
		var entryInstruction = GetEntryInstruction();
		var errorR842MoPCodes = new[] { PLMethodOfPaymentList.Codes.R, PLMethodOfPaymentList.Codes.D, PLMethodOfPaymentList.Codes.E };
		CombineAssertions(() =>
		{
			AssertEquals("No guarantees", 0, entryInstruction.Guarantees.Count);

			entryInstruction.Validation.ValidateAll();
			AssertNoRowMessageError("No entry fees", entryInstruction, message);

			var fee = GetEntryLineFee(entryInstruction);
			fee.CF_ChargeAmount = 10M;
			entryInstruction.CEI_Procedure = Constants.ProcedureCodes._11;

			foreach (var code in errorR842MoPCodes)
			{
				fee.CF_MethodOfPayment = code;
				entryInstruction.Validation.ValidateAll();
				AssertHasRowMessageError($"R842 error when MoP is {code}", entryInstruction, message);
			}

			entryInstruction.CEI_Procedure = Constants.ProcedureCodes._71;
			entryInstruction.Validation.ValidateAll();
			AssertNoRowMessageError("R842 - no error because 71 procedure", entryInstruction, message);

			entryInstruction.CEI_Procedure = Constants.ProcedureCodes._21;
			fee.CF_ChargeAmount = 0M;
			AssertNoRowMessageError("R842 - no error as no fee with Charge Amount > 0", entryInstruction, message);

			fee.CF_ChargeAmount = 10M;
			fee.CF_MethodOfPayment = PLMethodOfPaymentList.Codes.G;
			AssertNoRowMessageError("R842 - no error as no fee with MoP R/D/E", entryInstruction, message);

			fee.CF_MethodOfPayment = PLMethodOfPaymentList.Codes.E;
			entryInstruction.Guarantees.AddNew();
			entryInstruction.Validation.ValidateAll();
			AssertNoRowMessageError("R842 - no error as GuaranteeBondDetails exist", entryInstruction, message);
		});
	}

	public void TestCheckAllRelatedInvoicesMustHaveSameCurrency()
	{
		var entryInstruction = Factory.NewWithValidTestData<CusEntryInstruction>();
		AssertEquals(true, new CusEntryInstructionValidationExposed(entryInstruction).AllRelatedInvoicesMustHaveSameCurrency_Exposed);
	}

	protected override void SetUp()
	{
		base.SetUp();
		declaration = Factory.New<JobDeclaration>();
	}
	JobDeclaration declaration;

	CusEntryInstruction GetEntryInstruction()
	{
		return declaration.CustomsEntryInstructions.AddNew();
	}

	CusEntryLineFee GetEntryLineFee(CusEntryInstruction entryInstruction)
	{
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		entryHeader.CH_CEI_Instruction = entryInstruction.PK;
		var entryLine = entryHeader.AllEntryLines.AddNew();
		return entryLine.Fees.AddNew();
	}

	class CusEntryInstructionValidationExposed : CusEntryInstructionValidation
	{
		public CusEntryInstructionValidationExposed(CusEntryInstruction parent) : base(parent)
		{
		}

		public ZBool AllRelatedInvoicesMustHaveSameCurrency_Exposed => base.AllRelatedInvoicesMustHaveSameCurrency;
	}
}
