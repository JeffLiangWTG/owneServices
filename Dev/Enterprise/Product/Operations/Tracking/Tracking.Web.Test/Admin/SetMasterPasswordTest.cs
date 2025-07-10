using System.Web.UI;
using Enterprise.Tracking.Business;
using Enterprise.ZArchitecture.Web.Business.Testing;

namespace Enterprise.Tracking.Web.Testing
{
	[HttpContextEnabledTest]
	sealed class SetMasterPasswordTest : BaseTrackingPageTest
	{
		protected override string GetExpectedPageName()
		{
			return WebTracker.Pages.SetMasterPassword;
		}

		protected override Control GetNewControl()
		{
			return new SetMasterPasswordForTest();
		}
	}
}
