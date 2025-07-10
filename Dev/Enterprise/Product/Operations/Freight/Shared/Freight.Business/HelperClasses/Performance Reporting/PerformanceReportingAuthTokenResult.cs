namespace Enterprise.Freight.Business
{
	public class PerformanceReportingAuthTokenResult : IPerformanceReportingAuthTokenResult
	{
		public PerformanceReportingAuthTokenResult(IPerformanceReportingAuthToken token, string errorMessage = null)
		{
			Token = token;
			ErrorMessage = errorMessage;
		}

		public IPerformanceReportingAuthToken Token { get; }
		public string ErrorMessage { get; }
	}
}
