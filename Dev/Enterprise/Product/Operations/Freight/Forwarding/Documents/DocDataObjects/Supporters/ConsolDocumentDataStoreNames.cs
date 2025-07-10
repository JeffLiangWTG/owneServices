namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects
{
	public static class ConsolDocumentDataStoreNames
	{
		#region SuppressResourceStringsCheckRegion

		public const string SeaBookingRequest2 = Business.ConsolDocumentDataStoreNames.SeaBookingRequest2;
		public const string CargoSecurityDeclaration = "CargoSecurityDeclaration";
		public const string PortbaseExportNotification = "PortbaseExportNotification";
		public const string PortbaseImportNotification = "PortbaseImportNotification";
		public const string ContainerLoadPlan = "ContainerLoadPlan";
		public const string ShippingOrder = "ShippingOrder";
		public const string ETerminalReleaseManifest = "eTerminalReleaseManifest";
		public const string ContainerGrossWeightVerification = "ContainerGrossWeightVerification2";
		public const string AdvancedManifestBR = "AdvancedManifestBR";
		public const string AdvancedManifestUS = "ACASHouseChecklist";
		public const string AirBookingRequest = "AirBooking";
		public const string CargoDuesImport = "Cargo Dues 2 - Import";
		public const string CargoDuesExport = "Cargo Dues 2 - Export";
		public const string CargoDuesLoadCoastwise = "Cargo Dues 2 - Load Coastwise";
		public const string CargoDuesDischargeCoastwise = "Cargo Dues 2 - Discharge Coastwise";
		public const string TMiningSecureContainerRelease = "TMiningSecureContainerRelease";
		public const string DraftBillOfLading = "DraftBillOfLading";
		public const string ExportPreAdviceNotification = "Export Pre-Advice Notification";

		#region FR

		public const string DTI = "FR_DTI";
		public const string DTE = "FR_DTE";

		public const string ProvisionalUnpackingListLPD = "FR_LPD";
		public const string PortsContainerAdviceToBookingAMQ = "FR_AMQ";
		public const string OutturnReportCDM = "FR_CDM";
		public const string FinalContainerManifestLDE = "FR_LDE";

		public const string DemandeDeTracingImport = "FR_TRC_IMP";
		public const string DemandeDeTracingExport = "FR_TRC_EXP";

		public const string DossierImport = "FR_DOS_CON_IMP";
		public const string DossierExport = "FR_DOS_CON_EXP";

		#endregion

		#region BE

		public const string DangerousGoodsNotificationExport = "BEDangerousGoodsNotification_EXP";
		public const string DangerousGoodsNotificationImport = "BEDangerousGoodsNotification_IMP";
		public const string BEEDeskExportNotification = "BEEDeskExportNotification";
		public const string BECertifiedPickup = "BECertifiedPickup";

		#endregion

		#region DE

		public const string DEAdvancedLogisticsPortOrder = "DEAdvancedLogisticsPortOrder";

		#endregion

		#region NL

		public const string CGNExportNotification = "CGNExportNotification";

		#endregion

		#region IL

		public const string ILGatePassMovement = "IL_GatePassMovement";

		#endregion IL

		#endregion
	}
}
