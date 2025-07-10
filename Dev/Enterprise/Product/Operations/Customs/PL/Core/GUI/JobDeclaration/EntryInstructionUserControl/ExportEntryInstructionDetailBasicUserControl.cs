using System;

namespace Enterprise.Customs.PL.GUI;

public partial class ExportEntryInstructionDetailBasicUserControl : EntryInstructionDetailBasicUserControl
{
	public ExportEntryInstructionDetailBasicUserControl()
	{
		InitializeComponent();
	}

	protected override void OnLoad(EventArgs e)
	{
		base.OnLoad(e);

		SetEntryInstructionDetailsLayout();
	}

	public void SetEntryInstructionDetailsLayout()
	{
		DetailsLayoutControl.SetLayout(new EntryInstructionDetailsLayout());
	}
}
