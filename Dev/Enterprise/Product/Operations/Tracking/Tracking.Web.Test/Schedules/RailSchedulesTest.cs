using Enterprise.Tracking.Business;
using Enterprise.Tracking.Web.Testing;
using Enterprise.ZArchitecture.Web.Business.Testing;
using Enterprise.ZArchitecture.Web.GUI;

namespace Enterprise.Tracking.Web.Schedules.Testing
{
	[HttpContextEnabledTest]
	sealed class RailSchedulesTest : BaseTrackingPageTest
	{
		protected override string GetExpectedPageName()
		{
			return WebTracker.Pages.RailSchedules;
		}

		protected override System.Web.UI.Control GetNewControl()
		{
			return new RailSchedulesForTest();
		}

		class RailSchedulesForTest : RailSchedules
		{
			protected override ZGlobal GetNewTestGlobal()
			{
				return new TestGlobal();
			}
		}
	}
}
