using Enterprise.Customs.PL.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.PL.GUI;

public class AlternativeEvidenceDataLayout : IPanelLayoutProvider
{
	PanelLayout IPanelLayoutProvider.Layout => layout ??= CreateLayout();

	PanelLayout layout;

	static PanelLayout CreateLayout()
	{
		var builder = new AlternativeEvidenceDataLayoutBuilder<BaseMessageSendingObjectParent>();
		var commonBag = builder.CommonBag;

		builder.AddColumn();
		builder.Add(commonBag.EnquiryInformationCodeDropEdit, ControlWidthClass.Auto);
		builder.Add(commonBag.OfficeOfExitCodeFindBox, ControlWidthClass.Auto);
		builder.Add(commonBag.ExitDateDateTimeOffsetEdit, ControlWidthClass.Auto);

		builder.AddColumn();
		builder.Add(commonBag.AlternativeEvidenceGrid, ControlWidthClass.Auto);

		return builder.Build();
	}
}
