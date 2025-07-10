using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.Security;

namespace Enterprise.TransportBookings.Business
{
	public class DtbBookingParentInvoicingSupporter : IJobInvoicingSupporterWithChargeableFactorSource, IServiceDirection
	{
		public DtbBookingParentInvoicingSupporter(DtbBooking booking)
		{
			Booking = booking;
			ParentInvoicingSupporter = Booking.InvoicingPlugIn.InvoicingSupporter;
		}

		readonly DtbBooking Booking;
		readonly IJobInvoicingSupporter ParentInvoicingSupporter;

		ZString IJobInvoicingSupporter.OperationalJobRef
		{
			get { return Booking.KM_TransportReference.IsEmpty ? Booking.KM_JobID : Booking.KM_TransportReference; }
		}

		bool IJobInvoicingSupporter.ShowOperationalJobRefFilter => !Booking.ConsolidationSingleJob?.IsParentHidden ?? true;

		bool IJobInvoicingSupporter.CanCreateInvoicingJob => ParentInvoicingSupporter.CanCreateInvoicingJob;

		ZDateTime IJobInvoicingSupporter.ATA
		{
			get { return ParentInvoicingSupporter.ATA; }
		}

		ZDateTime IJobInvoicingSupporter.ATD
		{
			get { return ParentInvoicingSupporter.ATD; }
		}

		ZDecimal IJobInvoicingSupporter.ActualChargeable
		{
			get { return ParentInvoicingSupporter.ActualChargeable; }
		}

		ZString IJobInvoicingSupporter.ActualChargeableUnit
		{
			get { return ParentInvoicingSupporter.ActualChargeableUnit; }
		}

		ZDecimal IJobInvoicingSupporter.ActualLoadingMeters
		{
			get { return ParentInvoicingSupporter.ActualLoadingMeters; }
		}

		ZDecimal IJobInvoicingSupporter.ActualVolume
		{
			get { return ParentInvoicingSupporter.ActualVolume; }
		}

		ZDecimal IJobInvoicingSupporter.ExcessActualVolumeWeight
		{
			get { return ParentInvoicingSupporter.ExcessActualVolumeWeight; }
		}

		ZDecimal IJobInvoicingSupporter.ExcessChargeableVolumeWeight
		{
			get { return ParentInvoicingSupporter.ExcessChargeableVolumeWeight; }
		}

		ZString IJobInvoicingSupporter.ActualVolumeUnit
		{
			get { return ParentInvoicingSupporter.ActualVolumeUnit; }
		}

		ZDecimal IJobInvoicingSupporter.ActualWeight
		{
			get { return ParentInvoicingSupporter.ActualWeight; }
		}

		ZString IJobInvoicingSupporter.ActualWeightUnit
		{
			get { return ParentInvoicingSupporter.ActualWeightUnit; }
		}

		ZDateTime IJobInvoicingSupporter.ArrivalAtLoadPort
		{
			get { return ParentInvoicingSupporter.ArrivalAtLoadPort; }
		}

		ZDateTime IJobInvoicingSupporter.EstimatedArrivalAtLoadPort
		{
			get { return ParentInvoicingSupporter.EstimatedArrivalAtLoadPort; }
		}

		SecurityCheckpoint IJobInvoicingSupporter.AuditSecurity
		{
			get { return ParentInvoicingSupporter.AuditSecurity; }
		}

		OrgHeader IJobInvoicingSupporter.Consignee
		{
			get { return ParentInvoicingSupporter.Consignee; }
		}

		OrgHeader IJobInvoicingSupporter.Consignor
		{
			get { return ParentInvoicingSupporter.Consignor; }
		}

		OrgHeader IJobInvoicingSupporter.DeliveryAgent
		{
			get { return ParentInvoicingSupporter.DeliveryAgent; }
		}

		OrgHeader IJobInvoicingSupporter.ExportBroker
		{
			get { return ParentInvoicingSupporter.ExportBroker; }
		}

		OrgHeader IJobInvoicingSupporter.ImportBroker
		{
			get { return ParentInvoicingSupporter.ImportBroker; }
		}

		OrgHeader IJobInvoicingSupporter.Broker
		{
			get { return ParentInvoicingSupporter.Broker; }
		}

		OrgHeader IJobInvoicingSupporter.PickUpAgent
		{
			get { return ParentInvoicingSupporter.PickUpAgent; }
		}

		ZDecimal IJobInvoicingSupporter.ConsolExchangeRate
		{
			get { return ParentInvoicingSupporter.ConsolExchangeRate; }
		}

		ZString IJobInvoicingSupporter.ConsolNumber
		{
			get { return ParentInvoicingSupporter.ConsolNumber; }
		}

