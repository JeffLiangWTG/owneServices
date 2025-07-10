using CargoWise.Types;

namespace Enterprise.MasterFiles.Business
{
	public interface IOutstandingAmountValidationWrapper
	{
		ZDecimal OutstandingAmount { get; }
		ZDecimal TotalAmount { get; }
		ZDecimal MatchLinkAmountSum { get; }
		bool ShouldCheckTransactionHeaderOutstandingAmount { get; }
		bool ShouldCheckMatchLinkOutstandingAmount { get; }
		string GetTransactionHeaderErrorMessage();
		string GetMatchLinkErrorMessage();
	}
}
