using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.ASYCUDA.GUI;
using Enterprise.Customs.TW.Manifest.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.TW.Manifest.GUI
{
	public partial class AsycudaContainerBillLinkUserControl : ZUserControl, IAdditionalTabPage
	{
		public AsycudaContainerBillLinkUserControl()
		{
			InitializeComponent();
		}

		public new AsycudaBill CurrentDataItem => (AsycudaBill)base.CurrentDataItem;

		ZUserControl IAdditionalTabPage.AdditionalTabPageUserControl => this;
		ResourceStringData IAdditionalTabPage.AdditionalTabPageCaption => Res.GetData("D95253DF-C976-49D6-9F0C-3BF421BD81A3", "Containers");
		AdditionalTabPageVisibility IAdditionalTabPage.AdditionalControlVisibility => new AdditionalTabPageVisibility(h => true, null);
		int IAdditionalTabPage.TabPageSequence => 0;
	}
}
