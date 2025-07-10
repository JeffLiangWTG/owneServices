using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.ASYCUDA.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.TR.Manifest.GUI
{
	public partial class TRBillAdditionalUserControl : ZUserControl
		, IAdditionalTabPage
	{
		public TRBillAdditionalUserControl()
		{
			InitializeComponent();
		}

		ZUserControl IAdditionalTabPage.AdditionalTabPageUserControl => this;
		ResourceStringData IAdditionalTabPage.AdditionalTabPageCaption => Res.GetData("9045BF99-390D-4692-BF15-0A052FB4641D", "Related Declarations");
		AdditionalTabPageVisibility IAdditionalTabPage.AdditionalControlVisibility => new AdditionalTabPageVisibility(h => ((Business.AsycudaManifestHeader)h).IsBillRelatedDeclarationsTabVisible, h => h.AMA_NatureInfo, h => h.AMA_ManifestTypeInfo);
		int IAdditionalTabPage.TabPageSequence => 1;
	}
}
