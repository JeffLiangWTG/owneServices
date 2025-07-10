using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using EventConstants = CargoWise.EventReference.Constants;
using Params = CargoWise.EventReference.Constants.EventReferenceParameters.Codes;

namespace Enterprise.Freight.Forwarding.Business
{
	public class ShipmentPrepareForDispatchInstruction : NonPersistentBusinessObject
	{
		public ShipmentPrepareForDispatchInstruction(ForwardingShipment shipment, OrgAddress transitWarehouseAddressFromConsol)
		{
			Shipment = shipment;
			this.transitWarehouseAddressFromConsol = transitWarehouseAddressFromConsol;
		}

		public ForwardingShipment Shipment { get; }

		readonly OrgAddress transitWarehouseAddressFromConsol;

		public ZString ShipmentID => Shipment.JS_UniqueConsignRef;

		public ZInt NumberOfPacks => Shipment.JS_OuterPacks;

		public ZString PackType => Shipment.JS_F3_NKPackType;

		public ZString LastKnownTransitWarehouseStatus => Shipment.JS_Calc_LastKnownTransitWarehouseStatus;

		public ZString PrepareDispatchStatus
		{
			get
			{
				if (HasPreviouslySentPrepareDispatchInstruction)
				{
					return Res.GetString("7aaba912-e6be-3b94-4ae3-686bdafe02e9", "Sent");
				}

				return Res.GetString("592c8ea6-781e-c396-4c28-f3ab40b6f397", "Not Sent");
			}
		}

		public ZBool SelectedForDelivery
		{
			get
			{
				return selectedForDelivery;
			}
			set
			{
				selectedForDelivery = value;
				SelectedForDeliveryInfo.RefreshBinding();
			}
		}

		ZBool selectedForDelivery;

		public ZPropertyInfo SelectedForDeliveryInfo => GetZPropertyInfo(nameof(SelectedForDelivery));

		// The format describes the last known location + status of the shipment packages, e.g. 
		// AUSYD : RCV | NZAKL : DSP
		// This function returns true if all packages have been received at the TW of the pickup/destination of the consol

		public bool IsValidToSend
		{
			get
			{
				var lastKnownTransitWarehouseStatus = Shipment.JS_Calc_LastKnownTransitWarehouseStatus;

				var warehouseAndStatus = lastKnownTransitWarehouseStatus.Split(':');
				if (warehouseAndStatus.Length != 2)
				{
					return false;
				}

				var warehouse = warehouseAndStatus[0].Trim();
				if (warehouse.IsEmpty)
				{
					return false;
				}

				if (string.Equals(warehouse, transitWarehouseAddressFromConsol.Header?.OH_Code ?? ZString.Empty, StringComparison.OrdinalIgnoreCase))
				{
					var status = warehouseAndStatus[1].Trim();
					if (status == FreightConstants.PacklineLastKnownTransitWarehouseStatus.Codes.Received)
					{
						return true;
					}
				}

				return false;
			}
		}

		public bool HasPreviouslySentPrepareDispatchInstruction
		{
			get
			{
				return Shipment.Logs.MostRecentLogByEventTime(AutoEvents.ServiceRequested, LogIsOfParentConsolWarehouseDepot) != null;
			}
		}

		bool LogIsOfParentConsolWarehouseDepot(StmALog log)
		{
			if (log.Parameters.TryGetValue(Params.Facility, out var facilityParameter) &&
				log.Parameters.TryGetValue(Params.Warehouse, out var warehouseParameter))
			{
				return facilityParameter == EventConstants.Facilities.Code.Depot
					&& warehouseParameter == (transitWarehouseAddressFromConsol.Header?.OH_Code ?? ZString.Empty);
			}

			return false;
		}
	}
}
