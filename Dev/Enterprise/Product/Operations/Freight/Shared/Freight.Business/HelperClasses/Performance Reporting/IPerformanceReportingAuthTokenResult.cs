namespace Enterprise.Freight.Business
{
	public interface IPerformanceReportingAuthTokenResult
	{
		IPerformanceReportingAuthToken Token { get; }
		string ErrorMessage { get; }
	}
}
