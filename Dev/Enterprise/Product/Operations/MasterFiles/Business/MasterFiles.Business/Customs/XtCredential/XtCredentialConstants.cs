namespace Enterprise.MasterFiles.Business.Customs.XtCredential
{
	public static class XtCredentialConstants
	{
		public const string CustomsCredentialChange = nameof(CustomsCredentialChange);

		public static class MessageSubTypes
		{
			public const string New = "NEW";
			public const string Update = "UPD";
			public const string Delete = "DEL";
		}

		public static class IdentifierList
		{
			public const string CLC = "CLC";
			public const string JPC = "JPC";
		}
	}
}
