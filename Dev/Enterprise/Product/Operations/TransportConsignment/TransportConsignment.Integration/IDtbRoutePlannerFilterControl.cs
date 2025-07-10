using System;

namespace Enterprise.TransportConsignment.Integration
{
	public interface IDtbRoutePlannerFilterControl
	{
		void UpdateResultCountMessage(bool isLoadSuccessfully);
		DtbRoutePlannerViewMode ViewMode { get; set; }
		bool HasSearchBeenRun { get; }
		void SetViewModeWithoutRunningSearch(DtbRoutePlannerViewMode viewMode);
		event EventHandler PerformingSearch;
		event EventHandler PerformedSearch;
		void AddChildFilterControl(IDtbFilterControl filterControl);
		void PerformSearchOnChildren();
	}
}
