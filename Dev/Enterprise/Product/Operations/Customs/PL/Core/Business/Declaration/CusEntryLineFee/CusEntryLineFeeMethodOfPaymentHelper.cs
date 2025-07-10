using System.Linq;
using CargoWise.Types;
using static Enterprise.Customs.PL.Business.Constants;

namespace Enterprise.Customs.PL.Business.Declaration;

static class CusEntryLineFeeMethodOfPaymentHelper
{
	internal static bool CheckRuleR884(CusEntryLineFee entryLineFee)
	{
		var entryLine = entryLineFee?.EntryLine;
		return entryLine?.Header?.EntryInstruction is CusEntryInstruction instruction
				&& (instruction.CEI_Procedure == ProcedureCodes._51 || instruction.CEI_Procedure == ProcedureCodes._53)
				&& (
					entryLine.AdditionalInfos.Any(x => x.CSI_Code == AdditionalInfoCodes._4PL07)
					|| entryLine.Declaration is JobDeclaration declaration && declaration.AdditionalInfos.Cast<AdditionalInfo>().Any(x => x.CSI_Code == AdditionalInfoCodes._4PL07)
				)
				&& entryLine.InvoiceLines.Cast<JobComInvoiceLine>().SelectMany(x => x.ConcessionCodes).Any(CheckConcessionCodeRuleR884);
	}

	internal static bool CheckRuleR966(CusEntryLineFee entryLineFee)
	{
		var procedureCode = entryLineFee?.EntryLine?.Header?.EntryInstruction?.CEI_Procedure ?? ZString.Empty;
		return procedureCode == ProcedureCodes._49;
	}

	internal static bool CheckRuleR245(CusEntryLineFee entryLineFee)
	{
		var entryLine = entryLineFee?.EntryLine;
		return entryLine?.Header?.EntryInstruction is CusEntryInstruction instruction
				&& CheckProcedureRulesR245R246R247R250R251R252(instruction)
				&& entryLine.InvoiceLines.Cast<JobComInvoiceLine>().SelectMany(x => x.ConcessionCodes).Any(CheckRulesR245R251_CY_Code);
	}

	internal static bool CheckRuleR296(CusEntryLineFee entryLineFee)
	{
		var entryLine = entryLineFee?.EntryLine;
		var procedureCode = entryLine?.Header?.EntryInstruction?.CEI_Procedure ?? ZString.Empty;
		return procedureCode == ProcedureCodes._45
				|| procedureCode == ProcedureCodes._68
				|| procedureCode == ProcedureCodes._96
				|| (entryLine?.RandomLine.AdditionalProcedureCodes.Cast<AdditionalProcedureCode>().Any(x => x.CY_Code == ConcessionCodes.F06) ?? false);
	}

	internal static bool CheckRuleR247(CusEntryLineFee entryLineFee)
	{
		var entryLine = entryLineFee?.EntryLine;
		return entryLine?.Header?.EntryInstruction is CusEntryInstruction instruction
				&& CheckProcedureRulesR245R246R247R250R251R252(instruction)
				&& entryLine.InvoiceLines.Cast<JobComInvoiceLine>().SelectMany(x => x.ConcessionCodes).Any(CheckRulesR247R252_CY_Code);
	}

	internal static bool CheckRuleR1538(CusEntryLineFee entryLineFee)
	{
		return entryLineFee?.EntryLine is CusEntryLine entryLine
				&& (
					entryLine.AdditionalInfos.Any(x => x.CSI_Code == AdditionalInfoCodes._4PL10)
					|| entryLine.Declaration is JobDeclaration declaration && declaration.AdditionalInfos.Cast<AdditionalInfo>().Any(x => x.CSI_Code == AdditionalInfoCodes._4PL10)
				);
	}

	internal static bool CheckRuleR246(CusEntryLineFee entryLineFee)
	{
		var entryLine = entryLineFee?.EntryLine;
		return entryLine?.Header?.EntryInstruction is CusEntryInstruction instruction
				&& CheckProcedureRulesR245R246R247R250R251R252(instruction)
				&& entryLine.InvoiceLines.Cast<JobComInvoiceLine>().SelectMany(x => x.ConcessionCodes).Any(CheckRulesR246R250_CY_Code);
	}

	internal static bool CheckRuleR885(CusEntryLineFee entryLineFee)
		=> entryLineFee?.EntryLine?.FiscalReferencesCombined.Any(x => x.CFR_Code == FiscalReferenceCodeList.Codes.FR7_Taxpayer) == true;

	internal static bool CheckProcedureRulesR245R246R247R250R251R252(CusEntryInstruction instruction)
	{
		return instruction != null && (instruction.CEI_Procedure.StartsWith("4") || instruction.CEI_Procedure.StartsWith("6"));
	}

