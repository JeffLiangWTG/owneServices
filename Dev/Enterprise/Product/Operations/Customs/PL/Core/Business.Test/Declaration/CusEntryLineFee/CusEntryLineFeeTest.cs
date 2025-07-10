using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.PL.Business.Declaration.Testing;

[TestedType(typeof(CusEntryLineFee))]
class CusEntryLineFeeTest : EU.Business.Declaration.Testing.CusEntryLineFeeTest<JobDeclaration, CusEntryLine, CusEntryLineFee>
{
	public void TestValidation()
	{
		CombineAssertions(() =>
		{
			AssertType<CusEntryLineFeeValidation>("Non-IMP", GetFee().Validation);
			AssertType<ImportCusEntryLineFeeValidation>("IMP", GetFee(MessageTypeList.Codes.Import).Validation);
		});
	}

	public void TestCF_MethodOfPayment_MaxLength()
	{
		var fee = GetFee();
		AssertEquals(1, fee.CF_MethodOfPaymentInfo.MaxLength);
	}

	public void TestCF_MethodOfPayment_Uppercase()
	{
		var fee = GetFee();
		fee.CF_MethodOfPayment = "a";
		AssertEquals("A", fee.CF_MethodOfPayment);
	}

	public void TestMethodOfCalculationPL()
	{
		var entryLineFee = SetEntryLineFeeData().entryLineFee;
		entryLineFee.CF_MethodOfCalculation = Customs.Business.UniversalReferenceConstants.MethodOfCalculation.Percentage;
		entryLineFee.CF_Rate = 789M;
		CombineAssertions(() =>
		{
			AssertEquals("Tax rate is set", 789M, entryLineFee.CF_Rate);
			entryLineFee.CF_MethodOfCalculation = ZString.Empty;
			AssertEquals("Tax Rate is set to zero", ZDecimal.Zero, entryLineFee.CF_Rate);
		});
	}

	public void TestRateForDisplay()
	{
		var entryLineFee = SetEntryLineFeeData().entryLineFee;
		entryLineFee.CF_MethodOfCalculation = ZString.Empty;
		CombineAssertions(() =>
		{
			AssertEquals("Rate Display string is empty due to Method of Calculation", ZString.Empty, entryLineFee.CF_RateForDisplay);
			entryLineFee.CF_MethodOfCalculation = Customs.Business.UniversalReferenceConstants.MethodOfCalculation.Percentage;
			AssertEquals("Rate Display string for tax rate", "0,0000000", entryLineFee.CF_RateForDisplay);
			entryLineFee.CF_RateForDisplay = "2,3456";
			AssertEquals("CF_Rate is set from Rate Display string", 2.3456M, entryLineFee.CF_Rate);
			entryLineFee.CF_RateForDisplay = "";
			AssertEquals("CF_Rate is set zero from empty Rate Display string", 0M, entryLineFee.CF_Rate);
		});
	}

	public void TestRateForDisplay_ReadOnly()
	{
		var entryLineFee = SetEntryLineFeeData().entryLineFee;
		entryLineFee.CF_Rate = ZDecimal.Zero;
		entryLineFee.CF_RateOverrideReasonCode = "123";
		entryLineFee.CF_MethodOfCalculation = ZString.Empty;
		CombineAssertions(() =>
		{
			Assert("Rate Display string is readonly due to Method of Calculation", entryLineFee.CF_RateForDisplayInfo.ReadOnly);
			entryLineFee.CF_MethodOfCalculation = Customs.Business.UniversalReferenceConstants.MethodOfCalculation.Percentage;
			AssertEquals("Rate Display is editable", false, entryLineFee.CF_RateForDisplayInfo.ReadOnly);
			entryLineFee.CF_RateOverrideReasonCode = ZString.Empty;
			Assert("Rate Display string is readonly due to Rate Override Reason Code", entryLineFee.CF_RateForDisplayInfo.ReadOnly);
		});
	}

