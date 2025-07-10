namespace Enterprise.Rating.Web.Configuration
{
	/// <summary>
	/// A public interface to Rates APIs configurable application settings
	/// </summary>
	public interface IRatesAPIsAppSettings
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
		/// TokenExpiryDurationInSeconds
		/// </summary>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:UseTimespanForDuration", Justification = "Baseline")]
		int TokenExpiryDurationInSeconds { get; }

		/// <summary>
		/// SupportJsonMediaType
		/// </summary>
		bool SupportJsonMediaType { get; }

		/// <summary>
		/// ShowExceptionDetailsInResponse
		/// </summary>
		bool ShowExceptionDetailsInResponse { get; }
	}
}
