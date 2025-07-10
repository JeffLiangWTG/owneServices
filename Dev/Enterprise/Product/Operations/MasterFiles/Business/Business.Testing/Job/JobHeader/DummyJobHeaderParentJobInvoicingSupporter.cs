using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.Integration.Accounting;
using Enterprise.Security;

namespace Enterprise.MasterFiles.Business.Testing
{
	public class DummyJobHeaderParentJobInvoicingSupporter : IJobInvoicingSupporter
	{
		public DummyJobHeaderParentJobInvoicingSupporter()
		{
			var paymentTermInfos = new PaymentTermInfos();
			paymentTermInfos.AddOrReplace(new PaymentTermInfo(PaymentTermType.DomesticPaymentTerm, CostSell.Revenue, Constants.DomesticPaymentTerms.Collect));
			PaymentTerm = paymentTermInfos;
			CanCreateInvoicingJob = true;
		}

		public virtual JobHeader Job
		{
			get { return null; }
		}

		public bool CanCreateInvoicingJob { get; set; }

		public virtual ZDateTime GetOperationsSignificantDate(string significantDateCode)
		{
			return OperationsRevenueRecognitionDateForTest;
		}
		public ZDateTime OperationsRevenueRecognitionDateForTest { get; set; }

		public virtual ZDateTime GetOperationsSignificantDateByDirection(string significantDateCode, string direction)
		{
			return OperationsRevenueRecognitionDateByDirectionForTest;
		}
		public ZDateTime OperationsRevenueRecognitionDateByDirectionForTest { get; set; }

		public virtual ZDateTime GetShipmentDate() => ZDateTime.Empty;

		public virtual ZDateTime GetCustomsClearanceDate()
		{
			return CustomsClearanceDateForTest;
		}
		public ZDateTime CustomsClearanceDateForTest { get; set; }