	public void TestBaseValueForDisplay()
	{
		var entryLineFee = SetEntryLineFeeData().entryLineFee;
		entryLineFee.CF_MethodOfCalculation = Customs.Business.UniversalReferenceConstants.MethodOfCalculation.Percentage;
		entryLineFee.CF_ChargeType = EU.Business.UniversalReferenceConstants.RefCusRateCodes.Vat;
		CombineAssertions(() =>
		{
			AssertEquals("Base Amount is displayed for percentage calculation", "0,0000", entryLineFee.CF_BaseValueForDisplay);
			entryLineFee.CF_MethodOfCalculation = ZString.Empty;
			AssertEquals("Base Amount display string is empty due to Method of Calculation", ZString.Empty, entryLineFee.CF_BaseValueForDisplay);
			entryLineFee.CF_ChargeType = EU.Business.UniversalReferenceConstants.RefCusRateCodes.CustomsDutyOnIndustrialProducts;
			AssertEquals("Base Amount is displayed for A00", "0,0000", entryLineFee.CF_BaseValueForDisplay);
			entryLineFee.CF_BaseValueForDisplay = "2,34567";
			AssertEquals("Base Value is set from Base Amount string", 2.3457M, entryLineFee.CF_BaseValue);
			entryLineFee.CF_BaseValueForDisplay = "";
			AssertEquals("Base Value is set zero from empty Base Amount string", 0M, entryLineFee.CF_BaseValue);
		});
	}

	public void TestBaseValueForDisplay_ReadOnly()
	{
		var entryLineFee = SetEntryLineFeeData().entryLineFee;
		entryLineFee.CF_Rate = ZDecimal.Zero;
		entryLineFee.CF_RateOverrideReasonCode = "123";
		entryLineFee.CF_MethodOfCalculation = Customs.Business.UniversalReferenceConstants.MethodOfCalculation.Percentage;
		entryLineFee.CF_ChargeType = EU.Business.UniversalReferenceConstants.RefCusRateCodes.Vat;
		CombineAssertions(() =>
		{
			AssertEquals("Rate Display is editable for precentage calculation", false, entryLineFee.CF_BaseValueForDisplayInfo.ReadOnly);
			entryLineFee.CF_MethodOfCalculation = ZString.Empty;
			Assert("Base Amount string is readonly due to Method of Calculation", entryLineFee.CF_BaseValueForDisplayInfo.ReadOnly);
			entryLineFee.CF_ChargeType = EU.Business.UniversalReferenceConstants.RefCusRateCodes.CustomsDutyOnIndustrialProducts;
			AssertEquals("Base Amount string is editable for duty type A00", false, entryLineFee.CF_BaseValueForDisplayInfo.ReadOnly);
			entryLineFee.CF_RateOverrideReasonCode = ZString.Empty;
			Assert("Rate Display string is readonly due to Rate Override Reason Code", entryLineFee.CF_BaseValueForDisplayInfo.ReadOnly);
		});
	}

	public void TestCF_RateFieldType()
	{
		var entryLineFee = SetEntryLineFeeData().entryLineFee;
		entryLineFee.CF_MethodOfCalculation = ZString.Empty;
		CombineAssertions(() =>
		{
			AssertEquals("Field Type is Text", nameof(FieldType.Text), entryLineFee.CF_RateFieldType);
			entryLineFee.CF_MethodOfCalculation = Customs.Business.UniversalReferenceConstants.MethodOfCalculation.Percentage;
			AssertEquals("Field Type is Decimal", nameof(FieldType.Decimal), entryLineFee.CF_RateFieldType);
		});
	}

