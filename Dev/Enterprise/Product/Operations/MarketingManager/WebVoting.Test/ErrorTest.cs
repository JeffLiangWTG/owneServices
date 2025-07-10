using System;
using System.Web.UI.WebControls;
using CargoWise.Common;
using Enterprise.ZArchitecture.Web.GUI.WebControls;
using Enterprise.ZArchitecture.Web.GUI.WebControls.Testing;
using NUnit.Framework;

namespace Enterprise.MarketingManager.WebVoting
{
	class ErrorTest : ZPageTestCase
	{
		public void TestOnLoad_NavigateUrlIsSet()
		{
			Page.OnLoadInternal(EventArgs.Empty);
			AssertEquals("http://www.test.cargowise.com/", Page.HomeLinkInternal.NavigateUrl);
		}

		public void TestOnLoad_PageTitleAndMessageDescriptionSet()
		{
			SecureQueryString secureQueryString = new SecureQueryString();
			secureQueryString.Add("title", "some error");
			secureQueryString.Add("message", "your website is now doomed");
			secureQueryString.Add("pageTitle", "pageTitle123");
			Page.Request.QueryString["data"] = secureQueryString.ToString();

			Page.OnLoadInternal(EventArgs.Empty);
			AssertEquals("some error", Page.PageTitleInternal.Text);
			AssertEquals("your website is now doomed", Page.MessageDescriptionInternal.Text);
			AssertEquals("pageTitle123", Page.Title);
		}

		[ExpectNoExceptions]
		public void TestOnLoad_QueryStringExceptionShouldBeIgnored()
		{
			Page.Request.QueryString["data"] = "lalalalalalalalala";
			Page.OnLoadInternal(EventArgs.Empty);
		}

		new Error Page
		{
			get { return (Error)base.Page; }
		}

		protected override ZPage GetNewZPage()
		{
			Error result = new Error();
			result.SetHomeLinkInternal(new HyperLink());
			result.SetPageTitleInternal(new ZTextLabel());
			result.SetMessageDescriptionInternal(new Literal());
			return result;
		}
	}
}
