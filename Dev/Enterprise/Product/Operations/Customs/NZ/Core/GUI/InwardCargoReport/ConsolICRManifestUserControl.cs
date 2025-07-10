using Enterprise.Customs.NZ.Business.TradeSingleWindow;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.NZ.GUI
{
	public partial class ConsolICRManifestUserControl : Customs.GUI.ZManifestMessageHistoryUserControl
	{
		ZGroupBox interpretedGroupBox;
		ZArchitecture.ZTextBox eM_InterpretedTextBox;

		public ConsolICRManifestUserControl(ICRManifestStatus manifestStatus)
			: base(manifestStatus)
		{
			InitializeComponent();
		}
	}
}
