using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.Business
{
	public class GuaranteeTransactionCalculationResult
	{
		public GuaranteeTransactionCalculationResult(Money confirmedBalance, Money pendingBalance, Money totalBalance, ZDateTime calculationTimeUtc)
		{
			ConfirmedBalance = confirmedBalance;
			PendingBalance = pendingBalance;
			TotalBalance = totalBalance;
			CalculationTimeUtc = calculationTimeUtc;
		}

		public Money ConfirmedBalance { get; private set; }
		public Money PendingBalance { get; private set; }
		public Money TotalBalance { get; private set; }
		public ZDateTime CalculationTimeUtc { get; private set; }
	}
}
