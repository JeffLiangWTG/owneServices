using CargoWise.Types;

namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks
{
	public interface IPaymentAuthorisationResponse
	{
		ZString StatementBillNumber { get; }
		ZString StatementFiler { get; }
		ZString PaymentFiler { get; }
		ZDecimal PaymentAmount { get; }
		ZDate DateAccepted { get; }
		ZString AcceptanceErrorCode { get; }
		ZString AcceptanceErrorMessage { get; }
		ZString PayerUnitNumber { get; }
		ZString DispositionTypeCode { get; }
	}
}
