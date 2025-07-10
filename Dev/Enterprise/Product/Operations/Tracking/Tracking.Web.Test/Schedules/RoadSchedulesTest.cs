using Enterprise.Tracking.Business;
using Enterprise.Tracking.Web.Testing;
using Enterprise.ZArchitecture.Web.Business.Testing;
using Enterprise.ZArchitecture.Web.GUI;

namespace Enterprise.Tracking.Web.Schedules.Testing
{
	[HttpContextEnabledTest]
	sealed class RoadSchedulesTest : BaseTrackingPageTest
	{
		protected override string GetExpectedPageName()
		{
			return WebTracker.Pages.RoadSchedules;
		}

		protected override System.Web.UI.Control GetNewControl()
		{
			return new RoadSchedulesForTest();
		}

		class RoadSchedulesForTest : RoadSchedules
		{
			protected override ZGlobal GetNewTestGlobal()
			{
				return new TestGlobal();
			}
		}
	}
}
