using Enterprise.TransportBookings.Business.Testing;

namespace Enterprise.TransportBookings.Business.Test.Common
{
	public class ConsolidationViewModeServiceTest : DtbBookingTestCaseWithFactory
	{
		public void TestGetAndSetViewMode()
		{
			AssertEquals("Default ViewMode should be Single-Job Consolidation.", ConsolidationViewMode.SingleJob, ConsolidationViewModeService.GetViewMode(Factory));

			ConsolidationViewModeService.SetViewMode(Factory, ConsolidationViewMode.MultiJob);
			AssertEquals(ConsolidationViewMode.MultiJob, ConsolidationViewModeService.GetViewMode(Factory));

			ConsolidationViewModeService.SetViewMode(Factory, ConsolidationViewMode.SingleJob);
			AssertEquals(ConsolidationViewMode.SingleJob, ConsolidationViewModeService.GetViewMode(Factory));
		}
	}
}
