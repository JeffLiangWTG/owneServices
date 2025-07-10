using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.US.GUI
{
	public partial class ExportATFUserControl : ZUserControl
	{
		public ExportATFUserControl()
		{
			InitializeComponent();
		}

		internal void HideIrrelevantControlsForProduct()
		{
			ProductIrrevelantPanel.Visible = false;
		}
	}
}
