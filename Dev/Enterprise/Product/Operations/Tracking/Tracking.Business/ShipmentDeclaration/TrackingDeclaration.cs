using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Freight.Business;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Web.Business;
using Enterprise.ZArchitecture.Web.Shared;

namespace Enterprise.Tracking.Business
{
	public partial class TrackingDeclaration : IShipmentDeclaration, IUpdatableMilestoneEventsProvider, IMilestonesProvider, ITrackingEventsProvider
	{
		#region Overriden

		public override string TableName
		{
			get { return JobDeclarationSchema.Constants.TableName; }
		}

		#endregion

		#region IShipmentDeclaration Members

		#region TEUCount

		public ZDecimal TEUCount
		{
			get { return IBusinessObjectCollectionExtensions.ToArray<BaseCusContainer>(Declaration.CusContainers).Sum(c => c.Container?.RC_TEU ?? 0); }
		}
		public ZPropertyInfo TEUCountInfo
		{
			get { return GetZPropertyInfo(ShipmentDeclarationSchema.Constants.TEUCount); }
		}

		#endregion

		public ShipmentDeclarationSchema ShipmentDeclarationSchema
		{
			get { return schema ?? (schema = new ShipmentDeclarationSchema()); }
		}
		ShipmentDeclarationSchema schema;

		#region PersistentBizOPK

		public ZGuid PersistentBizOPK
		{
			get { return Declaration.PK; }
		}

		public ZPropertyInfo PersistentBizOPKInfo
		{
			get { return GetZPropertyInfo(ShipmentDeclarationSchema.Constants.PersistentBizOPK); }
		}

		#endregion

		#region Number

		public ZString Number
		{
			get { return Declaration.JE_DeclarationReference; }
		}
		public ZPropertyInfo NumberInfo
		{
			get { return GetZPropertyInfo(ShipmentDeclarationSchema.Constants.Number); }
		}

		#endregion

		#region HouseBill

		public ZString HouseBill
		{
			get { return Declaration.JE_HouseBill; }
		}
		public ZPropertyInfo HouseBillInfo
		{
			get { return GetZPropertyInfo(ShipmentDeclarationSchema.Constants.HouseBill); }
		}

		#endregion

		#region Consignor

		#region ConsignorPK

		public ZGuid ConsignorPK
		{
			get { return Declaration.JE_OH_Supplier; }
		}
		public ZPropertyInfo ConsignorPKInfo
		{
			get { return GetZPropertyInfo(ShipmentDeclarationSchema.Constants.ConsignorPK); }
		}

		#endregion

		#region ConsignorName

		public ZString ConsignorName
		{
			get { return Declaration.Consignor != null ? Declaration.Consignor.OH_FullNameTruncated : ZString.Empty; }
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
				WebAddressFormatter formatter = new WebAddressFormatter(ConsignorOrgAddress);
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
			get { return GetAddress1And2(ConsignorOrgAddress); }
		}

		public ZPropertyInfo ConsignorAddressInfo
		{
			get { return GetZPropertyInfo(ShipmentDeclarationSchema.Constants.ConsignorAddress); }
		}

		#endregion

		#region ConsignorCity

		public ZString ConsignorCity
		{
			get { return ConsignorOrgAddress != null ? ConsignorOrgAddress.OA_City : ZString.Empty; }
		}

		public ZPropertyInfo ConsignorCityInfo
		{
			get { return GetZPropertyInfo(ShipmentDeclarationSchema.Constants.ConsignorCity); }
		}

		#endregion

		#region ConsignorState

		public ZString ConsignorState
		{
			get { return ConsignorOrgAddress != null ? ConsignorOrgAddress.OA_State : ZString.Empty; }
		}

		public ZPropertyInfo ConsignorStateInfo
		{
			get { return GetZPropertyInfo(ShipmentDeclarationSchema.Constants.ConsignorState); }
		}

		#endregion

		#region ConsignorPostCode

		public ZString ConsignorPostCode
		{
			get { return ConsignorOrgAddress != null ? ConsignorOrgAddress.OA_PostCode : ZString.Empty; }
		}
		public ZPropertyInfo ConsignorPostCodeInfo
		{
			get { return GetZPropertyInfo(ShipmentDeclarationSchema.Constants.ConsignorPostCode); }
		}

