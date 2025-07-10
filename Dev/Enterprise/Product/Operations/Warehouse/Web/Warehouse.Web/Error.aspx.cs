using System;
using CargoWise.Common;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Warehouse.Web
{
	public partial class Error : BasePage
	{
		protected override void OnLoad(EventArgs e)
		{
			HomeLink.NavigateUrl = AppInstance.HomePage;
			HomeLink.Text = (NoResString)"Home"; // Hard-coded constant

			PageTitle.Text = (NoResString)"Error"; // Hard-coded constant
			MessageDescription.Text = Res.GetString("34fc0b38-05b4-4de8-9dff-68e28bf274b0", "A problem has been encountered. Sorry for any inconvenience this may have caused.");

			if (Request.Params["data"] != null)
			{
				try
				{
					SecureQueryString qs = new SecureQueryString(Request["data"]);
					PageTitle.Text = qs["title"];
					MessageDescription.Text = qs["message"];
				}
				catch (InvalidQueryStringException)
				{
				}
				catch (ExpiredQueryStringException)
				{
				}
			}
		}
	}
}
