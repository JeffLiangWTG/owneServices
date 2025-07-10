namespace CargoWise.RefDbRepo.ILReferenceData.Business
{
	public static class Constants
	{
		public static class MessageSubType
		{
			public const string ExRate = "347";
			public const string CustomCodes = "901";
		}

		public static class ServiceName
		{
			public const string ExRate = "GetCD_8347_8348_Web01_02_CurrencyRateSearch";
			public const string CustomCodes = "GetSYSTBL_MSG9000_9001_SystemTableRequest";
		}

		public static class CustomsRequestHeader
		{
			public const string SoftwareProvider = "WTG";
			public const string SoftwareVersion = "1.0";
			public const string WSDLVersion = "1.0";
		}

		public static class DataSetNames
		{
			public const string TradeGroups = "TradeGroupsResponse";
			public const string TradeGroupCountries = "TradeGroupCountriesResponse";
		}

		public static class ReceiveHandlerProcessor
		{
			public const string UnexpectedDataSetResponse = "TableAsDataSetTableData is null or empty";
		}

		public static class TableNames
		{
			public const string TradeGroups = "2009";
			public const string TradeGroupCountries = "23689";
		}
	}
}
