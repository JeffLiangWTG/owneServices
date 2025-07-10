using System;
using Enterprise.ZArchitecture.Web.GUI;

namespace Enterprise.Tracking.Web.Testing
{
	sealed class PageForPasswordResetHelperTest : BasePage
	{
		protected override ZGlobal GetNewTestGlobal()
		{
			return new TestGlobal();
		}

		protected override Uri RequestUrl => new Uri("http://www.test.com/WebTracker/Login/ForgotPassword.aspx");

		protected override string GetPageRelativePath()
		{
			return "default.aspx";
		}
	}
}
