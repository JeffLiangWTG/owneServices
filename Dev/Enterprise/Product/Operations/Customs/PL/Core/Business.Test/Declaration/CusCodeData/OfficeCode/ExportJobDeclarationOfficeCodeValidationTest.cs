using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.PL.Business.Testing;
using static Enterprise.Customs.PL.Business.Constants;

namespace Enterprise.Customs.PL.Business.Declaration.Testing;

sealed class ExportJobDeclarationOfficeCodeValidationTest : CusCodeDataValidationTest
{
	public void TestCheckRuleG0066()
	{
		var declaration = Factory.New<JobDeclaration>();
		var office = declaration.CustomsOfficesForBinding.AddNew();
		var instruction = declaration.CustomsEntryInstructions.AddNew();
		var messageError = "(G0066) Presentation Customs Office code is not allowed.";

		CombineAssertions(() =>
		{
			instruction.CEI_SubStyle = EntrySubStyleList.Codes.NormalDeclaration;
			office.CY_Code = EuOfficeCodesTypes.Codes.OfficeOfPresentation;
			AssertNoMessageError("!IsIE515BMessage", office.CY_CodeInfo, messageError);

			instruction.CEI_SubStyle = EntrySubStyleList.Codes.IncompleteDeclaration;
			office.Validation.ValidateCY_Code();
			AssertHasMessageError("IsIE515BMessage|B", office.CY_CodeInfo, messageError);

			instruction.CEI_SubStyle = EntrySubStyleList.Codes.PreliminaryDeclarationUnderCodeB;
			office.Validation.ValidateCY_Code();
			AssertHasMessageError("IsIE515BMessage|E", office.CY_CodeInfo, messageError);

			instruction.CEI_SubStyle = EntrySubStyleList.Codes.NormalDeclaration;
			office.Validation.ValidateCY_Code();
			AssertNoMessageError("!IsIE515BMessage again", office.CY_CodeInfo, messageError);
		});
	}

	public void TestCheckRuleR0006E()
	{
		Factory.AddCodeToCusMap_EUNAU(CusAuthorizationUsageType.C513);
		var declaration = Factory.New<JobDeclaration>();
		var office = declaration.CustomsOfficesForBinding.AddNew();
		var instruction = declaration.CustomsEntryInstructions.AddNew();
		var cusAuthorizationUsage = instruction.CusAuthorizationUsages.AddNew();
		var messageError = "(R0006E) Presentation Customs Office cannot be specified if the authorization code C513 does not exist.";

		CombineAssertions(() =>
		{
			cusAuthorizationUsage.AGC_Code = CusAuthorizationUsageType.C513;
			office.CY_Code = EuOfficeCodesTypes.Codes.OfficeOfPresentation;
			AssertNoMessageError("Valid", office.CY_CodeInfo, messageError);

			cusAuthorizationUsage.AGC_Code = ZString.Empty;
			office.Validation.ValidateCY_Code();
			AssertHasMessageError("Invalid", office.CY_CodeInfo, messageError);
		});
	}

	public void TestCheckRuleR0091ECodeRequired()
	{
		const string messageError = "[R0091E] Supervising Customs Office ’SCO’ is required with Additional Information code '00100'.";

		var declaration = Factory.New<JobDeclaration>();
		var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
		var additionDocument = invoiceLine.AdditionalInfos.AddNew();
		var office = declaration.CustomsOfficesForBinding.AddNew();

		CombineAssertions(() =>
		{
			additionDocument.CSI_Code = AdditionalInfoCodes._00100;
			additionDocument.CSI_SubType = AdditionalInfoTypes.INF;
			office.CY_Code = EuOfficeCodesTypes.Codes.CentralOfficeCommonDomain;
			office.Validation.ValidateAll();
			AssertHasRowMessageError("SCO code is required ", office, messageError);

			office.CY_Code = EuOfficeCodesTypes.Codes.SupervisingCustomsOffice;
			office.Validation.ValidateAll();
			AssertNoRowMessageError("SCO code exists", office, messageError);

			additionDocument.CSI_SubType = "AAA";
			office.CY_Data = ZString.Empty;
			office.Validation.ValidateAll();
			AssertNoRowMessageError("The SubType is not INF", office, messageError);

			additionDocument.CSI_Code = AdditionalInfoCodes._POW01;
			additionDocument.CSI_SubType = AdditionalInfoTypes.INF;
			office.Validation.ValidateAll();
			AssertNoRowMessageError("The code of additional document is not 00100", office, messageError);

			additionDocument.CSI_Code = AdditionalInfoCodes._00100;
			office.CY_Code = EuOfficeCodesTypes.Codes.ActualExitOffice;
			office.Validation.ValidateAll();
			AssertHasRowMessageError("The Customs Office is not SupervisingCustomsOffice", office, messageError);

			additionDocument.CSI_SubType = AdditionalInfoTypes.INF;
			office.CY_Code = EuOfficeCodesTypes.Codes.CentralOfficeCommonDomain;
			var officeExtra = declaration.CustomsOfficesForBinding.AddNew();
			officeExtra.CY_Code = EuOfficeCodesTypes.Codes.ActualExitOffice;
			office.Validation.ValidateAll();
			AssertHasRowMessageError("No SCO Code", office, messageError);

			officeExtra.CY_Code = EuOfficeCodesTypes.Codes.SupervisingCustomsOffice;
			office.Validation.ValidateAll();
			AssertNoRowMessageError("No SCO Code", office, messageError);
		});
	}

