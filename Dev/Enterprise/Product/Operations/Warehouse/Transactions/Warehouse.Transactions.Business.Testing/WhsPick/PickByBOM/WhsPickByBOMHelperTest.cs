using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Packing.Business;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	class WhsPickByBOMHelperTest : WhsTestCaseWithFactory
	{
		#region TestNewKitReceiveLine

		public void TestNewKitReceiveLine()
		{
			var receivePK = ZGuid.NewZGuid();
			var supplierPartPK = ZGuid.NewZGuid();
			var quantity = 100m;
			var line = WhsPickByBOMHelper.NewKitReceiveLine(Factory, receivePK, supplierPartPK, quantity);
			AssertEquals(receivePK, line.WE_WD);
			AssertEquals(supplierPartPK, line.WE_OP);
			AssertEquals(quantity, line.WE_TransactionQuantity);
			AssertEquals(quantity, line.WE_ClientOrderedUnits);
			AssertEquals(quantity, line.WE_StockOnHand);
			AssertEquals(InventoryStatus.Codes.Pending, line.WE_CurrentInventoryStatus);
			AssertEquals(InventoryStatus.Codes.Pending, line.WE_OriginalInventoryStatus);
			AssertEquals(ZDateTimeOffset.Empty, line.WE_AdjustmentArrivalDate);
		}

		#endregion

		#region TestUpdateKitReceiveLineToPUT

		public void TestUpdateKitReceiveLineToPUT()
		{
			var line = WhsPickByBOMHelper.NewKitReceiveLine(Factory, ZGuid.NewZGuid(), ZGuid.NewZGuid(), 100m);
			AssertEquals("Precondition", ZGuid.Empty, line.WE_WL);
			AssertEquals("Precondition", InventoryStatus.Codes.Pending, line.WE_OriginalInventoryStatus);
			AssertEquals("Precondition", InventoryStatus.Codes.Pending, line.WE_CurrentInventoryStatus);
			AssertEquals("Precondition", ZString.Empty, line.WE_DocketLineStatus);
			AssertEquals("Precondition", ZDateTimeOffset.Empty, line.WE_AdjustmentArrivalDate);
			AssertEquals("Precondition", ZDateTimeOffset.Empty, line.WE_UnloadedTime);
			AssertEquals("Precondition", ZString.Empty, line.WE_GS_NKUnloadedBy);

			var locationPK = ZGuid.NewZGuid();
			var pickedBy = "ABC";
			var pickedDate = ZDateTimeOffset.Now;
			WhsPickByBOMHelper.UpdateKitReceiveLineToPUT(line, locationPK, pickedBy, pickedDate);

			AssertEquals(locationPK, line.WE_WL);
			AssertEquals(InventoryStatus.Codes.Putaway, line.WE_OriginalInventoryStatus);
			AssertEquals(InventoryStatus.Codes.Putaway, line.WE_CurrentInventoryStatus);
			AssertEquals(DocketLineStatus.Codes.PickedForUnload, line.WE_DocketLineStatus);
			AssertEquals(pickedDate, line.WE_AdjustmentArrivalDate);
			AssertEquals(pickedDate, line.WE_UnloadedTime);
			AssertEquals(pickedBy, line.WE_GS_NKUnloadedBy);
		}

		#endregion

		#region TestCreatePickByBOMTransferLinesIfNecessary

		// Tested in PutawayStockInDockDoorOrPackingStationTest.cs, see tests start with TestPutawayStockInDockDoorOrPackingStation_ComponentsOfPickByBOM
		public void TestCreatePickByBOMTransferLinesIfNecessary()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var date1 = new ZDateTime(2024, 4, 1, 7, 0, 0);
			var dateOffset1 = data.Whs1.GetWarehouseBranchLocalDateTimeOffset(date1);
			Factory.Save();

			var bike = Helper.CreateProduct(data.Org1, "BIKE");
			bike.OP_IsComponentPickedOnSalesOrder = true;
			var wheel = Helper.CreateProduct(data.Org1, "WHEEL");
			Helper.CreateProductBOM(bike, wheel, 2m, PkgUnit.Unit);
			var location1 = data.Whs1.FindLocation("A-1");
			var dockdoorLocation = data.Whs1.DefaultOutboundDockDoorLocation;
			var locationString = dockdoorLocation.WLV_LocationString;
			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receiveLine1 = Helper.CreateWhsReceiveInventoryLine(receive1, wheel, 10m, location1);
			receive1.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive1);

			var pick = Factory.New<WhsPick>();
			pick.WP_WW_Whs = data.Whs1.PK;
			pick.WP_WL_DockDoor = data.Whs1.WW_DefaultInboundDockDoor;
			pick.WP_PickOption = WhsPickOption.Codes.Manual;
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1", pickOption: WhsPickOption.Codes.Manual);
			var orderLine = Helper.CreateWhsOrderLine(order, bike, 5m);

			pick.AddOrders(new[] { order });
			pick.AutoAllocateItemsWithMock();
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			pick = newFactory.Load<WhsPick>(pick.PK);
			orderLine = newFactory.Load<WhsOrderLine>(orderLine.PK);

			AssertEquals("Added component orderline should exist.", 1, orderLine.ChildComponentLines.Count);
			var componentOrderLine = orderLine.ChildComponentLines.First();
			AssertEquals("Should be allocated.", 1, componentOrderLine.PickLines.Count);
			var componentPickLine1 = componentOrderLine.PickLines.Single(l => l.WZ_Units == 10m);
			componentPickLine1.WZ_PickedDateTime = dateOffset1;
			newFactory.Save();

			var kitReceive = newFactory.Load<WhsReceive>(new ZQuery(WhsDocketSchema.WD_WP_ParentPickForReceive, pick.PK)).Single();
			var kitReceiveLine1 = kitReceive.Lines[0];
			var kitPickLines = newFactory.Load<WhsPickLine>(new ZQuery(WhsPickLineSchema.WZ_WE_InventoryLine, kitReceiveLine1.PK));
			AssertEquals("Precondition: Should be committed to relevant Order Line through PickLine.", 1, kitPickLines.Length);

			var componentTransferLine1 = (WhsTransferLine)pick.Transfers.Single().Lines.Single();
			AssertEquals("Precondition: Should be In-Transit.", InventoryStatus.Codes.InTransit, componentTransferLine1.WE_CurrentInventoryStatus);

			var dockdoorPK = data.Whs1.WW_DefaultInboundDockDoor;
			var wheelTransferLine = (WhsTransferLine)componentPickLine1.InventoryLine;
			var inTransitLines = new WhsTransferLine[] { wheelTransferLine };
			var (transferLinesToIgnoreWhenSettingLocation, kitPackages) = WhsPickByBOMHelper.CreatePickByBOMTransferLinesIfNecessary(newFactory, dockdoorPK, inTransitLines, GlbStaff.CurrentUser.GS_Code);

			AssertEquals(1, transferLinesToIgnoreWhenSettingLocation.Count);
			AssertEquals(wheelTransferLine, transferLinesToIgnoreWhenSettingLocation.Single());
			AssertEquals(0, kitPackages.Count());
		}

		public void TestCreatePickByBOMTransferLinesIfNecessary_KitPackages()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var date1 = new ZDateTime(2024, 4, 1, 7, 0, 0);
			var dateOffset1 = data.Whs1.GetWarehouseBranchLocalDateTimeOffset(date1);
			Factory.Save();

			var bike = Helper.CreateProduct(data.Org1, "BIKE");
			bike.OP_IsComponentPickedOnSalesOrder = true;
			var wheel = Helper.CreateProduct(data.Org1, "WHEEL");
			Helper.CreateProductBOM(bike, wheel, 2m, PkgUnit.Unit);
			var location1 = data.Whs1.FindLocation("A-1");
			var dockdoorLocation = data.Whs1.DefaultOutboundDockDoorLocation;
			var locationString = dockdoorLocation.WLV_LocationString;
			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receiveLine1 = Helper.CreateWhsReceiveInventoryLine(receive1, wheel, 10m, location1);
			receive1.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive1);

			var pick = Factory.New<WhsPick>();
			pick.WP_WW_Whs = data.Whs1.PK;
			pick.WP_WL_DockDoor = data.Whs1.WW_DefaultInboundDockDoor;
			pick.WP_PickOption = WhsPickOption.Codes.Manual;
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1", pickOption: WhsPickOption.Codes.Manual);
			var orderLine = Helper.CreateWhsOrderLine(order, bike, 5m);

			pick.AddOrders(new[] { order });
			pick.AutoAllocateItemsWithMock();

			// pack kit pick line to a package
			var kitPickLine = orderLine.PickLines.Single();
			var packageJob = PkgPackageJob.LoadOrCreatePackageJob(order);
			var package = packageJob.Packages.AddNew(PkgUnit.Pallet, "123");
			package.Pack(orderLine.ReleaseLines[0], 5m);
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			pick = newFactory.Load<WhsPick>(pick.PK);
			orderLine = newFactory.Load<WhsOrderLine>(orderLine.PK);

			AssertEquals("Added component orderline should exist.", 1, orderLine.ChildComponentLines.Count);
			var componentOrderLine = orderLine.ChildComponentLines.First();
			AssertEquals("Should be allocated.", 1, componentOrderLine.PickLines.Count);
			var componentPickLine1 = componentOrderLine.PickLines.Single(l => l.WZ_Units == 10m);
			componentPickLine1.WZ_PickedDateTime = dateOffset1;
			newFactory.Save();

			var kitReceive = Factory.Load<WhsReceive>(new ZQuery(WhsDocketSchema.WD_WP_ParentPickForReceive, pick.PK)).Single();
			var kitReceiveLine1 = kitReceive.Lines[0];
			var kitPickLines = Factory.Load<WhsPickLine>(new ZQuery(WhsPickLineSchema.WZ_WE_InventoryLine, kitReceiveLine1.PK));
			AssertEquals("Precondition: Should be committed to relevant Order Line through PickLine.", 1, kitPickLines.Length);

			var componentTransferLine1 = (WhsTransferLine)pick.Transfers.Single().Lines.Single();
			AssertEquals("Precondition: Should be In-Transit.", InventoryStatus.Codes.InTransit, componentTransferLine1.WE_CurrentInventoryStatus);

			var dockdoorPK = data.Whs1.WW_DefaultInboundDockDoor;
			var wheelTransferLine = (WhsTransferLine)componentPickLine1.InventoryLine;
			var inTransitLines = new WhsTransferLine[] { wheelTransferLine };
			var (transferLinesToIgnoreWhenSettingLocation, kitPackages) = WhsPickByBOMHelper.CreatePickByBOMTransferLinesIfNecessary(Factory, dockdoorPK, inTransitLines, GlbStaff.CurrentUser.GS_Code, isClosingPackagesWhenPutToDockDoor: true);

			AssertEquals(1, transferLinesToIgnoreWhenSettingLocation.Count);
			AssertEquals(wheelTransferLine, transferLinesToIgnoreWhenSettingLocation.Single());
			AssertEquals(1, kitPackages.Count());
			AssertEquals(package.PK, kitPackages.Single().PK);
		}

		public void TestCreatePickByBOMTransferLinesIfNecessary_MandatoryPartAttributes()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			Helper.CreateProductBOM(data.Part1, data.Part2);
			data.Part1.OP_IsComponentPickedOnSalesOrder = true;
			Helper.SetClientAllAttributeType(data.Org1, mandatoryAttributeType: true);
			Helper.SetProductAllAttributeUse(data.Org1, data.Part1, use: true, setReleaseCaptured: false, useSerialNumber: true);
			Helper.SetProductAllAttributeUse(data.Org1, data.Part2, use: true, setReleaseCaptured: false, useSerialNumber: true);
			Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var inv = Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 1m, data.Whs1.FindLocation("A-1"), "PID456", ZDate.Today.AddDays(20), ZDate.Today, "PA1", "PA2", "PA3", string.Empty);
			inv.WI_SerialNumber = "FRT";
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);

			var pick = Helper.Factory.New<WhsPick>();
			pick.WP_WW_Whs = data.Whs1.PK;
			pick.WP_WL_DockDoor = data.Whs1.WW_DefaultInboundDockDoor;
			pick.WP_PickOption = WhsPickOption.Codes.Manual;
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1", pickOption: WhsPickOption.Codes.Manual);
			Helper.CreateWhsOrderLine(order, data.Part1, 1m);

			pick.AddOrders(new[] { order });
			pick.AutoAllocateItemsWithMock();
			Factory.Save();

			var pickLine = order.Lines.Single(l => l.WE_OP == data.Part2.PK).PickLines.Single();
			pickLine.WZ_PickedDateTime = ZDateTimeOffset.Now;
			Factory.Save();

			var transfer = Factory.Load<WhsTransfer>(new ZQuery(WhsDocketSchema.WD_WP_ParentPickForTransfer, pick.PK)).Single();
			var transferLine = (WhsTransferLine)transfer.Lines.Single();
			AssertEquals("Precondition: in-transit.", InventoryStatus.Codes.InTransit, transferLine.WE_CurrentInventoryStatus);

			var inTransitLines = new WhsTransferLine[] { transferLine };
			var (transferLinesToIgnoreWhenSettingLocation, kitPackages) = WhsPickByBOMHelper.CreatePickByBOMTransferLinesIfNecessary(Factory, data.Whs1.WW_DefaultInboundDockDoor, inTransitLines, GlbStaff.CurrentUser.GS_Code, isClosingPackagesWhenPutToDockDoor: true);
			AssertEquals("Should have no error", false, transfer.GetErrors().Any());
		}

		#endregion
	}
}
