using CargoWise.Types;
using Enterprise.Warehouse.Transit.Business;
using Enterprise.Warehouse.Transit.DataTransfer.Universal;
using Res = Enterprise.Warehouse.Transit.DataTransfer.Universal.Res;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Warehouse.Transit.DataTransfer
{
	public class ShipmentTransitLogHelper : TransitLogTableHelper<UniversalShipment, TransitLogColumnIDs.ShipmentColumn>
	{
		protected override ZString GetValue(UniversalShipment shipment, TransitLogColumnIDs.ShipmentColumn columnID)
		{
			switch (columnID)
			{
				case TransitLogColumnIDs.ShipmentColumn.Source:
					return GetShipmentSource(shipment);
				case TransitLogColumnIDs.ShipmentColumn.Target:
					return GetShipmentTarget(shipment);
				default:
					return "";
			}
		}

		protected override ZString GetHeader(TransitLogColumnIDs.ShipmentColumn columnID)
		{
			switch (columnID)
			{
				case TransitLogColumnIDs.ShipmentColumn.Source:
					return Res.GetString("567a7039-6467-4041-ba10-47a0b61e13b9", "Data Source");
				case TransitLogColumnIDs.ShipmentColumn.Target:
					return Res.GetString("ca5e228f-0999-43bf-b4cb-74ad634a4109", "Data Target");
				default:
					return "";
			}
		}

		static ZString GetShipmentSource(UniversalShipment shipment)
		{
			if (shipment != null && shipment.FirstDataSource() != null)
			{
				return $"{shipment.FirstDataSource()?.Type} - {shipment.FirstDataSource()?.Key}"; // This is a format string only contains symbols
			}

			return "";
		}

		static ZString GetShipmentTarget(UniversalShipment shipment)
		{
			if (shipment != null && shipment.FirstDataTarget() != null)
			{
				return $"{shipment.FirstDataTarget()?.Type} - {shipment.FirstDataTarget()?.Key}"; // This is a format string only contains symbols
			}

			return "";
		}
	}
}
