namespace CargoWise.RefDbRepo.ILReferenceData.Services
{
	public static class Constants
	{
		public static class ProgramFunctions
		{
			public const string CustomsTableUpdate = "CUSTOMS_TABLE_UPDATE";
			public const string CustomsExchangeRateUpdate = "CUSTOMS_EXCHANGE_RATE_UPDATE";
			public const string CustomsTariffUpdate = "CUSTOMS_TARIFF_UPDATE";

			public const string Receive = "RECEIVE";
		}

		public static class XtMessageInfo
		{
			public const string ApplicationCode = "ILC";
			public const string MessageType = "GEN";
			public const string DestinationParty = "ILCUSTOMS";
			public const string SourceParty = "ILREFDATA";
		}
	}
}
