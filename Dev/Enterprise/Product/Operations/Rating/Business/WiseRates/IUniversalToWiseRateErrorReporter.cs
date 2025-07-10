namespace Enterprise.Rating.Business.WiseRates
{
	/// <summary>
	/// Silently logs errors when mapping from URS to WiseRate data types.
	/// </summary>
	/// <remarks>Generally this reflects some incorrect assumption in the URS
	/// data - we filter out the anomalous data for the user's results, and
	/// report the error for investigation.</remarks>
	public interface IUniversalToWiseRateErrorReporter
	{
		/// <summary>
		/// Silently log mapping error
		/// </summary>
		/// <param name="errorMessage">Error message that contains reason for failure</param>
		/// <param name="functionName">Name of function in which error was generated</param>
		/// <param name="mappingSources">URS objects from which we are trying to map</param>
		void ReportMappingError(string errorMessage, string functionName, params (string name, object obj)[] mappingSources);
	}
}
