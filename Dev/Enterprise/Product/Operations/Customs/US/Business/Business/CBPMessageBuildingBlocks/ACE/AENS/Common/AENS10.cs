using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.Business.BIRD.Common;

namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Common
{
	public partial class AENS10 : Abstract.AENS10, IBIRDHeaderRecord, IBIRDHeaderIDRecord, IBIRDBrokerRefernceRecord
	{
		#region IBIRDHeaderRecord Members

		void IBIRDHeaderRecord.Update(JobDeclaration declaration, INotifications notifications)
		{
			declaration.US_SchDEntry = DistrictPortOfEntry;
			if (!EntryFilerCode.IsEmpty)
			{
				declaration.US_EntryFilerCode = EntryFilerCode;
			}
			declaration.ImportEntryNumber = EntryNumber;
			declaration.US_EntryType = EntryTypeCode;
			declaration.US_LiveEntryIndicator = LiveEntryIndicator;

			BIRDTransportMode.SetTransportMode(declaration, ModeOfTransportationMOTCode, notifications);

			if (PGAExpeditedReleaseIndicator == "F")
			{
				declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFTZ;
				declaration.US_EntryDateElectionCode = EntryDateElectionCodeList.Codes.WeeklyEstimateFilingDate;
			}

			declaration.US_ConsolACE = ConsolidatedSummaryIndicator == "Y";
			declaration.US_NAFTAReconIndicator = TradeAgreementReconciliationIndicator == "Y";
			declaration.US_OtherReconIndicator = ReconciliationIssueCode.IsEmpty ? ReconIssueCodeList.Codes.NotApplicable : ReconIssueCodeList.ConvertFromENSIssueCode(ReconciliationIssueCode);
			declaration.US_ConsolidatedInformalIndicator = ShipmentUsageTypeCode;
			declaration.US_TaxDeferIndicator = DeferredTaxPaymentCode.IsEmpty ? TaxDeferIndicatorList.Codes.NotApplicableOrNoDeferredTax : (string)DeferredTaxPaymentCode;
			declaration.US_PaymentType = PaymentTypeCode;
			declaration.US_PreliminaryStatementPrintDate = PreliminaryStatementPrintDate;
			declaration.US_PeriodicStatementMM = PeriodicStatementMonth;
			declaration.US_ClientBranchDesignation = StatementClientBranchIdentifier;
			declaration.US_BondWaiverCode = BondWaiverReasonCode;
			declaration.US_PSC = PostSummaryCorrectionIndicator == "Y";
			declaration.US_AccLiqReq = AcceleratedLiquidationRequestIndicator == "Y";

			var iorWrapper = declaration.IORWrapper;
			if (iorWrapper != null && !KnownImporterIndicator.IsEmpty)
			{
				if (!iorWrapper.ZO_KnwImpInd.EqualsIgnoringCase(KnownImporterIndicator))
				{
					notifications.AddWarning(ZString.Format("The value for Importer of Record > Known Importer in this BIRD transaction is '{0}' and it is different to a value set in the organization. Please check and set it manually if required.", KnownImporterIndicator));
				}
			}
		}

		#endregion

		ZString IBIRDHeaderIDRecord.EntryFilerCode
		{
			get { return EntryFilerCode; }
		}

		ZString IBIRDHeaderIDRecord.EntryNumber
		{
			get { return EntryNumber; }
		}

		ZString IBIRDBrokerRefernceRecord.BrokerReferenceNumber
		{
			get { return BrokerReferenceNumber; }
		}
	}
}
