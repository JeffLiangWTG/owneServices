#define CODE_ANAYSIS

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using CargoWise.Application;
using CargoWise.BrandManager;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.UniversalCopy;
using CargoWiseOne.ResourceStrings;
using Enterprise.Core;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineIntegration.DocumentParsing;
using Enterprise.Environment;
using Enterprise.Integration.Accounting;
using Enterprise.Integration.Licensing;
using Enterprise.MasterFiles.Business.Accounting.CriticalValidation;
using Enterprise.MasterFiles.Integration;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Data.Mutex;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.MasterFiles.Business.Accounting.CriticalValidation.CriticalValidationInfoExtensions;

namespace Enterprise.MasterFiles.Business
{
	public interface IFountainResolver
	{
		void TryToResolve();
	}

	public interface IGenericJob
	{
		IJobInvoicingPlugIn Consumer { get; }
		ControllerID GetConsumerController();
	}

	/// <summary>
	/// Interface for BusinessObjects that are not transport jobs and can use the generic freight document wrapper FreightWrapperNonTransportJob
	/// </summary>
	public interface INonTransportJobHeaderParent : IJobHeaderParent
	{
	}

	public interface IGenericJobCostPlugIn : IGenericJobCostPlugInBase, IBillingPlugin
	{
		IGenericJobCostSupporter CostSupporter { get; }
	}

	public interface IGenericJobCostSupporter
	{
		ZGuid PK { get; }
		ZString Type { get; }
		ZGuid[] ShipmentsListPKs { get; }
		IJobInvoicingPlugIn[] ShipmentsList { get; }
		bool HasChanges { get; }
		bool IsInDatabase { get; }
		DocumentSupporter DocumentSupporter { get; }

		ZString MasterBillNum { get; }
		ZString TransportMode { get; }
		ZGuid GetCreditorPK(ZString chargeGroup, ZGuid rateProviderOrgPK);
		ZString TotalChargeableUnit { get; }
		Directions Direction { get; }
		bool IsBuyersConsol { get; }
		ZString PortOfLoading { get; }
		ZString PortOfDischarge { get; }
		ZString ConsolMode { get; }
		OrgHeader SendingForwarder { get; }
		OrgHeader ReceivingForwarder { get; }
		[DocumentFieldExcludeFromMap]
		ManyToManyBusinessObjectCollection Shipments { get; }
		ZDateTime ETD { get; }
		ZDateTime ETA { get; }

		IEnumerable<ZString> ExcludedApportionmentMethods { get; }
		bool IsApportionmentFilterEnabled { get; }
		ZDecimal FreeSpace { get; }

		SecurityCheckpoint JobConsolCostingCheckPoint { get; }
	}

	public class GenericJobCostSupporter : IGenericJobCostSupporter
	{
		public virtual ZGuid PK { get { return ZGuid.Empty; } }
		public virtual ZString Type { get { return ZString.Empty; } }
		public virtual ZGuid[] ShipmentsListPKs { get { return null; } }
		public virtual IJobInvoicingPlugIn[] ShipmentsList { get { return null; } }
		public virtual bool HasChanges { get { return false; } }
		public virtual bool IsInDatabase { get { return false; } }
		public virtual DocumentSupporter DocumentSupporter { get { return null; } }

		public virtual ZString MasterBillNum { get { return ZString.Empty; } }
		public virtual ZString TransportMode { get { return ZString.Empty; } }

		public virtual ZGuid GetCreditorPK(ZString chargeGroup, ZGuid rateProviderOrgPK) { return ZGuid.Empty; }
		public virtual ZString TotalChargeableUnit { get { return ZString.Empty; } }
		public virtual Directions Direction => Directions.Unknown;
		public virtual bool IsBuyersConsol { get { return false; } }
		public virtual ZString PortOfLoading { get { return ZString.Empty; } }
		public virtual ZString PortOfDischarge { get { return ZString.Empty; } }
		public virtual ZString ConsolMode { get { return ZString.Empty; } }
		public virtual OrgHeader SendingForwarder { get { return null; } }
		public virtual OrgHeader ReceivingForwarder { get { return null; } }
		public virtual ManyToManyBusinessObjectCollection Shipments { get { return null; } }
		public virtual ZDateTime ETD { get { return ZDateTime.Empty; } }
		public virtual ZDateTime ETA { get { return ZDateTime.Empty; } }

		public virtual IEnumerable<ZString> ExcludedApportionmentMethods { get { return Enumerable.Empty<ZString>(); } }
		public virtual bool IsApportionmentFilterEnabled { get { return true; } }

		public virtual ZDecimal FreeSpace => ZDecimal.Zero;

		public virtual SecurityCheckpoint JobConsolCostingCheckPoint => Env.Security.None;
	}

	public interface IJobInvoicingPlugIn : IJobHeaderParent
	{
		IJobInvoicingSupporter InvoicingSupporter { get; }
	}

	// Implement IBusinesssObjectProviderForDocumentWrapper when IJobInvoicingPlugin implementer but NOT a Business Object, however some custom implementation of getting BusinessObject for DocumentWrapper is required.
	public interface IBusinesssObjectProviderForDocumentWrapper
	{
		BusinessObject BusinessObjectForDocumentWrapper { get; }
	}

	public interface IBillingPlugin { }

	public static class IBillingPluginExtensions
	{
		public static bool IsGatewayBillingEnabled(this IBillingPlugin billing, ICompany company = null)
		{
			return billing is IGateway gateway && gateway.GatewayBillingSupporter.IsGatewayBillingEnabled(company);
		}

		public static (IOrgHeader sendingAgent, IOrgHeader receivingAgent) GatewayAgent(this IBillingPlugin billing, ICompany company = null)
		{
			return billing is IGateway gateway
				? gateway.GatewayBillingSupporter.GatewayAgent(company)
				: default;
		}
	}

	public interface IJobInvoicingSupporter : IJobNumber
	{
		bool CanCreateInvoicingJob { get; }
		ZGuid OverriddenDepartmentPK { get; }
		OrgHeader Consignee { get; }
		OrgHeader Consignor { get; }
		OrgHeader SendingAgent { get; }
		OrgHeader ReceivingAgent { get; }
		OrgHeader EarliestSendingAgent { get; }
		OrgHeader LatestReceivingAgent { get; }
		OrgHeader ControllingCustomer { get; }
		OrgHeader ControllingAgent { get; }
		OrgHeader GetDefaultCreditor(DefaultCreditorSetting defaultCreditorSetting);
		OrgHeader GetDefaultDebtor(AccChargeCode chargeCode, JobHeader job, ZString relatedJobNumber);
		OrgHeader PickUpAgent { get; }
		OrgHeader DeliveryAgent { get; }
		OrgHeader ImportBroker { get; }
		OrgHeader ExportBroker { get; }
		OrgHeader Broker { get; }
		OrgHeader OverriddenDefaultLocalClient { get; }
		bool IsDirectShipment { get; }
		ZDecimal ActualChargeable { get; }
		ZString ActualChargeableUnit { get; }
		ZString ConsolType { get; }
		RefCurrency ConsolRateCurrency { get; }
		ZDecimal ConsolExchangeRate { get; }
		ZDecimal GetConsolExchangeRate(ZString currencyCode);
		ZString ConsolNumber { get; }
		RefUNLOCO Origin { get; }
		RefUNLOCO Destination { get; }
		RefUNLOCO GetTranshipmentPort(CostSell costOrSell);
		ZString TransportMode { get; }
		ZString ContainerMode { get; }
		ZString ServiceLevel { get; }
		ZString CustomsEntryNumberType { get; }
		ZString CommunityTransitStatus { get; }
		PaymentTermInfos PaymentTerm { get; }
		bool IsStandaloneShipment { get; }
		bool IsImport { get; }
		bool IsExport { get; }
		bool IsDomestic { get; }
		bool IsCrossTrade { get; }
		bool IsPlugInReadOnly { get; }
		bool IsQuote { get; }
		bool IsBookingWithQuote { get; }
		bool IsNCTSPhase4 { get; }
		bool ProviderPreferred { get; }
		ZString ShipmentNumberOfColoadMaster { get; }
		JobInvoicingConsumerType ConsumerType { get; }
		ZString MasterBillNumber { get; }
		ZString HouseBillNumber { get; }
		ZDateTime ATA { get; }
		ZDateTime ATD { get; }
		ZDateTime ETA { get; }
		ZDateTime ETD { get; }
		ZDateTime ESP { get; }
		ZDateTime ESD { get; }
		ZDateTimeOffset FIN { get; }
		ZDateTimeOffset REQ { get; }
		ZDateTime ArrivalAtLoadPort { get; }
		ZDateTime EstimatedArrivalAtLoadPort { get; }
		ZDateTime ActualPickupDate { get; }
		ZDateTime ActualDeliveryDate { get; }
		ZDecimal ActualWeight { get; }
		ZString ActualWeightUnit { get; }
		ZDecimal ActualVolume { get; }
		ZString ActualVolumeUnit { get; }
		ZDecimal ExcessActualVolumeWeight { get; }
		ZDecimal ExcessChargeableVolumeWeight { get; }
		ZDecimal ActualLoadingMeters { get; }
		bool CreateAccountingJobOnSavingOfOperationsJob { get; }
		GlbBranch OperationsBranch { get; }
		SecurityCheckpoint AuditSecurity { get; }
		SecurityCheckpoint JobInvoicingSecurity { get; }
		ZDateTime GetOperationsSignificantDate(string significantDateCode);
		ZDateTime GetOperationsSignificantDateByDirection(string significantDateCode, string direction);
		ZDateTime GetCustomsClearanceDate();
		ZDateTime GetShipmentDate();
		SecurityCheckpoint EditSecurityCheckpoint { get; }
		ZString EditSecurityMessage { get; }
		bool EditSecurityLock { get; }
		void PostedStateChanged();
		int ContainerCount { get; }
		int OuterPackTotal { get; }
		ZDecimal TEUCount { get; }
		ZString DefaultChargeGroup { get; }
		ZString OperationalJobRef { get; }
		bool ShowOperationalJobRefFilter { get; }
		bool IncludeInConsolCosting(bool includeRelatedShipments);
		void SetDefaultsForNewCharge(JobCharge charge);
		bool TryGetContainersCostShare(AccChargeCode chargeCode, IEnumerable<IJobPaymentBasis> paymentBases, ZGuid consolPK, out ZDecimal cost);
		bool HasContainerCostShare(AccChargeCode chargeCode);
		JobHeader Job { get; }
		ZString VoyageVesselOrFlightDate { get; }
		SecurityCheckpoint ViewConsolCostFromOtherBRNorDEPSecurityCheckpoint { get; }

		string GetPaymentTermForCreditorOverride(CostSell costSell);

		/// <summary>
		/// This will be used to stop users from making changes or post the costs.
		/// </summary>
		string GetReasonNotToAllowChangeOnCostDetails(ZGuid chargeCodePK);
		string GetReasonNotToAllowPosting();
		string GetReasonNotToAllowAutoRate(AutoRateOptions options = default);
		string GetWarningForContinueAutoRate();
		OrgHeader GetOrganisationByBranchDefaultingRule(ZString defaultingRule);
		ILocation FixedPlaceOfSupply { get; }
		void ValidateJobProperty(ZPropertyInfo info);
		ZString ValidateOnInvoicingSupporter_JH_OA_LocalChargesAddr(ZGuid localChargesPK);
		ZString ValidateOnInvoicingSupporter_JH_OA_AgentCollectAddr(ZGuid agentCollectPK);
		IJobInvoicingPlugIn GetPreviousConsol(ZString relatedJobNumber);
		IZType[] GetPreviousSendingAgentFallBackCode(ZString relatedJobNumber);
		IZType[] GetConsolDirectionFallBackCode();
		IZType[] GetConsolTransportModeFallBackCode();
	}

	public static class JobInvoicingSupporterExtensions
	{
		public static Directions GetJobDirection(this IJobInvoicingSupporter supporter) => ImportExportHelper.GetJobDirection(() => supporter.IsImport, () => supporter.IsExport, () => supporter.IsCrossTrade, () => supporter.IsDomestic);
	}

	public class DefaultCreditorSetting
	{
		public AccChargeCode ChargeCode { get; }
		public string InvoiceType { get; }
		public ZGuid RateProviderOrgPK { get; }

		public DefaultCreditorSetting(AccChargeCode chargeCode, string invoiceType, ZGuid rateProviderOrgPK) : this(chargeCode, invoiceType)
		{
			RateProviderOrgPK = rateProviderOrgPK;
		}

		public DefaultCreditorSetting(AccChargeCode chargeCode, string invoiceType)
		{
			ChargeCode = chargeCode;
			InvoiceType = invoiceType;
		}
	}

	public class JobInvoicingSupporter : IJobInvoicingSupporter
	{
		public JobInvoicingSupporter(IJobHeaderParent parent)
		{
			this.JobHeaderParent = parent;
		}

		public IJobHeaderParent JobHeaderParent { get; }

		public JobHeader Job
		{
			get
			{
				var jobParent = GetJobHeaderParentCore();
				var result = jobParent != null && jobParent.Factory != null ? new JobHeader.Loader(jobParent).Load() : null;

				return result;
			}
		}

		public string JobNumber => GetJobHeaderParentCore()?.JobNumber ?? string.Empty;

		protected virtual IJobHeaderParent GetJobHeaderParentCore()
		{
			return JobHeaderParent;
		}

		public virtual bool CanCreateInvoicingJob => true;

