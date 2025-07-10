using System.Linq;
using Enterprise.Packing.Business;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	class WhsPickableDocketExtensionsTest : WhsTestCaseWithFactory
	{
		#region TestFireReleaseLineAddedOrRemoved

		public void TestFireReleaseLineAddedOrRemoved()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateProductBOM(data.Part1, data.Part2);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part2, 10m);
			Factory.Save();
			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 5m);
			Helper.CreatePickNew(order);
			AssertContainsExactElementsInAnyOrder(new[] { orderLine.ReleaseLines[0] }, order.PackableItemParents.Typed);
			var workOrder = Helper.CreateWhsWorkOrder(data.Org1, data.Whs1);
			var workOrderLine = Helper.CreateWhsWorkOrderLine(workOrder, data.Part1, 5m);
			Helper.CreatePickNew(workOrder);
			var releaseLine = orderLine.ReleaseLines.AddNew();
			int countChangedHitCount = 0;
			IPackingParentWithPackableItems packingParent = order;
			packingParent.PackableItemParentsCountChanged += (sender, e) => countChangedHitCount++;
			((WhsPickableDocket)order).FireReleaseLineAddedOrRemoved(releaseLine, true);
			AssertEquals("Should have Fired Release Line Added on Order.", 1, countChangedHitCount);
			workOrder.FireReleaseLineAddedOrRemoved(releaseLine, true);
			AssertEquals("Work Order should be unaffected.", 0, workOrderLine.ReleaseLines.Count);
		}

		#endregion

		#region TestSuspendPackableItemParentsCountChanged

		public void TestSuspendPackableItemParentsCountChanged()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateProductBOM(data.Part1, data.Part2);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part2, 10m);
			Factory.Save();
			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 5m);
			Helper.CreatePickNew(order);
			AssertContainsExactElementsInAnyOrder(new[] { orderLine.ReleaseLines[0] }, order.PackableItemParents.Typed);
			var workOrder = Helper.CreateWhsWorkOrder(data.Org1, data.Whs1);
			var workOrderLine = Helper.CreateWhsWorkOrderLine(workOrder, data.Part1, 5m);
			Helper.CreatePickNew(workOrder);
			var releaseLine = orderLine.ReleaseLines.AddNew();
			int countChangedHitCount = 0;
			IPackingParentWithPackableItems packingParent = order;
			packingParent.PackableItemParentsCountChanged += (sender, e) => countChangedHitCount++;
			using (((WhsPickableDocket)order).SuspendPackableItemParentsCountChanged())
			{
				((WhsPickableDocket)order).FireReleaseLineAddedOrRemoved(releaseLine, true);
				AssertEquals("Should not have Fired Release Line Added while Count Changed is Suspended.", 0,
					countChangedHitCount);
			}

			AssertEquals("Should have Fired Release Line Added on Order after Suspension is finished.", 1,
				countChangedHitCount);
			AssertNull(workOrder.SuspendPackableItemParentsCountChanged());
		}

		#endregion

		#region TestRegisterOrderLineWithClearedReleaseLines

		public void TestRegisterOrderLineWithClearedReleaseLines()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateProductBOM(data.Part1, data.Part2);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part2, 10m);
			Factory.Save();
			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 5m);
			Helper.CreatePickNew(order);
			AssertContainsExactElementsInAnyOrder(new[] { orderLine.ReleaseLines[0] }, order.PackableItemParents.Typed);
			var workOrder = Helper.CreateWhsWorkOrder(data.Org1, data.Whs1);
			var workOrderLine = Helper.CreateWhsWorkOrderLine(workOrder, data.Part1, 5m);
			Helper.CreatePickNew(workOrder);
			orderLine.ReleaseLines.ClearCollection(); // calls RegisterOrderLineWithClearedReleaseLines
			AssertEquals("Should contain a Release Line. Force Evaluation on PackableItemParents first.", 1,
				order.PackableItemParents.Typed.Count());
			AssertContainsExactElementsInAnyOrder(new[] { orderLine.ReleaseLines[0] }, order.PackableItemParents.Typed);
			// Work Order doesn't have PackableItem Parents so there's no way to test the lack of it.
			workOrderLine.ReleaseLines.ClearCollection();
			AssertNoExceptionThrown(() => workOrder.RegisterOrderLineWithClearedReleaseLines(workOrderLine));
		}

		#endregion
	}
}
