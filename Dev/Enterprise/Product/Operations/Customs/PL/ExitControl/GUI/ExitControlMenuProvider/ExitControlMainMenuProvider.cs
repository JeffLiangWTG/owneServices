using Enterprise.Customs.PL.ExitControl.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.PL.ExitControl.GUI;
public class ExitControlMainMenuProvider : EU.ExitControl.GUI.ExitControlMainMenuProvider
{
	public ExitControlMainMenuProvider(CusExitHeader header) : base(header)
	{
	}

	new CusExitHeader header => (CusExitHeader)base.header;

	protected override ZMenuItem[] GetAdditionalMainMenuItemsCore() =>
	[
		new ExitControlSendToCustomsMenuCreator(header).Create(),
	];
}
