using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.ASYCUDA.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.NZ.Manifest.GUI
{
	public partial class NZManifestSpecificUserControl : ZUserControl, IAdditionalTabPage
	{
		public NZManifestSpecificUserControl()
		{
			InitializeComponent();
		}

		ZUserControl IAdditionalTabPage.AdditionalTabPageUserControl => this;

		ResourceStringData IAdditionalTabPage.AdditionalTabPageCaption => Res.GetData("6474EBDF-6C39-4AE3-9401-317565098E8A", "Delivery Notification Party");

		AdditionalTabPageVisibility IAdditionalTabPage.AdditionalControlVisibility => new AdditionalTabPageVisibility(h => true, null);

		int IAdditionalTabPage.TabPageSequence => 1;
	}
}
