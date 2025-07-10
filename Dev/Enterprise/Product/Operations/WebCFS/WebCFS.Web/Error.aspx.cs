using System;
using CargoWise.Common;

namespace Enterprise.WebCFS.Web
{
	/// <summary>
	/// Summary description for Error.
	/// </summary>
	public partial class Error : BasePage
	{
		protected override string PageHeaderControlPath
		{
			get { return "Base/PageHeader.ascx"; }
		}

		protected void Page_Load(object sender, EventArgs e)
		{
			HomeLink.NavigateUrl = AppInstance.HomePage;
			HomeLink.Text = Res.GetString("2b3c6875-990f-4427-885e-b32c2de31fc3", "Home");

			PageTitle.Text = Res.GetString("fce82f71-8d80-4c93-b0c3-3803355528ed", "Error");
			MessageDescription.Text = Res.GetString("a46b1fa0-05f9-47fd-9444-2a67ab1c3d97", "A problem has been encountered. Sorry for any inconvenience this may have caused.");

			if (Request.Params["data"] != null)
			{
				try
				{
					SecureQueryString qs = new SecureQueryString(Request["data"]);
					PageTitle.Text = qs["title"];
					MessageDescription.Text = qs["message"];
				}
				catch (InvalidQueryStringException)
				{ }
				catch (ExpiredQueryStringException)
				{ }
			}
		}
	}
}
