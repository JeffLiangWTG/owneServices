using System.Linq;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.Warehouse.Web.WebService.Business;

namespace Enterprise.Warehouse.Web.WebService.Testing
{
	class ShortToteTest : WhsSecureServiceTestCase
	{
		#region TestShortTote_ErrorsPackageIsNull

		public void TestShortTote_ErrorsPackageIsNull()
		{
			var webService = GetNewWebService();
			AssertBusinessValidationError(webService, "packageInfo cannot be null.", webService.ShortTote(null));
		}

		#endregion

		#region TestShortTote_ErrorsScannedProductsIsNull

		public void TestShortTote_ErrorsScannedProductsIsNull()
		{
			var webService = GetNewWebService();
			AssertBusinessValidationError(webService, "ScannedProductInfos cannot be null.",
				webService.ShortTote(new PackageForPackingInfo() { ScannedProductInfos = null }));
		}

		#endregion

		#region TestShortTote_ErrorsPackageIsEmpty

		public void TestShortTote_ErrorsPackageIsEmpty()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			var staff = Helper.CreateGlbStaff("OP1", "Test1");
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "Receive1", data.Part1, 100m);
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "Order1", data.Part1, 100m);
			var pick = Helper.CreatePickNew(order);
			var package = order.PackageJob.Packages.AddNew("PLT", "ABC");
			package.SetIsTote(true);

			var productInfos = new[] { new WhsPackageProductInfo { } };
			var packageInfo = new PackageForPackingInfo { DocketID = order.WD_DocketID, PK = package.PK.ToGuid(), ScannedProductInfos = productInfos, ToteID = package.KP_PackageID };
			Helper.Factory.Save();

			var webService1 = GetNewWebService();
			SetupSecurityHeader(webService1, data.Whs1, staff);
			var response = webService1.ShortTote(packageInfo);
			AssertEquals(ErrorTypes.BusinessValidationError, response.Error);
			AssertEquals("There is nothing packed in Package with ID 'ABC'.", response.ErrorMessage);
			AssertEquals("PickLine value should not change", 100m, order.Lines[0].PickLines[0].WZ_Units);
		}

		#endregion

		#region TestShortTote_ErrorsIfPackageIsClosed

		public void TestShortTote_ErrorsIfPackageIsClosed()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			var staff = Helper.CreateGlbStaff("OP1", "Test1");
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "Receive1", data.Part1, 100m);
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "Order1", data.Part1, 100m);
			var pick = Helper.CreatePickNew(order);
			var package = order.PackageJob.Packages.AddNew("PLT", "ABC");
			package.SetIsTote(true);
			package.Pack(order.Lines[0].ReleaseLines[0], 100m);

			var productInfos = new[] { new WhsPackageProductInfo { Quantity = 100m, ExpectedQty = 100m, ProductPK = data.Part1.PK.ToGuid() } };
			var packageInfo = new PackageForPackingInfo { DocketID = order.WD_DocketID, PK = package.PK.ToGuid(), ScannedProductInfos = productInfos, ToteID = package.KP_PackageID };
			package.KP_ClosedTimeUtc = ZDateTime.UtcNow;
			Helper.Factory.Save();

			var webService1 = GetNewWebService();
			SetupSecurityHeader(webService1, data.Whs1, staff);
			var response = webService1.ShortTote(packageInfo);
			AssertEquals(ErrorTypes.BusinessValidationError, response.Error);
			AssertEquals("Package with ID 'ABC' is already closed.", response.ErrorMessage);
		}

		#endregion

		#region TestShortTote_ErrorsPackageDoesNotExists

		public void TestShortTote_ErrorsPackageDoesNotExists()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			var staff = Helper.CreateGlbStaff("OP1", "Test1");
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "Receive1", data.Part1, 100m);
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "Order1", data.Part1, 100m);
			var pick = Helper.CreatePickNew(order);
			var package = order.PackageJob.Packages.AddNew("PLT", "ABC");
			package.SetIsTote(true);
			package.Pack(order.Lines[0].ReleaseLines[0], 100m);

			var productInfos = new[] { new WhsPackageProductInfo { Quantity = 100m, ExpectedQty = 100m, ProductPK = data.Part1.PK.ToGuid() } };
			var packageInfo = new PackageForPackingInfo { DocketID = order.WD_DocketID, PK = package.PK.ToGuid(), ScannedProductInfos = productInfos };
			Helper.Factory.Save();

			var webService1 = GetNewWebService();
			SetupSecurityHeader(webService1, data.Whs1, staff);
			var response = webService1.ShortTote(new PackageForPackingInfo { ToteID = "DEF", PK = ZGuid.NewZGuid().ToGuid(), ScannedProductInfos = productInfos });
			AssertEquals(ErrorTypes.BusinessValidationError, response.Error);
			AssertEquals("Package ID 'DEF' does not exist.", response.ErrorMessage);
		}

		#endregion

		#region TestShortTote_ZeroQtyShouldUnpackProductFromPackage

		public void TestShortTote_ZeroQtyShouldUnpackProductFromPackage()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			var staff = Helper.CreateGlbStaff("OP1", "Test1");
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "Receive1", data.Part1, 100m);
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "Order1", data.Part1, 100m);
			var pick = Helper.CreatePickNew(order);
			var package = order.PackageJob.Packages.AddNew("PLT", "ABC");
			package.SetIsTote(true);
			package.Pack(order.Lines[0].ReleaseLines[0], 100m);

			var productInfos = new[] { new WhsPackageProductInfo { Quantity = 0m, ExpectedQty = 100m, ProductPK = data.Part1.PK.ToGuid() } };
			var packageInfo = new PackageForPackingInfo { DocketID = order.WD_DocketID, PK = package.PK.ToGuid(), ScannedProductInfos = productInfos };
			Helper.Factory.Save();

			var webService1 = GetNewWebService();
			SetupSecurityHeader(webService1, data.Whs1, staff);
			var response = webService1.ShortTote(packageInfo);

			AssertEquals("There should be no errors in the response.", true, string.IsNullOrEmpty(response.ErrorMessage));
			AssertEquals("Quantity is 0 on package.", 0m, package.PackedItemDivots.Sum(divot => divot.KI_PackedQty));
			AssertEquals("PickLine count is still 1", 1, order.Lines[0].PickLines.Count);
			AssertEquals("PickLine value should not change", 100m, order.Lines[0].PickLines[0].WZ_Units);
		}

		#endregion

		#region TestShortTote_ShortQtyUnpacksPickLinesCorrectly

		public void TestShortTote_ShortQtyUnpacksPickLinesCorrectly()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			var staff = Helper.CreateGlbStaff("OP1", "Test1");
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "Receive1", data.Part1, 100m);
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "Order1", data.Part1, 100m);
			var pick = Helper.CreatePickNew(order);
			var package = order.PackageJob.Packages.AddNew("PLT", "ABC");
			package.SetIsTote(true);
			package.Pack(order.Lines[0].ReleaseLines[0], 100m);

			var productInfos = new[] { new WhsPackageProductInfo { Quantity = 37m, ExpectedQty = 100m, ProductPK = data.Part1.PK.ToGuid() } };
			var packageInfo = new PackageForPackingInfo { DocketID = order.WD_DocketID, PK = package.PK.ToGuid(), ScannedProductInfos = productInfos, ToteID = package.KP_PackageID };
			Helper.Factory.Save();

			var webService1 = GetNewWebService();
			SetupSecurityHeader(webService1, data.Whs1, staff);
			var response = webService1.ShortTote(packageInfo);

			AssertEquals("Quantity is 37 in package.", 37m, package.PackedItemDivots.Sum(divot => divot.KI_PackedQty));
			AssertEquals("PickLine count is now 2", 2, order.Lines[0].PickLines.Count);
			AssertEquals("A split off PickLine with value  of 37 should exist", true, order.Lines[0].PickLines.Any(pl => pl.WZ_Units == 37m));
			AssertEquals("The the original pickLine value should change to 63.", true, order.Lines[0].PickLines.Any(pl => pl.WZ_Units == 63m));
		}

		#endregion

		#region TestShortTote_FullQtyProductAndShortOnAnother_UnpacksPickLinesCorrectly

		public void TestShortTote_FullQtyProductAndShortOnAnother_UnpacksPickLinesCorrectly()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			var staff = Helper.CreateGlbStaff("OP1", "Test1");
			var receive1 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "Receive1", data.Part1, 100m);
			var receive2 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "Receive2", data.Part2, 100m);
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine1 = Helper.CreateWhsOrderLine(order, data.Part1, 6m);
			var orderLine2 = Helper.CreateWhsOrderLine(order, data.Part2, 8m);
			var pick = Helper.CreatePickNew(order);
			var package = order.PackageJob.Packages.AddNew("PLT", "ABC");
			package.SetIsTote(true);
			package.Pack(orderLine1.ReleaseLines[0], 6m);
			package.Pack(orderLine2.ReleaseLines[0], 8m);

			var productInfos = new[]
{
				new WhsPackageProductInfo { Quantity = 6m, ExpectedQty = 6m, ProductPK = data.Part1.PK.ToGuid() },
				new WhsPackageProductInfo { Quantity = 1m, ExpectedQty = 8m, ProductPK = data.Part2.PK.ToGuid() }
			};
			var packageInfo = new PackageForPackingInfo { DocketID = order.WD_DocketID, PK = package.PK.ToGuid(), ScannedProductInfos = productInfos, ToteID = package.KP_PackageID };
			Helper.Factory.Save();

			AssertEquals("Preconditon - Quantity on packed item divots is correct - 6m.", true, package.PackedItemDivots.Any(divot => divot.PackedItem.Quantity == 6m));
			AssertEquals("Preconditon - Quantity on packed item divots is correct - 8m.", true, package.PackedItemDivots.Any(divot => divot.PackedItem.Quantity == 8m));

			var webService1 = GetNewWebService();
			SetupSecurityHeader(webService1, data.Whs1, staff);
			var response = webService1.ShortTote(packageInfo);

			AssertEquals("There should be no errors in the response.", true, string.IsNullOrEmpty(response.ErrorMessage));
			AssertEquals("Package has 2 divots", 2, package.PackedItemDivots.Count);
			AssertEquals("Quantity on packed item divots is correct - unsplit.", true, package.PackedItemDivots.Any(divot => divot.PackedItem.Quantity == 6m));
			AssertEquals("Quantity on packed item divots is correct - split packed.", true, package.PackedItemDivots.Any(divot => divot.PackedItem.Quantity == 1m));
			AssertEquals("Quantity on packed item divots is correct - split unpacked.", false, package.PackedItemDivots.Any(divot => divot.PackedItem.Quantity == 7m));

			AssertEquals("OrderLine1 pickLine count is still 1", 1, orderLine1.PickLines.Count);
			AssertEquals("OrderLine2 pickLine count is now 2", 2, orderLine2.PickLines.Count);
			AssertEquals("orderLine2 should have a pickLine with value 1", true, orderLine2.PickLines.Any(pl => pl.WZ_Units == 1m && pl.WZ_WE_InventoryLine == receive2.Lines[0].PK));
			AssertEquals("orderLine2 should have a pickLine with value 7", true, orderLine2.PickLines.Any(pl => pl.WZ_Units == 7m && pl.WZ_WE_InventoryLine == receive2.Lines[0].PK));
		}

		#endregion

		#region TestShortTote_WithRCAs_UnpacksCorrectly

		public void TestShortTote_WithRCAs_UnpacksCorrectly()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			var staff = Helper.CreateGlbStaff("OP1", "Test1");
			Helper.Factory.Save();

			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, PartAttributeTypeList.Codes.Mandatory);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true, setReleaseCaptured: true);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Two, PartAttributeTypeList.Codes.Mandatory);
			Helper.SetProductAttributeUse(data.Org1, data.Part2, AttributeNumber.Two, true, setReleaseCaptured: true);

			var receive1 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 6m);
			var receive2 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part2, 8m);
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine1 = Helper.CreateWhsOrderLine(order, data.Part1, 6m);
			var orderLine2 = Helper.CreateWhsOrderLine(order, data.Part2, 8m);
			Helper.CreatePickNew(order);
			Helper.Factory.Save();

			orderLine1.ReleaseLines[0].PartAttribute1 = "123";
			orderLine2.ReleaseLines[0].PartAttribute2 = "456";

			var package = order.PackageJob.Packages.AddNew();
			package.KP_PackageID = "TOTE01";
			package.SetIsTote(true);
			package.Pack(orderLine1.ReleaseLines[0], 6m);
			package.Pack(orderLine2.ReleaseLines[0], 8m);

			var productInfos = new[]
			{
				new WhsPackageProductInfo { Quantity = 4m, ExpectedQty = 6m, ProductPK = data.Part1.PK.ToGuid() },
				new WhsPackageProductInfo { Quantity = 5m, ExpectedQty = 8m, ProductPK = data.Part2.PK.ToGuid() }
			};
			var packageInfo = new PackageForPackingInfo { DocketID = order.WD_DocketID, PK = package.PK.ToGuid(), ScannedProductInfos = productInfos, ToteID = package.KP_PackageID };

			Helper.Factory.Save();

			AssertEquals("Prerequisite: Pick line for product1 has captured attributes.", true, orderLine1.PickLines.Any(pickLine => pickLine.HasReleaseCapturedAttribs));
			AssertEquals("Prerequisite: Pick line for product2 has captured attributes.", true, orderLine2.PickLines.Any(pickLine => pickLine.HasReleaseCapturedAttribs));

			var webService1 = GetNewWebService();
			SetupSecurityHeader(webService1, data.Whs1, staff);
			var response = webService1.ShortTote(packageInfo);

			AssertEquals("There should be no errors in the response.", true, string.IsNullOrEmpty(response.ErrorMessage));
			AssertEquals("Quantity is 9 in package.", 9m, package.PackedItemDivots.Sum(divot => divot.KI_PackedQty));
			AssertEquals("Quantity on packed item divots is correct.", true, package.PackedItemDivots.Any(divot => divot.PackedItem.Quantity == 4m && ((WhsPickLine)divot.PackedItem).WZ_ReleaseCapturedPartAttrib1 == "123"));
			AssertEquals("Quantity on packed item divots is correct.", true, package.PackedItemDivots.Any(divot => divot.PackedItem.Quantity == 5m && ((WhsPickLine)divot.PackedItem).WZ_ReleaseCapturedPartAttrib2 == "456"));
			AssertEquals("Pick lines for product1 exist and are correct", true, orderLine1.PickLines.Any(pl => pl.WZ_Units == 4m && pl.WZ_WE_InventoryLine == receive1.Lines[0].PK));
			AssertEquals("Pick lines for product1 exist and are correct", true, orderLine1.PickLines.Any(pl => pl.WZ_Units == 2m && pl.WZ_WE_InventoryLine == receive1.Lines[0].PK));
			AssertEquals("Pick lines for product2 exist and are correct", true, orderLine2.PickLines.Any(pl => pl.WZ_Units == 5m && pl.WZ_WE_InventoryLine == receive2.Lines[0].PK));
			AssertEquals("Pick lines for product2 exist and are correct", true, orderLine2.PickLines.Any(pl => pl.WZ_Units == 3m && pl.WZ_WE_InventoryLine == receive2.Lines[0].PK));
		}

		#endregion

		#region TestShortTote_ErrorsIfPackageIsNotTote

		public void TestShortTote_ErrorsIfPackageIsNotTote()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			var staff = Helper.CreateGlbStaff("OP1", "Test1");
			Helper.Factory.Save();

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			Helper.Factory.Save();

			Helper.CreatePickNew(order);

			var package = order.PackageJob.Packages.AddNew();
			package.KP_PackageID = "TOTE01";
			package.SetIsTote(false);
			package.Pack(orderLine.ReleaseLines[0], 10m);
			Helper.Factory.Save();

			AssertEquals("Precondition: package is not a tote.", false, package.GetIsTote());
			var productInfos = new[] { new WhsPackageProductInfo { Quantity = 100m, ExpectedQty = 100m, ProductPK = data.Part1.PK.ToGuid() } };
			var packageInfo = new PackageForPackingInfo { DocketID = order.WD_DocketID, PK = package.PK.ToGuid(), ScannedProductInfos = productInfos, ToteID = package.KP_PackageID };

			var webService1 = GetNewWebService();
			SetupSecurityHeader(webService1, data.Whs1, staff);
			var response = webService1.ShortTote(packageInfo);
			AssertEquals("There should be an error in the response.", false, string.IsNullOrEmpty(response.ErrorMessage));
			AssertEquals("The error should be correct.", "The package is not a tote.", response.ErrorMessage);
		}

		#endregion

		#region TestShortTote_ShortToteMultipleTimes

		public void TestShortTote_ShortToteMultipleTimes()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			var staff = Helper.CreateGlbStaff("OP1", "Test1");
			Helper.Factory.Save();

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 100m);
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 100m);
			Helper.Factory.Save();

			Helper.CreatePickNew(order);

			var package = order.PackageJob.Packages.AddNew();
			package.KP_PackageID = "TOTE01";
			package.SetIsTote(true);
			package.Pack(orderLine.ReleaseLines[0], 100m);
			Helper.Factory.Save();

			AssertEquals("Precondition: package is a tote.", true, package.GetIsTote());
			AssertEquals("Precondition: there is 1 package.", 1, order.PackageJob.Packages.Count);
			AssertEquals("Precondition: package divots is 1.", 1, package.PackedItemDivots.Count);
			AssertEquals("Precondition: package quantity is 100.", 100m, package.PackedItemDivots[0].KI_PackedQty);

			var productInfos1 = new[]
			{
				new WhsPackageProductInfo { Quantity = 70m, ExpectedQty = 100m, ProductPK = data.Part1.PK.ToGuid() }
			};
			var packageInfo1 = new PackageForPackingInfo { DocketID = order.WD_DocketID, PK = package.PK.ToGuid(), ScannedProductInfos = productInfos1, ToteID = package.KP_PackageID };

			// Short Tote first time
			var webService1 = GetNewWebService();
			SetupSecurityHeader(webService1, data.Whs1, staff);
			var response1 = webService1.ShortTote(packageInfo1);

			AssertEquals("There should be no error in the response.", true, string.IsNullOrEmpty(response1.ErrorMessage));
			AssertEquals("Quantity is 70 in package.", 70m, package.PackedItemDivots.Sum(divot => divot.KI_PackedQty));
			AssertEquals("There should be 2 picklines.", 2, orderLine.PickLines.Count);
			AssertEquals("Pickline in package is correct", true, orderLine.PickLines.Any(pl => pl.WZ_Units == 70m && pl.WZ_WE_InventoryLine == receive.Lines[0].PK));
			AssertEquals("Pickline not in package is correct", true, orderLine.PickLines.Any(pl => pl.WZ_Units == 30m && pl.WZ_WE_InventoryLine == receive.Lines[0].PK));

			var productInfos2 = new[]
			{
				new WhsPackageProductInfo { Quantity = 20m, ExpectedQty = 70m, ProductPK = data.Part1.PK.ToGuid() }
			};
			var packageInfo2 = new PackageForPackingInfo { DocketID = order.WD_DocketID, PK = package.PK.ToGuid(), ScannedProductInfos = productInfos2, ToteID = package.KP_PackageID };

			var webService2 = GetNewWebService();
			SetupSecurityHeader(webService2, data.Whs1, staff);
			var response2 = webService2.ShortTote(packageInfo2);

			AssertEquals("There should be no error in the response", true, string.IsNullOrEmpty(response2.ErrorMessage));
			AssertEquals("There should be 1 package", 1, order.PackageJob.Packages.Count);
			AssertEquals("Quantity in package is correct", 20m, package.PackedItemDivots.Sum(divot => divot.KI_PackedQty));
			AssertEquals("There should be 3 picklines.", 3, orderLine.PickLines.Count);
			AssertEquals("Pickline in package is correct", true, orderLine.PickLines.Any(pl => pl.WZ_Units == 20m && pl.WZ_WE_InventoryLine == receive.Lines[0].PK));
			AssertEquals("Pickline1 not in package is correct", true, orderLine.PickLines.Any(pl => pl.WZ_Units == 30m && pl.WZ_WE_InventoryLine == receive.Lines[0].PK));
			AssertEquals("Pickline2 not in package is correct", true, orderLine.PickLines.Any(pl => pl.WZ_Units == 50m && pl.WZ_WE_InventoryLine == receive.Lines[0].PK));
		}

		#endregion

		#region TestShortTote_ShortMultipleTotePickLines

		public void TestShortTote_ShortMultipleTotePickLines()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			var staff = Helper.CreateGlbStaff("OP1", "Test1");
			var receive1 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 50m);
			var receive2 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 40m);
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "Order1", data.Part1, 90m);
			var pick = Helper.CreatePickNew(order);
			var package = order.PackageJob.Packages.AddNew("PLT", "ABC");
			package.SetIsTote(true);
			package.Pack(order.Lines[0].ReleaseLines[0], 100m);

			var productInfos = new[] { new WhsPackageProductInfo { Quantity = 37m, ExpectedQty = 100m, ProductPK = data.Part1.PK.ToGuid() } };
			var packageInfo = new PackageForPackingInfo { DocketID = order.WD_DocketID, PK = package.PK.ToGuid(), ScannedProductInfos = productInfos, ToteID = package.KP_PackageID };
			Helper.Factory.Save();

			var webService1 = GetNewWebService();
			SetupSecurityHeader(webService1, data.Whs1, staff);
			var response = webService1.ShortTote(packageInfo);

			AssertEquals("There should be no error in the response.", true, string.IsNullOrEmpty(response.ErrorMessage));
			AssertEquals("There should be 1 package", 1, order.PackageJob.Packages.Count);
			AssertEquals("Quantity in package is correct", 37m, package.PackedItemDivots.Sum(divot => divot.KI_PackedQty));
			AssertEquals("There should be 3 picklines.", 3, order.Lines[0].PickLines.Count);
			AssertEquals("Pickline in package is correct", true, order.Lines[0].PickLines.Any(pl => pl.WZ_Units == 37m && pl.WZ_WE_InventoryLine == receive1.Lines[0].PK));
			AssertEquals("Pickline1 not in package is correct", true, order.Lines[0].PickLines.Any(pl => pl.WZ_Units == 13m && pl.WZ_WE_InventoryLine == receive1.Lines[0].PK));
			AssertEquals("Pickline2 not in package is correct", true, order.Lines[0].PickLines.Any(pl => pl.WZ_Units == 40m && pl.WZ_WE_InventoryLine == receive2.Lines[0].PK));
		}

		#endregion

		#region TestShortTote_ShortMultipleToteOrderLines

		public void TestShortTote_ShortMultipleToteOrderLines()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			var staff = Helper.CreateGlbStaff("OP1", "Test1");
			var receive1 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 60m);
			var receive2 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 40m);
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine1 = Helper.CreateWhsOrderLine(order, data.Part1, 50m);
			var orderLine2 = Helper.CreateWhsOrderLine(order, data.Part1, 50m);
			Helper.Factory.Save();

			Helper.CreatePickNew(order);

			var package = order.PackageJob.Packages.AddNew();
			package.KP_PackageID = "TOTE01";
			package.SetIsTote(true);
			package.Pack(orderLine1.ReleaseLines[0], 50m);
			package.Pack(orderLine2.ReleaseLines[0], 50m);
			Helper.Factory.Save();

			AssertEquals("Precondition: package is a tote", true, package.GetIsTote());
			AssertEquals("Precondition: there is 1 package", 1, order.PackageJob.Packages.Count);
			AssertEquals("Precondition: package divots is 3", 3, package.PackedItemDivots.Count);
			AssertEquals("Precondition: package quantity is 100", 100m, package.PackedItemDivots.Sum(divot => divot.KI_PackedQty));

			var pickLinesBeforeShortTote = order.Lines.SelectMany(l => l.PickLines);
			AssertEquals("Precondition: Pickline1 in package is correct", true, pickLinesBeforeShortTote.Any(pl => pl.WZ_Units == 50m && pl.WZ_WE_InventoryLine == receive1.Lines[0].PK));
			AssertEquals("Precondition: Pickline2 in package is correct", true, pickLinesBeforeShortTote.Any(pl => pl.WZ_Units == 10m && pl.WZ_WE_InventoryLine == receive1.Lines[0].PK));
			AssertEquals("Precondition: Pickline3 in package is correct", true, pickLinesBeforeShortTote.Any(pl => pl.WZ_Units == 40m && pl.WZ_WE_InventoryLine == receive2.Lines[0].PK));

			var productInfos = new[] { new WhsPackageProductInfo { Quantity = 75m, ExpectedQty = 100m, ProductPK = data.Part1.PK.ToGuid() } };
			var packageInfo = new PackageForPackingInfo { DocketID = order.WD_DocketID, PK = package.PK.ToGuid(), ScannedProductInfos = productInfos, ToteID = package.KP_PackageID };

			var webService = GetNewWebService();
			SetupSecurityHeader(webService, data.Whs1, staff);
			var response = webService.ShortTote(packageInfo);

			AssertEquals("There should be no error in the response.", true, string.IsNullOrEmpty(response.ErrorMessage));
			AssertEquals("There should be 1 package", 1, order.PackageJob.Packages.Count);
			AssertEquals("Quantity in package is correct", 75m, package.PackedItemDivots.Sum(divot => divot.KI_PackedQty));

			var currentPickLines = order.Lines.SelectMany(l => l.PickLines);
			AssertEquals("There should be 4 picklines.", 4, currentPickLines.Count());
			AssertEquals("Pickline1 in package is correct", true, currentPickLines.Any(pl => pl.WZ_Units == 50m && pl.WZ_WE_InventoryLine == receive1.Lines[0].PK));
			AssertEquals("Pickline2 in package is correct", true, currentPickLines.Any(pl => pl.WZ_Units == 15m && pl.WZ_WE_InventoryLine == receive2.Lines[0].PK));
			AssertEquals("Pickline3 in package is correct", true, currentPickLines.Any(pl => pl.WZ_Units == 10m && pl.WZ_WE_InventoryLine == receive1.Lines[0].PK));
			AssertEquals("Pickline4 NOT in package is correct", true, currentPickLines.Any(pl => pl.WZ_Units == 25m && pl.WZ_WE_InventoryLine == receive2.Lines[0].PK));
		}

		#endregion

		#region TestShortTote_UnpacksUnscannedProducts

		public void TestShortTote_UnpacksUnscannedProducts()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			var staff = Helper.CreateGlbStaff("OP1", "Test1");
			var receive1 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "Receive1", data.Part1, 100m);
			var receive2 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "Receive2", data.Part2, 100m);
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine1 = Helper.CreateWhsOrderLine(order, data.Part1, 6m);
			var orderLine2 = Helper.CreateWhsOrderLine(order, data.Part2, 8m);
			var pick = Helper.CreatePickNew(order);
			var package = order.PackageJob.Packages.AddNew("PLT", "ABC");
			package.SetIsTote(true);
			package.Pack(orderLine1.ReleaseLines[0], 6m);
			package.Pack(orderLine2.ReleaseLines[0], 8m);

			var productInfos = new[] { new WhsPackageProductInfo { Quantity = 4m, ExpectedQty = 6m, ProductPK = data.Part1.PK.ToGuid() } };
			var packageInfo = new PackageForPackingInfo { DocketID = order.WD_DocketID, PK = package.PK.ToGuid(), ScannedProductInfos = productInfos, ToteID = package.KP_PackageID };
			Helper.Factory.Save();

			var webService1 = GetNewWebService();
			SetupSecurityHeader(webService1, data.Whs1, staff);
			var response = webService1.ShortTote(packageInfo);

			AssertEquals("Quantity is 4 in package.", 4m, package.PackedItemDivots.Sum(divot => divot.KI_PackedQty));
			AssertEquals("The should be no Part2 products in package.", false, package.PackedItemDivots.Any(divot => divot.PackedItem.PK == orderLine2.PickLines[0].PK));
			AssertEquals("A split off Part1 PickLine with value of 4 should exist", true, order.Lines[0].PickLines.Any(pl => pl.WZ_Units == 4m && pl.WZ_WE_InventoryLine == receive1.Lines[0].PK));
			AssertEquals("The the original Part1 pickLine value should change to 2.", true, order.Lines[0].PickLines.Any(pl => pl.WZ_Units == 2m && pl.WZ_WE_InventoryLine == receive1.Lines[0].PK));
		}

		#endregion
	}
}
