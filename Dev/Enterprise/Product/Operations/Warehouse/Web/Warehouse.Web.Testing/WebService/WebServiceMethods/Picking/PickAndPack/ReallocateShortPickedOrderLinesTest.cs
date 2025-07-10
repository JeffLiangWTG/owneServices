using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.Warehouse.Web.WebService.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Moq;
using static Enterprise.Core.Constants;

namespace Enterprise.Warehouse.Web.WebService.Testing
{
	class ReallocateShortPickedOrderLinesTest : WhsPickingSecureServiceTestCase
	{
		#region TestReallocateShortPickedOrderLines_SingleLine

		public void TestReallocateShortPickedOrderLines_SingleLine()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);

			var staff = Helper.CreateGlbStaff("OP1", "Test1");
			Helper.Factory.Save();

			var loc1 = data.Whs1.FindLocation("A-1");
			var loc2 = data.Whs1.FindLocation("A-2");

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receiveLine1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 5m, loc1);
			var receiveLine2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 5m, loc2);
			receive.FinaliseDocketWithoutUserConfirmation();
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "Order1", Notify);
			var orderLine1 = Helper.CreateWhsOrderLine(order, data.Part1, 5m);

			var pick = Helper.CreatePickNew(order);
			var pickLine1 = orderLine1.PickLines.Single();
			AssignPickLine(pickLine1, staff);
			Helper.Factory.Save();

			var webService1 = GetNewWebService(data.Whs1, staff);
			var shortPickResponse = webService1.PickAndPossiblyReleaseCaptureAndPossiblyPack(
				new[] { new PickLinesToPickedPackTypeInfo(new[] { pickLine1.PK.ToGuid() }, Array.Empty<PickedPackTypeInfo>()) },
				new PickingInfo(0, false),
				null,
				true);

			AssertEquals("Pre-condition: Pick was marked as shorted.", true, pick.HasShortfallItems);
			AssertEquals("Pre-condition: Order was shorted.", 0m, order.WD_UnitsSent);
			AssertEquals("Pre-condition: 1 shorted OrderLinePK returned.", 1, shortPickResponse.ShortedOrderLinePKs.Length);
			AssertEquals("Pre-condition: PK of orderLine1 was returned.", orderLine1.PK, shortPickResponse.ShortedOrderLinePKs[0]);

			using (Globals.SetIsUserInteractiveForTest(false))
			{
				var webService2 = GetNewWebService(data.Whs1, staff);
				var response = webService2.ReallocateShortPickedOrderLines(pick.PK.ToGuid(), new[] { orderLine1.PK.ToGuid() }, Guid.Empty);

				AssertEquals("1 Pick Line Group returned after reallocation.", 1, response.Pick.Lines.Count);
				AssertEquals("The reallocated PickLine has 5 units to pick.", 5m, response.Pick.Lines[0].Units);
			}
		}

		#endregion

		#region TestReallocateShortPickedOrderLines_PartialShorted

		public void TestReallocateShortPickedOrderLines_PartialShorted()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);

			var staff = Helper.CreateGlbStaff("OP1", "Test1");
			Helper.Factory.Save();

			var loc1 = data.Whs1.FindLocation("A-1");
			var loc2 = data.Whs1.FindLocation("A-2");

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receiveLine1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 15m, loc1);
			var receiveLine2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 85m, loc2);
			receive.FinaliseDocketWithoutUserConfirmation();
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "Order1", Notify);
			var orderLine1 = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var orderLine2 = Helper.CreateWhsOrderLine(order, data.Part1, 5m);

			var pick = Helper.CreatePickNew(order);
			var pickLine1 = orderLine1.PickLines.Single();
			var pickLine2 = orderLine2.PickLines.Single();
			AssignPickLine(pickLine1, staff);
			AssignPickLine(pickLine2, staff);
			Helper.Factory.Save();

			var webService1 = GetNewWebService(data.Whs1, staff);
			var shortPickResponse = webService1.PickAndPossiblyReleaseCaptureAndPossiblyPack(
				new[] { new PickLinesToPickedPackTypeInfo(new[] { pickLine1.PK.ToGuid(), pickLine2.PK.ToGuid() }, Array.Empty<PickedPackTypeInfo>()) },
				new PickingInfo(11, false),
				null,
				true);

			AssertEquals("Pre-condition: Pick was marked as shorted.", true, pick.HasShortfallItems);
			AssertEquals("Pre-condition: Order was shorted.", 11m, order.WD_UnitsSent);
			AssertEquals("Pre-condition: 1 shorted OrderLinePK returned.", 1, shortPickResponse.ShortedOrderLinePKs.Length);
			AssertEquals("Pre-condition: PK of orderLine2 was returned.", orderLine2.PK, shortPickResponse.ShortedOrderLinePKs[0]);

			using (Globals.SetIsUserInteractiveForTest(false))
			{
				var webService2 = GetNewWebService(data.Whs1, staff);
				var response = webService2.ReallocateShortPickedOrderLines(pick.PK.ToGuid(), new[] { orderLine2.PK.ToGuid() }, Guid.Empty);

				AssertEquals("1 Pick Line Group returned after reallocation.", 1, response.Pick.Lines.Count);
				AssertEquals("The reallocated PickLine has 4 units to pick.", 4m, response.Pick.Lines[0].Units);
			}
		}

		#endregion

		#region TestReallocateShortPickedOrderLines_PickByUOMEnabled

		public void TestReallocateShortPickedOrderLines_PickByUOMEnabled()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			data.Whs1.WW_IsPickByUOMEnabled = true;

			var row = Helper.CreateRow(data.Whs1, "AA", 3, 1);
			var staff = Helper.CreateGlbStaff("OP1", "Test1");
			Helper.Factory.Save();

			var receive1 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 2m, row.Locations[0], "PLT001");
			var receive2 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 5m, row.Locations[1], "PLT002");
			Helper.Factory.Save();

			var order1 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "Order1", Notify);
			var orderLine1 = Helper.CreateWhsOrderLine(order1, data.Part1, 5m);

			var pick = Helper.CreatePickNew(order1);
			pick.OrderedInventories[0].AvailableInventories[0].PickLineQuantity = 0m;
			pick.OrderedInventories[0].AvailableInventories[1].PickLineQuantity = 0m;

			AssertEquals("Pre-condition: Pick Lines removed.", 0, pick.GetAllPickLines().Count());

			pick.OrderedInventories[0].AvailableInventories.Where(i => i.QuantityAvailableToPick == 2).Single().PickLineQuantity = 2m;
			pick.OrderedInventories[0].AvailableInventories.Where(i => i.QuantityAvailableToPick == 5).Single().PickLineQuantity = 3m;

			var pickLines = pick.GetAllPickLines();
			AssertEquals("Pre-condition: Pick Lines generated.", 2, pickLines.Count());

			var pickLine1 = pickLines.Single(l => l.WZ_Units == 2m);
			var pickLine2 = pickLines.Single(l => l.WZ_Units == 3m);
			AssignPickLine(pickLine1, staff);
			AssignPickLine(pickLine2, staff);
			Helper.Factory.Save();

			var webService1 = GetNewWebService(data.Whs1, staff);
			var shortPickResponse = webService1.PickAndPossiblyReleaseCaptureAndPossiblyPack(
				new[] { new PickLinesToPickedPackTypeInfo(new[] { pickLine1.PK.ToGuid() }, Array.Empty<PickedPackTypeInfo>()) },
				new PickingInfo(1, false),
				null,
				true);
			Helper.Factory.Save();

			AssertEquals("Pre-condition: Pick was marked as shorted.", true, pick.HasShortfallItems);
			AssertEquals("Pre-condition: Order1 was shorted.", 4m, order1.WD_UnitsSent);
			AssertEquals("Pre-condition: 1 PK of shorted orderLines were returned.", 1, shortPickResponse.ShortedOrderLinePKs.Length);
			AssertEquals("Pre-condition: PK of orderLine1 were returned.", orderLine1.PK.ToGuid(), shortPickResponse.ShortedOrderLinePKs[0]);

			using (Globals.SetIsUserInteractiveForTest(false))
			{
				var webService2 = GetNewWebService(data.Whs1, staff);
				var response = webService2.ReallocateShortPickedOrderLines(
					pick.PK.ToGuid(),
					new[] { orderLine1.PK.ToGuid() }, Guid.Empty);

				AssertEquals("1 Pick Line Group returned after reallocation", 1, response.Pick.Lines.Count);

				var pickLineGroup1 = response.Pick.Lines.Single();
				AssertEquals("pickLineGroup1 contains 2 Pick Lines", 2, pickLineGroup1.PKs.Length);
				AssertEquals("pickLineGroup1 has 1+3=4 units to pick.", 4m, pickLineGroup1.Units);
			}
		}

		#endregion

		#region TestReallocateShortPickedOrderLines_MultipleLines

		public void TestReallocateShortPickedOrderLines_MultipleLines()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 5, 1);

			var staff = Helper.CreateGlbStaff("OP1", "Test1");
			Helper.Factory.Save();

			var loc1 = data.Whs1.FindLocation("A-1");
			var loc2 = data.Whs1.FindLocation("A-2");
			var loc3 = data.Whs1.FindLocation("A-3");
			var loc4 = data.Whs1.FindLocation("A-4");
			var loc5 = data.Whs1.FindLocation("A-5");

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 5m, loc1);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 5m, loc2);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 5m, loc3);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 5m, loc4);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 80m, loc5);
			receive.FinaliseDocketWithoutUserConfirmation();

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part2, 10m);
			Helper.Factory.Save();

			var order1 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "Order1", Notify);
			var orderLine1 = Helper.CreateWhsOrderLine(order1, data.Part1, 5m);
			var orderLine2 = Helper.CreateWhsOrderLine(order1, data.Part1, 5m);
			var order2 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "Order2", Notify);
			var orderLine3 = Helper.CreateWhsOrderLine(order2, data.Part1, 5m);
			var orderLine4 = Helper.CreateWhsOrderLine(order2, data.Part1, 5m);
			var orderLine5 = Helper.CreateWhsOrderLine(order2, data.Part2, 10m);

			var pick = Helper.CreatePickNew(order1, order2);
			var pickLine1 = orderLine1.PickLines.Single();
			var pickLine2 = orderLine2.PickLines.Single();
			var pickLine3 = orderLine3.PickLines.Single();
			var pickLine4 = orderLine4.PickLines.Single();
			var pickLine5 = orderLine5.PickLines.Single();
			AssignPickLine(pickLine1, staff);
			AssignPickLine(pickLine2, staff);
			AssignPickLine(pickLine3, staff);
			AssignPickLine(pickLine4, staff);
			AssignPickLine(pickLine5, staff);
			Helper.Factory.Save();

			var webService1 = GetNewWebService(data.Whs1, staff);
			var shortPickResponse = webService1.PickAndPossiblyReleaseCaptureAndPossiblyPack(
				new[] { new PickLinesToPickedPackTypeInfo(new[] { pickLine1.PK.ToGuid(), pickLine2.PK.ToGuid(), pickLine3.PK.ToGuid(), pickLine4.PK.ToGuid() }, Array.Empty<PickedPackTypeInfo>()) },
				new PickingInfo(1, false),
				null,
				false);

			AssertEquals("Pre-condition: Pick was marked as shorted.", true, pick.HasShortfallItems);
			AssertEquals("Pre-condition: Order1 and Order2 was shorted.", 11m, order1.WD_UnitsSent + order2.WD_UnitsSent);
			AssertEquals("Pre-condition: 4 PK of shorted orderLines were returned.", 4, shortPickResponse.ShortedOrderLinePKs.Length);
			AssertContainsExactElementsInAnyOrder("Pre-condition: PK of orderLines were returned.",
				new[] { orderLine1.PK.ToGuid(), orderLine2.PK.ToGuid(), orderLine3.PK.ToGuid(), orderLine4.PK.ToGuid() },
				shortPickResponse.ShortedOrderLinePKs);

			using (Globals.SetIsUserInteractiveForTest(false))
			{
				var webService2 = GetNewWebService(data.Whs1, staff);
				var response = webService2.ReallocateShortPickedOrderLines(
					pick.PK.ToGuid(),
					new[] { orderLine1.PK.ToGuid(), orderLine2.PK.ToGuid(), orderLine3.PK.ToGuid(), orderLine4.PK.ToGuid() }, Guid.Empty);

				AssertEquals("2 Pick Line Group returned after reallocation", 2, response.Pick.Lines.Count);

				var pickLineGroup1 = response.Pick.Lines.Single(g => g.PKs.Length == 4);
				AssertEquals("pickLineGroup1 contains 4 Pick Lines", 4, pickLineGroup1.PKs.Length);
				AssertEquals("pickLineGroup1 has 4+5+5+5=19 units to pick.", 19m, pickLineGroup1.Units);

				var pickLineGroup2 = response.Pick.Lines.Single(g => g.PKs.Length == 1);
				AssertEquals("pickLineGroup2 wasn't picked or shorted, still have 10 units to pick.", 10m, pickLineGroup2.Units);
			}
		}

		#endregion

		#region TestReallocateShortPickedOrderLines_NoEnoughStockToReallocate

		public void TestReallocateShortPickedOrderLines_NoEnoughStockToReallocate()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);

			var staff = Helper.CreateGlbStaff("OP1", "Test1");
			Helper.Factory.Save();

			var loc1 = data.Whs1.FindLocation("A-1");
			var loc2 = data.Whs1.FindLocation("A-2");

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receiveLine1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 5m, loc1);
			var receiveLine2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 5m, loc2);
			receive.FinaliseDocketWithoutUserConfirmation();
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "Order1", Notify);
			var orderLine1 = Helper.CreateWhsOrderLine(order, data.Part1, 5m);

			var pick = Helper.CreatePickNew(order);
			var pickLine1 = orderLine1.PickLines.Single();
			AssignPickLine(pickLine1, staff);
			Helper.Factory.Save();

			var webService1 = GetNewWebService(data.Whs1, staff);
			var shortPickResponse = webService1.PickAndPossiblyReleaseCaptureAndPossiblyPack(
				new[] { new PickLinesToPickedPackTypeInfo(new[] { pickLine1.PK.ToGuid() }, Array.Empty<PickedPackTypeInfo>()) },
				new PickingInfo(1, false),
				null,
				true);

			AssertEquals("Pre-condition: Pickline1 was shorted.", 1m, pickLine1.WZ_Units);
			AssertEquals("Pre-condition: Pick was marked as shorted.", true, pick.HasShortfallItems);
			AssertEquals("Pre-condition: Order was shorted.", 1m, order.WD_UnitsSent);
			AssertEquals("Pre-condition: 1 shorted OrderLinePK returned.", 1, shortPickResponse.ShortedOrderLinePKs.Length);
			AssertEquals("Pre-condition: PK of orderLine1 was returned.", orderLine1.PK, shortPickResponse.ShortedOrderLinePKs[0]);

			var holdOrder = Helper.CreateWhsHoldOrder(data.Whs1, data.Org1);
			Helper.CreateWhsHoldOrderLine(holdOrder, data.Part1, fromHoldCode: "", toHoldCode: InventoryHoldCodes.Codes.Damaged, quantity: 3m);
			holdOrder.Finalise();
			Helper.Factory.Save();

			using (Globals.SetIsUserInteractiveForTest(false))
			{
				var webService2 = GetNewWebService(data.Whs1, staff);
				var response = webService2.ReallocateShortPickedOrderLines(pick.PK.ToGuid(), new[] { orderLine1.PK.ToGuid() }, Guid.Empty);

				var newPickeLine = response.Pick.Lines.Find(l => !l.PKs.Contains(pickLine1.PK.ToGuid()));
				AssertEquals("1 Pick Line Group returned after reallocation.", 1, response.Pick.Lines.Count);
				AssertEquals("1 unit picked, 4 units shorted, 3 units damaged, only 2 unit can be reallocated.", 2m, newPickeLine.Units);
			}
		}

		#endregion

		#region TestReallocateShortPickedOrderLines_NoStockReallocated

		public void TestReallocateShortPickedOrderLines_NoStockReallocated()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory);

			var staff = Helper.CreateGlbStaff("OP1", "Test1");
			Helper.Factory.Save();

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "Order1", Notify);
			var orderLine1 = Helper.CreateWhsOrderLine(order, data.Part1, 10m);

			var pick = Helper.CreatePickNew(order);
			var pickLine1 = orderLine1.PickLines.Single();
			AssignPickLine(pickLine1, staff);
			Helper.Factory.Save();

			var webService1 = GetNewWebService(data.Whs1, staff);
			var shortPickResponse = webService1.PickAndPossiblyReleaseCaptureAndPossiblyPack(
				new[] { new PickLinesToPickedPackTypeInfo(new[] { pickLine1.PK.ToGuid() }, Array.Empty<PickedPackTypeInfo>()) },
				new PickingInfo(0, false),
				null,
				true);

			AssertEquals("Pre-condition: Pick was marked as shorted.", true, pick.HasShortfallItems);
			AssertEquals("Pre-condition: Order was shorted.", 0m, order.WD_UnitsSent);
			AssertEquals("Pre-condition: 1 shorted OrderLinePK returned.", 1, shortPickResponse.ShortedOrderLinePKs.Length);
			AssertEquals("Pre-condition: PK of orderLine1 was returned.", orderLine1.PK, shortPickResponse.ShortedOrderLinePKs[0]);

			using (Globals.SetIsUserInteractiveForTest(false))
			{
				var webService2 = GetNewWebService(data.Whs1, staff);
				var response = webService2.ReallocateShortPickedOrderLines(pick.PK.ToGuid(), new[] { orderLine1.PK.ToGuid() }, Guid.Empty);

				AssertEquals("Error returned in response.", ErrorTypes.BusinessValidationError, response.Error);
				AssertEquals("Error returned in response.", "No stock could be reallocated for the shorted products.", response.ErrorMessage);
			}
		}

		#endregion

		#region TestReallocateShortPickedOrderLines_Failed

		public void TestReallocateShortPickedOrderLines_Failed()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory);

			var staff = Helper.CreateGlbStaff("OP1", "Test1");
			Helper.Factory.Save();

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "Order1", Notify);
			var orderLine1 = Helper.CreateWhsOrderLine(order, data.Part1, 5m);

			var pick = Helper.CreatePickNew(order);
			var pickLine1 = orderLine1.PickLines.Single();
			AssignPickLine(pickLine1, staff);
			Helper.Factory.Save();

			var webService1 = GetNewWebService(data.Whs1, staff);
			var shortPickResponse = webService1.PickAndPossiblyReleaseCaptureAndPossiblyPack(
				new[] { new PickLinesToPickedPackTypeInfo(new[] { pickLine1.PK.ToGuid() }, Array.Empty<PickedPackTypeInfo>()) },
				new PickingInfo(1, false),
				null,
				true);

			AssertEquals("Pre-condition: Pickline1 was shorted.", 1m, pickLine1.WZ_Units);
			AssertEquals("Pre-condition: Pick was marked as shorted.", true, pick.HasShortfallItems);
			AssertEquals("Pre-condition: Order was shorted.", 1m, order.WD_UnitsSent);
			AssertEquals("Pre-condition: 1 shorted OrderLinePK returned.", 1, shortPickResponse.ShortedOrderLinePKs.Length);
			AssertEquals("Pre-condition: PK of orderLine1 was returned.", orderLine1.PK, shortPickResponse.ShortedOrderLinePKs[0]);

			var allocationEngineMock = new Mock<IAllocationEngineManager>();
			allocationEngineMock.
				Setup(ae =>
					ae.Allocate(It.IsAny<WhsPick>(), It.IsAny<INotifications>(), It.IsNotNull<IPickStrategy>(), It.IsAny<IEnumerable<WhsPickOrderedInventory>>()))
				.Returns<WhsPick, INotifications, IPickStrategy, IEnumerable<WhsPickOrderedInventory>>((p, n, ps, ordInv) => AllocationResult.ErrorOrWarning)
				.Callback((WhsPick p, INotifications n, IPickStrategy ps, IEnumerable<WhsPickOrderedInventory> poi) =>
				{
					n.AddError("Some allocation error.");
				});

			using (ObjectFactory.Substitute(allocationEngineMock.Object))
			using (Globals.SetIsUserInteractiveForTest(false))
			{
				var webService2 = GetNewWebService(data.Whs1, staff);
				var response = webService2.ReallocateShortPickedOrderLines(pick.PK.ToGuid(), new[] { orderLine1.PK.ToGuid() }, Guid.Empty);

				AssertEquals("Error returned in response.", ErrorTypes.BusinessValidationError, response.Error);
				AssertEquals("Error returned in response.", "Some allocation error.", response.ErrorMessage.TrimEnd());
			}
		}

		#endregion

		#region TestReallocateShortPickedOrderLines_PickNotFound

		public void TestReallocateShortPickedOrderLines_PickNotFound()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			var staff = Helper.CreateGlbStaff("OP1", "Test1");
			Helper.Factory.Save();

			var webService2 = GetNewWebService(data.Whs1, staff);
			var response = webService2.ReallocateShortPickedOrderLines(new Guid(), new[] { new Guid() }, Guid.Empty);

			AssertEquals("Error returned in response.", ErrorTypes.BusinessValidationError, response.Error);
			AssertEquals("Pick was not found.", response.ErrorMessage);
		}

		#endregion

		#region TestReallocateShortPickedOrderLines_ShortedOrderedInventoriesNotFound

		public void TestReallocateShortPickedOrderLines_ShortedOrderedInventoriesNotFound()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			var staff = Helper.CreateGlbStaff("OP1", "Test1");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "Order1", Notify);
			var orderLine1 = Helper.CreateWhsOrderLine(order, data.Part1, 5m);
			var pick = Helper.CreatePickNew(order);
			Helper.Factory.Save();

			var webService2 = GetNewWebService(data.Whs1, staff);
			var response = webService2.ReallocateShortPickedOrderLines(pick.PK.ToGuid(), new[] { new Guid() }, Guid.Empty);

			AssertEquals("Error returned in response.", ErrorTypes.BusinessValidationError, response.Error);
			AssertEquals("Shorted Ordered Inventories not found.", response.ErrorMessage);
		}

		#endregion

		#region TestReallocateShortPickedOrderLines_DBHits

		public void TestReallocateShortPickedOrderLines_DBHits()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);

			var staff = Helper.CreateGlbStaff("OP1", "Test1");
			Helper.Factory.Save();

			var loc1 = data.Whs1.FindLocation("A-1");
			var loc2 = data.Whs1.FindLocation("A-2");

			var products = new List<OrgSupplierPart>(100);
			for (int i = 0; i < 100; i++)
			{
				var product = Helper.CreateProduct(data.Org1, "PP" + i);
				products.Add(product);

				var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R" + i);
				Helper.CreateWhsReceiveInventoryLine(receive, product, 1m, loc1);
				Helper.CreateWhsReceiveInventoryLine(receive, product, 1m, loc2);
				receive.FinaliseDocketWithoutUserConfirmation();
			}
			Helper.Factory.Save();

			var orders = new List<WhsPickableDocket>(100);
			var orderLines = new List<WhsPickableDocketLine>(100);
			for (int i = 0; i < 100; i++)
			{
				var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O" + i, Notify);
				var orderLine = Helper.CreateWhsOrderLine(order, products[i], 1m);
				orders.Add(order);
				orderLines.Add(orderLine);
			}

			var pick = Helper.CreatePickNew(orders.ToArray());
			pick.GetAllPickLines().ForEach(l => AssignPickLine(l, staff));
			Helper.Factory.Save();

			var pickLinePKs = pick.GetAllPickLines().Select(l => l.PK.ToGuid());
			var webService1 = GetNewWebService(data.Whs1, staff);
			var shortPickResponse = webService1.PickAndPossiblyReleaseCaptureAndPossiblyPack(
				new[] { new PickLinesToPickedPackTypeInfo(pickLinePKs.ToArray(), Array.Empty<PickedPackTypeInfo>()) },
				new PickingInfo(0, false),
				null,
				true);

			AssertEquals("Pre-condition: Pick was marked as shorted.", true, pick.HasShortfallItems);

			var expectedDbHits = new Dictionary<string, int>()
			{
				{ GlbBranchSchema.Constants.TableName, 1 },
				{ GlbCompanySchema.Constants.TableName, 1 },
				{ JobDocAddressSchema.Constants.TableName, 2 },
				{ OrgAddressSchema.Constants.TableName, 2 },
				{ OrgHeaderSchema.Constants.TableName, 1 },
				{ OrgMiscServSchema.Constants.TableName, 1 },
				{ OrgPartRelationSchema.Constants.TableName, 2 },
				{ OrgPartUnitSchema.Constants.TableName, 2 },
				{ OrgSupplierPartSchema.Constants.TableName, 2 },
				{ OrgSupplierPartBarcodeSchema.Constants.TableName, 2 },
				{ PkgPackageSchema.Constants.TableName, 2 },
				{ PkgPackageJobSchema.Constants.TableName, 2 },
				{ ProcessCompanyLinkRuleSchema.Constants.TableName, 1 },
				{ ProcessTasksSchema.Constants.TableName, 3 },
				{ ProcessTaskTemplateSchema.Constants.TableName, 2 },
				{ StmEventSchema.Constants.TableName, 1 },
				{ ProductionRuleSchema.Constants.TableName, 1 },
				{ WhsAreaSchema.Constants.TableName, 1 },
				{ WhsDocketSchema.Constants.TableName, 4 },
				{ WhsDocketLineSchema.Constants.TableName, 6 },
				{ WhsInventoryViewSchema.Constants.TableName, 1 },
				{ WhsLocationViewSchema.Constants.TableName, 3 },
				{ WhsPickSchema.Constants.TableName, 1 },
				{ WhsRowSchema.Constants.TableName, 1 },
				{ WhsPickLineSchema.Constants.TableName, 6 },
				{ StmNoteSchema.Constants.TableName, 2 },
				{ WhsClientPickPackParamsByWhsSchema.Constants.TableName, 1 },
			};

			var webService2 = GetNewWebService(data.Whs1, staff);
			using (TestCaseWithFactory.AssertDbHitsWithUsefulQueryInformation(expectedDbHits, webService2.Factory))
			using (Globals.SetIsUserInteractiveForTest(false))
			{
				var response = webService2.ReallocateShortPickedOrderLines(pick.PK.ToGuid(), orderLines.Select(o => o.PK.ToGuid()).ToArray(), Guid.Empty);
				AssertEquals(ErrorTypes.None, response.Error);
			}
		}

		#endregion

		#region TestReallocateShortPickedOrderLines_FactoryConcurrencySaveError

		public void TestReallocateShortPickedOrderLines_FactoryConcurrencySaveError()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);

			var staff = Helper.CreateGlbStaff("OP1", "Test1");
			Helper.Factory.Save();

			var loc1 = data.Whs1.FindLocation("A-1");
			var loc2 = data.Whs1.FindLocation("A-2");

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receiveLine1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 5m, loc1);
			var receiveLine2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 5m, loc2);
			receive.FinaliseDocketWithoutUserConfirmation();
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "Order1", Notify);
			var orderLine1 = Helper.CreateWhsOrderLine(order, data.Part1, 5m);

			var pick = Helper.CreatePickNew(order);
			var pickLine1 = orderLine1.PickLines.Single();
			AssignPickLine(pickLine1, staff);
			Helper.Factory.Save();

			var webService1 = GetNewWebService(data.Whs1, staff);
			var shortPickResponse = webService1.PickAndPossiblyReleaseCaptureAndPossiblyPack(
				new[] { new PickLinesToPickedPackTypeInfo(new[] { pickLine1.PK.ToGuid() }, Array.Empty<PickedPackTypeInfo>()) },
				new PickingInfo(0, false),
				null,
				true);

			AssertEquals("Pre-condition: Pick was marked as shorted.", true, pick.HasShortfallItems);
			AssertEquals("Pre-condition: Order was shorted.", 0m, order.WD_UnitsSent);
			AssertEquals("Pre-condition: 1 shorted OrderLinePK returned.", 1, shortPickResponse.ShortedOrderLinePKs.Length);
			AssertEquals("Pre-condition: PK of orderLine1 was returned.", orderLine1.PK, shortPickResponse.ShortedOrderLinePKs[0]);

			using (Globals.SetIsUserInteractiveForTest(false))
			{
				var webService2 = GetNewWebService(data.Whs1, staff);
				var innerException = new Exception();
				var concurrencyException = new ZDataConcurrencyException(innerException, ((IBusinessObjectInternals)pickLine1).Row, TestConnection);
				webService2.Factory.Saving += f => throw new ZSaveConcurrencyException(concurrencyException, Helper.Factory);

				var response = webService2.ReallocateShortPickedOrderLines(pick.PK.ToGuid(), new[] { orderLine1.PK.ToGuid() }, Guid.Empty);
				AssertEquals("ZSaveConcurrencyException should be logged as an Error.", "Another user has been assigned to or modified this Job. Please restart the operation and try again.", response.ErrorMessage);
			}
		}

		#endregion

		#region AssignPickLine

		void AssignPickLine(WhsPickLine pickLine, GlbStaff staff)
		{
			pickLine.WZ_GS_NKAssignedTo = staff.GS_Code;
			pickLine.WZ_IsPicking = true;
		}

		#endregion

		#region TestReallocateShortPickedOrderLines_ShortPickByBOMComponentLine

		public void TestReallocateShortPickedOrderLines_ShortPickByBOMComponentLine_SingleLine_FullyReallocated()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var staff = Helper.CreateGlbStaff("OP1", "Test1");
			Helper.Factory.Save();

			var bike = Helper.CreateProduct(data.Org1, "BIKE");
			bike.OP_IsComponentPickedOnSalesOrder = true;
			var wheel = Helper.CreateProduct(data.Org1, "WHEEL");
			Helper.CreateProductBOM(bike, wheel, 2m, PkgUnit.Unit);

			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveInventoryLine(receive1, wheel, 10m, data.Whs1.FindLocation("A-1"));
			receive1.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive1);

			var pick = Helper.Factory.New<WhsPick>();
			pick.WP_WW_Whs = data.Whs1.PK;
			pick.WP_WL_DockDoor = data.Whs1.WW_DefaultInboundDockDoor;
			pick.WP_PickOption = WhsPickOption.Codes.Manual;
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1", pickOption: WhsPickOption.Codes.Manual);
			var orderLine = Helper.CreateWhsOrderLine(order, bike, 5m);

			pick.AddOrders(new[] { order });
			pick.AutoAllocateItemsWithMock(); // Mock rules with FIFO, we are testing the higher level BOM parts
			var componentOrderLine = orderLine.ChildComponentLines.Single();

			Helper.Factory.Save();
			AssertEquals("Added component orderline should exist.", 1, orderLine.ChildComponentLines.Count);
			AssertEquals("Should be allocated.", 1, orderLine.ChildComponentLines.First().PickLines.Count);
			var componentPickLine = orderLine.ChildComponentLines.First().PickLines[0];
			AssertEquals("Allocated 10 wheels.", 10m, componentPickLine.WZ_Units);

			var createdReceive = Helper.Factory.Load<WhsReceive>(new ZQuery(WhsDocketSchema.WD_WP_ParentPickForReceive, pick.PK)).Single();
			AssertCreatedReceive(createdReceive, order);
			var createdReceiveLine1 = createdReceive.Lines[0];
			AssertCreatedReceiveLine(createdReceiveLine1, bike, 5m);
			var createdPickLines = Helper.Factory.Load<WhsPickLine>(new ZQuery(WhsPickLineSchema.WZ_WE_InventoryLine, createdReceiveLine1.PK));
			AssertEquals("Should be committed to relevant Order Line through PickLine.", 1, createdPickLines.Length);
			var relatedPickLine = createdPickLines[0];
			AssertCreatedPickLine(relatedPickLine, createdReceiveLine1, orderLine, 5m);
			AssertEquals("Links created.", 1, createdReceiveLine1.BOMComponentLinks.Count());
			var link = createdReceiveLine1.BOMComponentLinks.First();
			AssertCreatedLink(link, createdReceiveLine1, (WhsOrderLine)order.AllLines.Single(l => l.WE_OP == wheel.PK), 10m);

			var receive2 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R2");
			Helper.CreateWhsReceiveInventoryLine(receive2, wheel, 10m, data.Whs1.FindLocation("A-2"));
			receive2.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive2);
			Helper.Factory.Save();

			var webService1 = GetNewWebService(data.Whs1, staff);
			webService1.PickAndPossiblyReleaseCaptureAndPossiblyPack(
				new[] { new PickLinesToPickedPackTypeInfo(new[] { componentPickLine.PK.ToGuid() }, Array.Empty<PickedPackTypeInfo>()) },
				new PickingInfo(0m, false),
				null,
				true);
			AssertEquals("Nothing picked.", true, componentPickLine.IsDeleted);

			var newFactory = new BusinessObjectFactory();
			var reloadedReceive = newFactory.Load<WhsReceive>(new ZQuery(WhsDocketSchema.WD_WP_ParentPickForReceive, pick.PK)).Single();
			AssertEquals("Receive Line deleted due to Short Picking.", 0, reloadedReceive.Lines.Cast<WhsReceiveLine>().Count());
			orderLine = newFactory.Load<WhsOrderLine>(orderLine.PK);
			AssertEquals("Pick Line deleted due to Short Picking.", 0, orderLine.PickLines.Count);
			var linkQuery = new ZQuery(WhsBOMInventoryPivotSchema.WIP_WE_ComponentLine, orderLine.ChildComponentLines.Select(l => l.PK).ToArray());
			AssertEquals("BOM Inventory Links deleted due to Short Picking.", 0, newFactory.Load<WhsBOMInventoryPivot>(linkQuery).Length);

			using (Globals.SetIsUserInteractiveForTest(false))
			{
				var webService2 = GetNewWebService(data.Whs1, staff);
				var response = webService2.ReallocateShortPickedOrderLines(pick.PK.ToGuid(), new[] { componentOrderLine.PK.ToGuid() }, Guid.Empty);

				AssertEquals("1 Pick Line Group returned after reallocation.", 1, response.Pick.Lines.Count);
				AssertEquals("The reallocated PickLine has 10 units to pick.", 10m, response.Pick.Lines[0].Units);

				newFactory = new BusinessObjectFactory();
				reloadedReceive = newFactory.Load<WhsReceive>(new ZQuery(WhsDocketSchema.WD_WP_ParentPickForReceive, pick.PK)).Single();
				var reloadedReceiveLine = reloadedReceive.Lines.Cast<WhsReceiveLine>().Single();
				var reloadedPickLine = newFactory.Load<WhsPickLine>(new ZQuery(WhsPickLineSchema.WZ_WE_InventoryLine, reloadedReceiveLine.PK)).Single();
				var reloadedLink = reloadedReceiveLine.BOMComponentLinks.Single(l => l.WIP_WE_ComponentLine == componentOrderLine.PK);
				AssertCreatedReceive(reloadedReceive, order);
				AssertCreatedReceiveLine(reloadedReceiveLine, bike, 5m);
				AssertCreatedPickLine(reloadedPickLine, reloadedReceiveLine, orderLine, 5m);
				AssertCreatedLink(reloadedLink, reloadedReceiveLine, (WhsOrderLine)order.AllLines.Single(l => l.WE_OP == wheel.PK), 10m);
			}
		}

		public void TestReallocateShortPickedOrderLines_ShortPickByBOMComponentLine_SingleLine_NoEnoughStockToReallocate()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var staff = Helper.CreateGlbStaff("OP1", "Test1");
			Helper.Factory.Save();

			var bike = Helper.CreateProduct(data.Org1, "BIKE");
			bike.OP_IsComponentPickedOnSalesOrder = true;
			var wheel = Helper.CreateProduct(data.Org1, "WHEEL");
			Helper.CreateProductBOM(bike, wheel, 2m, PkgUnit.Unit);

			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveInventoryLine(receive1, wheel, 10m, data.Whs1.FindLocation("A-1"));
			receive1.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive1);

			var pick = Helper.Factory.New<WhsPick>();
			pick.WP_WW_Whs = data.Whs1.PK;
			pick.WP_WL_DockDoor = data.Whs1.WW_DefaultInboundDockDoor;
			pick.WP_PickOption = WhsPickOption.Codes.Manual;
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1", pickOption: WhsPickOption.Codes.Manual);
			var orderLine = Helper.CreateWhsOrderLine(order, bike, 5m);

			pick.AddOrders(new[] { order });
			pick.AutoAllocateItemsWithMock(); // Mock rules with FIFO, we are testing the higher level BOM parts
			var componentOrderLine = orderLine.ChildComponentLines.Single();

			Helper.Factory.Save();
			AssertEquals("Added component orderline should exist.", 1, orderLine.ChildComponentLines.Count);
			AssertEquals("Should be allocated.", 1, orderLine.ChildComponentLines.First().PickLines.Count);
			var componentPickLine = orderLine.ChildComponentLines.First().PickLines[0];
			AssertEquals("Allocated 10 wheels.", 10m, componentPickLine.WZ_Units);

			var createdReceive = Helper.Factory.Load<WhsReceive>(new ZQuery(WhsDocketSchema.WD_WP_ParentPickForReceive, pick.PK)).Single();
			AssertCreatedReceive(createdReceive, order);
			var createdReceiveLine1 = createdReceive.Lines[0];
			AssertCreatedReceiveLine(createdReceiveLine1, bike, 5m);
			var createdPickLines = Helper.Factory.Load<WhsPickLine>(new ZQuery(WhsPickLineSchema.WZ_WE_InventoryLine, createdReceiveLine1.PK));
			AssertEquals("Should be committed to relevant Order Line through PickLine.", 1, createdPickLines.Length);
			var relatedPickLine = createdPickLines[0];
			AssertCreatedPickLine(relatedPickLine, createdReceiveLine1, orderLine, 5m);
			AssertEquals("Links created.", 1, createdReceiveLine1.BOMComponentLinks.Count());
			var link = createdReceiveLine1.BOMComponentLinks.First();
			AssertCreatedLink(link, createdReceiveLine1, (WhsOrderLine)order.AllLines.Single(l => l.WE_OP == wheel.PK), 10m);

			var receive2 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R2");
			Helper.CreateWhsReceiveInventoryLine(receive2, wheel, 5m, data.Whs1.FindLocation("A-2"));
			receive2.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive2);
			Helper.Factory.Save();

			var webService1 = GetNewWebService(data.Whs1, staff);
			webService1.PickAndPossiblyReleaseCaptureAndPossiblyPack(
				new[] { new PickLinesToPickedPackTypeInfo(new[] { componentPickLine.PK.ToGuid() }, Array.Empty<PickedPackTypeInfo>()) },
				new PickingInfo(0m, false),
				null,
				true);
			AssertEquals("Nothing picked.", true, componentPickLine.IsDeleted);

			var newFactory = new BusinessObjectFactory();
			var reloadedReceive = newFactory.Load<WhsReceive>(new ZQuery(WhsDocketSchema.WD_WP_ParentPickForReceive, pick.PK)).Single();
			AssertEquals("Receive Line deleted due to Short Picking.", 0, reloadedReceive.Lines.Cast<WhsReceiveLine>().Count());
			orderLine = newFactory.Load<WhsOrderLine>(orderLine.PK);
			AssertEquals("Pick Line deleted due to Short Picking.", 0, orderLine.PickLines.Count);
			var linkQuery = new ZQuery(WhsBOMInventoryPivotSchema.WIP_WE_ComponentLine, orderLine.ChildComponentLines.Select(l => l.PK).ToArray());
			AssertEquals("BOM Inventory Links deleted due to Short Picking.", 0, newFactory.Load<WhsBOMInventoryPivot>(linkQuery).Length);

			using (Globals.SetIsUserInteractiveForTest(false))
			{
				var webService2 = GetNewWebService(data.Whs1, staff);
				var response = webService2.ReallocateShortPickedOrderLines(pick.PK.ToGuid(), new[] { componentOrderLine.PK.ToGuid() }, Guid.Empty);

				AssertEquals("1 Pick Line Group returned after reallocation.", 1, response.Pick.Lines.Count);
				AssertEquals("The reallocated PickLine has 5 units to pick.", 5m, response.Pick.Lines[0].Units);

				newFactory = new BusinessObjectFactory();
				reloadedReceive = newFactory.Load<WhsReceive>(new ZQuery(WhsDocketSchema.WD_WP_ParentPickForReceive, pick.PK)).Single();
				var reloadedReceiveLine = reloadedReceive.Lines.Cast<WhsReceiveLine>().Single();
				var reloadedPickLine = newFactory.Load<WhsPickLine>(new ZQuery(WhsPickLineSchema.WZ_WE_InventoryLine, reloadedReceiveLine.PK)).Single();
				var reloadedLink = reloadedReceiveLine.BOMComponentLinks.Single(l => l.WIP_WE_ComponentLine == componentOrderLine.PK);
				AssertCreatedReceive(reloadedReceive, order);
				AssertCreatedReceiveLine(reloadedReceiveLine, bike, 2m);
				AssertCreatedPickLine(reloadedPickLine, reloadedReceiveLine, orderLine, 2m);
				AssertCreatedLink(reloadedLink, reloadedReceiveLine, (WhsOrderLine)order.AllLines.Single(l => l.WE_OP == wheel.PK), 4m);
			}
		}

		public void TestReallocateShortPickedOrderLines_ShortPickByBOMComponentLine_SingleLine_NoEnoughStockToReallocateToBuildAKit()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var staff = Helper.CreateGlbStaff("OP1", "Test1");
			Helper.Factory.Save();

			var bike = Helper.CreateProduct(data.Org1, "BIKE");
			bike.OP_IsComponentPickedOnSalesOrder = true;
			var wheel = Helper.CreateProduct(data.Org1, "WHEEL");
			Helper.CreateProductBOM(bike, wheel, 2m, PkgUnit.Unit);

			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveInventoryLine(receive1, wheel, 10m, data.Whs1.FindLocation("A-1"));
			receive1.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive1);

			var pick = Helper.Factory.New<WhsPick>();
			pick.WP_WW_Whs = data.Whs1.PK;
			pick.WP_WL_DockDoor = data.Whs1.WW_DefaultInboundDockDoor;
			pick.WP_PickOption = WhsPickOption.Codes.Manual;
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1", pickOption: WhsPickOption.Codes.Manual);
			var orderLine = Helper.CreateWhsOrderLine(order, bike, 5m);

			pick.AddOrders(new[] { order });
			pick.AutoAllocateItemsWithMock(); // Mock rules with FIFO, we are testing the higher level BOM parts
			var componentOrderLine = orderLine.ChildComponentLines.Single();

			Helper.Factory.Save();
			AssertEquals("Added component orderline should exist.", 1, orderLine.ChildComponentLines.Count);
			AssertEquals("Should be allocated.", 1, orderLine.ChildComponentLines.First().PickLines.Count);
			var componentPickLine = orderLine.ChildComponentLines.First().PickLines[0];
			AssertEquals("Allocated 10 wheels.", 10m, componentPickLine.WZ_Units);

			var createdReceive = Helper.Factory.Load<WhsReceive>(new ZQuery(WhsDocketSchema.WD_WP_ParentPickForReceive, pick.PK)).Single();
			AssertCreatedReceive(createdReceive, order);
			var createdReceiveLine1 = createdReceive.Lines[0];
			AssertCreatedReceiveLine(createdReceiveLine1, bike, 5m);
			var createdPickLines = Helper.Factory.Load<WhsPickLine>(new ZQuery(WhsPickLineSchema.WZ_WE_InventoryLine, createdReceiveLine1.PK));
			AssertEquals("Should be committed to relevant Order Line through PickLine.", 1, createdPickLines.Length);
			var relatedPickLine = createdPickLines[0];
			AssertCreatedPickLine(relatedPickLine, createdReceiveLine1, orderLine, 5m);
			AssertEquals("Links created.", 1, createdReceiveLine1.BOMComponentLinks.Count());
			var link = createdReceiveLine1.BOMComponentLinks.First();
			AssertCreatedLink(link, createdReceiveLine1, (WhsOrderLine)order.AllLines.Single(l => l.WE_OP == wheel.PK), 10m);

			var receive2 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R2");
			Helper.CreateWhsReceiveInventoryLine(receive2, wheel, 1m, data.Whs1.FindLocation("A-2"));
			receive2.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive2);
			Helper.Factory.Save();

			var webService1 = GetNewWebService(data.Whs1, staff);
			webService1.PickAndPossiblyReleaseCaptureAndPossiblyPack(
				new[] { new PickLinesToPickedPackTypeInfo(new[] { componentPickLine.PK.ToGuid() }, Array.Empty<PickedPackTypeInfo>()) },
				new PickingInfo(0m, false),
				null,
				true);
			AssertEquals("Nothing picked.", true, componentPickLine.IsDeleted);

			var newFactory = new BusinessObjectFactory();
			var reloadedReceive = newFactory.Load<WhsReceive>(new ZQuery(WhsDocketSchema.WD_WP_ParentPickForReceive, pick.PK)).Single();
			AssertEquals("Receive Line deleted due to Short Picking.", 0, reloadedReceive.Lines.Cast<WhsReceiveLine>().Count());
			orderLine = newFactory.Load<WhsOrderLine>(orderLine.PK);
			AssertEquals("Pick Line deleted due to Short Picking.", 0, orderLine.PickLines.Count);
			var linkQuery = new ZQuery(WhsBOMInventoryPivotSchema.WIP_WE_ComponentLine, orderLine.ChildComponentLines.Select(l => l.PK).ToArray());
			AssertEquals("BOM Inventory Links deleted due to Short Picking.", 0, newFactory.Load<WhsBOMInventoryPivot>(linkQuery).Length);

			using (Globals.SetIsUserInteractiveForTest(false))
			{
				var webService2 = GetNewWebService(data.Whs1, staff);
				var response = webService2.ReallocateShortPickedOrderLines(pick.PK.ToGuid(), new[] { componentOrderLine.PK.ToGuid() }, Guid.Empty);

				AssertEquals("1 Pick Line Group returned after reallocation.", 1, response.Pick.Lines.Count);
				AssertEquals("The reallocated PickLine has 1 unit to pick.", 1m, response.Pick.Lines[0].Units);

				newFactory = new BusinessObjectFactory();
				reloadedReceive = newFactory.Load<WhsReceive>(new ZQuery(WhsDocketSchema.WD_WP_ParentPickForReceive, pick.PK)).Single();
				AssertEquals("Not enough stock rellocated to build a kit so no new Receive Lines.", 0, reloadedReceive.Lines.Cast<WhsReceiveLine>().Count());
				orderLine = newFactory.Load<WhsOrderLine>(orderLine.PK);
				AssertEquals("Not enough stock rellocated to build a kit so no new Pick Lines.", 0, orderLine.PickLines.Count);
				AssertEquals("Not enough stock rellocated to build a kit so no new Links.", 0, newFactory.Load<WhsBOMInventoryPivot>(linkQuery).Length);
			}
		}

		public void TestReallocateShortPickedOrderLines_ShortPickByBOMComponentLine_SingleLine_NoStockReallocated()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var staff = Helper.CreateGlbStaff("OP1", "Test1");
			Helper.Factory.Save();

			var bike = Helper.CreateProduct(data.Org1, "BIKE");
			bike.OP_IsComponentPickedOnSalesOrder = true;
			var wheel = Helper.CreateProduct(data.Org1, "WHEEL");
			Helper.CreateProductBOM(bike, wheel, 2m, PkgUnit.Unit);

			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveInventoryLine(receive1, wheel, 10m, data.Whs1.FindLocation("A-1"));
			receive1.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive1);

			var pick = Helper.Factory.New<WhsPick>();
			pick.WP_WW_Whs = data.Whs1.PK;
			pick.WP_WL_DockDoor = data.Whs1.WW_DefaultInboundDockDoor;
			pick.WP_PickOption = WhsPickOption.Codes.Manual;
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1", pickOption: WhsPickOption.Codes.Manual);
			var orderLine = Helper.CreateWhsOrderLine(order, bike, 5m);

			pick.AddOrders(new[] { order });
			pick.AutoAllocateItemsWithMock(); // Mock rules with FIFO, we are testing the higher level BOM parts
			var componentOrderLine = orderLine.ChildComponentLines.Single();

			Helper.Factory.Save();
			AssertEquals("Added component orderline should exist.", 1, orderLine.ChildComponentLines.Count);
			AssertEquals("Should be allocated.", 1, orderLine.ChildComponentLines.First().PickLines.Count);
			var componentPickLine = orderLine.ChildComponentLines.First().PickLines[0];
			AssertEquals("Allocated 10 wheels.", 10m, componentPickLine.WZ_Units);

			var createdReceive = Helper.Factory.Load<WhsReceive>(new ZQuery(WhsDocketSchema.WD_WP_ParentPickForReceive, pick.PK)).Single();
			AssertCreatedReceive(createdReceive, order);
			var createdReceiveLine1 = createdReceive.Lines[0];
			AssertCreatedReceiveLine(createdReceiveLine1, bike, 5m);
			var createdPickLines = Helper.Factory.Load<WhsPickLine>(new ZQuery(WhsPickLineSchema.WZ_WE_InventoryLine, createdReceiveLine1.PK));
			AssertEquals("Should be committed to relevant Order Line through PickLine.", 1, createdPickLines.Length);
			var relatedPickLine = createdPickLines[0];
			AssertCreatedPickLine(relatedPickLine, createdReceiveLine1, orderLine, 5m);
			AssertEquals("Links created.", 1, createdReceiveLine1.BOMComponentLinks.Count());
			var link = createdReceiveLine1.BOMComponentLinks.First();
			AssertCreatedLink(link, createdReceiveLine1, (WhsOrderLine)order.AllLines.Single(l => l.WE_OP == wheel.PK), 10m);

			var webService1 = GetNewWebService(data.Whs1, staff);
			webService1.PickAndPossiblyReleaseCaptureAndPossiblyPack(
				new[] { new PickLinesToPickedPackTypeInfo(new[] { componentPickLine.PK.ToGuid() }, Array.Empty<PickedPackTypeInfo>()) },
				new PickingInfo(0m, false),
				null,
				true);
			AssertEquals("Nothing picked.", true, componentPickLine.IsDeleted);

			var newFactory = new BusinessObjectFactory();
			var reloadedReceive = newFactory.Load<WhsReceive>(new ZQuery(WhsDocketSchema.WD_WP_ParentPickForReceive, pick.PK)).Single();
			AssertEquals("Receive Line deleted due to Short Picking.", 0, reloadedReceive.Lines.Cast<WhsReceiveLine>().Count());
			AssertEquals("Pick Line deleted due to Short Picking.", 0, orderLine.PickLines.Count);
			var linkQuery = new ZQuery(WhsBOMInventoryPivotSchema.WIP_WE_ComponentLine, orderLine.ChildComponentLines.Select(l => l.PK).ToArray());
			AssertEquals("BOM Inventory Links deleted due to Short Picking.", 0, newFactory.Load<WhsBOMInventoryPivot>(linkQuery).Length);

			using (Globals.SetIsUserInteractiveForTest(false))
			{
				var webService2 = GetNewWebService(data.Whs1, staff);
				var response = webService2.ReallocateShortPickedOrderLines(pick.PK.ToGuid(), new[] { componentOrderLine.PK.ToGuid() }, Guid.Empty);

				AssertEquals("No Pick Line Group returned after reallocation.", 0, response.Pick.Lines.Count);

				newFactory = new BusinessObjectFactory();
				reloadedReceive = newFactory.Load<WhsReceive>(new ZQuery(WhsDocketSchema.WD_WP_ParentPickForReceive, pick.PK)).Single();
				AssertEquals("Nothing rellocated so no new Receive Lines.", 0, reloadedReceive.Lines.Cast<WhsReceiveLine>().Count());
				orderLine = newFactory.Load<WhsOrderLine>(orderLine.PK);
				AssertEquals("Nothing rellocated so no new Pick Lines.", 0, orderLine.PickLines.Count);
				AssertEquals("Nothing rellocated so no new Links.", 0, newFactory.Load<WhsBOMInventoryPivot>(linkQuery).Length);
			}
		}

		public void TestReallocateShortPickedOrderLines_ShortPickByBOMComponentLine_SingleLine_Failed()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var staff = Helper.CreateGlbStaff("OP1", "Test1");
			Helper.Factory.Save();

			var bike = Helper.CreateProduct(data.Org1, "BIKE");
			bike.OP_IsComponentPickedOnSalesOrder = true;
			var wheel = Helper.CreateProduct(data.Org1, "WHEEL");
			Helper.CreateProductBOM(bike, wheel, 2m, PkgUnit.Unit);

			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveInventoryLine(receive1, wheel, 10m, data.Whs1.FindLocation("A-1"));
			receive1.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive1);

			var pick = Helper.Factory.New<WhsPick>();
			pick.WP_WW_Whs = data.Whs1.PK;
			pick.WP_WL_DockDoor = data.Whs1.WW_DefaultInboundDockDoor;
			pick.WP_PickOption = WhsPickOption.Codes.Manual;
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1", pickOption: WhsPickOption.Codes.Manual);
			var orderLine = Helper.CreateWhsOrderLine(order, bike, 5m);

			pick.AddOrders(new[] { order });
			pick.AutoAllocateItemsWithMock(); // Mock rules with FIFO, we are testing the higher level BOM parts
			var componentOrderLine = orderLine.ChildComponentLines.Single();

			Helper.Factory.Save();
			AssertEquals("Added component orderline should exist.", 1, orderLine.ChildComponentLines.Count);
			AssertEquals("Should be allocated.", 1, orderLine.ChildComponentLines.First().PickLines.Count);
			var componentPickLine = orderLine.ChildComponentLines.First().PickLines[0];
			AssertEquals("Allocated 10 wheels.", 10m, componentPickLine.WZ_Units);

			var createdReceive = Helper.Factory.Load<WhsReceive>(new ZQuery(WhsDocketSchema.WD_WP_ParentPickForReceive, pick.PK)).Single();
			AssertCreatedReceive(createdReceive, order);
			var createdReceiveLine1 = createdReceive.Lines[0];
			AssertCreatedReceiveLine(createdReceiveLine1, bike, 5m);
			var createdPickLines = Helper.Factory.Load<WhsPickLine>(new ZQuery(WhsPickLineSchema.WZ_WE_InventoryLine, createdReceiveLine1.PK));
			AssertEquals("Should be committed to relevant Order Line through PickLine.", 1, createdPickLines.Length);
			var relatedPickLine = createdPickLines[0];
			AssertCreatedPickLine(relatedPickLine, createdReceiveLine1, orderLine, 5m);
			AssertEquals("Links created.", 1, createdReceiveLine1.BOMComponentLinks.Count());
			var link = createdReceiveLine1.BOMComponentLinks.First();
			AssertCreatedLink(link, createdReceiveLine1, (WhsOrderLine)order.AllLines.Single(l => l.WE_OP == wheel.PK), 10m);

			var webService1 = GetNewWebService(data.Whs1, staff);
			webService1.PickAndPossiblyReleaseCaptureAndPossiblyPack(
				new[] { new PickLinesToPickedPackTypeInfo(new[] { componentPickLine.PK.ToGuid() }, Array.Empty<PickedPackTypeInfo>()) },
				new PickingInfo(0m, false),
				null,
				true);
			AssertEquals("Nothing picked.", true, componentPickLine.IsDeleted);

			var newFactory = new BusinessObjectFactory();
			var reloadedReceive = newFactory.Load<WhsReceive>(new ZQuery(WhsDocketSchema.WD_WP_ParentPickForReceive, pick.PK)).Single();
			AssertEquals("Receive Line deleted due to Short Picking.", 0, reloadedReceive.Lines.Cast<WhsReceiveLine>().Count());
			AssertEquals("Pick Line deleted due to Short Picking.", 0, orderLine.PickLines.Count);
			var linkQuery = new ZQuery(WhsBOMInventoryPivotSchema.WIP_WE_ComponentLine, orderLine.ChildComponentLines.Select(l => l.PK).ToArray());
			AssertEquals("BOM Inventory Links deleted due to Short Picking.", 0, newFactory.Load<WhsBOMInventoryPivot>(linkQuery).Length);

			var allocationEngineMock = new Mock<IAllocationEngineManager>();
			allocationEngineMock
				.Setup(ae =>
					ae.Allocate(It.IsAny<WhsPick>(), It.IsAny<INotifications>(), It.IsNotNull<IPickStrategy>(), It.IsAny<IEnumerable<WhsPickOrderedInventory>>()))
				.Returns<WhsPick, INotifications, IPickStrategy, IEnumerable<WhsPickOrderedInventory>>((p, n, ps, ordInv) => AllocationResult.ErrorOrWarning)
				.Callback((WhsPick p, INotifications n, IPickStrategy ps, IEnumerable<WhsPickOrderedInventory> poi) =>
				{
					n.AddError("Some allocation error.");
				});

			using (ObjectFactory.Substitute(allocationEngineMock.Object))
			using (Globals.SetIsUserInteractiveForTest(false))
			{
				var webService2 = GetNewWebService(data.Whs1, staff);
				var response = webService2.ReallocateShortPickedOrderLines(pick.PK.ToGuid(), new[] { componentOrderLine.PK.ToGuid() }, Guid.Empty);

				AssertEquals("Error returned in response.", ErrorTypes.BusinessValidationError, response.Error);
				AssertEquals("Error returned in response.", "Some allocation error.", response.ErrorMessage.TrimEnd());

				newFactory = new BusinessObjectFactory();
				reloadedReceive = newFactory.Load<WhsReceive>(new ZQuery(WhsDocketSchema.WD_WP_ParentPickForReceive, pick.PK)).Single();
				AssertEquals("Nothing rellocated so no new Receive Lines.", 0, reloadedReceive.Lines.Cast<WhsReceiveLine>().Count());
				orderLine = newFactory.Load<WhsOrderLine>(orderLine.PK);
				AssertEquals("Nothing rellocated so no new Pick Lines.", 0, orderLine.PickLines.Count);
				AssertEquals("Nothing rellocated so no new Links.", 0, newFactory.Load<WhsBOMInventoryPivot>(linkQuery).Length);
			}
		}

		public void TestReallocateShortPickedOrderLines_ShortPickByBOMComponentLine_OtherComponentsWereShorted()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var staff = Helper.CreateGlbStaff("OP1", "Test1");
			Helper.Factory.Save();

			var bike = Helper.CreateProduct(data.Org1, "BIKE");
			bike.OP_IsComponentPickedOnSalesOrder = true;
			var wheel = Helper.CreateProduct(data.Org1, "WHEEL");
			var frame = Helper.CreateProduct(data.Org1, "FRAME");
			Helper.CreateProductBOM(bike, wheel, 2m, PkgUnit.Unit);
			Helper.CreateProductBOM(bike, frame, 1m, PkgUnit.Unit);

			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveInventoryLine(receive1, wheel, 10m, data.Whs1.FindLocation("A-1"));
			Helper.CreateWhsReceiveInventoryLine(receive1, frame, 10m, data.Whs1.FindLocation("A-1"));
			receive1.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive1);

			var pick = Helper.Factory.New<WhsPick>();
			pick.WP_WW_Whs = data.Whs1.PK;
			pick.WP_WL_DockDoor = data.Whs1.WW_DefaultInboundDockDoor;
			pick.WP_PickOption = WhsPickOption.Codes.Manual;
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1", pickOption: WhsPickOption.Codes.Manual);
			var orderLine = Helper.CreateWhsOrderLine(order, bike, 5m);

			pick.AddOrders(new[] { order });
			pick.AutoAllocateItemsWithMock(); // Mock rules with FIFO, we are testing the higher level BOM parts
			Helper.Factory.Save();

			var receive2 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R2");
			Helper.CreateWhsReceiveInventoryLine(receive2, wheel, 10m, data.Whs1.FindLocation("A-2"));
			receive2.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive2);
			Helper.Factory.Save();

			AssertEquals("Added component orderline should exist.", 2, orderLine.ChildComponentLines.Count);
			var wheelComponentLine = orderLine.ChildComponentLines.Single(l => l.WE_OP == wheel.PK);
			var frameComponentLine = orderLine.ChildComponentLines.Single(l => l.WE_OP == frame.PK);
			AssertEquals("Should be allocated.", 1, orderLine.ChildComponentLines.First().PickLines.Count);
			var wheelPickLine = orderLine.ChildComponentLines.Single(l => l.WE_OP == wheel.PK).PickLines[0];
			var framePickLine = orderLine.ChildComponentLines.Single(l => l.WE_OP == frame.PK).PickLines[0];
			AssertEquals("Allocated 10 wheels.", 10m, wheelPickLine.WZ_Units);
			AssertEquals("Allocated 5 frames.", 5m, framePickLine.WZ_Units);

			var createdReceive = Helper.Factory.LoadTop1<WhsReceive>(new ZQuery(WhsDocketSchema.WD_WP_ParentPickForReceive, pick.PK));
			AssertCreatedReceive(createdReceive, order);
			var createdReceiveLine1 = createdReceive.Lines[0];
			AssertCreatedReceiveLine(createdReceiveLine1, bike, 5m);
			var createdPickLines = Helper.Factory.Load<WhsPickLine>(new ZQuery(WhsPickLineSchema.WZ_WE_InventoryLine, createdReceiveLine1.PK));
			AssertEquals("Should be committed to relevant Order Line through PickLine.", 1, createdPickLines.Length);
			var relatedPickLine = createdPickLines[0];
			AssertCreatedPickLine(relatedPickLine, createdReceiveLine1, orderLine, 5m);
			AssertEquals("Links created.", 2, createdReceiveLine1.BOMComponentLinks.Count());
			var wheelLink = createdReceiveLine1.BOMComponentLinks.Single(l => l.WIP_ComponentQuantity == 10m);
			var frameLink = createdReceiveLine1.BOMComponentLinks.Single(l => l.WIP_ComponentQuantity == 5m);
			AssertCreatedLink(wheelLink, createdReceiveLine1, (WhsOrderLine)order.AllLines.Single(l => l.WE_OP == wheel.PK), 10m);
			AssertCreatedLink(frameLink, createdReceiveLine1, (WhsOrderLine)order.AllLines.Single(l => l.WE_OP == frame.PK), 5m);

			// short frames
			var webService1 = GetNewWebService(data.Whs1, staff);
			webService1.PickAndPossiblyReleaseCaptureAndPossiblyPack(
				new[] { new PickLinesToPickedPackTypeInfo(new[] { framePickLine.PK.ToGuid() }, Array.Empty<PickedPackTypeInfo>()) },
				new PickingInfo(3m, false),
				null,
				true);
			AssertEquals("Picked 3 frames.", 3m, framePickLine.WZ_Units);

			var newFactory = new BusinessObjectFactory();
			var reloadedReceive = newFactory.Load<WhsReceive>(new ZQuery(WhsDocketSchema.WD_WP_ParentPickForReceive, pick.PK)).Single();
			var reloadedReceiveLine = reloadedReceive.Lines.Cast<WhsReceiveLine>().Single();
			var reloadedPickLine = newFactory.Load<WhsPickLine>(new ZQuery(WhsPickLineSchema.WZ_WE_InventoryLine, reloadedReceiveLine.PK)).Single();
			var reloadedWheelLink = createdReceiveLine1.BOMComponentLinks.Single(l => l.WIP_WE_ComponentLine == wheelComponentLine.PK);
			var reloadedFrameLink = createdReceiveLine1.BOMComponentLinks.Single(l => l.WIP_WE_ComponentLine == frameComponentLine.PK);
			AssertCreatedReceive(reloadedReceive, order);
			AssertCreatedReceiveLine(reloadedReceiveLine, bike, 3m);
			AssertCreatedPickLine(reloadedPickLine, reloadedReceiveLine, orderLine, 3m);
			AssertCreatedLink(reloadedWheelLink, reloadedReceiveLine, (WhsOrderLine)order.AllLines.Single(l => l.WE_OP == wheel.PK), 6m);
			AssertCreatedLink(reloadedFrameLink, reloadedReceiveLine, (WhsOrderLine)order.AllLines.Single(l => l.WE_OP == frame.PK), 3m);

			// short wheels
			var webService2 = GetNewWebService(data.Whs1, staff);
			webService2.PickAndPossiblyReleaseCaptureAndPossiblyPack(
				new[] { new PickLinesToPickedPackTypeInfo(new[] { wheelPickLine.PK.ToGuid() }, Array.Empty<PickedPackTypeInfo>()) },
				new PickingInfo(8m, false),
				null,
				true);
			AssertEquals("Picked 8 wheels.", 8m, wheelPickLine.WZ_Units);

			newFactory = new BusinessObjectFactory();
			reloadedReceive = newFactory.Load<WhsReceive>(new ZQuery(WhsDocketSchema.WD_WP_ParentPickForReceive, pick.PK)).Single();
			reloadedReceiveLine = reloadedReceive.Lines.Cast<WhsReceiveLine>().Single();
			reloadedPickLine = newFactory.Load<WhsPickLine>(new ZQuery(WhsPickLineSchema.WZ_WE_InventoryLine, reloadedReceiveLine.PK)).Single();
			reloadedWheelLink = createdReceiveLine1.BOMComponentLinks.Single(l => l.WIP_WE_ComponentLine == wheelComponentLine.PK);
			reloadedFrameLink = createdReceiveLine1.BOMComponentLinks.Single(l => l.WIP_WE_ComponentLine == frameComponentLine.PK);
			AssertCreatedReceive(reloadedReceive, order);
			AssertCreatedReceiveLine(reloadedReceiveLine, bike, 3m);
			AssertCreatedPickLine(reloadedPickLine, reloadedReceiveLine, orderLine, 3m);
			AssertCreatedLink(reloadedWheelLink, reloadedReceiveLine, (WhsOrderLine)order.AllLines.Single(l => l.WE_OP == wheel.PK), 6m);
			AssertCreatedLink(reloadedFrameLink, reloadedReceiveLine, (WhsOrderLine)order.AllLines.Single(l => l.WE_OP == frame.PK), 3m);

			using (Globals.SetIsUserInteractiveForTest(false))
			{
				// reallocate wheels
				var webService3 = GetNewWebService(data.Whs1, staff);
				var response = webService2.ReallocateShortPickedOrderLines(pick.PK.ToGuid(), new[] { wheelComponentLine.PK.ToGuid() }, Guid.Empty);

				AssertEquals("1 Pick Line Group returned after reallocation.", 1, response.Pick.Lines.Count);
				AssertEquals("The reallocated PickLine has 2 units to pick.", 2m, response.Pick.Lines[0].Units);

				newFactory = new BusinessObjectFactory();
				reloadedReceive = newFactory.Load<WhsReceive>(new ZQuery(WhsDocketSchema.WD_WP_ParentPickForReceive, pick.PK)).Single();
				reloadedReceiveLine = reloadedReceive.Lines.Cast<WhsReceiveLine>().Single();
				reloadedPickLine = newFactory.Load<WhsPickLine>(new ZQuery(WhsPickLineSchema.WZ_WE_InventoryLine, reloadedReceiveLine.PK)).Single();
				reloadedWheelLink = createdReceiveLine1.BOMComponentLinks.Single(l => l.WIP_WE_ComponentLine == wheelComponentLine.PK);
				reloadedFrameLink = createdReceiveLine1.BOMComponentLinks.Single(l => l.WIP_WE_ComponentLine == frameComponentLine.PK);
				AssertCreatedReceive(reloadedReceive, order);
				AssertCreatedReceiveLine(reloadedReceiveLine, bike, 3m);
				AssertCreatedPickLine(reloadedPickLine, reloadedReceiveLine, orderLine, 3m);
				AssertCreatedLink(reloadedWheelLink, reloadedReceiveLine, (WhsOrderLine)order.AllLines.Single(l => l.WE_OP == wheel.PK), 6m);
				AssertCreatedLink(reloadedFrameLink, reloadedReceiveLine, (WhsOrderLine)order.AllLines.Single(l => l.WE_OP == frame.PK), 3m);
			}
		}

		public void TestReallocateShortPickedOrderLines_ShortPickByBOMComponentLine_ReallocatedInventoryAreCommittedToDifferentOrderLines()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var staff = Helper.CreateGlbStaff("OP1", "Test1");
			Helper.Factory.Save();

			var bike = Helper.CreateProduct(data.Org1, "BIKE");
			bike.OP_IsComponentPickedOnSalesOrder = true;
			var wheel = Helper.CreateProduct(data.Org1, "WHEEL");
			Helper.CreateProductBOM(bike, wheel, 2m, PkgUnit.Unit);

			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveInventoryLine(receive1, wheel, 10m, data.Whs1.FindLocation("A-1"));
			receive1.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive1);

			var pick = Helper.Factory.New<WhsPick>();
			pick.WP_WW_Whs = data.Whs1.PK;
			pick.WP_WL_DockDoor = data.Whs1.WW_DefaultInboundDockDoor;
			pick.WP_PickOption = WhsPickOption.Codes.Manual;
			Helper.Factory.Save();

			var order1 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1", pickOption: WhsPickOption.Codes.Manual);
			var orderLine1 = Helper.CreateWhsOrderLine(order1, bike, 2m);

			var order2 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O2", pickOption: WhsPickOption.Codes.Manual);
			var orderLine2 = Helper.CreateWhsOrderLine(order2, bike, 3m);

			pick.AddOrders(new[] { order1, order2 });
			pick.AutoAllocateItemsWithMock(); // Mock rules with FIFO, we are testing the higher level BOM parts
			Helper.Factory.Save();
			AssertEquals("Added component orderline should exist.", 1, orderLine1.ChildComponentLines.Count);
			AssertEquals("Added component orderline should exist.", 1, orderLine2.ChildComponentLines.Count);
			var componentOrderLine1 = orderLine1.ChildComponentLines.Single();
			var componentOrderLine2 = orderLine2.ChildComponentLines.Single();
			AssertEquals("Should be allocated.", 1, orderLine1.ChildComponentLines.Single().PickLines.Count);
			AssertEquals("Should be allocated.", 1, orderLine2.ChildComponentLines.Single().PickLines.Count);
			var componentPickLine1 = orderLine1.ChildComponentLines.Single().PickLines[0];
			var componentPickLine2 = orderLine2.ChildComponentLines.Single().PickLines[0];
			AssertEquals("Allocated 4 wheels.", 4m, componentPickLine1.WZ_Units);
			AssertEquals("Allocated 6 wheels.", 6m, componentPickLine2.WZ_Units);

			var createdReceive = Helper.Factory.Load<WhsReceive>(new ZQuery(WhsDocketSchema.WD_WP_ParentPickForReceive, pick.PK)).Single();
			AssertCreatedReceive(createdReceive, order1);
			var createdReceiveLines = createdReceive.Lines.Cast<WhsReceiveLine>().ToArray();
			AssertEquals("There are 1 Receive Line for each Kit Order Line.", 2, createdReceiveLines.Length);
			var createdReceiveLine1 = createdReceiveLines.Single(l => l.WE_TransactionQuantity == 2m);
			var createdReceiveLine2 = createdReceiveLines.Single(l => l.WE_TransactionQuantity == 3m);
			AssertCreatedReceiveLine(createdReceiveLine1, bike, 2m);
			AssertCreatedReceiveLine(createdReceiveLine2, bike, 3m);
			var createdPickLines1 = Helper.Factory.Load<WhsPickLine>(new ZQuery(WhsPickLineSchema.WZ_WE_InventoryLine, createdReceiveLine1.PK));
			AssertEquals("Should be committed to relevant Order Line through PickLine.", 1, createdPickLines1.Length);
			var createdPickLines2 = Helper.Factory.Load<WhsPickLine>(new ZQuery(WhsPickLineSchema.WZ_WE_InventoryLine, createdReceiveLine2.PK));
			AssertEquals("Should be committed to relevant Order Line through PickLine.", 1, createdPickLines2.Length);
			var relatedPickLine1 = createdPickLines1[0];
			var relatedPickLine2 = createdPickLines2[0];
			AssertCreatedPickLine(relatedPickLine1, createdReceiveLine1, orderLine1, 2m);
			AssertCreatedPickLine(relatedPickLine2, createdReceiveLine2, orderLine2, 3m);
			AssertEquals("Links created.", 1, createdReceiveLine1.BOMComponentLinks.Count());
			var link1 = createdReceiveLine1.BOMComponentLinks.First();
			AssertCreatedLink(link1, createdReceiveLine1, (WhsOrderLine)order1.AllLines.Single(l => l.WE_OP == wheel.PK), 4m);
			var link2 = createdReceiveLine2.BOMComponentLinks.First();
			AssertCreatedLink(link2, createdReceiveLine2, (WhsOrderLine)order2.AllLines.Single(l => l.WE_OP == wheel.PK), 6m);

			var receive2 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R2");
			Helper.CreateWhsReceiveInventoryLine(receive2, wheel, 10m, data.Whs1.FindLocation("A-2"));
			receive2.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive2);
			Helper.Factory.Save();

			var webService1 = GetNewWebService(data.Whs1, staff);
			webService1.PickAndPossiblyReleaseCaptureAndPossiblyPack(
				new[] { new PickLinesToPickedPackTypeInfo(new[] { componentPickLine1.PK.ToGuid(), componentPickLine2.PK.ToGuid() }, Array.Empty<PickedPackTypeInfo>()) },
				new PickingInfo(2m, false),
				null,
				true);
			var componentPickLines = orderLine1.ChildComponentLines.Single().PickLines.Concat(orderLine2.ChildComponentLines.Single().PickLines).ToArray();
			AssertEquals("1 Pick Line remained.", 1, componentPickLines.Length);
			AssertEquals("Picked 2 wheels.", 2m, componentPickLines[0].WZ_Units);
			AssertNotNull(componentPickLines[0].WZ_PickedDateTime);

			var newFactory = new BusinessObjectFactory();
			var reloadedReceive = newFactory.Load<WhsReceive>(new ZQuery(WhsDocketSchema.WD_WP_ParentPickForReceive, pick.PK)).Single();
			var reloadedReceiveLines = reloadedReceive.Lines.Cast<WhsReceiveLine>().ToArray();
			AssertEquals("1 Receive Line is deleted because of shorting the same inventory.", 1, reloadedReceiveLines.Length);
			var reloadedReceiveLine = reloadedReceiveLines[0];
			var reloadedPickLine = newFactory.Load<WhsPickLine>(new ZQuery(WhsPickLineSchema.WZ_WE_InventoryLine, reloadedReceiveLine.PK)).Single();
			var reloadedLink = reloadedReceiveLine.BOMComponentLinks.First();
			AssertCreatedReceive(reloadedReceive, order1);
			AssertCreatedReceiveLine(reloadedReceiveLine, bike, 1m);
			var reloadedOrderLine = newFactory.Load<WhsOrderLine>(orderLine2.PK);
			AssertCreatedPickLine(reloadedPickLine, reloadedReceiveLine, reloadedOrderLine, 1m);
			AssertCreatedLink(reloadedLink, reloadedReceiveLine, reloadedOrderLine.ChildComponentLines.Cast<WhsOrderLine>().Single(), 2m);

			using (Globals.SetIsUserInteractiveForTest(false))
			{
				var webService2 = GetNewWebService(data.Whs1, staff);
				var response = webService2.ReallocateShortPickedOrderLines(pick.PK.ToGuid(), new[] { componentOrderLine1.PK.ToGuid(), componentOrderLine2.PK.ToGuid() }, Guid.Empty);

				AssertEquals("1 Pick Line Group returned after reallocation.", 1, response.Pick.Lines.Count);
				AssertEquals("The reallocated PickLine has 8 units to pick.", 8m, response.Pick.Lines[0].Units);

				newFactory = new BusinessObjectFactory();
				reloadedReceive = newFactory.Load<WhsReceive>(new ZQuery(WhsDocketSchema.WD_WP_ParentPickForReceive, pick.PK)).Single();
				reloadedReceiveLines = reloadedReceive.Lines.Cast<WhsReceiveLine>().ToArray();
				AssertEquals("1 more Receive Line created.", 2, reloadedReceiveLines.Length);
				var reloadedReceiveLine1 = reloadedReceiveLines.Single(l => l.WE_TransactionQuantity == 2m);
				var reloadedReceiveLine2 = reloadedReceiveLines.Single(l => l.WE_TransactionQuantity == 3m);
				var reloadedPickLine1 = newFactory.Load<WhsPickLine>(new ZQuery(WhsPickLineSchema.WZ_WE_InventoryLine, reloadedReceiveLine1.PK)).Single();
				var reloadedPickLine2 = newFactory.Load<WhsPickLine>(new ZQuery(WhsPickLineSchema.WZ_WE_InventoryLine, reloadedReceiveLine2.PK)).Single();
				var reloadedLink1 = reloadedReceiveLine1.BOMComponentLinks.First();
				var reloadedLink2 = reloadedReceiveLine2.BOMComponentLinks.First();
				AssertCreatedReceive(reloadedReceive, order1);
				AssertCreatedReceiveLine(reloadedReceiveLine1, bike, 2m);
				AssertCreatedReceiveLine(reloadedReceiveLine2, bike, 3m);
				AssertCreatedPickLine(reloadedPickLine1, reloadedReceiveLine1, orderLine1, 2m);
				AssertCreatedPickLine(reloadedPickLine2, reloadedReceiveLine2, orderLine2, 3m);
				AssertCreatedLink(reloadedLink1, reloadedReceiveLine1, (WhsOrderLine)order1.AllLines.Single(l => l.WE_OP == wheel.PK), 4m);
				AssertCreatedLink(reloadedLink2, reloadedReceiveLine2, (WhsOrderLine)order2.AllLines.Single(l => l.WE_OP == wheel.PK), 6m);
			}
		}

		void AssertCreatedReceive(WhsReceive createdReceive, WhsOrder order)
		{
			AssertNotNull("Should created a new Receive.", createdReceive);
			AssertEquals(order.WD_WW_Whs, createdReceive.WD_WW_Whs);
			AssertEquals(order.WD_OH_Client, createdReceive.WD_OH_Client);
			AssertEquals((byte)0, createdReceive.WD_ExternalReferenceSplit);
			AssertEquals(0m, createdReceive.WD_TotalUnits);
			AssertEquals(0, createdReceive.WD_PackagesSent);
			AssertEquals((short)0, createdReceive.WD_TotalPallets);

			AssertNull("Should not create Billing Job for this Receive.", createdReceive.JobHeader);
		}

		void AssertCreatedReceiveLine(WhsReceiveLine createdReceiveLine1, OrgSupplierPart kit, decimal stockOnHand)
		{
			AssertEquals(kit.PK, createdReceiveLine1.WE_OP);
			AssertEquals(stockOnHand, createdReceiveLine1.WE_TransactionQuantity);
			AssertEquals(ZDateTimeOffset.Empty, createdReceiveLine1.WE_AdjustmentArrivalDate);
		}

		void AssertCreatedPickLine(WhsPickLine pickLine, WhsReceiveLine receiveLine, WhsOrderLine orderLine, ZDecimal units)
		{
			AssertEquals(orderLine.PK, pickLine.WZ_WE_TransactionLine);
			AssertEquals(receiveLine.PK, pickLine.WZ_WE_InventoryLine);
			AssertEquals(units, pickLine.WZ_Units);
			AssertEquals(receiveLine.WE_F3_NKPackType, pickLine.WZ_UnitsUQ);
			AssertEquals(0m, pickLine.WZ_OriginalReservedQty);
			AssertEquals(string.Empty, pickLine.WZ_ReleaseCapturedPartAttrib1);
			AssertEquals(string.Empty, pickLine.WZ_ReleaseCapturedPartAttrib2);
			AssertEquals(string.Empty, pickLine.WZ_ReleaseCapturedPartAttrib3);
			AssertEquals(string.Empty, pickLine.WZ_ReleaseCapturedSerialNumber);
		}

		void AssertCreatedLink(WhsBOMInventoryPivot link, WhsReceiveLine receiveLine, WhsOrderLine orderLine, decimal componentQuantity)
		{
			AssertEquals(orderLine.PK, link.WIP_WE_ComponentLine);
			AssertEquals(receiveLine.PK, link.WIP_WE_InventoryLine);
			AssertEquals(componentQuantity, link.WIP_ComponentQuantity);
		}

		#endregion

		#region TestReallocateShortPickedOrderLines_UpdatesTasksAppropriately

		public void TestReallocateShortPickedOrderLines_UpdatesTasksAppropriately()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 5, 1);

			var staff = Helper.CreateGlbStaff("OP1", "Test1");
			Helper.Factory.Save();

			var loc1 = data.Whs1.FindLocation("A-1");
			var loc2 = data.Whs1.FindLocation("A-2");
			var loc3 = data.Whs1.FindLocation("A-3");
			var loc4 = data.Whs1.FindLocation("A-4");

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 5m, loc1);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 5m, loc2);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 5m, loc3);
			receive.FinaliseDocketWithoutUserConfirmation();

			Helper.Factory.Save();

			var order1 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "Order1", Notify);
			var orderLine1 = Helper.CreateWhsOrderLine(order1, data.Part1, 5m);
			var orderLine2 = Helper.CreateWhsOrderLine(order1, data.Part1, 5m);
			var order2 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "Order2", Notify);
			var orderLine3 = Helper.CreateWhsOrderLine(order2, data.Part1, 5m);

			var pick = Helper.CreatePickNew(order1, order2);
			var pickLine1 = orderLine1.PickLines.Single();
			var pickLine2 = orderLine2.PickLines.Single();
			var pickLine3 = orderLine3.PickLines.Single();
			AssignPickLine(pickLine1, staff);
			AssignPickLine(pickLine2, staff);
			AssignPickLine(pickLine3, staff);
			Helper.Factory.Save();

			var receive2 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 15m, loc4, "PID");

			var task = Helper.CreateProcessTaskForPickJob(pick, staff);
			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1, staff);
			var shortPickResponse = webService.PickAndPossiblyReleaseCaptureAndPossiblyPack(
				new[] { new PickLinesToPickedPackTypeInfo(new[] { pickLine1.PK.ToGuid(), pickLine2.PK.ToGuid(), pickLine3.PK.ToGuid() }, Array.Empty<PickedPackTypeInfo>()) },
				new PickingInfo(0, false),
				null,
				false);

			AssertEquals("Pre-condition: Pick was marked as shorted.", true, pick.HasShortfallItems);
			AssertEquals("Pre-condition: Order1 and Order2 was shorted.", 0m, order1.WD_UnitsSent + order2.WD_UnitsSent);
			AssertEquals("Pre-condition: 3 PKs of shorted orderLines were returned.", 3, shortPickResponse.ShortedOrderLinePKs.Length);
			AssertContainsExactElementsInAnyOrder("Pre-condition: PK of orderLines were returned.",
				new[] { orderLine1.PK.ToGuid(), orderLine2.PK.ToGuid(), orderLine3.PK.ToGuid() },
				shortPickResponse.ShortedOrderLinePKs);

			using (Globals.SetIsUserInteractiveForTest(false))
			{
				var webService2 = GetNewWebService(data.Whs1, staff);
				var response = webService2.ReallocateShortPickedOrderLines(
					pick.PK.ToGuid(),
					new[] { orderLine1.PK.ToGuid(), orderLine2.PK.ToGuid(), orderLine3.PK.ToGuid() }, task.PK.ToGuid());

				AssertEquals("1 Pick Line Group returned after reallocation", 1, response.Pick.Lines.Count);

				var pickLineGroup = response.Pick.Lines.Single();
				AssertEquals("pickLineGroup1 contains 3 Pick Lines", 3, pickLineGroup.PKs.Length);
				AssertEquals("pickLineGroup1 has 15 units to pick.", 15m, pickLineGroup.Units);
			}

			var newFactory = new BusinessObjectFactory();
			var pickLinesForTask = newFactory.Load<WhsPickLine>(new ZQuery(WhsPickLineSchema.WZ_P9_Task, task.PK));
			AssertEquals(3, pickLinesForTask.Length);

			var pickInNewFactory = newFactory.Load<WhsPick>(pick.PK);
			var newPickLinesFromTask = pickLinesForTask.Select(p => p.PK);
			var newPickLinesFromPick = pickInNewFactory.GetAllPickLines().Select(l => l.PK);
			AssertContainsExactElementsInAnyOrder(newPickLinesFromTask, newPickLinesFromPick);
		}

		public void TestReallocateShortPickedOrderLines_BadTaskPK()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 5, 1);

			var staff = Helper.CreateGlbStaff("OP1", "Test1");
			Helper.Factory.Save();

			var loc1 = data.Whs1.FindLocation("A-1");
			var loc2 = data.Whs1.FindLocation("A-2");

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 5m, loc1);
			receive.FinaliseDocketWithoutUserConfirmation();

			Helper.Factory.Save();

			var order1 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "Order1", Notify);
			var orderLine1 = Helper.CreateWhsOrderLine(order1, data.Part1, 5m);

			var pick = Helper.CreatePickNew(order1);
			var pickLine1 = orderLine1.PickLines.Single();
			AssignPickLine(pickLine1, staff);
			Helper.Factory.Save();

			var receive2 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 5m, loc2, "PID");

			var task = Helper.CreateProcessTaskForPickJob(pick, staff);
			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1, staff);
			var shortPickResponse = webService.PickAndPossiblyReleaseCaptureAndPossiblyPack(
				new[] { new PickLinesToPickedPackTypeInfo([pickLine1.PK.ToGuid()], Array.Empty<PickedPackTypeInfo>()) },
				new PickingInfo(0, false),
				null,
				false);

			AssertEquals("Pre-condition: Pick was marked as shorted.", true, pick.HasShortfallItems);
			AssertContainsExactElementsInAnyOrder("Pre-condition: PK of orderLines were returned.",
				[orderLine1.PK.ToGuid()],
				shortPickResponse.ShortedOrderLinePKs);

			using (Globals.SetIsUserInteractiveForTest(false))
			{
				var webService2 = GetNewWebService(data.Whs1, staff);
				var response = webService2.ReallocateShortPickedOrderLines(
					pick.PK.ToGuid(),
					[orderLine1.PK.ToGuid()],
					Guid.NewGuid());

				AssertBusinessValidationError(webService2, "Task was not found.", response);
			}
		}

		#endregion

	}
}
