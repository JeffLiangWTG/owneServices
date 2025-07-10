using System;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Rating.Business;

namespace Enterprise.Freight.Forwarding.Business
{
	public class ProfitShareForwardingShipmentWrapper : NonPersistentBusinessObject
	{
		public ProfitShareForwardingShipmentWrapper(ForwardingShipment shipment)
		{
			Argument.NotNull(shipment, nameof(shipment));
			Shipment = shipment;
		}

		public ProfitShareForwardingShipmentWrapper(ForwardingShipment shipment, ShipmentProfitShares shipmentProfitShares) : this(shipment)
		{
			JS_Calc_PickupAgentProfitShare = shipmentProfitShares.PSS_PickupAgentShare;
			JS_Calc_DeliveryAgentProfitShare = shipmentProfitShares.PSS_DeliveryAgentShare;
		}

		public ForwardingShipment Shipment { get; }

		protected override ZGuid GetPK() => Shipment.PK;

		public ZDecimal JS_Calc_PickupAgentProfitShare
		{
			get => calcPickupAgentProfitShare;
			set
			{
				if (calcPickupAgentProfitShare != value)
				{
					calcPickupAgentProfitShare = value;
					JS_Calc_PickupAgentProfitShareInfo.RefreshBinding();
				}
			}
		}
		ZDecimal calcPickupAgentProfitShare = 0m;

		public ZPropertyInfo JS_Calc_PickupAgentProfitShareInfo
		{
			get { return GetZPropertyInfo(nameof(JS_Calc_PickupAgentProfitShare)); }
		}

		public ZDecimal JS_Calc_DeliveryAgentProfitShare
		{
			get => calcDeliveryAgentProfitShare;
			set
			{
				if (calcDeliveryAgentProfitShare != value)
				{
					calcDeliveryAgentProfitShare = value;
					JS_Calc_DeliveryAgentProfitShareInfo.RefreshBinding();
				}
			}
		}
		ZDecimal calcDeliveryAgentProfitShare = 0m;

		public ZPropertyInfo JS_Calc_DeliveryAgentProfitShareInfo
		{
			get { return GetZPropertyInfo(nameof(JS_Calc_DeliveryAgentProfitShare)); }
		}

		public ZString JS_UniqueConsignRef => Shipment.JS_UniqueConsignRef;
		public ZString JS_JK_ConsolID => Shipment.JS_JK_ConsolID;
		public ZString JS_TransportMode => Shipment.JS_TransportMode;
		public ZString JS_PackingMode => Shipment.JS_PackingMode;
		public ZString JS_ShipmentType => Shipment.JS_ShipmentType;
		[List("Shipment.Lookups.RefUNLOCO_List")]
		public ZString JS_RL_NKOrigin => Shipment.JS_RL_NKOrigin;
		[List("Shipment.Lookups.DischargePorts")]
		public ZString JS_RL_NKDischargePort => Shipment.JS_RL_NKDischargePort;
		[List("Shipment.Lookups.RefUNLOCO_List")]
		public ZString JS_RL_NKDestination => Shipment.JS_RL_NKDestination;
		public ZString JS_HouseBill => Shipment.JS_HouseBill;
		public ZInt JS_OuterPacks => Shipment.JS_OuterPacks;
		public ZDecimal JS_ActualWeight => Shipment.JS_ActualWeight;
		public ZString JS_UnitOfWeight => Shipment.JS_UnitOfWeight;
		public ZDecimal JS_ActualVolume => Shipment.JS_ActualVolume;
		public ZString JS_UnitOfVolume => Shipment.JS_UnitOfVolume;
		public ZString JS_ChargeableUnit => Shipment.JS_ChargeableUnit;
		public ZDecimal JS_ActualChargeable => Shipment.JS_ActualChargeable;
		public ZDateTime JS_E_DEP => Shipment.JS_E_DEP;
		public ZDateTime JS_E_ARV => Shipment.JS_E_ARV;
		public ZString JS_GoodsDescription => Shipment.JS_GoodsDescription;
		public ZDecimal JS_GoodsValue => Shipment.JS_GoodsValue;
		public ZString JS_BookingReference => Shipment.JS_BookingReference;
		public ZBool JS_IsBooking => Shipment.JS_IsBooking;
		public ZString ColoadMasterShipmentHouseBill => Shipment.ColoadMasterShipmentHouseBill;
		public ZString JS_HBLContainerPackModeOverride => Shipment.JS_HBLContainerPackModeOverride;
		public ZInt JS_PackingOrder => Shipment.JS_PackingOrder;
		public ZString JS_ReleaseType => Shipment.JS_ReleaseType;
		public ZString JS_INCO => Shipment.JS_INCO;
		public ZString JS_PaymentTermDisplay => Shipment.JS_PaymentTermDisplay;
		[List("Shipment.Lookups.Broker_List")]
		public ZGuid JS_OH_ExportBroker => Shipment.JS_OH_ExportBroker;
		[List("Shipment.Lookups.Broker_List")]
		public ZGuid JS_OH_ImportBroker => Shipment.JS_OH_ImportBroker;
		[List("Shipment.Lookups.DeliveryAgents")]
		public ZGuid JS_OH_DeliveryAgent => Shipment.JS_OH_DeliveryAgent;
		[List("Shipment.Lookups.PickupAgent_List")]
		public ZGuid PickupAgentPK => Shipment.PickupAgentPK;
		public ZBool IsDomesticFreight => Shipment.IsDomesticFreight;
		public ZDateTime JS_HouseBillIssueDate => Shipment.JS_HouseBillIssueDate;
		public ZDecimal JS_Calc_ActualVolumeWeight => Shipment.JS_Calc_ActualVolumeWeight;
		public ZString JS_Calc_ActualVolumeWeightUnit => Shipment.JS_Calc_ActualVolumeWeightUnit;
		public ZString JS_ShipmentStatus => Shipment.JS_ShipmentStatus;
		public ZString NumbersAsString => Shipment.NumbersAsString;
		[List("Shipment.Lookups.LoadPorts")]
		public ZString JS_RL_NKLoadPort => Shipment.JS_RL_NKLoadPort;
		[List("Shipment.Lookups.FreightRateOrigins")]
		public ZString JS_RL_NKFreightRateOrigin => Shipment.JS_RL_NKFreightRateOrigin;
		[List("Shipment.Lookups.FreightRateDestinations")]
		public ZString JS_RL_NKFreightRateDestination => Shipment.JS_RL_NKFreightRateDestination;
	}

	public class ProfitShareForwardingShipmentWrapperCollection : NonPersistentBusinessObjectCollection<ProfitShareForwardingShipmentWrapper>
	{
		public ProfitShareForwardingShipmentWrapperCollection(BusinessObjectFactory factory) : base(factory) { }

		protected override BusinessObject CreateNonPersistentBusinessObject() => throw new NotSupportedException();

		protected override bool AllowNewCore => false;
	}
}
