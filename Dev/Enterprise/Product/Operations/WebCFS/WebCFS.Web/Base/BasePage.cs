using Enterprise.ZArchitecture.Web.GUI.WebControls;

namespace Enterprise.WebCFS.Web
{
	/// <summary>
	/// Page we inherit all web pages from
	/// </summary>
	public class BasePage : ZPage
	{
		public new Global AppInstance
		{
			get { return base.AppInstance as Global; }
		}

		protected override string PageHeaderControlPath
		{
			get { return "Base/PageHeaderWithNavigation.ascx"; }
		}
	}
}
