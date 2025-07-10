using Enterprise.Customs.NZ.Business.Declaration.OutwardReport;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.NZ.GUI
{
	public partial class ConsolManifestUserControl : Customs.GUI.ZManifestMessageHistoryUserControl
	{
		ZGroupBox interpretedGroupBox;
		protected ZDropEdit messagingModeDropEdit;
		CargoWise.Windows.UI.KLabel messagingModelbl;
		ZArchitecture.ZTextBox eM_InterpretedTextBox;

		public ConsolManifestUserControl(OutwardReportManifestStatus manifestStatus)
			: base(manifestStatus)
		{
			InitializeComponent();
		}
	}
}


