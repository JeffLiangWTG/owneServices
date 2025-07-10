using System.Web.UI;
using System.Web.UI.WebControls;
using Enterprise.ZArchitecture.Web.GUI.WebControls.Testing;

namespace Enterprise.Tracking.Web
{
	sealed class eDocAttachPopupTest : ZFileUploadLinkTest
	{
		protected override Unit GetExpectedWidth()
		{
			return Unit.Pixel(600);
		}

		protected override Unit GetExpectedHeight()
		{
			return Unit.Pixel(200);
		}

		protected override string ExpectedPathToIFrameSourcePage
		{
			get { return "/Runtime/Enterprise_Tracking_Web/" + RuntimeVersion + "/ZTextBoxButton/ZTextPopup/ZTextIFramePopup/ZButtonPopup/ZFileUploadLink/eDocAttachPopup/"; }
		}

		protected override string ExpectedIFrameSourcePage
		{
			get { return "eDocAttachPage.aspx"; }
		}

		protected override Control GetNewControl()
		{
			return new eDocAttachPopup();
		}
	}
}
