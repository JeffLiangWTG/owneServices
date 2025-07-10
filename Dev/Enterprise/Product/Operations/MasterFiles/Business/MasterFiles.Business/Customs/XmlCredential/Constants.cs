namespace Enterprise.MasterFiles.Business.Customs.XmlCredential
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Constant values")]
	public static class Constants
	{
		public static class GroupTypes
		{
			public const string CompanyType = "Company";
			public const string SystemType = "System";
			public const string StaffType = "Staff";
			public const string GroupType = "Group";
			public const string BranchType = "Branch";
		}

		public static class Configuration
		{
			public const string Version = "1.0";
			public const string EHubRecipient = "eHub";
			public const string XHRecipient = "CustomsConfiguration";
		}

		public static class IdentifierList
		{
			public const string IC2 = "IC2";
		}

		public static class CredentialDetails
		{
			public const string Current = "Current";
			public const string NextPassword = "Next";
		}

		public static class CertificateDetails
		{
			public const string Name = "Certificate";
		}

		public static class ItemTypes
		{
			public const string UserName = "UserName";
			public const string MailBoxID = "MailBoxID";
			public const string Message = "Message";
			public const string TokenType = "TokenType";
			public const string Token = "Token";
			public const string Issued = "Issued";
			public const string Expires = "Expires";
			public const string Platform = "Platform";
			public const string ReceiveAutomatically = "ReceiveAutomatically";
		}

		public static class CredentialStatusList
		{
			public const string Valid = "VAL";
			public const string Invalid = "INV";
		}
	}
}
