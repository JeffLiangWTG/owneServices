using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.NO.NCTS.Business;

sealed class ArrivalOutboundMessageInformationUpdater : IMessageInformationUpdater
{
	void IMessageInformationUpdater.UpdateInformation(NctsHeader header, EDIMessage message)
	{
		Argument.NotNull(header, nameof(header));
		Argument.NotNull(message, nameof(message));

		header.EffectiveMessageStatus = message.EM_MessageType.ToString() switch
		{
			NctsArrivalMessageTypeCodeList.Codes.ArrivalNotification => NctsMovementHeaderTransactionStatusList.Codes.ArrivalNotificationSent,
			NctsArrivalMessageTypeCodeList.Codes.UnloadingRemarks => NctsMovementHeaderTransactionStatusList.Codes.UnloadingRemarksSent,
			_ => ZString.Empty
		};
	}
}
