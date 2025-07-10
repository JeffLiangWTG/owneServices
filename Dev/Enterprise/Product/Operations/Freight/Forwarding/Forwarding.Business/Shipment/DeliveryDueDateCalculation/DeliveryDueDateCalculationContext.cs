using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Rating.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.Business
{
	public sealed class DeliveryDueDateCalculationContext
	{
		public DeliveryDueDateCalculationContext(BusinessObjectFactory factory,
			ZDateTime readyDate,
			ZString serviceLevel,
			ZString hblDeliveryMode,
			ZString pickupOrgCode,
			ZString pickupAddressCode,
			ZString pickupCFSOrgCode,
			ZString pickupCFSAddressCode,
			ZString deliveryCFSOrgCode,
			ZString deliveryCFSAddressCode,
			ZString deliveryOrgCode,
			ZString deliveryAddressCode,
			ZString mode,
			ZString deliveryType)
		{
			var isDTC = (hblDeliveryMode == Core.Constants.HBLDeliveryModes.Codes.DOOR_DOOR
					|| hblDeliveryMode == Core.Constants.HBLDeliveryModes.Codes.CFS_DOOR)
					&& deliveryType.EqualsIgnoringCase(Core.Constants.DeliveryTypes.DirectToCNE);

			Factory = factory;
			HBLDeliveryMode = hblDeliveryMode;
			ReadyDate = readyDate;
			ServiceLevel = factory.LoadFromNaturalKey<RefServiceLevel>(RefServiceLevelSchema.RS_Code, serviceLevel);
			ServiceLevelGenericTransitTimeHasBeenUsed = false;
			Mode = mode;
			PickupAddress = DeliveryDueDateCalculationHelper.LoadAddressByOrgCodeAndShortCode(factory, pickupOrgCode, pickupAddressCode);
			DeliveryAddress = DeliveryDueDateCalculationHelper.LoadAddressByOrgCodeAndShortCode(factory, deliveryOrgCode, deliveryAddressCode);
			CFSPickupAddress = DeliveryDueDateCalculationHelper.LoadAddressByOrgCodeAndShortCode(factory, pickupCFSOrgCode, pickupCFSAddressCode) as OrgAddress;
			IsDTC = isDTC;

			if (isDTC)
			{
				DeliveryAgentAddress = DeliveryDueDateCalculationHelper.LoadAddressByOrgCodeAndShortCode(factory, deliveryCFSOrgCode, deliveryCFSAddressCode) as OrgAddress;
			}
			else
			{
				CFSDeliveryAddress = DeliveryDueDateCalculationHelper.LoadAddressByOrgCodeAndShortCode(factory, deliveryCFSOrgCode, deliveryCFSAddressCode) as OrgAddress;
			}
		}

		public DeliveryDueDateCalculationContext(ForwardingShipment shipment)
		{
			ETAProvider = shipment.GetEtaProvider();

			Factory = shipment.Factory;
			HBLDeliveryMode = shipment.JS_HBLContainerPackModeOverride;
			var calculatedReadyDate = DeliveryDueDateCalculationHelper.GetReadyDateFromShipment(shipment);
			WhichDateSelectedAsReadyDate = calculatedReadyDate.WhichDateSelected;
			ReadyDate = calculatedReadyDate.ReadyDate;
			ServiceLevel = shipment.Factory.LoadFromNaturalKey<RefServiceLevel>(RefServiceLevelSchema.RS_Code, shipment.JS_RS_NKServiceLevel);
			ServiceLevelGenericTransitTimeHasBeenUsed = false;
			Mode = DeliveryDueDateCalculationHelper.GetRateModeFromShipment(shipment);

			PickupAddress = shipment.ConsignorPickupAddress == null
				? null
				: shipment.ConsignorPickupAddress.E2_AddressOverride ? shipment.ConsignorPickupAddress : shipment.ConsignorPickupAddress.Address;
			DeliveryAddress = shipment.ConsigneeDeliveryAddress == null
				? null
				: shipment.ConsigneeDeliveryAddress.E2_AddressOverride ? shipment.ConsigneeDeliveryAddress : shipment.ConsigneeDeliveryAddress.Address;

			CFSDeliveryAddress = shipment.JS_OA_ImportReleaseDepot_ZAddress?.OrgAddress as OrgAddress;
			CFSPickupAddress = shipment.JS_OA_ExportReceivingDepot_ZAddress?.OrgAddress as OrgAddress;
			DeliveryAgentAddress = shipment.DeliveryAgent != null ? shipment.DeliveryAgent.GetAddressWithFallback(ZArchitecture.Business.AddressType.DLV) : null;

			IsDTC = shipment.IsDTC;
		}

		public IDeliveryDueDateCalculationResult CalculateTimeOfArrivalToAirport()
		{
			if (ETAProvider == null)
			{
				return DeliveryDueDateCalculationResult.Failure(ETAProviderConstants.ErrorNoShipment, ETAProviderConstants.ErrorNoShipment);
			}
			return ETAProvider.CalculateTimeOfArrivalToAirport();
		}

		IETAProvider ETAProvider { get; set; }

		public BusinessObjectFactory Factory { get; private set; }

		public ZDateTime ReadyDate { get; private set; }

		public ZString WhichDateSelectedAsReadyDate { get; private set; }

		public ZString hblDeliveryModeBackingField { get; private set; }

		public ZString HBLDeliveryMode
		{
			get => hblDeliveryModeBackingField;
			private set
			{
				hblDeliveryModeBackingField = value;
				IsXtoCFS = new ZString[] { Core.Constants.HBLDeliveryModes.Codes.DOOR_CFS, Core.Constants.HBLDeliveryModes.Codes.CFS_CFS, Core.Constants.HBLDeliveryModes.Codes.ARPT_CFS }.Contains(value);
				IsXtoDoor = new ZString[] { Core.Constants.HBLDeliveryModes.Codes.DOOR_DOOR, Core.Constants.HBLDeliveryModes.Codes.CFS_DOOR, Core.Constants.HBLDeliveryModes.Codes.ARPT_DOOR }.Contains(value);
				IsDoortoX = new ZString[] { Core.Constants.HBLDeliveryModes.Codes.DOOR_DOOR, Core.Constants.HBLDeliveryModes.Codes.DOOR_CFS, Core.Constants.HBLDeliveryModes.Codes.DOOR_ARPT }.Contains(value);
				IsXtoAirport = new ZString[] { Core.Constants.HBLDeliveryModes.Codes.ARPT_ARPT, Core.Constants.HBLDeliveryModes.Codes.DOOR_ARPT, Core.Constants.HBLDeliveryModes.Codes.CFS_ARPT }.Contains(value);
			}
		}

		public RefServiceLevel ServiceLevel { get; private set; }

		public bool ServiceLevelGenericTransitTimeHasBeenUsed { get; set; }

		public ZString ServicelevelCode
		{
			get
			{
				return ServiceLevel?.RS_Code ?? ZString.Empty;
			}
		}

		public IDocAddress PickupAddress { get; private set; }

		public IDocAddress DeliveryAddress { get; private set; }

		public OrgAddress CFSPickupAddress { get; private set; }

		public OrgAddress CFSDeliveryAddress { get; private set; }

		public OrgAddress DeliveryAgentAddress { get; private set; }

		public ZString Mode { get; private set; }

		public bool IsDTC { get; private set; }

		public bool IsXtoCFS { get; private set; }

		public bool IsXtoDoor { get; private set; }

		public bool IsDoortoX { get; private set; }

		public bool IsXtoAirport { get; private set; }

		public DeliveryDueTime DeliveryDueTime => DeliveryDueTimeFunc();

		public Func<DeliveryDueTime> DeliveryDueTimeFunc => CalculateDeliveryDueTime;

		DeliveryDueTime CalculateDeliveryDueTime()
		{
			if (!ServiceLevelGenericTransitTimeHasBeenUsed)
			{
				if (DeliveryDueTimeFromTransportZoneSet != null)
				{
					return DeliveryDueTimeFromTransportZoneSet;
				}
			}

			if (ServiceLevel != null && !ServiceLevel.RS_DefaultDeliveryDueTime.IsEmpty)
			{
				return new DeliveryDueTime(ServiceLevel.RS_DefaultDeliveryDueTime.TimeOfDay, DeliveryDueTimeSource.ServiceLevel);
			}

			return DeliveryDueTimeFromTransportZoneSet;
		}

		public DeliveryDueTime DeliveryDueTimeFromTransportZoneSet
		{
			get
			{
				if (DestinationZoneItemFromDeliveryAddress != null && !DestinationZoneItemFromDeliveryAddress.TQ_DeliveryDueTime.IsEmpty)
				{
					return new DeliveryDueTime(DestinationZoneItemFromDeliveryAddress.TQ_DeliveryDueTime.TimeOfDay, DeliveryDueTimeSource.ZoneItem);
				}

				if (DestinationTransportProvider != null && !DestinationTransportProvider.TP_DefaultDeliveryDueTime.IsEmpty)
				{
					return new DeliveryDueTime(DestinationTransportProvider.TP_DefaultDeliveryDueTime.TimeOfDay, DeliveryDueTimeSource.TransportProvider);
				}

				return null;
			}
		}

		public TimeSpan? HoldForPickupTime
		{
			get
			{
				if (DestinationTransportProvider != null && !DestinationTransportProvider.TP_DefaultHoldForPickupTime.IsEmpty && IsXtoCFS)
				{
					return DestinationTransportProvider.TP_DefaultHoldForPickupTime.TimeOfDay;
				}

				return null;
			}
		}

		public bool DeliverOnWeekend => ServiceLevel?.DeliverOnWeekend ?? false;

		public RateTransportZoneItem OriginZoneItemFromPickupAddress => originZoneItemPickupAddress ?? (originZoneItemPickupAddress = DeliveryDueDateCalculationHelper.GetZoneItem(PickupAddress, CFSPickupAddress, Factory));
		RateTransportZoneItem originZoneItemPickupAddress;

		public RateTransportZoneItem DestinationZoneItemFromDeliveryAddress => destinationZoneItemDeliveryAddress ?? (destinationZoneItemDeliveryAddress = DeliveryDueDateCalculationHelper.GetZoneItem(DeliveryAddress, IsDTC ? DeliveryAgentAddress : CFSDeliveryAddress, Factory));
		RateTransportZoneItem destinationZoneItemDeliveryAddress;

		public RateTransportZoneItem OriginZoneItemFromCFSPickupAddress => originZoneItemCFSPickupAddress ?? (originZoneItemCFSPickupAddress = DeliveryDueDateCalculationHelper.GetZoneItem(PickupAddress, CFSPickupAddress, Factory, !IsDoortoX));
		RateTransportZoneItem originZoneItemCFSPickupAddress;

		public RateTransportZoneItem DestinationZoneItemFromCFSDeliveryAddress => destinationZoneItemFromCFSDeliveryAddress ?? (destinationZoneItemFromCFSDeliveryAddress = DeliveryDueDateCalculationHelper.GetZoneItem(DeliveryAddress, IsDTC ? DeliveryAgentAddress : CFSDeliveryAddress, Factory, !IsXtoDoor));
		RateTransportZoneItem destinationZoneItemFromCFSDeliveryAddress;

		public RateTransportProvider OriginTransportProvider => originTransportProvider ?? (originTransportProvider = DeliveryDueDateCalculationHelper.GetTransportProvider(PickupAddress, CFSPickupAddress, Factory));
		RateTransportProvider originTransportProvider;

		public RateTransportProvider DestinationTransportProvider => destinationTransportProvider ?? (destinationTransportProvider = DeliveryDueDateCalculationHelper.GetTransportProvider(DeliveryAddress, IsDTC ? DeliveryAgentAddress : CFSDeliveryAddress, Factory));
		RateTransportProvider destinationTransportProvider;

		public RateTransportZone OriginZone => originZone ?? (originZone = OriginZoneItemFromCFSPickupAddress?.Zone ?? OriginTransportProvider?.Zones.FirstOrDefault(x => x.TZ_IsActive));
		RateTransportZone originZone;

		public RateTransportZone DestinationZone => destinationZone ?? (destinationZone = DestinationZoneItemFromCFSDeliveryAddress?.Zone ?? DestinationTransportProvider?.Zones.FirstOrDefault(x => x.TZ_IsActive));
		RateTransportZone destinationZone;
	}
}
