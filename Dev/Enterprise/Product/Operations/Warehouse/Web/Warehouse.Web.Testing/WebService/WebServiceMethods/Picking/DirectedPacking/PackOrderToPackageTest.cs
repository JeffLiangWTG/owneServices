using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Packing.Business;
using Enterprise.Packing.Business.Testing;
using Enterprise.Warehouse.Environment.CodeLists;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.Warehouse.Web.WebService.Business;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;

namespace Enterprise.Warehouse.Web.WebService.Testing
{
	class PackOrderToPackageTest : WhsSecureServiceTestCase
	{
		public void TestPackOrderToPackage()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var user = Helper.CreateGlbStaff("AAA", "AAAAA");

			var packingStationLocationType = Helper.CreateLocationType("XYZ", "Test", false, 0, LocationClasses.Codes.PST);
			var newRow = Helper.CreateRowAndGenerateLocations(data.Whs1, "P");
			var packingLocation = newRow.Locations[0];
			packingLocation.WLV_WLT_LocationType = packingStationLocationType.PK;
			Factory.Save();

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part2, 15m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine1 = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var orderLine2 = Helper.CreateWhsOrderLine(order, data.Part2, 15m);
			Helper.CreatePickNew(order);

			var pickLine1 = orderLine1.PickLines.Single();
			var transferLine1 = Helper.PickAndMakeInTransitTransfer(pickLine1, ZDateTimeOffset.Now);
			transferLine1.WE_WL = packingLocation.PK;
			transferLine1.FinaliseDocketLine();

			var pickLine2 = orderLine2.PickLines.Single();
			var transferLine2 = Helper.PickAndMakeInTransitTransfer(pickLine2, ZDateTimeOffset.Now);
			transferLine2.WE_WL = packingLocation.PK;
			transferLine2.FinaliseDocketLine();
			Factory.Save();

			AssertEquals("Precondition", InventoryStatus.Codes.ReadyToPack, transferLine1.WE_CurrentInventoryStatus);
			AssertEquals("Precondition", InventoryStatus.Codes.ReadyToPack, transferLine2.WE_CurrentInventoryStatus);
			AssertEquals("Precondition", WhsOrderStatus.Codes.ReadyToPack, order.WarehouseOrderStatus);
			AssertEquals("Precondition", false, order.PackageJob.IsPacked(pickLine1));
			AssertEquals("Precondition", false, order.PackageJob.IsPacked(pickLine2));
			AssertEquals("Order has no packages.", false, order.PackageJob.Packages.Any());

			var productInfo1 = new WhsPackageProductInfo { ProductPK = data.Part1.PK.ToGuid(), ExpectedQty = 10m, Quantity = 10m };
			var productInfo2 = new WhsPackageProductInfo { ProductPK = data.Part2.PK.ToGuid(), ExpectedQty = 15m, Quantity = 15m };
			var packageInfo = new PackageForPackingInfo()
			{
				ToteID = "ABC",
				OrderReference = "O1",
				IsPackageSplitForPacking = false,
				DocketID = order.WD_DocketID,
				IsTote = true,
				ScannedProductInfos = new[] { productInfo1, productInfo2 }
			};

			var webService = GetNewWebService(data.Whs1, staff: user);
			var response = webService.PackOrderToPackage(packageInfo);
			AssertEquals("Should be no error.", ErrorTypes.None, response.Error);
			AssertNull("Should be no error.", response.ErrorMessage);

			Assert(order.PackageJob.IsPacked(pickLine1));
			Assert(order.PackageJob.IsPacked(pickLine2));

			var orderInNewFactory = new BusinessObjectFactory().Load<WhsOrder>(order.PK);
			var tote = orderInNewFactory.PackageJob.Packages.Single(pkg => pkg.KP_PackageID == "ABC");
			AssertNotNull("Tote is created.", tote);
			Assert(tote.GetIsTote());
			AssertEquals(tote.PK.ToGuid(), response.NewPackagePK);
			Assert(response.NewPackageID.IsNullOrEmpty());

			AssertEquals("There are 2 packed item divots in the package.", 2, tote.PackedItemDivots.Count);
			AssertEquals("Quantity on packed item divots is correct.", true, tote.PackedItemDivots.Any(divot => divot.PackedItem.Quantity == 10m));
			AssertEquals("Quantity on packed item divots is correct.", true, tote.PackedItemDivots.Any(divot => divot.PackedItem.Quantity == 15m));
		}

		public void TestPackOrderToPackage_MultiplePickLines()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var user = Helper.CreateGlbStaff("AAA", "AAAAA");

			var packingStationLocationType = Helper.CreateLocationType("XYZ", "Test", false, 0, LocationClasses.Codes.PST);
			var newRow = Helper.CreateRowAndGenerateLocations(data.Whs1, "P");
			var packingLocation = newRow.Locations[0];
			packingLocation.WLV_WLT_LocationType = packingStationLocationType.PK;
			Factory.Save();

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 4m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 6m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R3", data.Part2, 5m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R4", data.Part2, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine1 = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var orderLine2 = Helper.CreateWhsOrderLine(order, data.Part2, 15m);
			Helper.CreatePickNew(order);

			AssertEquals(2, orderLine1.PickLines.Count);
			orderLine1.PickLines.ForEach(l =>
			{
				var transferLine = Helper.PickAndMakeInTransitTransfer(l, ZDateTimeOffset.Now);
				transferLine.WE_WL = packingLocation.PK;
				transferLine.FinaliseDocketLine();
			});

			AssertEquals(2, orderLine2.PickLines.Count);
			orderLine2.PickLines.ForEach(l =>
			{
				var transferLine = Helper.PickAndMakeInTransitTransfer(l, ZDateTimeOffset.Now);
				transferLine.WE_WL = packingLocation.PK;
				transferLine.FinaliseDocketLine();
			});
			Factory.Save();

			Assert("Precondition", orderLine1.PickLines.All(pl => pl.IsUnpacked(Factory)));
			Assert("Precondition", orderLine2.PickLines.All(pl => pl.IsUnpacked(Factory)));
			AssertEquals("Precondition", WhsOrderStatus.Codes.ReadyToPack, order.WarehouseOrderStatus);

			var productInfo1 = new WhsPackageProductInfo { ProductPK = data.Part1.PK.ToGuid(), ExpectedQty = 10m, Quantity = 10m };
			var productInfo2 = new WhsPackageProductInfo { ProductPK = data.Part2.PK.ToGuid(), ExpectedQty = 15m, Quantity = 15m };
			var packageInfo = new PackageForPackingInfo()
			{
				ToteID = "ABC",
				OrderReference = "O1",
				IsPackageSplitForPacking = false,
				DocketID = order.WD_DocketID,
				IsTote = true,
				ScannedProductInfos = new[] { productInfo1, productInfo2 }
			};

			var webService = GetNewWebService(data.Whs1, staff: user);
			var response = webService.PackOrderToPackage(packageInfo);
			AssertEquals("Should be no error.", ErrorTypes.None, response.Error);
			AssertNull("Should be no error.", response.ErrorMessage);

			Assert(orderLine1.PickLines.All(pl => !pl.IsUnpacked(Factory)));
			Assert(orderLine2.PickLines.All(pl => !pl.IsUnpacked(Factory)));

