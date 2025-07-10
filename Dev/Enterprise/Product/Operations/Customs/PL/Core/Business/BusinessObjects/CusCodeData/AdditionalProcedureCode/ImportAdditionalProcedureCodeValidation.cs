using System.Linq;
using CargoWise.Types;

namespace Enterprise.Customs.PL.Business.Declaration;

public class ImportAdditionalProcedureCodeValidation : AdditionalProcedureCodeValidation
{
	public ImportAdditionalProcedureCodeValidation(AdditionalProcedureCode parent) : base(parent)
	{
	}

	JobComInvoiceLine ParentAsJobComInvoiceLine => Parent.ParentAsJobComInvoiceLine;

	ZString Tariff => ParentAsJobComInvoiceLine?.JI_Tariff.SubstringSafe(0, 4) ?? ZString.Empty;

	protected override void CheckCY_Code()
	{
		base.CheckCY_Code();

		var invoiceLine = ParentAsJobComInvoiceLine;
		if (invoiceLine != null)
		{
			var procedureCode = invoiceLine.ProcedureCodeBase;
			var previousProcedureCode = invoiceLine.PreviousProcedureCode;
			var concessionCode = Parent.CY_Code;
			ProcedureCodesHelper.CheckForRuleR407(procedureCode, concessionCode, Parent.CY_CodeInfo);
			ProcedureCodesHelper.CheckForRuleR414(procedureCode, concessionCode, Parent.CY_CodeInfo);
			var tariff = Tariff;
			ProcedureCodesHelper.CheckForRuleR419(concessionCode, Parent.CY_CodeInfo, tariff);
			ProcedureCodesHelper.CheckForRuleR421(concessionCode, Parent.CY_CodeInfo, tariff);
			ProcedureCodesHelper.CheckForRuleR424(procedureCode, concessionCode, Parent.CY_CodeInfo);
			ProcedureCodesHelper.CheckForRuleR480(procedureCode, concessionCode, Parent.CY_CodeInfo);
			ProcedureCodesHelper.CheckForRuleR859(procedureCode, concessionCode, Parent.CY_CodeInfo);
			ProcedureCodesHelper.CheckForRuleR975(previousProcedureCode, concessionCode, Parent.CY_CodeInfo);
			ProcedureCodesHelper.CheckForRuleR1049(procedureCode, concessionCode, Parent.CY_CodeInfo, invoiceLine);

			CheckRuleR1583();
			CheckRuleR1584();
			CheckRuleR1590();
		}
	}

	void CheckRuleR1583()
	{
		if (Parent.CY_Code == Constants.ConcessionCodes._2PL
			&& !ParentAsJobComInvoiceLine.AdditionalProcedureCodes
				.Cast<AdditionalProcedureCode>()
				.Any(x => x != Parent
						&& R1583ConcessionCodeExist(x.CY_Code)))
		{
			Parent.CY_CodeInfo.AddMessageError(Res.GetString("PLImportAdditionalProcedureCodeValidation|R1583", "(R1583) If a 2PL additional procedure code is specified, one of procedure codes C30, C31, C32, C34, C35, C36 is also required."));
		}

		bool R1583ConcessionCodeExist(ZString concessionCode) => concessionCode == Constants.ConcessionCodes.C30
																|| concessionCode == Constants.ConcessionCodes.C31
																|| concessionCode == Constants.ConcessionCodes.C32
																|| concessionCode == Constants.ConcessionCodes.C34
																|| concessionCode == Constants.ConcessionCodes.C35
																|| concessionCode == Constants.ConcessionCodes.C36;
	}

	void CheckRuleR1584()
	{
		if (IsR1584ConcessionCode(Parent.CY_Code)
			&& ParentAsJobComInvoiceLine.AdditionalProcedureCodes
				.Cast<AdditionalProcedureCode>()
				.Any(x => x != Parent
						&& x.CY_Code == Constants.ConcessionCodes._2PL))
		{
			Parent.CY_CodeInfo.AddMessageError(Res.GetString("PLImportAdditionalProcedureCodeValidation|R1584", "(R1584) If a 2PL additional procedure code is specified, procedure codes C07 and C08 are not allowed."));
		}

		bool IsR1584ConcessionCode(ZString concessionCode) => concessionCode == Constants.ConcessionCodes.C07
															|| concessionCode == Constants.ConcessionCodes.C08;
	}

	void CheckRuleR1590()
	{
		var parent = Parent;

		if (parent.CY_Code == Constants.ConcessionCodes.F48
			&& ParentAsJobComInvoiceLine is JobComInvoiceLine invoiceLine
			&& !invoiceLine.AdditionalProcedureCodes.Cast<AdditionalProcedureCode>().Any(x => x.CY_Code == Constants.ConcessionCodes.C07)
			&& !invoiceLine.FiscalReferences.Cast<CusFiscalReference>().Any(x => x.CFR_Code == FiscalReferenceCodeList.Codes.FR5_Vendor)
			&& (!invoiceLine.EntryInstruction?.FiscalReferences.Cast<CusFiscalReference>().Any(x => x.CFR_Code == FiscalReferenceCodeList.Codes.FR5_Vendor) ?? true))
		{
			parent.CY_CodeInfo.AddMessageError(Res.GetString("PLImportAdditionalProcedureCodeValidation|R1590", "(R1590) Procedure details code C07 or Fiscal role code FR5 is required."));
		}
	}
}
