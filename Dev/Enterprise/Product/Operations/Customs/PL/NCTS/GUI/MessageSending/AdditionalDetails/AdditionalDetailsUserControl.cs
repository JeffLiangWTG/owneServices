using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.PL.NCTS.GUI;

public partial class AdditionalDetailsUserControl : ZUserControl
{
	public AdditionalDetailsUserControl()
	{
		InitializeComponent();
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing)
		{
			components?.Dispose();
		}
		base.Dispose(disposing);
	}

	internal void UpdateLayout(IPanelLayoutProvider panelLayoutProvider)
	{
		DynamicAdditionalDetailsPanel.UpdateLayout(panelLayoutProvider);
	}
}
