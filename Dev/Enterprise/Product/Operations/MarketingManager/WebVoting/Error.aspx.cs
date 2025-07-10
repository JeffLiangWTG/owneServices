using System;
using System.Web.UI.WebControls;
using CargoWise.Common;
using Enterprise.ZArchitecture.Web.GUI.WebControls;

namespace Enterprise.MarketingManager.WebVoting
{
	/// <summary>
	/// Summary description for Error.
	/// </summary>
	public partial class Error : BasePage
	{
		#region InternalMembers
#if DEBUG
		protected internal HyperLink HomeLinkInternal => HomeLink;
		protected internal void SetHomeLinkInternal(HyperLink hyperlink) => HomeLink = hyperlink;
		protected internal ZTextLabel PageTitleInternal => PageTitle;
		protected internal void SetPageTitleInternal(ZTextLabel label) => PageTitle = label;
		protected internal Literal MessageDescriptionInternal => MessageDescription;
		protected internal void SetMessageDescriptionInternal(Literal literal) => MessageDescription = literal;
#endif
		#endregion
		protected override void OnLoad(EventArgs e)
		{
			HomeLink.NavigateUrl = AppInstance.HomePage;

			if (Request.QueryString["data"] != null)
			{
				try
				{
					SecureQueryString qs = new SecureQueryString(Request.QueryString["data"]);
					PageTitle.Text = qs["title"];
					MessageDescription.Text = qs["message"];
					var pageTitle = qs["pageTitle"];
					if (!string.IsNullOrWhiteSpace(pageTitle))
					{
						Title = pageTitle;
					}
				}
				catch (QueryStringException)
				{ }
			}
		}

#if DEBUG
		protected internal void OnLoadInternal(EventArgs e) => OnLoad(e);
#endif
	}
}
