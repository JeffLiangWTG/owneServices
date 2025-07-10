using System.Web.UI;
using Enterprise.Tracking.Business;
using Enterprise.Tracking.Web.Admin;
using Enterprise.ZArchitecture.Web.Business.Testing;
using Enterprise.ZArchitecture.Web.GUI;

namespace Enterprise.Tracking.Web.Testing
{
	[HttpContextEnabledTest]
	sealed class SetPasswordTest : BaseTrackingPageTest
	{
		protected override string GetExpectedPageName()
		{
			return WebTracker.Pages.SetPassword;
		}

		protected override Control GetNewControl()
		{
			return new SetPasswordForTest();
		}

		class SetPasswordForTest : SetPassword
		{
			protected override ZGlobal GetNewTestGlobal()
			{
				return new TestGlobal();
			}
		}
	}
}