		#endregion

		#endregion

		#region Consignee

		#region ConsigneePK

		public ZGuid ConsigneePK
		{
			get { return Declaration.JE_OH_Importer; }
		}

		public ZPropertyInfo ConsigneePKInfo
		{
			get { return GetZPropertyInfo(ShipmentDeclarationSchema.Constants.ConsigneePK); }
		}

		#endregion

		#region ConsigneeName

		public ZString ConsigneeName
		{
			get { return Declaration.Consignee != null ? Declaration.Consignee.OH_FullNameTruncated : ZString.Empty; }
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
				WebAddressFormatter formatter = new WebAddressFormatter(ConsigneeOrgAddress);
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
			get { return GetAddress1And2(ConsigneeOrgAddress); }
		}

		public ZPropertyInfo ConsigneeAddressInfo
		{
			get { return GetZPropertyInfo(ShipmentDeclarationSchema.Constants.ConsigneeAddress); }
		}

		#endregion

		#region ConsigneeCity

		public ZString ConsigneeCity
		{
			get { return ConsigneeOrgAddress != null ? ConsigneeOrgAddress.OA_City : ZString.Empty; }
		}

		public ZPropertyInfo ConsigneeCityInfo
		{
			get { return GetZPropertyInfo(ShipmentDeclarationSchema.Constants.ConsigneeCity); }
		}

		#endregion

		#region ConsigneeState

		public ZString ConsigneeState
		{
			get { return ConsigneeOrgAddress != null ? ConsigneeOrgAddress.OA_State : ZString.Empty; }
		}

		public ZPropertyInfo ConsigneeStateInfo
		{
			get { return GetZPropertyInfo(ShipmentDeclarationSchema.Constants.ConsigneeState); }
		}

		#endregion

		#region ConsigneePostCode

		public ZString ConsigneePostCode
		{
			get { return ConsigneeOrgAddress != null ? ConsigneeOrgAddress.OA_PostCode : ZString.Empty; }
		}

		public ZPropertyInfo ConsigneePostCodeInfo
		{
			get { return GetZPropertyInfo(ShipmentDeclarationSchema.Constants.ConsigneePostCode); }
		}

		#endregion

		#endregion

		#region Ports

		#region OriginPortCode

		public ZString OriginPortCode
		{
			get { return Declaration.JE_RL_NKOrigin; }
		}

		public ZPropertyInfo OriginPortCodeInfo
		{
			get { return GetZPropertyInfo(ShipmentDeclarationSchema.Constants.OriginPortCode); }
		}

		#endregion

		#region DestinationPortCode

		public ZString DestinationPortCode
		{
			get { return Declaration.JE_RL_NKFinalDestination; }
		}

		public ZPropertyInfo DestinationPortCodeInfo
		{
			get { return GetZPropertyInfo(ShipmentDeclarationSchema.Constants.DestinationPortCode); }
		}

		#endregion

		#region Main & Current LoadPort

		[RequiresSuppression]
		public ZString CurrentLoadPort
		{
			get { return Declaration.JE_RL_NKPortOfLoading; }
		}

		public ZPropertyInfo CurrentLoadPortInfo
		{
			get { return GetZPropertyInfo(ShipmentDeclarationSchema.Constants.CurrentLoadPort); }
		}

		[RequiresSuppression]
		public ZString MainLoadPort
		{
			get { return CurrentLoadPort; }
		}

		public ZPropertyInfo MainLoadPortInfo
		{
			get { return GetZPropertyInfo(ShipmentDeclarationSchema.Constants.MainLoadPort); }
		}

		#endregion

		#region Main & Current DischargePort

		[RequiresSuppression]
		public ZString CurrentDischargePort
		{
			get { return Declaration.JE_RL_NKPortOfArrival; }
		}

		public ZPropertyInfo CurrentDischargePortInfo
		{
			get { return GetZPropertyInfo(ShipmentDeclarationSchema.Constants.CurrentDischargePort); }
		}

		[RequiresSuppression]
		public ZString MainDischargePort
		{
			get { return CurrentDischargePort; }
		}

