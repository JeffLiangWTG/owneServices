using System.Linq;
using CargoWise.Types;
using static Enterprise.Customs.PL.Business.Constants;

namespace Enterprise.Customs.PL.Business.Declaration;

public class ImportCusEntryLineFeeValidation : CusEntryLineFeeValidation
{
	public ImportCusEntryLineFeeValidation(CusEntryLineFee parent) : base(parent)
	{
	}

	protected override void CheckCF_MethodOfPayment()
	{
		base.CheckCF_MethodOfPayment();

		CheckCF_MethodOfPayment_ForSpecificMethodOfPayment();

		CheckRuleR245();
		CheckRuleR246();
		CheckRuleR247();
		CheckRuleR296();
		CheckRuleR827ToR830();
		CheckRuleR832();
		CheckRuleR881();
		CheckRuleR884();
		CheckRuleR885();
		CheckRuleR890();
		CheckRuleR1002();
		CheckRuleR1004();
		CheckRuleR1530();
		CheckRuleR1538();
	}

	void CheckCF_MethodOfPayment_ForSpecificMethodOfPayment()
	{
		var methodOfPayment = Parent.CF_MethodOfPayment;
		if (!methodOfPayment.IsEmpty)
		{
			switch (methodOfPayment)
			{
				case PLMethodOfPaymentList.Codes.A:
					CheckRuleR835();
					break;
				case PLMethodOfPaymentList.Codes.D:
					CheckRuleR277();
					CheckRuleR836();
					CheckRuleR891();
					CheckRuleR1017();
					break;
				case PLMethodOfPaymentList.Codes.G:
					CheckRuleR257();
					CheckRuleR462();
					break;
				case PLMethodOfPaymentList.Codes.J:
					CheckRuleR840();
					CheckRuleR1009();
					break;
				case PLMethodOfPaymentList.Codes.L:
					CheckRuleR831();
					CheckRuleR891();
					break;
				case PLMethodOfPaymentList.Codes.R:
				case PLMethodOfPaymentList.Codes.E:
					CheckRuleR1010();
					break;
				case PLMethodOfPaymentList.Codes.Z:
					CheckRuleR250();
					CheckRuleR251();
					CheckRuleR252();
					CheckRuleR833();
					break;
			}
		}
	}

	void CheckRuleR1009()
	{
		if (Parent.EntryLine?.Header?.EntryInstruction is CusEntryInstruction instruction
			&& instruction.CEI_SubStyle != SubStyleCodes.A)
		{
			Parent.CF_MethodOfPaymentInfo.AddMessageError(Res.GetString("ImportCusEntryLineFeeValidation|CheckRuleR1009", "(R1009) Payment method 'J' is allowed only for standard declaration (Sub Style = 'A')."));
		}
	}

	void CheckRuleR462()
	{
		if (Parent.CF_ChargeType != EU.Business.UniversalReferenceConstants.RefCusRateCodes.Vat)
		{
			Parent.CF_MethodOfPaymentInfo.AddMessageError(Res.GetString("ImportCusEntryLineFeeValidation|CheckRuleR462",
				"(R462) - Method of Payment code G is allowed for B00 (VAT) charge type only."));
		}
	}

	void CheckRuleR1002()
	{
		var methodOfPayment = Parent.CF_MethodOfPayment;
		var chargeType = Parent.CF_ChargeType;

		if (!methodOfPayment.IsEmpty
			&& (chargeType == EU.Business.UniversalReferenceConstants.RefCusRateCodes.ProvisionalAntiDumpingDuty
				|| chargeType == EU.Business.UniversalReferenceConstants.RefCusRateCodes.ProvisionalCountervailingDuty)
			&& methodOfPayment != PLMethodOfPaymentList.Codes.D
			&& methodOfPayment != PLMethodOfPaymentList.Codes.L
			&& methodOfPayment != PLMethodOfPaymentList.Codes.Z)
		{
			Parent.CF_MethodOfPaymentInfo.AddMessageError(Res.GetString("ImportCusEntryLineFeeValidation|CheckRuleR1002",
				"(R1002) - Method of Payment code D or L or Z is required for the charge type A35/A45."));
		}
	}

