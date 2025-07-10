using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.PL.Business.Testing;
using static Enterprise.Customs.PL.Business.Constants;
using RefCusRateCodes = Enterprise.Customs.EU.Business.UniversalReferenceConstants.RefCusRateCodes;

namespace Enterprise.Customs.PL.Business.Declaration.Testing;

sealed class ImportCusEntryLineFeeValidationTest : CusEntryLineFeeValidationTest
{
	public void TestCheckRuleR1009()
	{
		const string messageError = "(R1009) Payment method 'J' is allowed only for standard declaration (Sub Style = 'A').";
		var (declaration, _, fee) = SetEntryLineFeeData();
		var instruction = declaration.CustomsEntryInstructions[0];

		CombineAssertions(() =>
		{
			instruction.CEI_SubStyle = "A";
			fee.CF_MethodOfPayment = "J";
			AssertNoMessageError("Sub Style = 'A', Payment method 'J'", fee.CF_MethodOfPaymentInfo, messageError);

			fee.CF_MethodOfPayment = "K";
			AssertNoMessageError("Sub Style = 'A', Payment method not 'J'", fee.CF_MethodOfPaymentInfo, messageError);

			instruction.CEI_SubStyle = "B";
			fee.CF_MethodOfPayment = "J";
			AssertHasMessageError("Sub Style != 'A', Payment method 'J'", fee.CF_MethodOfPaymentInfo, messageError);

			fee.CF_MethodOfPayment = "K";
			AssertNoMessageError("Sub Style != 'A', Payment method not 'J'", fee.CF_MethodOfPaymentInfo, messageError);
		});
	}

	public void TestCheckRuleR462()
	{
		var errorMessage = "(R462) - Method of Payment code G is allowed for B00 (VAT) charge type only.";
		var (_, _, fee) = SetEntryLineFeeData();
		var propertyInfo = fee.CF_MethodOfPaymentInfo;
		var validation = fee.Validation;
		CombineAssertions(() =>
		{
			fee.CF_ChargeType = RefCusRateCodes.CustomsDutyOnIndustrialProducts;
			validation.ValidateCF_MethodOfPayment();
			AssertNoMessageError("No error message - payment empty", propertyInfo, errorMessage);

			fee.CF_MethodOfPayment = PLMethodOfPaymentList.Codes.G;
			validation.ValidateCF_MethodOfPayment();
			AssertHasMessageError("Error message - CF_ChargeType != 'B00'", propertyInfo, errorMessage);

			fee.CF_ChargeType = RefCusRateCodes.Vat;
			validation.ValidateCF_MethodOfPayment();
			AssertNoMessageError("No error message - CF_ChargeType == 'B00'", propertyInfo, errorMessage);
		});
	}

	public void TestCheckRuleR1002()
	{
		var errorMessage = "(R1002) - Method of Payment code D or L or Z is required for the charge type A35/A45.";
		var (_, _, fee) = SetEntryLineFeeData();
		var propertyInfo = fee.CF_MethodOfPaymentInfo;
		var validation = fee.Validation;
		CombineAssertions(() =>
		{
			fee.CF_ChargeType = RefCusRateCodes.ProvisionalAntiDumpingDuty;
			validation.ValidateCF_MethodOfPayment();
			AssertNoMessageError("No error message - payment empty", propertyInfo, errorMessage);

			fee.CF_MethodOfPayment = PLMethodOfPaymentList.Codes.G;
			validation.ValidateCF_MethodOfPayment();
			AssertHasMessageError("Error message - wrong payment method for A35", propertyInfo, errorMessage);

			fee.CF_ChargeType = RefCusRateCodes.ProvisionalCountervailingDuty;
			validation.ValidateCF_MethodOfPayment();
			AssertHasMessageError("Error message - wrong payment method for A45", propertyInfo, errorMessage);

			fee.CF_ChargeType = RefCusRateCodes.ProvisionalAntiDumpingDuty;
			fee.CF_MethodOfPayment = PLMethodOfPaymentList.Codes.D;
			validation.ValidateCF_MethodOfPayment();
			AssertNoMessageError("No error message - A35 with D payment", propertyInfo, errorMessage);

			fee.CF_ChargeType = RefCusRateCodes.ProvisionalCountervailingDuty;
			fee.CF_MethodOfPayment = PLMethodOfPaymentList.Codes.L;
			validation.ValidateCF_MethodOfPayment();
			AssertNoMessageError("No error message - A45 with L payment", propertyInfo, errorMessage);

			fee.CF_MethodOfPayment = PLMethodOfPaymentList.Codes.Z;
			validation.ValidateCF_MethodOfPayment();
			AssertNoMessageError("No error message - A45 with Z payment", propertyInfo, errorMessage);
		});
	}

	public void TestCheckRuleR1004()
	{
		var errorMessage = "(R1004) - Method of Payment code J is required for all charges for the Entry.";
		var (_, entryLine, fee1) = SetEntryLineFeeData();
		CombineAssertions(() =>
		{
			fee1.CF_MethodOfPayment = PLMethodOfPaymentList.Codes.J;
			fee1.Validation.ValidateCF_MethodOfPayment();
			AssertNoMessageError("Only one other than J method of payment", fee1.CF_MethodOfPaymentInfo, errorMessage);

			var fee2 = entryLine.Fees.AddNew();
			fee2.CF_MethodOfPayment = PLMethodOfPaymentList.Codes.J;
			fee1.Validation.ValidateCF_MethodOfPayment();
			fee2.Validation.ValidateCF_MethodOfPayment();
			AssertNoMessageError("Two fees with J method of payment - 1st fee", fee1.CF_MethodOfPaymentInfo, errorMessage);
			AssertNoMessageError("Two fees with J method of payment - 2nd fee", fee2.CF_MethodOfPaymentInfo, errorMessage);

			fee2.CF_MethodOfPayment = PLMethodOfPaymentList.Codes.A;
			fee1.Validation.ValidateCF_MethodOfPayment();
			fee2.Validation.ValidateCF_MethodOfPayment();
			AssertHasMessageError("Two fees, one with J method of payment, other with different one - 1st fee", fee1.CF_MethodOfPaymentInfo, errorMessage);
			AssertHasMessageError("Two fees, one with J method of payment, other with different one - 1nd fee", fee2.CF_MethodOfPaymentInfo, errorMessage);

			fee1.CF_MethodOfPayment = PLMethodOfPaymentList.Codes.D;
			fee1.Validation.ValidateCF_MethodOfPayment();
			fee2.Validation.ValidateCF_MethodOfPayment();
			AssertNoMessageError("Two fees with other than J method of payment - 1st fee", fee1.CF_MethodOfPaymentInfo, errorMessage);
			AssertNoMessageError("Two fees with other than J method of payment - 2nd fee", fee2.CF_MethodOfPaymentInfo, errorMessage);
		});
	}

	public void TestCheckRuleR1010()
	{
		var errorMessagePrefix = "(R1010)";
		var (_, _, fee) = SetEntryLineFeeData();
		var propertyInfo = fee.CF_MethodOfPaymentInfo;
		var validation = fee.Validation;
		CombineAssertions(() =>
		{
			fee.CF_ChargeAmount = 0;
			validation.ValidateCF_MethodOfPayment();
			AssertNoMessageError("No error message - payment empty", propertyInfo, errorMessagePrefix);

			fee.CF_MethodOfPayment = PLMethodOfPaymentList.Codes.R;
			validation.ValidateCF_MethodOfPayment();
			AssertHasMessageErrorContaining("Error message - payment R", propertyInfo, errorMessagePrefix);

			fee.CF_MethodOfPayment = PLMethodOfPaymentList.Codes.E;
			validation.ValidateCF_MethodOfPayment();
			AssertHasMessageErrorContaining("Error message - payment E", propertyInfo, errorMessagePrefix);

			fee.CF_MethodOfPayment = PLMethodOfPaymentList.Codes.L;
			validation.ValidateCF_MethodOfPayment();
			AssertNoMessageError("No error message - payment L", propertyInfo, errorMessagePrefix);

			fee.CF_MethodOfPayment = PLMethodOfPaymentList.Codes.R;
			fee.CF_ChargeAmount = 10;
			validation.ValidateCF_MethodOfPayment();
			AssertNoMessageError("No error message - payment R, but amount > 0", propertyInfo, errorMessagePrefix);
		});
	}

	public void TestCheckRuleR1017()
	{
		var errorMessage = "(R1017) - Invalid method of payment for the charge type.";
		var (declaration, entryLine, fee) = SetEntryLineFeeData();
		var instruction = declaration.CustomsEntryInstructions[0];
		var propertyInfo = fee.CF_MethodOfPaymentInfo;
		var validation = fee.Validation;
		CombineAssertions(() =>
		{
			instruction.CEI_Procedure = Constants.ProcedureCodes._40;
			fee.CF_ChargeType = RefCusRateCodes.Vat;
			validation.ValidateCF_MethodOfPayment();
			AssertNoMessageError("No error message - payment empty", propertyInfo, errorMessage);

			fee.CF_MethodOfPayment = PLMethodOfPaymentList.Codes.D;
			validation.ValidateCF_MethodOfPayment();
			AssertHasMessageError("Error message", propertyInfo, errorMessage);

			instruction.CEI_Procedure = Constants.ProcedureCodes._71;
			validation.ValidateCF_MethodOfPayment();
			AssertNoMessageError("No error message - cei procedure not on the list", propertyInfo, errorMessage);

			instruction.CEI_Procedure = Constants.ProcedureCodes._40;
			fee.CF_MethodOfPayment = PLMethodOfPaymentList.Codes.A;
			validation.ValidateCF_MethodOfPayment();
			AssertNoMessageError("No error message - payment not D", propertyInfo, errorMessage);

			fee.CF_MethodOfPayment = PLMethodOfPaymentList.Codes.D;
			fee.CF_ChargeType = TaxTypeList.Codes.Additional1P1TaxDuties;
			validation.ValidateCF_MethodOfPayment();
			AssertNoMessageError("No error message - charge type on the list", propertyInfo, errorMessage);
		});
	}

