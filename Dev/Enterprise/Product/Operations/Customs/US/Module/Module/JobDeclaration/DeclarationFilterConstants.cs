
namespace Enterprise.Customs.US.Module
{
	public class DeclarationFilterConstants : Customs.Module.DeclarationFilterConstants
	{
		public static class MessageStatus
		{
			public const string NotSentForFilter = "NOT";
			public const string NotSentForFilterDescription = "Not Sent - only valid for exact match";
		}

		public const string NotYetReconciled = "Not Yet Reconciled";
		public const string Flagged = "Flagged";
		public const string Exclude = "Exclude";
		public const string ALL = "ALL";
		public const string Incomplete = "Incomplete";
		public const string TIBExpiryDate = "TIB Expiry Date";
		public const string EntryReleaseDate = "Release Date";
		public const string EntrySubmittedDate = "Entry Submitted Date";
		public const string EntryType = "Entry Type";
		public const string ReconIssue = "Recon Issue";
		public const string ImporterOfRecord = Business.JobDeclaration.Constants.USFilterConstants.ImporterOfRecord;
		public const string ITNumber = "IT Number";
		public const string ITDate = "IT Date";
		public const string ReleaseStatus = Business.JobDeclaration.Constants.USFilterConstants.ReleaseStatus;
		public const string CargoReleaseStatus = "CRL (Cargo Release) Status";
		public const string SimplifiedEntryBillStatus = "ACE Cargo Release Bill Status";
		public const string BillHoldOrExam = "Bill Hold or Exam";
		public const string EntrySummaryStatus = "ENS (Entry Summary) Status";
		public const string EntryMode = "Entry Mode";
		public const string CargoReleaseType = "Cargo Release Type";
		public const string ExportPort = "Export Port";
		public const string ExportDate = "Export Date";
		public const string ExportStatus = "EXP (Export) Status";
		public const string FTAReconIndicator = "FTA Recon";
		public const string ExcludeIORFilingTheirOwnRec = "Exclude IOR Filing Their Own Recon";
		public const string PortOfEntry = Business.JobDeclaration.Constants.USFilterConstants.PortOfEntry;
		public const string Carrier = "Carrier";
		public const string CarrierSCAC = "Carrier SCAC";
		public const string LocationOfGoods = "Location of Goods (FIRMS Code)";
		public const string ElectronicInvoiceStatus = "EI (Electronic Invoice) Status";
		public const string BLUMessageStatus = "BLU Status";
		public const string FDAMsgStatus = "OGA FDA Msg.Status";
		public const string FDAStatus = "OGA FDA Status";
		public const string StatementStatus = "Statement Status";
		public const string PaymentStatus = "Payment Status";
		public const string TotalDutiesAndFees = "Total Duties & Fees";
		public const string Reconciliation = "Reconciliation";
		public const string PaymentDueDate = "Payment Due Date";
		public const string Filer = "Entry Filer";
		public const string CRLEnabled = "3461 Enabled";
		public const string ENSEnabled = "7501 Enabled";
		public const string Audited = "Audited";
		public const string AuditRequired = "Audit Required";
		public const string SPINotApplicableAudit = "SPI Audit (Not Claimed)";
		public const string SPINotApplicableAuditDate = "SPI Audit Date";
		public const string CWNotAuditedAudit = "CW Audit (Not Audited)";
		public const string CWOAuditDate = "CW Audit Date";
		public const string FilingOption = "Filing Option";
		public const string InvoiceExportDate = "Invoice Export Date � Slow Search";
		public const string BondNumber = "Bond Producer Acc #";

		public const string FDANotApplicableAudit = "FDA Audit (Disclaimed)";
		public const string FDANotApplicableAuditDate = "FDA Audit Date";
		public const string Paperless = "Paperless Entry";

		public const string Closed = "Closed";
		public const string ClosingRequired = "Closing Required";
		public const string TIBClosed = "TIB Closed";
		public const string TIBClosedDate = "TIB Closed Date";

