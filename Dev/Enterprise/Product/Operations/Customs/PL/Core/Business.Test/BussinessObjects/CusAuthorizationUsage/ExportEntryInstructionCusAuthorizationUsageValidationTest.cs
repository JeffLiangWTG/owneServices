using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.PL.Business.Declaration;
using static Enterprise.Customs.PL.Business.Constants;
using EntrySubStyleList = Enterprise.Customs.PL.Business.Declaration.EntrySubStyleList;

namespace Enterprise.Customs.PL.Business.Testing;

sealed class ExportEntryInstructionCusAuthorizationUsageValidationTest : BusinessObjectValidationTestCase
{
	public void TestCheckAGC_Code()
	{
		CusAuthorizationUsageTestHelper.AddAuthorizationUsageCodes(Factory);
		var declaration = Factory.New<JobDeclaration>();
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		var cusAuthorizationUsage = entryInstruction.CusAuthorizationUsages.AddNew();
		var propertyInfo = cusAuthorizationUsage.AGC_CodeInfo;

		CombineAssertions(() =>
		{
			ValidationTestHelper.AssertErrorIfNotEntered(propertyInfo);
			ValidationTestHelper.AssertInvalidCodeMessageError(propertyInfo, "XXX", "123");
		});
	}

	public void TestCheckRuleG0066()
	{
		Factory.AddCodeToCusMap_EUNAU(Constants.CusAuthorizationUsageType.C513);
		var messageError = "(G0066) Centralized Clearance not allowed for Sub-style B and E.";
		var declaration = Factory.New<JobDeclaration>();
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		var cusAuthorizationUsage = entryInstruction.CusAuthorizationUsages.AddNew();
		var propertyInfo = cusAuthorizationUsage.AGC_CodeInfo;

		entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.NormalDeclaration;
		cusAuthorizationUsage.AGC_Code = CusAuthorizationUsageType.C513;
		CombineAssertions(() =>
		{
			AssertNoMessageError("CEI_SubStyle is NormalDeclaration and AGC_Code is C513", propertyInfo, messageError);

			entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.IncompleteDeclaration;
			cusAuthorizationUsage.Validation.ValidateAGC_Code();
			AssertHasMessageError("CEI_SubStyle is IncompleteDeclaration and AGC_Code is C513", propertyInfo, messageError);

			entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.PreliminaryDeclarationUnderCodeB;
			cusAuthorizationUsage.Validation.ValidateAGC_Code();
			AssertHasMessageError("CEI_SubStyle is PreliminaryDeclarationUnderCodeB and AGC_Code is C513", propertyInfo, messageError);

			cusAuthorizationUsage.AGC_Code = ZString.Empty;
			AssertNoMessageError("CEI_SubStyle is PreliminaryDeclarationUnderCodeB and AGC_Code is empty", propertyInfo, messageError);
		});
	}

	public void TestCheckRuleR0006E()
	{
		Factory.AddCodeToCusMap_EUNAU(Constants.CusAuthorizationUsageType.C513);
		var messageError = "(R0006E) Office of Presentation is required for Centralized Clearance.";
		var declaration = Factory.New<JobDeclaration>();
		var office = declaration.CustomsOfficesForBinding.AddNew();
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		var cusAuthorizationUsage = entryInstruction.CusAuthorizationUsages.AddNew();
		var propertyInfo = cusAuthorizationUsage.AGC_CodeInfo;

		office.CY_Code = EuOfficeCodesTypes.Codes.OfficeOfPresentation;
		office.CY_Data = "ABC";

		entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.IncompleteDeclaration;
		cusAuthorizationUsage.AGC_Code = CusAuthorizationUsageType.C513;
		CombineAssertions(() =>
		{
			AssertNoMessageError("CEI_SubStyle is IncompleteDeclaration and AGC_Code is C513", propertyInfo, messageError);

			entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.PreliminaryDeclarationUnderCodeB;
			cusAuthorizationUsage.Validation.ValidateAGC_Code();
			AssertNoMessageError("CEI_SubStyle is PreliminaryDeclarationUnderCodeB and AGC_Code is C513", propertyInfo, messageError);

			entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.NormalDeclaration;
			cusAuthorizationUsage.Validation.ValidateAGC_Code();
			AssertNoMessageError("CEI_SubStyle is NormalDeclaration and AGC_Code is C513", propertyInfo, messageError);

			cusAuthorizationUsage.AGC_Code = ZString.Empty;
			AssertNoMessageError("CEI_SubStyle is NormalDeclaration and AGC_Code is empty", propertyInfo, messageError);

			office.CY_Code = EuOfficeCodesTypes.Codes.ActualExitOffice;
			cusAuthorizationUsage.AGC_Code = CusAuthorizationUsageType.C513;
			AssertHasMessageError("Office is ActualExitOffice and AGC_Code is C513", propertyInfo, messageError);
		});
	}

