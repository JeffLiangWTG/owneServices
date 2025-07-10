using System;
using System.Web.UI;
using System.Web.UI.WebControls;
using Enterprise.Tracking.Business;
using Enterprise.ZArchitecture.Web.Business;
using Enterprise.ZArchitecture.Web.GUI.WebControls;

namespace Enterprise.Tracking.Web
{
	public class eDocAttachPopup : ZFileUploadLink
	{
		#region Overrides

		public override void RenderControl(HtmlTextWriter writer)
		{
			if (SiteUser.CanAddDocuments)
			{
				base.RenderControl(writer);
			}
		}

		protected override void OnInit(EventArgs e)
		{
			base.OnInit(e);
			Text = Res.GetString("960c4b56-76b8-44ab-925a-cf52d0f8cc78", "Add Document");
		}

		protected override Unit PopupWidth
		{
			get
			{
				return 600;
			}
		}

		protected override Unit PopupHeight
		{
			get
			{
				return 200;
			}
		}

		protected override string IFrameSourcePageName
		{
			get
			{
				return "eDocAttachPage.aspx";
			}
		}

		#endregion

		#region Implementation

		protected TrackingSiteUser SiteUser
		{
			get { return WebEnv.AppInstance.SiteUser as TrackingSiteUser; }
		}

		#endregion
	}
}
