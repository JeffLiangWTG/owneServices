using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Messaging;
using NCTS5ArrivalCustomsStatusList = Enterprise.Customs.EU.NCTS.Business.CodeDescriptionPairLists.NCTS5ArrivalCustomsStatusList;
using NctsHeader = Enterprise.Customs.EU.NCTS.Business.NctsHeader;

namespace Enterprise.Customs.SE.NCTS.Business;

public sealed class MessageSendingConfiguration : EU.NCTS.Business.MessageSendingConfiguration
{
	protected override void SetDefaultMessageTypeCore(EU.NCTS.Business.NctsHeaderMessageSendingObject nctsHeaderMessageSendingObject)
	{
		var nctsHeader = nctsHeaderMessageSendingObject.NctsHeader;
		if (nctsHeader.IsDepartureMovement)
		{
			if (nctsHeader.MovementReferenceNumber.IsEmpty)
			{
				nctsHeaderMessageSendingObject.MessageType = NCTSDepartureOutgoingMessageTypeList.Codes.DeclarationData;
			}
			else
			{
				if (nctsHeader.MovementHeader?.BM_AdditionalDeclarationType is ZString additionalDeclarationType && additionalDeclarationType == NctsTypeOfAdditionalDeclarationList.Codes.D)
				{
					nctsHeaderMessageSendingObject.MessageType = NCTSDepartureOutgoingMessageTypeList.Codes.PresentationNotification;
				}
				else
				{
					nctsHeaderMessageSendingObject.MessageType = NCTSDepartureOutgoingMessageTypeList.Codes.InvalidationRequest;
				}
			}
		}
		else if (nctsHeader.IsArrivalMovement)
		{
			if (nctsHeader.ArrivalMovementHeader?.BM_CustomsStatus is ZString status && status == NCTS5ArrivalCustomsStatusList.Codes.UnloadRemarks)
			{
				nctsHeaderMessageSendingObject.MessageType = NCTSArrivalOutgoingMessageTypeList.Codes.UnloadingRemarks;
			}
			else
			{
				nctsHeaderMessageSendingObject.MessageType = NCTSArrivalOutgoingMessageTypeList.Codes.ArrivalNotification;
			}
		}
	}

	protected override EU.NCTS.Business.NctsHeaderMessageSendingObjectParent GetNewNctsHeaderMessageSendingObjectParentCore(NctsHeader header)
		=> new NctsHeaderMessageSendingObjectParent(header);
}
