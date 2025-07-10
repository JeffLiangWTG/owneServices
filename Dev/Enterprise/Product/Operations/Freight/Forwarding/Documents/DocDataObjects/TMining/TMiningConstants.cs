namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects
{
	#region SuppressResourceStringsCheckRegion

	static class TMiningConstants
	{
		public static class DocumentNames
		{
			public const string TMiningSecureContainerRelease = Business.TMiningConstants.DocumentNames.TMiningSecureContainerRelease;
			public const string TMiningSecureContainerReleaseTransfer = Business.TMiningConstants.DocumentNames.TMiningSecureContainerReleaseTransfer;
			public const string TMiningSecureContainerReleaseRevoke = Business.TMiningConstants.DocumentNames.TMiningSecureContainerReleaseRevoke;
		}

		public static class SecureContainerReleaseMenuItemName
		{
			public const string Transfer = "Transfer";
			public const string Revoke = "Revoke";
		}

		public static class ParameterTypes
		{
			public const string ContainerRelease = "Container Release";
		}

		public static class AddInfoCollectionTypes
		{
			public const string OperationalPort_Code = "OperationalPort_Code";
		}
	}

	#endregion
}
