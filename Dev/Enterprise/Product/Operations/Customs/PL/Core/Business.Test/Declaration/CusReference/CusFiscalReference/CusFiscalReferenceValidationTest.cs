using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.PL.Business.Declaration.Testing;

sealed class CusFiscalReferenceValidationTest : BusinessObjectValidationTestCase
{
	public void TestCheckOwnerOrgPK()
	{
		var emptyOwnerError = "Please Enter Fiscal Reference Owner";
		var missingTINError = "The TIN number for selected organization is missing.";

		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		cusFiscalReference.CFR_ParentTableCode = CusEntryInstructionSchema.Constants.Prefix;
		cusFiscalReference.CFR_ParentID = entryInstruction.PK;

		cusFiscalReference.OwnerOrgPK = ZGuid.Empty;
		CombineAssertions(() =>
		{
			AssertHasError("Empty owner", cusFiscalReference.OwnerOrgPKInfo, emptyOwnerError);

			var organisation = Factory.New<OrgHeader>();
			var orgAddress = organisation.Addresses.AddNew();
			cusFiscalReference.OwnerOrgPK = organisation.PK;
			AssertNoError("Owner not empty", cusFiscalReference.OwnerOrgPKInfo, emptyOwnerError);
			AssertHasError("Owner without TIN", cusFiscalReference.OwnerOrgPKInfo, missingTINError);

			var orgCusCode = organisation.CustomsCodes.AddNew();
			orgCusCode.OK_CodeType = OrgCusCode.PolandCodeTypes.TIN;
			orgCusCode.OK_CustomsRegNo = "ABC";
			cusFiscalReference.OwnerOrgPK = organisation.PK;
			AssertNoError("Owner with TIN", cusFiscalReference.OwnerOrgPKInfo, missingTINError);
		});
	}

	public void TestCheckCFR_CodeR1504ParentAsEntryInstruction()
	{
		var messageError = "(R1504) Fiscal Role code FR1 cannot be present.";

		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		entryInstruction.CEI_Procedure = Constants.ProcedureCodes._42;
		var cusFiscalReference1 = entryInstruction.FiscalReferences.AddNew();
		cusFiscalReference1.CFR_Code = FiscalReferenceCodeList.Codes.FR1_Importer;

		var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
		var cusFiscalReference2 = invoiceLine.FiscalReferences.AddNew();
		cusFiscalReference2.CFR_Code = FiscalReferenceCodeList.Codes.FR3_TaxRepresentative;
		var organisation = Factory.New<OrgHeader>();
		var orgCusCode = organisation.CustomsCodes.AddNew();
		orgCusCode.OK_CodeType = OrgCusCode.PolandCodeTypes.TIN;
		orgCusCode.OK_CustomsRegNo = "PL123";
		cusFiscalReference2.OwnerOrgPK = organisation.PK;

		var organisation1 = Factory.New<OrgHeader>();
		var orgCusCode1 = organisation1.CustomsCodes.AddNew();
		orgCusCode1.OK_CodeType = OrgCusCode.PolandCodeTypes.TIN;
		orgCusCode1.OK_CustomsRegNo = "PM123";

		CombineAssertions(() =>
		{
			AssertNoMessageError("There is no invoice Line associated with the entry instruction, so there is no fiscal reference with code FR3 for the fiscal reference with code FR1 in the entry instruction", cusFiscalReference1.CFR_CodeInfo, messageError);

			invoiceLine.JI_CEI = entryInstruction.PK;
			cusFiscalReference1.Validation.ValidateCFR_Code();
			AssertHasMessageError("There is a Fiscal Reference with code FR1 when exist a fiscal reference with code FR3 and their entry instruction's procedure is 42 or 63", cusFiscalReference1.CFR_CodeInfo, messageError);

			entryInstruction.CEI_Procedure = Constants.ProcedureCodes._63;
			cusFiscalReference1.Validation.ValidateCFR_Code();
			AssertHasMessageError("There is a Fiscal Reference with code FR1 when exist a fiscal reference with code FR3 and their entry instruction's procedure is 42 or 63", cusFiscalReference1.CFR_CodeInfo, messageError);

			cusFiscalReference2.CFR_Code = FiscalReferenceCodeList.Codes.FR4_HolderOfDeferredPaymentAuth;
			cusFiscalReference1.Validation.ValidateCFR_Code();
			AssertNoMessageError("There is no Fiscal Reference with code FR3", cusFiscalReference1.CFR_CodeInfo, messageError);

			cusFiscalReference2.CFR_Code = FiscalReferenceCodeList.Codes.FR3_TaxRepresentative;
			entryInstruction.CEI_Procedure = Constants.ProcedureCodes._40;
			cusFiscalReference1.Validation.ValidateCFR_Code();
			AssertNoMessageError("There is no an entry instruction's procedure is 42 or 63", cusFiscalReference1.CFR_CodeInfo, messageError);

			entryInstruction.CEI_Procedure = Constants.ProcedureCodes._42;
			cusFiscalReference2.OwnerOrgPK = organisation1.PK;
			cusFiscalReference1.Validation.ValidateCFR_Code();
			AssertNoMessageError("There is a fiscal Reference with code FR3 but the Reference of this fiscal Reference does not start with PL", cusFiscalReference1.CFR_CodeInfo, messageError);

			var cusFiscalReference3 = entryInstruction.FiscalReferences.AddNew();
			cusFiscalReference3.OwnerOrgPK = organisation.PK;
			cusFiscalReference3.CFR_Code = FiscalReferenceCodeList.Codes.FR3_TaxRepresentative;
			cusFiscalReference1.Validation.ValidateCFR_Code();
			AssertHasMessageError("There is a Fiscal Reference with code FR1 when exist a fiscal reference with code FR3 and their entry instruction's procedure is 42 or 63", cusFiscalReference1.CFR_CodeInfo, messageError);

			entryInstruction.CEI_Procedure = Constants.ProcedureCodes._63;
			cusFiscalReference1.Validation.ValidateCFR_Code();
			AssertHasMessageError("There is a Fiscal Reference with code FR1 when a Fiscal Reference with code FR3 and their entry instruction's procedure is 42 or 63", cusFiscalReference1.CFR_CodeInfo, messageError);

			cusFiscalReference3.CFR_Code = FiscalReferenceCodeList.Codes.FR2_Customer;
			cusFiscalReference1.Validation.ValidateCFR_Code();
			AssertNoMessageError("There is no a Fiscal Reference with code FR3", cusFiscalReference1.CFR_CodeInfo, messageError);

			cusFiscalReference3.CFR_Code = FiscalReferenceCodeList.Codes.FR3_TaxRepresentative;
			cusFiscalReference3.OwnerOrgPK = organisation1.PK;
			cusFiscalReference1.Validation.ValidateCFR_Code();
			AssertNoMessageError("There is a fiscal Reference with code FR3 but the Reference of this fiscal Reference is not start with PL", cusFiscalReference1.CFR_CodeInfo, messageError);
		});
	}