	public void TestCheckRuleR1530()
	{
		var errorMessage = "(R1530) - Invalid method of payment.  It’s allowed to use only A,D,E G,H,J,L,R,Z.";
		var (_, _, fee) = SetEntryLineFeeData();
		var propertyInfo = fee.CF_MethodOfPaymentInfo;
		var validation = fee.Validation;
		CombineAssertions(() =>
		{
			validation.ValidateCF_MethodOfPayment();
			AssertNoMessageError("No error message - payment empty", propertyInfo, errorMessage);

			fee.CF_MethodOfPayment = PLMethodOfPaymentList.Codes.R;
			AssertNoMessageError("No error message - payment from list", propertyInfo, errorMessage);

			fee.CF_MethodOfPayment = PLMethodOfPaymentList.Codes.U;
			AssertHasMessageError("Error message", propertyInfo, errorMessage);

			fee.CF_MethodOfPayment = PLMethodOfPaymentList.Codes.E;
			AssertNoMessageError("Error message", propertyInfo, errorMessage);
		});
	}

	public void TestCheckRuleR1538()
	{
		var errorMessage = "(R1538) - Method of Payment code L is required for 1A1 charge type.";
		var (declaration, entryLine, fee) = SetEntryLineFeeData();
		var propertyInfo = fee.CF_MethodOfPaymentInfo;
		var validation = fee.Validation;
		CombineAssertions(() =>
		{
			var addInfo = invoiceLine.AdditionalInfos.AddNew();
			addInfo.CSI_Code = Constants.AdditionalInfoCodes._4PL10;
			fee.CF_ChargeType = TaxTypeList.Codes.ExciseTax;
			validation.ValidateCF_MethodOfPayment();
			AssertNoMessageError("No error message - payment empty", propertyInfo, errorMessage);

			fee.CF_MethodOfPayment = PLMethodOfPaymentList.Codes.R;
			validation.ValidateCF_MethodOfPayment();
			AssertHasMessageError("Error message - add info for invoice line", propertyInfo, errorMessage);

			invoiceLine.AdditionalInfos.RemoveAll();
			addInfo = invoiceLine.InvoiceHeader.AdditionalInfos.AddNew();
			addInfo.CSI_Code = Constants.AdditionalInfoCodes._4PL10;
			validation.ValidateCF_MethodOfPayment();
			AssertHasMessageError("Error message - add info for invoice", propertyInfo, errorMessage);

			invoiceLine.InvoiceHeader.AdditionalInfos.RemoveAll();
			addInfo = declaration.AdditionalInfos.AddNew();
			addInfo.CSI_Code = Constants.AdditionalInfoCodes._4PL10;
			validation.ValidateCF_MethodOfPayment();
			AssertHasMessageError("Error message - add info for declaration", propertyInfo, errorMessage);

			fee.CF_MethodOfPayment = PLMethodOfPaymentList.Codes.L;
			validation.ValidateCF_MethodOfPayment();
			AssertNoMessageError("No error message - method of payment is L", propertyInfo, errorMessage);

			fee.CF_MethodOfPayment = PLMethodOfPaymentList.Codes.R;
			fee.CF_ChargeType = TaxTypeList.Codes.AdditionalDutiesSecurity;
			validation.ValidateCF_MethodOfPayment();
			AssertNoMessageError("No error message - other charge type than 1A1", propertyInfo, errorMessage);

			fee.CF_ChargeType = TaxTypeList.Codes.ExciseTax;
			declaration.AdditionalInfos.RemoveAll();
			validation.ValidateCF_MethodOfPayment();
			AssertNoMessageError("No error message - no add info", propertyInfo, errorMessage);
		});
	}

	public void TestCheckRuleR245()
	{
		var errorMessage = "(R245) - Method of Payment code Z if required.";
		var (_, entryLine, fee) = SetEntryLineFeeData();
		var entryInstruction = entryLine.Header.EntryInstruction;
		var propertyInfo = fee.CF_MethodOfPaymentInfo;
		var validation = fee.Validation;

		CombineAssertions(() =>
		{
			fee.CF_ChargeType = RefCusRateCodes.CustomsDutyOnIndustrialProducts;
			fee.CF_MethodOfPayment = PLMethodOfPaymentList.Codes.Z;
			entryInstruction.CEI_Procedure = Constants.ProcedureCodes._40;
			var additionalProcedureCode = invoiceLine.AdditionalProcedureCodes.AddNew(Constants.ConcessionCodes.C02);
			validation.ValidateCF_MethodOfPayment();
			AssertNoMessageError("No error message - CF_MethodOfPayment == Z", propertyInfo, errorMessage);

			fee.CF_MethodOfPayment = PLMethodOfPaymentList.Codes.A;
			validation.ValidateCF_MethodOfPayment();
			AssertHasMessageError("Has error message", propertyInfo, errorMessage);

			fee.CF_ChargeType = RefCusRateCodes.Vat;
			validation.ValidateCF_MethodOfPayment();
			AssertNoMessageError("No error message - CF_ChargeType != A00", propertyInfo, errorMessage);

			fee.CF_ChargeType = RefCusRateCodes.CustomsDutyOnIndustrialProducts;
			entryInstruction.CEI_Procedure = Constants.ProcedureCodes._71;
			validation.ValidateCF_MethodOfPayment();
			AssertNoMessageError("No error message - CEI_Procedure start different than 4 and 6", propertyInfo, errorMessage);

			entryInstruction.CEI_Procedure = Constants.ProcedureCodes._40;
			additionalProcedureCode.CY_Code = Constants.ConcessionCodes.D51;
			validation.ValidateCF_MethodOfPayment();
			AssertNoMessageError("No error message - details code not from list", propertyInfo, errorMessage);
		});
	}

	public void TestCheckRuleR246()
	{
		var errorMessage = "(R246) - Method of Payment code Z if required.";
		var (_, entryLine, fee) = SetEntryLineFeeData();
		var entryInstruction = entryLine.Header.EntryInstruction;
		var propertyInfo = fee.CF_MethodOfPaymentInfo;
		var validation = fee.Validation;

		CombineAssertions(() =>
		{
			fee.CF_ChargeType = RefCusRateCodes.Vat;
			fee.CF_MethodOfPayment = PLMethodOfPaymentList.Codes.Z;
			entryInstruction.CEI_Procedure = Constants.ProcedureCodes._40;
			var additionalProcedureCode = invoiceLine.AdditionalProcedureCodes.AddNew(Constants.ConcessionCodes._3V0);
			validation.ValidateCF_MethodOfPayment();
			AssertNoMessageError("No error message - CF_MethodOfPayment == Z", propertyInfo, errorMessage);
			fee.CF_MethodOfPayment = PLMethodOfPaymentList.Codes.A;
			validation.ValidateCF_MethodOfPayment();
			AssertHasMessageError("Has error message", propertyInfo, errorMessage);
			fee.CF_ChargeType = RefCusRateCodes.CustomsDutyOnIndustrialProducts;
			validation.ValidateCF_MethodOfPayment();
			AssertNoMessageError("No error message - CF_ChargeType != B00", propertyInfo, errorMessage);
			fee.CF_ChargeType = RefCusRateCodes.Vat;
			entryInstruction.CEI_Procedure = Constants.ProcedureCodes._71;
			validation.ValidateCF_MethodOfPayment();
			AssertNoMessageError("No error message - CEI_Procedure start different than 4 and 6", propertyInfo, errorMessage);
			entryInstruction.CEI_Procedure = Constants.ProcedureCodes._40;
			additionalProcedureCode.CY_Code = Constants.ConcessionCodes.C02;
			validation.ValidateCF_MethodOfPayment();
			AssertNoMessageError("No error message - details code not from list", propertyInfo, errorMessage);

			entryInstruction.CEI_Procedure = Constants.ProcedureCodes._40;
			additionalProcedureCode.CY_Code = Constants.ConcessionCodes._0V7;
			validation.ValidateCF_MethodOfPayment();
			AssertNoMessageError("No error message - details code not from list", propertyInfo, errorMessage);

			entryInstruction.CEI_Procedure = Constants.ProcedureCodes._40;
			additionalProcedureCode.CY_Code = Constants.ConcessionCodes._4V6;
			validation.ValidateCF_MethodOfPayment();
			AssertNoMessageError("No error message - details code not from list", propertyInfo, errorMessage);

			entryInstruction.CEI_Procedure = Constants.ProcedureCodes._40;
			additionalProcedureCode.CY_Code = Constants.ConcessionCodes.F48;
			validation.ValidateCF_MethodOfPayment();
			AssertHasMessageError("Has error message", propertyInfo, errorMessage);

			entryInstruction.CEI_Procedure = Constants.ProcedureCodes._40;
			additionalProcedureCode.CY_Code = Constants.ConcessionCodes._4V7;
			validation.ValidateCF_MethodOfPayment();
			AssertHasMessageError("Has error message", propertyInfo, errorMessage);
		});
	}

	public void TestCheckRuleR247()
	{
		var errorMessage = "(R247) - Method of Payment code Z if required.";
		var (_, entryLine, fee) = SetEntryLineFeeData();
		var entryInstruction = entryLine.Header.EntryInstruction;
		var propertyInfo = fee.CF_MethodOfPaymentInfo;
		var validation = fee.Validation;

		CombineAssertions(() =>
		{
			fee.CF_ChargeType = TaxTypeList.Codes.ExciseTax;
			fee.CF_MethodOfPayment = PLMethodOfPaymentList.Codes.Z;
			entryInstruction.CEI_Procedure = Constants.ProcedureCodes._40;
			var additionalProcedureCode = invoiceLine.AdditionalProcedureCodes.AddNew(Constants.ConcessionCodes._8A2);
			validation.ValidateCF_MethodOfPayment();
			AssertNoMessageError("No error message - CF_MethodOfPayment == Z", propertyInfo, errorMessage);

			fee.CF_MethodOfPayment = PLMethodOfPaymentList.Codes.A;
			validation.ValidateCF_MethodOfPayment();
			AssertHasMessageError("Has error message", propertyInfo, errorMessage);

			fee.CF_ChargeType = RefCusRateCodes.CustomsDutyOnIndustrialProducts;
			validation.ValidateCF_MethodOfPayment();
			AssertNoMessageError("No error message - CF_ChargeType != 1A1", propertyInfo, errorMessage);

			fee.CF_ChargeType = TaxTypeList.Codes.ExciseTax;
			entryInstruction.CEI_Procedure = Constants.ProcedureCodes._71;
			validation.ValidateCF_MethodOfPayment();
			AssertNoMessageError("No error message - CEI_Procedure start different than 4 and 6", propertyInfo, errorMessage);

			entryInstruction.CEI_Procedure = Constants.ProcedureCodes._40;
			additionalProcedureCode.CY_Code = Constants.ConcessionCodes.C02;
			validation.ValidateCF_MethodOfPayment();
			AssertNoMessageError("No error message - details code not from list", propertyInfo, errorMessage);
		});
	}

