using Enterprise.Customs.NZ.Business.Declaration.OutwardReport;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.NZ.GUI
{
	public partial class OCRManifestUserControl : Customs.GUI.ZManifestMessageHistoryUserControl
	{
		ZGroupBox interpretedGroupBox;
		ZArchitecture.ZTextBox eM_InterpretedTextBox;
		ZPanel notifyPanel;

		public OCRManifestUserControl(OutwardReportManifestStatus manifestStatus)
			: base(manifestStatus)
		{
			InitializeComponent();
		}
	}
}
