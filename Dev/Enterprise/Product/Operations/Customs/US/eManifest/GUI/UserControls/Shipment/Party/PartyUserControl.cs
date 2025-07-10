using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.US.eManifest.GUI
{
	public partial class PartyUserControl : ZUserControl
	{
		public PartyUserControl()
		{
			InitializeComponent();
			PartyAddressControl.SetLabelCaptionVisible(false);
			PartyTypeDropEdit.AllowOverlap(PartyAddressControl);
		}
	}
}
