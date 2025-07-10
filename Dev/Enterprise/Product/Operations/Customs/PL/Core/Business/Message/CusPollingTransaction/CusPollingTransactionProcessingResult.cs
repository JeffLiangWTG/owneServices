using CargoWise.Types;

namespace Enterprise.Customs.PL.Business;

public record CusPollingTransactionProcessingResult(ZInt CreatedBacklogs, ZInt UpdatedTransactions, ZInt MessagesSent)
{
	public ZBool HasChanges => !CreatedBacklogs.IsEmpty || !UpdatedTransactions.IsEmpty || !MessagesSent.IsEmpty;
}
