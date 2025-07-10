namespace CargoWise.RefDbRepo.Staging.Common
{
	public static class Application
	{
		const int defaultErrorReportMaxCount = 100;
		static int errorReportMaxCount;
		static bool hasLoadAppsetting;

		public static int ErrorReportMaxCount
		{
			get
			{
				if (!hasLoadAppsetting)
				{
					_ = int.TryParse(ApplicationConfig.ErrorReportMaxCount, out errorReportMaxCount);
					hasLoadAppsetting = true;
				}
				return errorReportMaxCount > 0 ? errorReportMaxCount : defaultErrorReportMaxCount;
			}
		}
	}
}