		RefCurrency IJobInvoicingSupporter.ConsolRateCurrency
		{
			get { return ParentInvoicingSupporter.ConsolRateCurrency; }
		}

		ZString IJobInvoicingSupporter.ConsolType
		{
			get { return ParentInvoicingSupporter.ConsolType; }
		}

		JobInvoicingConsumerType IJobInvoicingSupporter.ConsumerType
		{
			get { return ParentInvoicingSupporter.ConsumerType; }
		}

		int IJobInvoicingSupporter.ContainerCount
		{
			get { return ParentInvoicingSupporter.ContainerCount; }
		}

		int IJobInvoicingSupporter.OuterPackTotal
		{
			get { return ParentInvoicingSupporter.OuterPackTotal; }
		}

		ZString IJobInvoicingSupporter.ContainerMode
		{
			get { return ParentInvoicingSupporter.ContainerMode; }
		}

		bool IJobInvoicingSupporter.CreateAccountingJobOnSavingOfOperationsJob
		{
			get { return ParentInvoicingSupporter.CreateAccountingJobOnSavingOfOperationsJob; }
		}

		ZString IJobInvoicingSupporter.DefaultChargeGroup
		{
			get { return ParentInvoicingSupporter.DefaultChargeGroup; }
		}

		OrgHeader IJobInvoicingSupporter.GetDefaultDebtor(AccChargeCode code, JobHeader job, ZString relatedJobNumber) => ParentInvoicingSupporter.GetDefaultDebtor(code, job, relatedJobNumber);

		RefUNLOCO IJobInvoicingSupporter.Destination
		{
			get { return ParentInvoicingSupporter.Destination; }
		}

		ZDateTime IJobInvoicingSupporter.ETA
		{
			get { return ParentInvoicingSupporter.ETA; }
		}

		ZDateTime IJobInvoicingSupporter.ETD
		{
			get { return ParentInvoicingSupporter.ETD; }
		}

		SecurityCheckpoint IJobInvoicingSupporter.EditSecurityCheckpoint
		{
			get { return ParentInvoicingSupporter.EditSecurityCheckpoint; }
		}

		bool IJobInvoicingSupporter.EditSecurityLock
		{
			get { return ParentInvoicingSupporter.EditSecurityLock; }
		}

		ZString IJobInvoicingSupporter.EditSecurityMessage
		{
			get { return ParentInvoicingSupporter.EditSecurityMessage; }
		}

		ZDecimal IJobInvoicingSupporter.GetConsolExchangeRate(ZString currencyCode)
		{
			return ParentInvoicingSupporter.GetConsolExchangeRate(currencyCode);
		}

		ZDateTime IJobInvoicingSupporter.GetCustomsClearanceDate()
		{
			return ParentInvoicingSupporter.GetCustomsClearanceDate();
		}

		ZDateTime IJobInvoicingSupporter.GetShipmentDate()
		{
			return ParentInvoicingSupporter.GetShipmentDate();
		}

		OrgHeader IJobInvoicingSupporter.GetDefaultCreditor(DefaultCreditorSetting defaultCreditorSetting)
		{
			return ParentInvoicingSupporter.GetDefaultCreditor(defaultCreditorSetting);
		}

		ZDateTime IJobInvoicingSupporter.GetOperationsSignificantDate(string significantDateCode)
		{
			return ParentInvoicingSupporter.GetOperationsSignificantDate(significantDateCode);
		}

		ZDateTime IJobInvoicingSupporter.GetOperationsSignificantDateByDirection(string significantDateCode, string direction)
		{
			return ParentInvoicingSupporter.GetOperationsSignificantDateByDirection(significantDateCode, direction);
		}

		OrgHeader IJobInvoicingSupporter.GetOrganisationByBranchDefaultingRule(ZString defaultingRule)
		{
			return ParentInvoicingSupporter.GetOrganisationByBranchDefaultingRule(defaultingRule);
		}

		string IJobInvoicingSupporter.GetReasonNotToAllowChangeOnCostDetails(ZGuid chargeCodePK)
		{
			return ParentInvoicingSupporter.GetReasonNotToAllowChangeOnCostDetails(chargeCodePK);
		}

		string IJobInvoicingSupporter.GetReasonNotToAllowPosting()
		{
			return ParentInvoicingSupporter.GetReasonNotToAllowPosting();
		}

		string IJobInvoicingSupporter.GetReasonNotToAllowAutoRate(AutoRateOptions options)
		{
			return ParentInvoicingSupporter.GetReasonNotToAllowAutoRate(options);
		}

		string IJobInvoicingSupporter.GetWarningForContinueAutoRate()
		{
			return ParentInvoicingSupporter.GetWarningForContinueAutoRate();
		}

