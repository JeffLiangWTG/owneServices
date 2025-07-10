using Enterprise.Customs.PL.NCTS.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.PL.NCTS.GUI;

public class ArrivalAdditionalDetailsLayout : IPanelLayoutProvider
{
	PanelLayout IPanelLayoutProvider.Layout => layout ?? (layout = CreateLayout());

	PanelLayout layout;

	static PanelLayout CreateLayout()
	{
		var builder = new ArrivalAdditionalDetailsLayoutBuilder<MessageSendingObject>();
		var commonBag = builder.CommonBag;

		builder.AddColumn();
		builder.Add(commonBag.TirPageNumberDropEdit, ControlWidthClass.Auto);
		builder.Add(commonBag.TirUnloadingNumberDropEdit, ControlWidthClass.Auto);

		return builder.Build();
	}
}
