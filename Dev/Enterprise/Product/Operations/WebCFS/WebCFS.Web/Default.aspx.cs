using System;
using System.Web;

namespace Enterprise.WebCFS.Web
{
	/// <summary>
	/// Summary description for _Default.
	/// </summary>
	public partial class _Default : BasePage
	{
		protected void Page_Load(object sender, EventArgs e)
		{
			HttpContext.Current.Response.Redirect(AppInstance.ApplicationRoot + AppInstance.RelativeContainerPage);
		}
	}
}