		public ZPropertyInfo MainDischargePortInfo
		{
			get { return GetZPropertyInfo(ShipmentDeclarationSchema.Constants.MainDischargePort); }
		}

		#endregion

		#endregion

		#region Suppressed Values

		#region ETDWithSuppression

		public ZDateTime ETDWithSuppression
		{
			get { return DateAtOriginWithSuppression; }
		}

		public ZPropertyInfo ETDWithSuppressionInfo
		{
			get { return GetZPropertyInfo(ShipmentDeclarationSchema.Constants.ETDWithSuppression); }
		}

		#endregion

		#region ETAWithSuppression

		public ZDateTime ETAWithSuppression
		{
			get { return DateOfArrivalWithSuppression; }
		}

		public ZPropertyInfo ETAWithSuppressionInfo
		{
			get { return GetZPropertyInfo(ShipmentDeclarationSchema.Constants.ETAWithSuppression); }
		}

		#endregion

		#region ETA

		public ZDateTime ETA
		{
			get { return Declaration.JE_DateAtFinalDestination; }
		}

		public ZPropertyInfo ETAInfo
		{
			get { return GetZPropertyInfo(ShipmentDeclarationSchema.Constants.ETA); }
		}

		#endregion

		#region Main & Current VoyageWithSuppression

		public ZString MainVoyageWithSuppression
		{
			get { return CurrentVoyageWithSuppression; }
		}

		public ZPropertyInfo MainVoyageWithSuppressionInfo
		{
			get { return GetZPropertyInfo(ShipmentDeclarationSchema.Constants.MainVoyageWithSuppression); }
		}

		public ZString CurrentVoyageWithSuppression
		{
			get { return (Declaration != null) ? Suppression.GetWebValue(Declaration.JE_VoyageFlightNo, Declaration, SuppressFields.FlightNumber) : ZString.Empty; }
		}

		public ZPropertyInfo CurrentVoyageWithSuppressionInfo
		{
			get { return GetZPropertyInfo(ShipmentDeclarationSchema.Constants.CurrentVoyageWithSuppression); }
		}

		#endregion

		#endregion

		#region Vessel

		public ZString MainVessel
		{
			get { return CurrentVessel; }
		}

		public ZPropertyInfo MainVesselInfo
		{
			get { return GetZPropertyInfo(ShipmentDeclarationSchema.Constants.MainVessel); }
		}

		public ZString CurrentVessel
		{
			get { return Declaration != null ? Declaration.JE_VesselName : ZString.Empty; }
		}

		public ZPropertyInfo CurrentVesselInfo
		{
			get { return GetZPropertyInfo(ShipmentDeclarationSchema.Constants.CurrentVessel); }
		}

		#endregion

		#region BookingReference

		public ZString BookingReference
		{
			get { return ZString.Empty; }
		}

		public ZPropertyInfo BookingReferenceInfo
		{
			get { return GetZPropertyInfo(ShipmentDeclarationSchema.Constants.BookingReference); }
		}

		#endregion

		#region OwnerReference

		public ZString OwnerReference
		{
			get { return Declaration.JE_OwnerRef; }
		}

		public ZPropertyInfo OwnerReferenceInfo
		{
			get { return GetZPropertyInfo(ShipmentDeclarationSchema.Constants.OwnerReference); }
		}

		#endregion

		#region TransportMode

		public ZString TransportMode
		{
			get { return Declaration.JE_TransportMode; }
		}

		public ZPropertyInfo TransportModeInfo
		{
			get { return GetZPropertyInfo(ShipmentDeclarationSchema.Constants.TransportMode); }
		}

		#endregion

		#region PacksWithUnits

		public ZString PacksWithUnits
		{
			get { return string.Format("{0} {1}", Declaration.JE_TotalNoOfPacks, Declaration.JE_TotalNoOfPacksPackType); }
		}

		public ZPropertyInfo PacksWithUnitsInfo
		{
			get { return GetZPropertyInfo(ShipmentDeclarationSchema.Constants.PacksWithUnits); }
		}

		#endregion

		#region VolumeWithUnits

		public ZString VolumeWithUnits
		{
			get { return string.Format("{0} {1}", Declaration.JE_TotalVolume, Declaration.JE_TotalVolumeUnit); }
		}

