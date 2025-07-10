using System.Linq;
using static Enterprise.Customs.PL.Business.Constants;

namespace Enterprise.Customs.PL.Business.Declaration;

public class ImportCusEntryLineValidation : CusEntryLineValidation
{
	public ImportCusEntryLineValidation(EU.Business.Declaration.CusEntryLine parent) : base(parent)
	{
	}

	public override void ValidateAll()
	{
		base.ValidateAll();
		CheckRuleR605();
	}

	void CheckRuleR605()
	{
		var invoiceLine = Parent.RandomLine;
		if (invoiceLine.EntryInstruction is CusEntryInstruction instruction)
		{
			var instructionProcedure = instruction.CEI_Procedure;
			var ruleIsValid = instructionProcedure == ProcedureCodes._71
							|| instructionProcedure == ProcedureCodes._76
							|| invoiceLine.ConcessionCodes.Any(concessionCode => concessionCode == ConcessionCodes._2PL)
							|| Parent.Fees.Count > 0;

			if (!ruleIsValid)
			{
				Parent.AddRowMessageError(Res.GetString($"PL{nameof(ImportCusEntryLineValidation)}|{nameof(CheckRuleR605)}", "(R605) Duty/Tax calculations are missing"));
			}
		}
	}
}