	void CheckRuleR1004()
	{
		if (!Parent.CF_MethodOfPayment.IsEmpty)
		{
			var allPaymentDistinctMethods = Parent.EntryLine.Fees.AllLineFees
				.Select(x => x.CF_MethodOfPayment)
				.Distinct().ToList();

			if (allPaymentDistinctMethods.Count > 1 && allPaymentDistinctMethods.Contains(PLMethodOfPaymentList.Codes.J))
			{
				Parent.CF_MethodOfPaymentInfo.AddMessageError(Res.GetString("ImportCusEntryLineFeeValidation|CheckRuleR1004",
					"(R1004) - Method of Payment code J is required for all charges for the Entry."));
			}
		}
	}

	void CheckRuleR1010()
	{
		if (Parent.CF_ChargeAmount <= 0M)
		{
			Parent.CF_MethodOfPaymentInfo.AddMessageError(Res.GetString("ImportCusEntryLineFeeValidation|CheckRuleR1010",
				"(R1010) - invalid method of payment. For R and E payment methods the Total amount must be > 0."));
		}
	}

	void CheckRuleR1017()
	{
		if (!CheckChargeTypeRuleR1010(Parent.CF_ChargeType)
			&& Parent.EntryLine?.Header?.EntryInstruction is CusEntryInstruction instruction
			&& CheckProcedureRuleR1010(instruction))
		{
			Parent.CF_MethodOfPaymentInfo.AddMessageError(Res.GetString("ImportCusEntryLineFeeValidation|CheckRuleR1017",
				"(R1017) - Invalid method of payment for the charge type."));
		}
	}

	void CheckRuleR1530()
	{
		var methodOfPayment = Parent.CF_MethodOfPayment;

		if (!methodOfPayment.IsEmpty && !CheckMethodOfPaymentRuleR1530(methodOfPayment))
		{
			Parent.CF_MethodOfPaymentInfo.AddMessageError(Res.GetString("ImportCusEntryLineFeeValidation|CheckRuleR1530",
				"(R1530) - Invalid method of payment.  It’s allowed to use only A,D,E G,H,J,L,R,Z."));
		}
	}

	void CheckRuleR1538()
	{
		var methodOfPayment = Parent.CF_MethodOfPayment;

		if (!methodOfPayment.IsEmpty && methodOfPayment != PLMethodOfPaymentList.Codes.L
									&& Parent.CF_ChargeType == TaxTypeList.Codes.ExciseTax
									&& HasAdditionalInformationCode(AdditionalInfoCodes._4PL10))
		{
			Parent.CF_MethodOfPaymentInfo.AddMessageError(Res.GetString("ImportCusEntryLineFeeValidation|CheckRuleR1538",
				"(R1538) - Method of Payment code L is required for 1A1 charge type."));
		}
	}

	void CheckRuleR245()
	{
		if (Parent.CF_MethodOfPayment != PLMethodOfPaymentList.Codes.Z
			&& Parent.CF_ChargeType == EU.Business.UniversalReferenceConstants.RefCusRateCodes.CustomsDutyOnIndustrialProducts
			&& CusEntryLineFeeMethodOfPaymentHelper.CheckRuleR245(Parent))
		{
			Parent.CF_MethodOfPaymentInfo.AddMessageError(Res.GetString("ImportCusEntryLineFeeValidation|CheckRuleR245", "(R245) - Method of Payment code Z if required."));
		}
	}

	void CheckRuleR246()
	{
		if (Parent.CF_MethodOfPayment != PLMethodOfPaymentList.Codes.Z
			&& Parent.CF_ChargeType == EU.Business.UniversalReferenceConstants.RefCusRateCodes.Vat
			&& CusEntryLineFeeMethodOfPaymentHelper.CheckRuleR246(Parent))
		{
			Parent.CF_MethodOfPaymentInfo.AddMessageError(Res.GetString("ImportCusEntryLineFeeValidation|CheckRuleR246", "(R246) - Method of Payment code Z if required."));
		}
	}

	void CheckRuleR247()
	{
		if (Parent.CF_MethodOfPayment != PLMethodOfPaymentList.Codes.Z
			&& Parent.CF_ChargeType == TaxTypeList.Codes.ExciseTax
			&& CusEntryLineFeeMethodOfPaymentHelper.CheckRuleR247(Parent))
		{
			Parent.CF_MethodOfPaymentInfo.AddMessageError(Res.GetString("ImportCusEntryLineFeeValidation|CheckRuleR247", "(R247) - Method of Payment code Z if required."));
		}
	}

