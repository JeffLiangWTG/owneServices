using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.PL.Business.Testing;
using static Enterprise.Core.Constants;
using static Enterprise.Customs.PL.Business.Constants;

namespace Enterprise.Customs.PL.Business.Declaration.Testing;

sealed class ExportCusEntryInstructionValidationTest : BusinessObjectValidationTestCase
{
	public void TestCheckRuleR0677()
	{
		Factory.AddCodeToCusMap_EUNAU(Constants.CusAuthorizationUsageType.C513);
		Factory.AddCodeToCusMap_EUNAU(Constants.CusAuthorizationUsageType.C512);
		var (_, entryInstruction) = GetExportInstruction();
		var cusAuthorizationUsage = entryInstruction.CusAuthorizationUsages.AddNew();
		const string messageError = "(R0677) An Authorization code C512 is required for a declaration Sub Style C or F";

		CombineAssertions(() =>
		{
			var subStyleList = new List<ZString> { Constants.SubStyleCodes.C, Constants.SubStyleCodes.F };
			foreach (var subStyle in subStyleList)
			{
				cusAuthorizationUsage.AGC_Code = ZString.Empty;
				entryInstruction.CEI_SubStyle = subStyle;
				AssertHasMessageError($"SubStyle {entryInstruction.CEI_SubStyle} Contains Empty Authorisation", entryInstruction.CEI_SubStyleInfo, messageError);

				cusAuthorizationUsage.AGC_Code = Constants.CusAuthorizationUsageType.C513;
				entryInstruction.Validation.ValidateCEI_SubStyle();
				AssertHasMessageError($"SubStyle {entryInstruction.CEI_SubStyle} Contains C513 Authorisation", entryInstruction.CEI_SubStyleInfo, messageError);

				cusAuthorizationUsage.AGC_Code = Constants.CusAuthorizationUsageType.C512;
				entryInstruction.Validation.ValidateCEI_SubStyle();
				AssertNoMessageError($"SubStyle {entryInstruction.CEI_SubStyle} Contains C512 Authorisation", entryInstruction.CEI_SubStyleInfo, messageError);
			}

			entryInstruction.CEI_SubStyle = Constants.SubStyleCodes.A;
			cusAuthorizationUsage.AGC_Code = Constants.CusAuthorizationUsageType.C513;
			entryInstruction.Validation.ValidateCEI_SubStyle();
			AssertNoMessageError($"SubStyle A Contains C513 Authorisation", entryInstruction.CEI_SubStyleInfo, messageError);
		});
	}

	public void TestCheckRuleR0678()
	{
		Factory.AddCodeToCusMap_EUNAU(Constants.CusAuthorizationUsageType.C513);
		Factory.AddCodeToCusMap_EUNAU(Constants.CusAuthorizationUsageType.C512);
		var (_, entryInstruction) = GetExportInstruction();
		var cusAuthorizationUsage = entryInstruction.CusAuthorizationUsages.AddNew();
		const string messageError = "(R0678) Only declaration Sub Style C or F is allowed for an Authorization code C512";

		CombineAssertions(() =>
		{
			var subStyleList = new List<ZString>() { Constants.SubStyleCodes.C, Constants.SubStyleCodes.F };
			foreach (var subStyle in subStyleList)
			{
				cusAuthorizationUsage.AGC_Code = ZString.Empty;
				entryInstruction.CEI_SubStyle = subStyle;
				AssertNoMessageError($"SubStyle {entryInstruction.CEI_SubStyle} Contains Empty Authorisation", entryInstruction.CEI_SubStyleInfo, messageError);

				cusAuthorizationUsage.AGC_Code = Constants.CusAuthorizationUsageType.C513;
				entryInstruction.Validation.ValidateCEI_SubStyle();
				AssertNoMessageError($"SubStyle {entryInstruction.CEI_SubStyle} Contains C513 Authorisation", entryInstruction.CEI_SubStyleInfo, messageError);

				cusAuthorizationUsage.AGC_Code = Constants.CusAuthorizationUsageType.C512;
				entryInstruction.Validation.ValidateCEI_SubStyle();
				AssertNoMessageError($"SubStyle {entryInstruction.CEI_SubStyle} Contains C512 Authorisation", entryInstruction.CEI_SubStyleInfo, messageError);
			}

			entryInstruction.CEI_SubStyle = Constants.SubStyleCodes.A;
			cusAuthorizationUsage.AGC_Code = Constants.CusAuthorizationUsageType.C513;
			entryInstruction.Validation.ValidateCEI_SubStyle();
			AssertNoMessageError($"SubStyle A Contains C513 Authorisation", entryInstruction.CEI_SubStyleInfo, messageError);

			cusAuthorizationUsage.AGC_Code = Constants.CusAuthorizationUsageType.C512;
			entryInstruction.Validation.ValidateCEI_SubStyle();
			AssertHasMessageError($"SubStyle A Contains C512 Authorisation", entryInstruction.CEI_SubStyleInfo, messageError);
		});
	}

