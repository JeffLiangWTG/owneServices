using CargoWise.Types;

namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Output
{
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.PeriodicDailyStatementACHDebitAuthorizationEntrySummaryPresentationResponse)]
	public partial class APDSE0 : MessageBlock
	{
	}

	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.PeriodicDailyStatementACHDebitAuthorizationEntrySummaryPresentationResponse)]
	public partial class APDSE1 : MessageBlock, IPaymentAuthorisationResponse
	{
		#region IPaymentAuthorisationResponse Members

		ZString IPaymentAuthorisationResponse.StatementBillNumber
		{
			get { return StatementNumber; }
		}

		ZString IPaymentAuthorisationResponse.StatementFiler
		{
			get { return StatementFilerCode; }
		}

		ZString IPaymentAuthorisationResponse.PaymentFiler
		{
			get { return ZString.Empty; }
		}

		ZDecimal IPaymentAuthorisationResponse.PaymentAmount
		{
			get { return ZDecimal.Zero; }
		}

		ZDate IPaymentAuthorisationResponse.DateAccepted
		{
			get { return DateAccepted; }
		}

		ZString IPaymentAuthorisationResponse.AcceptanceErrorCode
		{
			get { return ConditionCode; }
		}

		ZString IPaymentAuthorisationResponse.AcceptanceErrorMessage
		{
			get { return NarrativeText; }
		}

		ZString IPaymentAuthorisationResponse.PayerUnitNumber
		{
			get { return ZString.Empty; }
		}

		ZString IPaymentAuthorisationResponse.DispositionTypeCode
		{
			get { return DispositionTypeCode; }
		}

		#endregion
	}
}