	public void TestCheckRuleR250()
	{
		var errorMessage = "(R250) - Invalid Method of Payment code Z for CPCD.";
		var (_, entryLine, fee) = SetEntryLineFeeData();
		var entryInstruction = entryLine.Header.EntryInstruction;
		var propertyInfo = fee.CF_MethodOfPaymentInfo;
		var validation = fee.Validation;

		CombineAssertions(() =>
		{
			fee.CF_ChargeType = RefCusRateCodes.Vat;
			fee.CF_MethodOfPayment = PLMethodOfPaymentList.Codes.A;
			entryInstruction.CEI_Procedure = Constants.ProcedureCodes._40;
			validation.ValidateCF_MethodOfPayment();
			AssertNoMessageError("No error message - CF_MethodOfPayment != Z", propertyInfo, errorMessage);
			fee.CF_MethodOfPayment = PLMethodOfPaymentList.Codes.Z;
			validation.ValidateCF_MethodOfPayment();
			AssertHasMessageError("Has error message - empty", propertyInfo, errorMessage);
			var additionalProcedureCode = invoiceLine.AdditionalProcedureCodes.AddNew(Constants.ConcessionCodes.C02);
			validation.ValidateCF_MethodOfPayment();
			AssertHasMessageError("Has error message - not in the list", propertyInfo, errorMessage);
			fee.CF_ChargeType = RefCusRateCodes.CustomsDutyOnIndustrialProducts;
			validation.ValidateCF_MethodOfPayment();
			AssertNoMessageError("No error message - CF_ChargeType != B00", propertyInfo, errorMessage);
			fee.CF_ChargeType = RefCusRateCodes.Vat;
			entryInstruction.CEI_Procedure = Constants.ProcedureCodes._71;
			validation.ValidateCF_MethodOfPayment();
			AssertNoMessageError("No error message - CEI_Procedure start different than 4 and 6", propertyInfo, errorMessage);
			entryInstruction.CEI_Procedure = Constants.ProcedureCodes._40;
			additionalProcedureCode.CY_Code = Constants.ConcessionCodes._0V1;
			validation.ValidateCF_MethodOfPayment();
			AssertNoMessageError("No error message - details code from list", propertyInfo, errorMessage);

			entryInstruction.CEI_Procedure = Constants.ProcedureCodes._40;
			additionalProcedureCode.CY_Code = Constants.ConcessionCodes._0V7;
			validation.ValidateCF_MethodOfPayment();
			AssertHasMessageError("Has error message - not in the list", propertyInfo, errorMessage);

			entryInstruction.CEI_Procedure = Constants.ProcedureCodes._40;
			additionalProcedureCode.CY_Code = Constants.ConcessionCodes._4V6;
			validation.ValidateCF_MethodOfPayment();
			AssertHasMessageError("Has error message - not in the list", propertyInfo, errorMessage);

			entryInstruction.CEI_Procedure = Constants.ProcedureCodes._40;
			additionalProcedureCode.CY_Code = Constants.ConcessionCodes.F48;
			validation.ValidateCF_MethodOfPayment();
			AssertNoMessageError("No error message - details code from list", propertyInfo, errorMessage);

			entryInstruction.CEI_Procedure = Constants.ProcedureCodes._40;
			additionalProcedureCode.CY_Code = Constants.ConcessionCodes._4V7;
			validation.ValidateCF_MethodOfPayment();
			AssertNoMessageError("No error message - details code from list", propertyInfo, errorMessage);
		});
	}

	public void TestCheckRuleR251()
	{
		var errorMessage = "(R251) - Invalid Method of Payment code Z for CPCD.";
		var (_, entryLine, fee) = SetEntryLineFeeData();
		var entryInstruction = entryLine.Header.EntryInstruction;
		var propertyInfo = fee.CF_MethodOfPaymentInfo;
		var validation = fee.Validation;

		CombineAssertions(() =>
		{
			fee.CF_ChargeType = RefCusRateCodes.CustomsDutyOnIndustrialProducts;
			fee.CF_MethodOfPayment = PLMethodOfPaymentList.Codes.A;
			entryInstruction.CEI_Procedure = Constants.ProcedureCodes._40;
			validation.ValidateCF_MethodOfPayment();
			AssertNoMessageError("No error message - CF_MethodOfPayment != Z", propertyInfo, errorMessage);

			fee.CF_MethodOfPayment = PLMethodOfPaymentList.Codes.Z;
			validation.ValidateCF_MethodOfPayment();
			AssertHasMessageError("Has error message - empty", propertyInfo, errorMessage);

			var additionalProcedureCode = invoiceLine.AdditionalProcedureCodes.AddNew(Constants.ConcessionCodes._0V1);
			validation.ValidateCF_MethodOfPayment();
			AssertHasMessageError("Has error message - not in the list", propertyInfo, errorMessage);

			fee.CF_ChargeType = RefCusRateCodes.Vat;
			validation.ValidateCF_MethodOfPayment();
			AssertNoMessageError("No error message - CF_ChargeType != A00", propertyInfo, errorMessage);

			fee.CF_ChargeType = RefCusRateCodes.CustomsDutyOnIndustrialProducts;
			entryInstruction.CEI_Procedure = Constants.ProcedureCodes._71;
			validation.ValidateCF_MethodOfPayment();
			AssertNoMessageError("No error message - CEI_Procedure start different than 4 and 6", propertyInfo, errorMessage);

			entryInstruction.CEI_Procedure = Constants.ProcedureCodes._40;
			additionalProcedureCode.CY_Code = Constants.ConcessionCodes.C02;
			validation.ValidateCF_MethodOfPayment();
			AssertNoMessageError("No error message - details code from list", propertyInfo, errorMessage);
		});
	}

	public void TestCheckRuleR252()
	{
		var errorMessage = "(R252) - Invalid Method of Payment code Z for CPCD.";
		var (_, entryLine, fee) = SetEntryLineFeeData();
		var entryInstruction = entryLine.Header.EntryInstruction;
		var propertyInfo = fee.CF_MethodOfPaymentInfo;
		var validation = fee.Validation;

		CombineAssertions(() =>
		{
			fee.CF_ChargeType = TaxTypeList.Codes.ExciseTax;
			fee.CF_MethodOfPayment = PLMethodOfPaymentList.Codes.A;
			entryInstruction.CEI_Procedure = Constants.ProcedureCodes._40;
			validation.ValidateCF_MethodOfPayment();
			AssertNoMessageError("No error message - CF_MethodOfPayment != Z", propertyInfo, errorMessage);

			fee.CF_MethodOfPayment = PLMethodOfPaymentList.Codes.Z;
			validation.ValidateCF_MethodOfPayment();
			AssertHasMessageError("Has error message - empty", propertyInfo, errorMessage);

			var additionalProcedureCode = invoiceLine.AdditionalProcedureCodes.AddNew(Constants.ConcessionCodes._0V1);
			validation.ValidateCF_MethodOfPayment();
			AssertHasMessageError("Has error message - not in the list", propertyInfo, errorMessage);

			fee.CF_ChargeType = RefCusRateCodes.Vat;
			validation.ValidateCF_MethodOfPayment();
			AssertNoMessageError("No error message - CF_ChargeType != 1A1", propertyInfo, errorMessage);

			fee.CF_ChargeType = TaxTypeList.Codes.ExciseTax;
			entryInstruction.CEI_Procedure = Constants.ProcedureCodes._71;
			validation.ValidateCF_MethodOfPayment();
			AssertNoMessageError("No error message - CEI_Procedure start different than 4 and 6", propertyInfo, errorMessage);

			entryInstruction.CEI_Procedure = Constants.ProcedureCodes._40;
			additionalProcedureCode.CY_Code = Constants.ConcessionCodes._8A1;
			validation.ValidateCF_MethodOfPayment();
			AssertNoMessageError("No error message - details code from list", propertyInfo, errorMessage);
		});
	}

	public void TestCheckRuleR296()
	{
		const string messageError = "(R296) – Method of Payment code L is required for charge type 1A1.";
		var (declaration, entryLine, fee) = SetEntryLineFeeData();
		var entryInstruction = invoiceLine.EntryInstruction;

		CombineAssertions(() =>
		{
			fee.Validation.ValidateCF_MethodOfPayment();
			AssertNoMessageErrorContaining("Empty fee", fee.G4_MethodOfPaymentInfo, messageError);

			fee.Validation.ValidateCF_MethodOfPayment();
			fee.CF_ChargeType = TaxTypeList.Codes.ExciseTax;
			fee.CF_MethodOfPayment = PLMethodOfPaymentList.Codes.A;
			AssertNoMessageErrorContaining("1A1 fee with Payment A without Line Procedure", fee.G4_MethodOfPaymentInfo, messageError);

			entryInstruction.CEI_Procedure = ProcedureCodes._45;
			fee.Validation.ValidateCF_MethodOfPayment();
			AssertHasMessageErrorContaining("1A1 fee with Payment A with Line Procedure 45 without Concession", fee.G4_MethodOfPaymentInfo, messageError);

			entryInstruction.CEI_Procedure = ProcedureCodes._68;
			fee.Validation.ValidateCF_MethodOfPayment();
			AssertHasMessageErrorContaining("1A1 fee with Payment A with Line Procedure 68 without Concession", fee.G4_MethodOfPaymentInfo, messageError);

			entryInstruction.CEI_Procedure = ProcedureCodes._96;
			fee.Validation.ValidateCF_MethodOfPayment();
			AssertHasMessageErrorContaining("1A1 fee with Payment A with Line Procedure 96 without Concession", fee.G4_MethodOfPaymentInfo, messageError);

			entryInstruction.CEI_Procedure = ProcedureCodes._45;
			var concession = invoiceLine.AdditionalProcedureCodes.AddNew();
			concession.CY_Code = ConcessionCodes.F06;
			fee.Validation.ValidateCF_MethodOfPayment();
			AssertHasMessageErrorContaining("1A1 fee with Payment A with Line Procedure 45 with Concession F06", fee.G4_MethodOfPaymentInfo, messageError);

			entryInstruction.CEI_Procedure = ProcedureCodes._68;
			fee.Validation.ValidateCF_MethodOfPayment();
			AssertHasMessageErrorContaining("1A1 fee with Payment A with Line Procedure 68 with Concession F06", fee.G4_MethodOfPaymentInfo, messageError);

			concession.CY_Code = "AAA";
			fee.Validation.ValidateCF_MethodOfPayment();
			AssertHasMessageErrorContaining("1A1 fee with Payment A with Line Procedure 68 with Concession AAA", fee.G4_MethodOfPaymentInfo, messageError);

			concession.CY_Code = ConcessionCodes.F06;
			fee.CF_MethodOfPayment = PLMethodOfPaymentList.Codes.L;
			AssertNoMessageErrorContaining("1A1 fee with Payment L with Line Procedure 68 with Concession F06", fee.G4_MethodOfPaymentInfo, messageError);

			fee.CF_ChargeType = "AAA";
			fee.CF_MethodOfPayment = PLMethodOfPaymentList.Codes.A;
			AssertNoMessageErrorContaining("AAA fee with Payment A with Line Procedure 68 with Concession F06", fee.G4_MethodOfPaymentInfo, messageError);

			fee.CF_ChargeType = TaxTypeList.Codes.ExciseTax;
			entryInstruction.CEI_Procedure = "11";
			fee.Validation.ValidateCF_MethodOfPayment();
			AssertHasMessageErrorContaining("1A1 fee with Payment A with Line Procedure 11 with Concession F06", fee.G4_MethodOfPaymentInfo, messageError);
		});
	}