	void CheckRuleR250()
	{
		if (Parent.CF_ChargeType == EU.Business.UniversalReferenceConstants.RefCusRateCodes.Vat
			&& Parent.EntryLine is CusEntryLine entryLine
			&& entryLine.Header?.EntryInstruction is CusEntryInstruction instruction
			&& CusEntryLineFeeMethodOfPaymentHelper.CheckProcedureRulesR245R246R247R250R251R252(instruction)
			&& !entryLine.RandomLine.ConcessionCodes.Any(CusEntryLineFeeMethodOfPaymentHelper.CheckRulesR246R250_CY_Code))
		{
			Parent.CF_MethodOfPaymentInfo.AddMessageError(Res.GetString("ImportCusEntryLineFeeValidation|CheckRuleR250", "(R250) - Invalid Method of Payment code Z for CPCD."));
		}
	}

	void CheckRuleR251()
	{
		if (Parent.CF_ChargeType == EU.Business.UniversalReferenceConstants.RefCusRateCodes.CustomsDutyOnIndustrialProducts
			&& Parent.EntryLine is CusEntryLine entryLine
			&& entryLine.Header?.EntryInstruction is CusEntryInstruction instruction
			&& CusEntryLineFeeMethodOfPaymentHelper.CheckProcedureRulesR245R246R247R250R251R252(instruction)
			&& !entryLine.RandomLine.ConcessionCodes.Any(CusEntryLineFeeMethodOfPaymentHelper.CheckRulesR245R251_CY_Code))
		{
			Parent.CF_MethodOfPaymentInfo.AddMessageError(Res.GetString("ImportCusEntryLineFeeValidation|CheckRuleR251", "(R251) - Invalid Method of Payment code Z for CPCD."));
		}
	}

	void CheckRuleR252()
	{
		if (Parent.CF_ChargeType == TaxTypeList.Codes.ExciseTax
			&& Parent.EntryLine is CusEntryLine entryLine
			&& entryLine.Header?.EntryInstruction is CusEntryInstruction instruction
			&& CusEntryLineFeeMethodOfPaymentHelper.CheckProcedureRulesR245R246R247R250R251R252(instruction)
			&& !entryLine.RandomLine.ConcessionCodes.Any(CusEntryLineFeeMethodOfPaymentHelper.CheckRulesR247R252_CY_Code))
		{
			Parent.CF_MethodOfPaymentInfo.AddMessageError(Res.GetString("ImportCusEntryLineFeeValidation|CheckRuleR252", "(R252) - Invalid Method of Payment code Z for CPCD."));
		}
	}

	void CheckRuleR827ToR830()
	{
		if (Parent.EntryLine is CusEntryLine entryLine
			&& entryLine.Header?.EntryInstruction is CusEntryInstruction instruction
			&& IsPaymentMethodEOrGOrHOrJ()
			&& (instruction.CEI_Procedure == ProcedureCodes._51 || IsProcedureCode53WithConcessionCodeD01ToD30()))
		{
			Parent.CF_MethodOfPaymentInfo.AddMessageError(Res.GetString("ImportCusEntryLineFeeValidation|CheckRuleR827ToR830",
				"(R827...R830) – the {0} is invalid. E, G, H, J codes are not allowed.", Parent.CF_MethodOfPaymentInfo.HumanReadableName));
		}

		bool IsPaymentMethodEOrGOrHOrJ()
		{
			var methodOfPayment = Parent.CF_MethodOfPayment;

			return methodOfPayment == PLMethodOfPaymentList.Codes.E
					|| methodOfPayment == PLMethodOfPaymentList.Codes.G
					|| methodOfPayment == PLMethodOfPaymentList.Codes.H
					|| methodOfPayment == PLMethodOfPaymentList.Codes.J;
		}

		bool IsProcedureCode53WithConcessionCodeD01ToD30() => instruction.CEI_Procedure == ProcedureCodes._53
															&& entryLine.RandomLine.ConcessionCodes
																.Any(x => IsConcessionCodeD01To30(x));
	}