	public void TestCF_BaseValueFieldType()
	{
		var entryLineFee = SetEntryLineFeeData().entryLineFee;
		entryLineFee.CF_MethodOfCalculation = Customs.Business.UniversalReferenceConstants.MethodOfCalculation.Percentage;
		entryLineFee.CF_ChargeType = EU.Business.UniversalReferenceConstants.RefCusRateCodes.Vat;
		CombineAssertions(() =>
		{
			AssertEquals("Field Type is Decimal for percentage calculation", nameof(FieldType.Decimal), entryLineFee.CF_BaseValueFieldType);
			entryLineFee.CF_MethodOfCalculation = ZString.Empty;
			AssertEquals("Field Type is Text due to Method of Calculation", nameof(FieldType.Text), entryLineFee.CF_BaseValueFieldType);
			entryLineFee.CF_ChargeType = EU.Business.UniversalReferenceConstants.RefCusRateCodes.CustomsDutyOnIndustrialProducts;
			AssertEquals("FieldType is Decimal for A00", nameof(FieldType.Decimal), entryLineFee.CF_BaseValueFieldType);
		});
	}

	public void TestCF_RateDecimalPlaces()
	{
		var entryLineFee = SetEntryLineFeeData().entryLineFee;
		AssertEquals(CusEntryLineFeeSchema.CF_Rate.Scale, entryLineFee.CF_RateDecimalPlaces);
	}

	public void TestCF_BaseValueForDisplayDecimalPlaces()
	{
		var entryLineFee = SetEntryLineFeeData().entryLineFee;
		AssertEquals("Decimal places for PL", 4, entryLineFee.CF_BaseValueForDisplayDecimalPlaces);
	}

	public void TestDefaultMethodOfPaymentIfEmpty()
	{
		var entryLineFee = GetFee(MessageTypeList.Codes.Import);
		CombineAssertions(() =>
		{
			entryLineFee.EntryLine.Declaration.JE_PaymentMethod = PLMethodOfPaymentList.Codes.D;
			entryLineFee.EntryLine.Declaration.ZG_ExciseCode = PLMethodOfPaymentList.Codes.E;
			entryLineFee.EntryLine.Declaration.ZG_VATDeferType = PLMethodOfPaymentList.Codes.V;
			entryLineFee.CF_ChargeType = EU.Business.UniversalReferenceConstants.RefCusRateCodes.Vat;
			entryLineFee.DefaultMethodOfPaymentIfEmpty();
			AssertEquals("Import Vat Default Method of Payment", PLMethodOfPaymentList.Codes.V, entryLineFee.CF_MethodOfPayment);
			entryLineFee.CF_ChargeType = TaxTypeList.Codes.ExciseTax;
			entryLineFee.CF_MethodOfPayment = ZString.Empty;
			entryLineFee.DefaultMethodOfPaymentIfEmpty();
			AssertEquals("Import 1A1 Default Method of Payment", PLMethodOfPaymentList.Codes.E, entryLineFee.CF_MethodOfPayment);
			entryLineFee.CF_ChargeType = EU.Business.UniversalReferenceConstants.RefCusRateCodes.CustomsDutyOnIndustrialProducts;
			entryLineFee.CF_MethodOfPayment = ZString.Empty;
			entryLineFee.DefaultMethodOfPaymentIfEmpty();
			AssertEquals("Import Customs Duty Default Method of Payment", PLMethodOfPaymentList.Codes.D, entryLineFee.CF_MethodOfPayment);
			entryLineFee.EntryLine.Declaration.JE_PaymentMethod = ZString.Empty;
			entryLineFee.CF_MethodOfPayment = ZString.Empty;
			entryLineFee.DefaultMethodOfPaymentIfEmpty();
			AssertEquals("Import Customs Duty Default Method of Payment - Empty JE_PaymentMethod", ZString.Empty,
				entryLineFee.CF_MethodOfPayment);
			entryLineFee = GetFee();
			entryLineFee.EntryLine.Declaration.JE_PaymentMethod = PLMethodOfPaymentList.Codes.D;
			entryLineFee.DefaultMethodOfPaymentIfEmpty();
			AssertEquals("Export Customs Duty Default Method of Payment", ZString.Empty, entryLineFee.CF_MethodOfPayment);
		});
	}

