using Enterprise.Customs.GUI;

namespace Enterprise.Customs.PL.GUI;

public partial class AttachmentMessageUserControl : BaseCustomsEntryUserControl
{
	public AttachmentMessageUserControl()
	{
		InitializeComponent();
		MessageGrid.ReadOnly = true;
	}

	/// <summary> 
	/// Clean up any resources being used.
	/// </summary>
	protected override void Dispose(bool disposing)
	{
		if (disposing)
		{
			if (components != null)
			{
				components.Dispose();
			}
		}
		base.Dispose(disposing);
	}
}
