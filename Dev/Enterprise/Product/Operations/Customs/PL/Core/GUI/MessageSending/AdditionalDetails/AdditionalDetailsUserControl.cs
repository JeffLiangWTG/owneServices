using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.PL.GUI;

public partial class AdditionalDetailsUserControl : ZUserControl
{
	public AdditionalDetailsUserControl()
	{
		InitializeComponent();
	}

	internal void UpdateLayout(IPanelLayoutProvider panelLayoutProvider)
	{
		DynamicAdditionalDetailsPanel.UpdateLayout(panelLayoutProvider);
	}
}
