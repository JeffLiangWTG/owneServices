using Enterprise.Customs.GUI;

namespace Enterprise.Customs.PL.GUI;

public partial class PLEntryMessageUserControl : EU.GUI.EntryMessageUserControl
{
	public PLEntryMessageUserControl()
	{
		InitializeComponent();
	}

	protected override BaseCustomsEntryUserControl GetMessageUserControl() => new MessageUserControl();
}
