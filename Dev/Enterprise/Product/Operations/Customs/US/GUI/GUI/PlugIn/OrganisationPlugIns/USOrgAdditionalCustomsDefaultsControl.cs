using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.US.GUI
{
	public partial class USOrgAdditionalCustomsDefaultsControl : ZUserControl
	{
		public USOrgAdditionalCustomsDefaultsControl()
		{
			InitializeComponent();
		}

		public void ControlsVisibility(bool isVisible)
		{
			FirstSaleDropEdit.Visible = isVisible;
			ReconIndicatorDropEdit.Visible = isVisible;
			NAFTAReconIndicatorCheckBox.Visible = isVisible;

			FirstSaleDropEdit.Text = isVisible ? FirstSaleDropEdit.Text : string.Empty;
			ReconIndicatorDropEdit.Text = isVisible ? ReconIndicatorDropEdit.Text : string.Empty;
			NAFTAReconIndicatorCheckBox.Text = isVisible ? NAFTAReconIndicatorCheckBox.Text : string.Empty;
		}
	}
}