	public void TestDefaultMethodOfPaymentIfEmpty_R246()
	{
		var entryLineFee = GetFee(MessageTypeList.Codes.Import);
		var entryInstruction = entryLineFee.EntryLine.Header.EntryInstruction;
		CombineAssertions(() =>
		{
			entryLineFee.CF_ChargeType = EU.Business.UniversalReferenceConstants.RefCusRateCodes.Vat;
			entryInstruction.CEI_Procedure = Constants.ProcedureCodes._40;
			entryLineFee.CF_MethodOfPayment = ZString.Empty;
			var additionalProcedureCode = invoiceLine.AdditionalProcedureCodes.AddNew(Constants.ConcessionCodes._3V0);
			entryLineFee.DefaultMethodOfPaymentIfEmpty();
			AssertEquals("R246 fulfilled", PLMethodOfPaymentList.Codes.Z, entryLineFee.CF_MethodOfPayment);
			entryLineFee.CF_MethodOfPayment = ZString.Empty;
			entryInstruction.CEI_Procedure = Constants.ProcedureCodes._71;
			entryLineFee.DefaultMethodOfPaymentIfEmpty();
			AssertEquals("CEI_Procedure start different than 4 and 6", ZString.Empty, entryLineFee.CF_MethodOfPayment);
			entryInstruction.CEI_Procedure = Constants.ProcedureCodes._40;
			additionalProcedureCode.CY_Code = Constants.ConcessionCodes.C02;
			entryLineFee.DefaultMethodOfPaymentIfEmpty();
			AssertEquals("Details code not from list", ZString.Empty, entryLineFee.CF_MethodOfPayment);
		});
	}

	public void TestDefaultMethodOfPaymentIfEmpty_R885()
	{
		var entryLineFee = GetFee(MessageTypeList.Codes.Import);
		var entryInstruction = entryLineFee.EntryLine.Header.EntryInstruction;
		var declaration = entryLineFee.EntryLine.Declaration;
		CombineAssertions(() =>
		{
			var fiscalReference = invoiceLine.FiscalReferences.AddNew();
			fiscalReference.CFR_Code = FiscalReferenceCodeList.Codes.FR7_Taxpayer;
			entryLineFee.CF_ChargeType = EU.Business.UniversalReferenceConstants.RefCusRateCodes.Vat;
			entryLineFee.CF_MethodOfPayment = ZString.Empty;
			entryLineFee.DefaultMethodOfPaymentIfEmpty();
			AssertEquals("R885 fulfilled - add info for invoice line", PLMethodOfPaymentList.Codes.G, entryLineFee.CF_MethodOfPayment);

			entryLineFee.CF_MethodOfPayment = ZString.Empty;
			fiscalReference.CFR_Code = FiscalReferenceCodeList.Codes.FR3_TaxRepresentative;
			entryLineFee.DefaultMethodOfPaymentIfEmpty();
			AssertEquals("No add info", ZString.Empty, entryLineFee.CF_MethodOfPayment);
		});
	}

	public void TestDefaultMethodOfPaymentIfEmpty_R1538()
	{
		var entryLineFee = GetFee(MessageTypeList.Codes.Import);
		var entryInstruction = entryLineFee.EntryLine.Header.EntryInstruction;
		var declaration = entryLineFee.EntryLine.Declaration;
		CombineAssertions(() =>
		{
			var addInfo = invoiceLine.AdditionalInfos.AddNew();
			addInfo.CSI_Code = Constants.AdditionalInfoCodes._4PL10;
			entryLineFee.CF_ChargeType = TaxTypeList.Codes.ExciseTax;
			entryLineFee.CF_MethodOfPayment = ZString.Empty;
			entryLineFee.DefaultMethodOfPaymentIfEmpty();
			AssertEquals("R1538 fulfilled - add info for invoice line", PLMethodOfPaymentList.Codes.L, entryLineFee.CF_MethodOfPayment);
			entryLineFee.CF_MethodOfPayment = ZString.Empty;
			invoiceLine.AdditionalInfos.RemoveAll();
			addInfo = invoiceLine.InvoiceHeader.AdditionalInfos.AddNew();
			addInfo.CSI_Code = Constants.AdditionalInfoCodes._4PL10;
			entryLineFee.DefaultMethodOfPaymentIfEmpty();
			AssertEquals("R1538 fulfilled - add info for invoice", PLMethodOfPaymentList.Codes.L, entryLineFee.CF_MethodOfPayment);
			entryLineFee.CF_MethodOfPayment = ZString.Empty;
			invoiceLine.InvoiceHeader.AdditionalInfos.RemoveAll();
			addInfo = declaration.AdditionalInfos.AddNew();
			addInfo.CSI_Code = Constants.AdditionalInfoCodes._4PL10;
			entryLineFee.DefaultMethodOfPaymentIfEmpty();
			AssertEquals("R1538 fulfilled - add info for declaration", PLMethodOfPaymentList.Codes.L, entryLineFee.CF_MethodOfPayment);
			entryLineFee.CF_MethodOfPayment = ZString.Empty;
			declaration.AdditionalInfos.RemoveAll();
			entryLineFee.DefaultMethodOfPaymentIfEmpty();
			AssertEquals("No add info", ZString.Empty, entryLineFee.CF_MethodOfPayment);
		});
	}

