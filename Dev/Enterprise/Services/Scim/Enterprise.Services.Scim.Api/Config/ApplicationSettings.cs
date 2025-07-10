namespace Enterprise.Services.Scim.Api.Config
{
	/// <summary>
	/// Scim Service APIs application configurations.
	/// </summary>
	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Setting strings")]
	public static class ApplicationSettings
	{
		public const string ServerName = "ServerName";
		public const string DatabaseName = "DatabaseName";
		public const string Issuer = "Issuer";
		public const string AudienceId = "AudienceId";
		public const string MaxResults = "MaxResults";
		public const string SchemaPath = "SchemaPath";
		public const string KnownEndpointPath = "KnownEndpointPath";
		public const string ShowPii = "ShowPii";
		public const string ThrottleTimeInSeconds = "ThrottleTimeInSeconds";
		public const string ThrottleMaxRequestCount = "ThrottleMaxRequestCount";
		public const string ThrottleSkippedIps = "ThrottleSkippedIps";
		public const string SafelistSkippedIps = "SafelistSkippedIps";
	}
}
