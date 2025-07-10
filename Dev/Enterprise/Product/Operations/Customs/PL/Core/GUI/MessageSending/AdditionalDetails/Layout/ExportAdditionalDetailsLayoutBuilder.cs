using Enterprise.Customs.EU.Business;
using Enterprise.Customs.PL.Business;
using Enterprise.Customs.PL.Business.Declaration;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.PL.GUI;

public class ExportAdditionalDetailsLayoutBuilder<T> : ColumnLayoutBuilder<T, ExportAdditionalDetailsControlBag> where T : BaseMessageSendingObject
{
	public override ColumnLayoutBuilderCaptionWidthSize CaptionWidth => ColumnLayoutBuilderCaptionWidthSize.Medium;

	public override ExportAdditionalDetailsControlBag CommonBag => ExportAdditionalDetailsControlBag.Instance;

	protected override int MaxColumns => 2;

	protected override void SetDefaultVisibilities()
	{
		base.SetDefaultVisibilities();
		SetVisibility(CommonBag.SecurityDropEdit, SecurityDropEditVisible, x => x.ActionInfo);
		SetVisibility(CommonBag.AmendmentInvalidationReasonUserControl, AmendmentInvalidationReasonUserControlVisible, x => x.ActionInfo);
		SetVisibility(CommonBag.CorrectionAcceptanceDropEdit, ActionIsCC566, x => x.ActionInfo);
		SetVisibility(CommonBag.AcceptanceCommentUserControl, ActionIsCC566, x => x.ActionInfo);
	}

	bool SecurityDropEditVisible(BaseMessageSendingObject messageSendingObject) =>
		(messageSendingObject.Action == ExportMessageSendingObjectActionList.Codes.CC513
			|| messageSendingObject.Action == ExportMessageSendingObjectActionList.Codes.CC515)
		&& messageSendingObject.Header.Declaration.JE_EntryStyle != EntryStyleListExportUCC.Codes.ExportToSpecialTerritory;

	bool AmendmentInvalidationReasonUserControlVisible(BaseMessageSendingObject messageSendingObject) => messageSendingObject.Action == ExportMessageSendingObjectActionList.Codes.CC514;

	bool ActionIsCC566(BaseMessageSendingObject messageSendingObject) => messageSendingObject.Action == ExportMessageSendingObjectActionList.Codes.CC566;
}
