using static Enterprise.Customs.PL.Business.Constants;

namespace Enterprise.Customs.PL.Business.Declaration;

public class AdditionalProcedureCodeValidation(AdditionalProcedureCode parent) : EU.Business.AdditionalProcedureCodeValidation(parent)
{
	protected override void CheckCY_Code()
	{
		base.CheckCY_Code();

		CheckB0001E();
	}

	protected new AdditionalProcedureCode Parent => (AdditionalProcedureCode)base.Parent;

	protected override bool CY_CodeCheckFirst4Characters => false;

	void CheckB0001E()
	{
		if ((parent.ParentAsJobComInvoiceLine?.EntryInstruction?.IsAESTransitionPeriod() ?? false)
			&& parent.CY_Code == ConcessionCodes._1H2)
		{
			parent.CY_CodeInfo.AddMessageError(Res.GetString("AdditionalProcedureCodeValidation|RuleB0001E_MessageError",
				"[B0001E] \"1H2\" Additional Procedure code is not allowed during the transition period."));
		}
	}
}