	public void TestDefaultMethodOfPaymentIfEmpty_R247()
	{
		var entryLineFee = GetFee(MessageTypeList.Codes.Import);
		var entryInstruction = entryLineFee.EntryLine.Header.EntryInstruction;
		var declaration = entryLineFee.EntryLine.Declaration;
		CombineAssertions(() =>
		{
			entryLineFee.CF_ChargeType = TaxTypeList.Codes.ExciseTax;
			entryInstruction.CEI_Procedure = Constants.ProcedureCodes._40;
			entryLineFee.CF_MethodOfPayment = ZString.Empty;
			var additionalProcedureCode = invoiceLine.AdditionalProcedureCodes.AddNew(Constants.ConcessionCodes._8A2);
			entryLineFee.DefaultMethodOfPaymentIfEmpty();
			AssertEquals("R247 fulfilled", PLMethodOfPaymentList.Codes.Z, entryLineFee.CF_MethodOfPayment);
			entryLineFee.CF_MethodOfPayment = ZString.Empty;
			entryInstruction.CEI_Procedure = Constants.ProcedureCodes._71;
			entryLineFee.DefaultMethodOfPaymentIfEmpty();
			AssertEquals("CEI_Procedure start different than 4 and 6", ZString.Empty, entryLineFee.CF_MethodOfPayment);
			entryInstruction.CEI_Procedure = Constants.ProcedureCodes._40;
			additionalProcedureCode.CY_Code = Constants.ConcessionCodes.C02;
			entryLineFee.DefaultMethodOfPaymentIfEmpty();
			AssertEquals("Details code not from list", ZString.Empty, entryLineFee.CF_MethodOfPayment);
		});
	}

	public void TestDefaultMethodOfPaymentIfEmpty_R296()
	{
		var entryLineFee = GetFee(MessageTypeList.Codes.Import);
		var entryInstruction = entryLineFee.EntryLine.Header.EntryInstruction;
		var declaration = entryLineFee.EntryLine.Declaration;
		CombineAssertions(() =>
		{
			entryLineFee.CF_ChargeType = TaxTypeList.Codes.ExciseTax;
			entryInstruction.CEI_Procedure = Constants.ProcedureCodes._45;
			entryLineFee.CF_MethodOfPayment = ZString.Empty;
			var additionalProcedureCode = invoiceLine.AdditionalProcedureCodes.AddNew(Constants.ConcessionCodes.F06);
			entryLineFee.DefaultMethodOfPaymentIfEmpty();
			AssertEquals("R296 fulfilled", PLMethodOfPaymentList.Codes.L, entryLineFee.CF_MethodOfPayment);
			entryLineFee.CF_MethodOfPayment = ZString.Empty;
			entryInstruction.CEI_Procedure = Constants.ProcedureCodes._61;
			entryLineFee.DefaultMethodOfPaymentIfEmpty();
			AssertEquals("CEI_Procedure different than 45 and 68", PLMethodOfPaymentList.Codes.L, entryLineFee.CF_MethodOfPayment);
			entryLineFee.CF_MethodOfPayment = ZString.Empty;
			additionalProcedureCode.CY_Code = Constants.ConcessionCodes.C02;
			entryLineFee.DefaultMethodOfPaymentIfEmpty();
			AssertEquals("CEI_Procedure different than 45 and 68 and details code not from list", ZString.Empty, entryLineFee.CF_MethodOfPayment);
		});
	}