		ZString IJobInvoicingSupporter.HouseBillNumber
		{
			get { return ParentInvoicingSupporter.HouseBillNumber; }
		}

		bool IJobInvoicingSupporter.IncludeInConsolCosting(bool includeRelatedShipments)
		{
			return ParentInvoicingSupporter.IncludeInConsolCosting(includeRelatedShipments);
		}

		bool IJobInvoicingSupporter.IsDirectShipment
		{
			get { return ParentInvoicingSupporter.IsDirectShipment; }
		}

		bool IJobInvoicingSupporter.IsDomestic
		{
			get { return ParentInvoicingSupporter.IsDomestic; }
		}

		bool IJobInvoicingSupporter.IsExport
		{
			get { return ParentInvoicingSupporter.IsExport; }
		}

		bool IJobInvoicingSupporter.IsImport
		{
			get { return ParentInvoicingSupporter.IsImport; }
		}

		bool IJobInvoicingSupporter.IsCrossTrade
		{
			get { return ParentInvoicingSupporter.IsCrossTrade; }
		}

		bool IJobInvoicingSupporter.IsPlugInReadOnly
		{
			get { return ParentInvoicingSupporter.IsPlugInReadOnly; }
		}

		bool IJobInvoicingSupporter.IsQuote
		{
			get { return ParentInvoicingSupporter.IsQuote; }
		}

		bool IJobInvoicingSupporter.IsBookingWithQuote
		{
			get { return ParentInvoicingSupporter.IsBookingWithQuote; }
		}

		bool IJobInvoicingSupporter.ProviderPreferred
		{
			get { return ParentInvoicingSupporter.ProviderPreferred; }
		}

		bool IJobInvoicingSupporter.IsNCTSPhase4
		{
			get { return ParentInvoicingSupporter.IsNCTSPhase4; }
		}

		JobHeader IJobInvoicingSupporter.Job
		{
			get { return ParentInvoicingSupporter.Job; }
		}

		SecurityCheckpoint IJobInvoicingSupporter.JobInvoicingSecurity
		{
			get { return ParentInvoicingSupporter.JobInvoicingSecurity; }
		}

		ZString IJobInvoicingSupporter.MasterBillNumber
		{
			get { return ParentInvoicingSupporter.MasterBillNumber; }
		}

		GlbBranch IJobInvoicingSupporter.OperationsBranch
		{
			get { return ParentInvoicingSupporter.OperationsBranch; }
		}

		RefUNLOCO IJobInvoicingSupporter.Origin
		{
			get { return ParentInvoicingSupporter.Origin; }
		}

		ZGuid IJobInvoicingSupporter.OverriddenDepartmentPK
		{
			get { return ParentInvoicingSupporter.OverriddenDepartmentPK; }
		}

		void IJobInvoicingSupporter.PostedStateChanged()
		{
			ParentInvoicingSupporter.PostedStateChanged();
		}

		OrgHeader IJobInvoicingSupporter.ReceivingAgent
		{
			get { return ParentInvoicingSupporter.ReceivingAgent; }
		}

		OrgHeader IJobInvoicingSupporter.SendingAgent
		{
			get { return ParentInvoicingSupporter.SendingAgent; }
		}

		OrgHeader IJobInvoicingSupporter.LatestReceivingAgent => ((IJobInvoicingSupporter)this).ReceivingAgent;

		OrgHeader IJobInvoicingSupporter.EarliestSendingAgent => ((IJobInvoicingSupporter)this).SendingAgent;

		OrgHeader IJobInvoicingSupporter.ControllingCustomer
		{
			get { return ParentInvoicingSupporter.ControllingCustomer; }
		}

		OrgHeader IJobInvoicingSupporter.ControllingAgent
		{
			get { return ParentInvoicingSupporter.ControllingAgent; }
		}

		void IJobInvoicingSupporter.SetDefaultsForNewCharge(JobCharge charge)
		{
			ParentInvoicingSupporter.SetDefaultsForNewCharge(charge);
		}

		ZString IJobInvoicingSupporter.ShipmentNumberOfColoadMaster
		{
			get { return ParentInvoicingSupporter.ShipmentNumberOfColoadMaster; }
		}

		ZDecimal IJobInvoicingSupporter.TEUCount
		{
			get { return ParentInvoicingSupporter.TEUCount; }
		}

		RefUNLOCO IJobInvoicingSupporter.GetTranshipmentPort(CostSell costOrSell)
		{
			return ParentInvoicingSupporter.GetTranshipmentPort(costOrSell);
		}

		ZString IJobInvoicingSupporter.TransportMode
		{
			get { return ParentInvoicingSupporter.TransportMode; }
		}

