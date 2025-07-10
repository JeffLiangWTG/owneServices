namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Output
{
	using CargoWise.Types;

	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.AutomatedClearinghouseResponse)]
	public partial class ACHQU : MessageBlock, IPaymentAuthorisationResponse
	{
		#region IPaymentAuthorisationResponse Members

		ZString IPaymentAuthorisationResponse.StatementBillNumber
		{
			get { return StatementBillNumber; }
		}

		ZString IPaymentAuthorisationResponse.StatementFiler
		{
			get { return StatementFiler; }
		}

		ZString IPaymentAuthorisationResponse.PaymentFiler
		{
			get { return PaymentFiler; }
		}

		ZDecimal IPaymentAuthorisationResponse.PaymentAmount
		{
			get { return PaymentAmount; }
		}

		ZDate IPaymentAuthorisationResponse.DateAccepted
		{
			get { return DateAccepted; }
		}

		ZString IPaymentAuthorisationResponse.AcceptanceErrorCode
		{
			get { return AcceptanceErrorCode; }
		}

		ZString IPaymentAuthorisationResponse.AcceptanceErrorMessage
		{
			get { return AcceptanceErrorMessage; }
		}

		ZString IPaymentAuthorisationResponse.PayerUnitNumber
		{
			get { return PayerUnitNumber; }
		}

		ZString IPaymentAuthorisationResponse.DispositionTypeCode
		{
			get { return ZString.Empty; }
		}

		#endregion
	}
}
