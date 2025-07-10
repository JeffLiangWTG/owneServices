using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.ASYCUDA.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.TR.Manifest.GUI
{
	public partial class VisitedPortsForBillUserControl : ZUserControl, IAdditionalTabPage
	{
		public VisitedPortsForBillUserControl()
		{
			InitializeComponent();
		}

		ZUserControl IAdditionalTabPage.AdditionalTabPageUserControl => this;
		ResourceStringData IAdditionalTabPage.AdditionalTabPageCaption => Res.GetData("D615FB42-7CC5-4913-ABB6-9990D29D50ED", "Visited Ports");
		AdditionalTabPageVisibility IAdditionalTabPage.AdditionalControlVisibility => new AdditionalTabPageVisibility(h => true, null);
		public int TabPageSequence => 2;
	}
}
