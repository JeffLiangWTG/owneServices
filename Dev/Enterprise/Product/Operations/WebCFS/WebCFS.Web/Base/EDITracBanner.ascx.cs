using System;

using Enterprise.ZArchitecture.Web.GUI.WebControls;

namespace Enterprise.WebCFS.Web
{
	public partial class EDITracBanner : BaseUserControl
	{
		protected void Page_Load(object sender, EventArgs e)
		{
			LogoImage.ImageUrl = Page.AppInstance.LogoImage;
			LogoImage.NavigateUrl = Page.AppInstance.HomePage;
			LogoImage.ToolTip = Page.AppInstance.CompanyName;
			LogoImage.CssClass = "LogoImage";
		}
	}
}