	public void TestCheckRuleR0033E()
	{
		Factory.AddCodeToCusMap_EUNAU(Constants.CusAuthorizationUsageType.C601);
		const string messageError = "(R0033E) an authorization code C601 is required for requested procedure code 11";
		var (_, entryInstruction) = GetExportInstruction();
		var authorizationUsage = entryInstruction.CusAuthorizationUsages.AddNew();

		CombineAssertions(() =>
		{
			AssertNoMessageError("Empty declaration", entryInstruction.CEI_ProcedureInfo, messageError);

			entryInstruction.CEI_Procedure = Constants.ProcedureCodes._11;
			AssertHasMessageError("CEI_Procedure is 11", entryInstruction.CEI_ProcedureInfo, messageError);

			authorizationUsage.AGC_Code = Constants.CusAuthorizationUsageType.C601;
			entryInstruction.Validation.ValidateCEI_Procedure();
			AssertNoMessageError("authorizationUsage is C601", entryInstruction.CEI_ProcedureInfo, messageError);
		});
	}

	public void TestCheckSameIncotermPlaceCodeUsed()
	{
		const string messageError = "All Invoices on an Entry Instruction must have this same Incoterm Place code";
		var (declaration, entryInstruction) = GetExportInstruction();
		var invoice1 = declaration.Invoices.AddNew();
		var invoice2 = declaration.Invoices.AddNew();
		var invoiceLine1 = invoice1.InvoiceLines.AddNew();
		invoiceLine1.JI_CEI = entryInstruction.PK;
		var invoiceLine2 = invoice2.InvoiceLines.AddNew();
		invoiceLine2.JI_CEI = entryInstruction.PK;

		CombineAssertions(() =>
		{
			entryInstruction.Validation.ValidateAll();
			AssertNoRowMessageError("Empty Declaration", entryInstruction, messageError);

			invoice1.ZG_AgreedPlaceCode = CountryCodes.France;
			invoice2.ZG_AgreedPlaceCode = CountryCodes.Germany;
			entryInstruction.Validation.ValidateAll();
			AssertHasRowMessageError("Different ZG_AgreedPlaceCode", entryInstruction, messageError);

			invoice1.ZG_AgreedPlaceCode = CountryCodes.Poland;
			invoice2.ZG_AgreedPlaceCode = CountryCodes.Poland;
			entryInstruction.Validation.ValidateAll();
			AssertNoRowMessageError("Same ZG_AgreedPlaceCode", entryInstruction, messageError);
		});
	}

	public void TestCheckSameTransportChargesMoPUsed()
	{
		const string messageError = "All Invoices on an Entry Instruction must have this same Transp.Charges MoP";
		var (declaration, entryInstruction) = GetExportInstruction();
		var invoice1 = declaration.Invoices.AddNew();
		var invoice2 = declaration.Invoices.AddNew();
		var invoiceLine1 = invoice1.InvoiceLines.AddNew();
		invoiceLine1.JI_CEI = entryInstruction.PK;
		var invoiceLine2 = invoice2.InvoiceLines.AddNew();
		invoiceLine2.JI_CEI = entryInstruction.PK;

		CombineAssertions(() =>
		{
			entryInstruction.Validation.ValidateAll();
			AssertNoRowMessageError("Empty Declaration", entryInstruction, messageError);

			invoice1.ZG_TransportChargesMethodOfPayment = ExportTransportMethodOfPaymentList.Codes.D;
			invoice2.ZG_TransportChargesMethodOfPayment = ExportTransportMethodOfPaymentList.Codes.H;
			entryInstruction.Validation.ValidateAll();
			AssertHasRowMessageError("Different ZG_TransportChargesMethodOfPayment", entryInstruction, messageError);

			invoice1.ZG_TransportChargesMethodOfPayment = ExportTransportMethodOfPaymentList.Codes.A;
			invoice2.ZG_TransportChargesMethodOfPayment = ExportTransportMethodOfPaymentList.Codes.A;
			entryInstruction.Validation.ValidateAll();
			AssertNoRowMessageError("Same ZG_TransportChargesMethodOfPayment", entryInstruction, messageError);
		});
	}

