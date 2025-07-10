using CargoWise.Types;
using Enterprise.DataTransfer.Xml.XsdVersion1;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Forwarding.DataTransfer
{
	public class SterlingShipment : SterlingRecord
	{
		public SterlingShipment(SterlingCommerceConsolAndShipmentExporter master)
			: base(master)
		{
			UpdateFields();
		}

		#region Record

		#region Header

		public override ZString RecordHeader
		{
			get
			{
				return "SHP";
			}
		}

		#endregion

		#region Generate

		public override void GenerateRecord()
		{
			AddField(PRONUM);
			AddField(SHIPIDNUM);
			AddField(DateCreated);
			AddField(TransportMode);
			AddField(PackingMode);
			AddField(PortOfOrigin);
			AddField(PortOfOriginCountry);
			AddField(PortOfOriginCity);
			AddField(PortofDestination);
			AddField(PortofDestinationCountry);
			AddField(PortofDestinationCity);
			AddField(ShipmentStatus);
			AddField(TotalInnerPacksQty);
			AddField(TotalInnerPacksQtyDimensionType);
			AddField(TotalOuterPacksQty);
			AddField(TotalOuterPacksQtyDimensionType);
			AddField(GoodsDescription);
			AddField(Weight);
			AddField(WeightDimensionType);
			AddField(ChargeableWeight);
			AddField(ChargeableWeightDimensionType);
			AddField(Volume);
			AddField(VolumeDimensionValue);
			AddField(GoodsValue);
			AddField(GoodsValueCurrencyCode);
			AddField(ServiceLevel);
			AddField(Incoterm);
			AddField(ReleaseType);
			AddField(AgentReference);
			AddField(ShippedOnBoardType);
			AddField(MarksAndNumbers);
			AddField(OwnerReference);
			AddField(BookingReference);
			AddField(SCAC);
			AddField(DeliveryFrom);
			AddField(DeliveryRequiredBy);
			AddField(DeliveryCartageAdvised);
			AddField(GoodsDelivered);
			AddField(PickupFrom);
			AddField(PickupRequiredBy);
			AddField(PickupCartageAdvised);
			AddField(GoodsPickup);
			AddField(EstimatedDateTimeDeparture);
			AddField(EstimatedDateTimeArrival);
			AddField(HBLIssueDate);
			TerminateRecord();
		}

		#endregion

		#endregion

		#region UpdateFields

		public void UpdateFields()
		{
			fPRONUM = Master.Consol == null ? ZString.Empty : Master.Consol.Masterbill;
			fSHIPIDNUM = Master.Shipment.Housebill;
			fDateCreated = Master.Shipment.ShipmentDetails.DateCreated;
			fTransportMode = Master.Shipment.ShipmentDetails.TransportMode.ToString();
			fPackingMode = Master.Shipment.ShipmentDetails.PackingMode.ToString();
			fPortOfOrigin = Master.Shipment.ShipmentDetails.PortOfOrigin.Port.Value;
			fPortOfOriginCountry = Master.Shipment.ShipmentDetails.PortOfOrigin.Port.Country;
			fPortOfOriginCity = Master.Shipment.ShipmentDetails.PortOfOrigin.Port.City;
			fPortofDestination = Master.Shipment.ShipmentDetails.PortofDestination.Port.Value;
			fPortofDestinationCountry = Master.Shipment.ShipmentDetails.PortofDestination.Port.Country;
			fPortofDestinationCity = Master.Shipment.ShipmentDetails.PortofDestination.Port.City;
			fShipmentStatus = Master.Shipment.ShipmentDetails.ShipmentStatus;
			fTotalInnerPacksQty = Master.Shipment.ShipmentDetails.TotalInnerPacksQty.Value;
			fTotalInnerPacksQtyDimensionType = Master.Shipment.ShipmentDetails.TotalInnerPacksQty.DimensionType;
			fTotalOuterPacksQty = Master.Shipment.ShipmentDetails.TotalOuterPacksQty.Value;
			fTotalOuterPacksQtyDimensionType = Master.Shipment.ShipmentDetails.TotalOuterPacksQty.DimensionType;
			fGoodsDescription = Master.Shipment.ShipmentDetails.GoodsDescription;
			fWeight = Master.Shipment.ShipmentDetails.Weight.Value;
			fWeightDimensionType = Master.Shipment.ShipmentDetails.Weight.DimensionType;
			fChargeableWeight = Master.Shipment.ShipmentDetails.ChargeableWeight.Value;
			fChargeableWeightDimensionType = Master.Shipment.ShipmentDetails.ChargeableWeight.DimensionType;
			fVolume = Master.Shipment.ShipmentDetails.Volume.Value;
			fVolumeDimensionValue = Master.Shipment.ShipmentDetails.Volume.DimensionType;
			fGoodsValue = Master.Shipment.ShipmentDetails.GoodsValue.Value;
			fGoodsValueCurrencyCode = Master.Shipment.ShipmentDetails.GoodsValue.CurrencyCode;
			fServiceLevel = Master.Shipment.ShipmentDetails.ServiceLevel;
			fIncoterm = Master.Shipment.ShipmentDetails.Incoterm;
			fReleaseType = Master.Shipment.ShipmentDetails.ReleaseType.ToString();
			fAgentReference = Master.Shipment.ShipmentDetails.AgentReference;
			fShippedOnBoardType = Master.Shipment.ShipmentDetails.ShippedOnBoardType.ToString();
			fMarksAndNumbers = Master.Shipment.ShipmentDetails.MarksAndNumbers;
			fOwnerReference = Master.Shipment.ShipmentDetails.OwnerReference;
			fBookingReference = Master.Shipment.ShipmentDetails.BookingReference;
			Enterprise.DataTransfer.Xml.XsdVersion1.RegistrationNumber regNum = Master.Interchange.EDIOrganisation.OrganisationDetails.RegistrationNumbers.FindRegistrationNumber(RegistrationNumberTypes.CCC, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
			if (regNum != null)
			{
				fSCAC = regNum.Number;
			}
			fDeliveryFrom = Master.Shipment.ShipmentDetails.Deliver.DeliveryFrom;
			fDeliveryRequiredBy = Master.Shipment.ShipmentDetails.Deliver.DeliveryRequiredBy;
			fDeliveryCartageAdvised = Master.Shipment.ShipmentDetails.Deliver.CartageAdvised;
			fGoodsDelivered = Master.Shipment.ShipmentDetails.Deliver.GoodsDelivered;
			fPickupFrom = Master.Shipment.ShipmentDetails.Pickup.PickupFrom;
			fPickupRequiredBy = Master.Shipment.ShipmentDetails.Pickup.PickupRequiredBy;
			fPickupCartageAdvised = Master.Shipment.ShipmentDetails.Pickup.CartageAdvised;
			fGoodsPickup = Master.Shipment.ShipmentDetails.Pickup.GoodsPickup;
			fEstimatedDateTimeDeparture = Master.Shipment.ShipmentDetails.PortOfOrigin.EstimatedDateTime;
			fEstimatedDateTimeArrival = Master.Shipment.ShipmentDetails.PortofDestination.EstimatedDateTime;
			fHBLIssueDate = Master.Shipment.ShipmentDetails.HBLIssueDate;
		}

		#endregion

		#region Properties

		#region PRONUM

		public ZString PRONUM
		{
			get
			{
				return fPRONUM;
			}
		}
		ZString fPRONUM;

		#endregion

		#region SHIPIDNUM

		public ZString SHIPIDNUM
		{
			get
			{
				return fSHIPIDNUM;
			}
		}
		ZString fSHIPIDNUM;

		#endregion

		#region DateCreated

		public ZString DateCreated
		{
			get
			{
				return ToTimeFormat(fDateCreated);
			}
		}
		ZDateTime fDateCreated;

		#endregion

		#region TransportMode

		public ZString TransportMode
		{
			get
			{
				return fTransportMode;
			}
		}
		ZString fTransportMode;

		#endregion

		#region PackingMode

		public ZString PackingMode
		{
			get
			{
				return fPackingMode;
			}
		}
		ZString fPackingMode;

		#endregion

		#region PortOfOrigin

		public ZString PortOfOrigin
		{
			get
			{
				return fPortOfOrigin.ToString();
			}
		}
		ZString fPortOfOrigin;

		#endregion

		#region PortOfOriginCountry

		public ZString PortOfOriginCountry
		{
			get
			{
				return fPortOfOriginCountry;
			}
		}
		ZString fPortOfOriginCountry;

		#endregion

		#region PortOfOriginCity

		public ZString PortOfOriginCity
		{
			get
			{
				return fPortOfOriginCity;
			}
		}
		ZString fPortOfOriginCity;

		#endregion

		#region PortofDestination

		public ZString PortofDestination
		{
			get
			{
				return fPortofDestination.ToString();
			}
		}
		ZString fPortofDestination;

		#endregion

		#region PortofDestinationCountry

		public ZString PortofDestinationCountry
		{
			get
			{
				return fPortofDestinationCountry;
			}
		}
		ZString fPortofDestinationCountry;

		#endregion

		#region PortofDestinationCity

		public ZString PortofDestinationCity
		{
			get
			{
				return fPortofDestinationCity;
			}
		}
		ZString fPortofDestinationCity;

		#endregion

		#region ShipmentStatus

		public ZString ShipmentStatus
		{
			get
			{
				return fShipmentStatus;
			}
		}
		ZString fShipmentStatus;

		#endregion

		#region TotalInnerPacksQty

		public ZString TotalInnerPacksQty
		{
			get
			{
				return fTotalInnerPacksQty.ToString();
			}
		}
		ZDecimal fTotalInnerPacksQty;

		#endregion

		#region TotalInnerPacksQtyDimensionType

		public ZString TotalInnerPacksQtyDimensionType
		{
			get
			{
				return fTotalInnerPacksQtyDimensionType;
			}
		}
		ZString fTotalInnerPacksQtyDimensionType;

		#endregion

		#region TotalOuterPacksQty

		public ZString TotalOuterPacksQty
		{
			get
			{
				return fTotalOuterPacksQty.ToString();
			}
		}
		ZDecimal fTotalOuterPacksQty;

		#endregion

		#region TotalOuterPacksQtyDimensionType

		public ZString TotalOuterPacksQtyDimensionType
		{
			get
			{
				return fTotalOuterPacksQtyDimensionType;
			}
		}
		ZString fTotalOuterPacksQtyDimensionType;

		#endregion

		#region GoodsDescription

		public ZString GoodsDescription
		{
			get
			{
				return fGoodsDescription;
			}
		}
		ZString fGoodsDescription;

		#endregion

		#region Weight

		public ZString Weight
		{
			get
			{
				return fWeight.ToString();
			}
		}
		ZDecimal fWeight;

		#endregion

		#region WeightDimensionType

		public ZString WeightDimensionType
		{
			get
			{
				return fWeightDimensionType;
			}
		}
		ZString fWeightDimensionType;

		#endregion

		#region ChargeableWeight

		public ZString ChargeableWeight
		{
			get
			{
				return fChargeableWeight.ToString();
			}
		}
		ZDecimal fChargeableWeight;

		#endregion

		#region ChargeableWeightDimensionType

		public ZString ChargeableWeightDimensionType
		{
			get
			{
				return fChargeableWeightDimensionType;
			}
		}
		ZString fChargeableWeightDimensionType;

		#endregion

		#region Volume

		public ZString Volume
		{
			get
			{
				return fVolume.ToString();
			}
		}
		ZDecimal fVolume;

		#endregion

		#region VolumeDimensionValue

		public ZString VolumeDimensionValue
		{
			get
			{
				return fVolumeDimensionValue;
			}
		}
		ZString fVolumeDimensionValue;

		#endregion

		#region GoodsValue

		public ZString GoodsValue
		{
			get
			{
				return fGoodsValue.ToString();
			}
		}
		ZDecimal fGoodsValue;

		#endregion

		#region GoodsValueCurrencyCode

		public ZString GoodsValueCurrencyCode
		{
			get
			{
				return fGoodsValueCurrencyCode;
			}
		}
		ZString fGoodsValueCurrencyCode;

		#endregion

		#region ServiceLevel

		public ZString ServiceLevel
		{
			get
			{
				return fServiceLevel;
			}
		}
		ZString fServiceLevel;

		#endregion

		#region Incoterm

		public ZString Incoterm
		{
			get
			{
				return fIncoterm;
			}
		}
		ZString fIncoterm;

		#endregion

		#region ReleaseType

		public ZString ReleaseType
		{
			get
			{
				return fReleaseType;
			}
		}
		ZString fReleaseType;

		#endregion

		#region AgentReference

		public ZString AgentReference
		{
			get
			{
				return fAgentReference;
			}
		}
		ZString fAgentReference;

		#endregion

		#region ShippedOnBoardType

		public ZString ShippedOnBoardType
		{
			get
			{
				return fShippedOnBoardType;
			}
		}
		ZString fShippedOnBoardType;

		#endregion

		#region MarksAndNumbers

		public ZString MarksAndNumbers
		{
			get
			{
				return fMarksAndNumbers;
			}
		}
		ZString fMarksAndNumbers;

		#endregion

		#region OwnerReference

		public ZString OwnerReference
		{
			get
			{
				return fOwnerReference;
			}
		}
		ZString fOwnerReference;

		#endregion

		#region BookingReference

		public ZString BookingReference
		{
			get
			{
				return fBookingReference;
			}
		}
		ZString fBookingReference;

		#endregion

		#region SCAC

		public ZString SCAC
		{
			get
			{
				return fSCAC;
			}
		}
		ZString fSCAC;

		#endregion

		#region DeliveryFrom

		public ZString DeliveryFrom
		{
			get
			{
				return ToTimeFormat(fDeliveryFrom);
			}
		}
		ZDateTime fDeliveryFrom;

		#endregion

		#region DeliveryRequiredBy

		public ZString DeliveryRequiredBy
		{
			get
			{
				return ToTimeFormat(fDeliveryRequiredBy);
			}
		}
		ZDateTime fDeliveryRequiredBy;

		#endregion

		#region DeliveryCartageAdvised

		public ZString DeliveryCartageAdvised
		{
			get
			{
				return ToTimeFormat(fDeliveryCartageAdvised);
			}
		}
		ZDateTime fDeliveryCartageAdvised;

		#endregion

		#region GoodsDelivered

		public ZString GoodsDelivered
		{
			get
			{
				return ToTimeFormat(fGoodsDelivered);
			}
		}
		ZDateTime fGoodsDelivered;

		#endregion

		#region PickupFrom

		public ZString PickupFrom
		{
			get
			{
				return ToTimeFormat(fPickupFrom);
			}
		}
		ZDateTime fPickupFrom;

		#endregion

		#region PickupRequiredBy

		public ZString PickupRequiredBy
		{
			get
			{
				return ToTimeFormat(fPickupRequiredBy);
			}
		}
		ZDateTime fPickupRequiredBy;

		#endregion

		#region PickupCartageAdvised

		public ZString PickupCartageAdvised
		{
			get
			{
				return ToTimeFormat(fPickupCartageAdvised);
			}
		}
		ZDateTime fPickupCartageAdvised;

		#endregion

		#region GoodsPickup

		public ZString GoodsPickup
		{
			get
			{
				return ToTimeFormat(fGoodsPickup);
			}
		}
		ZDateTime fGoodsPickup;

		#endregion

		#region EstimatedDateTimeDeparture

		public ZString EstimatedDateTimeDeparture
		{
			get
			{
				return ToTimeFormat(fEstimatedDateTimeDeparture);
			}
		}
		ZDateTime fEstimatedDateTimeDeparture;

		#endregion

		#region EstimatedDateTimeArrival

		public ZString EstimatedDateTimeArrival
		{
			get
			{
				return ToTimeFormat(fEstimatedDateTimeArrival);
			}
		}
		ZDateTime fEstimatedDateTimeArrival;

		#endregion

		#region HBLIssueDate

		public ZString HBLIssueDate
		{
			get
			{
				return ToTimeFormat(fHBLIssueDate);
			}
		}
		ZDateTime fHBLIssueDate;

		#endregion

		#endregion
	}
}
