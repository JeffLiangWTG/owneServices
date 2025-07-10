using System;

namespace CargoWise.Blazor.Common
{
	/// <summary>
	/// Options for the instance of CargoWise running
	/// eg Database and branding options
	/// </summary>
	public class CargoWiseOptions
	{
		/// <summary>
		/// The name of the Server where the Database is hosted
		/// </summary>
		public string DbServerName { get; set; } = Environment.MachineName;

		/// <summary>
		/// Database name
		/// </summary>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Literal string is safe to use in this context.")]
		public string DatabaseName { get; set; } = "Odyssey";

		/// <summary>
		/// Client Name
		/// example: EDI
		/// </summary>
		public string ClientName { get; set; } = string.Empty;

		/// <summary>
		/// Domain Name of the authority URI
		/// </summary>
		public string Hostname { get; set; } = string.Empty;

		/// <summary>
		/// Override for the App Server Path
		/// </summary>
		public string AppServerPathOverride { get; set; }

		public string Persist { get; set; }

		public Guid? VersionBrokerProcessCorrelationId { get; set; }

		public Guid? SessionBrokerProcessCorrelationId { get; set; }

		public Guid? AppServerProcessCorrelationId { get; set; }

		/// <summary>
		/// If set to true from the command line/env the AppServer will attempt to read configuration from std-in
		/// Should be used for sensitive key passing
		/// </summary>
		public bool ReadConfigFromStdIn { get; set; }

		/// <summary>
		//  Gets or sets a value that determines the maximum duration state for a disconnected
		//  circuit is retained on the server.
		//  When a client disconnects, ASP.NET Core Components attempts to retain state on
		//  the server for an interval. This allows the client to re-establish a connection
		//  to the existing circuit on the server without losing any state in the event of
		//  transient connection issues.
		//  This value determines the maximum duration circuit state is retained by the server
		//  before being evicted.
		/// </summary>
		public TimeSpan DisconnectedCircuitRetentionPeriod { get; set; } = TimeSpan.FromMinutes(10);

		/// <summary>
		/// This is a workaround for variable test timings (especially on DAT).  We want to use a timeout that is
		/// less than 10 minutes for tests so that the tests run in a reasonable timeframe, however this leads to a
		/// race condition where the app can get shut down before the Playwright browser is ready to go.
		/// To workaround this issue, the initial timeout on app launch is configured to be higher.
		/// </summary>
		public TimeSpan CircuitNeverOpenedShutdownTimeLimit { get; set; } = TimeSpan.FromMinutes(10);

		public int BackChannelRetryCount { get; set; } = 2;
	}
}