			var orderInNewFactory = new BusinessObjectFactory().Load<WhsOrder>(order.PK);
			var tote = orderInNewFactory.PackageJob.Packages.Single(pkg => pkg.KP_PackageID == "ABC");
			AssertNotNull("Tote is created.", tote);
			Assert(tote.GetIsTote());
			AssertEquals(tote.PK.ToGuid(), response.NewPackagePK);
			Assert(response.NewPackageID.IsNullOrEmpty());

			AssertEquals("There are 4 packed item divots in the package.", 4, tote.PackedItemDivots.Count);
			AssertEquals("Quantity on packed item divots is correct.", true, tote.PackedItemDivots.Any(divot => divot.PackedItem.Quantity == 10m));
			AssertEquals("Quantity on packed item divots is correct.", true, tote.PackedItemDivots.Any(divot => divot.PackedItem.Quantity == 5m));
			AssertEquals("Quantity on packed item divots is correct.", true, tote.PackedItemDivots.Any(divot => divot.PackedItem.Quantity == 4m));
			AssertEquals("Quantity on packed item divots is correct.", true, tote.PackedItemDivots.Any(divot => divot.PackedItem.Quantity == 6m));
		}

		public void TestPackOrderToPackage_PartialPacking()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var user = Helper.CreateGlbStaff("AAA", "AAAAA");

			var packingStationLocationType = Helper.CreateLocationType("XYZ", "Test", false, 0, LocationClasses.Codes.PST);
			var newRow = Helper.CreateRowAndGenerateLocations(data.Whs1, "P");
			var packingLocation = newRow.Locations[0];
			packingLocation.WLV_WLT_LocationType = packingStationLocationType.PK;
			Factory.Save();

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part2, 15m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine1 = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var orderLine2 = Helper.CreateWhsOrderLine(order, data.Part2, 15m);
			Helper.CreatePickNew(order);

			var pickLine1 = orderLine1.PickLines.Single();
			var transferLine1 = Helper.PickAndMakeInTransitTransfer(pickLine1, ZDateTimeOffset.Now);
			transferLine1.WE_WL = packingLocation.PK;
			transferLine1.FinaliseDocketLine();

			var pickLine2 = orderLine2.PickLines.Single();
			var transferLine2 = Helper.PickAndMakeInTransitTransfer(pickLine2, ZDateTimeOffset.Now);
			transferLine2.WE_WL = packingLocation.PK;
			transferLine2.FinaliseDocketLine();
			Factory.Save();

			AssertEquals("Precondition", InventoryStatus.Codes.ReadyToPack, transferLine1.WE_CurrentInventoryStatus);
			AssertEquals("Precondition", InventoryStatus.Codes.ReadyToPack, transferLine2.WE_CurrentInventoryStatus);
			AssertEquals("Precondition", WhsOrderStatus.Codes.ReadyToPack, order.WarehouseOrderStatus);
			AssertEquals("Precondition", false, order.PackageJob.IsPacked(pickLine1));
			AssertEquals("Precondition", false, order.PackageJob.IsPacked(pickLine2));

			var productInfo1 = new WhsPackageProductInfo { ProductPK = data.Part1.PK.ToGuid(), ExpectedQty = 10m, Quantity = 6m };
			var productInfo2 = new WhsPackageProductInfo { ProductPK = data.Part2.PK.ToGuid(), ExpectedQty = 15m, Quantity = 7m };
			var packageInfo = new PackageForPackingInfo()
			{
				ToteID = "ABC",
				OrderReference = "O1",
				IsPackageSplitForPacking = true,
				DocketID = order.WD_DocketID,
				IsTote = true,
				ScannedProductInfos = new[] { productInfo1, productInfo2 }
			};

			var webService = GetNewWebService(data.Whs1, staff: user);
			var response = webService.PackOrderToPackage(packageInfo);
			AssertEquals("Should be no error.", ErrorTypes.None, response.Error);
			AssertNull("Should be no error.", response.ErrorMessage);

			var orderInNewFactory = new BusinessObjectFactory().Load<WhsOrder>(order.PK);
			var tote = orderInNewFactory.PackageJob.Packages.Single(pkg => pkg.KP_PackageID == "ABC");
			AssertNotNull("Tote is created.", tote);
			Assert(tote.GetIsTote());
			AssertEquals(tote.PK.ToGuid(), response.NewPackagePK);
			Assert(response.NewPackageID.IsNullOrEmpty());

			AssertEquals("There are 2 packed item divots in the package.", 2, tote.PackedItemDivots.Count);
			AssertEquals("Quantity on packed item divots is correct.", true, tote.PackedItemDivots.Any(divot => divot.PackedItem.Quantity == 6m));
			AssertEquals("Quantity on packed item divots is correct.", true, tote.PackedItemDivots.Any(divot => divot.PackedItem.Quantity == 7m));

			var unpackedPickLines = order.Lines.SelectMany(l => l.PickLines).Where(pl => pl.IsUnpacked(Factory));
			AssertEquals(2, unpackedPickLines.Count());
			var unPackedPickLinePart1 = unpackedPickLines.Single(pl => pl.ProductCode == data.Part1.OP_PartNum);
			AssertEquals(4m, unPackedPickLinePart1.WZ_Units);
			var unPackedPickLinePart2 = unpackedPickLines.Single(pl => pl.ProductCode == data.Part2.OP_PartNum);
			AssertEquals(8m, unPackedPickLinePart2.WZ_Units);
		}

		public void TestPackOrderToPackage_PartiallyPackedOrder()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var user = Helper.CreateGlbStaff("AAA", "AAAAA");

			var packingStationLocationType = Helper.CreateLocationType("XYZ", "Test", false, 0, LocationClasses.Codes.PST);
			var newRow = Helper.CreateRowAndGenerateLocations(data.Whs1, "P");
			var packingLocation = newRow.Locations[0];
			packingLocation.WLV_WLT_LocationType = packingStationLocationType.PK;
			Factory.Save();

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part2, 15m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine1 = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var orderLine2 = Helper.CreateWhsOrderLine(order, data.Part2, 15m);
			Helper.CreatePickNew(order);

			var pickLine1 = orderLine1.PickLines.Single();
			var transferLine1 = Helper.PickAndMakeInTransitTransfer(pickLine1, ZDateTimeOffset.Now);
			transferLine1.WE_WL = packingLocation.PK;
			transferLine1.FinaliseDocketLine();

			var pickLine2 = orderLine2.PickLines.Single();
			var transferLine2 = Helper.PickAndMakeInTransitTransfer(pickLine2, ZDateTimeOffset.Now);
			transferLine2.WE_WL = packingLocation.PK;
			transferLine2.FinaliseDocketLine();

			var packageJob = PkgPackageJob.LoadOrCreatePackageJob(order);
			var package1 = PackingHelper.CreatePackage(packageJob, "PKG1", 1, PkgUnit.Box);
			package1.Pack(orderLine1.ReleaseLines[0], 6m);

			var package2 = PackingHelper.CreatePackage(packageJob, "PKG2", 1, PkgUnit.Box);
			package2.Pack(orderLine2.ReleaseLines[0], 10m);
			Factory.Save();

			AssertEquals("Precondition", InventoryStatus.Codes.ReadyToPack, transferLine1.WE_CurrentInventoryStatus);
			AssertEquals("Precondition", InventoryStatus.Codes.ReadyToPack, transferLine2.WE_CurrentInventoryStatus);
			AssertEquals("Precondition", WhsOrderStatus.Codes.ReadyToPack, order.WarehouseOrderStatus);
			AssertEquals("Precondition", false, order.PackageJob.IsPacked(pickLine1));
			AssertEquals("Precondition", false, order.PackageJob.IsPacked(pickLine2));

			var productInfo1 = new WhsPackageProductInfo { ProductPK = data.Part1.PK.ToGuid(), ExpectedQty = 4m, Quantity = 4m };
			var productInfo2 = new WhsPackageProductInfo { ProductPK = data.Part2.PK.ToGuid(), ExpectedQty = 5m, Quantity = 5m };
			var packageInfo = new PackageForPackingInfo()
			{
				ToteID = "ABC",
				OrderReference = "O1",
				IsPackageSplitForPacking = false,
				DocketID = order.WD_DocketID,
				IsTote = true,
				ScannedProductInfos = new[] { productInfo1, productInfo2 }
			};

			var webService = GetNewWebService(data.Whs1, staff: user);
			var response = webService.PackOrderToPackage(packageInfo);
			AssertEquals("Should be no error.", ErrorTypes.None, response.Error);
			AssertNull("Should be no error.", response.ErrorMessage);

			var orderInNewFactory = new BusinessObjectFactory().Load<WhsOrder>(order.PK);
			var tote = orderInNewFactory.PackageJob.Packages.Single(pkg => pkg.KP_PackageID == "ABC");
			AssertNotNull("Tote is created.", tote);
			Assert(tote.GetIsTote());
			AssertEquals(tote.PK.ToGuid(), response.NewPackagePK);
			Assert(response.NewPackageID.IsNullOrEmpty());

			AssertEquals("There are 2 packed item divots in the package.", 2, tote.PackedItemDivots.Count);
			AssertEquals("Quantity on packed item divots is correct.", true, tote.PackedItemDivots.Any(divot => divot.PackedItem.Quantity == 4m));
			AssertEquals("Quantity on packed item divots is correct.", true, tote.PackedItemDivots.Any(divot => divot.PackedItem.Quantity == 5m));
		}

		public void TestPackOrderToPackage_NullPackageInfo()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var user = Helper.CreateGlbStaff("AAA", "AAAAA");
			Factory.Save();

			var webService = GetNewWebService(data.Whs1, staff: user);
			var response = webService.PackOrderToPackage(null);
			AssertEquals("Should be error.", ErrorTypes.BusinessValidationError, response.Error);
			AssertEquals("Should be error.", "Please provide a valid Package Info.", response.ErrorMessage);
			AssertEquals(Guid.Empty, response.NewPackagePK);
		}

		public void TestPackOrderToPackage_InvalidTote()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var user = Helper.CreateGlbStaff("AAA", "AAAAA");
			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			Factory.Save();

			var webService = GetNewWebService(data.Whs1, staff: user);
			var response = webService.PackOrderToPackage(new PackageForPackingInfo { PK = Guid.NewGuid(), ToteID = "", DocketID = order.WD_DocketID, IsTote = true });
			AssertEquals("Should be error.", ErrorTypes.BusinessValidationError, response.Error);
			AssertEquals("Should be error.", "Empty Tote ID.", response.ErrorMessage);
			AssertEquals(Guid.Empty, response.NewPackagePK);
			Assert(response.NewPackageID.IsNullOrEmpty());
		}

		public void TestPackOrderToPackage_InvalidTote_PackageExists_NonTote()
		{
			TestPackOrderToPackage_InvalidTote_PackageExistsCore(isTote: false, "A non Tote Package on the order was found using Package ID 'ABC' already.");
		}

		public void TestPackOrderToPackage_InvalidTote_PackageExists_Tote()
		{
			TestPackOrderToPackage_InvalidTote_PackageExistsCore(isTote: true, "Tote 'ABC' is already used on this order, select another Tote.");
		}

		void TestPackOrderToPackage_InvalidTote_PackageExistsCore(bool isTote, string expectedErrorMessage)
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var user = Helper.CreateGlbStaff("AAA", "AAAAA");
			Factory.Save();

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 10m);
			Helper.CreatePickNew(order);

			var packageJob = PkgPackageJob.LoadOrCreatePackageJob(order);
			var package = PackingHelper.CreatePackage(packageJob, "ABC", 1, PkgUnit.Box);
			package.Pack(order.Lines[0].ReleaseLines[0], 5m);
			if (isTote)
			{
				package.SetIsTote(true);
			}
			Factory.Save();

			Assert("Order has packages", order.PackageJob.Packages.Any());

			var webService = GetNewWebService(data.Whs1, staff: user);
			var response = webService.PackOrderToPackage(new PackageForPackingInfo { PK = Guid.NewGuid(), ToteID = "ABC", DocketID = order.WD_DocketID, IsTote = true });
			AssertEquals("Should be error.", ErrorTypes.BusinessValidationError, response.Error);
			AssertEquals("Should be error.", expectedErrorMessage, response.ErrorMessage);
			AssertEquals(Guid.Empty, response.NewPackagePK);
			Assert(response.NewPackageID.IsNullOrEmpty());
		}

		public void TestPackOrderToPackage_InvalidOrder()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var user = Helper.CreateGlbStaff("AAA", "AAAAA");
			Factory.Save();

			var webService = GetNewWebService(data.Whs1, staff: user);
			var response = webService.PackOrderToPackage(new PackageForPackingInfo { PK = Guid.NewGuid(), ToteID = "ABC", DocketID = "O1" });
			AssertEquals("Should be error.", ErrorTypes.BusinessValidationError, response.Error);
			AssertEquals("Should be error.", "No Warehouse Order found with Docket ID: O1.", response.ErrorMessage);
			AssertEquals(Guid.Empty, response.NewPackagePK);
			Assert(response.NewPackageID.IsNullOrEmpty());
		}

		public void TestPackOrderToPackage_UnableToPackOrder()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var user = Helper.CreateGlbStaff("AAA", "AAAAA");

			var packingStationLocationType = Helper.CreateLocationType("XYZ", "Test", false, 0, LocationClasses.Codes.PST);
			var newRow = Helper.CreateRowAndGenerateLocations(data.Whs1, "P");
			var packingLocation = newRow.Locations[0];
			packingLocation.WLV_WLT_LocationType = packingStationLocationType.PK;
			Factory.Save();

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			Helper.CreatePickNew(order);

			var pickLine = orderLine.PickLines.Single();
			var transferLine1 = Helper.PickAndMakeInTransitTransfer(pickLine, ZDateTimeOffset.Now);
			transferLine1.WE_WL = packingLocation.PK;
			transferLine1.FinaliseDocketLine();
			Factory.Save();

			AssertEquals("Precondition", InventoryStatus.Codes.ReadyToPack, transferLine1.WE_CurrentInventoryStatus);
			AssertEquals("Precondition", WhsOrderStatus.Codes.ReadyToPack, order.WarehouseOrderStatus);
			AssertEquals("Precondition", false, order.PackageJob.IsPacked(pickLine));

			var productInfo = new WhsPackageProductInfo { ProductPK = data.Part1.PK.ToGuid(), ExpectedQty = 12m, Quantity = 12m };
			var packageInfo = new PackageForPackingInfo()
			{
				ToteID = "ABC",
				OrderReference = "O1",
				IsTote = true,
				IsPackageSplitForPacking = false,
				DocketID = order.WD_DocketID,
				ScannedProductInfos = new[] { productInfo }
			};

			var webService = GetNewWebService(data.Whs1, staff: user);
			var response = webService.PackOrderToPackage(packageInfo);
			AssertEquals("Should be error.", ErrorTypes.BusinessValidationError, response.Error);
			AssertEquals("Should be error.", "Failed to pack Order 'O1' into 'ABC'.", response.ErrorMessage);
			AssertEquals(Guid.Empty, response.NewPackagePK);
			Assert(response.NewPackageID.IsNullOrEmpty());
		}

		public void TestPackOrderToPackage_UnableToPackOrder_UnmatchedProduct()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var user = Helper.CreateGlbStaff("AAA", "AAAAA");

			var packingStationLocationType = Helper.CreateLocationType("XYZ", "Test", false, 0, LocationClasses.Codes.PST);
			var newRow = Helper.CreateRowAndGenerateLocations(data.Whs1, "P");
			var packingLocation = newRow.Locations[0];
			packingLocation.WLV_WLT_LocationType = packingStationLocationType.PK;
			Factory.Save();

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			Helper.CreatePickNew(order);

			var pickLine = orderLine.PickLines.Single();
			var transferLine = Helper.PickAndMakeInTransitTransfer(pickLine, ZDateTimeOffset.Now);
			transferLine.WE_WL = packingLocation.PK;
			transferLine.FinaliseDocketLine();
			Factory.Save();

			AssertEquals("Precondition", InventoryStatus.Codes.ReadyToPack, transferLine.WE_CurrentInventoryStatus);
			AssertEquals("Precondition", WhsOrderStatus.Codes.ReadyToPack, order.WarehouseOrderStatus);
			AssertEquals("Precondition", false, order.PackageJob.IsPacked(pickLine));

			var productInfo1 = new WhsPackageProductInfo { ProductPK = data.Part1.PK.ToGuid(), ExpectedQty = 10m, Quantity = 10m };
			var productInfo2 = new WhsPackageProductInfo { ProductPK = data.Part2.PK.ToGuid(), ExpectedQty = 15m, Quantity = 15m };
			var packageInfo = new PackageForPackingInfo()
			{
				ToteID = "ABC",
				OrderReference = "O1",
				IsTote = true,
				IsPackageSplitForPacking = false,
				DocketID = order.WD_DocketID,
				ScannedProductInfos = new[] { productInfo1, productInfo2 }
			};

			var webService = GetNewWebService(data.Whs1, staff: user);
			var response = webService.PackOrderToPackage(packageInfo);
			AssertEquals("Should be error.", ErrorTypes.BusinessValidationError, response.Error);
			AssertEquals("Should be error.", "Failed to pack Order 'O1' into 'ABC'.", response.ErrorMessage);
			AssertEquals(Guid.Empty, response.NewPackagePK);
			Assert(response.NewPackageID.IsNullOrEmpty());
		}

		public void TestPackOrderToPackage_PickByBOM()
		{
			TestPackOrderToPackage_PickByBOMCore(partialPacking: false);
		}

		public void TestPackOrderToPackage_PickByBOM_PartialPacking()
		{
			TestPackOrderToPackage_PickByBOMCore(partialPacking: true);
		}

		void TestPackOrderToPackage_PickByBOMCore(bool partialPacking)
		{
			var user = Helper.CreateGlbStaff("AAA", "AAAAA");
			var data = new TestDataSimpleEnvironment(Factory);
			var bike = Helper.CreateProduct(data.Org1, "BIKE");
			bike.OP_IsComponentPickedOnSalesOrder = true;
			var wheel = Helper.CreateProduct(data.Org1, "WHEEL");
			var frame = Helper.CreateProduct(data.Org1, "FRAME");
			Helper.CreateProductBOM(bike, wheel, 2m, "UNT");
			Helper.CreateProductBOM(bike, frame, 1m, "UNT");

			var packingStationLocationType = Helper.CreateLocationType("XYZ", "Test", false, 0, LocationClasses.Codes.PST);
			var newRow = Helper.CreateRowAndGenerateLocations(data.Whs1, "P");
			var packingLocation = newRow.Locations[0];
			packingLocation.WLV_WLT_LocationType = packingStationLocationType.PK;
			Factory.Save();

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", wheel, 50m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", frame, 50m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine1 = Helper.CreateWhsOrderLine(order, bike, 10m);
			var pick = Helper.CreatePickNew(order);
			Factory.Save();

			var kitPickLine = orderLine1.PickLines.Single();
			var wheelOrderLine = orderLine1.ChildComponentLines.Single(l => l.WE_OP == wheel.PK);
			var frameOrderLine = orderLine1.ChildComponentLines.Single(l => l.WE_OP == frame.PK);
			var wheelPickLine = wheelOrderLine.PickLines.Single();
			var framePickLine = frameOrderLine.PickLines.Single();
			wheelPickLine.WZ_PickedDateTime = ZDateTimeOffset.Now;
			wheelPickLine.WZ_GS_NKAssignedTo = user.GS_Code;
			framePickLine.WZ_PickedDateTime = ZDateTimeOffset.Now;
			framePickLine.WZ_GS_NKAssignedTo = user.GS_Code;
			Factory.Save();

			// putaway all wheels and frames
			var webService0 = GetNewWebService(data.Whs1, staff: user);
			var response0 = webService0.PutawayStockInDockDoorOrPackingStation(pick.PK.ToGuid(), PickJobType.Pick, packingLocation.WLV_LocationString);
			CombineAssertions(() =>
			{
				AssertEquals("Should be no error.", ErrorTypes.None, response0.Error);
				AssertNull("Should be no error.", response0.ErrorMessage);
			});

			AssertEquals("Precondition", InventoryStatus.Codes.ReadyToPack, kitPickLine.InventoryLine.WE_CurrentInventoryStatus);
			AssertEquals("Precondition: This is intentional as the Component Lines are staged to be assembled from the Order's point of view", InventoryStatus.Codes.Staged, wheelPickLine.InventoryLine.WE_CurrentInventoryStatus);
			AssertEquals("Precondition: This is intentional as the Component Lines are staged to be assembled from the Order's point of view", InventoryStatus.Codes.Staged, framePickLine.InventoryLine.WE_CurrentInventoryStatus);
			AssertEquals("Precondition", WhsOrderStatus.Codes.ReadyToPack, order.WarehouseOrderStatus);
			AssertEquals("Precondition", false, order.PackageJob.IsPacked(kitPickLine));
			AssertEquals("Precondition", false, order.PackageJob.IsPacked(wheelPickLine));
			AssertEquals("Precondition", false, order.PackageJob.IsPacked(framePickLine));
			AssertEquals("Order has no packages.", 0, order.PackageJob.Packages.Count);

			var productInfo1 = new WhsPackageProductInfo { ProductPK = bike.PK.ToGuid(), ExpectedQty = 10m, Quantity = partialPacking ? 9m : 10m };
			var packageInfo = new PackageForPackingInfo()
			{
				ToteID = "ABC",
				OrderReference = "O1",
				IsPackageSplitForPacking = false,
				DocketID = order.WD_DocketID,
				IsTote = true,
				ScannedProductInfos = new[] { productInfo1 }
			};

			var webService = GetNewWebService(data.Whs1, staff: user);
			var response = webService.PackOrderToPackage(packageInfo);
			CombineAssertions(() =>
			{
				AssertEquals("Should be no error.", ErrorTypes.None, response.Error);
				AssertNull("Should be no error.", response.ErrorMessage);
			});

			if (partialPacking)
			{
				kitPickLine = orderLine1.PickLines.Single(pl => pl.WZ_Units == 9m);
				var splitKitPickLine = orderLine1.PickLines.Single(pl => pl.WZ_Units == 1m);
				AssertEquals(false, order.PackageJob.IsPacked(splitKitPickLine));
			}

			AssertEquals(true, order.PackageJob.IsPacked(kitPickLine));
			AssertEquals(false, order.PackageJob.IsPacked(wheelPickLine));
			AssertEquals(false, order.PackageJob.IsPacked(framePickLine));

			var orderInNewFactory = new BusinessObjectFactory().Load<WhsOrder>(order.PK);
			var tote = orderInNewFactory.PackageJob.Packages.Single(pkg => pkg.KP_PackageID == "ABC");
			AssertNotNull("Tote is created.", tote);
			AssertEquals(true, tote.GetIsTote());
			AssertEquals(tote.PK.ToGuid(), response.NewPackagePK);
			AssertEquals(true, response.NewPackageID.IsNullOrEmpty());

			AssertEquals("There are 1 packed item divot in the package.", 1, tote.PackedItemDivots.Count);
			var divot1 = tote.PackedItemDivots.Single();
			AssertEquals(kitPickLine.PK, divot1.KI_ParentID);
			AssertEquals(partialPacking ? 9m : 10m, divot1.KI_PackedQty);
		}

		public void TestPackOrderToPackage_PickByBOM_WithOrderedComponents()
		{
			var user = Helper.CreateGlbStaff("AAA", "AAAAA");
			var data = new TestDataSimpleEnvironment(Factory);
			var bike = Helper.CreateProduct(data.Org1, "BIKE");
			bike.OP_IsComponentPickedOnSalesOrder = true;
			var wheel = Helper.CreateProduct(data.Org1, "WHEEL");
			var frame = Helper.CreateProduct(data.Org1, "FRAME");
			Helper.CreateProductBOM(bike, wheel, 2m, "UNT");
			Helper.CreateProductBOM(bike, frame, 1m, "UNT");

			var packingStationLocationType = Helper.CreateLocationType("XYZ", "Test", false, 0, LocationClasses.Codes.PST);
			var newRow = Helper.CreateRowAndGenerateLocations(data.Whs1, "P");
			var packingLocation = newRow.Locations[0];
			packingLocation.WLV_WLT_LocationType = packingStationLocationType.PK;
			Factory.Save();

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", wheel, 50m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", frame, 50m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine1 = Helper.CreateWhsOrderLine(order, bike, 5m);
			var orderLine2 = Helper.CreateWhsOrderLine(order, frame, 1m);
			var pick = Helper.CreatePickNew(order);
			Factory.Save();

			var kitPickLine = orderLine1.PickLines.Single();
			var wheelOrderLine = orderLine1.ChildComponentLines.Single(l => l.WE_OP == wheel.PK);
			var frameOrderLine = orderLine1.ChildComponentLines.Single(l => l.WE_OP == frame.PK);
			var wheelPickLine = wheelOrderLine.PickLines.Single();
			var framePickLine = frameOrderLine.PickLines.Single();
			var normalFramePickLine = orderLine2.PickLines.Single();
			wheelPickLine.WZ_PickedDateTime = ZDateTimeOffset.Now;
			wheelPickLine.WZ_GS_NKAssignedTo = user.GS_Code;
			framePickLine.WZ_PickedDateTime = ZDateTimeOffset.Now;
			framePickLine.WZ_GS_NKAssignedTo = user.GS_Code;
			normalFramePickLine.WZ_PickedDateTime = ZDateTimeOffset.Now;
			normalFramePickLine.WZ_GS_NKAssignedTo = user.GS_Code;
			Factory.Save();

			// putaway all wheels and frames
			var webService0 = GetNewWebService(data.Whs1, staff: user);
			var response0 = webService0.PutawayStockInDockDoorOrPackingStation(pick.PK.ToGuid(), PickJobType.Pick, packingLocation.WLV_LocationString);
			CombineAssertions(() =>
			{
				AssertEquals("Should be no error.", ErrorTypes.None, response0.Error);
				AssertNull("Should be no error.", response0.ErrorMessage);
			});

			AssertEquals("Precondition", InventoryStatus.Codes.ReadyToPack, kitPickLine.InventoryLine.WE_CurrentInventoryStatus);
			AssertEquals("Precondition: This is intentional as the Component Lines are staged to be assembled from the Order's point of view", InventoryStatus.Codes.Staged, wheelPickLine.InventoryLine.WE_CurrentInventoryStatus);
			AssertEquals("Precondition: This is intentional as the Component Lines are staged to be assembled from the Order's point of view", InventoryStatus.Codes.Staged, framePickLine.InventoryLine.WE_CurrentInventoryStatus);
			AssertEquals("Precondition", WhsOrderStatus.Codes.ReadyToPack, order.WarehouseOrderStatus);
			AssertEquals("Precondition", false, order.PackageJob.IsPacked(kitPickLine));
			AssertEquals("Precondition", false, order.PackageJob.IsPacked(wheelPickLine));
			AssertEquals("Precondition", false, order.PackageJob.IsPacked(framePickLine));
			AssertEquals("Order has no packages.", 0, order.PackageJob.Packages.Count);

			var productInfo1 = new WhsPackageProductInfo { ProductPK = bike.PK.ToGuid(), ExpectedQty = 5m, Quantity = 5m };
			var productInfo2 = new WhsPackageProductInfo { ProductPK = frame.PK.ToGuid(), ExpectedQty = 1m, Quantity = 1m };
			var packageInfo = new PackageForPackingInfo()
			{
				ToteID = "ABC",
				OrderReference = "O1",
				IsPackageSplitForPacking = false,
				DocketID = order.WD_DocketID,
				IsTote = true,
				ScannedProductInfos = new[] { productInfo1, productInfo2 }
			};

			var webService = GetNewWebService(data.Whs1, staff: user);
			var response = webService.PackOrderToPackage(packageInfo);
			CombineAssertions(() =>
			{
				AssertEquals("Should be no error.", ErrorTypes.None, response.Error);
				AssertNull("Should be no error.", response.ErrorMessage);
			});

			AssertEquals(true, order.PackageJob.IsPacked(kitPickLine));
			AssertEquals(false, order.PackageJob.IsPacked(wheelPickLine));
			AssertEquals(false, order.PackageJob.IsPacked(framePickLine));

			var orderInNewFactory = new BusinessObjectFactory().Load<WhsOrder>(order.PK);
			var tote = orderInNewFactory.PackageJob.Packages.Single(pkg => pkg.KP_PackageID == "ABC");
			AssertNotNull("Tote is created.", tote);
			AssertEquals(true, tote.GetIsTote());
			AssertEquals(tote.PK.ToGuid(), response.NewPackagePK);
			AssertEquals(true, response.NewPackageID.IsNullOrEmpty());

			AssertEquals("There are 2 packed item divots in the package.", 2, tote.PackedItemDivots.Count);
			var divot1 = tote.PackedItemDivots.Single(di => di.KI_ParentID == kitPickLine.PK);
			var divot2 = tote.PackedItemDivots.Single(di => di.KI_ParentID == normalFramePickLine.PK);
			AssertEquals(5m, divot1.KI_PackedQty);
			AssertEquals(1m, divot2.KI_PackedQty);
		}

		public void TestPackOrderToPackage_PickByBOM_PutawayComponentsCannotBuildKitsEvenly()
		{
			var user = Helper.CreateGlbStaff("AAA", "AAAAA");
			var data = new TestDataSimpleEnvironment(Factory);
			var bike = Helper.CreateProduct(data.Org1, "BIKE");
			bike.OP_IsComponentPickedOnSalesOrder = true;
			var wheel = Helper.CreateProduct(data.Org1, "WHEEL");
			var frame = Helper.CreateProduct(data.Org1, "FRAME");
			Helper.CreateProductBOM(bike, wheel, 2m, "UNT");
			Helper.CreateProductBOM(bike, frame, 1m, "UNT");

			var packingStationLocationType = Helper.CreateLocationType("XYZ", "Test", false, 0, LocationClasses.Codes.PST);
			var newRow = Helper.CreateRowAndGenerateLocations(data.Whs1, "P");
			var packingLocation = newRow.Locations[0];
			packingLocation.WLV_WLT_LocationType = packingStationLocationType.PK;
			Factory.Save();

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", wheel, 50m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", frame, 50m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine1 = Helper.CreateWhsOrderLine(order, bike, 5m);
			var pick = Helper.CreatePickNew(order);
			Factory.Save();

			var kitPickLine = orderLine1.PickLines.Single();
			var wheelOrderLine = orderLine1.ChildComponentLines.Single(l => l.WE_OP == wheel.PK);
			var frameOrderLine = orderLine1.ChildComponentLines.Single(l => l.WE_OP == frame.PK);
			var wheelPickLine = wheelOrderLine.PickLines.Single();
			var framePickLine1 = frameOrderLine.PickLines.Single();
			var framePickLine2 = framePickLine1.Split(2m);
			wheelPickLine.WZ_PickedDateTime = ZDateTimeOffset.Now;
			wheelPickLine.WZ_GS_NKAssignedTo = user.GS_Code;
			framePickLine1.WZ_PickedDateTime = ZDateTimeOffset.Now;
			framePickLine1.WZ_GS_NKAssignedTo = user.GS_Code;
			Factory.Save();

			// putaway all wheels and 3 frames, so there will only be 3 bikes packable
			var webService0 = GetNewWebService(data.Whs1, staff: user);
			var response0 = webService0.PutawayStockInDockDoorOrPackingStation(pick.PK.ToGuid(), PickJobType.Pick, packingLocation.WLV_LocationString);
			CombineAssertions(() =>
			{
				AssertEquals("Should be no error.", ErrorTypes.None, response0.Error);
				AssertNull("Should be no error.", response0.ErrorMessage);
			});

			AssertEquals("Precondition", InventoryStatus.Codes.ReadyToPack, kitPickLine.InventoryLine.WE_CurrentInventoryStatus);
			AssertEquals("Precondition: This is intentional as the Component Lines are staged to be assembled from the Order's point of view", InventoryStatus.Codes.Staged, wheelPickLine.InventoryLine.WE_CurrentInventoryStatus);
			AssertEquals("Precondition: This is intentional as the Component Lines are staged to be assembled from the Order's point of view", InventoryStatus.Codes.Staged, framePickLine1.InventoryLine.WE_CurrentInventoryStatus);
			AssertEquals("Precondition", DocketStatus.Codes.Picking, order.WarehouseOrderStatus);
			AssertEquals("Precondition", false, order.PackageJob.IsPacked(kitPickLine));
			AssertEquals("Precondition", false, order.PackageJob.IsPacked(wheelPickLine));
			AssertEquals("Precondition", false, order.PackageJob.IsPacked(framePickLine1));
			AssertEquals("Order has no packages.", 0, order.PackageJob.Packages.Count);

			var productInfo1 = new WhsPackageProductInfo { ProductPK = bike.PK.ToGuid(), ExpectedQty = 5m, Quantity = 3m };
			var packageInfo = new PackageForPackingInfo()
			{
				ToteID = "ABC",
				OrderReference = "O1",
				IsPackageSplitForPacking = false,
				DocketID = order.WD_DocketID,
				IsTote = true,
				ScannedProductInfos = new[] { productInfo1 }
			};

			var webService = GetNewWebService(data.Whs1, staff: user);
			var response = webService.PackOrderToPackage(packageInfo);
			CombineAssertions(() =>
			{
				AssertEquals("Should be no error.", ErrorTypes.None, response.Error);
				AssertNull("Should be no error.", response.ErrorMessage);
			});

			AssertEquals(true, order.PackageJob.IsPacked(kitPickLine));
			AssertEquals(false, order.PackageJob.IsPacked(wheelPickLine));
			AssertEquals(false, order.PackageJob.IsPacked(framePickLine1));

			var orderInNewFactory = new BusinessObjectFactory().Load<WhsOrder>(order.PK);
			var tote = orderInNewFactory.PackageJob.Packages.Single(pkg => pkg.KP_PackageID == "ABC");
			AssertNotNull("Tote is created.", tote);
			AssertEquals(true, tote.GetIsTote());
			AssertEquals(tote.PK.ToGuid(), response.NewPackagePK);
			AssertEquals(true, response.NewPackageID.IsNullOrEmpty());

			AssertEquals("There are 1 packed item divot in the package.", 1, tote.PackedItemDivots.Count);
			var divot1 = tote.PackedItemDivots.Single(di => di.KI_ParentID == kitPickLine.PK);
			AssertEquals(3m, divot1.KI_PackedQty);
		}

		public void TestPackOrderToPackage_DBHits()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var user = Helper.CreateGlbStaff("AAA", "AAAAA");

			var packingStationLocationType = Helper.CreateLocationType("XYZ", "Test", false, 0, LocationClasses.Codes.PST);
			var newRow = Helper.CreateRowAndGenerateLocations(data.Whs1, "P");
			var packingLocation = newRow.Locations[0];
			packingLocation.WLV_WLT_LocationType = packingStationLocationType.PK;
			Factory.Save();

			var products = new List<OrgSupplierPart>();
			for (var i = 0; i < 10; i++)
			{
				var product = Helper.CreateProduct($"P1{i}", data.Org1);
				Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, $"R{i}", product, 10m);
				products.Add(product);
			}
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLines = new List<WhsOrderLine>();
			var productInfos = new List<WhsPackageProductInfo>();
			for (var i = 0; i < 10; i++)
			{
				var product = products[i];
				var orderLine = Helper.CreateWhsOrderLine(order, product, 10m);
				orderLines.Add(orderLine);

				var productInfo = new WhsPackageProductInfo { ProductPK = product.PK.ToGuid(), ExpectedQty = 10m, Quantity = 10m };
				productInfos.Add(productInfo);
			}
			Helper.CreatePickNew(order);

			foreach (var orderLine in orderLines)
			{
				var pickLine = orderLine.PickLines.Single();
				var transferLine = Helper.PickAndMakeInTransitTransfer(pickLine, ZDateTimeOffset.Now);
				transferLine.WE_WL = packingLocation.PK;
				transferLine.FinaliseDocketLine();
			}
			Factory.Save();

			AssertEquals("Precondition", WhsOrderStatus.Codes.ReadyToPack, order.WarehouseOrderStatus);

			var packageInfo = new PackageForPackingInfo()
			{
				ToteID = "ABC",
				OrderReference = "O1",
				IsTote = true,
				IsPackageSplitForPacking = false,
				DocketID = order.WD_DocketID,
				ScannedProductInfos = productInfos.ToArray(),
			};

			var expectedDBHits = new Dictionary<string, int>
			{
				{ OrgHeaderSchema.Constants.TableName, 1 },
				{ OrgMiscServSchema.Constants.TableName, 1 },
				{ OrgSupplierPartSchema.Constants.TableName, 1 },
				{ PkgPackageSchema.Constants.TableName, 1 },
				{ PkgPackageJobSchema.Constants.TableName, 1 },
				{ RefPackTypeSchema.Constants.TableName, 1 },
				{ WhsDocketSchema.Constants.TableName, 4 },
				{ WhsDocketLineSchema.Constants.TableName, 3 },
				{ WhsPickSchema.Constants.TableName, 1 },
				{ WhsPickLineSchema.Constants.TableName, 1 },
				{ WhsWarehouseSchema.Constants.TableName, 1 },
			};

			var webService = GetNewWebService(data.Whs1, staff: user);
			using (TestCaseWithFactory.AssertDbHitsWithUsefulQueryInformation(expectedDBHits, webService.Factory))
			{
				var response = webService.PackOrderToPackage(packageInfo);
				AssertEquals("Should be no error.", ErrorTypes.None, response.Error);
				AssertNull("Should be no error.", response.ErrorMessage);
			}

			var orderInNewFactory = new BusinessObjectFactory().Load<WhsOrder>(order.PK);
			var tote = orderInNewFactory.PackageJob.Packages.Single(pkg => pkg.KP_PackageID == "ABC");
			AssertNotNull("Tote is created.", tote);
			Assert(tote.GetIsTote());
			AssertEquals("There are 10 packed item divots in the package.", 10, tote.PackedItemDivots.Count);
		}

		public void TestPackOrderToPackage_NotTote()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var user = Helper.CreateGlbStaff("AAA", "AAAAA");
			Helper.SetProductWeightAndVolume(data.Part1, 0m, "", 0m, "");
			Helper.SetProductWeightAndVolume(data.Part2, 0m, "", 0m, "");
			PackingRegistry.Instance.SetVolumeUnitForTest(Volume.Litre);

			var packingStationLocationType = Helper.CreateLocationType("XYZ", "Test", false, 0, LocationClasses.Codes.PST);
			var newRow = Helper.CreateRowAndGenerateLocations(data.Whs1, "P");
			var packingLocation = newRow.Locations[0];
			packingLocation.WLV_WLT_LocationType = packingStationLocationType.PK;
			Helper.Factory.Save();

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part2, 15m);
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			order.WD_TotalCubicUnit = Volume.Litre;
			var orderLine1 = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var orderLine2 = Helper.CreateWhsOrderLine(order, data.Part2, 15m);
			Helper.CreatePickNew(order);

			var pickLine1 = orderLine1.PickLines.Single();
			var transferLine1 = Helper.PickAndMakeInTransitTransfer(pickLine1, ZDateTimeOffset.Now);
			transferLine1.WE_WL = packingLocation.PK;
			transferLine1.FinaliseDocketLine();

			var pickLine2 = orderLine2.PickLines.Single();
			var transferLine2 = Helper.PickAndMakeInTransitTransfer(pickLine2, ZDateTimeOffset.Now);
			transferLine2.WE_WL = packingLocation.PK;
			transferLine2.FinaliseDocketLine();
			Helper.Factory.Save();

			AssertEquals("Order has no packages.", false, order.PackageJob.Packages.Any());

			var productInfo1 = new WhsPackageProductInfo { ProductPK = data.Part1.PK.ToGuid(), ExpectedQty = 10m, Quantity = 10m };
			var productInfo2 = new WhsPackageProductInfo { ProductPK = data.Part2.PK.ToGuid(), ExpectedQty = 15m, Quantity = 15m };
			var packageInfo = new PackageForPackingInfo()
			{
				OrderReference = "O1",
				DocketID = order.WD_DocketID,
				IsDirectedPacking = true,
				IsTote = false,
				WeightUQ = Weight.Kilograms,
				PackType = PkgUnit.Carton,
				Weight = 8m,
				Length = 10m,
				Width = 5m,
				Height = 2m,
				DimensionUQ = Length.Centimetres,
				ScannedProductInfos = new[] { productInfo1, productInfo2 }
			};

			var webService = GetNewWebService(data.Whs1, staff: user);
			var response = webService.PackOrderToPackage(packageInfo);
			AssertEquals("Should be no error.", ErrorTypes.None, response.Error);
			AssertNull("Should be no error.", response.ErrorMessage);

			Assert(order.PackageJob.IsPacked(pickLine1));
			Assert(order.PackageJob.IsPacked(pickLine2));

			var orderInNewFactory = new BusinessObjectFactory().Load<WhsOrder>(order.PK);
			var newPackage = orderInNewFactory.PackageJob.Packages.Single(pkg => pkg.KP_PackageID == "O1-001");
			AssertNotNull("Package is created.", newPackage);
			AssertEquals(newPackage.PK.ToGuid(), response.NewPackagePK);
			AssertEquals("O1-001", response.NewPackageID);

			AssertEquals("There are 2 packed item divots in the package.", 2, newPackage.PackedItemDivots.Count);
			AssertEquals("Quantity on packed item divots is correct.", true, newPackage.PackedItemDivots.Any(divot => divot.PackedItem.Quantity == 10m));
			AssertEquals("Quantity on packed item divots is correct.", true, newPackage.PackedItemDivots.Any(divot => divot.PackedItem.Quantity == 15m));
			CombineAssertions(() =>
			{
				AssertEquals("PackageType should carton", PkgUnit.Carton, newPackage.KP_F3_NKPackType);
				AssertEquals("Package is not a tote.", false, newPackage.GetIsTote());
				AssertEquals("Package is not held.", false, newPackage.KP_IsHeld);
				AssertEquals("Weight should be copied from the package info", 8m, newPackage.KP_Weight);
				AssertEquals("WeightUQ should be copied from the package info", Weight.Kilograms, newPackage.KP_WeightUQ);
				AssertEquals("Length should be copied from the package info", 10m, newPackage.KP_Length);
				AssertEquals("Width should be copied from the package info", 5m, newPackage.KP_Width);
				AssertEquals("Height should be copied from the package info", 2m, newPackage.KP_Height);
				AssertEquals("DimensionUQ should be copied from the package info", Length.Centimetres, newPackage.KP_DimensionUQ);
			});
		}

		public void TestPackOrderToPackage_NotTote_FailsToPack()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var user = Helper.CreateGlbStaff("AAA", "AAAAA");
			var packingStationLocationType = Helper.CreateLocationType("XYZ", "Test", false, 0, LocationClasses.Codes.PST);
			var newRow = Helper.CreateRowAndGenerateLocations(data.Whs1, "P");
			var packingLocation = newRow.Locations[0];
			packingLocation.WLV_WLT_LocationType = packingStationLocationType.PK;
			Helper.Factory.Save();

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			order.WD_TotalCubicUnit = Volume.Litre;
			var orderLine1 = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			Helper.CreateWhsOrderLine(order, data.Part2, 15m);
			Helper.CreatePickNew(order);

			var pickLine = orderLine1.PickLines.Single();
			var transferLine = Helper.PickAndMakeInTransitTransfer(pickLine, ZDateTimeOffset.Now);
			transferLine.WE_WL = packingLocation.PK;
			transferLine.FinaliseDocketLine();
			Helper.Factory.Save();

			AssertEquals("Order has no packages.", false, order.PackageJob.Packages.Any());

			var productInfo1 = new WhsPackageProductInfo { ProductPK = data.Part1.PK.ToGuid(), ExpectedQty = 10m, Quantity = 10m };
			var productInfo2 = new WhsPackageProductInfo { ProductPK = data.Part2.PK.ToGuid(), ExpectedQty = 15m, Quantity = 15m };
			var packageInfo = new PackageForPackingInfo()
			{
				OrderReference = "O1",
				DocketID = order.WD_DocketID,
				IsDirectedPacking = true,
				IsTote = false,
				WeightUQ = Weight.Kilograms,
				PackType = PkgUnit.Carton,
				Weight = 8m,
				Length = 10m,
				Width = 5m,
				Height = 2m,
				DimensionUQ = Length.Centimetres,
				ScannedProductInfos = new[] { productInfo1, productInfo2 }
			};

			var webService = GetNewWebService(data.Whs1, staff: user);
			var response = webService.PackOrderToPackage(packageInfo);
			AssertEquals("Should be error.", ErrorTypes.BusinessValidationError, response.Error);
			AssertEquals("Should be error.", "Failed to pack Order 'O1' into carton.", response.ErrorMessage);
			AssertEquals(Guid.Empty, response.NewPackagePK);
			Assert(response.NewPackageID.IsNullOrEmpty());

			var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var orderInNewFactory = newFactory.Load<WhsOrder>(order.PK);
			AssertEquals("No packages created.", false, orderInNewFactory.PackageJob.Packages.Any());
		}

		public void TestPackOrderToPackage_NotTote_DBHits()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var user = Helper.CreateGlbStaff("AAA", "AAAAA");
			var packingStationLocationType = Helper.CreateLocationType("XYZ", "Test", false, 0, LocationClasses.Codes.PST);
			var newRow = Helper.CreateRowAndGenerateLocations(data.Whs1, "P");
			var packingLocation = newRow.Locations[0];
			packingLocation.WLV_WLT_LocationType = packingStationLocationType.PK;
			Factory.Save();

			var products = new List<OrgSupplierPart>();
			for (var i = 0; i < 10; i++)
			{
				var product = Helper.CreateProduct($"P1{i}", data.Org1);
				Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, $"R{i}", product, 10m);
				products.Add(product);
			}
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLines = new List<WhsOrderLine>();
			var productInfos = new List<WhsPackageProductInfo>();
			for (var i = 0; i < 10; i++)
			{
				var product = products[i];
				var orderLine = Helper.CreateWhsOrderLine(order, product, 10m);
				orderLines.Add(orderLine);

				var productInfo = new WhsPackageProductInfo { ProductPK = product.PK.ToGuid(), ExpectedQty = 10m, Quantity = 10m };
				productInfos.Add(productInfo);
			}
			Helper.CreatePickNew(order);

			foreach (var orderLine in orderLines)
			{
				var pickLine = orderLine.PickLines.Single();
				var transferLine = Helper.PickAndMakeInTransitTransfer(pickLine, ZDateTimeOffset.Now);
				transferLine.WE_WL = packingLocation.PK;
				transferLine.FinaliseDocketLine();
			}
			Factory.Save();

			AssertEquals("Order has no packages.", false, order.PackageJob.Packages.Any());

			var packageInfo = new PackageForPackingInfo()
			{
				IsDirectedPacking = true,
				IsTote = false,
				OrderReference = "O1",
				IsPackageSplitForPacking = false,
				DocketID = order.WD_DocketID,
				ScannedProductInfos = productInfos.ToArray(),
			};

			var expectedDBHits = new Dictionary<string, int>
			{
				{ OrgAddressSchema.Constants.TableName, 2 },
				{ OrgCusCodeSchema.Constants.TableName, 2 },
				{ OrgHeaderSchema.Constants.TableName, 2 },
				{ OrgMiscServSchema.Constants.TableName, 1 },
				{ OrgSupplierPartSchema.Constants.TableName, 1 },
				{ PkgPackageSchema.Constants.TableName, 1 },
				{ PkgPackageJobSchema.Constants.TableName, 1 },
				{ PkgPackageJobPackageHeaderPivotSchema.Constants.TableName, 1 },
				{ RefPackTypeSchema.Constants.TableName, 1 },
				{ WhsDocketSchema.Constants.TableName, 4 },
				{ WhsDocketLineSchema.Constants.TableName, 3 },
				{ WhsPickSchema.Constants.TableName, 1 },
				{ WhsPickLineSchema.Constants.TableName, 1 },
				{ WhsWarehouseSchema.Constants.TableName, 1 },
			};

			var webService = GetNewWebService(data.Whs1, staff: user);
			using (TestCaseWithFactory.AssertDbHitsWithUsefulQueryInformation(expectedDBHits, webService.Factory))
			{
				var response = webService.PackOrderToPackage(packageInfo);
				AssertEquals("Should be no error.", ErrorTypes.None, response.Error);
				AssertNull("Should be no error.", response.ErrorMessage);
				Assert(response.NewPackagePK != Guid.Empty);
				Assert(!response.NewPackageID.IsNullOrEmpty());
			}

			var orderInNewFactory = new BusinessObjectFactory().Load<WhsOrder>(order.PK);
			var package = orderInNewFactory.PackageJob.Packages.Single();
			AssertEquals("There are 10 packed item divots in the package.", 10, package.PackedItemDivots.Count);
		}

		#region Implementation

		BusinessObjectFactory Factory => Helper.Factory;

		PackingTestHelper PackingHelper => packingHelper ?? (packingHelper = new PackingTestHelper(Factory));
		PackingTestHelper packingHelper;

		#endregion
	}
}
