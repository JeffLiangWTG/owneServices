using System.Collections.Generic;
using CargoWise.EntityFramework;

namespace Enterprise.Freight.Forwarding.Business
{
	public class ConsolPrepareForDispatchInstruction : NonPersistentBusinessObject
	{
		public ConsolPrepareForDispatchInstruction(ForwardingConsol consol, TransitWarehouseInstructionHelper.Direction direction)
		{
			Direction = direction;
			Consol = consol;
		}

		public TransitWarehouseInstructionHelper.Direction Direction { get; }

		public ForwardingConsol Consol { get; }

		public ShipmentPrepareForDispatchInstructionCollection ShipmentsForSelection
		{
			get
			{
				if (shipmentsForSelection == null)
				{
					shipmentsForSelection = new ShipmentPrepareForDispatchInstructionCollection(Consol, Direction);
				}

				return shipmentsForSelection;
			}
		}

		ShipmentPrepareForDispatchInstructionCollection shipmentsForSelection;

		public IEnumerable<ShipmentPrepareForDispatchInstruction> GetAllSelectedShipments()
		{
			return ShipmentsForSelection
				.Where(shipmentWrapper => shipmentWrapper.SelectedForDelivery);
		}
	}
}