	public void TestCheckCFR_CodeR1504ParentAsInvoiceLine()
	{
		var messageError = "(R1504) Fiscal Role code FR1 cannot be present.";

		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		entryInstruction.CEI_Procedure = Constants.ProcedureCodes._42;
		var cusFiscalReference1 = entryInstruction.FiscalReferences.AddNew();
		cusFiscalReference1.CFR_Code = FiscalReferenceCodeList.Codes.FR3_TaxRepresentative;

		var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
		var cusFiscalReference2 = invoiceLine.FiscalReferences.AddNew();
		cusFiscalReference2.CFR_Code = FiscalReferenceCodeList.Codes.FR1_Importer;
		var organisation = Factory.New<OrgHeader>();
		var orgCusCode = organisation.CustomsCodes.AddNew();
		orgCusCode.OK_CodeType = OrgCusCode.PolandCodeTypes.TIN;
		orgCusCode.OK_CustomsRegNo = "PL123";
		cusFiscalReference1.OwnerOrgPK = organisation.PK;

		var organisation1 = Factory.New<OrgHeader>();
		var orgCusCode1 = organisation1.CustomsCodes.AddNew();
		orgCusCode1.OK_CodeType = OrgCusCode.PolandCodeTypes.TIN;
		orgCusCode1.OK_CustomsRegNo = "PM123";

		CombineAssertions(() =>
		{
			AssertNoMessageError("There is no entry instruction associated with the invoice Line, so there is no entry instruction with procedure 42 or 63", cusFiscalReference2.CFR_CodeInfo, messageError);

			invoiceLine.JI_CEI = entryInstruction.PK;
			cusFiscalReference2.Validation.ValidateCFR_Code();
			AssertHasMessageError("There is a Fiscal Reference with code FR1 when exist a fiscal reference with code FR3 and their entry instruction's procedure is 42 or 63", cusFiscalReference2.CFR_CodeInfo, messageError);

			entryInstruction.CEI_Procedure = Constants.ProcedureCodes._63;
			cusFiscalReference2.Validation.ValidateCFR_Code();
			AssertHasMessageError("There is a Fiscal Reference with code FR1 when exist a fiscal reference with code FR3 and their entry instruction's procedure is 42 or 63", cusFiscalReference2.CFR_CodeInfo, messageError);

			cusFiscalReference1.CFR_Code = FiscalReferenceCodeList.Codes.FR4_HolderOfDeferredPaymentAuth;
			cusFiscalReference2.Validation.ValidateCFR_Code();
			AssertNoMessageError("There is no Fiscal Reference with code FR3", cusFiscalReference2.CFR_CodeInfo, messageError);

			cusFiscalReference1.CFR_Code = FiscalReferenceCodeList.Codes.FR3_TaxRepresentative;
			entryInstruction.CEI_Procedure = Constants.ProcedureCodes._40;
			cusFiscalReference2.Validation.ValidateCFR_Code();
			AssertNoMessageError("There is no an entry instruction's procedure is 42 or 63", cusFiscalReference2.CFR_CodeInfo, messageError);

			entryInstruction.CEI_Procedure = Constants.ProcedureCodes._42;
			cusFiscalReference1.OwnerOrgPK = organisation1.PK;
			cusFiscalReference2.Validation.ValidateCFR_Code();
			AssertNoMessageError("There is a fiscal Reference with code FR3 but the Reference of this fiscal Reference does not start with PL", cusFiscalReference1.CFR_CodeInfo, messageError);
		});
	}

