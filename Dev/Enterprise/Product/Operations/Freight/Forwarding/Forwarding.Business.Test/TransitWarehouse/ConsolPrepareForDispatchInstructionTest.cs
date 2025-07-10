using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	[TestedType(typeof(ConsolPrepareForDispatchInstruction))]
	sealed class ConsolPrepareForDispatchInstructionTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return new ConsolPrepareForDispatchInstruction(Factory.NewWithValidTestData<ForwardingConsol>(), TransitWarehouseInstructionHelper.Direction.Pickup);
		}

		public void TestGetAllSelectedShipments()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.Shipments.AddNew();
			consol.Shipments.AddNew();
			consol.Shipments.AddNew();

			Factory.Save();

			var consolForPrepareDipatch = new ConsolPrepareForDispatchInstruction(consol, TransitWarehouseInstructionHelper.Direction.Delivery);
			var shipmentsForSelection = consolForPrepareDipatch.ShipmentsForSelection;

			shipmentsForSelection[0].SelectedForDelivery = true;

			AssertEquals("Only one shipment selected for delivery", consolForPrepareDipatch.GetAllSelectedShipments().Count(), 1);
		}
	}
}
