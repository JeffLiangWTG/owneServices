using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.PL.Business.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.PL.Business.Declaration.Testing;

sealed class ImportJobComInvoiceHeaderValidationTest : BusinessObjectValidationTestCase
{
	public void TestCheckRuleR278()
	{
		var messageError = "(R278) You have not entered a Transaction Circumstances.";
		var (declaration, entryInstruction, invoice, invoiceLine) = GetNewImportInvoiceHeader();

		CombineAssertions(() =>
		{
			invoice.Validation.ValidateTranCircumstance1();
			AssertHasMessageError("Empty Setup", invoice.TranCircumstanceCode1Info, messageError);

			var concessionCode = invoiceLine.AdditionalProcedureCodes.AddNew();
			entryInstruction.CEI_Procedure = Constants.ProcedureCodes._31;
			concessionCode.CY_Code = Constants.ConcessionCodes._2PL;
			invoice.Validation.ValidateTranCircumstance1();
			AssertNoMessageError("Concession Code is 2PL with Procedure Code 31", invoice.TranCircumstanceCode1Info, messageError);

			entryInstruction.CEI_Procedure = Constants.ProcedureCodes._71;
			invoice.Validation.ValidateTranCircumstance1();
			AssertNoMessageError("Concession Code is 2PL with Procedure Code 71", invoice.TranCircumstanceCode1Info, messageError);

			entryInstruction.CEI_Procedure = Constants.ProcedureCodes._76;
			invoice.Validation.ValidateTranCircumstance1();
			AssertNoMessageError("Concession Code is 2PL with Procedure Code 76", invoice.TranCircumstanceCode1Info, messageError);

			entryInstruction.CEI_Procedure = Constants.ProcedureCodes._31;
			concessionCode.CY_Code = Constants.ConcessionCodes.C20;
			invoice.Validation.ValidateTranCircumstance1();
			AssertHasMessageError("Concession Code is not 2PL with procedure code 71/76 not specified", invoice.TranCircumstanceCode1Info, messageError);

			concessionCode.CY_Code = Constants.ConcessionCodes._2PL;
			invoice.TranCircumstanceCode1 = "A";
			AssertNoMessageError("ZG_ValuationMethod is not empty", invoice.TranCircumstanceCode1Info, messageError);
		});
	}

	public void TestCheckJZ_OH_Supplier()
	{
		var (declaration, entryInstruction, invoice, invoiceLine) = GetNewImportInvoiceHeader();

		var orgHeaderBUS = Factory.NewWithValidTestData<OrgHeader>();
		orgHeaderBUS.OH_Category = OrgConstants.Category.Business;
		orgHeaderBUS.OH_FullName = new ZString('1', 80);

		var orgHeaderNAT = Factory.NewWithValidTestData<OrgHeader>();
		orgHeaderNAT.OH_Category = OrgConstants.Category.NaturalPersonIndividual;
		orgHeaderNAT.OH_FullName = new ZString('1', 40).Insert(3, " ");

		var orgHeaderNATEmpty = Factory.NewWithValidTestData<OrgHeader>();
		orgHeaderNATEmpty.OH_Category = OrgConstants.Category.NaturalPersonIndividual;
		orgHeaderNATEmpty.OH_FullName = ZString.Empty;

		PLOrgHeaderValidationHelperTest.AssertValidationOrganizationName(invoice.JZ_OH_SupplierInfo, orgHeaderBUS, orgHeaderNAT, orgHeaderNATEmpty);
	}

	public void TestCheckJZ_IncoTermPlace()
	{
		var (declaration, entryInstruction, invoice, invoiceLine) = GetNewImportInvoiceHeader();
		var propertyInfo = invoice.JZ_IncoTermPlaceInfo;

		var entryInstruction2 = declaration.CustomsEntryInstructions.AddNew();
		var invoiceLine2 = invoice.InvoiceLines.AddNew();
		invoiceLine2.JI_CEI = entryInstruction2.PK;
		entryInstruction2.CEI_Procedure = Constants.ProcedureCodes._71;

		CombineAssertions(() =>
		{
			AssertNoMessageErrorContaining("Empty setup", propertyInfo, MandatoryValidation.YouHaveNotEntered);

			entryInstruction.CEI_Procedure = Constants.ProcedureCodes._48;
			invoice.Validation.ValidateJZ_IncoTermPlace();
			AssertHasMessageErrorContaining("procedure code is not 71 and 76", propertyInfo, MandatoryValidation.YouHaveNotEntered);

			entryInstruction.CEI_Procedure = Constants.ProcedureCodes._71;
			invoice.Validation.ValidateJZ_IncoTermPlace();
			AssertNoMessageErrorContaining("procedure code 71", propertyInfo, MandatoryValidation.YouHaveNotEntered);

			entryInstruction.CEI_Procedure = Constants.ProcedureCodes._76;
			invoice.Validation.ValidateJZ_IncoTermPlace();
			AssertNoMessageErrorContaining("procedure code 76", propertyInfo, MandatoryValidation.YouHaveNotEntered);

			entryInstruction2.CEI_Procedure = Constants.ProcedureCodes._48;
			invoice.Validation.ValidateJZ_IncoTermPlace();
			AssertHasMessageErrorContaining("entryInstruction2 procedure code is not 71 and 76", propertyInfo, MandatoryValidation.YouHaveNotEntered);

			invoice.JZ_IncoTermPlace = "ABC";
			AssertNoMessageErrorContaining("Empty setup", propertyInfo, MandatoryValidation.YouHaveNotEntered);
		});
	}

	(JobDeclaration, CusEntryInstruction, JobComInvoiceHeader, JobComInvoiceLine) GetNewImportInvoiceHeader()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = MessageTypeList.Codes.Import;
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		var invoice = declaration.Invoices.AddNew();
		var invoiceLine = invoice.InvoiceLines.AddNew();
		invoiceLine.JI_CEI = entryInstruction.PK;

		return (declaration, entryInstruction, invoice, invoiceLine);
	}
}
