using System;
using Enterprise.ZArchitecture.Web.GUI.WebControls;

namespace Enterprise.Tracking.Web
{
	public partial class RunUrl : ZPage
	{
		protected void Page_Load(object sender, EventArgs e)
		{
			string uRL = GetStringFromParameter("Ref");

			if (!string.IsNullOrEmpty(uRL))
			{
				Response.Redirect(uRL);
			}
		}
	}
}
