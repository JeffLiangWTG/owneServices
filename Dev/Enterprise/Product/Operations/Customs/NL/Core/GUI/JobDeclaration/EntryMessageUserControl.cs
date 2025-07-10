using Enterprise.Customs.GUI;

namespace Enterprise.Customs.NL.GUI;

public partial class EntryMessageUserControl : EU.GUI.EntryMessageUserControl
{
	public EntryMessageUserControl()
	{
		InitializeComponent();
	}

	protected override BaseCustomsEntryUserControl GetMessageUserControl()
	{
		return new MessageUserControl();
	}
}
