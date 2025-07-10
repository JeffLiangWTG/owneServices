using Enterprise.Customs.NZ.Business.TradeSingleWindow;

namespace Enterprise.Customs.NZ.GUI
{
	public partial class BrokerageICRManifestUserControl : Customs.GUI.ZManifestMessageHistoryUserControl
	{
		ZArchitecture.GUI.ZGroupBox interpretedGroupBox;
		ZArchitecture.ZTextBox eM_InterpretedTextBox;

		public BrokerageICRManifestUserControl(ICRManifestStatus manifestStatus)
			: base(manifestStatus)
		{
			InitializeComponent();
		}
	}
}