	public void TestCheckCFR_CodeR1592ParentAsInvoiceLine()
	{
		var messageError = "(R1592) Fiscal role code FR5 is invalid for the declaration sub style entered.";

		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		invoiceLine.JI_CEI = entryInstruction.PK;
		entryInstruction.CEI_SubStyle = Constants.SubStyleCodes.A;
		var cusFiscalReference1 = invoiceLine.FiscalReferences.AddNew();
		cusFiscalReference1.CFR_Code = FiscalReferenceCodeList.Codes.FR3_TaxRepresentative;

		CombineAssertions(() =>
		{
			AssertNoMessageError("There is no Fiscal Reference with code FR5 and its entry instruction's sub style is A", cusFiscalReference1.CFR_CodeInfo, messageError);

			entryInstruction.CEI_SubStyle = Constants.SubStyleCodes.B;
			cusFiscalReference1.Validation.ValidateCFR_Code();
			AssertNoMessageError("There is no Fiscal Reference with code FR5", cusFiscalReference1.CFR_CodeInfo, messageError);

			cusFiscalReference1.CFR_Code = FiscalReferenceCodeList.Codes.FR5_Vendor;
			AssertHasMessageError("There is a Fiscal Reference with code FR5 and its entry instruction's sub style is not A", cusFiscalReference1.CFR_CodeInfo, messageError);

			invoiceLine.JI_CEI = ZGuid.Empty;
			cusFiscalReference1.Validation.ValidateCFR_Code();
			AssertNoMessageError("There is no Entry Instruction, so there is no entry instruction's sub style is not A", cusFiscalReference1.CFR_CodeInfo, messageError);
		});
	}

	public void TestCheckCFR_CodeR1592ParentAsEntryInstruction()
	{
		var messageError = "(R1592) Fiscal role code FR5 is invalid for the declaration sub style entered.";

		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		entryInstruction.CEI_SubStyle = Constants.SubStyleCodes.A;
		var cusFiscalReference1 = entryInstruction.FiscalReferences.AddNew();
		cusFiscalReference1.CFR_Code = FiscalReferenceCodeList.Codes.FR3_TaxRepresentative;

		CombineAssertions(() =>
		{
			AssertNoMessageError("There is no Fiscal Reference with code FR5 and its entry instruction's sub style is A", cusFiscalReference1.CFR_CodeInfo, messageError);

			entryInstruction.CEI_SubStyle = Constants.SubStyleCodes.B;
			cusFiscalReference1.Validation.ValidateCFR_Code();
			AssertNoMessageError("There is no Fiscal Reference with code FR5", cusFiscalReference1.CFR_CodeInfo, messageError);

			cusFiscalReference1.CFR_Code = FiscalReferenceCodeList.Codes.FR5_Vendor;
			AssertHasMessageError("There is a Fiscal Reference with code FR5 and its entry instruction's sub style is not A", cusFiscalReference1.CFR_CodeInfo, messageError);
		});
	}