	public void TestCheckRuleR277()
	{
		const string messageError = "(R277) For the selected customs procedure code MoP=’D’ must be mixed with other Method of Payment code.";
		var (declaration, entryLine, fee) = SetEntryLineFeeData();
		var entryInstruction = invoiceLine.EntryInstruction;
		var secFee = entryLine.Fees.AddNew();
		var listOfR277ProcedureCodes = new ZString[] { ProcedureCodes._40, ProcedureCodes._42, ProcedureCodes._45
			, ProcedureCodes._61, ProcedureCodes._63, ProcedureCodes._68 };

		CombineAssertions(() =>
		{
			fee.Validation.ValidateCF_MethodOfPayment();
			AssertNoMessageErrorContaining("Empty fee", fee.CF_MethodOfPaymentInfo, messageError);

			fee.CF_MethodOfPayment = PLMethodOfPaymentList.Codes.D;
			entryInstruction.CEI_Procedure = ProcedureCodes._53;
			var concession = invoiceLine.AdditionalProcedureCodes.AddNew();
			concession.CY_Code = ConcessionCodes.D51;
			fee.Validation.ValidateCF_MethodOfPayment();
			AssertHasMessageErrorContaining("Procedure 53 with Concession D51 without other method of payment D", fee.CF_MethodOfPaymentInfo, messageError);

			concession.CY_Code = ZString.Empty;
			fee.Validation.ValidateCF_MethodOfPayment();
			AssertNoMessageErrorContaining("Procedure 53 without Concession D51 without other method of payment D", fee.CF_MethodOfPaymentInfo, messageError);

			concession.CY_Code = ConcessionCodes.D51;
			fee.CF_MethodOfPayment = PLMethodOfPaymentList.Codes.A;
			AssertNoMessageErrorContaining("CF_MethodOfPayment D does not exist (1st fee)", fee.CF_MethodOfPaymentInfo, messageError);

			secFee.CF_MethodOfPayment = PLMethodOfPaymentList.Codes.A;
			fee.CF_MethodOfPayment = PLMethodOfPaymentList.Codes.D;
			AssertNoMessageErrorContaining("CF_MethodOfPayment A exists (2nd fee)", fee.CF_MethodOfPaymentInfo, messageError);

			concession.CY_Code = ZString.Empty;
			foreach (var procedureCode in listOfR277ProcedureCodes)
			{
				entryInstruction.CEI_Procedure = procedureCode;
				secFee.CF_MethodOfPayment = PLMethodOfPaymentList.Codes.D;
				fee.Validation.ValidateCF_MethodOfPayment();
				AssertHasMessageErrorContaining($"CF_MethodOfPayment other than D does not exist procedure code {procedureCode}", fee.CF_MethodOfPaymentInfo, messageError);

				secFee.CF_MethodOfPayment = PLMethodOfPaymentList.Codes.A;
				fee.Validation.ValidateCF_MethodOfPayment();
				AssertNoMessageErrorContaining($"CF_MethodOfPayment other than D exists (2nd fee) procedure code {procedureCode}", fee.CF_MethodOfPaymentInfo, messageError);
			}
		});
	}

	public void TestCheckRuleR257()
	{
		const string messageError = "(R257.2) – For the Method of Payment code G (art.33a VAT) the fiscal role indicator FR7 is required.";
		var (declaration, entryLine, fee) = SetEntryLineFeeData();

		CombineAssertions(() =>
		{
			void RunCaseDifferentAssertions(string fiscalRoleMessage, bool fiscalRolePresent)
			{
				void RunSingleAssertion()
				{
					var message = $"{fee.CF_ChargeType} fee with Payment {fee.CF_MethodOfPayment} without AdditionalInfos, {fiscalRoleMessage}";
					fee.Validation.ValidateCF_MethodOfPayment();
					if (fee.CF_ChargeType == RefCusRateCodes.Vat && fee.CF_MethodOfPayment == PLMethodOfPaymentList.Codes.G && !fiscalRolePresent)
					{
						AssertHasMessageErrorContaining(message, fee.CF_MethodOfPaymentInfo, messageError);
					}
					else
					{
						AssertNoMessageErrorContaining(message, fee.CF_MethodOfPaymentInfo, messageError);
					}
				}

				fee.CF_ChargeType = RefCusRateCodes.Vat;
				fee.CF_MethodOfPayment = PLMethodOfPaymentList.Codes.G;
				RunSingleAssertion();

				fee.CF_ChargeType = "AAA";
				RunSingleAssertion();
				fee.CF_ChargeType = RefCusRateCodes.Vat;

				fee.CF_MethodOfPayment = PLMethodOfPaymentList.Codes.B;
				RunSingleAssertion();
			}

			RunCaseDifferentAssertions("no any FR7", false);

			var entryInstruction = declaration.CustomsEntryInstructions.Single();
			var entryInstructionFiscalRef = entryInstruction.FiscalReferences.AddNew();
			entryInstructionFiscalRef.CFR_Code = FiscalReferenceCodeList.Codes.FR7_Taxpayer;
			RunCaseDifferentAssertions("FR7 present in entryInstruction", true);
			entryInstructionFiscalRef.CFR_Code = FiscalReferenceCodeList.Codes.FR2_Customer;

			invoiceLine.FiscalReferences.AddNew().CFR_Code = FiscalReferenceCodeList.Codes.FR7_Taxpayer;
			RunCaseDifferentAssertions("FR7 present in invoiceLines", true);
		});
	}

	public void TestCheckRuleR827ToR830()
	{
		const string messageErrorPrefix = "(R827...R830)";
		var (declaration, entryLine, fee) = SetEntryLineFeeData();
		var entryInstruction = invoiceLine.EntryInstruction;
		var secFee = entryLine.Fees.AddNew();

		var plMethodOfPaymentList = new PLMethodOfPaymentList();
		var procedureCodeOfR827ToR830 = ProcedureCodes._51;
		var procedureCodeHalfConditionR827ToR830 = ProcedureCodes._53;
		var invalidMethodOfPayment = new string[] { PLMethodOfPaymentList.Codes.E, PLMethodOfPaymentList.Codes.G, PLMethodOfPaymentList.Codes.H, PLMethodOfPaymentList.Codes.J };
		var validMethodOfPayment = plMethodOfPaymentList.GetAllCodes().Except(invalidMethodOfPayment).ToList();
		var codes = Enumerable.Range(1, 30).Select(x => "D" + x.ToString().PadLeft(2, '0')).ToArray();

		CombineAssertions(() =>
		{
			fee.Validation.ValidateCF_MethodOfPayment();
			AssertNoMessageErrorContaining("Empty fee", fee.CF_MethodOfPaymentInfo, messageErrorPrefix);

			var concession = invoiceLine.AdditionalProcedureCodes.AddNew();
			entryInstruction.CEI_Procedure = procedureCodeOfR827ToR830;
			concession.CY_Code = ConcessionCodes.D51;
			fee.CF_MethodOfPayment = invalidMethodOfPayment[0];
			AssertHasMessageErrorContaining($"The method of payment {fee.CF_MethodOfPayment} is invalid", fee.CF_MethodOfPaymentInfo, messageErrorPrefix);

			entryInstruction.CEI_Procedure = procedureCodeHalfConditionR827ToR830;
			fee.Validation.ValidateCF_MethodOfPayment();
			AssertNoMessageErrorContaining($"MethodOfPayment {fee.CF_MethodOfPayment} doesn't exist procedure code {entryInstruction.CEI_Procedure}", fee.CF_MethodOfPaymentInfo, messageErrorPrefix);

			foreach (var code in codes)
			{
				concession.CY_Code = code;
				fee.Validation.ValidateCF_MethodOfPayment();
				AssertHasMessageErrorContaining($"Concession code {code}, The method of payment {fee.CF_MethodOfPayment} is invalid", fee.CF_MethodOfPaymentInfo, messageErrorPrefix);
			}

			fee.CF_MethodOfPayment = validMethodOfPayment[0];
			AssertNoMessageErrorContaining($"The method of payment {fee.CF_MethodOfPayment} is valid", fee.CF_MethodOfPaymentInfo, messageErrorPrefix);

			entryInstruction.CEI_Procedure = ProcedureCodes._42;
			fee.CF_MethodOfPayment = invalidMethodOfPayment[0];
			AssertNoMessageErrorContaining($"without Concession D51 or D53, the MethodOfPayment {fee.CF_MethodOfPayment} is valid", fee.CF_MethodOfPaymentInfo, messageErrorPrefix);
		});
	}

