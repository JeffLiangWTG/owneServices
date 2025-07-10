namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Input
{
	using CargoWise.Types;

	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.ABIStatementACHPaymentReroute)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.ABIStatementACHPaymentRerouteResponse)]
	[OutputBlock("QR")]
	public partial class DSTQR : MessageBlock, IStatementReroute
	{
		#region IStatementReroute Members

		ZDate IStatementReroute.TransmissionDate
		{
			get { return TransmissionDateOfStatementOrACHPaymentTransaction; }
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
			get { return ZString.Empty; }
		}

		ZString IStatementReroute.FinalDailyStatementRequest
		{
			get { return FinalStatementRequest; }
		}

		ZString IStatementReroute.FinalPeriodicMonthlyStatementRequest
		{
			get { return ZString.Empty; }
		}

		ZString IStatementReroute.ACHPaymentRequest
		{
			get { return ACHPaymentRequest; }
		}

		ZString IStatementReroute.PeriodicStatementPaymentAuthorizationRequest
		{
			get { return PeriodicStatementPaymentAuthorizationRequest; }
		}

		#endregion
	}
}