	public void TestCheckCFR_CodeR1636ParentAsInvoiceLine()
	{
		var messageError = "(R1636) Fiscal role code FR7 is invalid for the requested procedure entered.";

		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		invoiceLine.JI_CEI = entryInstruction.PK;
		var cusFiscalReference1 = invoiceLine.FiscalReferences.AddNew();
		cusFiscalReference1.CFR_Code = FiscalReferenceCodeList.Codes.FR7_Taxpayer;

		CombineAssertions(() =>
		{
			foreach (var procCode in new[] { Constants.ProcedureCodes._40, Constants.ProcedureCodes._44, Constants.ProcedureCodes._45, Constants.ProcedureCodes._46, Constants.ProcedureCodes._61, Constants.ProcedureCodes._68 })
			{
				invoiceLine.EntryInstruction.CEI_Procedure = procCode;
				cusFiscalReference1.Validation.ValidateCFR_Code();
				AssertNoMessageError("There is a Fiscal Reference with code FR7 but none of its entry instruction's procedure in {40, 44, 45, 46, 61, 68}", cusFiscalReference1.CFR_CodeInfo, messageError);
			}

			invoiceLine.EntryInstruction.CEI_Procedure = Constants.ProcedureCodes._41;
			cusFiscalReference1.Validation.ValidateCFR_Code();
			AssertHasMessageError("There is a Fiscal Reference with code FR7 and its entry instruction's procedure in {40, 44, 45, 46, 61, 68}", cusFiscalReference1.CFR_CodeInfo, messageError);

			cusFiscalReference1.CFR_Code = FiscalReferenceCodeList.Codes.FR4_HolderOfDeferredPaymentAuth;
			AssertNoMessageError("There is no Fiscal Reference with code FR7", cusFiscalReference1.CFR_CodeInfo, messageError);

			invoiceLine.JI_CEI = ZGuid.Empty;
			cusFiscalReference1.CFR_Code = FiscalReferenceCodeList.Codes.FR7_Taxpayer;
			AssertNoMessageError("There is no Entry Instruction ,so there is no entry instruction's procedure in {40, 44, 45, 46, 61, 68}", cusFiscalReference1.CFR_CodeInfo, messageError);
		});
	}

	public void TestCheckCFR_CodeR1636ParentAsEntryInstruction()
	{
		var messageError = "(R1636) Fiscal role code FR7 is invalid for the requested procedure entered.";

		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		var cusFiscalReference1 = entryInstruction.FiscalReferences.AddNew();
		cusFiscalReference1.CFR_Code = FiscalReferenceCodeList.Codes.FR7_Taxpayer;

		CombineAssertions(() =>
		{
			foreach (var procCode in new[] { Constants.ProcedureCodes._40, Constants.ProcedureCodes._44, Constants.ProcedureCodes._45, Constants.ProcedureCodes._46, Constants.ProcedureCodes._61, Constants.ProcedureCodes._68 })
			{
				entryInstruction.CEI_Procedure = procCode;
				cusFiscalReference1.Validation.ValidateCFR_Code();
				AssertNoMessageError("There is no Fiscal Reference with code FR7 and its entry instruction's procedure in {40, 44, 45, 46, 61, 68}", cusFiscalReference1.CFR_CodeInfo, messageError);
			}

			entryInstruction.CEI_Procedure = Constants.ProcedureCodes._41;
			cusFiscalReference1.Validation.ValidateCFR_Code();
			AssertHasMessageError("There is a Fiscal Reference with code FR7 and its entry instruction's procedure in {40, 44, 45, 46, 61, 68}", cusFiscalReference1.CFR_CodeInfo, messageError);

			cusFiscalReference1.CFR_Code = FiscalReferenceCodeList.Codes.FR4_HolderOfDeferredPaymentAuth;
			AssertNoMessageError("There is no Fiscal Reference with code FR7", cusFiscalReference1.CFR_CodeInfo, messageError);
		});
	}

