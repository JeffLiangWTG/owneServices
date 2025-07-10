using CargoWise.Types;

namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks
{
	public interface IStatementReroute
	{
		ZDate TransmissionDate { get; }
		ZString ImporterOfRecordNumber { get; }
		ZString ClientBranch { get; }
		ZString StatementNumber { get; }
		ZString ScopeIndicator { get; }
		ZString PreliminaryDailyStatementRequest { get; }
		ZString PreliminaryPeriodicMonthlyStatementRequest { get; }
		ZString FinalDailyStatementRequest { get; }
		ZString FinalPeriodicMonthlyStatementRequest { get; }
		ZString ACHPaymentRequest { get; }
		ZString PeriodicStatementPaymentAuthorizationRequest { get; }
	}
}