		public ZPropertyInfo VolumeWithUnitsInfo
		{
			get { return GetZPropertyInfo(ShipmentDeclarationSchema.Constants.VolumeWithUnits); }
		}

		#endregion

		#region WeightWithUnits

		public ZString WeightWithUnits
		{
			get { return string.Format("{0} {1}", Declaration.JE_TotalWeight, Declaration.JE_TotalWeightUnit); }
		}

		public ZPropertyInfo WeightWithUnitsInfo
		{
			get { return GetZPropertyInfo(ShipmentDeclarationSchema.Constants.WeightWithUnits); }
		}

		#endregion

		#region GoodsValue

		public ZDecimal GoodsValue
		{
			get { return ZDecimal.Zero; }
		}

		public ZPropertyInfo GoodsValueInfo
		{
			get { return GetZPropertyInfo(ShipmentDeclarationSchema.Constants.GoodsValue); }
		}

		#endregion

		#region GoodsValueCurrency

		public ZString GoodsValueCurrency
		{
			get { return ZString.Empty; }
		}

		public ZPropertyInfo GoodsValueCurrencyInfo
		{
			get { return GetZPropertyInfo(ShipmentDeclarationSchema.Constants.GoodsValueCurrency); }
		}

		#endregion

		#region GoodsDescription

		public ZString GoodsDescription
		{
			get { return Declaration.JE_GoodsDescription; }
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
			get { return Declaration.DocsAndCartage != null ? Declaration.DocsAndCartage.JP_EstimatedPickup : ZDateTime.Empty; }
		}

		public ZPropertyInfo EstimatedPickupDateInfo
		{
			get { return GetZPropertyInfo(ShipmentDeclarationSchema.Constants.EstimatedPickupDate); }
		}

		#endregion

		#region PickupDateRequiredBy

		public ZDateTime PickupDateRequiredBy
		{
			get { return Declaration.DocsAndCartage != null ? Declaration.DocsAndCartage.JP_PickupRequiredBy : ZDateTime.Empty; }
		}

		public ZPropertyInfo PickupDateRequiredByInfo
		{
			get { return GetZPropertyInfo(ShipmentDeclarationSchema.Constants.PickupDateRequiredBy); }
		}

		#endregion

		#region EstimatedDeliveryDate

		public ZDateTime EstimatedDeliveryDate
		{
			get { return Declaration.DocsAndCartage != null ? Declaration.DocsAndCartage.JP_EstimatedDelivery : ZDateTime.Empty; }
		}

		public ZPropertyInfo EstimatedDeliveryDateInfo
		{
			get { return GetZPropertyInfo(ShipmentDeclarationSchema.Constants.EstimatedDeliveryDate); }
		}

		#endregion

		#region DeliveryDateRequiredBy

		public ZDateTime DeliveryDateRequiredBy
		{
			get { return Declaration.DocsAndCartage != null ? Declaration.DocsAndCartage.JP_DeliveryRequiredBy : ZDateTime.Empty; }
		}

		public ZPropertyInfo DeliveryDateRequiredByInfo
		{
			get { return GetZPropertyInfo(ShipmentDeclarationSchema.Constants.DeliveryDateRequiredBy); }
		}

		#endregion

		#region DeliveryDate

		public ZDateTime DeliveryDate
		{
			get { return Declaration.DocsAndCartage != null ? Declaration.DocsAndCartage.JP_DeliveryCartageCompleted : ZDateTime.Empty; }
		}

		public ZPropertyInfo DeliveryDateInfo
		{
			get { return GetZPropertyInfo(ShipmentDeclarationSchema.Constants.DeliveryDate); }
		}

		#endregion

		#region ActualPickupDate

