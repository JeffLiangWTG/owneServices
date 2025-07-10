using System.Web.UI;
using Enterprise.Tracking.Business;
using Enterprise.ZArchitecture.Web.Business.Testing;

namespace Enterprise.Tracking.Web.Testing
{
	[HttpContextEnabledTest]
	sealed class ResetPasswordTest : BaseTrackingPageTest
	{
		protected override string GetExpectedPageName()
		{
			return WebTracker.Pages.ResetPassword;
		}

		protected override Control GetNewControl()
		{
			return new ResetPasswordForTest();
		}
	}
}
