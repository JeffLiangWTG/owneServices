using Enterprise.Tracking.Business;
using Enterprise.Tracking.Web.Testing;
using Enterprise.ZArchitecture.Web.Business.Testing;
using Enterprise.ZArchitecture.Web.GUI;

namespace Enterprise.Tracking.Web.Schedules.Testing
{
	[HttpContextEnabledTest]
	sealed class FlightSchedulesTest : BaseTrackingPageTest
	{
		protected override string GetExpectedPageName()
		{
			return WebTracker.Pages.FlightSchedules;
		}

		protected override System.Web.UI.Control GetNewControl()
		{
			return new FlightSchedulesForTest();
		}

		class FlightSchedulesForTest : FlightSchedules
		{
			protected override ZGlobal GetNewTestGlobal()
			{
				return new TestGlobal();
			}
		}
	}
}
