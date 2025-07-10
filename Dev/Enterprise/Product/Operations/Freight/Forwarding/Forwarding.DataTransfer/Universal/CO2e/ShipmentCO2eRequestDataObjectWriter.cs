using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Freight.CarbonEmissions.Business;
using Enterprise.Freight.CarbonEmissions.Integration;
using Enterprise.Freight.DataTransfer.Universal;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.DataTransfer.Universal.CO2e;
using Enterprise.Freight.Integration;
using Enterprise.Registry.Business;
using Enterprise.TransportCommon.Shared;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Freight.Forwarding.DataTransfer
{
	public sealed class ShipmentCO2eRequestDataObjectWriter : CO2eLegBasedRequestDataObjectWriter<ForwardingShipment, Transport>
	{
		public ShipmentCO2eRequestDataObjectWriter(IDataWritingManager writeManager) : base(writeManager)
		{
		}

		public ShipmentCO2eRequestDataObjectWriter(IDataWritingManager manager, ICO2eCalculationSupporter hostSupporter) : base(manager, hostSupporter)
		{
		}

		protected override DataObjectWriter<Transport, TransportLeg> GetLegDataObjectWriter()
		{
			return new CO2eTransportLegDataObjectWriter(writeManager);
		}

		protected override DataContextType GetTopLevelDataContextType()
		{
			return DataContextType.ForwardingShipment;
		}

		protected override void PopulateLegs(Shipment shipmentData, ForwardingShipment shipment)
		{
			if (!(shipment as ICO2ePrePostCarriage).RequiresPrePostCarriageLegs || shipment.Consols.Count == 0)
			{
				base.PopulateLegs(shipmentData, shipment);
			}
		}

		protected override void PopulateSubShipments(Shipment shipmentData, ForwardingShipment shipment)
		{
			if ((shipment as ICO2ePrePostCarriage).RequiresPrePostCarriageLegs)
			{
				shipmentData.SetSubShipmentCollection(() => GetSubShipmentCollection(shipment));
			}
		}

		protected override IEnumerable<PrePostCarriageLegWrapper> GetPreCarriageLegs(ForwardingShipment shipment)
		{
			var hblDeliveryModeToUse = FreightDataRegistry.Instance.UseHBLDeliveryModeForGHGCalculation.Value
				? shipment.JS_HBLContainerPackModeOverride
				: ZString.Empty;
			var result = shipment.GetPreCarriageLegs(hblDeliveryModeToUse).ToList();

			var sortedConsols = shipment.Consols.Cast<ForwardingConsol>().ToArray();
			MovementLegComparer.SortMovementLegsByPorts(sortedConsols);
			if (sortedConsols.Length > 0 && !(sortedConsols[0].PackDepotAddress == null && sortedConsols[0].HasTransportBooking(DtbBookingDirection.PIC)))
			{
				var shipmentLocation = (shipment as ICO2ePrePostCarriage).GetPreCarriageLocations(hblDeliveryModeToUse)
					.LastOrDefault(location => !location.IsEmpty);
				var firstConsolLocation = ((ICO2ePrePostCarriage)sortedConsols[0]).GetPreCarriageLocations(hblDeliveryModeToUse)
					.FirstOrDefault(location => !location.IsEmpty);
				result.AddLastLeg(shipmentLocation, firstConsolLocation, shipmentLocation?.TransportMode);
			}

			return result;
		}

		protected override IEnumerable<PrePostCarriageLegWrapper> GetPostCarriageLegs(ForwardingShipment shipment)
		{
			var hblDeliveryModeToUse = FreightDataRegistry.Instance.UseHBLDeliveryModeForGHGCalculation.Value
				? shipment.JS_HBLContainerPackModeOverride
				: ZString.Empty;
			var result = shipment.GetPostCarriageLegs(hblDeliveryModeToUse).ToList();

			var sortedConsols = shipment.Consols.Cast<ForwardingConsol>().ToArray();
			MovementLegComparer.SortMovementLegsByPorts(sortedConsols);
			if (sortedConsols.Length > 0 && !(sortedConsols[sortedConsols.Length - 1].UnpackDepotAddress == null && sortedConsols[sortedConsols.Length - 1].HasTransportBooking(DtbBookingDirection.DLV)))
			{
				var lastConsolLocation = ((ICO2ePrePostCarriage)sortedConsols[sortedConsols.Length - 1]).GetPostCarriageLocations(hblDeliveryModeToUse)
					.LastOrDefault(location => !location.IsEmpty);
				var shipmentLocation = (shipment as ICO2ePrePostCarriage).GetPostCarriageLocations(hblDeliveryModeToUse)
					.FirstOrDefault(location => !location.IsEmpty);
				result.AddFirstLeg(lastConsolLocation, shipmentLocation, shipmentLocation?.TransportMode);
			}

			return result;
		}

		DataObjectList<Shipment> GetSubShipmentCollection(ForwardingShipment shipment)
		{
			var hblDeliveryModeToUse = FreightDataRegistry.Instance.UseHBLDeliveryModeForGHGCalculation.Value
				? shipment.JS_HBLContainerPackModeOverride
				: ZString.Empty;
			var sortedConsols = shipment.Consols.Cast<ForwardingConsol>().ToArray();
			MovementLegComparer.SortMovementLegsByPorts(sortedConsols);

			var result = new DataObjectList<Shipment>();
			for (var idx = 0; idx < sortedConsols.Length; idx++)
			{
				var isFirst = idx == 0;
				var isLast = idx == sortedConsols.Length - 1;
				var lastLocation = isLast
					? null
					: (sortedConsols[idx + 1] as ICO2ePrePostCarriage).GetPreCarriageLocations(ZString.Empty).FirstOrDefault(location => !location.IsEmpty);

				var shipmentDO = new RelatedConsolCO2eRequestDataObjectWriter(shipment, lastLocation,
						isFirst ? hblDeliveryModeToUse : ZString.Empty,
						isLast ? hblDeliveryModeToUse : ZString.Empty,
						isFirst,
						isLast,
						writeManager)
					.GetDataObject(sortedConsols[idx]);
				if (shipmentDO != null)
				{
					result.Add(shipmentDO);
				}
			}

			return result.Count == 0 ? null : result;
		}
	}
}
