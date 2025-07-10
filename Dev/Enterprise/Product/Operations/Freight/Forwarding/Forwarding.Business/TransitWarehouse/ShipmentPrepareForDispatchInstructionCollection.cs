using System.Linq;
using CargoWise.EntityFramework;

namespace Enterprise.Freight.Forwarding.Business
{
	public class ShipmentPrepareForDispatchInstructionCollection : NonPersistentBusinessObjectCollection<ShipmentPrepareForDispatchInstruction>
	{
		public ShipmentPrepareForDispatchInstructionCollection(ForwardingConsol parentConsol, TransitWarehouseInstructionHelper.Direction direction)
		{
			InitializeCollection(parentConsol, direction);
		}

		void InitializeCollection(ForwardingConsol parentConsol, TransitWarehouseInstructionHelper.Direction direction)
		{
			var supporter = parentConsol as ITransitWarehouseInstructionSupporter;
			var instructionDestination = TransitWarehouseInstructionHelper.GetTransitWarehouseAddress(supporter, direction);

			foreach (var shipment in parentConsol.Shipments.OfType<ForwardingShipment>())
			{
				Add(new ShipmentPrepareForDispatchInstruction(shipment, instructionDestination));
			}
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			throw new System.InvalidOperationException("Collection should not allow new objects.");
		}

		protected override bool AllowNewCore => false;

		protected override bool AllowRemoveCore => false;
	}
}
