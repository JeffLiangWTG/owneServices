namespace Enterprise.Services.Scim.Api.Config
{
	/// <summary>
	/// A public interface to SCIM.Service APIs configurable application settings
	/// </summary>
	public interface IAppSettings
	{
		/// <summary>
		/// CW1's DB Server Name
		/// </summary>
		string ServerName { get; }

		/// <summary>
		/// CW1's Database Name
		/// </summary>
		string DatabaseName { get; }

		/// <summary>
		/// Scim find API endpoint max results per request
		/// </summary>
		string MaxResults { get; }

		/// <summary>
		/// SCIM resource schema definitions path
		/// </summary>
		string SchemaPath { get; }

		/// <summary>
		///  Flag to enable showing exception messages regarding JWT middleware
		/// </summary>
		bool ShowPii { get; }

		/// <summary>
		/// Time in seconds to throttle requests
		/// </summary>
		int ThrottleTimeInSeconds { get; }

		/// <summary>
		/// Maximum number of requests allowed within the throttle time
		/// </summary>
		int ThrottleMaxRequestCount { get; }

		/// <summary>
		/// List of IP addresses to skip throttling
		/// </summary>
		string ThrottleSkippedIps { get; }

		/// <summary>
		/// List of IP addresses to skip safelisting
		/// </summary>
		string SafelistSkippedIps { get; }
	}
}