	public void TestDefaultMethodOfPaymentIfEmpty_R245()
	{
		var entryLineFee = GetFee(MessageTypeList.Codes.Import);
		var entryInstruction = entryLineFee.EntryLine.Header.EntryInstruction;
		var declaration = entryLineFee.EntryLine.Declaration;
		CombineAssertions(() =>
		{
			entryLineFee.CF_ChargeType = EU.Business.UniversalReferenceConstants.RefCusRateCodes.CustomsDutyOnIndustrialProducts;
			entryInstruction.CEI_Procedure = Constants.ProcedureCodes._40;
			entryLineFee.CF_MethodOfPayment = ZString.Empty;
			var additionalProcedureCode = invoiceLine.AdditionalProcedureCodes.AddNew(Constants.ConcessionCodes.C02);
			entryLineFee.DefaultMethodOfPaymentIfEmpty();
			AssertEquals("R245 fulfilled", PLMethodOfPaymentList.Codes.Z, entryLineFee.CF_MethodOfPayment);
			entryLineFee.CF_MethodOfPayment = ZString.Empty;
			entryInstruction.CEI_Procedure = Constants.ProcedureCodes._31;
			entryLineFee.DefaultMethodOfPaymentIfEmpty();
			AssertEquals("CEI_Procedure start different than 4 and 6", ZString.Empty, entryLineFee.CF_MethodOfPayment);
			entryInstruction.CEI_Procedure = Constants.ProcedureCodes._40;
			additionalProcedureCode.CY_Code = Constants.ConcessionCodes.F06;
			entryLineFee.DefaultMethodOfPaymentIfEmpty();
			AssertEquals("Details code not from list", ZString.Empty, entryLineFee.CF_MethodOfPayment);
		});
	}

	public void TestDefaultMethodOfPaymentIfEmpty_R966()
	{
		var entryLineFee = GetFee(MessageTypeList.Codes.Import);
		var entryInstruction = entryLineFee.EntryLine.Header.EntryInstruction;
		var declaration = entryLineFee.EntryLine.Declaration;
		CombineAssertions(() =>
		{
			entryLineFee.CF_ChargeType = EU.Business.UniversalReferenceConstants.RefCusRateCodes.CustomsDutyOnIndustrialProducts;
			entryInstruction.CEI_Procedure = Constants.ProcedureCodes._49;
			entryLineFee.CF_MethodOfPayment = ZString.Empty;
			entryLineFee.DefaultMethodOfPaymentIfEmpty();
			AssertEquals("R966 fulfilled", PLMethodOfPaymentList.Codes.L, entryLineFee.CF_MethodOfPayment);
			entryLineFee.CF_MethodOfPayment = ZString.Empty;
			entryInstruction.CEI_Procedure = Constants.ProcedureCodes._31;
			entryLineFee.DefaultMethodOfPaymentIfEmpty();
			AssertEquals("CEI_Procedure different than 49", ZString.Empty, entryLineFee.CF_MethodOfPayment);
		});
	}