	public void TestCheckRuleR881_LRequired()
	{
		var errorMessage = "(R881.1) – Method of Payment code L is required with 4PL07 Additional Information code.";
		var (declaration, entryLine, fee) = SetEntryLineFeeData();
		var propertyInfo = fee.CF_MethodOfPaymentInfo;
		var validation = fee.Validation;
		var instruction = declaration.CustomsEntryInstructions[0];
		CombineAssertions(() =>
		{
			instruction.CEI_Procedure = Constants.ProcedureCodes._53;
			var addInfo = invoiceLine.AdditionalInfos.AddNew();
			addInfo.CSI_Code = Constants.AdditionalInfoCodes._4PL07;
			fee.CF_ChargeType = RefCusRateCodes.CustomsDutyOnIndustrialProducts;
			validation.ValidateCF_MethodOfPayment();
			AssertNoMessageError("No error message - payment empty", propertyInfo, errorMessage);

			fee.CF_MethodOfPayment = PLMethodOfPaymentList.Codes.G;
			validation.ValidateCF_MethodOfPayment();
			AssertHasMessageError("Error message - add info for invoice line", propertyInfo, errorMessage);

			invoiceLine.AdditionalInfos.RemoveAll();
			addInfo = invoiceLine.InvoiceHeader.AdditionalInfos.AddNew();
			addInfo.CSI_Code = Constants.AdditionalInfoCodes._4PL07;
			validation.ValidateCF_MethodOfPayment();
			AssertHasMessageError("Error message - add info for invoice", propertyInfo, errorMessage);

			invoiceLine.InvoiceHeader.AdditionalInfos.RemoveAll();
			addInfo = declaration.AdditionalInfos.AddNew();
			addInfo.CSI_Code = Constants.AdditionalInfoCodes._4PL07;
			validation.ValidateCF_MethodOfPayment();
			AssertHasMessageError("Error message - add info for declaration", propertyInfo, errorMessage);

			fee.CF_ChargeType = RefCusRateCodes.Vat;
			validation.ValidateCF_MethodOfPayment();
			AssertNoMessageError("No error message - CF_ChargeType == 'B00'", propertyInfo, errorMessage);

			fee.CF_ChargeType = RefCusRateCodes.CustomsDutyOnIndustrialProducts;
			fee.CF_MethodOfPayment = PLMethodOfPaymentList.Codes.L;
			validation.ValidateCF_MethodOfPayment();
			AssertNoMessageError("No error message - payment is L", propertyInfo, errorMessage);

			fee.CF_MethodOfPayment = PLMethodOfPaymentList.Codes.G;
			declaration.AdditionalInfos.RemoveAll();
			validation.ValidateCF_MethodOfPayment();
			AssertNoMessageError("No error message - no additional info", propertyInfo, errorMessage);

			addInfo = invoiceLine.AdditionalInfos.AddNew();
			addInfo.CSI_Code = Constants.AdditionalInfoCodes._4PL07;
			instruction.CEI_Procedure = Constants.ProcedureCodes._11;
			validation.ValidateCF_MethodOfPayment();
			AssertNoMessageError("No error message - other procedure code", propertyInfo, errorMessage);
		});
	}

	public void TestCheckRuleR881_LForbidden()
	{
		var errorMessage = "(R881.2) – Method of Payment code L is invalid without 4PL07 Additional Information code.";
		var (declaration, entryLine, fee) = SetEntryLineFeeData();
		var propertyInfo = fee.CF_MethodOfPaymentInfo;
		var validation = fee.Validation;
		var instruction = declaration.CustomsEntryInstructions[0];
		CombineAssertions(() =>
		{
			instruction.CEI_Procedure = Constants.ProcedureCodes._51;
			fee.CF_ChargeType = RefCusRateCodes.CustomsDutyOnIndustrialProducts;
			validation.ValidateCF_MethodOfPayment();
			AssertNoMessageError("No error message - payment empty", propertyInfo, errorMessage);

			fee.CF_MethodOfPayment = PLMethodOfPaymentList.Codes.L;
			validation.ValidateCF_MethodOfPayment();
			AssertHasMessageError("Error message", propertyInfo, errorMessage);

			fee.CF_ChargeType = RefCusRateCodes.Vat;
			validation.ValidateCF_MethodOfPayment();
			AssertNoMessageError("No error message - CF_ChargeType == 'B00'", propertyInfo, errorMessage);

			fee.CF_ChargeType = RefCusRateCodes.CustomsDutyOnIndustrialProducts;
			fee.CF_MethodOfPayment = PLMethodOfPaymentList.Codes.G;
			validation.ValidateCF_MethodOfPayment();
			AssertNoMessageError("No error message - payment is not L", propertyInfo, errorMessage);

			fee.CF_MethodOfPayment = PLMethodOfPaymentList.Codes.L;
			var addInfo = invoiceLine.AdditionalInfos.AddNew();
			addInfo.CSI_Code = Constants.AdditionalInfoCodes._4PL07;
			validation.ValidateCF_MethodOfPayment();
			AssertNoMessageError("No error message - additional info exists for invoice line", propertyInfo, errorMessage);

			invoiceLine.AdditionalInfos.RemoveAll();
			addInfo = invoiceLine.InvoiceHeader.AdditionalInfos.AddNew();
			addInfo.CSI_Code = Constants.AdditionalInfoCodes._4PL07;
			validation.ValidateCF_MethodOfPayment();
			AssertNoMessageError("No error message - additional info exists for invoice", propertyInfo, errorMessage);

			invoiceLine.InvoiceHeader.AdditionalInfos.RemoveAll();
			addInfo = declaration.AdditionalInfos.AddNew();
			addInfo.CSI_Code = Constants.AdditionalInfoCodes._4PL07;
			validation.ValidateCF_MethodOfPayment();
			AssertNoMessageError("No error message - additional info exists for declaration", propertyInfo, errorMessage);

			declaration.AdditionalInfos.RemoveAll();
			instruction.CEI_Procedure = Constants.ProcedureCodes._11;
			validation.ValidateCF_MethodOfPayment();
			AssertNoMessageError("No error message - other procedure code", propertyInfo, errorMessage);
		});
	}

	public void TestCheckRuleR881_LDRequired()
	{
		var errorMessage = "(R881.3) – Method of Payment code L or D is required without 4PL07 Additional Information code.";
		var (declaration, entryLine, fee) = SetEntryLineFeeData();
		var propertyInfo = fee.CF_MethodOfPaymentInfo;
		var validation = fee.Validation;
		var instruction = declaration.CustomsEntryInstructions[0];
		CombineAssertions(() =>
		{
			instruction.CEI_Procedure = Constants.ProcedureCodes._51;
			fee.CF_ChargeType = RefCusRateCodes.Vat;
			validation.ValidateCF_MethodOfPayment();
			AssertNoMessageError("No error message - payment empty", propertyInfo, errorMessage);

			fee.CF_MethodOfPayment = PLMethodOfPaymentList.Codes.A;
			validation.ValidateCF_MethodOfPayment();
			AssertHasMessageError("Error message", propertyInfo, errorMessage);

			fee.CF_ChargeType = RefCusRateCodes.CustomsDutyOnIndustrialProducts;
			validation.ValidateCF_MethodOfPayment();
			AssertNoMessageError("No error message - CF_ChargeType == 'A00'", propertyInfo, errorMessage);

			fee.CF_ChargeType = RefCusRateCodes.Vat;
			fee.CF_MethodOfPayment = PLMethodOfPaymentList.Codes.L;
			validation.ValidateCF_MethodOfPayment();
			AssertNoMessageError("No error message - payment is not {L,D}", propertyInfo, errorMessage);

			fee.CF_MethodOfPayment = PLMethodOfPaymentList.Codes.E;
			var addInfo = invoiceLine.AdditionalInfos.AddNew();
			addInfo.CSI_Code = Constants.AdditionalInfoCodes._4PL07;
			validation.ValidateCF_MethodOfPayment();
			AssertNoMessageError("No error message - additional info exists for invoice line", propertyInfo, errorMessage);

			invoiceLine.AdditionalInfos.RemoveAll();
			addInfo = invoiceLine.InvoiceHeader.AdditionalInfos.AddNew();
			addInfo.CSI_Code = Constants.AdditionalInfoCodes._4PL07;
			validation.ValidateCF_MethodOfPayment();
			AssertNoMessageError("No error message - additional info exists for invoice", propertyInfo, errorMessage);

			invoiceLine.InvoiceHeader.AdditionalInfos.RemoveAll();
			addInfo = declaration.AdditionalInfos.AddNew();
			addInfo.CSI_Code = Constants.AdditionalInfoCodes._4PL07;
			validation.ValidateCF_MethodOfPayment();
			AssertNoMessageError("No error message - additional info exists for declaration", propertyInfo, errorMessage);

			declaration.AdditionalInfos.RemoveAll();
			instruction.CEI_Procedure = Constants.ProcedureCodes._11;
			validation.ValidateCF_MethodOfPayment();
			AssertNoMessageError("No error message - other procedure code", propertyInfo, errorMessage);

			instruction.CEI_Procedure = Constants.ProcedureCodes._53;
			invoiceLine.AdditionalProcedureCodes.AddNew(Constants.ConcessionCodes.D51);
			validation.ValidateCF_MethodOfPayment();
			AssertNoMessageError("No error message - CPCD exists", propertyInfo, errorMessage);
		});
	}

	public void TestCheckRuleR884()
	{
		var errorMessage = "(R884) – Method of Payment code L is required.";
		var (declaration, entryLine, fee) = SetEntryLineFeeData();
		var propertyInfo = fee.CF_MethodOfPaymentInfo;
		var validation = fee.Validation;
		var instruction = declaration.CustomsEntryInstructions[0];
		CombineAssertions(() =>
		{
			invoiceLine.AdditionalProcedureCodes.AddNew(Constants.ConcessionCodes.D05);
			var addInfo = invoiceLine.AdditionalInfos.AddNew();
			addInfo.CSI_Code = Constants.AdditionalInfoCodes._4PL07;
			instruction.CEI_Procedure = Constants.ProcedureCodes._51;
			validation.ValidateCF_MethodOfPayment();
			AssertNoMessageError("No error message - payment empty", propertyInfo, errorMessage);

			fee.CF_MethodOfPayment = PLMethodOfPaymentList.Codes.A;
			validation.ValidateCF_MethodOfPayment();
			AssertHasMessageError("Error message - add info for invoice line", propertyInfo, errorMessage);

			invoiceLine.AdditionalInfos.RemoveAll();
			addInfo = invoiceLine.InvoiceHeader.AdditionalInfos.AddNew();
			addInfo.CSI_Code = Constants.AdditionalInfoCodes._4PL07;
			validation.ValidateCF_MethodOfPayment();
			AssertHasMessageError("Error message - add info for invoice", propertyInfo, errorMessage);

			invoiceLine.InvoiceHeader.AdditionalInfos.RemoveAll();
			addInfo = declaration.AdditionalInfos.AddNew();
			addInfo.CSI_Code = Constants.AdditionalInfoCodes._4PL07;
			validation.ValidateCF_MethodOfPayment();
			AssertHasMessageError("Error message - add info for declaration", propertyInfo, errorMessage);

			fee.CF_MethodOfPayment = PLMethodOfPaymentList.Codes.L;
			validation.ValidateCF_MethodOfPayment();
			AssertNoMessageError("No error message - payment is L", propertyInfo, errorMessage);

			fee.CF_MethodOfPayment = PLMethodOfPaymentList.Codes.A;
			declaration.AdditionalInfos.RemoveAll();
			validation.ValidateCF_MethodOfPayment();
			AssertNoMessageError("No error message - no add info", propertyInfo, errorMessage);

			addInfo = invoiceLine.AdditionalInfos.AddNew();
			addInfo.CSI_Code = Constants.AdditionalInfoCodes._4PL07;
			instruction.CEI_Procedure = Constants.ProcedureCodes._11;
			validation.ValidateCF_MethodOfPayment();
			AssertNoMessageError("No error message - other procedure code", propertyInfo, errorMessage);

			instruction.CEI_Procedure = Constants.ProcedureCodes._53;
			invoiceLine.AdditionalProcedureCodes.RemoveAll();
			validation.ValidateCF_MethodOfPayment();
			AssertNoMessageError("No error message - no CPCD", propertyInfo, errorMessage);
		});
	}

