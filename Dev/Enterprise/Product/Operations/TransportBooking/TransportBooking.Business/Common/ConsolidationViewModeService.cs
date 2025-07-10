using CargoWise.EntityFramework;

namespace Enterprise.TransportBookings.Business
{
	public enum ConsolidationViewMode
	{
		SingleJob,
		MultiJob
	}

	public class ConsolidationViewModeService
	{
		public static ConsolidationViewMode GetViewMode(BusinessObjectFactory factory)
		{
			return factory.GetCachedValue<ConsolidationViewModeService>().ViewMode;
		}

		public static void SetViewMode(BusinessObjectFactory factory, ConsolidationViewMode viewMode)
		{
			factory.GetCachedValue<ConsolidationViewModeService>().ViewMode = viewMode;
		}

		ConsolidationViewMode ViewMode = ConsolidationViewMode.SingleJob;
	}
}
