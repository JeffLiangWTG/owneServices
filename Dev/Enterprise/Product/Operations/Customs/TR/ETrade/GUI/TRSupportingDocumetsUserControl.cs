using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.ASYCUDA.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.TR.ETrade.GUI
{
	public partial class TRSupportingDocumetsUserControl : ZUserControl, IAdditionalTabPage
	{
		public TRSupportingDocumetsUserControl()
		{
			InitializeComponent();
		}
		#region IAdditionalTabPage Members
		ZUserControl IAdditionalTabPage.AdditionalTabPageUserControl => this;
		ResourceStringData IAdditionalTabPage.AdditionalTabPageCaption => Res.GetData("9B2814C5 - 6108 - 476E-9587 - 8C3435D0E0C4", "Supporting Documents");
		AdditionalTabPageVisibility IAdditionalTabPage.AdditionalControlVisibility => new AdditionalTabPageVisibility(h => true, null);
		int IAdditionalTabPage.TabPageSequence => 1;
		#endregion
	}
}