	public void TestCheckRuleR885()
	{
		var errorMessage = "(R885) If fiscal role code FR7 is specified then payment method G is required for fee type B00 (VAT).";
		var (declaration, _, fee) = SetEntryLineFeeData();
		var propertyInfo = fee.CF_MethodOfPaymentInfo;
		var validation = fee.Validation;

		CombineAssertions(() =>
		{
			fee.CF_MethodOfPayment = PLMethodOfPaymentList.Codes.E;
			fee.CF_ChargeType = RefCusRateCodes.Vat;
			validation.ValidateCF_MethodOfPayment();
			AssertNoMessageError("No error message - no fiscal references, MethodOfPayment E, fee type B00", propertyInfo, errorMessage);

			var entryInstruction = declaration.CustomsEntryInstructions.Single();
			var entryInstructionFiscalReference = entryInstruction.FiscalReferences.AddNew();
			entryInstructionFiscalReference.CFR_Code = FiscalReferenceCodeList.Codes.FR7_Taxpayer;
			Validate("in entryInstruction");
			void Validate(string placeOfFiscalReference)
			{
				validation.ValidateCF_MethodOfPayment();
				AssertHasMessageError($"Error message - has FR7 fiscal references {placeOfFiscalReference}, MethodOfPayment E, fee type B00", propertyInfo, errorMessage);

				fee.CF_MethodOfPayment = PLMethodOfPaymentList.Codes.G;
				validation.ValidateCF_MethodOfPayment();
				AssertNoMessageError($"Error message - has FR7 fiscal references {placeOfFiscalReference}, MethodOfPayment G, fee type B00", propertyInfo, errorMessage);

				fee.CF_MethodOfPayment = PLMethodOfPaymentList.Codes.E;
				validation.ValidateCF_MethodOfPayment();
				AssertHasMessageError($"Error message - has FR7 fiscal references {placeOfFiscalReference}, MethodOfPayment E, fee type B00", propertyInfo, errorMessage);

				fee.CF_ChargeType = RefCusRateCodes.ProvisionalAntiDumpingDuty;
				validation.ValidateCF_MethodOfPayment();
				AssertNoMessageError($"Error message - has FR7 fiscal references {placeOfFiscalReference}, MethodOfPayment G, fee type A35", propertyInfo, errorMessage);
			}

			fee.CF_MethodOfPayment = PLMethodOfPaymentList.Codes.E;
			fee.CF_ChargeType = RefCusRateCodes.Vat;
			entryInstructionFiscalReference.CFR_Code = FiscalReferenceCodeList.Codes.FR3_TaxRepresentative;
			validation.ValidateCF_MethodOfPayment();
			AssertNoMessageError("No error message - only FR3 fiscal reference in entryInstruction, MethodOfPayment E, fee type B00", propertyInfo, errorMessage);

			var invoiceLineFiscalReference = invoiceLine.FiscalReferences.AddNew();
			invoiceLineFiscalReference.CFR_Code = FiscalReferenceCodeList.Codes.FR7_Taxpayer;
			Validate("in invoiceLine");

			fee.CF_MethodOfPayment = PLMethodOfPaymentList.Codes.E;
			fee.CF_ChargeType = RefCusRateCodes.Vat;
			invoiceLineFiscalReference.CFR_Code = FiscalReferenceCodeList.Codes.FR3_TaxRepresentative;
			validation.ValidateCF_MethodOfPayment();
			AssertNoMessageError("No error message - only FR3 fiscal reference in invoiceLine, MethodOfPayment E, fee type B00", propertyInfo, errorMessage);
		});
	}

	public void TestCheckRuleR890()
	{
		var errorMessage = "(R890) – Method of Payment code L or D is required for A00 charge type.";
		var (declaration, entryLine, fee) = SetEntryLineFeeData();
		var propertyInfo = fee.CF_MethodOfPaymentInfo;
		var validation = fee.Validation;
		var instruction = declaration.CustomsEntryInstructions[0];
		CombineAssertions(() =>
		{
			invoiceLine.AdditionalProcedureCodes.AddNew(Constants.ConcessionCodes.D51);
			instruction.CEI_Procedure = Constants.ProcedureCodes._53;
			fee.CF_ChargeType = RefCusRateCodes.CustomsDutyOnIndustrialProducts;
			validation.ValidateCF_MethodOfPayment();
			AssertNoMessageError("No error message - payment empty", propertyInfo, errorMessage);

			fee.CF_MethodOfPayment = PLMethodOfPaymentList.Codes.E;
			validation.ValidateCF_MethodOfPayment();
			AssertHasMessageError("Error message", propertyInfo, errorMessage);

			fee.CF_MethodOfPayment = PLMethodOfPaymentList.Codes.D;
			validation.ValidateCF_MethodOfPayment();
			AssertNoMessageError("No error message - payment is D", propertyInfo, errorMessage);

			fee.CF_MethodOfPayment = PLMethodOfPaymentList.Codes.A;
			fee.CF_ChargeType = RefCusRateCodes.AdditionalDutyCountervailingSafeguardChargeVariableCharge;
			validation.ValidateCF_MethodOfPayment();
			AssertNoMessageError("No error message - CF_ChargeType != 'A00'", propertyInfo, errorMessage);

			fee.CF_ChargeType = RefCusRateCodes.CustomsDutyOnIndustrialProducts;
			instruction.CEI_Procedure = Constants.ProcedureCodes._11;
			validation.ValidateCF_MethodOfPayment();
			AssertNoMessageError("No error message - other procedure code", propertyInfo, errorMessage);

			instruction.CEI_Procedure = Constants.ProcedureCodes._53;
			invoiceLine.AdditionalProcedureCodes.RemoveAll();
			validation.ValidateCF_MethodOfPayment();
			AssertNoMessageError("No error message - no additional procedure", propertyInfo, errorMessage);
		});
	}

	public void TestCheckRuleR891()
	{
		var errorMessage = "(R891) – Method of Payment code L and D is invalid for B00 charge type.";
		var (declaration, entryLine, fee) = SetEntryLineFeeData();
		var propertyInfo = fee.CF_MethodOfPaymentInfo;
		var validation = fee.Validation;
		var instruction = declaration.CustomsEntryInstructions[0];
		CombineAssertions(() =>
		{
			invoiceLine.AdditionalProcedureCodes.AddNew(Constants.ConcessionCodes.D51);
			instruction.CEI_Procedure = Constants.ProcedureCodes._53;
			fee.CF_ChargeType = RefCusRateCodes.Vat;
			validation.ValidateCF_MethodOfPayment();
			AssertNoMessageError("No error message - payment empty", propertyInfo, errorMessage);

			fee.CF_MethodOfPayment = PLMethodOfPaymentList.Codes.L;
			validation.ValidateCF_MethodOfPayment();
			AssertHasMessageError("Error message", propertyInfo, errorMessage);

			fee.CF_MethodOfPayment = PLMethodOfPaymentList.Codes.E;
			validation.ValidateCF_MethodOfPayment();
			AssertNoMessageError("No error message - payment is not {L,D}", propertyInfo, errorMessage);

			fee.CF_MethodOfPayment = PLMethodOfPaymentList.Codes.D;
			fee.CF_ChargeType = RefCusRateCodes.AdditionalDutyCountervailingSafeguardChargeVariableCharge;
			validation.ValidateCF_MethodOfPayment();
			AssertNoMessageError("No error message - CF_ChargeType != 'B00'", propertyInfo, errorMessage);

			fee.CF_ChargeType = RefCusRateCodes.Vat;
			instruction.CEI_Procedure = Constants.ProcedureCodes._11;
			validation.ValidateCF_MethodOfPayment();
			AssertNoMessageError("No error message - other procedure code", propertyInfo, errorMessage);

			instruction.CEI_Procedure = Constants.ProcedureCodes._53;
			invoiceLine.AdditionalProcedureCodes.RemoveAll();
			validation.ValidateCF_MethodOfPayment();
			AssertNoMessageError("No error message - no additional procedure", propertyInfo, errorMessage);
		});
	}

