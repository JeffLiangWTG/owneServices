using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.PL.GUI;

public partial class EntryInstructionDetailsLayoutControl : ZUserControl
{
	public EntryInstructionDetailsLayoutControl()
	{
		InitializeComponent();
	}

	public void SetLayout(IPanelLayoutProvider layout)
	{
		DetailsPanel.UpdateLayout(layout);
	}
}