	bool IsConcessionCodeD01To30(ZString concessionCode) =>
		concessionCode == ConcessionCodes.D01 || concessionCode == ConcessionCodes.D02 || concessionCode == ConcessionCodes.D03 || concessionCode == ConcessionCodes.D04 || concessionCode == ConcessionCodes.D05 ||
		concessionCode == ConcessionCodes.D06 || concessionCode == ConcessionCodes.D07 || concessionCode == ConcessionCodes.D08 || concessionCode == ConcessionCodes.D09 || concessionCode == ConcessionCodes.D10 ||
		concessionCode == ConcessionCodes.D11 || concessionCode == ConcessionCodes.D12 || concessionCode == ConcessionCodes.D13 || concessionCode == ConcessionCodes.D14 || concessionCode == ConcessionCodes.D15 ||
		concessionCode == ConcessionCodes.D16 || concessionCode == ConcessionCodes.D17 || concessionCode == ConcessionCodes.D18 || concessionCode == ConcessionCodes.D19 || concessionCode == ConcessionCodes.D20 ||
		concessionCode == ConcessionCodes.D21 || concessionCode == ConcessionCodes.D22 || concessionCode == ConcessionCodes.D23 || concessionCode == ConcessionCodes.D24 || concessionCode == ConcessionCodes.D25 ||
		concessionCode == ConcessionCodes.D26 || concessionCode == ConcessionCodes.D27 || concessionCode == ConcessionCodes.D28 || concessionCode == ConcessionCodes.D29 || concessionCode == ConcessionCodes.D30;

	void CheckRuleR881()
	{
		var methodOfPayment = Parent.CF_MethodOfPayment;

		if (!methodOfPayment.IsEmpty
			&& Parent.EntryLine is CusEntryLine entryLine
			&& entryLine.Header?.EntryInstruction is CusEntryInstruction instruction
			&& (instruction.CEI_Procedure == ProcedureCodes._51
				|| instruction.CEI_Procedure == ProcedureCodes._53))
		{
			var additionalCodeExists = entryLine.AdditionalInfos
											.Any(x => x.CSI_Code == AdditionalInfoCodes._4PL07)
										|| entryLine.Declaration is JobDeclaration declaration
										&& declaration.AdditionalInfos
											.Cast<AdditionalInfo>()
											.Any(x => x.CSI_Code == AdditionalInfoCodes._4PL07);

			var a00ChargeType = Parent.CF_ChargeType == EU.Business.UniversalReferenceConstants.RefCusRateCodes.CustomsDutyOnIndustrialProducts;

			if (methodOfPayment != PLMethodOfPaymentList.Codes.L
				&& a00ChargeType
				&& additionalCodeExists)
			{
				Parent.CF_MethodOfPaymentInfo.AddMessageError(Res.GetString("ImportCusEntryLineFeeValidation|CheckRuleR881LRequired",
					"(R881.1) – Method of Payment code L is required with 4PL07 Additional Information code."));
			}

			if (methodOfPayment == PLMethodOfPaymentList.Codes.L
				&& a00ChargeType
				&& !additionalCodeExists)
			{
				Parent.CF_MethodOfPaymentInfo.AddMessageError(Res.GetString("ImportCusEntryLineFeeValidation|CheckRuleR881LForbidden",
					"(R881.2) – Method of Payment code L is invalid without 4PL07 Additional Information code."));
			}

			if (!a00ChargeType
				&& methodOfPayment != PLMethodOfPaymentList.Codes.L
				&& methodOfPayment != PLMethodOfPaymentList.Codes.D
				&& !additionalCodeExists
				&& !entryLine.RandomLine.ConcessionCodes.Any(x => x == ConcessionCodes.D51))
			{
				Parent.CF_MethodOfPaymentInfo.AddMessageError(Res.GetString("ImportCusEntryLineFeeValidation|CheckRuleR881LDRequired",
					"(R881.3) – Method of Payment code L or D is required without 4PL07 Additional Information code."));
			}
		}
	}

	void CheckRuleR884()
	{
		var methodOfPayment = Parent.CF_MethodOfPayment;
		if (!methodOfPayment.IsEmpty
			&& methodOfPayment != PLMethodOfPaymentList.Codes.L
			&& CusEntryLineFeeMethodOfPaymentHelper.CheckRuleR884(Parent))
		{
			Parent.CF_MethodOfPaymentInfo.AddMessageError(Res.GetString("ImportCusEntryLineFeeValidation|CheckRuleR884",
				"(R884) – Method of Payment code L is required."));
		}
	}