		public virtual ZGuid OverriddenDepartmentPK { get { return ZGuid.Empty; } }
		public virtual OrgHeader Consignee { get { return null; } }
		public virtual OrgHeader Consignor { get { return null; } }
		public virtual OrgHeader SendingAgent { get { return null; } }
		public virtual OrgHeader ReceivingAgent { get { return null; } }
		public virtual OrgHeader EarliestSendingAgent { get { return SendingAgent; } }
		public virtual OrgHeader LatestReceivingAgent { get { return ReceivingAgent; } }
		public virtual OrgHeader ControllingCustomer { get { return null; } }
		public virtual OrgHeader ControllingAgent { get { return null; } }
		public virtual IJobInvoicingPlugIn GetPreviousConsol(ZString relatedJobNumber) => null;
		public virtual IZType[] GetPreviousSendingAgentFallBackCode(ZString relatedJobNumber) => Array.Empty<IZType>();
		public virtual IZType[] GetConsolDirectionFallBackCode() => Array.Empty<IZType>();
		public virtual IZType[] GetConsolTransportModeFallBackCode() => Array.Empty<IZType>();
		public virtual OrgHeader GetDefaultCreditor(DefaultCreditorSetting defaultCreditorSetting) { return null; }

		public virtual OrgHeader GetDefaultDebtor(AccChargeCode chargeCode, JobHeader job, ZString relatedJobNumber)
		{
			if (chargeCode == null)
			{
				return null;
			}

			var party = IncoTermRegistry.GetLocalClientOrAgentRegardlessOfIncoterm(chargeCode);

			if (party == ChargedParty.None)
			{
				var costOrSell = CostSell.Revenue;
				var jobDirection = this.GetJobDirection();
				var defaultingParam = job?.GetParamForCrossTradeDebtorDefaulting();

				if (jobDirection == Directions.CrossTrade &&
					JobHeader.IsCrossTradeDebtorDefaultingFunctionalityEnabled() &&
					defaultingParam != null &&
					(defaultingParam.JobType.Code == JobInvoicingConsumerTypes.ShipmentCode || defaultingParam.JobType.Code == JobInvoicingConsumerTypes.QuotedBookingCode))
				{
					var crossTradeParty = PaymentTerm.GetChargedPartyForCrossTradeJob(chargeCode.AC_ChargeGroup, costOrSell, defaultingParam);
					switch (crossTradeParty)
					{
						case ChargedPartyForCrossTradeJob.Unknown:
							party = ChargedParty.None;
							break;
						case ChargedPartyForCrossTradeJob.Agent:
							party = ChargedParty.Agent;
							break;
						case ChargedPartyForCrossTradeJob.LocalClient:
							party = ChargedParty.LocalClient;
							break;
						case ChargedPartyForCrossTradeJob.ControllingCustomerFallingBackToLocalClient:
							var ccBill = GetRelatedPartyFromControllingCustomer(defaultingParam.JobType, false);
							if (ccBill != null)
							{
								return ccBill;
							}
							party = ChargedParty.LocalClient;
							break;
						case ChargedPartyForCrossTradeJob.ControllingCustomerFallingBackToAgent:
							ccBill = GetRelatedPartyFromControllingCustomer(defaultingParam.JobType, true);
							if (ccBill != null)
							{
								return ccBill;
							}
							party = ChargedParty.Agent;
							break;
						default:
							break;
					}
				}
				else if (party == ChargedParty.None)
				{
					var exportImport = PaymentTermInfos.GetExportImport(costOrSell, jobDirection, job?.LocalCharges, job?.AgentCollect, Consignor, Consignee);
					party = PaymentTerm.GetChargedParty(chargeCode.AC_ChargeGroup, jobDirection, exportImport, costOrSell);
				}
			}

			return GetDebtorFromParty(party, job);
		}

		public virtual OrgHeader GetRelatedPartyFromControllingCustomer(JobInvoicingConsumerType jobType, bool isDelivery)
		{
			if (jobType != null && ControllingCustomer != null)
			{
				var relatedPartyOrg = ControllingCustomer.GetRelatedBillToParty(jobType, isDelivery);
				if (relatedPartyOrg != null)
				{
					return relatedPartyOrg;
				}
				else
				{
					return ControllingCustomer;
				}
			}
			return null;
		}

		OrgHeader GetDebtorFromParty(ChargedParty party, JobHeader job)
		{
			if (party == ChargedParty.None)
			{
				return null;
			}

			OrgHeader result = null;

			if (party.HasFlag(ChargedParty.LocalClient))
			{
				result = job?.LocalCharges;
			}

			if (result == null && party.HasFlag(ChargedParty.Agent))
			{
				result = job?.AgentCollect;
			}

			if (result == null && party.HasFlag(ChargedParty.Consignor))
			{
				result = Consignor?.GetRelatedBillToParty(ConsumerType, false);
			}

			if (result == null && party.HasFlag(ChargedParty.Consignee))
			{
				result = Consignee?.GetRelatedBillToParty(ConsumerType, true);
			}

			return result ?? job?.LocalCharges;
		}

		public virtual OrgHeader PickUpAgent { get { return null; } }
		public virtual OrgHeader DeliveryAgent { get { return null; } }
		public virtual OrgHeader ImportBroker { get { return null; } }
		public virtual OrgHeader ExportBroker { get { return null; } }
		public virtual OrgHeader Broker { get { return null; } }
		public virtual OrgHeader OverriddenDefaultLocalClient { get { return null; } }
		public virtual bool IsDirectShipment { get { return false; } }
		public virtual ZDecimal ActualChargeable { get { return ZDecimal.Zero; } }
		public virtual ZString ActualChargeableUnit { get { return ZString.Empty; } }
		public virtual ZString VoyageVesselOrFlightDate { get { return ZString.Empty; } }
		public virtual ZString ConsolType { get { return Enterprise.Core.Constants.JobInvoicingDefaultDepartmentConsolType.NoConsol; } }
		public virtual RefCurrency ConsolRateCurrency { get { return null; } }
		public virtual ZDecimal ConsolExchangeRate { get { return ZDecimal.Zero; } }
		public virtual ZDecimal GetConsolExchangeRate(ZString currencyCode) { return ZDecimal.Zero; }
		public virtual ZString ConsolNumber { get { return ZString.Empty; } }
		public virtual RefUNLOCO Origin { get { return null; } }
		public virtual RefUNLOCO Destination { get { return null; } }
		public virtual RefUNLOCO GetTranshipmentPort(CostSell costOrSell) => null;
		public virtual ZString TransportMode { get { return ZString.Empty; } }
		public virtual ZString ContainerMode { get { return ZString.Empty; } }
		public virtual ZString ServiceLevel => ZString.Empty;
		public virtual ZString CustomsEntryNumberType => ZString.Empty;
		public virtual ZString CommunityTransitStatus => ZString.Empty;
		public virtual PaymentTermInfos PaymentTerm { get { return null; } }
		public virtual bool IsStandaloneShipment => false;
		public virtual bool IsImport { get { return false; } }
		public virtual bool IsExport { get { return false; } }
		public virtual bool IsCrossTrade { get { return (JobHeaderParent as IImportExport)?.IsCrossTrade() ?? false; } }
		public virtual bool IsQuote { get { return false; } }
		public virtual bool IsBookingWithQuote { get { return false; } }
		public virtual bool IsNCTSPhase4 { get { return false; } }
		public virtual bool ProviderPreferred { get { return true; } }
		public virtual bool IsDomestic { get { return false; } }
		public virtual bool IsPlugInReadOnly { get { return false; } }
		public virtual ZString ShipmentNumberOfColoadMaster { get { return ZString.Empty; } }
		public virtual JobInvoicingConsumerType ConsumerType { get { return null; } }
		public virtual ZString MasterBillNumber { get { return ZString.Empty; } }
		public virtual ZString HouseBillNumber { get { return ZString.Empty; } }
		public virtual ZDateTime ATA { get { return ZDateTime.Empty; } }
		public virtual ZDateTime ATD { get { return ZDateTime.Empty; } }
		public virtual ZDateTime ETA { get { return ZDateTime.Empty; } }
		public virtual ZDateTime ETD { get { return ZDateTime.Empty; } }
		public virtual ZDateTime ESP { get { return ZDateTime.Empty; } }
		public virtual ZDateTime ESD { get { return ZDateTime.Empty; } }
		public virtual ZDateTimeOffset FIN { get { return ZDateTimeOffset.Empty; } }
		public virtual ZDateTimeOffset REQ { get { return ZDateTimeOffset.Empty; } }
		public virtual ZDateTime ArrivalAtLoadPort { get { return ZDateTime.Empty; } }
		public virtual ZDateTime EstimatedArrivalAtLoadPort { get { return ZDateTime.Empty; } }
		public virtual ZDateTime ActualPickupDate { get { return ZDateTime.Empty; } }
		public virtual ZDateTime ActualDeliveryDate { get { return ZDateTime.Empty; } }
		public virtual ZDecimal ActualWeight { get { return ZDecimal.Zero; } }
		public virtual ZString ActualWeightUnit { get { return ZString.Empty; } }
		public virtual ZDecimal ActualVolume { get { return ZDecimal.Zero; } }
		public virtual ZString ActualVolumeUnit { get { return ZString.Empty; } }
		public virtual ZDecimal ExcessActualVolumeWeight => ZDecimal.Zero;
		public virtual ZDecimal ExcessChargeableVolumeWeight => ZDecimal.Zero;
		public virtual ZDecimal ActualLoadingMeters { get { return ZDecimal.Zero; } }
		public virtual bool CreateAccountingJobOnSavingOfOperationsJob { get { return false; } }
		public virtual GlbBranch OperationsBranch { get { return null; } }
		public virtual ZDateTime GetOperationsSignificantDate(string significantDateCode) { return ZDateTime.Empty; }
		public virtual ZDateTime GetOperationsSignificantDateByDirection(string significantDateCode, string direction) { return ZDateTime.Empty; }
		public virtual ZDateTime GetCustomsClearanceDate() { return ZDateTime.Empty; }
		protected virtual ZDateTime GetPickupDate() { return ZDateTime.Empty; }
		protected virtual ZDateTime GetDeliveryDate() { return ZDateTime.Empty; }
		public virtual ZDateTime GetShipmentDate() { return IsExport ? ATD : ATA; }
		public virtual ZString EditSecurityMessage { get { return ZString.Empty; } }
		public virtual bool EditSecurityLock { get { return false; } }
		public virtual void PostedStateChanged() { }
		public virtual int ContainerCount { get { return 0; } }
		public virtual ZDecimal TEUCount { get { return 0m; } }
		public virtual int OuterPackTotal { get { return 0; } }
		public virtual ZString DefaultChargeGroup { get { return ZString.Empty; } }
		public virtual ZString OperationalJobRef { get { return ZString.Empty; } }
		public virtual bool ShowOperationalJobRefFilter { get { return false; } }
		public virtual ILocation FixedPlaceOfSupply { get { return null; } }

		public SecurityCheckpoint AuditSecurity
		{
			get
			{
				return GetJobDeniedRateSecurityCheckPoint() ?? GetAuditSecurityCore();
			}
		}

		protected virtual SecurityCheckpoint GetAuditSecurityCore()
		{
			return Env.Security.None;
		}

		public SecurityCheckpoint JobInvoicingSecurity
		{
			get
			{
				return GetJobDeniedRateSecurityCheckPoint() ?? GetJobInvoicingSecurityCore();
			}
		}

		protected virtual SecurityCheckpoint GetJobInvoicingSecurityCore()
		{
			return Env.Security.None;
		}

		public SecurityCheckpoint EditSecurityCheckpoint
		{
			get
			{
				return GetJobDeniedRateSecurityCheckPoint() ?? GetEditSecurityCheckpointCore();
			}
		}

		public virtual SecurityCheckpoint ViewConsolCostFromOtherBRNorDEPSecurityCheckpoint
		{
			get { return Env.Security.None; }
		}

		protected virtual SecurityCheckpoint GetEditSecurityCheckpointCore()
		{
			return Env.Security.None;
		}

		SecurityCheckpoint GetJobDeniedRateSecurityCheckPoint()
		{
			return Job == null ? null : Job.DeniedRateSecurityCheckPoint;
		}

		public bool IncludeInConsolCosting(bool includeRelatedShipments) { return IncludeInConsolCostingCore(includeRelatedShipments); }

		public ZString GetVoyageVesselOrFlightDatesCore(string transportMode, ZDateTime departureDate, string vessel, string voyageFlightNumber)
		{
			var result = ZString.Empty;
			var vesselOrFlightDate = ZString.Empty;

			if (transportMode == Enterprise.Core.Constants.TransportModes.Air)
			{
				vesselOrFlightDate = departureDate.ToShortDateString().ToUpper();
			}
			else if (transportMode == Enterprise.Core.Constants.TransportModes.Sea)
			{
				vesselOrFlightDate = vessel;
			}

			result += voyageFlightNumber;
			if (!vesselOrFlightDate.IsEmpty)
			{
				result += "/" + vesselOrFlightDate;
			}

			return result;
		}

		protected virtual bool IncludeInConsolCostingCore(bool includeRelatedShipments) { return false; }

		public void SetDefaultsForNewCharge(JobCharge charge) { SetDefaultsForNewChargeCore(charge); }
		protected virtual void SetDefaultsForNewChargeCore(JobCharge charge) { }

