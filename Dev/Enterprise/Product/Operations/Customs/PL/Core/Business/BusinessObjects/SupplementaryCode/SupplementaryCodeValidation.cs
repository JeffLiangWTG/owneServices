using System.Linq;
using Enterprise.Customs.Business;
using Enterprise.Customs.PL.Business.Declaration;
using AdditionalProcedureCode = Enterprise.Customs.PL.Business.Declaration.AdditionalProcedureCode;

namespace Enterprise.Customs.PL.Business;

public class SupplementaryCodeValidation : EU.Business.SupplementaryCodeValidation
{
	public SupplementaryCodeValidation(BaseSupplementaryCode parent) : base(parent)
	{
	}

	protected override void CheckCY_Code()
	{
		base.CheckCY_Code();

		if (!Parent.CY_Code.IsEmpty)
		{
			CheckRuleR1045();
		}
	}

	void CheckRuleR1045()
	{
		if (Parent.SupplementaryCodeSupporter is JobComInvoiceLine invoiceLine
			&& invoiceLine.IsImport
			&& invoiceLine.AdditionalProcedureCodes.Cast<AdditionalProcedureCode>()
				.Any(x => x.CY_Code == Constants.ConcessionCodes._2PL))
		{
			Parent.CY_CodeInfo.AddMessageError(Res.GetString("PLImportSupplementaryCodeValidation|R1045", "(R1045) Additional Sup.Codes are not allowed with procedure details code 2PL."));
		}
	}
}
