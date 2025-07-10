namespace CargoWise.RefDbRepo.Common.ErrorReporting
{
	public static class ErrorReportingKnownExceptionsMapping
	{
		public readonly static string[] SQLConnectionExceptionMessages = new[] {
			"An exception has been raised that is likely due to a transient failure. If you are connecting to a SQL Azure database consider using SqlAzureExecutionStrategy",
			"A network-related or instance-specific error occurred while establishing a connection to SQL Server. The server was not found or was not accessible. Verify that the instance name is correct and that SQL Server is configured to allow remote connections.",
			"A connection was successfully established with the server, but then an error occurred during the pre-login handshake.",
			"The underlying provider failed on Open",
			"The wait completed due to an abandoned mutex.",
			"An error occurred while starting a transaction on the provider connection. See the inner exception for details.",
			"Invalid operation. The connection is closed.",
			"A transport-level error has occurred when receiving results from the server. (provider: Session Provider, error: 19 - Physical connection is not usable)",
			"SHUTDOWN is in progress.",
			"Failed to fetch"
		};

		public readonly static string ErrorReportingTestMessage = "This is an issue created by Test Fixtures";
	}
}
