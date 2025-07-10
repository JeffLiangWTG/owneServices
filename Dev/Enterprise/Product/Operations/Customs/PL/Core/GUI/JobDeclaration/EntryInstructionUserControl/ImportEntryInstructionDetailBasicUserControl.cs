namespace Enterprise.Customs.PL.GUI;

public partial class ImportEntryInstructionDetailBasicUserControl : EntryInstructionDetailBasicUserControl
{
	public ImportEntryInstructionDetailBasicUserControl()
	{
		InitializeComponent();
		SetEntryInstructionDetailsLayout();
	}

	public void SetEntryInstructionDetailsLayout()
	{
		DetailsLayoutControl.SetLayout(new EntryInstructionDetailsLayout());
	}
}