	public void TestCheckRuleR0091EDataSame()
	{
		const string messageError = "[R0091E] Supervising Customs Office ’SCO’ must be equal Customs Office of Export with Additional Information code '00100'.";
		var declaration = Factory.New<JobDeclaration>();
		var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
		var additionDocument = invoiceLine.AdditionalInfos.AddNew();
		var office = declaration.CustomsOfficesForBinding.AddNew();

		CombineAssertions(() =>
		{
			additionDocument.CSI_Code = AdditionalInfoCodes._00100;
			additionDocument.CSI_SubType = AdditionalInfoTypes.INF;
			office.CY_Code = EuOfficeCodesTypes.Codes.SupervisingCustomsOffice;
			declaration.JE_CustomsOffice = "AAAA";
			office.CY_Data = "SSSS";
			office.Validation.ValidateAll();
			AssertHasRowMessageError("The Data is different from Customs Office of Export", office, messageError);

			office.CY_Data = "AAAA";
			office.Validation.ValidateAll();
			AssertNoRowMessageError("The Data is the same with Customs Office of Export", office, messageError);

			additionDocument.CSI_SubType = "TTT";
			office.Validation.ValidateAll();
			AssertNoRowMessageError("The SubType is not INF", office, messageError);

			additionDocument.CSI_Code = AdditionalInfoCodes._POW01;
			additionDocument.CSI_SubType = AdditionalInfoTypes.INF;
			office.Validation.ValidateAll();
			AssertNoRowMessageError("The code of additional document is not 00100", office, messageError);

			additionDocument.CSI_Code = AdditionalInfoCodes._00100;
			office.CY_Code = EuOfficeCodesTypes.Codes.ActualExitOffice;
			office.Validation.ValidateAll();
			AssertNoRowMessageError("The Customs Office is not SupervisingCustomsOffice", office, messageError);

			var officeExtra = declaration.CustomsOfficesForBinding.AddNew();
			officeExtra.CY_Code = EuOfficeCodesTypes.Codes.SupervisingCustomsOffice;
			officeExtra.CY_Data = "SSSS";
			office.Validation.ValidateAll();
			AssertHasRowMessageError("The Data is different from Customs Office of Export", office, messageError);
		});
	}

	public void TestCheckRuleR0007E()
	{
		Factory.AddCodeToCusMap_EUNAU(Constants.CusAuthorizationUsageType.C513);
		var declaration = Factory.New<JobDeclaration>();
		var office = declaration.CustomsOfficesForBinding.AddNew();
		var instruction = declaration.CustomsEntryInstructions.AddNew();
		var cusAuthorizationUsage = instruction.CusAuthorizationUsages.AddNew();
		var messageError = "(R0007E) A Decl. Customs Office code cannot be the same as a Customs Office of Presentation";

		declaration.JE_CustomsOffice = "AAAA";
		office.CY_Data = "AAAA";
		instruction.CEI_SubStyle = "A";
		cusAuthorizationUsage.AGC_Code = CusAuthorizationUsageType.C513;
		office.CY_Code = EuOfficeCodesTypes.Codes.OfficeOfPresentation;
		CombineAssertions(() =>
		{
			office.Validation.ValidateCY_Data();
			AssertHasMessageError("CEI_SubStyle is A with same OfficeOfPresentation and JE_CustomsOffice", office.CY_DataInfo, messageError);

			instruction.CEI_SubStyle = "B";
			office.Validation.ValidateCY_Data();
			AssertHasMessageError("CEI_SubStyle is B  with same OfficeOfPresentation and JE_CustomsOffice", office.CY_DataInfo, messageError);

			office.CY_Data = "EEEE";
			AssertNoMessageError("CEI_SubStyle is B and OfficeOfPresentation and JE_CustomsOffice are different", office.CY_DataInfo, messageError);

			office.CY_Data = "AAAA";
			instruction.CEI_SubStyle = "E";
			office.Validation.ValidateCY_Data();
			AssertHasMessageError("CEI_SubStyle is E with same OfficeOfPresentation and JE_CustomsOffice", office.CY_DataInfo, messageError);

			office.CY_Data = "EEEE";
			AssertNoMessageError("CEI_SubStyle is E and OfficeOfPresentation and JE_CustomsOffice are different", office.CY_DataInfo, messageError);
		});
	}

	public void TestCheckRuleG0187()
	{
		var declaration = Factory.New<JobDeclaration>();
		var office = declaration.CustomsOfficesForBinding.AddNew();
		var messageError = "(G0187) A Decl. Customs Office of Export and Supervising Customs Office codes must be different.";

		declaration.JE_CustomsOffice = "AAAA";
		office.CY_Code = EuOfficeCodesTypes.Codes.SupervisingCustomsOffice;
		CombineAssertions(() =>
		{
			office.CY_Data = "AAAA";
			AssertHasMessageError("Customs Office of Export and Supervising Customs Office are same", office.CY_DataInfo, messageError);

			office.CY_Data = "EEEE";
			AssertNoMessageError("Customs Office of Export and Supervising Customs Office  are different", office.CY_DataInfo, messageError);
		});
	}
}