	public void TestCheckRuleR831()
	{
		var errorMessage = "(R831) For the selected requested procedure code the duty (A00) method of payment code L is only allowed with the additional information code 4PL07.";
		var (declaration, entryLine, fee) = SetEntryLineFeeData();
		var propertyInfo = fee.CF_MethodOfPaymentInfo;
		var validation = fee.Validation;
		var instruction = declaration.CustomsEntryInstructions[0];

		var d01ToD30ConcessionCodes = new string[] {
			ConcessionCodes.D01, ConcessionCodes.D02, ConcessionCodes.D03, ConcessionCodes.D04, ConcessionCodes.D05
			, ConcessionCodes.D06, ConcessionCodes.D07, ConcessionCodes.D08, ConcessionCodes.D09, ConcessionCodes.D10
			, ConcessionCodes.D11, ConcessionCodes.D12, ConcessionCodes.D13, ConcessionCodes.D14, ConcessionCodes.D15
			, ConcessionCodes.D16, ConcessionCodes.D17, ConcessionCodes.D18, ConcessionCodes.D19, ConcessionCodes.D20
			, ConcessionCodes.D21, ConcessionCodes.D22, ConcessionCodes.D23, ConcessionCodes.D24, ConcessionCodes.D25
			, ConcessionCodes.D26, ConcessionCodes.D27, ConcessionCodes.D28, ConcessionCodes.D29, ConcessionCodes.D30
		};
		var concession = invoiceLine.AdditionalProcedureCodes.AddNew(ZString.Empty);
		fee.CF_ChargeType = ChargeTypes.A00;
		CombineAssertions(() =>
		{
			foreach (var procedureCode in new[] { ProcedureCodes._51, ProcedureCodes._53 })
			{
				instruction.CEI_Procedure = procedureCode;

				concession.CY_Code = "D31";
				fee.CF_MethodOfPayment = PLMethodOfPaymentList.Codes.L;
				if (procedureCode == ProcedureCodes._51)
				{
					AssertHasMessageError("ConcessionCode is not one of D01...D30 for ProcedureCode 51", propertyInfo, errorMessage);
				}
				else
				{
					AssertNoMessageError("ConcessionCode is not one of D01...D30", propertyInfo, errorMessage);
				}

				fee.CF_MethodOfPayment = PLMethodOfPaymentList.Codes.T;
				AssertNoMessageError("ConcessionCode is not one of D01...D30 and not MethodOfPayment L", propertyInfo, errorMessage);

				foreach (var concessionCode in d01ToD30ConcessionCodes)
				{
					concession.CY_Code = concessionCode;
					fee.CF_MethodOfPayment = PLMethodOfPaymentList.Codes.L;
					AssertHasMessageError($"foreach - Procedure:{procedureCode} ConcessionCode:{concessionCode} ChargeType:A00 MethodOfPayment:L", propertyInfo, errorMessage);

					var addInfo = invoiceLine.AdditionalInfos.AddNew();
					addInfo.CSI_Code = Constants.AdditionalInfoCodes._4PL05;
					validation.ValidateCF_MethodOfPayment();
					AssertHasMessageError($"Procedure:{procedureCode} ConcessionCode:{concessionCode} ChargeType:A00 MethodOfPayment:L AdditionalInfo is not 4PL07", propertyInfo, errorMessage);

					addInfo.CSI_Code = Constants.AdditionalInfoCodes._4PL07;
					validation.ValidateCF_MethodOfPayment();
					AssertNoMessageError($"Procedure:{procedureCode} ConcessionCode:{concessionCode} ChargeType:A00 MethodOfPayment:L InvoiceLineAdditionalInfo", propertyInfo, errorMessage);

					invoiceLine.AdditionalInfos.RemoveAll();
					addInfo = invoiceLine.InvoiceHeader.AdditionalInfos.AddNew();
					addInfo.CSI_Code = Constants.AdditionalInfoCodes._4PL07;
					validation.ValidateCF_MethodOfPayment();
					AssertNoMessageError($"Procedure:{procedureCode} ConcessionCode:{concessionCode} ChargeType:A00 MethodOfPayment:L InvoiceAdditionalInfo", propertyInfo, errorMessage);

					invoiceLine.InvoiceHeader.AdditionalInfos.RemoveAll();
					addInfo = declaration.AdditionalInfos.AddNew();
					addInfo.CSI_Code = Constants.AdditionalInfoCodes._4PL07;
					validation.ValidateCF_MethodOfPayment();
					AssertNoMessageError($"Procedure:{procedureCode} ConcessionCode:{concessionCode} ChargeType:A00 MethodOfPayment:L DeclarationAdditionalInfo", propertyInfo, errorMessage);

					declaration.AdditionalInfos.RemoveAll();
					fee.CF_ChargeType = ChargeTypes.A35;
					validation.ValidateCF_MethodOfPayment();
					AssertNoMessageError($"Procedure:{procedureCode} ConcessionCode:{concessionCode} ChargeType:A35 MethodOfPayment:L", propertyInfo, errorMessage);

					fee.CF_ChargeType = ChargeTypes.A00;
					fee.CF_MethodOfPayment = PLMethodOfPaymentList.Codes.T;
					AssertNoMessageError($"Procedure:{procedureCode} ConcessionCode:{concessionCode} ChargeType:A00 MethodOfPayment:T", propertyInfo, errorMessage);
				}
			}

			instruction.CEI_Procedure = ProcedureCodes._42;
			validation.ValidateCF_MethodOfPayment();
			AssertNoMessageError("ProcedureCode is not 51 or 53", propertyInfo, errorMessage);
		});
	}

	public void TestCheckRuleR832_PaymentD()
	{
		var errorMessage = "(R832) For the requested procedure code selected and the payment method used, the Total amount value is invalid or additional information code 4PL07 is missing.";
		var (declaration, entryLine, fee) = SetEntryLineFeeData();
		var propertyInfo = fee.CF_MethodOfPaymentInfo;
		var validation = fee.Validation;
		var instruction = declaration.CustomsEntryInstructions[0];

		CombineAssertions(() =>
		{
			instruction.CEI_Procedure = ProcedureCodes._48;
			fee.CF_ChargeAmount = 0m;
			fee.CF_MethodOfPayment = PLMethodOfPaymentList.Codes.D;
			AssertHasMessageError("MethodOfPayment=D ProcedureCode=48 ChargeAmount=0", propertyInfo, errorMessage);

			fee.CF_MethodOfPayment = PLMethodOfPaymentList.Codes.A;
			AssertNoMessageError("MethodOfPayment!=D ProcedureCode=48 ChargeAmount=0", propertyInfo, errorMessage);

			fee.CF_ChargeAmount = 2.5m;
			fee.CF_MethodOfPayment = PLMethodOfPaymentList.Codes.D;
			AssertNoMessageError("MethodOfPayment=D ProcedureCode=48 ChargeAmount!=0", propertyInfo, errorMessage);

			fee.CF_ChargeAmount = 0m;
			instruction.CEI_Procedure = ProcedureCodes._42;
			validation.ValidateCF_MethodOfPayment();
			AssertNoMessageError("MethodOfPayment=D ProcedureCode!=48 ChargeAmount=0", propertyInfo, errorMessage);
		});
	}

	public void TestCheckRuleR832_PaymentNotD()
	{
		var errorMessage = "(R832) For the requested procedure code selected and the payment method used, the Total amount value is invalid or additional information code 4PL07 is missing.";
		var (declaration, entryLine, fee) = SetEntryLineFeeData();
		var propertyInfo = fee.CF_MethodOfPaymentInfo;
		var validation = fee.Validation;
		var instruction = declaration.CustomsEntryInstructions[0];

		CombineAssertions(() =>
		{
			instruction.CEI_Procedure = ProcedureCodes._48;
			fee.CF_ChargeAmount = 0m;
			fee.CF_MethodOfPayment = PLMethodOfPaymentList.Codes.A;
			AssertNoMessageError("MethodOfPayment!=D ProcedureCode=48 ChargeAmount=0", propertyInfo, errorMessage);

			fee.CF_ChargeAmount = 2.5m;
			validation.ValidateCF_MethodOfPayment();
			AssertHasMessageError("MethodOfPayment!=D ProcedureCode=48 ChargeAmount!=0", propertyInfo, errorMessage);

			var addInfo = invoiceLine.AdditionalInfos.AddNew();
			addInfo.CSI_Code = Constants.AdditionalInfoCodes._4PL05;
			validation.ValidateCF_MethodOfPayment();
			AssertHasMessageError($"MethodOfPayment!=D ProcedureCode=48 ChargeAmount!=0 AdditionalInfo is not 4PL07", propertyInfo, errorMessage);

			addInfo.CSI_Code = Constants.AdditionalInfoCodes._4PL07;
			validation.ValidateCF_MethodOfPayment();
			AssertNoMessageError($"MethodOfPayment!=D ProcedureCode=48 ChargeAmount!=0 InvoiceLineAdditionalInfo", propertyInfo, errorMessage);

			invoiceLine.AdditionalInfos.RemoveAll();
			addInfo = invoiceLine.InvoiceHeader.AdditionalInfos.AddNew();
			addInfo.CSI_Code = Constants.AdditionalInfoCodes._4PL07;
			validation.ValidateCF_MethodOfPayment();
			AssertNoMessageError($"MethodOfPayment!=D ProcedureCode=48 ChargeAmount!=0 InvoiceAdditionalInfo", propertyInfo, errorMessage);

			invoiceLine.InvoiceHeader.AdditionalInfos.RemoveAll();
			addInfo = declaration.AdditionalInfos.AddNew();
			addInfo.CSI_Code = Constants.AdditionalInfoCodes._4PL07;
			validation.ValidateCF_MethodOfPayment();
			AssertNoMessageError($"MethodOfPayment!=D ProcedureCode=48 ChargeAmount!=0 DeclarationAdditionalInfo", propertyInfo, errorMessage);

			declaration.AdditionalInfos.RemoveAll();
			fee.CF_ChargeAmount = 0m;
			instruction.CEI_Procedure = ProcedureCodes._42;
			validation.ValidateCF_MethodOfPayment();
			AssertNoMessageError("MethodOfPayment!=D ProcedureCode!=48 ChargeAmount=0", propertyInfo, errorMessage);
		});
	}

	public void TestCheckRuleR833()
	{
		var errorMessage = "(R833) For the selected requested procedure code the method of payment code for Z is not allowed.";
		var (declaration, entryLine, fee) = SetEntryLineFeeData();
		var propertyInfo = fee.CF_MethodOfPaymentInfo;
		var validation = fee.Validation;
		var instruction = declaration.CustomsEntryInstructions[0];

		var d01ToD30ConcessionCodes = new string[] {
			ConcessionCodes.D01, ConcessionCodes.D02, ConcessionCodes.D03, ConcessionCodes.D04, ConcessionCodes.D05
			, ConcessionCodes.D06, ConcessionCodes.D07, ConcessionCodes.D08, ConcessionCodes.D09, ConcessionCodes.D10
			, ConcessionCodes.D11, ConcessionCodes.D12, ConcessionCodes.D13, ConcessionCodes.D14, ConcessionCodes.D15
			, ConcessionCodes.D16, ConcessionCodes.D17, ConcessionCodes.D18, ConcessionCodes.D19, ConcessionCodes.D20
			, ConcessionCodes.D21, ConcessionCodes.D22, ConcessionCodes.D23, ConcessionCodes.D24, ConcessionCodes.D25
			, ConcessionCodes.D26, ConcessionCodes.D27, ConcessionCodes.D28, ConcessionCodes.D29, ConcessionCodes.D30
		};
		var concession = invoiceLine.AdditionalProcedureCodes.AddNew(ZString.Empty);
		CombineAssertions(() =>
		{
			foreach (var procedureCode in new[] { ProcedureCodes._51, ProcedureCodes._53 })
			{
				instruction.CEI_Procedure = procedureCode;

				concession.CY_Code = "D31";
				fee.CF_MethodOfPayment = PLMethodOfPaymentList.Codes.Z;
				if (procedureCode == ProcedureCodes._51)
				{
					AssertHasMessageError("ConcessionCode is not one of D01...D30 for ProcedureCode 51", propertyInfo, errorMessage);
				}
				else
				{
					AssertNoMessageError("ConcessionCode is not one of D01...D30", propertyInfo, errorMessage);
				}

				fee.CF_MethodOfPayment = PLMethodOfPaymentList.Codes.T;
				AssertNoMessageError("ConcessionCode is not one of D01...D30 and not MethodOfPayment Z", propertyInfo, errorMessage);

				foreach (var concessionCode in d01ToD30ConcessionCodes)
				{
					concession.CY_Code = concessionCode;
					fee.CF_MethodOfPayment = PLMethodOfPaymentList.Codes.Z;
					AssertHasMessageError($"foreach - Procedure:{procedureCode} ConcessionCode:{concessionCode} MethodOfPayment:Z", propertyInfo, errorMessage);

					fee.CF_ChargeType = ChargeTypes.A00;
					fee.CF_MethodOfPayment = PLMethodOfPaymentList.Codes.T;
					AssertNoMessageError($"Procedure:{procedureCode} ConcessionCode:{concessionCode} MethodOfPayment:T", propertyInfo, errorMessage);
				}
			}

			instruction.CEI_Procedure = ProcedureCodes._42;
			validation.ValidateCF_MethodOfPayment();
			AssertNoMessageError("ProcedureCode is not 51 or 53", propertyInfo, errorMessage);
		});
	}