		public virtual ZGuid OverriddenDepartmentPK { get; set; }
		public virtual OrgHeader Consignee { get; set; }
		public virtual OrgHeader Consignor { get; set; }
		public virtual OrgHeader ControllingCustomer { get; set; }
		public virtual OrgHeader ControllingAgent { get; set; }
		public virtual OrgHeader SendingAgent { get; set; }
		public virtual OrgHeader ReceivingAgent { get; set; }
		public virtual OrgHeader EarliestSendingAgent { get; set; }
		public virtual OrgHeader LatestReceivingAgent { get; set; }
		public virtual IJobInvoicingPlugIn GetPreviousConsol(ZString relatedJobNumber) => null;
		public virtual IZType[] GetPreviousSendingAgentFallBackCode(ZString relatedJobNumber) => Array.Empty<IZType>();
		public virtual IZType[] GetConsolDirectionFallBackCode() => Array.Empty<IZType>();
		public virtual IZType[] GetConsolTransportModeFallBackCode() => Array.Empty<IZType>();
		public virtual OrgHeader GetDefaultCreditor(DefaultCreditorSetting defaultCreditorSetting) => null;
		public virtual OrgHeader GetDefaultDebtor(AccChargeCode chargeCode, JobHeader job, ZString relatedJobNumber) => job.LocalCharges;
		public virtual OrgHeader PickUpAgent { get; set; }
		public virtual OrgHeader DeliveryAgent { get; set; }
		public virtual OrgHeader ImportBroker { get; set; }
		public virtual OrgHeader ExportBroker { get; set; }
		public virtual OrgHeader Broker { get; set; }
		public virtual OrgHeader OverriddenDefaultLocalClient { get { return null; } }
		public virtual bool IsDirectShipment { get; set; }
		public virtual ZDecimal ActualChargeable { get; set; }
		public virtual ZString ActualChargeableUnit { get; set; }
		public virtual ZString VoyageVesselOrFlightDate { get; set; }
		public virtual ZString ConsolType { get; set; }
		public virtual RefCurrency ConsolRateCurrency { get; set; }
		public virtual ZDecimal ConsolExchangeRate { get; set; }
		public virtual ZDecimal GetConsolExchangeRate(ZString currencyCode) { return 0m; }
		public virtual ZString ConsolNumber { get; set; }
		public virtual RefUNLOCO Origin { get; set; }
		public virtual RefUNLOCO Destination { get; set; }
		public virtual RefUNLOCO GetTranshipmentPort(CostSell costOrSell) => null;
		public virtual ZString TransportMode { get; set; }
		public virtual ZString ContainerMode { get; set; }
		public virtual ZString ServiceLevel { get; set; }
		public virtual ZString CustomsEntryNumberType { get; set; }
		public virtual ZString CommunityTransitStatus { get; set; }
		public virtual PaymentTermInfos PaymentTerm { get; set; }
		public virtual bool IsStandaloneShipment { get; set; }
		public virtual bool IsImport { get; set; }
		public virtual bool IsExport { get; set; }
		public virtual bool IsDomestic { get; set; }
		public virtual bool IsCrossTrade { get; set; }
		public virtual bool IsPlugInReadOnly { get; set; }
		public virtual bool IsQuote { get; set; }
		public virtual bool IsNCTSPhase4 { get; set; }
		public virtual bool IsBookingWithQuote { get; set; }
		public virtual bool ProviderPreferred { get; set; }
		public virtual ZString ShipmentNumberOfColoadMaster { get; set; }
		public virtual JobInvoicingConsumerType ConsumerType { get; set; }
		public virtual ZString MasterBillNumber { get; set; }
		public virtual ZString HouseBillNumber { get; set; }
		public virtual ZDateTime ATA { get; set; }
		public virtual ZDateTime ATD { get; set; }
		public virtual ZDateTime ETA { get; set; }
		public virtual ZDateTime ETD { get; set; }
		public virtual ZDateTime ESP { get; set; }
		public virtual ZDateTime ESD { get; set; }
		public virtual ZDateTimeOffset FIN { get; set; }
		public virtual ZDateTimeOffset REQ { get; set; }
		public virtual ZDateTime ArrivalAtLoadPort { get; set; }
		public virtual ZDateTime EstimatedArrivalAtLoadPort { get; set; }
		public virtual ZDateTime ActualPickupDate { get; set; }
		public virtual ZDateTime ActualDeliveryDate { get; set; }
		public virtual ZDecimal ActualWeight { get; set; }
		public virtual ZString ActualWeightUnit { get; set; }
		public virtual ZDecimal ActualVolume { get; set; }
		public virtual ZString ActualVolumeUnit { get; set; }
		public virtual ZDecimal ExcessActualVolumeWeight { get; set; }
		public virtual ZDecimal ExcessChargeableVolumeWeight { get; set; }
		public virtual ZDecimal ActualLoadingMeters { get; set; }
		public virtual bool CreateAccountingJobOnSavingOfOperationsJob { get; set; }
		public virtual GlbBranch OperationsBranch { get; set; }
		public virtual SecurityCheckpoint AuditSecurity { get { return Env.Security.None; } }
		public virtual SecurityCheckpoint JobInvoicingSecurity { get { return Env.Security.None; } }
		public virtual SecurityCheckpoint EditSecurityCheckpoint { get { return Env.Security.None; } }
		public virtual ZString EditSecurityMessage { get; set; }
		public virtual bool EditSecurityLock { get; set; }
		public virtual void PostedStateChanged() { }
		public virtual int ContainerCount { get; set; }
		public virtual ZDecimal TEUCount { get; set; }
		public virtual int OuterPackTotal { get; set; }
		public virtual ZString DefaultChargeGroup { get; set; }
		public virtual ZString OperationalJobRef { get; set; }
		public virtual bool ShowOperationalJobRefFilter { get; set; }
		public virtual bool IncludeInConsolCosting(bool includeRelatedShipments) { return true; }
		public virtual SecurityCheckpoint ViewConsolCostFromOtherBRNorDEPSecurityCheckpoint { get { return Env.Security.None; } }
		public virtual ILocation FixedPlaceOfSupply { get { return null; } }
		public virtual string JobNumber { get; }

		public virtual bool TryGetContainersCostShare(AccChargeCode chargeCode, IEnumerable<IJobPaymentBasis> paymentBases, ZGuid consolPK, out ZDecimal cost)
		{
			cost = 0m;
			return false;
		}

		public virtual bool HasContainerCostShare(AccChargeCode chargeCode)
		{
			return false;
		}

		#region SetDefaultsForNewCharge

		public virtual void SetDefaultsForNewCharge(JobCharge charge)
		{
			SetDefaultsForNewChargeCalled++;
		}

		public int SetDefaultsForNewChargeCalled
		{
			get;
			private set;
		}

		#endregion

		public virtual string GetPaymentTermForCreditorOverride(CostSell costOrSell) => string.Empty;

		/// <summary>
		/// This will be used to stop users from making changes or post the costs.
		/// </summary>
		public virtual string GetReasonNotToAllowChangeOnCostDetails(ZGuid chargeCodePK) { return null; }
		public virtual string GetReasonNotToAllowPosting() { return null; }
		public virtual string GetReasonNotToAllowAutoRate(AutoRateOptions options = default) { return null; }
		public virtual string GetWarningForContinueAutoRate() { return null; }
		public virtual OrgHeader GetOrganisationByBranchDefaultingRule(ZString defaultingRule) { return null; }
		public virtual void ValidateJobProperty(ZPropertyInfo info) { }
		public virtual ZString ValidateOnInvoicingSupporter_JH_OA_LocalChargesAddr(ZGuid localChargesPK) { return ZString.Empty; }
		public virtual ZString ValidateOnInvoicingSupporter_JH_OA_AgentCollectAddr(ZGuid agentCollectPK) { return ZString.Empty; }
	}
}
