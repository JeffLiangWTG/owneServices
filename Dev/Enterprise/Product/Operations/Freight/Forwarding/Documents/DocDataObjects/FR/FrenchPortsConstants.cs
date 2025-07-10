namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.FR
{
	#region SuppressResourceStringsCheckRegion

	static class FrenchPortsConstants
	{
		public static class PCS
		{
			public const string MGI = "MGI";
			public const string Soget = "SOGET";
		}

		public static class DocumentNames
		{
			public const string DOSImport = "File Creation Request (DOS) - Import";
			public const string DOSExport = "File Creation Request (DOS) - Export";
			public const string CAEDImport = "Customs Clearance Check (CAED) - Import";
			public const string CAEDExport = "Customs Clearance Check (CAED) - Export";
			public const string ProvisionalUnpackingListLPD = "Provisional Unpacking List (LPD)";
			public const string PortsContainerAdviceToBookingAMQ = "Container Advice to Booking (AMQ)";
			public const string PortsGoodsReceivedCRESA = "Goods Received (CRESA)";
			public const string CINExportNotification = "Export Notification (755)";

			public const string DTI = "Underbond Movement Request - Import (DTI)";
			public const string DTE = "Underbond Movement Request - Export (DTE)";

			public const string FinalContainerManifestLDE = "Final Container Manifest (LDE)";
			public const string OutturnReportCDM = "Outturn Report (CDM)";
			public const string GoodsReceivedCRESA = "Goods Received (CRESA)";

			public const string TracingImport = "Tracing Request (TRC) - Import";
			public const string TracingExport = "Tracing Request (TRC) - Export";
		}
	}

	#endregion
}
