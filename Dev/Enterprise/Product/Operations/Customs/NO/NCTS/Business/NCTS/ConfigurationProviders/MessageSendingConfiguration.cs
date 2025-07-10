using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Business.CodeDescriptionPairLists;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.NO.NCTS.Business;

sealed class MessageSendingConfiguration : EU.NCTS.Business.MessageSendingConfiguration
{
	public override CodeDescriptionPairList MessageTypeList(EU.NCTS.Business.NctsHeader header) => header switch
	{
		{ IsArrivalMovement: true } => new NctsArrivalMessageTypeCodeList(),
		{ IsDepartureMovement: true } => new NctsDepartureMessageTypeCodeList(),
		_ => base.MessageTypeList(header),
	};

	protected override void SetDefaultMessageTypeCore(EU.NCTS.Business.NctsHeaderMessageSendingObject nctsHeaderMessageSendingObject)
	{
		if (nctsHeaderMessageSendingObject is { NctsHeader: { IsArrivalMovement: true } header })
		{
			SetDefaultMessageTypeForArrivalMovement(header as NctsHeader, nctsHeaderMessageSendingObject);
			return;
		}
		base.SetDefaultMessageTypeCore(nctsHeaderMessageSendingObject);
	}

	protected override EU.NCTS.Business.NctsHeaderMessageSendingObjectParent GetNewNctsHeaderMessageSendingObjectParentCore(EU.NCTS.Business.NctsHeader header)
		=> new NctsHeaderMessageSendingObjectParent(header as NctsHeader);

	static void SetDefaultMessageTypeForArrivalMovement(NctsHeader header, EU.NCTS.Business.NctsHeaderMessageSendingObject nctsHeaderMessageSendingObject)
	{
		var customsStatus = header.ArrivalMovementHeader?.BM_CustomsStatus ?? ZString.Empty;
		nctsHeaderMessageSendingObject.MessageType = customsStatus.ToString() switch
		{
			NCTS5ArrivalCustomsStatusList.Codes.UnloadPermissionGranted => NctsArrivalMessageTypeCodeList.Codes.UnloadingRemarks,
			_ => NctsArrivalMessageTypeCodeList.Codes.ArrivalNotification
		};
	}
}