		public ZDateTime ActualPickupDate
		{
			get { return Declaration.DocsAndCartage != null ? Declaration.DocsAndCartage.JP_PickupCartageCompleted : ZDateTime.Empty; }
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
			get { return Declaration.JE_RS_NKServiceLevel; }
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

		#region ReceivedDate

		public ZDateTime ReceivedDate
		{
			get { return ZDateTime.Empty; }
		}

		public ZPropertyInfo ReceivedDateInfo
		{
			get { return GetZPropertyInfo(ShipmentDeclarationSchema.Constants.ReceivedDate); }
		}

		#endregion

		#region ReceivedBy

		public ZString ReceivedBy
		{
			get { return ZString.Empty; }
		}

		public ZPropertyInfo ReceivedByInfo
		{
			get { return GetZPropertyInfo(ShipmentDeclarationSchema.Constants.ReceivedBy); }
		}

		#endregion

		#region PiecesReceived

		public ZInt PiecesReceived
		{
			get { return ZInt.Zero; }
		}

		public ZPropertyInfo PiecesReceivedInfo
		{
			get { return GetZPropertyInfo(ShipmentDeclarationSchema.Constants.PiecesReceived); }
		}

		#endregion

		#region BookedOnline

		public ZBool BookedOnline
		{
			get { return false; }
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

				foreach (BaseCusContainer container in Declaration.CusContainers)
				{
					ZString code = container.CO_ContainerNumber;
					if (!code.IsEmpty && !containers.Contains(code))
					{
						containers.Add(code);
						if (containers.Count > 3)
						{
							containers[3] = "...";
							break;
						}
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
			get { return Declaration.JE_MessageType == "EXP" ? Declaration.JE_OH_Forwarder : ZGuid.Empty; }
		}

		public ZPropertyInfo SendingForwarderPKInfo
		{
			get { return GetZPropertyInfo(ShipmentDeclarationSchema.Constants.SendingForwarderPK); }
		}

		#endregion

		#region ReceivingForwarderPK

		public ZGuid ReceivingForwarderPK
		{
			get { return Declaration.JE_MessageType != "EXP" ? Declaration.JE_OH_Forwarder : ZGuid.Empty; }
		}

		public ZPropertyInfo ReceivingForwarderPKInfo
		{
			get { return GetZPropertyInfo(ShipmentDeclarationSchema.Constants.ReceivingForwarderPK); }
		}

		#endregion

		#region MasterBill

		public ZString MasterBill
		{
			get { return Declaration.JE_MasterBill; }
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

		[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "Follows existing implementation")]
		public BusinessObject[] BusinessObjectsWithRelatedEvents
		{
			get { return Declaration.BusinessObjectsWithRelatedEvents; }
		}

		public Logs Logs
		{
			get { return Declaration.Logs; }
		}

		public BusinessObjectFactory LogsFactory
		{
			get { return ((IStmALogParent)Declaration).LogsFactory; }
		}

		public ZGuid LogsParentPK
		{
			get { return ((IStmALogParent)Declaration).LogsParentPK; }
		}

		public string LogsParentTableName
		{
			get { return ((IStmALogParent)Declaration).LogsParentTableName; }
		}

		public StmALogCollection TrackingEvents
		{
			get { return this.GetTrackingEvents(SiteUser); }
		}

		public bool CanViewTrackingEvents
		{
			get { return SiteUser?.CanViewEvents ?? false; }
		}

		void IStmALogParent.ProcessLog(IStmALog log)
		{
		}

		bool IStmALogParent.DeferFiringWorkflow
		{
			get { return false; }
		}

		#endregion

		#region Milestones

		public void ReloadMilestones()
		{
			milestones = null;
		}

		public TrackingMilestoneCollection Milestones
		{
			get { return milestones ?? (milestones = (Declaration.Shipment != null) ? new TrackingMilestoneCollection(Declaration.Shipment, this, false) : new TrackingMilestoneCollection(Declaration, this, false)); }
		}
		TrackingMilestoneCollection milestones;

		public TrackingMilestoneCollection EditableMilestones
		{
			get { return editableMilestones ?? (editableMilestones = (Declaration.Shipment != null) ? new TrackingMilestoneCollection(Declaration.Shipment, this, true) : new TrackingMilestoneCollection(Declaration, this, true)); }
		}
		TrackingMilestoneCollection editableMilestones;

		#endregion

		#region IUpdatableMilestoneEventsProvider

		public List<string> UpdatableMilestoneEventCodes
		{
			get
			{
				WebPartyTypeOrgPairCollection webParties = new WebPartyTypeOrgPairCollection();
				webParties.Add(WebPartyType.Supplier, Declaration.Supplier);
				webParties.Add(WebPartyType.Forwarder, Declaration.Forwarder);
				webParties.Add(WebPartyType.Carrier, Declaration.ShippingLine);
				webParties.Add(WebPartyType.Importer, Declaration.Importer);
				if (Declaration is Customs.US.Business.JobDeclaration)
				{
					webParties.Add(WebPartyType.UltimateConsignee, ((Customs.US.Business.JobDeclaration)Declaration).ConsigneeOrgAddress);
					webParties.Add(WebPartyType.ExternalBroker, ((Customs.US.Business.JobDeclaration)Declaration).ExternalBroker);
				}
				return (new UpdateableMilestoneEventsHelper(SiteUser)).GetUpdateableMilestoneEvents(WebDataRegistry.Instance.DeclarationMilestoneEventUpdates.Value, webParties);
			}
		}

		#endregion

		public ZString ReleaseStatusDesc
		{
			get
			{
				if (Declaration is Customs.US.Business.JobDeclaration)
				{
					return ((Customs.US.Business.JobDeclaration)Declaration).ReleaseStatusDesc;
				}
				return Declaration.JE_EntryStatusDescription;
			}
		}

		public ZString FormattedEntryNumber
		{
			get
			{
				if (Declaration is Customs.US.Business.JobDeclaration)
				{
					var declaration = (Customs.US.Business.JobDeclaration)Declaration;
					return Customs.US.Business.CusEntryHeader.GetFormmattedFilerCodeAndEntryNumber(declaration.US_EntryFilerCode, declaration.DecEntryNumber);
				}
				return Declaration.DeclarationNumber;
			}
		}

		#region LoadingMeters

		public ZDecimal LoadingMeters
		{
			get { return Decimal.Zero; }
		}

		public ZPropertyInfo LoadingMetersInfo
		{
			get { return GetZPropertyInfo(ShipmentDeclarationSchema.Constants.LoadingMeters); }
		}

		#endregion

		#region ContainerMode

		public ZString ContainerMode
		{
			get { return Declaration.JE_ContainerMode; }
		}

		public ZPropertyInfo ContainerModeInfo
		{
			get { return GetZPropertyInfo(ShipmentDeclarationSchema.Constants.ContainerMode); }
		}

		#endregion

		#region ChargesApply

		public ZString ChargesApply
		{
			get { return ZString.Empty; }
		}

		public ZPropertyInfo ChargesApplyInfo
		{
			get { return GetZPropertyInfo(ShipmentDeclarationSchema.Constants.ChargesApply); }
		}

		public ICodeDescriptionPairList ChargesApply_List
		{
			get { return chargesApply_List ?? (chargesApply_List = new ReadOnlyCodeDescriptionPairList()); }
		}
		ICodeDescriptionPairList chargesApply_List;

		#endregion

		#region ReleaseType

		public ZString ReleaseType
		{
			get { return ZString.Empty; }
		}

		public ZPropertyInfo ReleaseTypeInfo
		{
			get { return GetZPropertyInfo(ShipmentDeclarationSchema.Constants.ReleaseType); }
		}

		public ICodeDescriptionPairList ReleaseType_List
		{
			get { return releaseType_List ?? (releaseType_List = new ReadOnlyCodeDescriptionPairList()); }
		}
		ICodeDescriptionPairList releaseType_List;

		#endregion

		#region OnBoard

		public ZString OnBoard
		{
			get { return ZString.Empty; }
		}

		public ZPropertyInfo OnBoardInfo
		{
			get { return GetZPropertyInfo(ShipmentDeclarationSchema.Constants.OnBoard); }
		}

		public ICodeDescriptionPairList OnBoard_List
		{
			get { return onBoard_List ?? (onBoard_List = new ReadOnlyCodeDescriptionPairList()); }
		}
		ICodeDescriptionPairList onBoard_List;

		#endregion

		#region AdditionalTerms

		public ZString AdditionalTerms
		{
			get { return ZString.Empty; }
		}

		public ZPropertyInfo AdditionalTermsInfo
		{
			get { return GetZPropertyInfo(ShipmentDeclarationSchema.Constants.AdditionalTerms); }
		}

		#endregion

		#region InspectionTypeCode

		public ZString InspectionTypeCode
		{
			get { return ZString.Empty; }
		}

		public ZPropertyInfo InspectionTypeCodeInfo
		{
			get { return GetZPropertyInfo(ShipmentDeclarationSchema.Constants.InspectionTypeCode); }
		}

		#endregion

		#region PaymentTerm

		public ZString PaymentTerm
		{
			get { return ZString.Empty; }
		}

		public ZPropertyInfo PaymentTermInfo
		{
			get { return GetZPropertyInfo(ShipmentDeclarationSchema.Constants.PaymentTerm); }
		}

		public ICodeDescriptionPairList PaymentTerm_List
		{
			get { return paymentTerm_List ?? (paymentTerm_List = new ReadOnlyCodeDescriptionPairList()); }
		}
		ICodeDescriptionPairList paymentTerm_List;

		#endregion

		#region PickupAgentFullName

		public ZString PickupAgentFullName
		{
			get { return ZString.Empty; }
		}

		public ZPropertyInfo PickupAgentFullNameInfo
		{
			get { return GetZPropertyInfo(ShipmentDeclarationSchema.Constants.PickupAgentFullName); }
		}

		#endregion

		#region DeliveryAgentFullName

		public ZString DeliveryAgentFullName
		{
			get { return ZString.Empty; }
		}

		public ZPropertyInfo DeliveryAgentFullNameInfo
		{
			get { return GetZPropertyInfo(ShipmentDeclarationSchema.Constants.DeliveryAgentFullName); }
		}

		#endregion

		#region DeclarationCountry

		public ZString DeclarationCountry
		{
			get
			{
				return Declaration?.Country.Description ?? ZString.Empty;
			}
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

		public ZDateTime FirstLegLoadETD => Declaration?.TransportsIncludingRelated.FirstLeg?.ETDWithSuppression ?? ZDateTime.Empty;

		public ZPropertyInfo FirstLegLoadETDInfo => GetZPropertyInfo(ShipmentDeclarationSchema.Constants.FirstLegLoadETD);

		public ZDateTime FirstLegLoadATD => Declaration?.TransportsIncludingRelated.FirstLeg?.ATDWithSuppression ?? ZDateTime.Empty;

		public ZPropertyInfo FirstLegLoadATDInfo => GetZPropertyInfo(ShipmentDeclarationSchema.Constants.FirstLegLoadATD);

		public ZDateTime LastLegDischargeETA => Declaration?.TransportsIncludingRelated.LastLeg?.ETAWithSuppression ?? ZDateTime.Empty;

		public ZPropertyInfo LastLegDischargeETAInfo => GetZPropertyInfo(ShipmentDeclarationSchema.Constants.LastLegDischargeETA);

		public ZDateTime LastLegDischargeATA => Declaration?.TransportsIncludingRelated.LastLeg?.ATAWithSuppression ?? ZDateTime.Empty;

		public ZPropertyInfo LastLegDischargeATAInfo => GetZPropertyInfo(ShipmentDeclarationSchema.Constants.LastLegDischargeATA);

		#endregion

		#endregion

		#region Implementation

		#region Address

		OrgAddress ConsignorOrgAddress
		{
			get { return Declaration.Consignor != null ? Declaration.Consignor.Addresses.DefaultAddressOfType(OrgAddressType.Office, false) : null; }
		}

		OrgAddress ConsigneeOrgAddress
		{
			get { return Declaration.Consignee != null ? Declaration.Consignee.Addresses.DefaultAddressOfType(OrgAddressType.Office, false) : null; }
		}

		ZString GetAddress1And2(OrgAddress address)
		{
			if (address != null)
			{
				return string.Format("{0}{1}",
					address.OA_Address1,
					!address.OA_Address2.IsEmpty ? ", " + address.OA_Address2 : "");
			}

			return ZString.Empty;
		}

		#endregion

		#endregion

		#region ICancellable

		public string CanCancel()
		{
			return Declaration.CanCancel();
		}

		public string CanReactivate()
		{
			return Declaration.CanReactivate();
		}

		public bool IsCancelled
		{
			get { return Declaration.IsCancelled; }
			set { Declaration.IsCancelled = value; }
		}

		public bool IsCancelledHasChanged
		{
			get { return Declaration.IsCancelledHasChanged; }
		}

		#endregion
	}
}
