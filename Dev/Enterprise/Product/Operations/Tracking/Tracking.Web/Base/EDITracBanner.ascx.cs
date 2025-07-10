using System;
using Enterprise.ZArchitecture.Web.GUI.WebControls;

namespace Enterprise.Tracking.Web
{
	public partial class EDITracBanner : BaseUserControl
	{
		protected System.Web.UI.HtmlControls.HtmlTable BannerTable;
		protected System.Web.UI.HtmlControls.HtmlTableCell BannerCell;

		protected void Page_Load(object sender, EventArgs e)
		{
			LogoImage.ImageUrl = Page.AppInstance.LogoImage;
			LogoImage.NavigateUrl = ZAppInstance.HomePage;
			LogoImage.ToolTip = ZAppInstance.CompanyName;
			LogoImage.CssClass = "LogoImage";
		}

		#region Web Form Designer generated code
		override protected void OnInit(EventArgs e)
		{
			//
			// CODEGEN: This call is required by the ASP.NET Web Form Designer.
			//
			InitializeComponent();
			base.OnInit(e);
		}

		/// <summary>
		///		Required method for Designer support - do not modify
		///		the contents of this method with the code editor.
		/// </summary>
		void InitializeComponent()
		{
		}
		#endregion
	}
}
