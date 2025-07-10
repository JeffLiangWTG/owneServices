namespace Enterprise.Customs.TR.NCTS.Business
{
	public static class NCTSMessageProviderConstants
	{
		public static class NCTSHeader
		{
			public const string SyntaxIdentifier = "UNOC";
			public const int SyntaxVersionNumber = 3;
			public const string CountryMapType = "CNTRY";
			public const string ZeroValue = "0";
		}

		public static class Messages
		{
			public const string MessageSender = "NTA.TR";
			public const string MessageRecipient = "NTA.TR";
			public const string MessageType = "CC015B";
		}

		public static class PreviousDocuments
		{
			public const string WarehouseCode = "ANT";
		}
	}
}