	public void TestCheckRuleR0026E()
	{
		var agcCodeList = new List<ZString> { Constants.CusAuthorizationUsageType.C512, Constants.CusAuthorizationUsageType.C513
			, Constants.CusAuthorizationUsageType.C514, Constants.CusAuthorizationUsageType.C515 };
		foreach (var agcCode in agcCodeList)
		{
			Factory.AddCodeToCusMap_EUNAU(agcCode);
		}

		var messageError = "(R0026E) For Authorization codes C512,C513,C514,C515 only one unique authorization number is allowed";
		var declaration = Factory.New<JobDeclaration>();
		var office = declaration.CustomsOfficesForBinding.AddNew();
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		var cusAuthorizationUsage = entryInstruction.CusAuthorizationUsages.AddNew();
		var duplicateCusAuthorizationUsage = entryInstruction.CusAuthorizationUsages.AddNew();

		CombineAssertions(() =>
		{
			cusAuthorizationUsage.AGC_Code = "C511";
			duplicateCusAuthorizationUsage.AGC_Code = "C511";
			cusAuthorizationUsage.Validation.ValidateAGC_Code();
			AssertNoMessageError($"Authorisation codes : C511 and C511", cusAuthorizationUsage.AGC_CodeInfo, messageError);
			foreach (var agcCode in agcCodeList)
			{
				cusAuthorizationUsage.AGC_Code = agcCode;
				duplicateCusAuthorizationUsage.AGC_Code = ZString.Empty;
				cusAuthorizationUsage.Validation.ValidateAGC_Code();
				AssertNoMessageError($"Authorisation codes : empty and {agcCode}", cusAuthorizationUsage.AGC_CodeInfo, messageError);

				duplicateCusAuthorizationUsage.AGC_Code = "C511";
				cusAuthorizationUsage.Validation.ValidateAGC_Code();
				AssertNoMessageError($"Authorisation codes : C511 and {agcCode}", cusAuthorizationUsage.AGC_CodeInfo, messageError);

				duplicateCusAuthorizationUsage.AGC_Code = agcCode;
				cusAuthorizationUsage.Validation.ValidateAGC_Code();
				AssertHasMessageError($"Authorisation codes : Duplicated {agcCode}", cusAuthorizationUsage.AGC_CodeInfo, messageError);
			}
		});
	}

	public void TestCheckRuleR0027E()
	{
		Factory.AddCodeToCusMap_EUNAU(Constants.CusAuthorizationUsageType.C512);
		Factory.AddCodeToCusMap_EUNAU(Constants.CusAuthorizationUsageType.C514);
		var messageError = "(R0027E) Authorization codes C512,C514 cannot appear together in one customs declaration";
		var declaration = Factory.New<JobDeclaration>();
		var office = declaration.CustomsOfficesForBinding.AddNew();
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		var cusAuthorizationUsage1 = entryInstruction.CusAuthorizationUsages.AddNew();
		var cusAuthorizationUsage2 = entryInstruction.CusAuthorizationUsages.AddNew();

		CombineAssertions(() =>
		{
			cusAuthorizationUsage1.AGC_Code = Constants.CusAuthorizationUsageType.C512;
			cusAuthorizationUsage2.AGC_Code = Constants.CusAuthorizationUsageType.C512;
			AssertNoMessageError("Duplicate C512", cusAuthorizationUsage2.AGC_CodeInfo, messageError);

			cusAuthorizationUsage1.AGC_Code = Constants.CusAuthorizationUsageType.C512;
			cusAuthorizationUsage2.AGC_Code = Constants.CusAuthorizationUsageType.C514;
			AssertHasMessageError("C512 and C514", cusAuthorizationUsage2.AGC_CodeInfo, messageError);

			cusAuthorizationUsage1.AGC_Code = Constants.CusAuthorizationUsageType.C514;
			cusAuthorizationUsage2.AGC_Code = Constants.CusAuthorizationUsageType.C512;
			AssertHasMessageError("C514 and C512", cusAuthorizationUsage2.AGC_CodeInfo, messageError);

			cusAuthorizationUsage1.AGC_Code = Constants.CusAuthorizationUsageType.C514;
			cusAuthorizationUsage2.AGC_Code = Constants.CusAuthorizationUsageType.C513;
			AssertNoMessageError("C514 and C513", cusAuthorizationUsage2.AGC_CodeInfo, messageError);
		});
	}