	internal static bool CheckRulesR246R250_CY_Code(ZString code)
	{
		return code == ConcessionCodes._0V1 || code == ConcessionCodes._0V2 || code == ConcessionCodes._0V3 || code == ConcessionCodes._0V4 ||
				code == ConcessionCodes._0V6 || code == ConcessionCodes._0V8 || code == ConcessionCodes._0V9 ||
				code == ConcessionCodes._1V0 || code == ConcessionCodes._1V1 || code == ConcessionCodes._1V5 || code == ConcessionCodes._1V6 ||
				code == ConcessionCodes._1V8 || code == ConcessionCodes._1V9 || code == ConcessionCodes._2V0 || code == ConcessionCodes._2V5 ||
				code == ConcessionCodes._2V6 || code == ConcessionCodes._2V7 || code == ConcessionCodes._2V8 || code == ConcessionCodes._2V9 ||
				code == ConcessionCodes._3V0 || code == ConcessionCodes._3V1 || code == ConcessionCodes._3V2 || code == ConcessionCodes._3V3 ||
				code == ConcessionCodes._3V4 || code == ConcessionCodes._3V5 || code == ConcessionCodes._3V6 || code == ConcessionCodes._3V7 ||
				code == ConcessionCodes._3V8 || code == ConcessionCodes._3V9 || code == ConcessionCodes._4V0 || code == ConcessionCodes._4V1 ||
				code == ConcessionCodes._4V2 || code == ConcessionCodes._4V3 || code == ConcessionCodes._4V4 || code == ConcessionCodes._4V5 ||
				code == ConcessionCodes._4V7 || code == ConcessionCodes._5V5 || code == ConcessionCodes.F48;
	}

	internal static bool CheckRulesR247R252_CY_Code(ZString code)
	{
		return code == ConcessionCodes._6A1 || code == ConcessionCodes._6A2 || code == ConcessionCodes._6A3 || code == ConcessionCodes._6A4 ||
				code == ConcessionCodes._6A5 || code == ConcessionCodes._6A6 || code == ConcessionCodes._6A7 || code == ConcessionCodes._6A8 ||
				code == ConcessionCodes._6A9 || code == ConcessionCodes._7A1 || code == ConcessionCodes._7A2 || code == ConcessionCodes._7A3 ||
				code == ConcessionCodes._7A4 || code == ConcessionCodes._7A5 || code == ConcessionCodes._7A6 || code == ConcessionCodes._7A7 ||
				code == ConcessionCodes._7A8 || code == ConcessionCodes._7A9 || code == ConcessionCodes._8A1 || code == ConcessionCodes._8A2 ||
				code == ConcessionCodes._8A3 || code == ConcessionCodes._8A4 || code == ConcessionCodes._8A8;
	}

	internal static bool CheckRulesR245R251_CY_Code(ZString code)
	{
		return code == ConcessionCodes.C01 || code == ConcessionCodes.C02 || code == ConcessionCodes.C03 || code == ConcessionCodes.C04 ||
				code == ConcessionCodes.C06 || code == ConcessionCodes.C07 || code == ConcessionCodes.C08 || code == ConcessionCodes.C09 ||
				code == ConcessionCodes.C10 || code == ConcessionCodes.C11 || code == ConcessionCodes.C12 || code == ConcessionCodes.C13 ||
				code == ConcessionCodes.C14 || code == ConcessionCodes.C15 || code == ConcessionCodes.C16 || code == ConcessionCodes.C17 ||
				code == ConcessionCodes.C18 || code == ConcessionCodes.C19 || code == ConcessionCodes.C20 || code == ConcessionCodes.C21 ||
				code == ConcessionCodes.C22 || code == ConcessionCodes.C23 || code == ConcessionCodes.C24 || code == ConcessionCodes.C25 ||
				code == ConcessionCodes.C26 || code == ConcessionCodes.C27 || code == ConcessionCodes.C28 || code == ConcessionCodes.C29 ||
				code == ConcessionCodes.C30 || code == ConcessionCodes.C31 || code == ConcessionCodes.C32 || code == ConcessionCodes.C33 ||
				code == ConcessionCodes.C34 || code == ConcessionCodes.C35 || code == ConcessionCodes.C36 || code == ConcessionCodes.C37 ||
				code == ConcessionCodes.C38 || code == ConcessionCodes.C39 || code == ConcessionCodes.C40 || code == ConcessionCodes.C41 ||
				code == ConcessionCodes.B02 || code == ConcessionCodes.B03 || code == ConcessionCodes.F01 || code == ConcessionCodes.F02 ||
				code == ConcessionCodes.F03 || code == ConcessionCodes.F21 || code == ConcessionCodes.F22 || code == ConcessionCodes._3PL ||
				code == ConcessionCodes._4PL || code == ConcessionCodes._5PL || code == ConcessionCodes._6PL || code == ConcessionCodes._7PL ||
				code == ConcessionCodes._1C1;
	}

	static bool CheckConcessionCodeRuleR884(ZString code)
	{
		return code == ConcessionCodes.D01 || code == ConcessionCodes.D02 || code == ConcessionCodes.D03 || code == ConcessionCodes.D04 ||
				code == ConcessionCodes.D05 || code == ConcessionCodes.D06 || code == ConcessionCodes.D07 || code == ConcessionCodes.D08 ||
				code == ConcessionCodes.D09 || code == ConcessionCodes.D10 || code == ConcessionCodes.D11 || code == ConcessionCodes.D12 ||
				code == ConcessionCodes.D13 || code == ConcessionCodes.D14 || code == ConcessionCodes.D15 || code == ConcessionCodes.D16 ||
				code == ConcessionCodes.D17 || code == ConcessionCodes.D18 || code == ConcessionCodes.D19 || code == ConcessionCodes.D20 ||
				code == ConcessionCodes.D21 || code == ConcessionCodes.D22 || code == ConcessionCodes.D23 || code == ConcessionCodes.D24 ||
				code == ConcessionCodes.D25 || code == ConcessionCodes.D26 || code == ConcessionCodes.D27 || code == ConcessionCodes.D28 ||
				code == ConcessionCodes.D29 || code == ConcessionCodes.D30;
	}
}
