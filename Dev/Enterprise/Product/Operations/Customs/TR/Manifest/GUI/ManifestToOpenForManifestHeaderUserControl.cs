using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.ASYCUDA.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.TR.Manifest.GUI
{
	public partial class ManifestToOpenForManifestHeaderUserControl : ZUserControl, IAdditionalTabPage
	{
		public ManifestToOpenForManifestHeaderUserControl()
		{
			InitializeComponent();
		}

		#region IAdditionalTabPage Members
		ZUserControl IAdditionalTabPage.AdditionalTabPageUserControl => this;
		ResourceStringData IAdditionalTabPage.AdditionalTabPageCaption => Res.GetData("EF0D0A04-3DB6-4F55-B44F-B7224705FCCD", "Manifest to Open");
		AdditionalTabPageVisibility IAdditionalTabPage.AdditionalControlVisibility => new AdditionalTabPageVisibility(h => ((Business.AsycudaManifestHeader)h).IsManifestToOpenPageVisible, h => h.AMA_ManifestTypeInfo, h => h.AMA_TransportModeInfo);
		int IAdditionalTabPage.TabPageSequence => 2;
		#endregion
	}
}