		OrgHeader IJobInvoicingSupporter.OverriddenDefaultLocalClient
		{
			get { return ParentInvoicingSupporter.OverriddenDefaultLocalClient; }
		}

		bool IJobInvoicingSupporter.TryGetContainersCostShare(AccChargeCode chargeCode, IEnumerable<IJobPaymentBasis> paymentBases, ZGuid consolPK, out ZDecimal cost)
		{
			return ParentInvoicingSupporter.TryGetContainersCostShare(chargeCode, paymentBases, consolPK, out cost);
		}
		bool IJobInvoicingSupporter.HasContainerCostShare(AccChargeCode chargeCode)
		{
			return ParentInvoicingSupporter.HasContainerCostShare(chargeCode);
		}

		void IJobInvoicingSupporter.ValidateJobProperty(ZPropertyInfo info)
		{
		}

		ZString IJobInvoicingSupporter.ValidateOnInvoicingSupporter_JH_OA_LocalChargesAddr(ZGuid localChargesPK) => ZString.Empty;

		ZString IJobInvoicingSupporter.ValidateOnInvoicingSupporter_JH_OA_AgentCollectAddr(ZGuid agentCollectPK) => ZString.Empty;

		IJobInvoicingPlugIn IJobInvoicingSupporter.GetPreviousConsol(ZString relatedJobNumber) => null;

		IZType[] IJobInvoicingSupporter.GetPreviousSendingAgentFallBackCode(ZString relatedJobNumber) => Array.Empty<IZType>();

		IZType[] IJobInvoicingSupporter.GetConsolDirectionFallBackCode() => Array.Empty<IZType>();

		IZType[] IJobInvoicingSupporter.GetConsolTransportModeFallBackCode() => Array.Empty<IZType>();

		ZString IJobInvoicingSupporter.VoyageVesselOrFlightDate
		{
			get { return ParentInvoicingSupporter.VoyageVesselOrFlightDate; }
		}

		SecurityCheckpoint IJobInvoicingSupporter.ViewConsolCostFromOtherBRNorDEPSecurityCheckpoint
		{
			get { return ParentInvoicingSupporter.ViewConsolCostFromOtherBRNorDEPSecurityCheckpoint; }
		}

		PaymentTermInfos IJobInvoicingSupporter.PaymentTerm
		{
			get { return ParentInvoicingSupporter.PaymentTerm; }
		}

		ILocation IJobInvoicingSupporter.FixedPlaceOfSupply
		{
			get { return ParentInvoicingSupporter.FixedPlaceOfSupply; }
		}

		ZDateTime IJobInvoicingSupporter.ESD
		{
			get { return ParentInvoicingSupporter.ESD; }
		}

		ZDateTime IJobInvoicingSupporter.ESP
		{
			get { return ParentInvoicingSupporter.ESP; }
		}

		ZDateTimeOffset IJobInvoicingSupporter.FIN
		{
			get { return ParentInvoicingSupporter.FIN; }
		}

		ZDateTimeOffset IJobInvoicingSupporter.REQ
		{
			get { return ParentInvoicingSupporter.REQ; }
		}

		ZDateTime IJobInvoicingSupporter.ActualPickupDate
		{
			get { return ParentInvoicingSupporter.ActualPickupDate; }
		}

		ZDateTime IJobInvoicingSupporter.ActualDeliveryDate
		{
			get { return ParentInvoicingSupporter.ActualDeliveryDate; }
		}

		ZString IJobInvoicingSupporter.ServiceLevel => ParentInvoicingSupporter.ServiceLevel;

		ZString IJobInvoicingSupporter.CustomsEntryNumberType => ParentInvoicingSupporter.CustomsEntryNumberType;

		ZString IJobInvoicingSupporter.CommunityTransitStatus => ParentInvoicingSupporter.CommunityTransitStatus;

		bool IJobInvoicingSupporter.IsStandaloneShipment => ParentInvoicingSupporter.IsStandaloneShipment;

		string IJobNumber.JobNumber => ParentInvoicingSupporter.JobNumber;

		public ZString ServiceDirection
		{
			get
			{
				return ParentInvoicingSupporter is IServiceDirection
					? ((IServiceDirection)ParentInvoicingSupporter).ServiceDirection
					: ZString.Empty;
			}
		}

		ChargeableFactorSource IJobInvoicingSupporterWithChargeableFactorSource.ChargeableFactorSource => ChargeableFactorSource.TransportBooking;

		public string GetPaymentTermForCreditorOverride(CostSell costOrSell) => ParentInvoicingSupporter.GetPaymentTermForCreditorOverride(costOrSell);
	}
}