		public virtual bool TryGetContainersCostShare(AccChargeCode chargeCode, IEnumerable<IJobPaymentBasis> paymentBases, ZGuid consolPK, out ZDecimal cost)
		{
			cost = 0m;
			return false;
		}

		public virtual bool HasContainerCostShare(AccChargeCode chargeCode)
		{
			return false;
		}

		public virtual string GetPaymentTermForCreditorOverride(CostSell costOrSell) => PaymentTerm?.GetPaymentTermInfo(costOrSell)?.Value ?? string.Empty;

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

	public class JobHeaderTypeDecider : TypeDecider
	{
		public override Type GetTypeForNew()
		{
			return ObjectFactory.GetType<IJobHeader>();
		}

		public override Type GetTypeForBinding()
		{
			return GetTypeForNew();
		}

		public override Type GetTypeForLoad(DataRow row, BusinessObjectFactory factory)
		{
			return GetTypeForNew();
		}
	}

	/// <summary>
	/// "Vanilla" Job that can be used in any solution. Use Job from Accounting solution if you need to have Job Invoicing features.
	/// </summary>
	[DescriptionProperty(AutoJobHeader.Schema.JH_HoldReason)]
	[ActionFieldFollow(true)]
	[CodeProperty(AutoJobHeader.Schema.JH_JobNum)]
	[UniversalCopyIgnoreBusinessObject]
	public abstract class JobHeader : AutoJobHeader, IJobHeader, IJobNumber, IWorkflowTriggerEventSource, ILicensedComponent, IHaveJobHeader, IDataVersionLoggingSupported
	{
		public new abstract class Schema : AutoJobHeader.Schema
		{
			public const string JH_TotalProfitRevenueMargin = "JH_TotalProfitRevenueMargin";
		}

		public JobHeader(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
			ConcurrencyInfo.SetConcurrencyPolicy(this, nameof(JH_Status), ConcurrencyPolicy.Strict);
			ConcurrencyInfo.SetConcurrencyPolicy(this, nameof(JH_OA_LocalChargesAddr), ConcurrencyPolicy.Strict);
			ConcurrencyInfo.SetConcurrencyPolicy(this, nameof(JH_UniqueJobInvoiceNumber), ConcurrencyPolicy.Strict);
			ConcurrencyInfo.SetConcurrencyPolicy(this, nameof(JH_SystemLastEditTimeUtc), ConcurrencyPolicy.Ignore);
			ConcurrencyInfo.SetConcurrencyPolicy(this, nameof(JH_SystemLastEditUser), ConcurrencyPolicy.Ignore);

			this.RecordJobConstructorStackTrace();

			this.DefaultValuesHasBeenAssigned = false;

			if (factory.TryGetDisposableManager(out var manager))
			{
				manager.Subscribe(this);
			}
		}

		bool hasChangesBeforeSaving;

		public string ConstructorStackTrace => CriticalValidationInfoCollectorService.GetService(Factory).GetInfoSafe(PK, CriticalValidationInfoCollectorServiceKeyType.JobConstructorStackTrace, useNeverClearedInfo: true);

		#region Dependencies
		IJobSavingWithoutMutexErrorReporter JobSavingWithoutMutexErrorReporter => jobSavingWithoutMutexErrorReporter ?? new JobSavingWithoutMutexErrorReporter();
		IJobSavingWithoutMutexErrorReporter jobSavingWithoutMutexErrorReporter;

#if DEBUG
		public void SubstituteJobSavingWithoutMutexErrorReporter_ForTestOnly(IJobSavingWithoutMutexErrorReporter replacement) => jobSavingWithoutMutexErrorReporter = replacement;
#endif
		#endregion

		public virtual ZDecimal JH_TotalProfitRevenueMargin { get; }

		public ZPropertyInfo JH_TotalProfitRevenueMarginInfo => GetZPropertyInfo(nameof(JH_TotalProfitRevenueMargin));

		public override void OnLoaded()
		{
			base.OnLoaded();
			CollectOnLoadedInfo();
		}

		void CollectOnLoadedInfo()
		{
			var infoCollector = CriticalValidationInfoCollectorService.GetOrCreateService(Factory);
			infoCollector.AddInfoWhenAllowed(PK, CriticalValidationInfoCollectorServiceKeyType.LoadJobHeader,
				() => FormattableString.Invariant($@"JH_GC: {JH_GC}, JH_Parent_ID: {JH_ParentID}, JH_ParentTableCode: {JH_ParentTableCode}, CurrentCompany: {GlbCompany.CurrentCompany.PK}, job.IsInDatabase: {IsInDatabase}, StackTrace ->\r\n {System.Environment.StackTrace}"));
		}

		public static readonly JobHeaderTypeDecider TypeDecider = new JobHeaderTypeDecider();

		#region IDataVersionLoggingSupported

		bool IDataVersionLoggingSupported.IsDataVersionsAutoLogged => true;

		DataVersionLogValueFormatter IDataVersionLoggingSupported.DataVersionLogValueFormatter => this.GetDefaultDataVersionLogFormatter();

		#endregion

		#region GSTID

		public ZGuid GetGSTID(AccChargeCode chargeCode, ZString relatedJobNumber, CostSell costOrSell, out ZGuid overrideInvTaxMsg)
		{
			return GetGSTID(chargeCode, relatedJobNumber, costOrSell, null, out overrideInvTaxMsg);
		}

		public ZGuid GetGSTID(AccChargeCode chargeCode, ZString relatedJobNumber, CostSell costOrSell, ILocation chargeFixedPlaceOfSupplyLocation, out ZGuid overrideInvTaxMsg)
		{
			ZGuid orgPK = costOrSell == CostSell.Cost
							? GetCreditorPK(chargeCode, GetChargeType(chargeCode), ZString.Empty, ZGuid.Empty)
							: GetDebtorPK(chargeCode, relatedJobNumber);

			return GetGSTID(Factory.Load<OrgHeader>(orgPK), chargeCode, Branch, costOrSell, chargeFixedPlaceOfSupplyLocation, ZString.Empty, out overrideInvTaxMsg);
		}

		public ZGuid GetGSTID(JobCharge charge, CostSell costOrSell, out ZGuid overrideInvTaxMsg)
		{
			return GetGSTID(charge, costOrSell, null, out overrideInvTaxMsg);
		}

		public ZGuid GetGSTID(JobCharge charge, CostSell costOrSell, ILocation chargeFixedPlaceOfSupplyLocation, out ZGuid overrideInvTaxMsg)
		{
			overrideInvTaxMsg = ZGuid.Empty;
			if (charge != null)
			{
				return GetGSTID(costOrSell == CostSell.Cost ? charge.CostAccount : charge.SellAccount,
					charge.ChargeCode,
					GetBranchForTax(charge, costOrSell),
					costOrSell,
					chargeFixedPlaceOfSupplyLocation,
					costOrSell == CostSell.Cost ? charge.JR_CostSupplyType : charge.JR_SellSupplyType,
					out overrideInvTaxMsg);
			}

			return ZGuid.Empty;
		}

		GlbBranch GetBranchForTax(JobCharge charge, CostSell costOrSell)
		{
			if (AccountingMasterFilesUtils.IsTaxBranchApplicable && AccountingMasterFilesRegistry.Instance.EnableBranchLevelTaxOverrideRuleConfigurations.Value)
			{
				switch (costOrSell)
				{
					case CostSell.Cost:
						return Factory.Load<GlbBranch>(charge.JR_GB_CostTaxBranch);

					case CostSell.Revenue:
						return Factory.Load<GlbBranch>(charge.JR_GB_SellTaxBranch);
				}
			}

			return charge.Branch;
		}

		public ZGuid GetGSTID(OrgHeader org, AccChargeCode chargeCode, GlbBranch branch, CostSell costOrSell, out ZGuid overrideInvTaxMsg)
		{
			return GetGSTID(org, chargeCode, branch, costOrSell, null, ZString.Empty, out overrideInvTaxMsg);
		}

		public ZGuid GetGSTID(OrgHeader org, AccChargeCode chargeCode, GlbBranch branch, CostSell costOrSell, ILocation chargeFixedPlaceOfSupplyLocation, ZString supplyType, out ZGuid overrideInvTaxMsg)
		{
			overrideInvTaxMsg = ZGuid.Empty;
			ZGuid result = ZGuid.Empty;
			if (org != null && GlbCompany.CurrentCompany.GC_IsGSTRegistered)
			{
				bool taxApplicable;
				var autoJRJHelper = ObjectFactory.Get<IAutoJobRevenueJournalHelper>();

				if (autoJRJHelper.IsAutoJRJEnabled && (Company == null || (org.IsProxyOrg(Company) && !autoJRJHelper.IsExcludedFromAutoJRJ(branch, org, Company.GC_RN_NKCountryCode))))
				{
					taxApplicable = false;
				}
				else
				{
					var companyData = org.CompanyData;
					taxApplicable = costOrSell == CostSell.Cost ? companyData.IsAPTaxApplicable : companyData.IsARTaxApplicable;
				}

				if (taxApplicable && chargeCode != null)
				{
					result = GetGSTIDCore(org, chargeCode, branch, costOrSell, chargeFixedPlaceOfSupplyLocation, supplyType, out overrideInvTaxMsg);
				}
			}

			return result;
		}

		protected virtual ZGuid GetGSTIDCore(OrgHeader org, AccChargeCode chargeCode, GlbBranch branch, CostSell costOrSell, ILocation chargeFixedPlaceOfSupplyLocation, ZString supplyType, out ZGuid overrideInvTaxMsg)
		{
			overrideInvTaxMsg = ZGuid.Empty;
			if (chargeCode != null)
			{
				AccTaxRate taxRate = Factory.Load<AccTaxRate>(chargeCode.AC_AT_GSTRate);
				if (taxRate != null)
				{
					overrideInvTaxMsg = taxRate.AT_A9_DefaultVatClass;
				}
			}
			return chargeCode != null ? chargeCode.AC_AT_GSTRate : ZGuid.Empty;
		}

		public virtual ZGuid GetDebtorPK(AccChargeCode chargeCode, ZString relatedJobNumber)
		{
			return LocalChargesPK;
		}

		public virtual ZGuid GetCreditorPK(AccChargeCode chargeCode, ZString chargeType, ZString invoiceType, ZGuid rateProviderOrgPK)
		{
			return ZGuid.Empty;
		}

		protected virtual ZString GetChargeType(AccChargeCode chargeCode)
		{
			return chargeCode != null ? chargeCode.AC_ChargeType : ZString.Empty;
		}

		#endregion

		#region Loader

