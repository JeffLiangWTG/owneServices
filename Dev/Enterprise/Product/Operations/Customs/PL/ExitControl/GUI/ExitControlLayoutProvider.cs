using Enterprise.Customs.EU.ExitControl.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.PL.ExitControl.GUI;

public sealed class ExitControlLayoutProvider : ExitControlUcc6LayoutProvider
{
	protected override IPanelLayoutProvider HeaderDetailsPanelLayoutCore => new HeaderDetailsLayout();

	protected override bool GetAuthorizationIsActiveCore() => true;
}
