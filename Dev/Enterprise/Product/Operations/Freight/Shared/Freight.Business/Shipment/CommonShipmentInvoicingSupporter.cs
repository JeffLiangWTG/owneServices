using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Business
{
	public class CommonShipmentInvoicingSupporter : JobInvoicingSupporter, IImportExport
	{
		public CommonShipmentInvoicingSupporter(CommonShipment parent)
			: base(parent)
		{
			Argument.NotNull(parent, "parent");
			Shipment = parent;
		}

		#region Consol fields

		public override ZString ConsolType
		{
			get { return FirstConsol != null ? FirstConsol.JK_AgentType : (ZString)Constants.JobInvoicingDefaultDepartmentConsolType.NoConsol; }
		}

		public override ZDecimal ConsolExchangeRate
		{
			get { return FirstConsol != null ? FirstConsol.FreightCostsExchangeRate : 0; }
		}

		public override RefCurrency ConsolRateCurrency
		{
			get { return FirstConsol != null ? FirstConsol.FreightCostsCurrency : null; }
		}

		public override ZString ConsolNumber
		{
			get { return FirstConsol != null ? FirstConsol.JK_UniqueConsignRef : null; }
		}

		public override ZDecimal GetConsolExchangeRate(ZString currencyCode)
		{
			if (!currencyCode.IsEmpty && FirstConsol != null)
			{
				return FirstConsol.GetExchangeRateFromFreightCostsOrSchedule(currencyCode);
			}

			return base.GetConsolExchangeRate(currencyCode);
		}

		CommonConsol FirstConsol
		{
			get { return Shipment.Consols.Cast<CommonConsol>().FirstOrDefault(); }
		}

		public override SecurityCheckpoint ViewConsolCostFromOtherBRNorDEPSecurityCheckpoint
		{
			get { return Env.Security.MaintainConsolJobInvoicingAllowViewCosts; }
		}

		#endregion

		#region Organizations

		#region Sending / Receiving Agents

		public override OrgHeader SendingAgent
		{
			get
			{
				var consol = ConsolFallbackForAgents;
				if (consol != null && consol.JK_OA_SendingForwarderAddress.IsValid)
				{
					return consol.SendingForwarder;
				}

				return null;
			}
		}

		public override OrgHeader ReceivingAgent
		{
			get
			{
				OrgHeader result = Shipment.DeliveryAgent;
				if (result == null)
				{
					var consol = ConsolFallbackForAgents;
					if (consol != null)
					{
						result = consol.ReceivingForwarder;
					}
				}

				return result;
			}
		}

		public OrgHeader ConsolReceivingAgentWithNoShipmentDeliveryAgentFallback
		{
			get
			{
				OrgHeader result = null;
				var consol = ConsolFallbackForAgents;
				if (consol != null)
				{
					result = consol.ReceivingForwarder;
				}
				return result;
			}
		}

		CommonConsol ConsolFallbackForAgents
		{
			get
			{
				CommonConsol result = null;
				if (Shipment.Consols.Count > 0)
				{
					foreach (CommonConsol consol in Shipment.Consols)
					{
						if (!consol.IsDomestic())
						{
							result = consol;
							break;
						}
					}
					if (result == null)
					{
						result = Shipment.Consols[0];
					}
				}

				return result;
			}
		}

		#endregion

		public override OrgHeader Consignee
		{
			get { return Shipment.Consignee; }
		}

		public override OrgHeader Consignor
		{
			get { return Shipment.Consignor; }
		}

		public override OrgHeader PickUpAgent
		{
			get { return Shipment.PickupAgent; }
		}

		public override OrgHeader DeliveryAgent
		{
			get { return Shipment.DeliveryAgent; }
		}

		public override OrgHeader ImportBroker
		{
			get { return Shipment.ImportBroker; }
		}

		public override OrgHeader ExportBroker
		{
			get { return Shipment.ExportBroker; }
		}

		public override OrgHeader Broker
		{
			get { return IsImport ? Shipment.ImportBroker : (IsExport ? Shipment.ExportBroker : null); }
		}

		#endregion

		#region Measures

		public override ZDecimal ActualChargeable
		{
			get { return Shipment.JS_ActualChargeable; }
		}

		public override ZString ActualChargeableUnit
		{
			get { return Shipment.JS_ChargeableUnit; }
		}

		public override ZDecimal ActualWeight
		{
			get { return Shipment.JS_ActualWeight; }
		}

		public override ZString ActualWeightUnit
		{
			get { return Shipment.JS_UnitOfWeight; }
		}

		public override ZDecimal ActualVolume
		{
			get { return Shipment.JS_ActualVolume; }
		}

		public override ZString ActualVolumeUnit
		{
			get { return Shipment.JS_UnitOfVolume; }
		}

		public override ZDecimal ExcessActualVolumeWeight => Shipment.JS_Calc_ExcessActualVolumeWeight;

		public override ZDecimal ExcessChargeableVolumeWeight => Shipment.JS_Calc_ExcessChargeableVolumeWeight;

		public override ZDecimal ActualLoadingMeters
		{
			get { return Shipment.IsRoadLoadingMetersEnabled ? Shipment.JS_LoadingMeters : ZDecimal.Zero; }
		}

		public override int ContainerCount
		{
			get { return Shipment.JS_Calc_ContainerCount; }
		}

		public override ZDecimal TEUCount
		{
			get { return Shipment.ContainerTEUCount; }
		}

		public override int OuterPackTotal
		{
			get { return Shipment.TotalOuterPacks; }
		}

		#endregion

		#region Modes and Numbers

		public override ZString ServiceLevel => Shipment.JS_RS_NKServiceLevel;

		public override ZString TransportMode
		{
			get { return Shipment.JS_TransportMode; }
		}

		public override ZString ContainerMode
		{
			get { return Shipment.JS_PackingMode; }
		}

		public override PaymentTermInfos PaymentTerm
		{
			get { return Shipment.RatingAdapter.PaymentTerm; }
		}

		public override string GetPaymentTermForCreditorOverride(CostSell costOrSell)
		{
			if (Shipment.Consols.Any())
			{
				var paymentTerm = Shipment.Consols[0].JK_PrepaidCollect;
				if (Shipment.Consols.Cast<CommonConsol>().All(consol => consol.JK_PrepaidCollect == paymentTerm))
				{
					return paymentTerm;
				}
			}

			return null;
		}

		public override JobInvoicingConsumerType ConsumerType
		{
			get
			{
				JobInvoicingConsumerType result = null;
				if (!Shipment.IsDeleted)
				{
					result = Shipment.RatingAdapter.ConsumerType;
					if (!Shipment.JS_IsForwardRegistered)
					{
						if (Shipment.JS_IsBooking)
						{
							result = JobInvoicingConsumerTypes.QuotedBooking;
						}
						else if (Shipment.JS_IsCFSRegistered)
						{
							result = JobInvoicingConsumerTypes.CFSShipment; //Should be just Shipment.RatingAdapter.ConsumerType. Could not do so straight away because would have to fix too many tests
						}
					}
				}
				return result;
			}
		}

		public override ZString MasterBillNumber
		{
			get
			{
				CommonConsol consol = null;
				if (Shipment.Consols.Count == 1)
				{
					consol = Shipment.Consols[0];
				}
				else if (Shipment.Consols.Count > 1)
				{
					consol = Shipment.FindCorrectConsol();
				}

				return consol != null ? consol.JK_MasterBillNum : ZString.Empty;
			}
		}

		public override ZString HouseBillNumber
		{
			get { return Shipment.JS_HouseBill; }
		}

		public override ZString ShipmentNumberOfColoadMaster
		{
			get { return Shipment.CoLoadMasterShipment != null ? Shipment.CoLoadMasterShipment.JS_UniqueConsignRef : ZString.Empty; }
		}

		public override ZString CustomsEntryNumberType
		{
			get { return Shipment.CustomsEntryNumberType; }
		}

		public override ZString CommunityTransitStatus
		{
			get { return Shipment.JS_CommunityTransitStatus; }
		}

		#endregion

		#region Dates

		public override ZDateTime ATA
		{
			get { return Shipment.RatingAdapter.JobDatesProvider.GetJobDateByType(JobDateTypes.Codes.ArrivalDate); }
		}

		public override ZDateTime ATD
		{
			get { return Shipment.RatingAdapter.JobDatesProvider.GetJobDateByType(JobDateTypes.Codes.DepartureDate); }
		}

		public override ZDateTime ETA
		{
			get { return Shipment.JS_E_ARV; }
		}

		public override ZDateTime ETD
		{
			get { return Shipment.JS_E_DEP; }
		}

		public override ZDateTime ESP
		{
			get { return Shipment.DocsAndCartage.JP_EstimatedPickup; }
		}

		public override ZDateTime ESD
		{
			get { return Shipment.DocsAndCartage.JP_EstimatedDelivery; }
		}

		public sealed override ZDateTime ArrivalAtLoadPort
		{
			get
			{
				var result = ZDateTime.Empty;

				var sailing = Sailing;
				if (sailing != null)
				{
					if (sailing.Origin.JA_A_ARV.IsValid)
					{
						result = sailing.Origin.JA_A_ARV;
					}
					else if (sailing.Origin.JA_E_ARV.IsValid)
					{
						result = sailing.Origin.JA_E_ARV;
					}
				}

				return result;
			}
		}

		public sealed override ZDateTime EstimatedArrivalAtLoadPort
		{
			get
			{
				var result = ZDateTime.Empty;

				var sailing = Sailing;
				if (sailing != null && sailing.Origin.JA_E_ARV.IsValid)
				{
					result = sailing.Origin.JA_E_ARV;
				}

				return result;
			}
		}

		public override ZDateTime ActualPickupDate
		{
			get { return Shipment.DocsAndCartage.JP_PickupCartageCompleted; }
		}

		public override ZDateTime ActualDeliveryDate
		{
			get { return Shipment.DocsAndCartage.JP_DeliveryCartageCompleted; }
		}

		#endregion

		#region Sailing Schedule

		protected virtual JobSailing Sailing
		{
			get
			{
				Transport transport = null;

				var consolLoadedAtShipmentOrigin = Shipment.Consols.Cast<CommonConsol>().FirstOrDefault(c => c.Transports.DepartureTransport != null && c.Transports.DepartureTransport.JW_RL_NKLoadPort == Shipment.JS_RL_NKOrigin);

				if (consolLoadedAtShipmentOrigin != null)
				{
					transport = consolLoadedAtShipmentOrigin.Transports.DepartureTransport;
				}

				if (transport == null && Shipment.DepartureConsol != null)
				{
					transport = Shipment.DepartureConsol.Transports.FindTransportByLoadPort(Shipment.DepartureConsol.JK_RL_NKLoadPort) ?? Shipment.DepartureConsol.Transports.DepartureTransport;
				}

				return transport != null && transport.JW_IsLinked ? transport.Sailing : null;
			}
		}

		#endregion

		#region Locations

		public override bool IsDirectShipment
		{
			get { return Shipment.IsDirectShipment; }
		}

		public override RefUNLOCO Origin
		{
			get
			{
				RefUNLOCO result = Shipment.Origin;
				if (result == null)
				{
					CommonConsol consol = Shipment.FindCorrectConsol();
					if (consol != null)
					{
						result = consol.LoadPort;
					}
				}

				return result;
			}
		}

		public override RefUNLOCO Destination
		{
			get
			{
				RefUNLOCO result = Shipment.Destination;
				if (result == null)
				{
					CommonConsol consol = Shipment.FindCorrectConsol();
					if (consol != null)
					{
						result = consol.DischargePort;
					}
				}

				return result;
			}
		}

		public override RefUNLOCO GetTranshipmentPort(CostSell costOrSell) =>
			Shipment.RatingAdapter.GetVia(costOrSell)?.UNLOCO;

		public override bool IsImport
		{
			get
			{
				ZString origin = Origin != null ? Origin.Code : ZString.Empty;
				ZString destination = Destination != null ? Destination.Code : ZString.Empty;
				return ImportExportHelper.IsImport(origin, destination);
			}
		}

		public override bool IsExport
		{
			get
			{
				ZString origin = Origin != null ? Origin.Code : ZString.Empty;
				ZString destination = Destination != null ? Destination.Code : ZString.Empty;
				return ImportExportHelper.IsExport(origin, destination);
			}
		}

		#endregion

		#region Security

		protected override SecurityCheckpoint GetAuditSecurityCore()
		{
			return null;
		}

		protected override SecurityCheckpoint GetJobInvoicingSecurityCore()
		{
			return ConsumerType == JobInvoicingConsumerTypes.CFSShipment ? Env.Security.CFSShipmentJobInvoicing : Env.Security.MaintainShipmentJobInvoicing;
		}

		public override ZString EditSecurityMessage
		{
			get { return EditSecurityMessageCore; }
		}

		protected virtual ZString EditSecurityMessageCore
		{
			get { return ZString.Empty; }
		}

		public override bool EditSecurityLock
		{
			get { return EditSecurityLockCore; }
		}

		protected virtual bool EditSecurityLockCore
		{
			get { return false; }
		}

		#endregion

		#region Accounting related / Miscellaneous

		public override bool CreateAccountingJobOnSavingOfOperationsJob
		{
			get
			{
				if (ConsumerType == JobInvoicingConsumerTypes.Shipment)
				{
					var isDebtor = false;

					if (Consignee != null && Consignee.GetFreightBillTo(true, TransportMode, ContainerMode) != null && Shipment.IsCrossTrade())
					{
						isDebtor = Consignee.GetFreightBillTo(true, TransportMode, ContainerMode).OH_IsDebtor || Consignee.OH_IsDebtor;
					}

					var isLocal = Origin != null && ImportExportHelper.IsBranchCountry(Origin.Code);

					var isAttachedToAnyExportOrImportConsol = false;
					var consol = Shipment.GetFirstOrCorrectConsol();
					if (consol != null && (consol.IsExport() || consol.IsImport()))
					{
						isAttachedToAnyExportOrImportConsol = true;
					}

					return (isLocal && this.IsDomestic()) || this.IsImport() || this.IsExport() || isDebtor || isAttachedToAnyExportOrImportConsol;
				}

				return false;
			}
		}

		public override ZDateTime GetOperationsSignificantDate(string significantDateCode)
		{
			return Shipment.RatingAdapter.JobDatesProvider.GetJobDateByType(significantDateCode);
		}

		public override ZDateTime GetOperationsSignificantDateByDirection(string significantDateCode, string direction)
		{
			return Shipment.RatingAdapter.JobDatesProvider.GetJobDateByType(significantDateCode, direction);
		}

		public override ZDateTime GetCustomsClearanceDate()
		{
			ZDateTime customsClearanceDate = ZDateTime.Empty;
			StmALog mostRecentClearedLog = Shipment.Logs.MostRecentLogByEventTime(Events.CustomsCleared, new ZQuery(StmALogSchema.SL_IsEstimate, false))
										   ?? Shipment.Logs.MostRecentLogByEventTime(Events.ExportCustomsCleared, new ZQuery(StmALogSchema.SL_IsEstimate, false));
			if (mostRecentClearedLog == null)
			{
				mostRecentClearedLog = Shipment.Logs.MostRecentLogByEventTime(Events.CustomsCleared, new ZQuery(StmALogSchema.SL_IsEstimate, true));
				if (mostRecentClearedLog == null)
				{
					mostRecentClearedLog = Shipment.Logs.MostRecentLogByEventTime(Events.ExportCustomsCleared, new ZQuery(StmALogSchema.SL_IsEstimate, true));
				}
			}

			if (mostRecentClearedLog != null)
			{
				customsClearanceDate = mostRecentClearedLog.SL_EventTime;
			}
			return customsClearanceDate;
		}

		#region VoyageVesselOrFlightDate

		public override ZString VoyageVesselOrFlightDate
		{
			get
			{
				var result = string.Empty;
				if (Shipment != null)
				{
					if (ConsumerType == JobInvoicingConsumerTypes.AgencyBillOfLading || ConsumerType == JobInvoicingConsumerTypes.AgencyBooking)
					{
						var vesselOrFlightDate = Shipment.Sailing != null ? Shipment.Sailing.JX_JV_NKVessel : ZString.Empty;
						var voyageFlight = Shipment.Sailing != null ? Shipment.Sailing.JX_JV_VoyageFlight : ZString.Empty;

						result = GetVoyageVesselOrFlightDatesCore(Enterprise.Core.Constants.TransportModes.Sea, ZDateTime.Empty, vesselOrFlightDate, voyageFlight);
					}
					else if (Shipment.Consols != null && Shipment.Consols.Count > 0)
					{
						foreach (CommonConsol consol in Shipment.Consols)
						{
							var departureDate = ZDateTime.Empty;
							if (!consol.JK_JX_JA_A_DEP.IsEmpty)
							{
								departureDate = consol.JK_JX_JA_A_DEP;
							}
							else if (!consol.JK_JX_JA_E_DEP.IsEmpty)
							{
								departureDate = consol.JK_JX_JA_E_DEP;
							}
							else if (Shipment != null)
							{
								departureDate = Shipment.JS_E_DEP;
							}

							result += GetVoyageVesselOrFlightDatesCore(consol.JK_TransportMode, departureDate, consol.JK_JX_JV_NKVessel, consol.JK_JX_JV_VoyageFlight) + ", ";
						}
						result = result.TrimEnd(", ".ToCharArray());
					}
				}
				return result;
			}
		}

		#endregion

		#endregion

		public override OrgHeader GetOrganisationByBranchDefaultingRule(ZString defaultingRule)
		{
			OrgHeader resultOrganisation = base.GetOrganisationByBranchDefaultingRule(defaultingRule);
			if (resultOrganisation == null)
			{
				switch (defaultingRule)
				{
					case Constants.ChargeCodeBranchDefaultingRule.ArrivalCTO:
						{
							var consol = ConsolFallbackForAgents;
							resultOrganisation = consol == null ? null : consol.ArrivalCTOAddress?.Header;
						}
						break;
					case Constants.ChargeCodeBranchDefaultingRule.DepartureCTO:
						{
							var consol = ConsolFallbackForAgents;
							resultOrganisation = consol == null ? null : consol.DepartureCTOAddress?.Header;
						}
						break;
					case Constants.ChargeCodeBranchDefaultingRule.ConsolArrivalLocalTransport:
						{
							var consol = ConsolFallbackForAgents;
							resultOrganisation = consol == null ? null : consol.ArrivalUnpackCFSTransport;
						}
						break;
					case Constants.ChargeCodeBranchDefaultingRule.ConsolDepartureLocalTransport:
						{
							var consol = ConsolFallbackForAgents;
							resultOrganisation = consol == null ? null : consol.DeparturePackCFSTransport;
						}
						break;
					case Constants.ChargeCodeBranchDefaultingRule.ReceivingAgent:
						resultOrganisation = ConsolReceivingAgentWithNoShipmentDeliveryAgentFallback;
						break;
					case Constants.ChargeCodeBranchDefaultingRule.DeliveryAgentWithReceivingAgentFallback:
						resultOrganisation = ReceivingAgent;
						break;
					case Constants.ChargeCodeBranchDefaultingRule.SendingAgent:
						resultOrganisation = SendingAgent;
						break;
					case Constants.ChargeCodeBranchDefaultingRule.ShipmentDeliveryLocalTransportCompany:
						resultOrganisation = Shipment.DocsAndCartage.DeliveryCartageCo;
						break;
					case Constants.ChargeCodeBranchDefaultingRule.ShipmentPickupLocalTransportCompany:
						resultOrganisation = Shipment.DocsAndCartage.PickupCartageCo;
						break;
					case Constants.ChargeCodeBranchDefaultingRule.ShipmentExportBroker:
						resultOrganisation = Shipment.ExportBroker;
						break;
					case Constants.ChargeCodeBranchDefaultingRule.ShipmentImportBroker:
						resultOrganisation = Shipment.ImportBroker;
						break;
				}
			}

			return resultOrganisation;
		}

		#region Implementation

		public CommonShipment Shipment { get; private set; }

		public Directions JobDirection
		{
			get
			{
				var origin = Origin != null ? Origin.Code : ZString.Empty;
				var destination = Destination != null ? Destination.Code : ZString.Empty;
				return ImportExportHelper.GetJobDirection(origin, destination);
			}
		}

		#endregion // Implementation
	}
}
