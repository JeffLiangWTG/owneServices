using Enterprise.Tracking.Business;
using Enterprise.Tracking.Web.Testing;
using Enterprise.ZArchitecture.Web.Business.Testing;
using Enterprise.ZArchitecture.Web.GUI;

namespace Enterprise.Tracking.Web.Schedules.Testing
{
	[HttpContextEnabledTest]
	sealed class SailingSchedulesTest : BaseTrackingPageTest
	{
		protected override string GetExpectedPageName()
		{
			return WebTracker.Pages.SailingSchedules;
		}

		protected override System.Web.UI.Control GetNewControl()
		{
			return new SailingSchedulesForTest();
		}

		class SailingSchedulesForTest : SailingSchedules
		{
			protected override ZGlobal GetNewTestGlobal()
			{
				return new TestGlobal();
			}
		}
	}
}
