using System;
using System.Web;
using Enterprise.ZArchitecture.Web.Business.Testing;
using Enterprise.ZArchitecture.Web.GUI;
using NUnit.Framework;

namespace Enterprise.WebCFS.Web.Testing
{
	public class DefaultTest : TestCase
	{
		[HttpContextEnabledTest]
		public void TestOnLoadRedirect()
		{
			var page = new DefaultPageForTest();

			page.PageLoadForTest();

			Assert("Request should be redirected", HttpContext.Current.Response.IsRequestBeingRedirected);
			AssertEquals("Should be redirected to Containers page", "/webapp/Containers/Containers.aspx", HttpContext.Current.Response.RedirectLocation);
		}
	}

	class DefaultPageForTest : _Default
	{
		public void PageLoadForTest() => Page_Load(null, EventArgs.Empty);

		protected override ZGlobal GetNewTestGlobal()
		{
			return new TestGlobalWithAppRoot();
		}

		class TestGlobalWithAppRoot : TestGlobal
		{
			public override string ApplicationRoot => "/webapp/";
		}
	}
}