	void CheckRuleR885()
	{
		var methodOfPayment = Parent.CF_MethodOfPayment;
		if (!methodOfPayment.IsEmpty
			&& methodOfPayment != PLMethodOfPaymentList.Codes.G
			&& Parent.CF_ChargeType == EU.Business.UniversalReferenceConstants.RefCusRateCodes.Vat
			&& CusEntryLineFeeMethodOfPaymentHelper.CheckRuleR885(Parent))
		{
			Parent.CF_MethodOfPaymentInfo.AddMessageError(Res.GetString("ImportCusEntryLineFeeValidation|CheckRuleR885",
				"(R885) If fiscal role code FR7 is specified then payment method G is required for fee type B00 (VAT)."));
		}
	}

	void CheckRuleR890()
	{
		var methodOfPayment = Parent.CF_MethodOfPayment;
		if (!methodOfPayment.IsEmpty
			&& methodOfPayment != PLMethodOfPaymentList.Codes.L
			&& methodOfPayment != PLMethodOfPaymentList.Codes.D
			&& Parent.CF_ChargeType == EU.Business.UniversalReferenceConstants.RefCusRateCodes.CustomsDutyOnIndustrialProducts
			&& Parent.EntryLine is CusEntryLine entryLine
			&& entryLine.Header?.EntryInstruction is CusEntryInstruction instruction
			&& instruction.CEI_Procedure == ProcedureCodes._53
			&& entryLine.RandomLine.ConcessionCodes
				.Any(x => x == ConcessionCodes.D51))
		{
			Parent.CF_MethodOfPaymentInfo.AddMessageError(Res.GetString("ImportCusEntryLineFeeValidation|CheckRuleR890",
				"(R890) – Method of Payment code L or D is required for A00 charge type."));
		}
	}

	void CheckRuleR891()
	{
		if (Parent.CF_ChargeType == EU.Business.UniversalReferenceConstants.RefCusRateCodes.Vat
			&& Parent.EntryLine is CusEntryLine entryLine
			&& entryLine.Header?.EntryInstruction is CusEntryInstruction instruction
			&& instruction.CEI_Procedure == ProcedureCodes._53
			&& entryLine.RandomLine.ConcessionCodes
				.Any(x => x == ConcessionCodes.D51))
		{
			Parent.CF_MethodOfPaymentInfo.AddMessageError(Res.GetString("ImportCusEntryLineFeeValidation|CheckRuleR891",
				"(R891) – Method of Payment code L and D is invalid for B00 charge type."));
		}
	}

	static bool CheckMethodOfPaymentRuleR1530(ZString methodOfPayment)
	{
		return methodOfPayment == PLMethodOfPaymentList.Codes.A || methodOfPayment == PLMethodOfPaymentList.Codes.D ||
				methodOfPayment == PLMethodOfPaymentList.Codes.E || methodOfPayment == PLMethodOfPaymentList.Codes.G ||
				methodOfPayment == PLMethodOfPaymentList.Codes.H || methodOfPayment == PLMethodOfPaymentList.Codes.J ||
				methodOfPayment == PLMethodOfPaymentList.Codes.L || methodOfPayment == PLMethodOfPaymentList.Codes.R ||
				methodOfPayment == PLMethodOfPaymentList.Codes.Z;
	}

	static bool CheckChargeTypeRuleR1010(ZString chargeType)
	{
		return chargeType == TaxTypeList.Codes.AdditionalDutiesSecurity || chargeType == TaxTypeList.Codes.Additional1P1TaxDuties || chargeType == TaxTypeList.Codes.GuaranteedCharges ||
				chargeType == EU.Business.UniversalReferenceConstants.RefCusRateCodes.ProvisionalAntiDumpingDuty ||
				chargeType == EU.Business.UniversalReferenceConstants.RefCusRateCodes.ProvisionalCountervailingDuty;
	}

	static bool CheckProcedureRuleR1010(CusEntryInstruction instruction)
	{
		return instruction.CEI_Procedure == ProcedureCodes._40 || instruction.CEI_Procedure == ProcedureCodes._42 ||
				instruction.CEI_Procedure == ProcedureCodes._45 || instruction.CEI_Procedure == ProcedureCodes._49 ||
				instruction.CEI_Procedure == ProcedureCodes._61 || instruction.CEI_Procedure == ProcedureCodes._63 ||
				instruction.CEI_Procedure == ProcedureCodes._68;
	}

