namespace Enterprise.Customs.NO.Registry;

static class FTPSettingsEMMADocRegistry
{
	#region DefaultValues

	public static FTPSettingsRegistry DefaultValues => new()
	{
		Url = Constants.URL,
		Port = Constants.Port,
	};

	static class Constants
	{
		public const string URL = "ftpedoc.emma.no";
		public const string Port = "21";
	}

	#endregion
}
