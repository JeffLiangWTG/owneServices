using Enterprise.Customs.PL.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.PL.GUI;

public class ImportAdditionalDetailsLayout : IPanelLayoutProvider
{
	PanelLayout IPanelLayoutProvider.Layout => layout ?? (layout = CreateLayout());

	PanelLayout layout;

	static PanelLayout CreateLayout()
	{
		var builder = new ImportAdditionalDetailsLayoutBuilder<BaseMessageSendingObject>();
		var commonBag = builder.CommonBag;

		return builder.Build();
	}
}
