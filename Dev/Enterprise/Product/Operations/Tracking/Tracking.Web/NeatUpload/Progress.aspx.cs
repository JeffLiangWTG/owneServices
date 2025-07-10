using System;
using System.Web;
using Enterprise.ZArchitecture.Web.GUI;

namespace Enterprise.Tracking.Web.NeatUpload
{
	public partial class Progress : Brettle.Web.NeatUpload.ProgressPage
	{
		protected override void OnLoad(EventArgs e)
		{
			try
			{
				base.OnLoad(e);
			}
			catch (FormatException)
			{
				if (HttpContext.Current?.ApplicationInstance is ZGlobal appInstance)
				{
					HttpContext.Current.Response.Redirect(appInstance.ErrorPage);
				}
			}
		}
	}
}
