using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.ASYCUDA.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.NO.Manifest.GUI
{
	sealed partial class PreviousDocumentsUserControl : ZUserControl, IAdditionalTabPage
	{
		public PreviousDocumentsUserControl()
		{
			InitializeComponent();
		}

		#region IAdditionalTabPage Members

		ZUserControl IAdditionalTabPage.AdditionalTabPageUserControl => this;

		ResourceStringData IAdditionalTabPage.AdditionalTabPageCaption => Res.GetData("853F2257-1DD9-4430-AB05-0BC606984A9C", "Previous Documents");

		AdditionalTabPageVisibility IAdditionalTabPage.AdditionalControlVisibility => new (h => true, null);

		int IAdditionalTabPage.TabPageSequence => 1;

		#endregion
	}
}
