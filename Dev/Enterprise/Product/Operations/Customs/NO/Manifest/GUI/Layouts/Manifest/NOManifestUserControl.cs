using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.NO.Manifest.GUI
{
	sealed partial class NOManifestUserControl : ZUserControl
	{
		public NOManifestUserControl()
		{
			InitializeComponent();
			MasterBillDynamicLayoutPanel.UpdateLayout(GetMasterBillLayout());
		}

		internal IPanelLayoutProvider GetMasterBillLayout() => new MasterBillLayout();
	}
}