	public void TestDefaultMethodOfPaymentIfEmpty_R884()
	{
		var entryLineFee = GetFee(MessageTypeList.Codes.Import);
		var entryInstruction = entryLineFee.EntryLine.Header.EntryInstruction;
		var declaration = entryLineFee.EntryLine.Declaration;
		CombineAssertions(() =>
		{
			entryLineFee.CF_ChargeType = EU.Business.UniversalReferenceConstants.RefCusRateCodes.CustomsDutyOnIndustrialProducts;
			invoiceLine.AdditionalProcedureCodes.AddNew(Constants.ConcessionCodes.D05);
			var addInfo = invoiceLine.AdditionalInfos.AddNew();
			addInfo.CSI_Code = Constants.AdditionalInfoCodes._4PL07;
			entryInstruction.CEI_Procedure = Constants.ProcedureCodes._51;
			entryLineFee.CF_MethodOfPayment = ZString.Empty;
			entryLineFee.DefaultMethodOfPaymentIfEmpty();
			AssertEquals("R884 fulfilled - A00", PLMethodOfPaymentList.Codes.L, entryLineFee.CF_MethodOfPayment);
			entryLineFee.CF_MethodOfPayment = ZString.Empty;
			entryLineFee.CF_ChargeType = EU.Business.UniversalReferenceConstants.RefCusRateCodes.DefinitiveCountervailingDuty;
			entryLineFee.DefaultMethodOfPaymentIfEmpty();
			AssertEquals("R884 fulfilled - A40", PLMethodOfPaymentList.Codes.L, entryLineFee.CF_MethodOfPayment);
			entryLineFee.CF_MethodOfPayment = ZString.Empty;
			entryInstruction.CEI_Procedure = Constants.ProcedureCodes._42;
			entryLineFee.DefaultMethodOfPaymentIfEmpty();
			AssertEquals("CEI_Procedure different than 51 or 53", ZString.Empty, entryLineFee.CF_MethodOfPayment);
			entryInstruction.CEI_Procedure = Constants.ProcedureCodes._51;
			invoiceLine.AdditionalInfos.RemoveAll();
			entryLineFee.DefaultMethodOfPaymentIfEmpty();
			AssertEquals("No add info", ZString.Empty, entryLineFee.CF_MethodOfPayment);
			addInfo = invoiceLine.AdditionalInfos.AddNew();
			addInfo.CSI_Code = Constants.AdditionalInfoCodes._4PL07;
			entryInstruction.CEI_Procedure = Constants.ProcedureCodes._11;
			entryLineFee.DefaultMethodOfPaymentIfEmpty();
			AssertEquals("Other procedure code", ZString.Empty, entryLineFee.CF_MethodOfPayment);
			entryInstruction.CEI_Procedure = Constants.ProcedureCodes._51;
			invoiceLine.AdditionalProcedureCodes.RemoveAll();
			entryLineFee.DefaultMethodOfPaymentIfEmpty();
			AssertEquals("No CPCD", ZString.Empty, entryLineFee.CF_MethodOfPayment);
		});
	}

	public void TestAllowZeroOrEmptyAmountPL()
	{
		var entryLineFee = Factory.New<CusEntryLineFee>();
		Assert("Zero Amount is allowed", entryLineFee.AllowZeroOrEmptyAmount);
	}

