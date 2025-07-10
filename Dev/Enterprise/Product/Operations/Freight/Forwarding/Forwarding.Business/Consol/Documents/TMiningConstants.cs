namespace Enterprise.Freight.Forwarding.Business
{
	public static class TMiningConstants
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Constant strings")]
		public static class DocumentNames
		{
			public const string TMiningSecureContainerRelease = "Secure Container Release";
			public const string TMiningSecureContainerReleaseTransfer = "Secure Container Release - Transfer";
			public const string TMiningSecureContainerReleaseRevoke = "Secure Container Release - Revoke";
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Constant strings")]
		public static class SecureContainerReleaseStatus
		{
			public const string Assigned = "Assigned";
			public const string Accepted = "Accepted";
			public const string TransferSentAwaitingResponse = "TransferSentAwaitingResponse";
			public const string TransferSent = "TransferSent";
			public const string TransferRejected = "TransferRejected";
			public const string RevokeSentAwaitingResponse = "RevokeSentAwaitingResponse";
			public const string RevokeSent = "RevokeSent";
			public const string RevokeRejected = "RevokeRejected";
			public const string Revoked = "Revoked";
			public const string NotApplicable = "";
		}

		public static class ParameterMessageTypes
		{
			public const string SecureContainerRelease = DocumentNames.TMiningSecureContainerRelease;
			public const string SecureContainerReleaseTransfer = DocumentNames.TMiningSecureContainerReleaseTransfer;
			public const string SecureContainerReleaseRevoke = DocumentNames.TMiningSecureContainerReleaseRevoke;
		}
	}
}