		public const string TotalOutstanding = "Total Outstanding";
		public const string TotalInvoiced = "Total Invoiced (DSB)";
		public const string TotalBilled = "Total Billed";
		public const string LiquidationDate = "Liquidation Date";
		public const string PrelimStatemPrintDate = "Preliminary Statement Print Date";
		public const string StatementNo = "Statement #";
		public const string PaymentType = "Payment Type";
		public const string PaymentByBroker = "Payment By Broker";
		public const string EstimatedEntryDate = "Estimated Entry Date";
		public const string DeferredTaxDueDate = "Deferred Tax Due Date";
		public const string LoadingSchedDK = "Loading Port(Sched D/K)";
		public const string DischargeSchedDK = "Discharge Port(Sched D/K)";
		public const string BIRDBrokerRef = "BIRD Broker Ref";
		public const string ElectronicInvoiceRequested = "EI (Electronic Invoice) Requested";
		public const string EntryDate = "Date at Entry Port";
		public const string DeferredIndicator = "Tax Deferrable Ind.";
		public const string SoldEnRoute = "Sold En Route";
		public const string ConsolidatedJobNo = Business.JobDeclaration.Constants.USFilterConstants.ConsolidatedJobNo;

		public const string StatusNotificationDispositionCode = "7501 Notification Disposition Code";
		public const string EntrySummaryActions = "Entry Summary Actions (UC)";
		public const string CargoReleaseComments = "Cargo Release Comments";

		public const string ISFBillStatus = "ISF Bill Status";
		public const string ShipperReferenceNumber = "Shipper Reference # (AES)";
		public const string ApplicationCode = "Message Mode";
		public const string NotifyParty = "Notify Party";
		public const string SoldToParty = "Sold To Party";
		public const string PSCIndicator = "PSC Indicator";
		public const string PresentationDate = "Presentation Date";
		public const string UltinateConsignee = Business.JobDeclaration.Constants.USFilterConstants.UltinateConsignee;
		public const string CensusWarningsOverriden = "Census Warnings Overridden";
		public const string SuretyCode = "Surety Code";
		public const string AnticipLiquidationDate = "Anticipated Liquidation Date";
		public const string ImportSpecialistTeam = "Import Specialist Team Number";
		public const string ContainsSplitShipments = "Contains Split Shipments";

		public const string FTZAdmissionStatus = "FTZ Admission Status";
		public const string FTZConcurrenceStatus = "FTZ Concurrence Status";
		public const string FTZDeliveryOfGoodsStatus = "FTZ Delivery Of Goods Status";
		public const string FTZGoodsArrivalStatus = "FTZ Goods Arrival Status";
		public const string FTZPTTStatus = "FTZ Permit To Transfer Status";
		public const string FTZAdmissionNumber = "FTZ Admission #";

		public const string WarehouseEntryFiler = "Warehouse Entry Filer";
		public const string WarehouseEntryNumber = "Warehouse Entry #";

		public const string DISStatus = "DIS Status";
		public const string PGAStatus = "PGA Status";

		public const string BasicSTBDisposition = "Basic STB Disposition";
		public const string AdditionalBondDisposition = "Additional Bond Disposition";
		public const string ImporterEIN = "Importer EIN #";
		public const string ImporterOfRecordEIN = "Importer of Record EIN #";
		public const string FIRMSCode = "FIRMS Code";
		public const string PreparerDistrictPort = "Preparer District Port";
		public const string SPIInvLine = "SPI - Inv Line";
		public const string PGAReplaceUpdateNeeded = "PGA Replace/Update Needed";
		public const string PGACorrectionStatus = "PGA Correction Status";
		public const string PGAExpeditedRelease = "PGA Expedited Release";

		public const string IssuerScacBol = "Issuer SCAC + Bill of Lading Number";
		public const string ActionStatus = "Action Status";
		public const string QuotaStatus = "Quota Status";
		public const string EBondMessageStatus = "EBond Message Status";

		public const string AESSeverity = "AES Severity (current)";
		public const string AESResponseCode = "AES Response Code (current)";

		public static class ColumnnCaptions
		{
			public const string Discharge = "Discharge";
			public const string ReleaseDate = Business.JobDeclaration.Constants.USFilterConstants.ReleaseDate;
		}

		public static class FilterCodeAndDesc
		{
			public const string ALLMayProceedManuallyClosed = "All May Proceed/Manually Closed";
			public const string HasAnyPGAStatus = "Has Any PGA Status";
			public const string HasNoPGAStatus = "Has No PGA Status";
			public const string HasAnyManuallyClosed = "Has Any Manually Closed";
			public const string MayNotProceed = "May Not Proceed";
			public const string HoldIntact = "Hold Intact";
			public const string Space = "Space";
			public const string Blank = "Blank";
		}
	}
}
