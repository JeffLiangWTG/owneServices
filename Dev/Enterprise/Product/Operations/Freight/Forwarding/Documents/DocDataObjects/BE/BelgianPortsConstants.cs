using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.BE
{
	#region SuppressResourceStringsCheckRegion

	static class BelgianPortsConstants
	{
		public static class DocumentNames
		{
			public const string IFTDGNImport = "Dangerous Goods Notification - Import";
			public const string IFTDGNExport = "Dangerous Goods Notification - Export";
			public const string ExportNotification = "Export Notification (EBADEC)";
			public const string CPuReleaseRightAcceptDecline = CertifiedPickupConstants.DocumentNames.AcceptDecline;
			public const string CPuReleaseRightTransfer = CertifiedPickupConstants.DocumentNames.Transfer;
			public const string CPuReleaseRightRevoke = CertifiedPickupConstants.DocumentNames.Revoke;
		}

		public static class EuOfficeCodesTypes
		{
			public const string OfficeOfExit = "EXT";
			public const string ActualExitOffice = "AEC";
		}

		public static class HandlingInstructions
		{
			public const string Discharge = "LDI";
			public const string Loading = "LLO";
		}

		public static class ContextCollectionTypes
		{
			public const string ReleaseFromParty = "ReleaseFromParty";
			public const string ReleaseFromPartyId = "ReleaseFromPartyId";
			public const string ReleaseFromPartyCode = "ReleaseFromPartyCode";
		}

		public static class AddInfoCollectionTypes
		{
			public const string OperationalPort_Code = "OperationalPort_Code";
			public const string Terminal_Code = "Terminal_Code";
		}

		public static class CertifiedPickupMenuItemName
		{
			public const string AcceptDecline = "Accept/Decline";
			public const string Transfer = "Transfer";
			public const string Revoke = "Revoke";
		}
	}

	#endregion
}
