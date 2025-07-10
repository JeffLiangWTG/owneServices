using CargoWise.Common;
using CargoWise.Customs.PL.MessageContracts.Interfaces.NCTS;

namespace Enterprise.Customs.PL.NCTS.Business;

public class CC014CTransitOperationProvider : ICC014CTransitOperation
{
	public CC014CTransitOperationProvider(MessageSendingObject messageSendingObject)
	{
		this.messageSendingObject = Argument.NotNull(messageSendingObject, nameof(messageSendingObject));
	}

	readonly MessageSendingObject messageSendingObject;

	public string MRN => messageSendingObject.MovementReferenceNumber.IsEmpty ? null : messageSendingObject.MovementReferenceNumber;

	public string LRN => string.IsNullOrEmpty(MRN) ? EDIMessage.PL_NCTS_LRN_PlaceHolder : null;
}
