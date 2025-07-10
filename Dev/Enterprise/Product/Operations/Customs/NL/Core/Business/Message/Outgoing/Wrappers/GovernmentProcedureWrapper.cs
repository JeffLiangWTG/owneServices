using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.NL.MessageContracts.Interfaces;
using Enterprise.Customs.NL.Business.Declaration;

namespace Enterprise.Customs.NL.Business;

public class GovernmentProcedureWrapper : IProcedure
{
	public GovernmentProcedureWrapper(JobComInvoiceLine invoiceLine)
	{
		this.invoiceLine = Argument.NotNull(invoiceLine, nameof(invoiceLine));
	}
	readonly JobComInvoiceLine invoiceLine;

	public string CurrentCode => invoiceLine.JI_Procedure.SubstringSafe(0, 2);
	public string PreviousCode => invoiceLine.JI_Procedure.SubstringSafe(2, 2);
	public IReadOnlyCollection<IAdditionalProcedure> AdditionalProcedures => additionalProcedures ??= GetAdditionalProcedures().ToArray();
	IReadOnlyCollection<IAdditionalProcedure> additionalProcedures;

	IEnumerable<IAdditionalProcedure> GetAdditionalProcedures()
	{
		var index = 1;
		if (!invoiceLine.JI_Procedure.SubstringSafe(4).IsEmpty)
		{
			yield return new AdditionalProcedureWrapper(invoiceLine.JI_Procedure, index++);
		}
		foreach (EU.Business.AdditionalProcedureCode addProcCode in invoiceLine.AdditionalProcedureCodes.Cast<EU.Business.AdditionalProcedureCode>())
		{
			if (!addProcCode.CY_Code.SubstringSafe(4).IsEmpty)
			{
				yield return new AdditionalProcedureWrapper(addProcCode.CY_Code, index++);
			}
		}
	}
}
