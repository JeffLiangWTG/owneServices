using Enterprise.Customs.PL.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.PL.GUI;

public class ExportAdditionalDetailsLayout : IPanelLayoutProvider
{
	PanelLayout IPanelLayoutProvider.Layout => layout ?? (layout = CreateLayout());

	PanelLayout layout;

	static PanelLayout CreateLayout()
	{
		var builder = new ExportAdditionalDetailsLayoutBuilder<BaseMessageSendingObject>();
		var commonBag = builder.CommonBag;

		builder.AddColumn();
		builder.Add(commonBag.SecurityDropEdit, ControlWidthClass.Auto);
		builder.Add(commonBag.AmendmentInvalidationReasonUserControl, ControlWidthClass.Auto);
		builder.Add(commonBag.CorrectionAcceptanceDropEdit, ControlWidthClass.Auto);
		builder.Add(commonBag.AcceptanceCommentUserControl, ControlWidthClass.Auto);
		return builder.Build();
	}
}