	public void TestCheckRuleR835()
	{
		var errorMessage = "(R835) For the selected requested procedure code the method of payment code for A is not allowed.";
		var (declaration, entryLine, fee) = SetEntryLineFeeData();
		var propertyInfo = fee.CF_MethodOfPaymentInfo;
		var validation = fee.Validation;
		var instruction = declaration.CustomsEntryInstructions[0];

		var d01ToD30ConcessionCodes = new string[] {
			ConcessionCodes.D01, ConcessionCodes.D02, ConcessionCodes.D03, ConcessionCodes.D04, ConcessionCodes.D05
			, ConcessionCodes.D06, ConcessionCodes.D07, ConcessionCodes.D08, ConcessionCodes.D09, ConcessionCodes.D10
			, ConcessionCodes.D11, ConcessionCodes.D12, ConcessionCodes.D13, ConcessionCodes.D14, ConcessionCodes.D15
			, ConcessionCodes.D16, ConcessionCodes.D17, ConcessionCodes.D18, ConcessionCodes.D19, ConcessionCodes.D20
			, ConcessionCodes.D21, ConcessionCodes.D22, ConcessionCodes.D23, ConcessionCodes.D24, ConcessionCodes.D25
			, ConcessionCodes.D26, ConcessionCodes.D27, ConcessionCodes.D28, ConcessionCodes.D29, ConcessionCodes.D30
		};
		var concession = invoiceLine.AdditionalProcedureCodes.AddNew(ZString.Empty);
		CombineAssertions(() =>
		{
			foreach (var procedureCode in new[] { ProcedureCodes._51, ProcedureCodes._53 })
			{
				instruction.CEI_Procedure = procedureCode;

				concession.CY_Code = "D31";
				fee.CF_MethodOfPayment = PLMethodOfPaymentList.Codes.A;

				if (procedureCode == ProcedureCodes._51)
				{
					AssertHasMessageError("ConcessionCode is not one of D01...D30 for ProcedureCode 51", propertyInfo, errorMessage);
				}
				else
				{
					AssertNoMessageError("ConcessionCode is not one of D01...D30", propertyInfo, errorMessage);
				}

				fee.CF_MethodOfPayment = PLMethodOfPaymentList.Codes.T;
				AssertNoMessageError("ConcessionCode is not one of D01...D30 and not MethodOfPayment A", propertyInfo, errorMessage);

				foreach (var concessionCode in d01ToD30ConcessionCodes)
				{
					concession.CY_Code = concessionCode;
					fee.CF_MethodOfPayment = PLMethodOfPaymentList.Codes.A;
					AssertHasMessageError($"foreach - Procedure:{procedureCode} ConcessionCode:{concessionCode} MethodOfPayment:A", propertyInfo, errorMessage);

					fee.CF_ChargeType = ChargeTypes.A00;
					fee.CF_MethodOfPayment = PLMethodOfPaymentList.Codes.T;
					AssertNoMessageError($"Procedure:{procedureCode} ConcessionCode:{concessionCode} MethodOfPayment:T", propertyInfo, errorMessage);
				}
			}

			instruction.CEI_Procedure = ProcedureCodes._42;
			validation.ValidateCF_MethodOfPayment();
			AssertNoMessageError("ProcedureCode is not 51 or 53", propertyInfo, errorMessage);
		});
	}

	public void TestCheckRuleR836()
	{
		var errorMessage = "(R836) For the requested procedure code selected the payment method ’D’ and duty amount = 0 is not allowed.";
		var (declaration, entryLine, fee) = SetEntryLineFeeData();
		var propertyInfo = fee.CF_MethodOfPaymentInfo;
		var validation = fee.Validation;
		var instruction = declaration.CustomsEntryInstructions[0];

		var procedureCodes = new[] { ProcedureCodes._40, ProcedureCodes._42, ProcedureCodes._44
			, ProcedureCodes._45, ProcedureCodes._61, ProcedureCodes._63, ProcedureCodes._68 };
		var chargeTypes = new[] { ChargeTypes._1P1, ChargeTypes._1S1, ChargeTypes._1T1
			, ChargeTypes.A35, ChargeTypes.A45 };

		fee.CF_MethodOfPayment = PLMethodOfPaymentList.Codes.D;
		CombineAssertions(() =>
		{
			foreach (var chargeType in chargeTypes)
			{
				invoiceLine.AdditionalProcedureCodes.RemoveAll();
				foreach (var procedureCode in procedureCodes)
				{
					instruction.CEI_Procedure = procedureCode;
					fee.CF_ChargeAmount = 0m;
					fee.CF_ChargeType = chargeType;
					validation.ValidateCF_MethodOfPayment();
					AssertHasMessageError($"ProcedureCode={procedureCode} chargeType={chargeType} ChargeAmount=0 MethodOfPayment=D", propertyInfo, errorMessage);

					fee.CF_ChargeAmount = 2m;
					validation.ValidateCF_MethodOfPayment();
					AssertNoMessageError($"ProcedureCode={procedureCode} chargeType={chargeType} ChargeAmount=!0 MethodOfPayment=D", propertyInfo, errorMessage);

					fee.CF_ChargeAmount = 0m;
					fee.CF_MethodOfPayment = PLMethodOfPaymentList.Codes.A;
					AssertNoMessageError($"ProcedureCode={procedureCode} chargeType={chargeType} ChargeAmount=0 MethodOfPayment=A", propertyInfo, errorMessage);

					fee.CF_ChargeType = "000";
					fee.CF_MethodOfPayment = PLMethodOfPaymentList.Codes.D;
					AssertNoMessageError($"ProcedureCode={procedureCode} chargeType=000 ChargeAmount=0", propertyInfo, errorMessage);
				}

				fee.CF_ChargeType = chargeType;
				instruction.CEI_Procedure = ProcedureCodes._53;
				invoiceLine.AdditionalProcedureCodes.AddNew(ConcessionCodes.D27);
				validation.ValidateCF_MethodOfPayment();
				AssertNoMessageError($"ProcedureCode=53 chargeType={chargeType} Concession!=D51", propertyInfo, errorMessage);

				invoiceLine.AdditionalProcedureCodes.AddNew(ConcessionCodes.D51);
				validation.ValidateCF_MethodOfPayment();
				AssertHasMessageError($"ProcedureCode=53 chargeType={chargeType} Concession=D51", propertyInfo, errorMessage);
			}
		});
	}

	public void TestCheckRuleR840()
	{
		var errorMessage = "(R840) For the requested procedure code and transport mode selected the payment method ’J’ is not allowed.";
		var (declaration, entryLine, fee) = SetEntryLineFeeData();
		var propertyInfo = fee.CF_MethodOfPaymentInfo;
		var validation = fee.Validation;
		var instruction = declaration.CustomsEntryInstructions[0];

		var procedureCodes = new[] { ProcedureCodes._40, ProcedureCodes._42, ProcedureCodes._44
			, ProcedureCodes._45, ProcedureCodes._61, ProcedureCodes._63, ProcedureCodes._68 };

		CombineAssertions(() =>
		{
			foreach (var procedureCode in procedureCodes)
			{
				instruction.CEI_Procedure = procedureCode;
				declaration.JE_TransportMode = Core.Constants.TransportModes.Rail;
				fee.CF_MethodOfPayment = PLMethodOfPaymentList.Codes.J;
				AssertHasMessageError($"ProcedureCode={procedureCode} MethodOfPayment=J TransportMode!=Mail", propertyInfo, errorMessage);

				declaration.JE_TransportMode = Core.Constants.TransportModes.Mail;
				validation.ValidateCF_MethodOfPayment();
				AssertNoMessageError($"ProcedureCode={procedureCode} MethodOfPayment=J TransportMode=Mail", propertyInfo, errorMessage);

				declaration.JE_TransportMode = Core.Constants.TransportModes.Road;
				fee.CF_MethodOfPayment = PLMethodOfPaymentList.Codes.A;
				AssertNoMessageError($"ProcedureCode={procedureCode} MethodOfPayment=A TransportMode!=Mail", propertyInfo, errorMessage);
			}

			fee.CF_MethodOfPayment = PLMethodOfPaymentList.Codes.J;
			instruction.CEI_Procedure = ProcedureCodes._53;
			invoiceLine.AdditionalProcedureCodes.AddNew(ConcessionCodes.D27);
			validation.ValidateCF_MethodOfPayment();
			AssertNoMessageError($"ProcedureCode=53 Concession!=D51", propertyInfo, errorMessage);

			invoiceLine.AdditionalProcedureCodes.AddNew(ConcessionCodes.D51);
			validation.ValidateCF_MethodOfPayment();
			AssertHasMessageError($"ProcedureCode=53 Concession=D51", propertyInfo, errorMessage);
		});
	}

	protected override (JobDeclaration declaration, CusEntryLine entryLine, CusEntryLineFee entryLineFee) SetEntryLineFeeData()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		var invoiceHeader = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
		invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
		invoiceLine.JI_CEI = entryInstruction.PK;
		invoiceLine.AdditionalInfos.AddNew();
		var merger = new LineMerger(declaration);
		merger.DoMerge();
		var entryLine = declaration.ActiveEntryHeaders.Cast<CusEntryHeader>().First().MergedLines.First();
		var entryLineFee = entryLine.Fees.AddNew();
		return (declaration, entryLine, entryLineFee);
	}

	JobComInvoiceLine invoiceLine;
}
