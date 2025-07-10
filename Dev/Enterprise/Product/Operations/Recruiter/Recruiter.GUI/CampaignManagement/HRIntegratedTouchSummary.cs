using Enterprise.MarketingManager.GUI;

namespace Enterprise.Recruiter.GUI
{
	class HRIntegratedTouchSummary : IntegratedTouchSummary
	{
		protected override void SetupTrackingStatusControl()
		{
			TrackingStatusControl = new HRTrackingStatusChartUserControl();
		}
	}
}
