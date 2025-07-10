namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Input
{
	using CargoWise.Types;

	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.PeriodicMonthlyStatementReroute)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.PeriodicMonthlyStatementRerouteResponse)]
	[OutputBlock("QR")]
	public partial class PMSQR : MessageBlock, IStatementReroute
	{
		#region IStatementReroute Members

		ZDate IStatementReroute.TransmissionDate
		{
			get { return TransmissionDateOfStatement; }
		}

		ZString IStatementReroute.ImporterOfRecordNumber
		{
			get { return ImporterOfRecordNumber; }
		}

		ZString IStatementReroute.ClientBranch
		{
			get { return ClientBranch; }
		}

		ZString IStatementReroute.StatementNumber
		{
			get { return StatementNumber; }
		}

		ZString IStatementReroute.ScopeIndicator
		{
			get { return ScopeIndicator; }
		}

		ZString IStatementReroute.PreliminaryDailyStatementRequest
		{
			get { return PreliminaryStatementRequest; }
		}

		ZString IStatementReroute.PreliminaryPeriodicMonthlyStatementRequest
		{
			get { return PreliminaryPeriodicMonthlyStatementRequest; }
		}

		ZString IStatementReroute.FinalDailyStatementRequest
		{
			get { return FinalStatementRequest; }
		}

		ZString IStatementReroute.FinalPeriodicMonthlyStatementRequest
		{
			get { return FinalPeriodicMonthlyStatementRequest; }
		}

		ZString IStatementReroute.ACHPaymentRequest
		{
			get { return ZString.Empty; }
		}

		ZString IStatementReroute.PeriodicStatementPaymentAuthorizationRequest
		{
			get { return ZString.Empty; }
		}

		#endregion
	}
}