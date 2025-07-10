using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.PL.NCTS.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.PL.NCTS.GUI;

public class ArrivalAdditionalDetailsLayoutBuilder<T> : ColumnLayoutBuilder<T, ArrivalAdditionalDetailsControlBag> where T : MessageSendingObject
{
	public override ColumnLayoutBuilderCaptionWidthSize CaptionWidth => ColumnLayoutBuilderCaptionWidthSize.Long;

	public override ArrivalAdditionalDetailsControlBag CommonBag => ArrivalAdditionalDetailsControlBag.Instance;

	protected override int MaxColumns => 1;

	protected override void SetDefaultVisibilities()
	{
		base.SetDefaultVisibilities();
		SetVisibility(CommonBag.TirPageNumberDropEdit, TirTextBoxVisible, x => x.MessageTypeInfo);
		SetVisibility(CommonBag.TirUnloadingNumberDropEdit, TirTextBoxVisible, x => x.MessageTypeInfo);
	}

	bool TirTextBoxVisible(MessageSendingObject messageSendingObject)
	{
		var arrivalMovementHeader = messageSendingObject.NctsHeader.ArrivalMovementHeader;

		return messageSendingObject.MessageType == ArrivalMessageSendingObjectTypeList.Codes.URM
			&& (string.IsNullOrEmpty(arrivalMovementHeader.BM_InBondEntryType)
			|| arrivalMovementHeader.BM_InBondEntryType == NctsPhase5DeclarationTypeList.Codes.TIR);
	}
}
