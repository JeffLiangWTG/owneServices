using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Warehouse.Integration.Warehouse;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	public class WhsPickOnSalesOrderDetectorTest : WhsTestCaseWithFactory
	{
		#region TestClassIsSpringedOut

		public void TestClassIsSpringedOut()
		{
			var detector = ObjectFactory.Get<IWhsPickOnSalesOrderDetector>();
			AssertNotNull(detector);
			Assert(detector is WhsPickOnSalesOrderDetector);
		}

		#endregion

		#region TestIsComponentUsedToBuiltKitOnSalesOrder_NoKitsCreated

		public void TestIsComponentUsedToBuiltKitOnSalesOrder_NoKitsCreated()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			data.Part1.OP_IsComponentPickedOnSalesOrder = true;
			Helper.CreateProductBOM(data.Part1, data.Part2, 4m, "UNT");

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part2, 100m);
			Factory.Save();
			// ordering just components
			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part2, 10m);
			var pick = Helper.CreatePickNew(order);
			AssertEquals("Precondition - pick allocated.", 1, pick.GetAllPickLines().Count());
			Factory.Save();

			var detector = ObjectFactory.Get<IWhsPickOnSalesOrderDetector>();
			Factory.ClearCachedValue<bool>("IsComponentUsedToBuiltKitOnSalesOrder|" + data.Part2.PK);
			AssertEquals("Components were picked directly - not as part of Kits. Should return false.", false, detector.IsComponentUsedToBuiltKitOnSalesOrder(Factory, data.Part2.PK));
		}

		#endregion

		#region TestIsComponentUsedToBuiltKitOnSalesOrder_KitsWereBuilt

		public void TestIsComponentUsedToBuiltKitOnSalesOrder_KitsWereBuilt()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			data.Part1.OP_IsComponentPickedOnSalesOrder = true;
			Helper.CreateProductBOM(data.Part1, data.Part2, 4m, "UNT");

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part2, 100m);
			Factory.Save();

			// ordering kits
			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 5m);
			var pick = Helper.CreatePickNew(order);
			AssertEquals("Precondition - pick allocated.", true, pick.GetAllPickLines().Any());
			AssertNotNull("Components were picked to build kits.", pick.GetAllPickLines().Single(l => l.DocketLine.WE_OP == data.Part2.PK));
			Factory.Save();

			var detector = ObjectFactory.Get<IWhsPickOnSalesOrderDetector>();
			Factory.ClearCachedValue<bool>("IsComponentUsedToBuiltKitOnSalesOrder|" + data.Part2.PK);
			AssertEquals("Components were used to create kits. Should return true.", true, detector.IsComponentUsedToBuiltKitOnSalesOrder(Factory, data.Part2.PK));

			Factory.ClearCachedValue<bool>("IsComponentUsedToBuiltKitOnSalesOrder|" + data.Part1.PK);
			AssertEquals("If you pass-in kit product - should return false.", false, detector.IsComponentUsedToBuiltKitOnSalesOrder(Factory, data.Part1.PK));
		}

		#endregion

		#region TestIsComponentUsedToBuiltKitOnSalesOrder_KitsWereOrderedButNotBuilt

		public void TestIsComponentUsedToBuiltKitOnSalesOrder_KitsWereOrderedButNotBuilt()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			data.Part1.OP_IsComponentPickedOnSalesOrder = true;
			Helper.CreateProductBOM(data.Part1, data.Part2, 4m, "UNT");

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 100m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part2, 100m);
			Factory.Save();

			// ordering kits
			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 5m);
			var pick = Helper.CreatePickNew(order);
			AssertEquals("Precondition - pick allocated.", 1, pick.GetAllPickLines().Count());
			AssertEquals("Kit was picked directly", data.Part1, pick.GetAllPickLines().Single().SupplierPart);
			Factory.Save();

			var detector = ObjectFactory.Get<IWhsPickOnSalesOrderDetector>();
			Factory.ClearCachedValue<bool>("IsComponentUsedToBuiltKitOnSalesOrder|" + data.Part2.PK);
			AssertEquals("Components were not used to create kits. Should return false.", false, detector.IsComponentUsedToBuiltKitOnSalesOrder(Factory, data.Part2.PK));

			Factory.ClearCachedValue<bool>("IsComponentUsedToBuiltKitOnSalesOrder|" + data.Part1.PK);
			AssertEquals("If you pass-in kit product - should return false.", false, detector.IsComponentUsedToBuiltKitOnSalesOrder(Factory, data.Part1.PK));
		}

		#endregion

		#region TestIsComponentUsedToBuiltKitOnSalesOrder_KitsWereBuilt_AllPicksWereFinalised

		public void TestIsComponentUsedToBuiltKitOnSalesOrder_KitsWereBuilt_AllPicksWereFinalised()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			data.Part1.OP_IsComponentPickedOnSalesOrder = true;
			Helper.CreateProductBOM(data.Part1, data.Part2, 4m, "UNT");

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part2, 100m);
			Factory.Save();

			// ordering kits
			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 5m);
			var pick = Helper.CreatePickNew(order);
			AssertEquals("Precondition - pick allocated.", true, pick.GetAllPickLines().Any());
			AssertNotNull("Components were picked to build kits.", pick.GetAllPickLines().Single(l => l.DocketLine.WE_OP == data.Part2.PK));
			Factory.Save();

			pick.GetAllPickLines().Where(pl => !pl.IsPickByBOMKitPickLine()).ForEach(l =>
			{
				l.WZ_PickedDateTime = ZDateTimeOffset.Now;
				l.WZ_GS_NKAssignedTo = "E";
			});

			Factory.Save();

			order.FinaliseDocketAlwaysFinalisingPick();
			Factory.Save();
			AssertIsFinalisedPrecondition(order);
			AssertIsFinalisedPrecondition(pick);

			var detector = ObjectFactory.Get<IWhsPickOnSalesOrderDetector>();
			Factory.ClearCachedValue<bool>("IsComponentUsedToBuiltKitOnSalesOrder|" + data.Part2.PK);
			AssertEquals("Components were used to create kits, but Pick has been finalised. Should return false.", false, detector.IsComponentUsedToBuiltKitOnSalesOrder(Factory, data.Part2.PK));

			Factory.ClearCachedValue<bool>("IsComponentUsedToBuiltKitOnSalesOrder|" + data.Part1.PK);
			AssertEquals("If you pass-in kit product - should return false.", false, detector.IsComponentUsedToBuiltKitOnSalesOrder(Factory, data.Part1.PK));
		}

		#endregion

		#region TestIsComponentUsedToBuiltKitOnSalesOrder_KitsWereBuilt_AllPicksWereFinalised_ButNotAllTransformedYet

		public void TestIsComponentUsedToBuiltKitOnSalesOrder_KitsWereBuilt_AllPicksWereFinalised_ButNotAllTransformedYet()
		{
			// This test can be deleted after the removal of the Transformation PopulateWhsPickByBOMData.cs
			var data = new TestDataSimpleEnvironment(Factory);
			data.Part1.OP_IsComponentPickedOnSalesOrder = true;
			Helper.CreateProductBOM(data.Part1, data.Part2, 4m, "UNT");

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part2, 100m);
			Factory.Save();

			// Create a Finalised Order with the old data shape.
			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 5m);
			var pick1 = Helper.CreatePickNew(order1);
			AssertEquals("Precondition - pick allocated.", true, pick1.GetAllPickLines().Any());
			AssertNotNull("Components were picked to build kits.", pick1.GetAllPickLines().Single(l => l.DocketLine.WE_OP == data.Part2.PK));
			Factory.Save();

			pick1.GetAllPickLines().Where(pl => !pl.IsPickByBOMKitPickLine()).ForEach(l =>
			{
				l.WZ_PickedDateTime = ZDateTimeOffset.Now;
				l.WZ_GS_NKAssignedTo = "E";
			});

			Factory.Save();

			order1.FinaliseDocketAlwaysFinalisingPick();
			MockOldDataShapeForFinalisedPick(order1);
			Factory.Save();
			AssertIsFinalisedPrecondition(order1);
			AssertIsFinalisedPrecondition(pick1);

			// Create a Finalised Order with the new data shape.
			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O2", data.Part1, 5m);
			var pick2 = Helper.CreatePickNew(order2);
			AssertEquals("Precondition - pick allocated.", true, pick2.GetAllPickLines().Any());
			AssertNotNull("Components were picked to build kits.", pick2.GetAllPickLines().Single(l => l.DocketLine.WE_OP == data.Part2.PK));
			Factory.Save();

			pick2.GetAllPickLines().Where(pl => !pl.IsPickByBOMKitPickLine()).ForEach(l =>
			{
				l.WZ_PickedDateTime = ZDateTimeOffset.Now;
				l.WZ_GS_NKAssignedTo = "E";
			});

			Factory.Save();

			order2.FinaliseDocketAlwaysFinalisingPick();
			Factory.Save();
			AssertIsFinalisedPrecondition(order2);
			AssertIsFinalisedPrecondition(pick2);

			var detector = ObjectFactory.Get<IWhsPickOnSalesOrderDetector>();
			Factory.ClearCachedValue<bool>("IsComponentUsedToBuiltKitOnSalesOrder|" + data.Part2.PK);
			AssertEquals("There are Finalised Picks in the old data shape.", true, detector.IsComponentUsedToBuiltKitOnSalesOrder(Factory, data.Part2.PK));

			Factory.ClearCachedValue<bool>("IsComponentUsedToBuiltKitOnSalesOrder|" + data.Part1.PK);
			AssertEquals("If you pass-in kit product - should return false.", false, detector.IsComponentUsedToBuiltKitOnSalesOrder(Factory, data.Part1.PK));
		}

		void MockOldDataShapeForFinalisedPick(WhsOrder order)
		{
			// we have to delete data to mock old data shape now
			var kitPickLine = order.Lines.Single(l => l.IsBOMProduct).PickLines.Single(pl => pl.IsPickByBOMKitPickLine());
			var kitInventoryLine = kitPickLine.InventoryLineForAvailableInventory;
			kitInventoryLine.Docket.Delete();
			kitPickLine.Delete();
		}

		#endregion

		#region TestIsComponentUsedToBuiltKitOnSalesOrder_KitsWereBuilt_SomePicksWereNotFinalised

		public void TestIsComponentUsedToBuiltKitOnSalesOrder_KitsWereBuilt_SomePicksWereNotFinalised()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			data.Part1.OP_IsComponentPickedOnSalesOrder = true;
			Helper.CreateProductBOM(data.Part1, data.Part2, 4m, "UNT");

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part2, 100m);
			Factory.Save();

			// ordering kits
			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);
			AssertEquals("Precondition - pick allocated.", true, pick.GetAllPickLines().Any());
			AssertNotNull("Components were picked to build kits.", pick.GetAllPickLines().Single(l => l.DocketLine.WE_OP == data.Part2.PK));
			Factory.Save();
			AssertEquals(false, pick.IsFinalised);

			var detector = ObjectFactory.Get<IWhsPickOnSalesOrderDetector>();

			Factory.ClearCachedValue<bool>("IsComponentUsedToBuiltKitOnSalesOrder|" + data.Part2.PK);
			AssertEquals("Kit was built on sales oreder. Should return true.", true, detector.IsComponentUsedToBuiltKitOnSalesOrder(Factory, data.Part2.PK));

			Factory.ClearCachedValue<bool>("IsComponentUsedToBuiltKitOnSalesOrder|" + data.Part1.PK);
			AssertEquals("For Kit product should return false.", false, detector.IsComponentUsedToBuiltKitOnSalesOrder(Factory, data.Part1.PK));
		}

		#endregion

		#region TestIsComponentUsedToBuiltKitOnSalesOrder_WorkOrder

		public void TestIsComponentUsedToBuiltKitOnSalesOrder_WorkOrder()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			data.Part1.OP_IsComponentPickedOnSalesOrder = true;
			Helper.CreateProductBOM(data.Part1, data.Part2, 4m, "UNT");

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part2, 100m);
			Factory.Save();

			// trying to assemble 5 units of Part1
			var workOrder = Helper.CreateWhsWorkOrderWithLine(data.Org1, data.Whs1, data.Part1, 5m);
			var pick = Helper.CreatePickNew(workOrder);
			AssertEquals("Precondition - pick allocated.", 1, pick.GetAllPickLines().Count());
			Factory.Save();

			var detector = ObjectFactory.Get<IWhsPickOnSalesOrderDetector>();
			Factory.ClearCachedValue<bool>("IsComponentUsedToBuiltKitOnSalesOrder|" + data.Part2.PK);
			AssertEquals("Components used in workorder. Should return false.", false, detector.IsComponentUsedToBuiltKitOnSalesOrder(Factory, data.Part2.PK));
		}

		#endregion

		#region TestCachesValuesInFactory

		public void TestCachesValuesInFactory()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			data.Part1.OP_IsComponentPickedOnSalesOrder = true;
			Helper.CreateProductBOM(data.Part1, data.Part2, 4m, "UNT");

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part2, 100m);
			Factory.Save();
			var detector = ObjectFactory.Get<IWhsPickOnSalesOrderDetector>();
			AssertEquals("At this time there are no orders at all.", false, detector.IsComponentUsedToBuiltKitOnSalesOrder(Factory, data.Part2.PK));

			// ordering kits
			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 5m);
			var pick = Helper.CreatePickNew(order);
			AssertEquals("Precondition - pick allocated.", true, pick.GetAllPickLines().Any());
			Factory.Save();

			AssertEquals("Previous result already stored in cache, should return false.", false, detector.IsKitBuiltOnSalesOrder(Factory, data.Part1.PK));
			AssertEquals("Previous result already stored in cache, should return false.", false, detector.IsComponentUsedToBuiltKitOnSalesOrder(Factory, data.Part2.PK));

			var anotherFactory = new BusinessObjectFactory();
			AssertEquals("Other factory has no cache and new value is calculated.", true, detector.IsKitBuiltOnSalesOrder(anotherFactory, data.Part1.PK));
			AssertEquals("Other factory has no cache and new value is calculated.", true, detector.IsComponentUsedToBuiltKitOnSalesOrder(anotherFactory, data.Part2.PK));
		}

		#endregion

		#region TestIsKitBuiltOnSalesOrder_NoKitsCreated

		public void TestIsKitBuiltOnSalesOrder_NoKitsCreated()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			data.Part1.OP_IsComponentPickedOnSalesOrder = true;
			Helper.CreateProductBOM(data.Part1, data.Part2, 4m, "UNT");

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part2, 100m);
			Factory.Save();
			// ordering just components
			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part2, 10m);
			var pick = Helper.CreatePickNew(order);
			AssertEquals("Precondition - pick allocated.", 1, pick.GetAllPickLines().Count());
			Factory.Save();

			var detector = ObjectFactory.Get<IWhsPickOnSalesOrderDetector>();

			Factory.ClearCachedValue<bool>("IsKitBuiltOnSalesOrder|" + data.Part1.PK);
			AssertEquals("Kit wasn't built. Should return false.", false, detector.IsKitBuiltOnSalesOrder(Factory, data.Part1.PK));

			Factory.ClearCachedValue<bool>("IsKitBuiltOnSalesOrder|" + data.Part2.PK);
			AssertEquals("For component product should return false.", false, detector.IsKitBuiltOnSalesOrder(Factory, data.Part2.PK));
		}

		#endregion

		#region TestIsKitBuiltOnSalesOrder_KitsWereBuilt

		public void TestIsKitBuiltOnSalesOrder_KitsWereBuilt()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			data.Part1.OP_IsComponentPickedOnSalesOrder = true;
			Helper.CreateProductBOM(data.Part1, data.Part2, 4m, "UNT");

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part2, 100m);
			Factory.Save();

			// ordering kits
			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);
			AssertEquals("Precondition - pick allocated.", true, pick.GetAllPickLines().Any());
			AssertNotNull("Components were picked to build kits.", pick.GetAllPickLines().Single(l => l.DocketLine.WE_OP == data.Part2.PK));
			Factory.Save();

			var detector = ObjectFactory.Get<IWhsPickOnSalesOrderDetector>();

			Factory.ClearCachedValue<bool>("IsKitBuiltOnSalesOrder|" + data.Part1.PK);
			AssertEquals("Kit was built on sales oreder. Should return true.", true, detector.IsKitBuiltOnSalesOrder(Factory, data.Part1.PK));

			Factory.ClearCachedValue<bool>("IsKitBuiltOnSalesOrder|" + data.Part2.PK);
			AssertEquals("For component product should return false.", false, detector.IsKitBuiltOnSalesOrder(Factory, data.Part2.PK));
		}

		#endregion

		#region TestIsKitBuiltOnSalesOrder_KitsWereOrderedButNotBuilt

		public void TestIsKitBuiltOnSalesOrder_KitsWereOrderedButNotBuilt()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			data.Part1.OP_IsComponentPickedOnSalesOrder = true;
			Helper.CreateProductBOM(data.Part1, data.Part2, 4m, "UNT");

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 100m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part2, 100m);
			Factory.Save();

			// ordering kits
			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 5m);
			var pick = Helper.CreatePickNew(order);
			AssertEquals("Precondition - pick allocated.", 1, pick.GetAllPickLines().Count());
			AssertEquals("Kit was picked directly", data.Part1, pick.GetAllPickLines().Single().SupplierPart);
			Factory.Save();

			var detector = ObjectFactory.Get<IWhsPickOnSalesOrderDetector>();

			Factory.ClearCachedValue<bool>("IsKitBuiltOnSalesOrder|" + data.Part1.PK);
			AssertEquals("Kit was picked directly - not built.", false, detector.IsKitBuiltOnSalesOrder(Factory, data.Part1.PK));

			Factory.ClearCachedValue<bool>("IsKitBuiltOnSalesOrder|" + data.Part2.PK);
			AssertEquals("For component product should return false.", false, detector.IsKitBuiltOnSalesOrder(Factory, data.Part2.PK));
		}

		#endregion

		#region TestIsKitBuiltOnSalesOrder_WorkOrder

		public void TestIsKitBuiltOnSalesOrder_WorkOrder()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			data.Part1.OP_IsComponentPickedOnSalesOrder = true;
			Helper.CreateProductBOM(data.Part1, data.Part2, 4m, "UNT");

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part2, 100m);
			Factory.Save();

			// trying to assemble 5 units of Part1
			var workOrder = Helper.CreateWhsWorkOrderWithLine(data.Org1, data.Whs1, data.Part1, 5m);
			var pick = Helper.CreatePickNew(workOrder);
			AssertEquals("Precondition - pick allocated.", 1, pick.GetAllPickLines().Count());
			Factory.Save();

			var detector = ObjectFactory.Get<IWhsPickOnSalesOrderDetector>();
			Factory.ClearCachedValue<bool>("IsKitBuiltOnSalesOrder|" + data.Part1.PK);
			AssertEquals("Components used in workorder. Should return false.", false, detector.IsKitBuiltOnSalesOrder(Factory, data.Part1.PK));
		}

		#endregion

		#region TestIsKitBuiltOnSalesOrder_AllPicksWereFinalised

		public void TestIsKitBuiltOnSalesOrder_AllPicksWereFinalised()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			data.Part1.OP_IsComponentPickedOnSalesOrder = true;
			Helper.CreateProductBOM(data.Part1, data.Part2, 4m, "UNT");

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part2, 100m);
			Factory.Save();

			// ordering kits
			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);
			AssertEquals("Precondition - pick allocated.", true, pick.GetAllPickLines().Any());
			AssertNotNull("Components were picked to build kits.", pick.GetAllPickLines().Single(l => l.DocketLine.WE_OP == data.Part2.PK));

			pick.GetAllPickLines().Where(pl => !pl.IsPickByBOMKitPickLine()).ForEach(l =>
			{
				l.WZ_PickedDateTime = ZDateTimeOffset.Now;
				l.WZ_GS_NKAssignedTo = "E";
			});

			Factory.Save();

			order.FinaliseDocketAlwaysFinalisingPick();
			Factory.Save();
			AssertIsFinalisedPrecondition(order);
			AssertIsFinalisedPrecondition(pick);

			var detector = ObjectFactory.Get<IWhsPickOnSalesOrderDetector>();

			Factory.ClearCachedValue<bool>("IsKitBuiltOnSalesOrder|" + data.Part1.PK);
			AssertEquals("All Picks are Finalised and in new data shape. Should return false.", false, detector.IsKitBuiltOnSalesOrder(Factory, data.Part1.PK));

			Factory.ClearCachedValue<bool>("IsKitBuiltOnSalesOrder|" + data.Part2.PK);
			AssertEquals("For component product should return false.", false, detector.IsKitBuiltOnSalesOrder(Factory, data.Part2.PK));
		}

		#endregion

		#region TestIsKitBuiltOnSalesOrder_AllPicksWereFinalised_ButNotAllTransformedYet

		public void TestIsKitBuiltOnSalesOrder_AllPicksWereFinalised_ButNotAllTransformedYet()
		{
			// This test can be deleted after the removal of the Transformation PopulateWhsPickByBOMData.cs
			var data = new TestDataSimpleEnvironment(Factory);
			data.Part1.OP_IsComponentPickedOnSalesOrder = true;
			Helper.CreateProductBOM(data.Part1, data.Part2, 4m, "UNT");

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part2, 1000m);
			Factory.Save();

			// Create a Finalised Order with the old data shape.
			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 10m);
			var pick1 = Helper.CreatePickNew(order1);
			AssertEquals("Precondition - pick allocated.", true, pick1.GetAllPickLines().Any());
			AssertNotNull("Components were picked to build kits.", pick1.GetAllPickLines().Single(l => l.DocketLine.WE_OP == data.Part2.PK));

			pick1.GetAllPickLines().Where(pl => !pl.IsPickByBOMKitPickLine()).ForEach(l =>
			{
				l.WZ_PickedDateTime = ZDateTimeOffset.Now;
				l.WZ_GS_NKAssignedTo = "E";
			});

			Factory.Save();

			order1.FinaliseDocketAlwaysFinalisingPick();
			MockOldDataShapeForFinalisedPick(order1);
			Factory.Save();
			AssertIsFinalisedPrecondition(order1);
			AssertIsFinalisedPrecondition(pick1);

			// Create a Finalised Order with the new data shape.
			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O2", data.Part1, 10m);
			var pick2 = Helper.CreatePickNew(order2);
			AssertEquals("Precondition - pick allocated.", true, pick2.GetAllPickLines().Any());
			AssertNotNull("Components were picked to build kits.", pick2.GetAllPickLines().Single(l => l.DocketLine.WE_OP == data.Part2.PK));

			pick2.GetAllPickLines().Where(pl => !pl.IsPickByBOMKitPickLine()).ForEach(l =>
			{
				l.WZ_PickedDateTime = ZDateTimeOffset.Now;
				l.WZ_GS_NKAssignedTo = "E";
			});

			Factory.Save();

			order2.FinaliseDocketAlwaysFinalisingPick();
			Factory.Save();
			AssertIsFinalisedPrecondition(order2);
			AssertIsFinalisedPrecondition(pick2);
			var detector = ObjectFactory.Get<IWhsPickOnSalesOrderDetector>();

			Factory.ClearCachedValue<bool>("IsKitBuiltOnSalesOrder|" + data.Part1.PK);
			AssertEquals("There are Finalised Picks in the old data shape.", true, detector.IsKitBuiltOnSalesOrder(Factory, data.Part1.PK));

			Factory.ClearCachedValue<bool>("IsKitBuiltOnSalesOrder|" + data.Part2.PK);
			AssertEquals("For component product should return false.", false, detector.IsKitBuiltOnSalesOrder(Factory, data.Part2.PK));
		}

		#endregion

		#region TestIsKitBuiltOnSalesOrder_SomePicksWereNotFinalised

		public void TestIsKitBuiltOnSalesOrder_SomePicksWereNotFinalised()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			data.Part1.OP_IsComponentPickedOnSalesOrder = true;
			Helper.CreateProductBOM(data.Part1, data.Part2, 4m, "UNT");

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part2, 100m);
			Factory.Save();

			// ordering kits
			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);
			AssertEquals("Precondition - pick allocated.", true, pick.GetAllPickLines().Any());
			AssertNotNull("Components were picked to build kits.", pick.GetAllPickLines().Single(l => l.DocketLine.WE_OP == data.Part2.PK));
			Factory.Save();
			AssertEquals(false, pick.IsFinalised);

			var detector = ObjectFactory.Get<IWhsPickOnSalesOrderDetector>();

			Factory.ClearCachedValue<bool>("IsKitBuiltOnSalesOrder|" + data.Part1.PK);
			AssertEquals("Kit was built on sales oreder. Should return true.", true, detector.IsKitBuiltOnSalesOrder(Factory, data.Part1.PK));

			Factory.ClearCachedValue<bool>("IsKitBuiltOnSalesOrder|" + data.Part2.PK);
			AssertEquals("For component product should return false.", false, detector.IsKitBuiltOnSalesOrder(Factory, data.Part2.PK));
		}

		#endregion
	}
}
