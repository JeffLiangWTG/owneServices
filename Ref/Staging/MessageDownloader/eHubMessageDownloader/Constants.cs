using CargoWise.RefDbRepo.Staging.Schema_New;

namespace CargoWise.RefDbRepo.Staging.eHubMessageDownloader
{
	public static class Constants
	{
		public const string SourceError = "ERR";
		public const string SubSourceUnknown = "UNKNOWN";
		public const string RefDataRepoMessage = "RDM";

		public const string ZATariffSubSource = DataSourceConstants.SubSource.ZaTariffs;
		public const string ZAExchangeRateSubSource = "ZA Exchange Rates";
		public const string PRODATApplicationReference = "PRODAT";
		public const string GESMESApplicationReference = "GESMES";

		public static class SupportedSchemaName
		{
			public const string ZACustoms = "ZACustoms";
			public const string GenericMessageDelivery = "http://cargowise.com/ehub/core/genericmessagedelivery#GenericMessageInterchange";
		}
	}
}