	(JobDeclaration, CusEntryInstruction) GetExportInstruction()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		return (declaration, entryInstruction);
	}

	public void TestCheckRuleR0030E()
	{
		const string messageError = "(R0030E) A supporting document C710 or 4DK3 is required for each Entry Line";
		var (declaration, entryInstruction) = GetExportInstruction();
		var additionalInfo = declaration.AdditionalInfos.AddNew();
		additionalInfo.CSI_SubType = AdditionalInfoTypes.INF;
		var supportingDocument = declaration.SupportingDocuments.AddNew();

		CombineAssertions(() =>
		{
			entryInstruction.Validation.ValidateCEI_Procedure();
			AssertNoMessageError("Empty declaration", entryInstruction.CEI_ProcedureInfo, messageError);

			entryInstruction.CEI_Procedure = ProcedureCodes._21;
			AssertNoMessageError("CEI_Procedure 21", entryInstruction.CEI_ProcedureInfo, messageError);

			entryInstruction.CEI_Procedure = ProcedureCodes._31;
			AssertNoMessageError("CEI_Procedure 31", entryInstruction.CEI_ProcedureInfo, messageError);

			additionalInfo.CSI_Code = AdditionalInfoCodes._00100;
			entryInstruction.Validation.ValidateCEI_Procedure();
			AssertNoMessageError("CEI_Procedure 31 with Additional Info code 00100", entryInstruction.CEI_ProcedureInfo, messageError);

			entryInstruction.CEI_Procedure = ProcedureCodes._21;
			AssertHasMessageError("CEI_Procedure 21 with Additional Info code 00100", entryInstruction.CEI_ProcedureInfo, messageError);

			additionalInfo.CSI_SubType = "asd";
			entryInstruction.Validation.ValidateCEI_Procedure();
			AssertNoMessageError("Additional Info type is not INF", entryInstruction.CEI_ProcedureInfo, messageError);

			additionalInfo.CSI_SubType = AdditionalInfoTypes.INF;
			additionalInfo.CSI_Code = AdditionalInfoCodes._4PL12;
			entryInstruction.Validation.ValidateCEI_Procedure();
			AssertNoMessageError("CEI_Procedure 31 with Additional Info code 00100", entryInstruction.CEI_ProcedureInfo, messageError);

			additionalInfo.CSI_Code = AdditionalInfoCodes._00100;
			supportingDocument.CSI_Code = SupportingDocumentCodes.C513;
			entryInstruction.Validation.ValidateCEI_Procedure();
			AssertHasMessageError("Supporting Document C513", entryInstruction.CEI_ProcedureInfo, messageError);

			supportingDocument.CSI_Code = SupportingDocumentCodes.C710;
			entryInstruction.Validation.ValidateCEI_Procedure();
			AssertNoMessageError("Supporting Document C710", entryInstruction.CEI_ProcedureInfo, messageError);

			supportingDocument.CSI_Code = SupportingDocumentCodes._4DK3;
			entryInstruction.Validation.ValidateCEI_Procedure();
			AssertNoMessageError("Supporting Document _4DK3", entryInstruction.CEI_ProcedureInfo, messageError);
		});
	}

	public void TestCheckRuleR0224()
	{
		const string messageError = "(R0224) The sum of Gross Weight < sum of Net Weight (Customs Quantity)";
		var declaration = Factory.New<JobDeclaration>();
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		var invoice = declaration.Invoices.AddNew();
		var invoiceLine1 = invoice.InvoiceLines.AddNew();
		var invoiceLine2 = invoice.InvoiceLines.AddNew();

		CombineAssertions(() =>
		{
			AssertNoRowMessageError("Empty Declaration", entryInstruction, messageError);

			invoiceLine1.JI_CustomsQuantity = 10m;
			invoiceLine1.JI_CustomsUnitQty = "KGM";
			invoiceLine1.JI_Weight = 9m;
			entryInstruction.Validation.ValidateAll();
			AssertHasRowMessageError("JI_Weight no UQ and less than JI_CustomsQuantity", entryInstruction, messageError);

			invoiceLine1.JI_WeightUQ = "KG";
			entryInstruction.Validation.ValidateAll();
			AssertHasRowMessageError("JI_Weight less than JI_CustomsQuantity", entryInstruction, messageError);

			invoiceLine1.JI_Weight = 10m;
			entryInstruction.Validation.ValidateAll();
			AssertNoRowMessageError("JI_Weight equal JI_CustomsQuantity", entryInstruction, messageError);

			invoiceLine1.JI_Weight = 11m;
			entryInstruction.Validation.ValidateAll();
			AssertNoRowMessageError("JI_Weight greater than JI_CustomsQuantity", entryInstruction, messageError);

			invoiceLine1.JI_CustomsQuantity = 10m;
			invoiceLine1.JI_CustomsUnitQty = "KGM";
			invoiceLine2.JI_CustomsQuantity = 30m;
			invoiceLine2.JI_CustomsUnitQty = "KGM";
			invoiceLine1.JI_Weight = 0m;
			invoiceLine1.JI_WeightUQ = "KG";
			entryInstruction.Validation.ValidateAll();
			AssertHasRowMessageError("Total JI_Weight less than Total JI_CustomsQuantity", entryInstruction, messageError);

			invoiceLine1.JI_Weight = 39m;
			entryInstruction.Validation.ValidateAll();
			AssertHasRowMessageError("Total JI_Weight less than Total JI_CustomsQuantity", entryInstruction, messageError);

			invoiceLine1.JI_Weight = 40m;
			entryInstruction.Validation.ValidateAll();
			AssertNoRowMessageError("Total JI_Weight equal to Total JI_CustomsQuantity", entryInstruction, messageError);

			invoiceLine1.JI_Weight = 41m;
			entryInstruction.Validation.ValidateAll();
			AssertNoRowMessageError("Total JI_Weight greater than Total JI_CustomsQuantity", entryInstruction, messageError);
		});
	}

	public void TestCheckAllowedSubStyleCodes_C_F()
		=> AssertCheckAllowedSubStyleCodes(["C", "F"], "For simplified declaration with Sub Style (Additional Declaration Type) 'C' or 'F' can exist only Entries with the “Sub Style” codes: 'C' or 'F'.");

	public void TestCheckAllowedSubStyleCodes_B_E()
		=> AssertCheckAllowedSubStyleCodes(["B", "E"], "For simplified declaration with Sub Style (Additional Declaration Type) 'B' or 'E' can exist only Entries with the “Sub Style” codes: 'B' or 'E'.");

	public void TestCheckAllowedSubStyleCodes_X()
		=> AssertCheckAllowedSubStyleCodes(["X"], "For Supplementary Declaration with Sub Style (Additional Declaration Type) 'X' can be used only Entries with the “Sub Style” codes: 'X'.");

	public void TestCheckAllowedSubStyleCodes_Y()
		=> AssertCheckAllowedSubStyleCodes(["Y"], "For Supplementary Declaration with Sub Style (Additional Declaration Type) 'Y' can be used only Entries with the “Sub Style” codes: 'Y'.");

	void AssertCheckAllowedSubStyleCodes(string[] allowedIfPresentCodes, string messageError) => CombineAssertions(() =>
	{
		var declaration = Factory.New<JobDeclaration>();

		var allLetters = Enumerable.Range('A', 26).Select(x => ((char)x).ToString()).ToArray();
		var deniedValues = allLetters.Except(allowedIfPresentCodes).ToArray();

		var entryInstruction1 = declaration.CustomsEntryInstructions.AddNew();
		var entryInstruction2 = declaration.CustomsEntryInstructions.AddNew();
		AssertNoError("Empty CEI_SubStyle 1", entryInstruction1.CEI_SubStyleInfo, messageError);
		AssertNoError("Empty CEI_SubStyle 2", entryInstruction2.CEI_SubStyleInfo, messageError);

		entryInstruction1.CEI_SubStyle = "A";
		entryInstruction2.Validation.ValidateCEI_SubStyle();
		AssertNoError("A and empty 1", entryInstruction1.CEI_SubStyleInfo, messageError);
		AssertNoError("A and empty 2", entryInstruction2.CEI_SubStyleInfo, messageError);

		foreach (var simplifiedValue in allowedIfPresentCodes)
		{
			entryInstruction1.CEI_SubStyle = simplifiedValue;
			foreach (var deniedValue in deniedValues)
			{
				entryInstruction2.CEI_SubStyle = deniedValue;
				entryInstruction1.Validation.ValidateCEI_SubStyle();
				AssertNoError($"{simplifiedValue} and {deniedValue} 1", entryInstruction1.CEI_SubStyleInfo, messageError);
				AssertHasError($"{simplifiedValue} and {deniedValue} 2", entryInstruction2.CEI_SubStyleInfo, messageError);
			}
			foreach (var nonDeniedValue in allowedIfPresentCodes)
			{
				entryInstruction2.CEI_SubStyle = nonDeniedValue;
				entryInstruction1.Validation.ValidateCEI_SubStyle();
				AssertNoError($"{simplifiedValue} and {nonDeniedValue} 1", entryInstruction1.CEI_SubStyleInfo, messageError);
				AssertNoError($"{simplifiedValue} and {nonDeniedValue} 2", entryInstruction2.CEI_SubStyleInfo, messageError);
			}
		}

		foreach (var notSimplifiedValue in deniedValues)
		{
			entryInstruction1.CEI_SubStyle = notSimplifiedValue;
			foreach (var deniedValue in deniedValues)
			{
				entryInstruction2.CEI_SubStyle = deniedValue;
				entryInstruction1.Validation.ValidateCEI_SubStyle();
				AssertNoError($"{notSimplifiedValue} and {deniedValue} 1", entryInstruction1.CEI_SubStyleInfo, messageError);
				AssertNoError($"{notSimplifiedValue} and {deniedValue} 2", entryInstruction2.CEI_SubStyleInfo, messageError);
			}
		}
	});
}