	public void TestCheckRuleR0031E()
	{
		Factory.AddCodeToCusMap_EUNAU(CusAuthorizationUsageType.C019);
		Factory.AddCodeToCusMap_EUNAU(CusAuthorizationUsageType.C601);
		var messageError = "(R0031E) Supporting Document C710 or 4DK3 is required for each Entry Line";
		var declaration = Factory.New<JobDeclaration>();
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		var invoice = declaration.Invoices.AddNew();
		var invoiceLine = invoice.InvoiceLines.AddNew();
		var declarationSupportingDocument = declaration.SupportingDocuments.AddNew();
		var invoiceSupportingDocument = invoice.SupportingDocuments.AddNew();
		var invoiceLineSupportingDocument = invoiceLine.SupportingDocuments.AddNew();
		var cusAuthorizationUsage = entryInstruction.CusAuthorizationUsages.AddNew();

		CombineAssertions(() =>
		{
			AssertNoMessageError("Empty declaration", cusAuthorizationUsage.AGC_CodeInfo, messageError);

			cusAuthorizationUsage.AGC_Code = CusAuthorizationUsageType.C019;
			AssertNoMessageError("AGC_Code is C019", cusAuthorizationUsage.AGC_CodeInfo, messageError);
			cusAuthorizationUsage.AGC_Code = CusAuthorizationUsageType.C601;
			AssertNoMessageError("AGC_Code is C601", cusAuthorizationUsage.AGC_CodeInfo, messageError);
			entryInstruction.CEI_Procedure = ProcedureCodes._11;
			cusAuthorizationUsage.Validation.ValidateAGC_Code();
			AssertHasMessageError("CEI_Procedure is 11 and AGC_Code is C601", cusAuthorizationUsage.AGC_CodeInfo, messageError);
			entryInstruction.CEI_Procedure = ProcedureCodes._21;
			cusAuthorizationUsage.Validation.ValidateAGC_Code();
			AssertNoMessageError("CEI_Procedure is 21 and AGC_Code is C601", cusAuthorizationUsage.AGC_CodeInfo, messageError);

			cusAuthorizationUsage.AGC_Code = CusAuthorizationUsageType.C019;
			AssertHasMessageError("CEI_Procedure is 21 and AGC_Code is C019", cusAuthorizationUsage.AGC_CodeInfo, messageError);
			entryInstruction.CEI_Procedure = ProcedureCodes._11;
			cusAuthorizationUsage.Validation.ValidateAGC_Code();
			AssertNoMessageError("CEI_Procedure is 11 and AGC_Code is C019", cusAuthorizationUsage.AGC_CodeInfo, messageError);

			declarationSupportingDocument.CSI_Code = SupportingDocumentCodes.C710;
			cusAuthorizationUsage.Validation.ValidateAGC_Code();
			AssertNoMessageError("declaration SupportingDocument is C710", cusAuthorizationUsage.AGC_CodeInfo, messageError);

			declarationSupportingDocument.CSI_Code = ZString.Empty;
			invoiceSupportingDocument.CSI_Code = SupportingDocumentCodes.C710;
			cusAuthorizationUsage.Validation.ValidateAGC_Code();
			AssertNoMessageError("invoice SupportingDocument is C710", cusAuthorizationUsage.AGC_CodeInfo, messageError);

			invoiceSupportingDocument.CSI_Code = ZString.Empty;
			invoiceLineSupportingDocument.CSI_Code = SupportingDocumentCodes.C710;
			cusAuthorizationUsage.Validation.ValidateAGC_Code();
			AssertNoMessageError("invoiceLine SupportingDocument is C710", cusAuthorizationUsage.AGC_CodeInfo, messageError);
		});
	}

	public void TestCheckRuleC0848()
	{
		Factory.AddCodeToCusMap_EUNAU(CusAuthorizationUsageType.C626);
		Factory.AddCodeToCusMap_EUNAU(CusAuthorizationUsageType.C627);
		Factory.AddCodeToCusMap_EUNAU(CusAuthorizationUsageType.C514);

		var declaration = Factory.New<JobDeclaration>();
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		var cusAuthorizationUsage = entryInstruction.CusAuthorizationUsages.AddNew();
		var propertyInfo = cusAuthorizationUsage.AGC_OH_OwnerInfo;

		CombineAssertions(() =>
		{
			AssertNoNotifications("Empty is invalid only when Code is C626 or C627", propertyInfo);

			cusAuthorizationUsage.AGC_Code = CusAuthorizationUsageType.C626;
			cusAuthorizationUsage.Validation.ValidateAGC_OH_Owner();
			AssertHasMessageErrorContaining("Code C626 when empty owner", propertyInfo, MandatoryValidation.YouHaveNotEntered);

			cusAuthorizationUsage.AGC_Code = CusAuthorizationUsageType.C627;
			cusAuthorizationUsage.Validation.ValidateAGC_OH_Owner();
			AssertHasMessageErrorContaining("Code C627 when empty owner", propertyInfo, MandatoryValidation.YouHaveNotEntered);

			cusAuthorizationUsage.AGC_Code = CusAuthorizationUsageType.C514;
			cusAuthorizationUsage.Validation.ValidateAGC_OH_Owner();
			AssertNoMessageErrorContaining("Code is not C626 or C627 when empty owner", propertyInfo, MandatoryValidation.YouHaveNotEntered);
		});
	}
}
