using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Freight.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Web.Business;
using Enterprise.ZArchitecture.Web.Shared;
using Constants = Enterprise.Core.Constants;
using static Enterprise.Integration.Customs.CA;

namespace Enterprise.Tracking.Business
{
	public partial class TrackingShipment :
		IShipmentDeclaration,
		IMilestonesProvider,
		IUpdatableMilestoneEventsProvider,
		ITrackingEventsProvider
	{
		#region IShipmentDeclaration Members

		public ShipmentDeclarationSchema ShipmentDeclarationSchema
		{
			get { return schema ?? (schema = new ShipmentDeclarationSchema()); }
		}
		ShipmentDeclarationSchema schema;

		#region PersistentBizOPK

		public ZGuid PersistentBizOPK
		{
			get { return PK; }
		}

		public ZPropertyInfo PersistentBizOPKInfo
		{
			get { return GetZPropertyInfo(ShipmentDeclarationSchema.Constants.PersistentBizOPK); }
		}

		#endregion

		#region TEUCount

		public ZDecimal TEUCount
		{
			get { return JS_Calc_TEUCount; }
		}
		public ZPropertyInfo TEUCountInfo
		{
			get { return GetZPropertyInfo(ShipmentDeclarationSchema.Constants.TEUCount); }
		}

		#endregion

		#region Number

		public ZString Number
		{
			get { return JS_UniqueConsignRef; }
		}
		public ZPropertyInfo NumberInfo
		{
			get { return GetZPropertyInfo(ShipmentDeclarationSchema.Constants.Number); }
		}

		#endregion

		#region HouseBill

		public ZString HouseBill
		{
			get
			{
				if (CurrentTransport != null && CurrentTransport.JK_AgentType == Constants.AgentType.Direct)
				{
					return CurrentTransport.JK_MasterBillNum;
				}

				return JS_HouseBill;
			}
		}

		public ZPropertyInfo HouseBillInfo
		{
			get { return GetZPropertyInfo(ShipmentDeclarationSchema.Constants.HouseBill); }
		}

		#endregion

		#region Consignor

		#region ConsignorName

		public ZString ConsignorName
		{
			get { return Consignor != null ? Consignor.OH_FullNameTruncated : ConsignorDocumentaryAddress.E2_CompanyNameTruncated; }
		}

		public ZPropertyInfo ConsignorNameInfo
		{
			get { return GetZPropertyInfo(ShipmentDeclarationSchema.Constants.ConsignorName); }
		}

		#endregion

		#region ConsignorFullAddress

		public ZString ConsignorFullAddress
		{
			get
			{
				WebAddressFormatter formatter = new WebAddressFormatter(ConsignorDocumentaryAddress);
				return formatter.FormattedAddressWithCompanyName(ConsignorName);
			}
		}

		public ZPropertyInfo ConsignorFullAddressInfo
		{
			get { return GetZPropertyInfo(ShipmentDeclarationSchema.Constants.ConsignorFullAddress); }
		}

		#endregion

		#region ConsignorAddress

		public ZString ConsignorAddress
		{
			get { return GetAddress1And2(ConsignorDocumentaryAddress); }
		}

		public ZPropertyInfo ConsignorAddressInfo
		{
			get { return GetZPropertyInfo(ShipmentDeclarationSchema.Constants.ConsignorAddress); }
		}

		#endregion

		#region ConsignorCity

		public ZString ConsignorCity
		{
			get { return GetCity(ConsignorDocumentaryAddress); }
		}

		public ZPropertyInfo ConsignorCityInfo
		{
			get { return GetZPropertyInfo(ShipmentDeclarationSchema.Constants.ConsignorCity); }
		}

		#endregion

		#region ConsignorState

		public ZString ConsignorState
		{
			get { return GetState(ConsignorDocumentaryAddress); }
		}

		public ZPropertyInfo ConsignorStateInfo
		{
			get { return GetZPropertyInfo(ShipmentDeclarationSchema.Constants.ConsignorState); }
		}

		#endregion

		#region ConsignorPostCode

		public ZString ConsignorPostCode
		{
			get { return GetPostCode(ConsignorDocumentaryAddress); }
		}

		public ZPropertyInfo ConsignorPostCodeInfo
		{
			get { return GetZPropertyInfo(ShipmentDeclarationSchema.Constants.ConsignorPostCode); }
		}

		#endregion

		#endregion

		#region Consignee

		#region ConsigneeName

		public ZString ConsigneeName
		{
			get { return Consignee != null ? Consignee.OH_FullNameTruncated : ConsigneeDocumentaryAddress.E2_CompanyNameTruncated; }
		}

		public ZPropertyInfo ConsigneeNameInfo
		{
			get { return GetZPropertyInfo(ShipmentDeclarationSchema.Constants.ConsigneeName); }
		}

		#endregion

		#region ConsigneeFullAddress

		public ZString ConsigneeFullAddress
		{
			get
			{
				WebAddressFormatter formatter = new WebAddressFormatter(ConsigneeDocumentaryAddress);
				return formatter.FormattedAddressWithCompanyName(ConsigneeName);
			}
		}

		public ZPropertyInfo ConsigneeFullAddressInfo
		{
			get { return GetZPropertyInfo(ShipmentDeclarationSchema.Constants.ConsigneeFullAddress); }
		}

		#endregion

		#region ConsigneeAddress

		public ZString ConsigneeAddress
		{
			get { return GetAddress1And2(ConsigneeDocumentaryAddress); }
		}

		public ZPropertyInfo ConsigneeAddressInfo
		{
			get { return GetZPropertyInfo(ShipmentDeclarationSchema.Constants.ConsigneeAddress); }
		}

		#endregion

		#region ConsigneeCity

		public ZString ConsigneeCity
		{
			get { return GetCity(ConsigneeDocumentaryAddress); }
		}

		public ZPropertyInfo ConsigneeCityInfo
		{
			get { return GetZPropertyInfo(ShipmentDeclarationSchema.Constants.ConsigneeCity); }
		}

		#endregion

		#region ConsigneeState

		public ZString ConsigneeState
		{
			get { return GetState(ConsigneeDocumentaryAddress); }
		}

		public ZPropertyInfo ConsigneeStateInfo
		{
			get { return GetZPropertyInfo(ShipmentDeclarationSchema.Constants.ConsigneeState); }
		}

		#endregion

		#region ConsigneePostCode

		public ZString ConsigneePostCode
		{
			get { return GetPostCode(ConsigneeDocumentaryAddress); }
		}

		public ZPropertyInfo ConsigneePostCodeInfo
		{
			get { return GetZPropertyInfo(ShipmentDeclarationSchema.Constants.ConsigneePostCode); }
		}

		#endregion

		#endregion

		#region DeliverTo

		#region DeliverToName

		public ZString DeliverToName
		{
			get { return ConsigneeDeliveryAddress.E2_CompanyNameTruncated; }
		}

		#endregion

		#region DeliverToFullAddress

		public ZString DeliverToFullAddress
		{
			get
			{
				WebAddressFormatter formatter = new WebAddressFormatter(ConsigneeDeliveryAddress);
				return formatter.FormattedAddressWithCompanyName(DeliverToName);
			}
		}

		#endregion

		#endregion

		#region PickupFrom

		#region PickupFromName

		public ZString PickupFromName
		{
			get { return ConsignorPickupAddress.E2_CompanyNameTruncated; }
		}

		#endregion

		#region PickupFromFullAddress

		public ZString PickupFromFullAddress
		{
			get
			{
				WebAddressFormatter formatter = new WebAddressFormatter(ConsignorPickupAddress);
				return formatter.FormattedAddressWithCompanyName(PickupFromName);
			}
		}

		#endregion

		#endregion

		#region Ports

		#region Current & Main LoadPort

		[RequiresSuppression]
		public ZString CurrentLoadPort
		{
			get { return CurrentTransport != null ? CurrentTransport.JA_RL_NKPortOfLoading : ZString.Empty; }
		}

		public ZPropertyInfo CurrentLoadPortInfo
		{
			get { return GetZPropertyInfo(ShipmentDeclarationSchema.Constants.CurrentLoadPort); }
		}

		[RequiresSuppression]
		public ZString MainLoadPort
		{
			get { return MainTransport != null ? MainTransport.JA_RL_NKPortOfLoading : ZString.Empty; }
		}

		public ZPropertyInfo MainLoadPortInfo
		{
			get { return GetZPropertyInfo(ShipmentDeclarationSchema.Constants.MainLoadPort); }
		}

		#endregion

		#region Current & Main DischargePort

		[RequiresSuppression]
		public ZString CurrentDischargePort
		{
			get { return CurrentTransport != null ? CurrentTransport.JB_RL_NKPortOfDischarge : ZString.Empty; }
		}

		public ZPropertyInfo CurrentDischargePortInfo
		{
			get { return GetZPropertyInfo(ShipmentDeclarationSchema.Constants.CurrentDischargePort); }
		}

		[RequiresSuppression]
		public ZString MainDischargePort
		{
			get { return MainTransport != null ? MainTransport.JB_RL_NKPortOfDischarge : ZString.Empty; }
		}

		public ZPropertyInfo MainDischargePortInfo
		{
			get { return GetZPropertyInfo(ShipmentDeclarationSchema.Constants.MainDischargePort); }
		}

		#endregion

		#region OriginPortCode

		public ZString OriginPortCode
		{
			get { return JS_RL_NKOrigin; }
		}

		public ZPropertyInfo OriginPortCodeInfo
		{
			get { return GetZPropertyInfo(ShipmentDeclarationSchema.Constants.OriginPortCode); }
		}

		#endregion

		#region DestinationPortCode

		public ZString DestinationPortCode
		{
			get { return JS_RL_NKDestination; }
		}
		public ZPropertyInfo DestinationPortCodeInfo
		{
			get { return GetZPropertyInfo(ShipmentDeclarationSchema.Constants.DestinationPortCode); }
		}

		#endregion

		#endregion

		#region Current & Main Vessel

		public ZString CurrentVessel
		{
			get { return CurrentTransport != null ? CurrentTransport.JV_RV_NKVessel : ZString.Empty; }
		}

		public ZPropertyInfo CurrentVesselInfo
		{
			get { return GetZPropertyInfo(ShipmentDeclarationSchema.Constants.CurrentVessel); }
		}

		public ZString MainVessel
		{
			get { return MainTransport != null ? MainTransport.JV_RV_NKVessel : ZString.Empty; }
		}

		public ZPropertyInfo MainVesselInfo
		{
			get { return GetZPropertyInfo(ShipmentDeclarationSchema.Constants.MainVessel); }
		}

		#endregion

		#region Suppressed Values

		#region ETA

		public ZDateTime ETA
		{
			get { return JS_E_ARV; }
		}

		public ZPropertyInfo ETAInfo
		{
			get { return GetZPropertyInfo(ShipmentDeclarationSchema.Constants.ETA); }
		}

		#endregion

		#region VoyageWithSuppression

		public ZString CurrentVoyageWithSuppression
		{
			get { return CurrentTransport != null ? Suppression.GetWebValue(CurrentTransport.JV_VoyageFlight, this, SuppressFields.FlightNumber) : ZString.Empty; }
		}

		public ZPropertyInfo CurrentVoyageWithSuppressionInfo
		{
			get { return GetZPropertyInfo(ShipmentDeclarationSchema.Constants.CurrentVoyageWithSuppression); }
		}

		public ZString MainVoyageWithSuppression
		{
			get { return MainTransport != null ? Suppression.GetWebValue(MainTransport.JV_VoyageFlight, this, SuppressFields.FlightNumber) : ZString.Empty; }
		}

		public ZPropertyInfo MainVoyageWithSuppressionInfo
		{
			get { return GetZPropertyInfo(ShipmentDeclarationSchema.Constants.MainVoyageWithSuppression); }
		}

		#endregion

		#endregion

		#region BookingReference

		public ZString BookingReference
		{
			get { return JS_BookingReference; }
		}

		public ZPropertyInfo BookingReferenceInfo
		{
			get { return GetZPropertyInfo(ShipmentDeclarationSchema.Constants.BookingReference); }
		}

		#endregion

		#region OwnerReference

		public ZString OwnerReference
		{
			get
			{
				if (LastDeclaration != null)
				{
					return LastDeclaration.JE_OwnerRef;
				}
				else
				{
					return ZString.Empty;
				}
			}
		}

		public ZPropertyInfo OwnerReferenceInfo
		{
			get { return GetZPropertyInfo(ShipmentDeclarationSchema.Constants.OwnerReference); }
		}

		#endregion

		#region TransportMode

		// TransportMode property is already defined in one of the parent classes

		public ZPropertyInfo TransportModeInfo
		{
			get { return GetZPropertyInfo(ShipmentDeclarationSchema.Constants.TransportMode); }
		}

		#endregion

		#region PacksWithUnits

		public ZString PacksWithUnits
		{
			get { return string.Format("{0} {1}", JS_OuterPacks, JS_F3_NKPackType); }
		}

		public ZPropertyInfo PacksWithUnitsInfo
		{
			get { return GetZPropertyInfo(ShipmentDeclarationSchema.Constants.PacksWithUnits); }
		}

		#endregion

		#region VolumeWithUnits

		public ZString VolumeWithUnits
		{
			get
			{
				var roundedDecimal = this.GetRoundedValue(JobShipmentSchema.JS_ActualVolume, JS_ActualVolumeInfo, JS_ActualVolume);
				var formattedDecimal = FormatNumber(roundedDecimal, DefaultNumberOfDecimalsSupporterHelperForFreight.GetDefaultNumberOfDecimalsMetaDataProperty(this, JS_ActualVolumeInfo.PropertyDescriptor));
				return string.Format("{0} {1}", formattedDecimal, JS_UnitOfVolume);
			}
		}

		public ZPropertyInfo VolumeWithUnitsInfo
		{
			get { return GetZPropertyInfo(ShipmentDeclarationSchema.Constants.VolumeWithUnits); }
		}

		#endregion

		#region WeightWithUnits

		public ZString WeightWithUnits
		{
			get
			{
				var roundedDecimal = this.GetRoundedValue(JobShipmentSchema.JS_ActualWeight, JS_ActualWeightInfo, JS_ActualWeight);
				var formattedDecimal = FormatNumber(roundedDecimal, DefaultNumberOfDecimalsSupporterHelperForFreight.GetDefaultNumberOfDecimalsMetaDataProperty(this, JS_ActualWeightInfo.PropertyDescriptor));
				return string.Format("{0} {1}", formattedDecimal, JS_UnitOfWeight);
			}
		}

		protected string FormatNumber(ZDecimal number, int decimalsToShow)
		{
			return Utilities.FormatNumber(number, decimalsToShow, WebEnvShared.ClientCulture);
		}

		public ZPropertyInfo WeightWithUnitsInfo
		{
			get { return GetZPropertyInfo(ShipmentDeclarationSchema.Constants.WeightWithUnits); }
		}

		#endregion

		#region GoodsValue

		public ZDecimal GoodsValue
		{
			get { return JS_GoodsValue; }
		}

		public ZPropertyInfo GoodsValueInfo
		{
			get { return GetZPropertyInfo(ShipmentDeclarationSchema.Constants.GoodsValue); }
		}

		#endregion

		#region GoodsValueCurrency

		public ZString GoodsValueCurrency
		{
			get { return JS_RX_NKGoodsValueCurr; }
		}

		public ZPropertyInfo GoodsValueCurrencyInfo
		{
			get { return GetZPropertyInfo(ShipmentDeclarationSchema.Constants.GoodsValueCurrency); }
		}

		#endregion

		#region GoodsDescription

		public ZString GoodsDescription
		{
			get { return JS_GoodsDescription; }
		}

		public ZPropertyInfo GoodsDescriptionInfo
		{
			get { return GetZPropertyInfo(ShipmentDeclarationSchema.Constants.GoodsDescription); }
		}

		#endregion

		#region DocsAndCartage

		#region EstimatedPickupDate

		public ZDateTime EstimatedPickupDate
		{
			get { return IsDeleted ? ZDateTime.Empty : DocsAndCartage.JP_EstimatedPickup; }
		}

		public ZPropertyInfo EstimatedPickupDateInfo
		{
			get { return GetZPropertyInfo(ShipmentDeclarationSchema.Constants.EstimatedPickupDate); }
		}

		#endregion

		#region PickupDateRequiredBy

		public ZDateTime PickupDateRequiredBy
		{
			get { return IsDeleted ? ZDateTime.Empty : DocsAndCartage.JP_PickupRequiredBy; }
		}

		public ZPropertyInfo PickupDateRequiredByInfo
		{
			get { return GetZPropertyInfo(ShipmentDeclarationSchema.Constants.PickupDateRequiredBy); }
		}

		#endregion

		#region EstimatedDeliveryDate

		public ZDateTime EstimatedDeliveryDate
		{
			get { return IsDeleted ? ZDateTime.Empty : DocsAndCartage.JP_EstimatedDelivery; }
		}

		public ZPropertyInfo EstimatedDeliveryDateInfo
		{
			get { return GetZPropertyInfo(ShipmentDeclarationSchema.Constants.EstimatedDeliveryDate); }
		}

		#endregion

		#region DeliveryDateRequiredBy

		public ZDateTime DeliveryDateRequiredBy
		{
			get { return IsDeleted ? ZDateTime.Empty : DocsAndCartage.JP_DeliveryRequiredBy; }
		}

		public ZPropertyInfo DeliveryDateRequiredByInfo
		{
			get { return GetZPropertyInfo(ShipmentDeclarationSchema.Constants.DeliveryDateRequiredBy); }
		}

		#endregion

		#region DeliveryDate

		public ZDateTime DeliveryDate
		{
			get { return IsDeleted ? ZDateTime.Empty : DocsAndCartage.JP_DeliveryCartageCompleted; }
		}

		public ZPropertyInfo DeliveryDateInfo
		{
			get { return GetZPropertyInfo(ShipmentDeclarationSchema.Constants.DeliveryDate); }
		}

		#endregion

		#region ActualPickupDate

		public ZDateTime ActualPickupDate
		{
			get { return IsDeleted ? ZDateTime.Empty : DocsAndCartage.JP_PickupCartageCompleted; }
		}

		public ZPropertyInfo ActualPickupDateInfo
		{
			get { return GetZPropertyInfo(ShipmentDeclarationSchema.Constants.ActualPickupDate); }
		}

		#endregion

		#endregion

		#region ServiceLevelCode

		public ZString ServiceLevelCode
		{
			get { return JS_RS_NKServiceLevel; }
		}

		public ZPropertyInfo ServiceLevelCodeInfo
		{
			get { return GetZPropertyInfo(ShipmentDeclarationSchema.Constants.ServiceLevelCode); }
		}

		#endregion

		#region Charges

		public ZString Charges
		{
			get { return charges ?? (charges = InvoiceLoader.ChargesTotalsAsString); }
		}
		string charges;

		public ZPropertyInfo ChargesInfo
		{
			get { return GetZPropertyInfo(ShipmentDeclarationSchema.Constants.Charges); }
		}

		#endregion

		#region LastDeliveredLeg

		#region ReceivedDate

		public ZDateTime ReceivedDate
		{
			get { return LastDeliveredLeg != null ? LastDeliveredLeg.EU_PickupDeliveryTime : ZDateTime.Empty; }
		}

		public ZPropertyInfo ReceivedDateInfo
		{
			get { return GetZPropertyInfo(ShipmentDeclarationSchema.Constants.ReceivedDate); }
		}

		#endregion

		#region ReceivedBy

		public ZString ReceivedBy
		{
			get { return LastDeliveredLeg != null ? LastDeliveredLeg.EU_GoodsSignForBy : ZString.Empty; }
		}

		public ZPropertyInfo ReceivedByInfo
		{
			get { return GetZPropertyInfo(ShipmentDeclarationSchema.Constants.ReceivedBy); }
		}

		#endregion

		#region PiecesReceived

		public ZInt PiecesReceived
		{
			get { return LastDeliveredLeg != null ? LastDeliveredLeg.TotalDeliveredPackages : ZInt.Zero; }
		}

		public ZPropertyInfo PiecesReceivedInfo
		{
			get { return GetZPropertyInfo(ShipmentDeclarationSchema.Constants.PiecesReceived); }
		}

		#endregion

		#endregion

		#region BookedOnline

		public ZBool BookedOnline
		{
			get { return JS_SystemCreateUser == "ZZ"; }
		}

		public ZPropertyInfo BookedOnlineInfo
		{
			get { return GetZPropertyInfo(ShipmentDeclarationSchema.Constants.BookedOnline); }
		}

		#endregion

		#region Top3Containers

		public ZString Top3Containers
		{
			get
			{
				var containers = new List<ZString>();

				TrackingPackLineCollection lines = new TrackingPackLineCollection(this);
				lines.Load();
				foreach (TrackingPackLine line in lines)
				{
					foreach (CommonContainer container in line.Containers)
					{
						ZString code = container.ContainerCode;
						if (!code.IsEmpty && !containers.Contains(code))
						{
							containers.Add(code);
							if (containers.Count > 3)
							{
								break;
							}
						}
					}
					if (containers.Count > 3)
					{
						containers[3] = "...";
						break;
					}
				}

				ZStringBuilder result = new ZStringBuilder();

				foreach (ZString containerCode in containers)
				{
					result.Append(containerCode);
				}

				return result.ToStringWithDelimiterBetweenAppends(string.Format(", {0}", System.Environment.NewLine));
			}
		}

		public ZPropertyInfo Top3ContainersInfo
		{
			get { return GetZPropertyInfo(ShipmentDeclarationSchema.Constants.Top3Containers); }
		}

		#endregion

		#region SendingForwarderPK

		public ZGuid SendingForwarderPK
		{
			get
			{
				if (CurrentTransport != null)
				{
					OrgAddress sfAddress = Factory.Load<OrgAddress>(CurrentTransport.JK_OA_SendingForwarderAddress);
					if (sfAddress != null)
					{
						return sfAddress.OA_OH;
					}
				}

				return ZGuid.Empty;
			}
		}

		public ZPropertyInfo SendingForwarderPKInfo
		{
			get { return GetZPropertyInfo(ShipmentDeclarationSchema.Constants.SendingForwarderPK); }
		}

		#endregion

		#region ReceivingForwarderPK

		public ZGuid ReceivingForwarderPK
		{
			get
			{
				if (CurrentTransport != null)
				{
					OrgAddress rfAddress = Factory.Load<OrgAddress>(CurrentTransport.JK_OA_ReceivingForwarderAddress);
					if (rfAddress != null)
					{
						return rfAddress.OA_OH;
					}
				}

				return ZGuid.Empty;
			}
		}

		public ZPropertyInfo ReceivingForwarderPKInfo
		{
			get { return GetZPropertyInfo(ShipmentDeclarationSchema.Constants.ReceivingForwarderPK); }
		}

		#endregion

		#region MasterBill

		public ZString MasterBill
		{
			get { return CurrentTransport != null ? CurrentTransport.JK_MasterBillNum : ZString.Empty; }
		}

		public ZPropertyInfo MasterBillInfo
		{
			get { return GetZPropertyInfo(ShipmentDeclarationSchema.Constants.MasterBill); }
		}

		#endregion

		#region ShipmentDeclarationLookups

		public ShipmentDeclarationLookups ShipmentDeclarationLookups
		{
			get
			{
				if (lookups == null || !IsLookupsCachedInBase)
				{
					lookups = new ShipmentDeclarationLookups(this);
				}
				return lookups;
			}
		}
		ShipmentDeclarationLookups lookups;

		#endregion

		#region TrackingEvents

		public StmALogCollection TrackingEvents
		{
			get { return this.GetTrackingEvents(SiteUser); }
		}

		public bool CanViewTrackingEvents
		{
			get { return SiteUser?.CanViewEvents ?? false; }
		}

		#endregion

		#region Milestones

		public void ReloadMilestones()
		{
			milestones = null;
		}

		public TrackingMilestoneCollection Milestones
		{
			get { return milestones ?? (milestones = new TrackingMilestoneCollection(this)); }
		}
		TrackingMilestoneCollection milestones;

		public TrackingMilestoneCollection EditableMilestones
		{
			get { return editableMilestones ?? (editableMilestones = new TrackingMilestoneCollection(this, true)); }
		}
		TrackingMilestoneCollection editableMilestones;

		#endregion

		#region IUpdatableMilestoneEventsProvider

		public List<string> UpdatableMilestoneEventCodes
		{
			get
			{
				WebPartyTypeOrgPairCollection webParties = new WebPartyTypeOrgPairCollection();
				webParties = WebPartyProvider.GetWebParties(this);
				return (new UpdateableMilestoneEventsHelper(SiteUser)).GetUpdateableMilestoneEvents(WebDataRegistry.Instance.ShipmentMilestoneEventUpdates.Value, webParties);
			}
		}

		#endregion

		#region DeclarationCountry

		public ZString DeclarationCountry
		{
			get { return ZString.Empty; }
		}

		#endregion

		#region ReleaseStatusWrapper

		public IReleaseStatusWrapper ReleaseStatusWrapper
		{
			get
			{
				if (releaseStatusWrapper == null && GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Constants.CountryCodes.Canada)
				{
					var customsStatusProvider = (IForwardingShipmentCustomsStatusProvider)Activator.CreateInstance(ObjectFactory.GetType("CAForwardingShipmentCustomsStatusProvider"), this);
					releaseStatusWrapper = customsStatusProvider.GetReleaseStatusWrapper();
				}
				return releaseStatusWrapper;
			}
		}
		IReleaseStatusWrapper releaseStatusWrapper;

		#endregion

		#endregion

		#region Implementation

		#region Transport

		protected WebShipmentTransport MainTransport
		{
			get
			{
				foreach (WebShipmentTransport transport in ShipmentTransports)
				{
					if (transport.JW_TransportType == Constants.TransportPlanningType.MainVessel)
					{
						return transport;
					}
				}
				return null;
			}
		}

		protected WebShipmentTransport CurrentTransport
		{
			get
			{
				WebShipmentTransport result = null;
				foreach (WebShipmentTransport transport in ShipmentTransports)
				{
					if (!transport.JW_ATD.IsEmpty && transport.JW_ATA.IsEmpty)
					{
						return transport;
					}

					if (result == null || !transport.JW_ATA.IsEmpty)
					{
						result = transport;
					}
				}
				return result;
			}
		}

		protected WebShipmentTransportCollection ShipmentTransports
		{
			get
			{
				return shipmentTransports ?? (shipmentTransports = GetShipmentTransports());
			}
		}
		protected WebShipmentTransportCollection shipmentTransports;

		protected WebShipmentTransportCollection GetShipmentTransports()
		{
			WebShipmentTransportCollection collection = new WebShipmentTransportCollection(Factory, PK);
			collection.Load();
			return collection;
		}

		#endregion

		#region Address

		ZString GetAddress1And2(JobDocAddress jobDocAddress)
		{
			List<string> lines = new List<string>();

			if (jobDocAddress != null)
			{
				if (jobDocAddress.E2_AddressOverride)
				{
					lines.Add(jobDocAddress.E2_Address1);
					if (!jobDocAddress.E2_Address2.IsEmpty)
					{
						lines.Add(jobDocAddress.E2_Address2);
					}
				}
				else if (jobDocAddress.Address != null)
				{
					lines.Add(jobDocAddress.Address.OA_Address1);
					if (!jobDocAddress.Address.OA_Address2.IsEmpty)
					{
						lines.Add(jobDocAddress.Address.OA_Address2);
					}
				}
			}

			return string.Join(", ", lines.ToArray());
		}

		ZString GetCity(JobDocAddress jobDocAddress)
		{
			if (jobDocAddress != null)
			{
				if (jobDocAddress.E2_AddressOverride)
				{
					return jobDocAddress.E2_City;
				}
				else if (jobDocAddress.Address != null)
				{
					return jobDocAddress.Address.OA_City;
				}
			}
			return ZString.Empty;
		}

		ZString GetState(JobDocAddress jobDocAddress)
		{
			if (jobDocAddress != null)
			{
				if (jobDocAddress.E2_AddressOverride)
				{
					return jobDocAddress.E2_State;
				}
				else if (jobDocAddress.Address != null)
				{
					return jobDocAddress.Address.OA_State;
				}
			}
			return ZString.Empty;
		}

		ZString GetPostCode(JobDocAddress jobDocAddress)
		{
			if (jobDocAddress != null)
			{
				if (jobDocAddress.E2_AddressOverride)
				{
					return jobDocAddress.E2_Postcode;
				}
				else if (jobDocAddress.Address != null)
				{
					return jobDocAddress.Address.OA_PostCode;
				}
			}
			return ZString.Empty;
		}

		#endregion

		CommonPickupDeliveryConfirm LastDeliveredLeg
		{
			get
			{
				if (lastDeliveredLeg == null)
				{
					foreach (CommonPickupDeliveryConfirm leg in DeliveryConfirms)
					{
						if (lastDeliveredLeg == null || lastDeliveredLeg.EU_PickupDeliveryTime < leg.EU_PickupDeliveryTime)
						{
							lastDeliveredLeg = leg;
						}
					}
				}

				return lastDeliveredLeg;
			}
		}
		CommonPickupDeliveryConfirm lastDeliveredLeg;

		protected override Freight.Forwarding.Business.ForwardingShipmentProcessTaskCollection GetNewWorkflowItems()
		{
			return this.GetOrCreateProcessTaskCollection(() => new TrackingShipmentProcessTaskCollection(this));
		}

		public ZBool IsShippingBillOfLading
		{
			get { return JS_IsShipping && Regex.IsMatch(JS_ShipmentStatus, string.Format("^({0}|{1})$", ShipmentStatusList.Codes.WebFwdInstruction, ShipmentStatusList.Codes.Confirmed)); }
		}

		public ZBool IsShippingBooking
		{
			get { return JS_IsShipping && Regex.IsMatch(JS_ShipmentStatus, string.Format("^({0}|{1}|{2})$", ShipmentStatusList.Codes.WebBooking, ShipmentStatusList.Codes.Booked, ShipmentStatusList.Codes.WaitListed)); }
		}

		#region LoadingMeters

		public ZDecimal LoadingMeters
		{
			get { return JS_LoadingMeters; }
		}

		public ZPropertyInfo LoadingMetersInfo
		{
			get { return GetZPropertyInfo(ShipmentDeclarationSchema.Constants.LoadingMeters); }
		}

		#endregion

		#region ContainerMode

		public ZString ContainerMode
		{
			get { return JS_PackingMode; }
		}

		public ZPropertyInfo ContainerModeInfo
		{
			get { return GetZPropertyInfo(ShipmentDeclarationSchema.Constants.ContainerMode); }
		}

		#endregion

		#region ChargesApply

		public ZString ChargesApply
		{
			get { return JS_HBLAWBChargesDisplay; }
		}

		public ZPropertyInfo ChargesApplyInfo
		{
			get { return GetZPropertyInfo(ShipmentDeclarationSchema.Constants.ChargesApply); }
		}

		public ICodeDescriptionPairList ChargesApply_List
		{
			get { return Lookups.JS_HBLAWBChargesDisplay_List; }
		}

		#endregion

		#region ReleaseType

		public ZString ReleaseType
		{
			get { return JS_ReleaseType; }
		}

		public ZPropertyInfo ReleaseTypeInfo
		{
			get { return GetZPropertyInfo(ShipmentDeclarationSchema.Constants.ReleaseType); }
		}

		public ICodeDescriptionPairList ReleaseType_List
		{
			get { return Lookups.JS_ReleaseType_List; }
		}

		#endregion

		#region OnBoard

		public ZString OnBoard
		{
			get { return JS_ShippedOnBoard; }
		}

		public ZPropertyInfo OnBoardInfo
		{
			get { return GetZPropertyInfo(ShipmentDeclarationSchema.Constants.OnBoard); }
		}

		public ICodeDescriptionPairList OnBoard_List
		{
			get { return Lookups.JS_ShippedOnBoard_List; }
		}

		#endregion

		#region AdditionalTerms

		public ZString AdditionalTerms
		{
			get { return JS_AdditionalTerms; }
		}

		public ZPropertyInfo AdditionalTermsInfo
		{
			get { return GetZPropertyInfo(ShipmentDeclarationSchema.Constants.AdditionalTerms); }
		}

		#endregion

		#region InspectionTypeCode

		public ZString InspectionTypeCode
		{
			get { return JS_InspectionTypeCode; }
		}

		public ZPropertyInfo InspectionTypeCodeInfo
		{
			get { return GetZPropertyInfo(ShipmentDeclarationSchema.Constants.InspectionTypeCode); }
		}

		#endregion

		#region PaymentTerm

		public ZString PaymentTerm
		{
			get { return JS_INCO; }
		}

		public ZPropertyInfo PaymentTermInfo
		{
			get { return GetZPropertyInfo(ShipmentDeclarationSchema.Constants.PaymentTerm); }
		}

		public ICodeDescriptionPairList PaymentTerm_List
		{
			get { return Lookups.JS_INCO_List; }
		}

		#endregion

		#region DeliveryAgentFullName

		public ZString DeliveryAgentFullName
		{
			get { return DeliveryAgent != null ? DeliveryAgent.OH_FullNameTruncated : ZString.Empty; }
		}

		public ZPropertyInfo DeliveryAgentFullNameInfo
		{
			get { return GetZPropertyInfo(ShipmentDeclarationSchema.Constants.DeliveryAgentFullName); }
		}

		#endregion

		#region PickupAgentFullName

		public ZString PickupAgentFullName
		{
			get { return PickupAgent != null ? PickupAgent.OH_FullNameTruncated : ZString.Empty; }
		}

		public ZPropertyInfo PickupAgentFullNameInfo
		{
			get { return GetZPropertyInfo(ShipmentDeclarationSchema.Constants.PickupAgentFullName); }
		}

		#endregion

		#region Top3JobNotes

		public ZString Top3JobNotes => NotesHelper.Top3JobNotes;

		public ZPropertyInfo Top3JobNotesInfo
		{
			get { return GetZPropertyInfo(ShipmentDeclarationSchema.Constants.Top3JobNotes); }
		}

		#endregion

		#region First and Last Leg Dates

		public ZDateTime FirstLegLoadETD => FirstLoadConsol?.Transports.DepartureTransport?.ETDWithSuppression ?? ZDateTime.Empty;

		public ZPropertyInfo FirstLegLoadETDInfo => GetZPropertyInfo(ShipmentDeclarationSchema.Constants.FirstLegLoadETD);

		public ZDateTime FirstLegLoadATD => FirstLoadConsol?.Transports.DepartureTransport?.ATDWithSuppression ?? ZDateTime.Empty;

		public ZPropertyInfo FirstLegLoadATDInfo => GetZPropertyInfo(ShipmentDeclarationSchema.Constants.FirstLegLoadATD);

		public ZDateTime LastLegDischargeETA => LastDischargeConsol?.Transports.ArrivalTransport?.ETAWithSuppression ?? ZDateTime.Empty;

		public ZPropertyInfo LastLegDischargeETAInfo => GetZPropertyInfo(ShipmentDeclarationSchema.Constants.LastLegDischargeETA);

		public ZDateTime LastLegDischargeATA => LastDischargeConsol?.Transports.ArrivalTransport?.ATAWithSuppression ?? ZDateTime.Empty;

		public ZPropertyInfo LastLegDischargeATAInfo => GetZPropertyInfo(ShipmentDeclarationSchema.Constants.LastLegDischargeATA);

		#endregion

		#endregion
	}
}