	public void TestIncludeForVatCalculationCore()
	{
		const string rateCodeA00 = TaxTypeList.Codes.CustomsDuties;
		const string rateCodeA20 = TaxTypeList.Codes.AdditionalDuties;
		const string rateCodeA35 = TaxTypeList.Codes.ProvisionalAntidumpingDuties;
		const string rateCodeA45 = TaxTypeList.Codes.ProvisionalCountervailingDuties;
		const string rateCode1P1 = TaxTypeList.Codes.AdditionalDutiesSecurity;
		const string rateCode1S1 = TaxTypeList.Codes.Additional1P1TaxDuties;
		const string rateCode1T1 = TaxTypeList.Codes.GuaranteedCharges;

		new[] { rateCodeA00, rateCodeA20, rateCodeA35, rateCodeA45, rateCode1P1, rateCode1S1, rateCode1T1 }.ForEach(x =>
			CusRefRateCodeViewTestHelper.CreateAndSaveCusRateCodeForCountryIfNotExists(Factory, GlbCompany.CurrentCompany.GC_RN_NKCountryCode, "ADD", x));

		var declaration = Factory.New<JobDeclaration>();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();

		const decimal defaultAmount = 200m;

		var testCases = new (string, string, bool)[]
		{
			(rateCodeA00, PLMethodOfPaymentList.Codes.A, true),
			(rateCodeA00, PLMethodOfPaymentList.Codes.D, true),
			(rateCodeA00, PLMethodOfPaymentList.Codes.L, false),
			(rateCodeA00, PLMethodOfPaymentList.Codes.Z, false),

			(rateCodeA20, PLMethodOfPaymentList.Codes.A, true),
			(rateCodeA20, PLMethodOfPaymentList.Codes.D, true),
			(rateCodeA20, PLMethodOfPaymentList.Codes.L, false),
			(rateCodeA20, PLMethodOfPaymentList.Codes.Z, false),

			(rateCodeA45, PLMethodOfPaymentList.Codes.A, false),
			(rateCode1P1, PLMethodOfPaymentList.Codes.A, false),
			(rateCode1S1, PLMethodOfPaymentList.Codes.A, false),
			(rateCodeA35, PLMethodOfPaymentList.Codes.A, false),
			(rateCode1T1, PLMethodOfPaymentList.Codes.A, false),

			(rateCodeA35, PLMethodOfPaymentList.Codes.D, false),
			(rateCodeA45, PLMethodOfPaymentList.Codes.D, false),
			(rateCode1P1, PLMethodOfPaymentList.Codes.D, false),
			(rateCode1S1, PLMethodOfPaymentList.Codes.D, false),
			(rateCode1T1, PLMethodOfPaymentList.Codes.D, false),

			(rateCodeA35, PLMethodOfPaymentList.Codes.L, false),
			(rateCodeA45, PLMethodOfPaymentList.Codes.L, false),
			(rateCode1P1, PLMethodOfPaymentList.Codes.L, false),
			(rateCode1S1, PLMethodOfPaymentList.Codes.L, false),
			(rateCode1T1, PLMethodOfPaymentList.Codes.L, false),

			(rateCodeA35, PLMethodOfPaymentList.Codes.Z, false),
			(rateCodeA45, PLMethodOfPaymentList.Codes.Z, false),
			(rateCode1P1, PLMethodOfPaymentList.Codes.Z, false),
			(rateCode1S1, PLMethodOfPaymentList.Codes.Z, false),
			(rateCode1T1, PLMethodOfPaymentList.Codes.Z, false),
		};

		CombineAssertions(() =>
		{
			foreach (var (codeType, methodOfPayment, shouldBeIncludedForVatCalculation) in testCases)
			{
				var entryLine = entryHeader.AllEntryLines.AddNew();
				var fee = entryLine.Fees.AddNew();
				fee.CF_ChargeType = codeType;
				fee.CF_MethodOfPayment = methodOfPayment;
				fee.CF_ChargeAmount = defaultAmount;

				AssertEquals($"Customs Duty On {codeType}-{methodOfPayment} fee", defaultAmount, entryLine.Fees.GetAmount(codeType));

				var expectedVat = shouldBeIncludedForVatCalculation ? defaultAmount : 0m;
				AssertEquals($"DutyDetailsForVAT for {codeType}-{methodOfPayment}", expectedVat, entryLine.DutyDetailsForVAT);
			}
		});
	}

	protected override BusinessObject GetNewBusinessObjectForDefaultLightValidationTest() => GetFee();

	protected override int ExpectedCF_BaseValueDecimalPlaces => 4;

	protected CusEntryLineFee GetFee(string messageType = "")
	{
		var declaration = Factory.New<JobDeclaration>();

		if (!string.IsNullOrEmpty(messageType))
		{
			declaration.JE_MessageType = messageType;
		}

		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		var invoiceHeader = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
		invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
		invoiceLine.JI_CEI = entryInstruction.PK;
		var merger = new LineMerger(declaration);
		merger.DoMerge();
		var entryLine = declaration.ActiveEntryHeaders.Cast<CusEntryHeader>().First().MergedLines.Cast<CusEntryLine>()
			.First();
		var entryLineFee = entryLine.Fees.AddNew();
		return entryLineFee;
	}

	JobComInvoiceLine invoiceLine;
}
