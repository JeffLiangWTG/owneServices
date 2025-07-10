using WTG.StaticAnalysis.Annotation;

namespace CargoWise.RefDataRepo.Ent.Client
{
#if DEBUG
	public
#endif
	static class ErrorReportingKnownExceptionsMapping
	{
		[ThreadSafe]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Exception messages")]
		public static readonly string[] IgnoredExceptionMessages = new[] {
			"An exception has been raised that is likely due to a transient failure. If you are connecting to a SQL Azure database consider using SqlAzureExecutionStrategy",
			"A network-related or instance-specific error occurred while establishing a connection to SQL Server. The server was not found or was not accessible. Verify that the instance name is correct and that SQL Server is configured to allow remote connections.",
			"A connection was successfully established with the server, but then an error occurred during the pre-login handshake.",
			"The underlying provider failed on Open",
			"The wait completed due to an abandoned mutex.",
			"End of Stream encountered before parsing was completed."
		};
	}
}
