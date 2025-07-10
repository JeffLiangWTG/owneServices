using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.PL.MessageContracts.Interfaces.AES;
using CargoWise.EntityFramework;
using Enterprise.Customs.PL.Business.Declaration;
using static Enterprise.Customs.PL.Business.AesRuleHelper;

namespace Enterprise.Customs.PL.Business;

public class AESProcedureProvider : IProcedure
{
	public AESProcedureProvider(JobComInvoiceLine invoiceLine)
	{
		this.invoiceLine = Argument.NotNull(invoiceLine, nameof(invoiceLine));
	}

	readonly JobComInvoiceLine invoiceLine;

	public string RequestedProcedure => CachedValueHelper.GetValue(ref requestedProcedure, () => invoiceLine.EntryInstruction is CusEntryInstruction instruction ? instruction.CEI_Procedure : invoiceLine.ProcedureCodeBase);
	CachedValue<string> requestedProcedure;

	public string PreviousProcedure => invoiceLine.PreviousProcedureCode;

	public IReadOnlyCollection<IAdditionalProcedure> AdditionalProcedures => additionalProcedures ??= invoiceLine.ConcessionCodes
		.OrderBy(x => RuleR0093E.GetKeyEuLessThanPl(x, RuleR0093E.Patterns.n1an2))
		.Select((x, i) => new AESAdditionalProcedureProvider(x.ToString(), i + 1))
		.ToArray<IAdditionalProcedure>();
	IReadOnlyCollection<IAdditionalProcedure> additionalProcedures;
}