		public new class Loader : BusinessObject.Loader
		{
			public Loader(IJobHeaderParent parent)
				: this(parent.Factory, parent)
			{
			}

			public Loader(BusinessObjectFactory factory, IJobHeaderParent parent)
				: base(factory)
			{
				this.parent = parent;

				var provider = parent as IJobHeaderParentProvider;
				if (provider != null)
				{
					this.parent = provider.Parent;
				}
			}

			public JobHeader Load()
			{
				return Load(false, GlbCompany.CurrentCompany);
			}

			public JobHeader Load(bool setParent, bool setJobDefaults = true)
			{
				return Load(setParent, GlbCompany.CurrentCompany, setJobDefaults);
			}

			public JobHeader Load(bool setParent, GlbCompany company, bool setJobDefaults = true)
			{
				JobHeader job = LoadJob(company, true);
				if (job != null)
				{
					job.CollectOnLoadedInfo();

					if (setParent)
					{
						if (setJobDefaults)
						{
							job.Parent = parent;
						}
						else
						{
							using (job.RefreshJobParentSuspender.GetSuspender())
							{
								job.Parent = parent;
							}
						}
					}
				}

				return job;
			}

			public bool HasInactiveJobHeader(GlbCompany company)
			{
				var jobHeader = LoadJob(company, false);
				return jobHeader?.IsCancelled ?? false;
			}

			public MultilingualString GetJobDeleteErrorMessage()
			{
				var jobHeader = LoadJob(null, false);
				return jobHeader?.ReasonForNotAbleToDelete ?? (NoResString)string.Empty;
			}

			JobHeader LoadJob(GlbCompany company, bool getActiveJobOnly, bool shouldQueryDB = false)
			{
				var query = new ZQuery();
				query.AddToFilter(JobHeaderSchema.JH_ParentID, parent.PK);
				if (company != null)
				{
					query.AddToFilter(JobHeaderSchema.JH_GC, company.PK);
				}
				if (getActiveJobOnly)
				{
					query.AddToFilter(JobHeaderSchema.JH_IsActive, true);
				}

				var isNewParent = parent != null && !parent.IsInDatabase;
				query.FetchOnlyFromLocalCache = shouldQueryDB || isNewParent;
				var result = (JobHeader)Factory.LoadTop1<IJobHeader>(query);

				if (result == null && shouldQueryDB && !isNewParent)
				{
					result = LoadJobFromDB(query);
				}

				return result;
			}

			JobHeader LoadJobFromDB(ZQuery sourceQuery)
			{
				var dbQuery = new ZDBOnlyQuery(typeof(JobHeader)) { IgnoreDbQueryCache = true };
				dbQuery.AddToFilter(sourceQuery);
				return (JobHeader)Factory.LoadTop1<IJobHeader>(dbQuery);
			}

			#region GetJobCreationError

			static class MessageTemplates
			{
				public static string GetInvoicingJobCreationNotSupportedMessage(string jobNumber)
				{
					return Res.GetString("7E24E8AF-C2F5-4449-811F-051D24823B86", "Operational job '{0}' does not support creation of invoicing job.", jobNumber);
				}

				[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1099:ResGetStringDefaultTextMustBeStringLiteral", Justification = "Baseline")]
				public static string GetBillingJobLockedResubmitLaterMessage(string jobNumber, string loginName)
				{
					return Res.GetString(
						"7e004c2e-c858-4226-a171-c300da2cf764",
						"User {0} has created a Billing Job for {1} within {2}, but hasn't saved it yet.\r\n" +
						"Please resubmit this message after the user has saved the Billing Job.",
						GetLoginNameSafe(loginName),
						jobNumber,
						BrandingFactory.Instance.ProductName
					);
				}

				public static string GetBillingJobLockedByAnotherUser(string jobNumber, string loginName)
				{
					return Res.GetString(
						"a86fb77f-2970-4222-a7e3-12e2ebafed09",
						"User {1} is in the process of creating the Job {0}. You cannot work on the job until he/she saves it or cancels the changes.",
						jobNumber,
						GetLoginNameSafe(loginName)
					);
				}

				public static string GetActivateBillingJobLockedByAnotherUser(string jobNumber, string loginName)
				{
					return Res.GetString(
						"5CFFC64F-049B-48C7-8F9E-78CD957ABE8E",
						"User {1} is in the process of re-activating the Job {0}. You cannot work on the job until he/she saves it or cancels the changes.",
						jobNumber,
						GetLoginNameSafe(loginName)
					);
				}

				[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1099:ResGetStringDefaultTextMustBeStringLiteral", Justification = "Baseline")]
				public static string GetBillingJobLockedCloseAndRetry(string jobNumber)
				{
					return Res.GetString(
						"8ee854e8-df16-4a60-9e78-235896c89ea0",
						"You have created the job {0} on another form, but haven't saved it yet.\r\n" +
						"Please close or save other forms that use job {0} to continue.",
						jobNumber
					);
				}

				[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1099:ResGetStringDefaultTextMustBeStringLiteral", Justification = "Baseline")]
				public static string GetActivateBillingJobLockedCloseAndRetry(string jobNumber)
				{
					return Res.GetString(
						"706DDDCB-D228-487B-B563-36DA71FB88B2",
						"You have re-activated the job {0} on another form, but haven't saved it yet.\r\n" +
						"Please close or save other forms that use job {0} to continue.",
						jobNumber
					);
				}

				[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Fixed code")]
				public const string UndefinedUserName = "(undefined)";

				static string GetLoginNameSafe(string loginName) => loginName ?? UndefinedUserName;
			}

			public JobCreationError GetJobCreationError()
			{
				JobCreationErrorType errorType = GetJobCreationErrorType();

				if (errorType == JobCreationErrorType.JobIsNotAllowed)
				{
					return new JobCreationError(errorType, MessageTemplates.GetInvoicingJobCreationNotSupportedMessage(parent.JobNumber));
				}

				if (errorType == JobCreationErrorType.MutexError)
				{
					string mutexHolderLoginName = GetMutexHolderLoginName(parent);
					var jobWithMutex = LoadJob(GlbCompany.CurrentCompany, false);

					if (mutexHolderLoginName == GlbStaff.CurrentUser.GS_LoginName)
					{
						if (jobWithMutex != null && jobWithMutex.IsInDatabase)
						{
							return new JobCreationError(errorType, MessageTemplates.GetActivateBillingJobLockedCloseAndRetry(parent.JobNumber));
						}
						else
						{
							return new JobCreationError(errorType, MessageTemplates.GetBillingJobLockedCloseAndRetry(parent.JobNumber));
						}
					}

					if (!Globals.IsUserInteractive)
					{
						Factory.SetContext(BusinessContext.JobLockedByAnotherUser);
					}

					if (jobWithMutex != null && jobWithMutex.IsInDatabase)
					{
						return new JobCreationError(errorType, MessageTemplates.GetActivateBillingJobLockedByAnotherUser(parent.JobNumber, mutexHolderLoginName));
					}
					else
					{
						return new JobCreationError(errorType, MessageTemplates.GetBillingJobLockedByAnotherUser(parent.JobNumber, mutexHolderLoginName));
					}
				}

				throw new InvalidOperationException("Unexpected errorType during job creation: " + errorType);
			}

			public JobCreationError GetJobCreationErrorForService()
			{
				JobCreationErrorType errorType = GetJobCreationErrorType();

				if (errorType == JobCreationErrorType.JobIsNotAllowed)
				{
					return new JobCreationError(errorType, MessageTemplates.GetInvoicingJobCreationNotSupportedMessage(parent.JobNumber));
				}

				if (errorType == JobCreationErrorType.MutexError)
				{
					string mutexHolderLoginName = GetMutexHolderLoginName(parent);
					return new JobCreationError(errorType, MessageTemplates.GetBillingJobLockedResubmitLaterMessage(parent.JobNumber, mutexHolderLoginName));
				}

				throw new InvalidOperationException("Unexpected errorType during job creation: " + errorType);
			}

			JobCreationErrorType GetJobCreationErrorType()
			{
				if (!IsJobAllowedToBeCreated)
				{
					return JobCreationErrorType.JobIsNotAllowed;
				}

				return JobCreationErrorType.MutexError;
			}

			static string GetMutexHolderLoginName(IJobHeaderParent parent)
			{
#if DEBUG
				if (Globals.IsTest && ShouldUseNullMutexHoldingUser_ForTestOnly)
				{
					return null;
				}
#endif

				using (ZGlobalMutex mutex = GetCreateOrActivateJobMutex(parent.PK))
				{
					LockInfo info = mutex.GetLockInfo();
					return info?.UserWithLock?.GS_LoginName;
				}
			}

			bool IsJobAllowedToBeCreated
			{
				get
				{
					var jobInvoicingPlugin = parent as IJobInvoicingPlugIn;
					return jobInvoicingPlugin == null || jobInvoicingPlugin.InvoicingSupporter.CanCreateInvoicingJob;
				}
			}

			public struct JobCreationError
			{
				public JobCreationError(JobCreationErrorType errorType, string errorMessage)
				{
					Type = errorType;
					Message = errorMessage;
				}

				public JobCreationErrorType Type { get; }

				public string Message { get; }

				public override string ToString() => Message;

				public static implicit operator string(JobCreationError value) => value.ToString();

				public static implicit operator ZString(JobCreationError value) => value.ToString();

				public bool IsValidMutexError => Type == JobCreationErrorType.MutexError && !Message.Contains(MessageTemplates.UndefinedUserName);
			}

			public enum JobCreationErrorType
			{
				JobIsNotAllowed,
				MutexError
			}

			#endregion

			#region Create

			public JobHeader TryCreate()
			{
				return TryCreate(false, GlbBranch.CurrentBranch);
			}

			public JobHeader TryCreate(GlbBranch branch)
			{
				return TryCreate(false, branch);
			}

			public JobHeader TryCreateWithMutex(bool fallBackToCurrentBranch = true)
			{
				return TryCreate(true, GlbBranch.CurrentBranch, fallBackToCurrentBranch);
			}

			public JobHeader TryCreateWithMutex(GlbBranch branch)
			{
				return TryCreate(true, branch);
			}

			JobHeader TryCreate(bool aquireMutex, GlbBranch branch, bool fallBackToBranch = true)
			{
				try
				{
					Factory.SetContext(BusinessContext.JobCreatedFromJobLoader);
					var parentBizo = parent as BusinessObject;

					var result = LoadJob(branch.Company, false, true);

					if (result != null && !result.JH_IsActive)
					{
						using (SetJobHasChangesSuspender.IsSuspended ? result.SuspendSettingHasChanges() : null)
						{
							TryActivateJobHeaderWithMutex(aquireMutex, result);
						}

						if (!result.JH_IsActive)
						{
							result = null;
						}
					}
					else if (result == null && IsJobAllowedToBeCreated)
					{
						if (aquireMutex)
						{
							ZGlobalMutex mutex = GetCreateOrActivateJobMutex(parent.PK);
							if (mutex.Lock())
							{
								result = (JobHeader)Factory.New<IJobHeader>();
								result.Mutex = mutex;
							}
						}
						else
						{
							result = (JobHeader)Factory.New<IJobHeader>();
						}
					}

					if (result != null)
					{
						using (result.SuspendSettingHasChanges())
						{
							result.Parent = parent;
							result.JH_GC = branch.Company.PK;
							if ((result.JH_GB.IsEmpty || !(result.Parent is IJobInvoicingPlugIn)) && fallBackToBranch)
							{
								using (result.GetSetJobDefaultsInProgress())
								{
									result.JH_GB = branch.PK;
								}
							}

							if (!result.IsInDatabase
								&& ((ObjectFactory.Get<IAccounting>().Registry?.EnableElectronicProcessingChargeFunctionality as BooleanRegistryItem)?.GetFallBackValueAtAllLevels(result.JH_GC.ToGuid(), Guid.Empty, Guid.Empty) ?? false)
								&& ObjectFactory.Get<IAccounting>().IsIncludedInElectronicProcessingChargeConfiguration(result.JH_A_JOP, (result.Parent as IJobInvoicingPlugIn)?.InvoicingSupporter?.ConsumerType.Code))
							{
								result.JH_IsDisbursement = true;
							}
						}

						if (!SetJobHasChangesSuspender.IsSuspended)
						{
							result.SetHasChangesIfHasErrors();
						}
					}
					return result;
				}
				finally
				{
					Factory.RemoveContext(BusinessContext.JobCreatedFromJobLoader);
				}
			}

			void TryActivateJobHeaderWithMutex(bool aquireMutex, JobHeader jobHeader)
			{
				if (aquireMutex)
				{
					var mutex = GetCreateOrActivateJobMutex(parent.PK);
					if (mutex.Lock())
					{
						jobHeader.ActivateJob();
						jobHeader.Mutex = mutex;
					}
				}
				else
				{
					jobHeader.ActivateJob();
				}
			}

			public FunctionalitySuspender SetJobHasChangesSuspender
			{
				get { return setJobHasChangesSuspender ?? (setJobHasChangesSuspender = new FunctionalitySuspender()); }
			}
			FunctionalitySuspender setJobHasChangesSuspender;

			#endregion

			#region LoadOrCreate

			public JobHeader TryLoadOrCreate()
			{
				return TryLoadOrCreate(false, GlbBranch.CurrentBranch);
			}

			public JobHeader TryLoadOrCreate(GlbBranch branch)
			{
				return TryLoadOrCreate(false, branch);
			}

			public JobHeader TryLoadOrCreateWithMutex()
			{
				return TryLoadOrCreate(true, GlbBranch.CurrentBranch);
			}

			public JobHeader TryLoadOrCreateWithMutex(GlbBranch branch)
			{
				return TryLoadOrCreate(true, branch);
			}

			JobHeader TryLoadOrCreate(bool aquireMutex, GlbBranch branch)
			{
				JobHeader result = Load()
					?? TryCreate(aquireMutex, branch);
				if (result != null && result.parent == null)
				{
					result.Parent = parent;
				}
				return result;
			}

			#endregion

			#region Implementation

			readonly IJobHeaderParent parent;

			protected override Type GetTypeOfBusinessObjectToLoad()
			{
				return typeof(JobHeader);
			}

			#endregion
		}

		public bool IsJobActivating => JH_IsActiveInfo.HasChanges && JH_IsActive;

		#region SetJobDefaultsInProgress

		protected IDisposable GetSetJobDefaultsInProgress()
		{
			return new SetJobDefaultsInProgress(this);
		}

		protected bool IsSetJobDefaultsInProgress;

		protected class SetJobDefaultsInProgress : IDisposable
		{
			public SetJobDefaultsInProgress(JobHeader parentJob)
			{
				this.ParentJob = parentJob;
				parentJob.IsSetJobDefaultsInProgress = true;
			}

			readonly JobHeader ParentJob;

			void IDisposable.Dispose()
			{
				ParentJob.IsSetJobDefaultsInProgress = false;
			}
		}

		#endregion

		protected void SetHasChangesIfHasErrors()
		{
#if DEBUG
			if (Globals.IsTest && Factory.HasAnyOfContexts(BusinessContext.DisableSetHasChangesIfHasErrors_ForTestOnly))
			{
				return;
			}
#endif

			if (!IsInDatabase && !IsSettingHasChangesSuspended && !HasChanges && HasErrors)
			{
				HasChanges = true; //this will force user to save form and so to fix validation errors, otherwise job will be saved with validation errors in any other non GUI Factory.Save call.
			}
		}
		#endregion

		#region DeleteAllJobs

		#region CannotDeleteMessage

		public override MultilingualString ReasonForNotAbleToDelete
		{
			get
			{
				if (IsInDatabase)
				{
					var companyQuery = new ZQuery(JobHeaderSchema.JH_JobNum, JH_JobNum);
					companyQuery.AddToFilter(JobHeaderSchema.JH_ParentID, JH_ParentID);
					var allJobsFromDifferentCompany = Factory.Load<JobHeader>(companyQuery);
					var companyCodesOfJobHeader = string.Join(", ", allJobsFromDifferentCompany.Select(x => x.Company.GC_Code));

					return ResString.GetMultilingualString("E7A997B5-ADAE-46DF-97C8-2F33309AF5B0", @"This record cannot be deleted.
An Invoicing Job Header ({0}) has been created in the company {1}.", JH_JobNum, companyCodesOfJobHeader);
				}
				else
				{
					return (NoResString)JobHeaderParentDeletionHelper.CheckIfCanDeleteJobHeaderParent(JH_ParentID, Res.GetString("d7f57810-ae4d-4deb-a96e-a440ac968537", "This record"));
				}
			}
		}

		public override bool CanDelete
		{
			get { return string.IsNullOrEmpty(ReasonForNotAbleToDelete); }
		}

		#endregion

		public static void DeleteAllJobs(IJobHeaderParent parent, bool withCanDeleteCheck)
		{
			var query = new ZQuery(JobHeaderSchema.JH_ParentID, parent.PK);
			query.AddToFilter(JobHeaderSchema.JH_GC, SQLComparisonOperator.NotEqual, ZGuid.Empty);
			JobHeader[] jobs = (JobHeader[])parent.Factory.Load<IJobHeader>(query);
			foreach (JobHeader job in jobs)
			{
				if (withCanDeleteCheck && !job.CanDelete)
				{
					continue;
				}

				job.Delete();
			}
		}

		public static void DeleteAllJobs(IJobHeaderParent parent)
		{
			DeleteAllJobs(parent, false);
		}

		#endregion

		#region DeactivateAllJobs

		#region CannotDeactivateMessage

		public MultilingualString ReasonForNotAbleToDeactivate
		{
			get
			{
				return (NoResString)JobHeaderParentDeletionHelper.CheckIfCanCancelJobHeaderParent(JH_ParentID, Res.GetString("d7f57810-ae4d-4deb-a96e-a440ac968537", "This record"));
			}
		}

		public bool CanDeactivate
		{
			get { return string.IsNullOrEmpty(ReasonForNotAbleToDeactivate); }
		}

		#endregion

		public static void DeactivateAllJobs(IJobHeaderParent parent, bool withCanDeactivateCheck)
		{
			var query = new ZQuery(JobHeaderSchema.JH_ParentID, parent.PK);
			JobHeader[] jobs = (JobHeader[])parent.Factory.Load<IJobHeader>(query);
			foreach (JobHeader job in jobs)
			{
				if (withCanDeactivateCheck && !job.CanDeactivate)
				{
					continue;
				}

				if (!job.HasChanges)
				{
					job.SetContext(BusinessContext.JobDeactivationForAllCompanies);
				}

				job.MarkAsInactive();
			}
		}

		public static void DeactivateAllJobs(IJobHeaderParent parent)
		{
			DeactivateAllJobs(parent, false);
		}

		#endregion

		#region BusinessObject Overrides

		bool FactoryWithValidContext
		{
			get { return !Factory.HasContext(BusinessContext.IncompleteInvoiceSaving) && !Factory.HasContext(BusinessContext.PreviewInvoice); }
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			JH_GC = Env.CurrentCompany.PK;
		}

		protected override void ReloadCore()
		{
			base.ReloadCore();
			LogResolver.Reset();
		}

		protected sealed override void OnFactorySaving()
		{
			if (FactoryWithValidContext)
			{
				base.OnFactorySaving();

				OnFactorySavingCore();
			}
		}

		protected virtual void OnFactorySavingCore()
		{
		}

		protected sealed override void OnFactorySaved(bool saveSucceeded)
		{
			if (FactoryWithValidContext)
			{
				base.OnFactorySaved(saveSucceeded);
				if (saveSucceeded)
				{
					UnlockMutex();
				}
				else
				{
					if (!IsInDatabase && !JH_JobLocalReference.IsEmpty)
					{
						JH_JobLocalReference = ZString.Empty;
					}
				}

				OnFactorySavedCore(saveSucceeded);
			}
		}

		protected virtual void OnFactorySavedCore(bool saveSucceeded)
		{
		}

		void ResetJHBranchIsNullOccurrenceCounter()
		{
			AccountingMasterFilesRegistry.Instance.JHBranchIsNullOccurrenceCounter.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 0);
		}

		protected virtual void SetDefaultBranchAndDepartmentIfNull()
		{
			if (JH_GB.IsEmpty)
			{
				JH_GB = GlbBranch.CurrentBranch.PK;
			}
			if (JH_GE.IsEmpty)
			{
				JH_GE = GlbDepartment.CurrentDepartment.PK;
			}
		}

		public override void OnSaving()
		{
			ReportJobIsChangedByDifferentCompany();
			CheckWhetherIsEligibleToSave();

			if ((Branch == null) || (Department == null))
			{
				AccountingMasterFilesRegistry.Instance.JHBranchIsNullOccurrenceCounter.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 1 + AccountingMasterFilesRegistry.Instance.JHBranchIsNullOccurrenceCounter.Value);

				if (AccountingMasterFilesRegistry.Instance.JHBranchIsNullOccurrenceCounter.Value <= AccountingMasterFilesRegistry.Instance.MaxOccurrencesBeforeReportJHBranchIsNull.Value)
				{
					bool wasBranchNull = (Branch == null);
					bool wasDepartmentNull = (Department == null);

					SetDefaultBranchAndDepartmentIfNull();

					if (wasBranchNull && (Branch != null))
					{
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
						Logs.AddNew(Events.EditedARecord, string.Format("Branch Changed: From 'null' To default '{0}'", Branch.GB_Code));
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
					}

					if (wasDepartmentNull && Department != null)
					{
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
						Logs.AddNew(Events.EditedARecord, string.Format("Department Changed: From 'null' To default '{0}'", Department.GE_Code));
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
					}
				}
				else
				{
					ResetJHBranchIsNullOccurrenceCounter();
				}
			}

			base.OnSaving();

			Argument.NotNull(Branch, this.JH_GBInfo.Name, this.GetJobCreationWithEmptyBranchMessage());
			Argument.NotNull(Department, this.JH_GEInfo.Name, this.GetJobCreationWithEmptyDepartmentMessage());

			if (!IsInDatabase)
			{
				if (JH_JobLocalReference.IsEmpty)
				{
					IJobReferenceNumberGenerator jobReferenceNumberGenerator = ObjectFactory.Get<IJobReferenceNumberGenerator>();
					JH_JobLocalReference = jobReferenceNumberGenerator.GenerateLocalJobReferenceNumber(JH_GC, JH_GB, JH_GE, Factory);
				}

				this.ReportInvalidJobCreationAsDeveloperException();
			}

			AddBillingJobEditEvent();
			AddStatusUpdatedEvent();
			LogResolver.ResolveLog();

			JobSavingWithoutMutexErrorReporter.ReportErrorWhenJobIsSavedWithoutMutex(this, Mutex);
		}

		protected sealed override void OnFactorySavingBeforeTransactionCore()
		{
			hasChangesBeforeSaving = HasChanges;
			if (FactoryWithValidContext)
			{
				base.OnFactorySavingBeforeTransactionCore();

				if (Parent != null && !Parent.IsDeleted && !IsInDatabase && Parent is ICancellable)
				{
					ICancellable pluginDataAsICancellable = Parent as ICancellable;

					bool isBeingActivated = !pluginDataAsICancellable.IsCancelled && pluginDataAsICancellable.IsCancelledHasChanged;
					if (isBeingActivated && (Branch == null || Department == null))
					{
						Delete();
					}
				}

				OnFactorySavingBeforeTransactionCore2();
			}
		}

		void ReportJobIsChangedByDifferentCompany()
		{
#if DEBUG
			if (Globals.IsTest && Testing.SuspendToTestReportJobIsChangedByDifferentCompanyAttribute.IsActive)
			{
				return;
			}
#endif
			if (!hasChangesBeforeSaving || JH_GC == GlbCompany.CurrentCompany.PK)
			{
				return;
			}

			if(this.HasContext(BusinessContext.JobDeactivationForAllCompanies))
			{
				return;
			}

			/*
			 * For now, we understand that the non-login company's job could be loaded as common requirement by such as Forwarding Team
			 * When you deal this issue
			 *  If you find BusinessContext.InvoicingPlugInGUI, please find what cause the change(s) and stop the changing.
			 *  --> It is because CW1 GUI should only change login company's data.
			 *  ** Suspicious code list
			 *  *** ChargeWithCost -> OnFactorySavingInCompanyContext -> ProcessAccrualAndWIP()
			 *
			 *  If you do not find BusinessContext.InvoicingPlugInGUI, please take note and assign the WI to very senior developer.
			 *  --> It is because the issue can be caused by service task or other unknown process.
			 *  --> And we need to analysis the behavior more detail.
			 */
			var service = CriticalValidationInfoCollectorService.GetService(Factory);
			var messageBuilder = new ZStringBuilder();
			messageBuilder.AppendLine($"CurrentCompany: {GlbCompany.CurrentCompany.PK}({GlbCompany.CurrentCompany.GC_Code})");
			messageBuilder.AppendLine();
			messageBuilder.AppendLine(this.GetAllPropertyValues());
			messageBuilder.AppendLine();
			messageBuilder.AppendLine(service.GetInfoSafe(PK, CriticalValidationInfoCollectorServiceKeyType.JobHeaderJH_OA_LocalChargesAddrSetterCallStack));
			messageBuilder.AppendLine(service.GetInfoSafe(PK, CriticalValidationInfoCollectorServiceKeyType.JobHeaderJH_OA_AgentCollectAddrSetterCallStack));
			messageBuilder.AppendLine(service.GetInfoSafe(PK, CriticalValidationInfoCollectorServiceKeyType.JobJH_GS_NKRepSalesSetterCallStack));
			messageBuilder.AppendLine(service.GetInfoSafe(PK, CriticalValidationInfoCollectorServiceKeyType.JobRegisteredAsEditableChild, useNeverClearedInfo: true));
			messageBuilder.AppendLine(service.GetInfoSafe(PK, CriticalValidationInfoCollectorServiceKeyType.JobChargeLoadFirstInDatabase, useNeverClearedInfo: true));
			messageBuilder.AppendLine(ConstructorStackTrace);
			messageBuilder.AppendLine(service.GetInfoSafe(PK, CriticalValidationInfoCollectorServiceKeyType.MarkJobHeaderAsInactive));
			messageBuilder.AppendLine(GetUserContextSwitchLog());

			ExceptionReporter.Instance.ReportDeveloperException("JobIsChangedByDifferentCompany_3"
				, messageBuilder.ToStringWithNewLineBetweenAppends()
				, new DeveloperNotificationException((NoResString)"Job is not designed to be changed in different company."));
		}

		string GetUserContextSwitchLog()
		{
			if (Env.Instance.UserContextLogger is UserContextSwitchLogger logger)
			{
				return $"UserContextSwitchLog:\r\n{string.Join(System.Environment.NewLine, logger.Logs)}";
			}
			return (NoResString)"No UserContextSwitchLog";
		}

		protected virtual void OnFactorySavingBeforeTransactionCore2()
		{
		}

		public virtual void MarkAsInactive()
		{
			if (IsInDatabase)
			{
				IJobReferenceNumberGenerator jobReferenceNumberGenerator = ObjectFactory.Get<IJobReferenceNumberGenerator>();
				jobReferenceNumberGenerator.AddJobDeactivatedRecordToParentJobLog(JH_ParentID, JH_ParentTableCode, JH_JobLocalReference, Factory);
			}

			if (Parent != null)
			{
				Parent.OnJobDeleting(this);
			}

			Dispose();

			if (IsInDatabase)
			{
				SetJH_IsActiveCore(false);
			}
			else
			{
				base.Delete();
			}

			if (Parent != null)
			{
				Parent.OnJobDeleted(this);
			}

			CriticalValidationInfoCollectorService.GetOrCreateService(Factory).AddLastInfoWhenAllowed(PK
				, CriticalValidationInfoCollectorServiceKeyType.MarkJobHeaderAsInactive
				, () => System.Environment.StackTrace);
		}

		public void ActivateJob()
		{
			var jobLocalReference = JH_JobLocalReference;
			var createdUser = JH_SystemCreateUser;
			var createdTime = JH_SystemCreateTimeUtc;
			SetDefaultValues();
			JH_JobLocalReference = jobLocalReference;
			JH_SystemCreateUser = createdUser;
			JH_SystemCreateTimeUtc = createdTime;
			SetJH_IsActiveCore(true);
			disposed = false;
			LogResolver.MarkActive();
		}

		public override void Delete()
		{
			if (IsInDatabase)
			{
				throw new InvalidOperationException("You cannot delete Job in Database.");
			}

			if (!IsDeleted)
			{
				if (IsInDatabase)
				{
					IJobReferenceNumberGenerator jobReferenceNumberGenerator = ObjectFactory.Get<IJobReferenceNumberGenerator>();
					jobReferenceNumberGenerator.AddJobDeletedRecordToParentJobLog(JH_ParentID, JH_ParentTableCode, JH_JobLocalReference, Factory);
				}

				if (Parent != null)
				{
					Parent.OnJobDeleting(this);
				}

				Dispose();
				base.Delete();

				if (Parent != null)
				{
					Parent.OnJobDeleted(this);
				}
			}
			else
			{
				Dispose();
				base.Delete();
			}

			CriticalValidationInfoCollectorService.GetOrCreateService(Factory).AddLastInfoWhenAllowed(PK, CriticalValidationInfoCollectorServiceKeyType.DeleteJobHeader, () =>
			{
				if (!IsInDatabase)
				{
					return System.Environment.StackTrace;
				}
				return null;
			}, CriticalValidationInfoCollectorService.CollectionFrequency.CollectAlways_ForEveryUserAndAction_THIS_CAN_IMPACT_PERFORMANCE);
		}

		protected override void DeleteForDataRefresh()
		{
			var shouldFireEvent = Parent != null && !IsDeleted;

			if (shouldFireEvent)
			{
				Parent.OnJobDeleting(this);
			}

			base.DeleteForDataRefresh();

			if (shouldFireEvent)
			{
				Parent.OnJobDeleted(this);
			}
		}

		protected override AutologState AutoLoggingState => AutologState.AutoLogged;

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore()
		{
			return new JobHeaderFetchStrategy(this);
		}

		internal void AddBillingJobEditEvent()
		{
			Logs.AddNewSinglePerTransaction(Events.BillingJobEdit);
		}

		internal void AddStatusUpdatedEvent()
		{
			if (IsInDatabase && JH_StatusInfo.OriginalValue.ToString() == JH_Status)
			{
				return;
			}

			var parameters = new Dictionary<string, string>();
			parameters[CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Type] = Constants.EventReferenceMessageTypes.JobStatus;
			parameters[CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Old] = IsInDatabase ? JH_StatusInfo.OriginalValue.ToString() : null;
			parameters[CargoWise.EventReference.Constants.EventReferenceParameters.Codes.New] = JH_Status;
			parameters[CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Reason] = JH_HoldReason;

			Logs.AddNewSinglePerTransaction(Events.StatusUpdated, StmALog.GenerateEventReference(string.Empty, parameters));
		}

		protected override void OnUpdatedByDataRefresh()
		{
			base.OnUpdatedByDataRefresh();
			JH_OA_LocalChargesAddrInfo.RefreshBinding();
		}

		#endregion

		#region Property Overrides

		public override ZGuid JH_JH_ParentJob
		{
			get { return base.JH_JH_ParentJob; }
			set
			{
				JobHeader header = Factory.Load<JobHeader>(value);
				if (header == null || header.JH_GC == this.JH_GC)
				{
					base.JH_JH_ParentJob = value;
				}
				else
				{
					var service = CriticalValidationInfoCollectorService.GetService(Factory);
					var message = new ZStringBuilder();
					message.Append(ZString.Format("Tried to set JH_JH_ParentJob on JobHeader(PK:{0}, GC:{1}) to JobHeader(PK:{2}, GC:{3}), CurrentCompany: {4}",
						PK, JH_GC, value, header.JH_GC, GlbCompany.CurrentCompany.PK));

					if (service != null)
					{
						message.Append((NoResString)"Current Job:");
						message.Append(service.GetInfo(PK, CriticalValidationInfoCollectorServiceKeyType.LoadJobHeader));
						message.Append(service.GetInfo(PK, CriticalValidationInfoCollectorServiceKeyType.JH_GCChangedFromNonEmptyToNonEmpty));
						message.Append(this.GetAllPropertyValues());
						message.Append((NoResString)"Parent Job:");
						message.Append(service.GetInfo(value, CriticalValidationInfoCollectorServiceKeyType.LoadJobHeader));
						message.Append(service.GetInfo(value, CriticalValidationInfoCollectorServiceKeyType.JH_GCChangedFromNonEmptyToNonEmpty));
						message.Append(header.GetAllPropertyValues());
					}
					message.Append((NoResString)"Please assign this issue to Ajit Prabhu/Andrii Sarnavskyi for further investigation");
					ErrorReporter.Instance.Report("JH_JH_ParentJob Setter_1", message.ToStringWithNewLineBetweenAppends(), null);
				}
			}
		}

		public override ZString JH_Status
		{
			get
			{
				if (IsDeleted)
				{
					return JobHeaderStatus.Closed.Code;
				}
				else
				{
					return base.JH_Status;
				}
			}
			set
			{
				if (JH_Status != value)
				{
					if (value == JobHeaderStatus.Closed.Code)
					{
						JH_A_JCL = ZDateTime.Now;
					}
					else
					{
						if (!JH_A_JOP.IsValid)
						{
							JH_A_JOP = ZDateTime.Now;
						}
						JH_A_JCL = ZDateTime.Empty;
					}

					base.JH_Status = value;
				}
			}
		}

		[ResourceStringData("79d95fb8-29a5-47db-891b-0fe92fdffc51", ShortCaption = "Hold Reason", Caption = "Job Status Hold reason")]
		public override ZString JH_HoldReason { get => base.JH_HoldReason; set => base.JH_HoldReason = value; }

		public override ZGuid JH_GC
		{
			get { return base.JH_GC; }
			set
			{
				var previousValue = JH_GC;
				base.JH_GC = value;
				RecordLastJH_GCChangeFromNonEmptyToNonEmpty(previousValue);
				this.RecordLastJH_GCChange();
			}
		}

		void RecordLastJH_GCChangeFromNonEmptyToNonEmpty(ZGuid previousValue)
		{
			if (IsInDatabase && previousValue != ZGuid.Empty && JH_GC != ZGuid.Empty && previousValue != JH_GC)
			{
				CriticalValidationInfoCollectorService
					.GetOrCreateService(Factory)
					.AddInfoWhenAllowed(PK,
										CriticalValidationInfoCollectorServiceKeyType.JH_GCChangedFromNonEmptyToNonEmpty,
										() => FormattableString.Invariant($"Previous JH_GC: {previousValue}, New JH_GC: {JH_GC}, CurrentCompany: {GlbCompany.CurrentCompany.PK}, StackTrace ->\r\n{System.Environment.StackTrace}"));
			}
		}

		public override ZGuid JH_GB
		{
			get
			{
				return base.JH_GB;
			}
			set
			{
				var previousValue = JH_GB;
				base.JH_GB = value;
				if (!IsValidationSuspended && GlbBranchCombinationValidation.ShouldValidateCombination(Branch))
				{
					Validation.ValidateJH_GE();
				}

				this.RecordLastJH_GBChangeFromValidToEmpty(previousValue);
				this.RecordLastJH_GBChange();
			}
		}

		public override ZGuid JH_GE
		{
			get
			{
				return base.JH_GE;
			}
			set
			{
				base.JH_GE = value;
				this.RecordLastJH_GEChange();
			}
		}

		[BusinessObjectTestExclude]
		[ReadOnly(true)]
		public override ZBool JH_IsActive
		{
			get
			{
				return base.JH_IsActive;
			}
			set
			{
				ErrorReporter.Instance.Report("Cannot update JH_IsActive", "We should call MarkAsInactive or ActivateJob to modify JH_IsActive", null);
			}
		}

		void SetJH_IsActiveCore(bool value)
		{
			base.JH_IsActive = value;
		}

		[ActionField(ReadOnly = true)]
		public override ZBool JH_IsDisbursement { get => base.JH_IsDisbursement; set => base.JH_IsDisbursement = value; }

		#endregion

		#region Active Log Resolver

		ActiveLogResolver LogResolver => logResolver ?? (logResolver = new ActiveLogResolver(this));
		ActiveLogResolver logResolver;

		class ActiveLogResolver
		{
			public ActiveLogResolver(JobHeader jobHeader)
			{
				JobHeader = jobHeader;
			}

			public void MarkActive()
			{
				ActivateJobLogEventTime = ZDateTime.Now;
			}

			public void ResolveLog()
			{
				if (JobHeader.IsInDatabase && ActivateJobLogEventTime.HasValue)
				{
					var jobReferenceNumberGenerator = ObjectFactory.Get<IJobReferenceNumberGenerator>();
					jobReferenceNumberGenerator.AddJobActivatedRecordToParentJobLog(JobHeader.JH_ParentID, JobHeader.JH_ParentTableCode, JobHeader.JH_JobLocalReference, JobHeader.Factory, ActivateJobLogEventTime.Value);
				}
				ActivateJobLogEventTime = null;
			}

			public void Reset() => ActivateJobLogEventTime = null;

			ZDateTime? ActivateJobLogEventTime;

			JobHeader JobHeader { get; }
		}

		#endregion

		#region UniqueIndexFailureHandler

		protected override IEnumerable<IUniqueIndexFailureHandler> UniqueIndexFailureHandlers
		{
			get { yield return new MyUniqueIndexFailureHandler(this); }
		}

		class MyUniqueIndexFailureHandler : IUniqueIndexFailureHandler
		{
			public MyUniqueIndexFailureHandler(JobHeader jobHeader)
			{
				JobHeader = jobHeader;
			}

			readonly JobHeader JobHeader;

			public void NotifyUserAndAttemptToResolve(INotificationHandler notifier, string indexName)
			{
				var jobHeader_JH_GC_Code = JobHeader.JH_GC.IsValid ? JobHeader.Factory.Load<GlbCompany>(JobHeader.JH_GC)?.GC_Code ?? ZString.Empty : ZString.Empty;
				var message = new ZStringBuilder(FormattableString.Invariant(
$@"A Job Header record with same number and company ({JobHeader.JH_JobNum}, {jobHeader_JH_GC_Code}) already exist in the database.
System will update the number, try to save again. If it doesn't solve your problem, you must close this job and re-apply your changes.
"));

				if (JobHeader.IsManuallyCreated)
				{
					message.AppendLine(Res.GetString("FAF0D897-13D4-4867-B561-C9802617718E", "If this Job Header was copied with Universal Copy Template, please ensure that Job Number is not copied in the Copy Template."));
				}

				notifier.ReportError(message.ToString(), Res.GetString("d49b3e2e-925c-41b0-832a-751b5b7134da", "Job changes"));

				JobHeader.JH_JobNum = ZString.Empty;
				if (JobHeader.Parent != null)
				{
					IFountainResolver generator = JobHeader.Parent as IFountainResolver;
					if (generator != null)
					{
						generator.TryToResolve();
					}
				}
			}

			public IEnumerable<string> HandledUniqueIndexNames
			{
				get
				{
					yield return JobHeaderSchema.Constants.Indexes.NR_UX__JH_JobNum_JH_GC;
					yield return JobHeaderSchema.Constants.Indexes.FK_UC__JH_GC_JH_ParentID;
				}
			}
		}

		#endregion

		#region Mutex

		public static ZGlobalMutex GetCreateOrActivateJobMutex(ZGuid pK)
		{
			return new ZGlobalMutex(MutexIDs.JobBeingCreatedForShipment, GetMutexKey(pK));
		}

		static ZString GetMutexKey(ZGuid pK)
		{
			return pK + "_" + GlbCompany.CurrentCompany.GC_Code;
		}

		void UnlockMutex()
		{
			if (Mutex != null && Mutex.HasLock)
			{
				Mutex.Unlock();
			}
		}

		ZGlobalMutex Mutex;

#if DEBUG
		public static ZGlobalMutex GetMutex_ForTestOnly(ZGuid pK)
		{
			return GetCreateOrActivateJobMutex(pK);
		}

		public static ZGlobalMutex GetMutexForTest(JobHeader job)
		{
			return job.Mutex;
		}

		public static IDisposable SetTemporaryNullMutexHoldingUser_ForTestOnly()
		{
			ShouldUseNullMutexHoldingUser_ForTestOnly = true;

			return new DisposableAction(() => ShouldUseNullMutexHoldingUser_ForTestOnly = false);
		}

		[ThreadStatic]
		static bool ShouldUseNullMutexHoldingUser_ForTestOnly;
#endif

		#endregion

		#region Parent

		IJobHeaderParentCore IJobHeader.Parent => Parent;

		public IJobHeaderParent Parent
		{
			get
			{
				if (parent == null && IsManuallyCreated && JH_ParentID.IsValid && JH_ParentTableCode.IsValid)
				{
					var parentType = BusinessObjectFactory.GetBusinessObjectBaseTypeFromTablePrefix(JH_ParentTableCode, false);
					if (parentType != null)
					{
						parent = Factory.Load(parentType, JH_ParentID) as IJobHeaderParent;
					}
				}
				return parent;
			}
			set
			{
				parent = value;
				RefreshParent();
			}
		}

		void RefreshParent()
		{
			using (TemporarilyLoginToJobCompany())
			{
				if (!IsDeleted && JH_IsActive && !RefreshJobParentSuspender.IsSuspended)
				{
					if (parent != null && parent.Factory == Factory)
					{
						parent.OnJobCreating(this);
					}

					SetParentCore(parent);

					if (parent != null && parent.Factory == Factory)
					{
						parent.OnJobCreated(this);
					}
					DefaultValuesHasBeenAssigned = true;
				}
			}
		}

		IDisposable TemporarilyLoginToJobCompany()
		{
			var userContext = (!IsDeleted &&
							GlbCompany.CurrentCompany.PK != JH_GC &&
							JH_GB.IsValid &&
							JH_GE.IsValid)
				? new UserContext(Env.CurrentUserPK, JH_GB.ToGuid(), JH_GE.ToGuid())
				: null;
			return AddInterCompanyJobOperationBusinessContextAfterLoginToJobCompany(userContext);
		}

		public IDisposable AddInterCompanyJobOperationBusinessContextAfterLoginToJobCompany(IUserContext context)
		{
			DisposableAction wrappedAction = null;
			var changedUserContext = (context != null && !IsDeleted && context.Company.PK == JH_GC.ToGuid()) ? Env.SetTemporaryUserContext(context) : null;
			if (changedUserContext != null)
			{
				wrappedAction = new DisposableAction(CreateAction, DisposeAction);
			}
			return wrappedAction;

			void CreateAction()
			{
				this.SetContext(BusinessContext.InterCompanyJobOperation);
			}
			void DisposeAction()
			{
				changedUserContext.Dispose();
				this.RemoveContext(BusinessContext.InterCompanyJobOperation);
			}
		}

		protected virtual void SetParentCore(IJobHeaderParent value) { }

		IJobHeaderParent parent;

		FunctionalitySuspender fRefreshJobParentSuspender;

		internal FunctionalitySuspender RefreshJobParentSuspender
		{
			get { return fRefreshJobParentSuspender ?? (fRefreshJobParentSuspender = new FunctionalitySuspender()); }
		}

		void ResetParentScreeningStatus(ZGuid orgAddressPK)
		{
			if (!IsDeleted && HasChanges)
			{
				IShouldUpdateScreeningStatus parentProvider = null;
				IScreeningStatusProvider parentScreeningStatusProvider = null;

				if (Parent != null)
				{
					parentProvider = Parent as IShouldUpdateScreeningStatus;
					parentScreeningStatusProvider = Parent.IsDeleted ? null : Parent as IScreeningStatusProvider;
				}

				if (parentProvider != null)
				{
					ScreeningStatusUpdater.SetShouldUpdateScreeningStatusByOrgAddress(parentProvider, parentScreeningStatusProvider?.ScreeningStatus, Factory, orgAddressPK);
				}
			}
		}

		#endregion

		#region IJobNumber

		string IJobNumber.JobNumber
		{
			get { return this.JH_JobNum; }
		}

		public override ZString JH_JobNum
		{
			get
			{
				return base.JH_JobNum;
			}
			set
			{
				if (Factory != null && !JH_JobNumInfo.Value.Equals(value))
				{
					if (!JH_JobNum.IsEmpty)
					{
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
						Logs.AddNew(Events.EditedARecord, ZString.Format("Changed job number from {0} to {1}", JH_JobNum, value));
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
					}

					Factory.UpdateNaturalKeyCache(this, JobHeaderSchema.JH_JobNum, JH_JobNum, value);
				}
				base.JH_JobNum = value;
			}
		}

		#endregion

		#region IDisposable Members

		public bool IsDisposed => disposed;

		bool disposed;

		public void Dispose()
		{
			if (!IsInDatabase && !IsDeleted && (Mutex?.HasLock ?? false))
			{
				CriticalValidationInfoCollectorService.GetOrCreateService(Factory).AddInfoWhenAllowed(PK, CriticalValidationInfoCollectorServiceKeyType.JobDisposedBeforeSaving, () => System.Environment.StackTrace, CriticalValidationInfoCollectorService.CollectionFrequency.CollectAlways_ForEveryUserAndAction_THIS_CAN_IMPACT_PERFORMANCE);
			}

			DisposeSafely();
			GC.SuppressFinalize(this);
		}

		~JobHeader()
		{
			// Do not need to dispose the Mutex if it is null.
			// DisposableActionForDbConnection() might spin up an entirely new DB connection; this is pointless if the Mutex is null.
			// And extremely dangerous on the finalizer thread.
			// And, if the app is shutting down, it is not possible to create a new DB connection.
			if (Mutex == null || System.Environment.HasShutdownStarted)
			{
				return;
			}

			using (Db.DisposableActionForDbConnection())
			{
				DisposeSafely();
			}
		}

		void DisposeSafely()
		{
			if (!disposed)
			{
				try
				{
					UnlockMutex();
				}
				catch (Exception exc)
				{
					if (exc.IsCriticalException())
					{
						throw;
					}
				}

				disposed = true;
			}
		}

		#endregion

		#region Dispose and Prevent Save

		public void DisposeAndPreventSave()
		{
			Factory.SetContext(BusinessContext.JobIsDisposedShouldPreventSave);
			Dispose();
		}

		public void DisposeAndDeleteNew()
		{
			Dispose();

			if (!IsInDatabase)
			{
				Delete();
			}
		}

		void CheckWhetherIsEligibleToSave()
		{
			if (Factory.HasContext(BusinessContext.JobIsDisposedShouldPreventSave))
			{
				var exceptionHeader = ResString.GetMultilingualString("8f3d5eeb-1c05-44c4-8af7-a2597cbd3008", "Cannot Save Invoicing Job Header");
				var exceptionMsg = ResString.GetMultilingualString("5aa6e282-355c-4b54-9748-ad835212d910",
					"Cannot save the Invoicing Job Header because it's already disposed.\r\nThis may due to errors occurred before closing the form.\r\n\r\nPlease reload current form to continue.");
				throw new ZCannotSaveException(exceptionMsg, exceptionHeader);
			}
		}

		#endregion

		#region Job Defaults

		public bool DefaultValuesHasBeenAssigned
		{
			get;
			protected set;
		}

		public virtual void SetDefaultsForJob()
		{
		}

		protected abstract bool IsChargesCollectionLoaded { get; }

		#endregion

		#region IWorkflowTriggerEventSource Members

		public IReadOnlyList<IWorkflowProviderCore> ParentWorkflowProviders
		{
			get
			{
				InitialiseForWorkflow();
				if (Parent is IWorkflowProviderCore parent && parent != null)
				{
					return new IWorkflowProviderCore[1] { parent };
				}
				return Array.Empty<IWorkflowProviderCore>();
			}
		}

		public void InitializeParentFromGenericJobWithSettingDefaults()
		{
			if (!IsDeleted && !JH_ParentID.IsEmpty && !JH_ParentTableCode.IsEmpty)
			{
				if (Parent == null)
				{
					var genericJob = this.LoadGenericJob();
					if (genericJob != null)
					{
						Parent = genericJob.Consumer;
					}
				}
				else if (!DefaultValuesHasBeenAssigned && !Factory.HasContext(BusinessContext.SkipJobHeaderRefreshParentDuringWIPAccrualReversing))
				{
					RefreshParent();
				}
			}
		}

		public void InitializeParentFromGenericJobWithoutSettingDefaults()
		{
			using (RefreshJobParentSuspender.GetSuspender())
			{
				InitializeParentFromGenericJobWithSettingDefaults();
			}
		}

		protected virtual void InitialiseForWorkflow()
		{
			InitializeParentFromGenericJobWithSettingDefaults();
		}

		public IGlbCompany JobHeaderCompany
		{
			get { return Company; }
		}

		#endregion

		#region Quote Charges

		public void CopyChargesFromSpotQuote()
		{
			if (!IsSpotQuoteDetailCopyingSuspended)
			{
				CopyChargesFromSpotQuoteCore();
			}
		}

		protected virtual void CopyChargesFromSpotQuoteCore()
		{
		}

		public bool IsSpotQuoteDetailCopyingSuspended { get; set; }

		public virtual void CopyJobBillingInformation(IJobHeaderParent jobHeaderParent, bool skipTransactionInfo, bool overrideComment, bool resetGSTTaxDefault, bool copyExchangeRates, bool copyRateAudit)
		{
		}

		#endregion

		#region OA

		[List("Lookups.AgentCollects")]
		public override ZGuid JH_OA_AgentCollectAddr
		{
			get { return base.JH_OA_AgentCollectAddr; }
			set
			{
				var oldvalue = JH_OA_AgentCollectAddr;
				base.JH_OA_AgentCollectAddr = value;
				ResetParentScreeningStatus(base.JH_OA_AgentCollectAddr);

				var infoCollector = CriticalValidationInfoCollectorService.GetOrCreateService(Factory);
				infoCollector.AddInfoWhenAllowed(PK, CriticalValidationInfoCollectorServiceKeyType.JobHeaderJH_OA_AgentCollectAddrSetterCallStack,
					() => FormattableString.Invariant($@"JH_OA_AgentCollectAddr: Old Value: {oldvalue}, New Value: {JH_OA_AgentCollectAddr} , StackTrace ->\r\n {System.Environment.StackTrace}"));
			}
		}

		[List("Lookups.LocalCharges")]
		public override ZGuid JH_OA_LocalChargesAddr
		{
			get { return base.JH_OA_LocalChargesAddr; }
			set
			{
				if (value != JH_OA_LocalChargesAddr)
				{
					var oldValue = JH_OA_AgentCollectAddr;
					base.JH_OA_LocalChargesAddr = value;

					if (LocalChargesAddrChanged != null)
					{
						LocalChargesAddrChanged(this, EventArgs.Empty);
					}

					var infoCollector = CriticalValidationInfoCollectorService.GetOrCreateService(Factory);
					infoCollector.AddInfoWhenAllowed(PK, CriticalValidationInfoCollectorServiceKeyType.JobHeaderJH_OA_LocalChargesAddrSetterCallStack,
						() => FormattableString.Invariant($@"JH_OA_LocalChargesAddr: Old Value: {oldValue}, New Value: {JH_OA_LocalChargesAddr} , StackTrace ->\r\n {System.Environment.StackTrace}"));
				}

				ResetParentScreeningStatus(base.JH_OA_LocalChargesAddr);
			}
		}

		public event EventHandler LocalChargesAddrChanged;

		public OrgHeader AgentCollect
		{
			get { return !IsDeleted && AgentCollectAddr != null ? AgentCollectAddr.Header : null; }
		}

		public OrgHeader LocalCharges
		{
			get { return !IsDeleted && LocalChargesAddr != null ? LocalChargesAddr.Header : null; }
		}

		public virtual CurrencyConverter CurrencyConverter { get; }

		[List("Lookups.AgentCollects")]
		public virtual ZGuid AgentCollectPK
		{
			get { return !IsDeleted && AgentCollectAddr != null ? AgentCollectAddr.OA_OH : ZGuid.Empty; }
			set
			{
				OrgHeader org = Factory.Load<OrgHeader>(value);
				JH_OA_AgentCollectAddr = GetDefaultAddressByHeader(org);
				AgentCollectPKInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo AgentCollectPKInfo
		{
			get { return GetZPropertyInfo(nameof(AgentCollectPK)); }
		}

		[List("Lookups.LocalCharges")]
		public virtual ZGuid LocalChargesPK
		{
			get { return !IsDeleted && LocalChargesAddr != null ? LocalChargesAddr.OA_OH : ZGuid.Empty; }
			set
			{
				OrgHeader org = Factory.Load<OrgHeader>(value);
				JH_OA_LocalChargesAddr = GetDefaultAddressByHeaderInCommonLanguage(org);
				LocalChargesPKInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo LocalChargesPKInfo
		{
			get { return GetZPropertyInfo(nameof(LocalChargesPK)); }
		}

		protected override ZAddress GetNewJH_OA_AgentCollectAddr_ZAddress()
		{
			ZAddress result = base.GetNewJH_OA_AgentCollectAddr_ZAddress();
			result.DefaultAddressType = AddressType.ARM;
			result.GetDefaultAddress = GetDefaultAddressByHeader;
			return result;
		}

		protected override ZAddress GetNewJH_OA_LocalChargesAddr_ZAddress()
		{
			return GetLocalZAddressWithContact();
		}

		protected ZGuid GetDefaultAddressByHeader(IOrgHeader org)
		{
			OrgAddress result = null;
			OrgHeader header = org as OrgHeader;
			if (header != null)
			{
				result = header.Addresses.DefaultAddressOfType(OrgAddressType.Receivables)
					?? header.Addresses.DefaultAddressOfType(OrgAddressType.Postal)
					?? header.Addresses.DefaultAddressOfType(OrgAddressType.Office);
			}
			return result == null ? ZGuid.Empty : result.PK;
		}

		public static ZGuid GetDefaultAddressByHeaderInCommonLanguage(IOrgHeader org)
		{
			OrgAddress result = null;
			OrgHeader header = org as OrgHeader;
			if (header != null && header.AddressesActive.Count > 0)
			{
				OrgAddressDefaultFinder finder = new OrgAddressDefaultFinder(IBusinessObjectCollectionExtensions.ToArray<OrgAddress>(header.AddressesActive), true, null);

				result = finder.DefaultAddressOfType(OrgAddressType.Receivables)
					?? finder.DefaultAddressOfType(OrgAddressType.Postal)
					?? finder.DefaultAddressOfType(OrgAddressType.Office);
			}

			return result == null ? ZGuid.Empty : result.PK;
		}

		#endregion

		#region LocalZAddressWithContact

		public ZAddressWithContact LocalZAddressWithContact
		{
			get
			{
				if (fLocalZAddressWithContact == null)
				{
					fLocalZAddressWithContact = GetLocalZAddressWithContact();
				}
				return fLocalZAddressWithContact;
			}
		}
		ZAddressWithContact fLocalZAddressWithContact;

		ZAddressWithContact GetLocalZAddressWithContact()
		{
			var result = new ZAddressWithContact(JH_OC_LocalBillingContactInfo, JH_OA_LocalChargesAddrInfo);
			result.DefaultAddressType = AddressType.ARM;
			result.GetDefaultAddress = GetDefaultAddressByHeaderInCommonLanguage;
			return result;
		}

		#endregion

		#region RateSecurityCheckpPoint

		public SecurityCheckpoint DeniedRateSecurityCheckPoint
			=> !IsInDatabase ? null : Factory.GetCached(ref deniedRateSecurityCheckPoint, GetDeniedRateSecurityCheckpoint);

		CachedProperty<SecurityCheckpoint> deniedRateSecurityCheckPoint;

		SecurityCheckpoint GetDeniedRateSecurityCheckpoint()
		{
			// a logic here implemented in a way to prevent unnecessary loading charge in a memory. This is important performance issue when post invoices for many jobs.

			var orgHeaderPKs = new HashSet<ZGuid>();
			if (LocalChargesAddr != null)
			{
				orgHeaderPKs.Add(LocalChargesAddr.OA_OH);
			}

			var chargeQuery = new ZQuery(JobChargeSchema.JR_JH, PK) { FetchOnlyFromLocalCache = true };
			var chargesInMemory = Factory.Load<JobCharge>(chargeQuery);
			foreach (var charge in chargesInMemory)
			{
				orgHeaderPKs.Add(charge.JR_OH_SellAccount);
				orgHeaderPKs.Add(charge.JR_OH_CostAccount);
			}

			if (IsInDatabase && !IsChargesCollectionLoaded)
			{
				var orgsByJobPK = GetOrgPKsFromChargesInDbGroupedByJob(Factory, PK);
				if (orgsByJobPK.TryGetValue(PK, out var orgsFromChargesInDB))
				{
					orgsFromChargesInDB.ForEach(x => orgHeaderPKs.Add(x));
				}
			}

			var result = RateSecurityHelper.GetFirstDeniedSecurityCheckPoint(orgHeaderPKs, Factory);
			return result == null || result.SecurityCheckPoint == null || result.SecurityCheckPoint.IsAllowed ? null : result.SecurityCheckPoint;
		}

		public static HashSet<ZGuid> GetOrgPKsFromChargesInDb(BusinessObjectFactory factory, params JobHeader[] jobs)
		{
			var jobsPK = jobs
				.Where(job => !job.IsChargesCollectionLoaded)
				.Select(job => job.PK)
				.Distinct()
				.ToArray();
			return GetOrgPKsFromChargesInDbGroupedByJob(factory, jobsPK)
				.SelectMany(kv => kv.Value)
				.ToHashSet();
		}

		static IReadOnlyDictionary<ZGuid, ZGuid[]> GetOrgPKsFromChargesInDbGroupedByJob(BusinessObjectFactory factory, params ZGuid[] jobPks)
		{
			if (!jobPks.Any())
			{
				return new Dictionary<ZGuid, ZGuid[]>();
			}
			var jobPKsHash = new HashSet<ZGuid>(jobPks);

			var cachedJobPKs = GetCachedOrgPKsFromChargesInDbGroupedByJob(factory);

			var notCachedJobPKs = jobPks.Except(cachedJobPKs.Keys).ToArray();
			var needToQueryDatabase = notCachedJobPKs.Any() || !factory.HasContext(BusinessContext.UseCacheToGetOrgsFromJobChargesInDb);
			if (needToQueryDatabase)
			{
				var chargeQuery = factory.HasContext(BusinessContext.UseCacheToGetOrgsFromJobChargesInDb)
					? new ZQuery(JobChargeSchema.JR_JH, notCachedJobPKs)
					: new ZQuery(JobChargeSchema.JR_JH, jobPKsHash);

				var chargesInDB = new DynamicBusinessObjectCollection(factory);
				chargesInDB.Load(string.Format(System.Globalization.CultureInfo.InvariantCulture, $@"
	SELECT
		{JobChargeSchema.Constants.JR_JH},
		{JobChargeSchema.Constants.JR_OH_SellAccount},
		{JobChargeSchema.Constants.JR_OH_CostAccount}
	FROM {JobChargeSchema.Constants.SqlSchemaName}.{JobChargeSchema.Constants.TableName}
	{chargeQuery.GetAsWhereClause(false)}
"), chargeQuery.Params);

				var orgPKsFromChargesInDb = chargesInDB
					.Select(row => (
						JR_JH: (ZGuid)row[JobChargeSchema.Constants.JR_JH],
						JR_OH: new ZGuid[] {
							(ZGuid)row[JobChargeSchema.Constants.JR_OH_SellAccount],
							(ZGuid)row[JobChargeSchema.Constants.JR_OH_CostAccount] }
						)
					)
					.GroupBy(row => row.JR_JH)
					.ToDictionary(
						group => group.Key,
						group => group.SelectMany(row => row.JR_OH.Where(x => x.IsValid)).Distinct().ToArray());

				orgPKsFromChargesInDb.ForEach(x => cachedJobPKs[x.Key] = x.Value);
				notCachedJobPKs.Except(orgPKsFromChargesInDb.Keys).ForEach(x => cachedJobPKs[x] = Array.Empty<ZGuid>());
#if DEBUG
				if (Globals.IsTest)
				{
					GetOrgPKsFromChargesInDbGroupedByJob_LoadCount_ForTestOnly++;
				}
#endif
			}
			return cachedJobPKs.Where(x => jobPKsHash.Contains(x.Key)).ToDictionary(x => x.Key, x => x.Value);
		}

#if DEBUG
		[ThreadStatic]
		internal static ZInt GetOrgPKsFromChargesInDbGroupedByJob_LoadCount_ForTestOnly;
#endif

		static Dictionary<ZGuid, ZGuid[]> GetCachedOrgPKsFromChargesInDbGroupedByJob(BusinessObjectFactory factory)
				=> factory.GetCachedValue($"{GlbCompany.CurrentCompany.PK.ToStringKey()}_OrgPKsFromChargesInDbGroupedByJob",
					() => new Dictionary<ZGuid, ZGuid[]>(),
					CacheStalenessPolicy.StaleOnFactorySave);

		#endregion

		#region CFX

		[DecimalPlaces(nameof(PercentageDecimals))]
		public override ZDecimal JH_AgentChargesCFX
		{
			get => base.JH_AgentChargesCFX;
			set => base.JH_AgentChargesCFX = value;
		}

		[DecimalPlaces(nameof(PercentageDecimals))]
		public override ZDecimal JH_LocalChargesCFX
		{
			get => base.JH_LocalChargesCFX;
			set => base.JH_LocalChargesCFX = value;
		}

		public int PercentageDecimals => Core.Constants.DecimalPlaces.DefaultNumberOfDecimalsForPercentages;

		#endregion

		#region Get Total Profit and Loss

		public virtual Money GetTotalProfitAndLossLocal(Func<JobCharge, bool> appliesToCharge)
		{
			return Money.Empty;
		}

		#endregion

		#region Gateway Billing

		public bool IsGatewayBillingJob(GlbCompany company = null) => IsGatewayLegacyJob || (JH_ParentTableCode == JobConsolSchema.Constants.Prefix && Parent.IsGatewayBillingEnabled(company));

		public bool IsGatewayLegacyJob
		{
			get
			{
				return JH_ParentTableCode == JobConsolSchema.Constants.Prefix && IsInDatabase &&
					JH_JobNum.EndsWith(Core.Constants.GatewaySuffixForJobHeaderDeprecated, StringComparison.OrdinalIgnoreCase);
			}
		}

		#endregion

		public override void OnSaved(bool saveSucceeded)
		{
			base.OnSaved(saveSucceeded);

			if (saveSucceeded)
			{
				if (IsGatewayBillingJob())
				{
					Env.Licence.GatewayBilling.Login(this);
				}
			}
		}

		#region ILicensedComponent Members

		public LicensedComponentManager LicensedComponentManager
		{
			get { return ((ILicensedComponent)this).LicensedComponentManager as LicensedComponentManager; }
		}

		IDisposable ILicensedComponent.LicensedComponentManager
		{
			get { return licensedComponentManager ?? (licensedComponentManager = new LicensedComponentManager(this)); }
		}
		LicensedComponentManager licensedComponentManager;

		#endregion

#if DEBUG

		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, PropertyDescriptor[] propertyPath)
		{
			if (JH_GC.IsEmpty)
			{
				JH_GC = GlbCompany.CurrentCompany.PK; //do this before base call to avoid creation of new GlbCompany
			}
			if (JH_GB.IsEmpty)
			{
				JH_GB = GlbBranch.CurrentBranch.PK;
			}
			if (JH_GE.IsEmpty)
			{
				JH_GE = GlbDepartment.CurrentDepartment.PK;
			}
			if (JH_ParentID.IsEmpty)
			{
				JH_ParentID = ZGuid.NewZGuid();
			}
			if (JH_ParentTableCode.IsEmpty)
			{
				JH_ParentTableCode = "Z0";
			}
			if (JH_Status.IsEmpty)
			{
				JH_Status = JobHeaderStatus.Working.Code;
			}
			base.FillWithValidTestDataCore(kind, propertyPath);
		}

		public virtual void LoadCharges_ForTestOnly()
		{
		}

#endif

		public bool IsManuallyCreated
		{
			get
			{
				return !IsInDatabase && isManuallyCreated; // Can be true only for Job not in database
			}
			set
			{
				if (value && !isManuallyCreated) // Once set it cannot be reset
				{
					isManuallyCreated = true;
					HasChanges = true;
				}
			}
		}
		bool isManuallyCreated;

		public bool IsReadyForCostPosting
		{
			get
			{
				return JH_Status == JobHeaderStatus.JobReadyForCostPosting.Code ||
					JH_Status == JobHeaderStatus.JobReadyForRevenueAndCostPosting.Code;
			}
		}

		public bool IsReadyForRevenuePosting
		{
			get
			{
				return JH_Status == JobHeaderStatus.JobReadyForRevenuePosting.Code ||
					JH_Status == JobHeaderStatus.JobReadyForRevenueAndCostPosting.Code;
			}
		}

		public bool IsClosed => !IsDeleted && JH_Status == JobHeaderStatus.Closed.Code;

		public bool IsReadyForFinancialClosure => JH_Status == JobHeaderStatus.JobReadyForFinancialClosure.Code;

		public bool IsReadyForFinancialClosureWithoutModifySecurity => CheckIsReadyForFinancialClosureWithoutModifySecurity();

		bool CheckIsReadyForFinancialClosureWithoutModifySecurity()
		{
			var result = false;
			bool isRunningFromServiceTask = !Env.Instance.ServiceTaskCode.IsNullOrEmpty();

			if (IsReadyForFinancialClosure && (isRunningFromServiceTask || !Env.Security.ModifyChargesforFinancialClosureJob.IsAllowed))
			{
				result = true;
			}

			return result;
		}

		public bool IsReadyForFinancialClosureWithoutPostSecurity => CheckIsReadyForFinancialClosureWithoutPostSecurity();

		bool CheckIsReadyForFinancialClosureWithoutPostSecurity()
		{
			var result = false;
			bool isRunningFromServiceTask = !Env.Instance.ServiceTaskCode.IsNullOrEmpty();

			if (IsReadyForFinancialClosure && (isRunningFromServiceTask || !Env.Security.AllowPostingChargesforFinancialClosureJob.IsAllowed))
			{
				result = true;
			}

			return result;
		}

		IJobHeader IHaveJobHeader.JobHeader => this;

		#region GetStrategies

		IBusinessObjectStrategy[] strategies;
		protected override IBusinessObjectStrategy[] GetStrategies()
		{
			if (strategies == null)
			{
				var baseStrategies = base.GetStrategies();
				var strategiesList = new List<IBusinessObjectStrategy>(baseStrategies);

				strategiesList.Add(new JobHeaderCommissionSourceMonitorStrategy(this));
				strategies = strategiesList.ToArray();
			}

			return strategies;
		}

		#endregion

		#region CommissionReversal

		public virtual bool HasNonReversedCommissionHeaders() { return false; }
		public virtual bool HasNonReversedCommissionHeaders(ZGuid orgPk) { return false; }
		public virtual void ReverseTransactionCommissions() { }

		protected ZGuid commissionsReversalOrgPk = ZGuid.Empty;
		public void SetOrgForCommissionsReversal(ZGuid orgPk)
		{
			commissionsReversalOrgPk = orgPk;
		}

		public static string GetCommissionReversalWarningMessage(ZPropertyInfo[] infos)
		{
			return Res.GetString(
				"BB2C9478-4992-4FEF-8563-2F2B29C86A44",
				"Commission calculations have already been processed for this job. Existing commission transactions will be reversed on Save. To avoid commission reversal, please restore original values for {0}. Commission for this job will only be reprocessed once the job is reopened and re-closed.",
				string.Join(", ", infos.Select(i => i.HumanReadableName)));
		}

		public static string CommissionReversalWarningMessageHeader
		{
			get { return Res.GetString("69295587-7416-429B-92F6-39B24A0D947D", "Reverse Existing Commissions"); }
		}

		#endregion

		#region Revenue Recognition Helper

		public virtual string GetRevenueRecognitionDetails(AccChargeCode chargeCode) => "";

		#endregion

		#region JobExchangeRates

		public virtual void UpdateExchangeRates(bool updateEmptyExchangeRates) { }

		public virtual bool HasEmptyExchangeRates()
		{
			return false;
		}

		#endregion

		#region Cross Trade Debtor Defaulting

		public static bool IsCrossTradeDebtorDefaultingFunctionalityEnabled() => AccountingMasterFilesRegistry.Instance.EnableCrossTradeDebtorDefaultingFunctionality.Value;

		public virtual CrossTradeDebtorDefaultingParam GetParamForCrossTradeDebtorDefaulting() => null;

		public virtual bool CanCrossTradeDebtorDefaultingBeApplied => false;

		#endregion
	}
}

#region Test
#if DEBUG

namespace Enterprise.MasterFiles.Business.Testing
{
	using System.Collections.Generic;

	#region Test Classes

	public class DummyJobHeader : JobHeader
	{
		public List<string> ProcessLogs;

		public DummyJobHeader(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{ }

		public override ZDecimal JH_TotalProfitRevenueMargin => throw new NotImplementedException();

		protected override void SetParentCore(IJobHeaderParent value)
		{
			if (ProcessLogs != null)
			{
				ProcessLogs.Add("SetParentCore");
			}
		}

		protected override bool IsChargesCollectionLoaded
		{
			get { return false; }
		}
	}

	#endregion
}

#endif
#endregion
