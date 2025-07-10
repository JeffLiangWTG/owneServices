using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.ASYCUDA.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.TR.Manifest.GUI
{
	public partial class VisitedPortsForManifestHeaderUserControl : ZUserControl, IAdditionalTabPage
	{
		public VisitedPortsForManifestHeaderUserControl()
		{
			InitializeComponent();
		}

		#region IAdditionalTabPage Members

		ZUserControl IAdditionalTabPage.AdditionalTabPageUserControl => this;
		ResourceStringData IAdditionalTabPage.AdditionalTabPageCaption => Res.GetData("A7773E6B-E1EB-45F2-9C6B-833CAC680339", "Itinerary");
		AdditionalTabPageVisibility IAdditionalTabPage.AdditionalControlVisibility => new AdditionalTabPageVisibility(h => ((Business.AsycudaManifestHeader)h).IsItineraryTabePageVisible, h => h.AMA_ApplicationCodeInfo, h => h.AMA_ManifestTypeInfo, h => h.AMA_TransportModeInfo);
		int IAdditionalTabPage.TabPageSequence => 1;
		#endregion
	}
}