	public void TestCheckCFR_ReferenceR1507ParentAsEntryInstruction()
	{
		var messageError = "(R1507) Tax number starting PL is required.";

		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		entryInstruction.CEI_Procedure = Constants.ProcedureCodes._42;
		var cusFiscalReference1 = entryInstruction.FiscalReferences.AddNew();
		cusFiscalReference1.CFR_Code = FiscalReferenceCodeList.Codes.FR1_Importer;

		var organisation = Factory.New<OrgHeader>();
		var orgCusCode = organisation.CustomsCodes.AddNew();
		orgCusCode.OK_CodeType = OrgCusCode.PolandCodeTypes.TIN;
		orgCusCode.OK_CustomsRegNo = "PL123";
		cusFiscalReference1.OwnerOrgPK = organisation.PK;

		CombineAssertions(() =>
		{
			AssertNoMessageError("The first two characters of CFR_Reference equal to PL", cusFiscalReference1.CFR_ReferenceInfo, messageError);

			var organisation1 = Factory.New<OrgHeader>();
			orgCusCode = organisation1.CustomsCodes.AddNew();
			orgCusCode.OK_CodeType = OrgCusCode.PolandCodeTypes.TIN;
			orgCusCode.OK_CustomsRegNo = "PM123";
			cusFiscalReference1.OwnerOrgPK = organisation1.PK;
			cusFiscalReference1.Validation.ValidateCFR_Reference();
			AssertHasMessageError("The first two characters of CFR_Reference not equal to PL and CFR_Code equal to FR1 and its entry instruction's procedure is 42 or 63", cusFiscalReference1.CFR_ReferenceInfo, messageError);

			cusFiscalReference1.CFR_Code = FiscalReferenceCodeList.Codes.FR4_HolderOfDeferredPaymentAuth;
			cusFiscalReference1.Validation.ValidateCFR_Reference();
			AssertNoMessageError("There is no Fiscal Reference with code FR1", cusFiscalReference1.CFR_ReferenceInfo, messageError);

			cusFiscalReference1.CFR_Code = FiscalReferenceCodeList.Codes.FR1_Importer;
			entryInstruction.CEI_Procedure = Constants.ProcedureCodes._40;
			cusFiscalReference1.Validation.ValidateCFR_Reference();
			AssertNoMessageError("Fiscal Reference's entry instruction's procedure is not 42 or 63", cusFiscalReference1.CFR_ReferenceInfo, messageError);
		});
	}

	public void TestCheckCFR_ReferenceR1507ParentAsInvoiceLine()
	{
		var messageError = "(R1507) Tax number starting PL is required.";

		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		invoiceLine.JI_CEI = entryInstruction.PK;
		entryInstruction.CEI_Procedure = Constants.ProcedureCodes._42;
		var cusFiscalReference1 = invoiceLine.FiscalReferences.AddNew();
		cusFiscalReference1.CFR_Code = FiscalReferenceCodeList.Codes.FR1_Importer;

		var organisation = Factory.New<OrgHeader>();
		var orgCusCode = organisation.CustomsCodes.AddNew();
		orgCusCode.OK_CodeType = OrgCusCode.PolandCodeTypes.TIN;
		orgCusCode.OK_CustomsRegNo = "PL123";
		cusFiscalReference1.OwnerOrgPK = organisation.PK;

		CombineAssertions(() =>
		{
			AssertNoMessageError("The first two characters of CFR_Reference equal to PL", cusFiscalReference1.CFR_ReferenceInfo, messageError);

			var organisation1 = Factory.New<OrgHeader>();
			orgCusCode = organisation1.CustomsCodes.AddNew();
			orgCusCode.OK_CodeType = OrgCusCode.PolandCodeTypes.TIN;
			orgCusCode.OK_CustomsRegNo = "PM123";
			cusFiscalReference1.OwnerOrgPK = organisation1.PK;
			cusFiscalReference1.Validation.ValidateCFR_Reference();
			AssertHasMessageError("The first two characters of CFR_Reference not equal to PL and CFR_Code equal to FR1 and its entry instruction's procedure is 42 or 63", cusFiscalReference1.CFR_ReferenceInfo, messageError);

			cusFiscalReference1.CFR_Code = FiscalReferenceCodeList.Codes.FR4_HolderOfDeferredPaymentAuth;
			cusFiscalReference1.Validation.ValidateCFR_Reference();
			AssertNoMessageError("There is no Fiscal Reference with code FR1", cusFiscalReference1.CFR_ReferenceInfo, messageError);

			cusFiscalReference1.CFR_Code = FiscalReferenceCodeList.Codes.FR1_Importer;
			entryInstruction.CEI_Procedure = Constants.ProcedureCodes._40;
			cusFiscalReference1.Validation.ValidateCFR_Reference();
			AssertNoMessageError("Fiscal Reference's entry instruction's procedure is not 42 or 63", cusFiscalReference1.CFR_ReferenceInfo, messageError);

			invoiceLine.JI_CEI = ZGuid.Empty;
			entryInstruction.CEI_Procedure = Constants.ProcedureCodes._42;
			cusFiscalReference1.Validation.ValidateCFR_Reference();
			AssertNoMessageError("There is no Entry Instruction ,so there is no entry instruction's procedure in {42, 63}", cusFiscalReference1.CFR_CodeInfo, messageError);
		});
	}