	void CheckRuleR296()
	{
		if (Parent.CF_ChargeType == TaxTypeList.Codes.ExciseTax
			&& Parent.CF_MethodOfPayment != PLMethodOfPaymentList.Codes.L
			&& CusEntryLineFeeMethodOfPaymentHelper.CheckRuleR296(Parent))
		{
			Parent.CF_MethodOfPaymentInfo.AddMessageError(Res.GetString("ImportCusEntryLineFeeValidation|CheckRuleR296", "(R296) – Method of Payment code L is required for charge type 1A1."));
		}
	}

	void CheckRuleR277()
	{
		if (NotDTypeMethodOfPaymentExists()
			&& (IsRuleR277ProcedureCode()
				|| (Parent.EntryLine is CusEntryLine entryLine
					&& (entryLine.Header?.EntryInstruction is CusEntryInstruction instruction
						&& instruction.CEI_Procedure == ProcedureCodes._53)
					&& entryLine.RandomLine.ConcessionCodes
						.Any(x => x == ConcessionCodes.D51))))
		{
			Parent.CF_MethodOfPaymentInfo.AddMessageError(Res.GetString("ImportCusEntryLineFeeValidation|CheckRuleR277", "(R277) For the selected customs procedure code MoP=’D’ must be mixed with other Method of Payment code."));
		}

		bool IsRuleR277ProcedureCode()
		{
			var procedureCode = Parent.EntryLine?.Header?.EntryInstruction?.CEI_Procedure ?? ZString.Empty;
			return procedureCode != ZString.Empty
					&& (procedureCode == ProcedureCodes._40
						|| procedureCode == ProcedureCodes._42
						|| procedureCode == ProcedureCodes._45
						|| procedureCode == ProcedureCodes._61
						|| procedureCode == ProcedureCodes._63
						|| procedureCode == ProcedureCodes._68);
		}

		bool NotDTypeMethodOfPaymentExists() => !Parent.EntryLine.Fees.Cast<CusEntryLineFee>()
			.Any(x => !x.CF_MethodOfPayment.IsEmpty
					&& x.CF_MethodOfPayment != PLMethodOfPaymentList.Codes.D);
	}

	void CheckRuleR257()
	{
		var parent = Parent;
		if (parent.CF_ChargeType == EU.Business.UniversalReferenceConstants.RefCusRateCodes.Vat
			&& parent.CF_MethodOfPayment == PLMethodOfPaymentList.Codes.G
			&& parent.EntryLine?.FiscalReferencesCombined.All(x => x.CFR_Code != FiscalReferenceCodeList.Codes.FR7_Taxpayer) != false)
		{
			parent.CF_MethodOfPaymentInfo.AddMessageError(Res.GetString("ImportCusEntryLineFeeValidation|CheckRuleR257", "(R257.2) – For the Method of Payment code G (art.33a VAT) the fiscal role indicator FR7 is required."));
		}
	}

	void CheckRuleR831()
	{
		if (Parent.CF_ChargeType == ChargeTypes.A00
			&& CheckR831AndR833AndR835ProcedureCodeAndProcedureDetail()
			&& !HasInvoiceLineAdditionalInformationCode4PL07())
		{
			Parent.CF_MethodOfPaymentInfo.AddMessageError(Res.GetString("ImportCusEntryLineFeeValidation|CheckRuleR831", "(R831) For the selected requested procedure code the duty (A00) method of payment code L is only allowed with the additional information code 4PL07."));
		}
	}

	bool HasInvoiceLineAdditionalInformationCode4PL07() => HasAdditionalInformationCode(Constants.AdditionalInfoCodes._4PL07);

	void CheckRuleR833()
	{
		if (CheckR831AndR833AndR835ProcedureCodeAndProcedureDetail())
		{
			Parent.CF_MethodOfPaymentInfo.AddMessageError(Res.GetString("ImportCusEntryLineFeeValidation|CheckRuleR833", "(R833) For the selected requested procedure code the method of payment code for Z is not allowed."));
		}
	}

	void CheckRuleR835()
	{
		if (CheckR831AndR833AndR835ProcedureCodeAndProcedureDetail())
		{
			Parent.CF_MethodOfPaymentInfo.AddMessageError(Res.GetString("ImportCusEntryLineFeeValidation|CheckRuleR835", "(R835) For the selected requested procedure code the method of payment code for A is not allowed."));
		}
	}

