namespace Enterprise.Services.ServiceHost
{
	public static class eAdaptorLogs
	{
		public static string AdaptorDisabled(string name)
		{
			return string.Format(eAdaptorDisabled, name);  // Reason
		}
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Reason")]
		const string eAdaptorDisabled = "{0} is disabled. If this is wrong please contact support.";
	}
}