	public void TestCheckCFR_ReferenceR1621ParentAsEntryInstruction()
	{
		var messageError = "(R1621) Tax number starting PL is required.";

		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		var cusFiscalReference1 = entryInstruction.FiscalReferences.AddNew();
		cusFiscalReference1.CFR_Code = FiscalReferenceCodeList.Codes.FR7_Taxpayer;

		var organisation = Factory.New<OrgHeader>();
		var orgCusCode = organisation.CustomsCodes.AddNew();
		orgCusCode.OK_CodeType = OrgCusCode.PolandCodeTypes.TIN;
		orgCusCode.OK_CustomsRegNo = "PL123";
		cusFiscalReference1.OwnerOrgPK = organisation.PK;

		CombineAssertions(() =>
		{
			AssertNoMessageError("The first two characters of CFR_Reference equal to PL", cusFiscalReference1.CFR_ReferenceInfo, messageError);

			var organisation1 = Factory.New<OrgHeader>();
			orgCusCode = organisation1.CustomsCodes.AddNew();
			orgCusCode.OK_CodeType = OrgCusCode.PolandCodeTypes.TIN;
			orgCusCode.OK_CustomsRegNo = "PM123";
			cusFiscalReference1.OwnerOrgPK = organisation1.PK;
			cusFiscalReference1.Validation.ValidateCFR_Reference();
			AssertHasMessageError("The first two characters of CFR_Reference not equal to PL and CFR_Code equal to FR7", cusFiscalReference1.CFR_ReferenceInfo, messageError);

			cusFiscalReference1.CFR_Code = FiscalReferenceCodeList.Codes.FR4_HolderOfDeferredPaymentAuth;
			AssertNoMessageError("There is no Fiscal Reference with code FR7", cusFiscalReference1.CFR_ReferenceInfo, messageError);
		});
	}

	public void TestCheckCFR_ReferenceR1621ParentAsInvoiceLine()
	{
		var messageError = "(R1621) Tax number starting PL is required.";

		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		invoiceLine.JI_CEI = entryInstruction.PK;
		var cusFiscalReference1 = invoiceLine.FiscalReferences.AddNew();
		cusFiscalReference1.CFR_Code = FiscalReferenceCodeList.Codes.FR7_Taxpayer;

		var organisation = Factory.New<OrgHeader>();
		var orgCusCode = organisation.CustomsCodes.AddNew();
		orgCusCode.OK_CodeType = OrgCusCode.PolandCodeTypes.TIN;
		orgCusCode.OK_CustomsRegNo = "PL123";
		cusFiscalReference1.OwnerOrgPK = organisation.PK;
		cusFiscalReference1.Validation.ValidateCFR_Reference();

		CombineAssertions(() =>
		{
			AssertNoMessageError("The first two characters of CFR_Reference equal to PL", cusFiscalReference1.CFR_ReferenceInfo, messageError);

			var organisation1 = Factory.New<OrgHeader>();
			orgCusCode = organisation1.CustomsCodes.AddNew();
			orgCusCode.OK_CodeType = OrgCusCode.PolandCodeTypes.TIN;
			orgCusCode.OK_CustomsRegNo = "PM123";
			cusFiscalReference1.OwnerOrgPK = organisation1.PK;
			cusFiscalReference1.Validation.ValidateCFR_Reference();
			AssertHasMessageError("The first two characters of CFR_Reference not equal to PL and CFR_Code equal to FR7", cusFiscalReference1.CFR_ReferenceInfo, messageError);

			cusFiscalReference1.CFR_Code = FiscalReferenceCodeList.Codes.FR4_HolderOfDeferredPaymentAuth;
			AssertNoMessageError("There is no Fiscal Reference with code FR7", cusFiscalReference1.CFR_ReferenceInfo, messageError);
		});
	}

	protected override void SetUp()
	{
		base.SetUp();
		cusFiscalReference = Factory.NewWithValidTestData<CusFiscalReference>();
	}
	CusFiscalReference cusFiscalReference;
}