	bool CheckR831AndR833AndR835ProcedureCodeAndProcedureDetail()
	{
		var entryLine = Parent.EntryLine;
		var procedureCode = entryLine.Header?.EntryInstruction?.CEI_Procedure ?? ZString.Empty;

		return procedureCode == ProcedureCodes._51
				|| (procedureCode == ProcedureCodes._53
					&& entryLine.RandomLine is JobComInvoiceLine invoiceLine
					&& invoiceLine.ConcessionCodes
						.Any(x => IsConcessionCodeD01To30(x)));
	}

	void CheckRuleR832()
	{
		if (Parent.EntryLine.RandomLine is JobComInvoiceLine invoiceLine
			&& (invoiceLine.EntryInstruction?.CEI_Procedure.Equals(ProcedureCodes._48) ?? false)
			&& ((Parent.CF_MethodOfPayment == PLMethodOfPaymentList.Codes.D && Parent.CF_ChargeAmount.IsEmpty)
				|| (Parent.CF_MethodOfPayment != PLMethodOfPaymentList.Codes.D
					&& !Parent.CF_ChargeAmount.IsEmpty
					&& !HasInvoiceLineAdditionalInformationCode4PL07())
			))
		{
			Parent.CF_MethodOfPaymentInfo.AddMessageError(Res.GetString("ImportCusEntryLineFeeValidation|CheckRuleR832", "(R832) For the requested procedure code selected and the payment method used, the Total amount value is invalid or additional information code 4PL07 is missing."));
		}
	}

	void CheckRuleR836()
	{
		var parent = Parent;
		if (HasR840OrR836ProcedureCodeAndProcedureDetails()
			&& parent.CF_ChargeAmount.IsEmpty
			&& HasR836ChargeType())
		{
			Parent.CF_MethodOfPaymentInfo.AddMessageError(Res.GetString("ImportCusEntryLineFeeValidation|CheckRuleR836", "(R836) For the requested procedure code selected the payment method ’D’ and duty amount = 0 is not allowed."));
		}

		bool HasR836ChargeType()
		{
			var chargeType = parent.CF_ChargeType;

			return chargeType == ChargeTypes._1P1
					|| chargeType == ChargeTypes._1S1
					|| chargeType == ChargeTypes._1T1
					|| chargeType == ChargeTypes.A35
					|| chargeType == ChargeTypes.A45;
		}
	}

	void CheckRuleR840()
	{
		if (HasR840OrR836ProcedureCodeAndProcedureDetails()
			&& Parent.EntryLine.RandomLine is JobComInvoiceLine invoiceLine
			&& invoiceLine.Declaration.TransportMode != Core.Constants.TransportModes.Mail)
		{
			Parent.CF_MethodOfPaymentInfo.AddMessageError(Res.GetString("ImportCusEntryLineFeeValidation|CheckRuleR840", "(R840) For the requested procedure code and transport mode selected the payment method ’J’ is not allowed."));
		}
	}

	bool HasR840OrR836ProcedureCodeAndProcedureDetails()
	{
		var entryLine = Parent.EntryLine;
		var procedureCode = entryLine.RandomLine.EntryInstruction?.CEI_Procedure ?? ZString.Empty;

		return HasR840OrR836ProcedureCode()
				|| (procedureCode == ProcedureCodes._53
					&& entryLine.RandomLine.ConcessionCodes
						.Any(x => x == ConcessionCodes.D51));

		bool HasR840OrR836ProcedureCode()
		{
			return procedureCode == ProcedureCodes._40
					|| procedureCode == ProcedureCodes._42
					|| procedureCode == ProcedureCodes._44
					|| procedureCode == ProcedureCodes._45
					|| procedureCode == ProcedureCodes._61
					|| procedureCode == ProcedureCodes._63
					|| procedureCode == ProcedureCodes._68;
		}
	}

	bool HasAdditionalInformationCode(ZString csiCode) => Parent.EntryLine.RandomLine is JobComInvoiceLine invoiceLine
														&& (invoiceLine.HasAdditionalInfoCode(csiCode)
															|| invoiceLine.InvoiceHeader.HasAdditionalInfoCode(csiCode)
															|| invoiceLine.Declaration.HasAdditionalInfoCode(csiCode));
}
