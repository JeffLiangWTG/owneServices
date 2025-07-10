using Enterprise.Tracking.Business;
using Enterprise.ZArchitecture.Web.Business.Testing;
using Enterprise.ZArchitecture.Web.GUI;

namespace Enterprise.Tracking.Web.Testing
{
	[HttpContextEnabledTest]
	sealed class ForgotPasswordPageTest : BaseTrackingPageTest
	{
		public void TestNotCacheable()
		{
			var page = new ForgotPasswordForTest();

			Assert(!page.CacheableForTest);
		}

		protected override string GetExpectedPageName()
		{
			return WebTracker.Pages.ForgotPassword;
		}

		protected override System.Web.UI.Control GetNewControl()
		{
			return new ForgotPasswordForTest();
		}

		class ForgotPasswordForTest : ForgotPassword
		{
			public bool CacheableForTest => Cacheable;

			protected override ZGlobal GetNewTestGlobal()
			{
				return new TestGlobal();
			}
		}
	}
}
