using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.Packing.Business;
using Enterprise.Packing.Business.Testing;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.Warehouse.Web.WebService.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Warehouse.Web.WebService.Testing
{
	class ClosePackageForPackingTest : WhsSecureServiceTestCase
	{
		#region TestClosePackageForPacking_ToteIsNull

		public void TestClosePackageForPacking_ToteIsNull()
		{
			var webService = GetNewWebService();
			AssertBusinessValidationError(webService, "Please provide a valid Package Info.", webService.ClosePackageForPacking(null, Guid.Empty));
		}

		#endregion

		#region TestClosePackageForPacking_ToteNotExist

		public void TestClosePackageForPacking_ToteNotExist()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			var printer = Helper.CreatePrintQueue("PrintQueue", "TestPrinter");

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			Helper.Factory.Save();

			var pick = Helper.CreatePickNew(order);

			var package1 = order.PackageJob.Packages.AddNew();
			package1.KP_PackageID = "TOTE02";
			package1.SetIsTote(true);
			package1.Pack(orderLine.ReleaseLines[0], 5m);

			var package2 = order.PackageJob.Packages.AddNew();
			package2.KP_PackageID = "TOTE03";
			package2.SetIsTote(true);
			package2.Pack(orderLine.ReleaseLines[0], 5m);

			var package3 = order.PackageJob.Packages.AddNew();
			package3.KP_PackageID = "PKG1";
			package3.Pack(orderLine.ReleaseLines[0], 5m);

			Helper.Factory.Save();

			var packageInfo = new PackageForPackingInfo()
			{
				PK = Guid.NewGuid(),
				ToteID = "TOTE01",
				OrderReference = "O1",
				DocketID = order.WD_DocketID
			};

			var webService = GetNewWebService();
			webService.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			var response = webService.ClosePackageForPacking(packageInfo, printer.PK.ToGuid());
			AssertNull("PackageInfo should be null", response.PackageForPackingInfo);
			AssertEquals("Should Error as no Tote can be found.", ErrorTypes.BusinessValidationError, response.Error);
			AssertEquals("Tote # Or Package ID 'TOTE01' cannot be found.", response.ErrorMessage);
		}

		#endregion

		#region TestClosePackageForPacking_OrderIsNotExist

		public void TestClosePackageForPacking_OrderIsNotExist()
		{
			var packingHelper = new PackingTestHelper(Helper.Factory);
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			var printer = Helper.CreatePrintQueue("PrintQueue", "TestPrinter");

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			Helper.Factory.Save();

			var pick = Helper.CreatePickNew(order);

			Helper.Factory.Save();

			var packageInfo = new PackageForPackingInfo()
			{
				PK = Guid.NewGuid(),
				ToteID = "TOTE1",
				OrderReference = "O1",
				DocketID = "XXXXXX"
			};

			var webService = GetNewWebService();
			webService.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			var response = webService.ClosePackageForPacking(packageInfo, printer.PK.ToGuid());
			AssertNull("PackageInfo should be null", response.PackageForPackingInfo);
			AssertEquals("Should Error as no Order can be found.", ErrorTypes.BusinessValidationError, response.Error);
			AssertEquals("No Warehouse Order found with Docket ID: XXXXXX.", response.ErrorMessage);
		}

		#endregion

		#region TestClosePackageForPacking_ToteNotInOrder

		public void TestClosePackageForPacking_ToteNotInOrder()
		{
			var packingHelper = new PackingTestHelper(Helper.Factory);
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			var printer = Helper.CreatePrintQueue("PrintQueue", "TestPrinter");

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 20m);
			Helper.Factory.Save();

			var order1 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine1 = Helper.CreateWhsOrderLine(order1, data.Part1, 10m);

			var order2 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O2");
			var orderLine2 = Helper.CreateWhsOrderLine(order2, data.Part1, 10m);
			Helper.Factory.Save();

			var pick = Helper.CreatePickNew(order1, order2);

			var package1 = order1.PackageJob.Packages.AddNew();
			package1.KP_PackageID = "TOTE01";
			package1.SetIsTote(true);
			package1.Pack(orderLine1.ReleaseLines[0], 10m);

			var package2 = order2.PackageJob.Packages.AddNew();
			package2.KP_PackageID = "TOTE02";
			package2.SetIsTote(true);
			package2.Pack(orderLine2.ReleaseLines[0], 10m);
			Helper.Factory.Save();

			AssertEquals("Precondition: Tote01 not in this order", false, order1.PackageJob.Packages.Any(p => p.KP_PackageID == "TOTE02"));

			var packageInfo = new PackageForPackingInfo()
			{
				PK = package2.PK.ToGuid(),
				ToteID = "TOTE02",
				OrderReference = "O1",
				DocketID = order1.WD_DocketID
			};

			var webService = GetNewWebService();
			webService.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			var response = webService.ClosePackageForPacking(packageInfo, printer.PK.ToGuid());
			AssertNull("PackageInfo should be null", response.PackageForPackingInfo);
			AssertEquals("Should Error as Tote is not in Order.", ErrorTypes.BusinessValidationError, response.Error);
			AssertEquals("Package 'TOTE02' cannot be found in Order 'W00000002'.", response.ErrorMessage);
		}

		#endregion

		#region TestClosePackageForPacking_UnableGeneratePackageID

		public void TestClosePackageForPacking_UnableGeneratePackageID()
		{
			var packingHelper = new PackingTestHelper(Helper.Factory);
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			var printer = Helper.CreatePrintQueue("PrintQueue", "TestPrinter");

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			Helper.Factory.Save();

			var pick = Helper.CreatePickNew(order);
			Helper.Factory.Save();

			var package = order.PackageJob.Packages.AddNew();
			package.KP_PackageID = "TOTE01";
			package.SetIsTote(true);
			package.Pack(orderLine.ReleaseLines[0], 5m);
			Helper.Factory.Save();

			AssertEquals("Precondition: Package is Tote", true, package.GetIsTote());

			var packageInfo = new PackageForPackingInfo()
			{
				PK = package.PK.ToGuid(),
				WeightUQ = "KG",
				ToteID = "TOTE01",
				OrderReference = "O1",
				DocketID = order.WD_DocketID
			};

			var webService = GetNewWebService();
			webService.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;

			BusinessObjectFactory.SavingEventHandler action = null;
			action = delegate
			{
				UnitTestUserNotification.Instance.ClearMessages();
				webService.Factory.Saving -= action;
				var row = ((INeedRow)order).Row;
				throw new ZSaveException(new DummyDataException(row, TestConnection), order.Factory);
			};
			webService.Factory.Saving += action;

			var response = webService.ClosePackageForPacking(packageInfo, printer.PK.ToGuid());
			AssertNull("PackageInfo should be null", response.PackageForPackingInfo);
			AssertEquals("Should Error as Tote is not valid.", ErrorTypes.BusinessValidationError, response.Error);
			AssertEquals("Issue closing and saving Package:\r\nBlah", response.ErrorMessage);
			Assert("There should be no error reported.", string.IsNullOrEmpty(UnitTestUserNotification.Instance.LastMessage.Text));
		}

		public void TestClosePackageForPacking_UnableGeneratePackageID_CannotSaveException()
		{
			var packingHelper = new PackingTestHelper(Helper.Factory);
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			var printer = Helper.CreatePrintQueue("PrintQueue", "TestPrinter");

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			Helper.Factory.Save();

			var pick = Helper.CreatePickNew(order);
			Helper.Factory.Save();

			var package = order.PackageJob.Packages.AddNew();
			package.KP_PackageID = "TOTE01";
			package.SetIsTote(true);
			package.Pack(orderLine.ReleaseLines[0], 5m);
			Helper.Factory.Save();

			AssertEquals("Precondition: Package is Tote", true, package.GetIsTote());

			var packageInfo = new PackageForPackingInfo()
			{
				PK = package.PK.ToGuid(),
				WeightUQ = "KG",
				ToteID = "TOTE01",
				OrderReference = "O1",
				DocketID = order.WD_DocketID
			};

			var webService = GetNewWebService();
			webService.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;

			BusinessObjectFactory.SavingEventHandler action = null;
			action = delegate
			{
				UnitTestUserNotification.Instance.ClearMessages();
				webService.Factory.Saving -= action;
				throw new ZCannotSaveException("Test - Cannot Save", "Test Exception");
			};
			webService.Factory.Saving += action;

			var response = webService.ClosePackageForPacking(packageInfo, printer.PK.ToGuid());
			AssertNull("PackageInfo should be null", response.PackageForPackingInfo);
			AssertEquals("Should Error as Tote is not valid.", ErrorTypes.BusinessValidationError, response.Error);
			AssertEquals("Test - Cannot Save", response.ErrorMessage);
			Assert("There should be no error reported.", string.IsNullOrEmpty(UnitTestUserNotification.Instance.LastMessage.Text));
		}

		#endregion

		#region TestClosePackageForPacking_ClearHoldStatus

		public void TestClosePackageForPacking_ClearHoldStatus()
		{
			var packingHelper = new PackingTestHelper(Helper.Factory);
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			var printer = Helper.CreatePrintQueue("PrintQueue", "TestPrinter");

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			Helper.Factory.Save();

			Helper.CreatePickNew(order);
			Helper.Factory.Save();

			var package = order.PackageJob.Packages.AddNew();
			package.KP_PackageID = "TOTE01";
			package.KP_Weight = 10m;
			package.KP_WeightUQ = Constants.Weight.Grams;
			package.KP_Length = 5m;
			package.KP_Width = 4m;
			package.KP_Height = 3m;
			package.KP_DimensionUQ = Constants.Length.Metres;
			package.SetIsTote(true);
			package.Pack(orderLine.ReleaseLines[0], 5m);
			package.KP_IsHeld = true;
			Helper.Factory.Save();

			AssertEquals("Precondition: Package is Tote", true, package.GetIsTote());
			AssertEquals("Precondition: Package is on hold", true, package.KP_IsHeld);

			var packageInfo = new PackageForPackingInfo()
			{
				PK = package.PK.ToGuid(),
				ToteID = "TOTE01",
				OrderReference = "O1",
				DocketID = order.WD_DocketID,
				WeightUQ = Constants.Weight.Kilograms,
				Weight = 8m,
				Length = 10m,
				Width = 5m,
				Height = 2m,
				DimensionUQ = Constants.Length.Centimetres
			};

			var webService = GetNewWebService();
			webService.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			var response = webService.ClosePackageForPacking(packageInfo, printer.PK.ToGuid());
			var newToteInfo = response.PackageForPackingInfo;
			AssertNotNull("PackageInfo should not be null", newToteInfo);
			AssertEquals("Should have no error as Tote is valid.", ErrorTypes.None, response.Error);
			AssertEquals("The new package id should not be empty", false, string.IsNullOrEmpty(newToteInfo.PackageID));

			var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var newPackage = newFactory.Load<PkgPackage>(package.PK);
			AssertPackage(package, "TOTE01", Constants.PkgUnit.Carton,
				8m, Constants.Weight.Kilograms,
				10m, 5m, 2m, Constants.Length.Centimetres,
				false, true, false);

			var newOrder = newFactory.Load<WhsOrder>(order.PK);
			AssertNotNull(newOrder);
			AssertEquals("Should find out log", 1, Helper.FindLogs(newOrder.Logs, ZArchitecture.Business.Events.WarehouseOrderPacking, "Transferred items from Tote TOTE01 into Package O1-001.").Length);
		}

		#endregion

		#region TestClosePackageForPacking_Error_HoldStatusNotCleared

		public void TestClosePackageForPacking_Error_HoldStatusNotCleared()
		{
			var packingHelper = new PackingTestHelper(Helper.Factory);
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			var printer = Helper.CreatePrintQueue("PrintQueue", "TestPrinter");

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			Helper.Factory.Save();

			var pick = Helper.CreatePickNew(order);

			var package1 = order.PackageJob.Packages.AddNew();
			package1.KP_PackageID = "TOTE01";
			package1.Pack(orderLine.ReleaseLines[0], 5m);
			package1.KP_IsHeld = true;
			Helper.Factory.Save();

			AssertEquals("Precondition: Not a valid Tote", false, package1.GetIsTote());
			AssertEquals("Precondition: Package is held.", true, package1.KP_IsHeld);

			var packageInfo = new PackageForPackingInfo()
			{
				PK = Guid.NewGuid(),
				ToteID = "TOTE01",
				OrderReference = "O1",
				DocketID = order.WD_DocketID
			};

			var webService = GetNewWebService();
			webService.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			var response = webService.ClosePackageForPacking(packageInfo, printer.PK.ToGuid());
			AssertNull("PackageInfo should be null", response.PackageForPackingInfo);
			AssertEquals("Should Error as Tote is not valid.", ErrorTypes.BusinessValidationError, response.Error);
			AssertEquals("Tote # Or Package ID 'TOTE01' cannot be found.", response.ErrorMessage);

			var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var newPackage = newFactory.Load<PkgPackage>(package1.PK);

			AssertEquals(true, newPackage.KP_IsHeld);
		}

		#endregion

		#region TestClosePackageForPacking

		public void TestClosePackageForPacking()
		{
			var packingHelper = new PackingTestHelper(Helper.Factory);
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			Helper.SetProductWeightAndVolume(data.Part1, 0m, "", 0m, "");
			var printer = Helper.CreatePrintQueue("PrintQueue", "TestPrinter");

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			order.WD_TotalCubicUnit = Constants.Volume.Litre;
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			Helper.Factory.Save();

			var pick = Helper.CreatePickNew(order);
			Helper.Factory.Save();

			var package = order.PackageJob.Packages.AddNew("TOT");
			package.KP_PackageID = "TOTE01";
			package.KP_Weight = 10m;
			package.KP_WeightUQ = Constants.Weight.Grams;
			package.KP_VolumeUQ = Constants.Volume.Litre;
			package.KP_Length = 5m;
			package.KP_Width = 4m;
			package.KP_Height = 3m;
			package.KP_DimensionUQ = Constants.Length.Metres;
			package.SetIsTote(true);
			package.Pack(orderLine.ReleaseLines[0], 5m);
			Helper.Factory.Save();
			AssertEquals("Precondition: Package is Tote", true, package.GetIsTote());
			AssertEquals("Precondition", true, order.UsePackingWeightAndVolume);
			AssertEquals("Precondition", 1, order.WD_PackagesSent);
			AssertEquals("Precondition", 0.01m, order.WD_WeightSent);
			AssertEquals("Precondition", 60000m, order.WD_CubicSent);

			var packageInfo = new PackageForPackingInfo()
			{
				PK = package.PK.ToGuid(),
				ToteID = "TOTE01",
				OrderReference = "O1",
				DocketID = order.WD_DocketID,
				IsUsingCarrierLabelIntegration = true,
				WeightUQ = Constants.Weight.Kilograms,
				Weight = 8m,
				Length = 10m,
				Width = 5m,
				Height = 2m,
				DimensionUQ = Constants.Length.Centimetres,
				OrderIsUsingDirectedPackingConsolidation = true,
				PackageRequiresPutaway = true,
				AssignedDockDoorLocationString = "LOCATION",
				AssignedDockDoorLocationStringUserFriendly = "LOCATIONUSERFRIENDLY",
				AssignedPutawayLocationClass = "CON",
				AssignedPutawayLocationString = "PUTAWAYLOCATION",
				AssignedPutawayLocationStringUserFriendly = "PUTAWAYLOCATIONUSERFRIENDLY",
			};

			var webService = GetNewWebService();
			webService.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			var webServiceFactory = webService.Factory;

			var saveCount = 0;
			webService.Factory.Saving += _ => saveCount++;

			var response = webService.ClosePackageForPacking(packageInfo, printer.PK.ToGuid());
			var newToteInfo = response.PackageForPackingInfo;
			AssertNotNull("PackageInfo should not be null", newToteInfo);
			AssertEquals("Should Error as Tote is not valid.", ErrorTypes.None, response.Error);
			AssertEquals("The new package id should not be empty", false, string.IsNullOrEmpty(newToteInfo.PackageID));
			AssertEquals("Should have only saved once.", 1, saveCount);

			AssertEquals("IsUsingDirectedPackingConsolidation", true, newToteInfo.OrderIsUsingDirectedPackingConsolidation);
			AssertEquals("PackageRequiresPutaway", true, newToteInfo.PackageRequiresPutaway);
			AssertEquals("AssignedDockDoorLocationString", "LOCATION", newToteInfo.AssignedDockDoorLocationString);
			AssertEquals("AssignedDockDoorLocationStringUserFriendly", "LOCATIONUSERFRIENDLY", newToteInfo.AssignedDockDoorLocationStringUserFriendly);
			AssertEquals("AssignedPutawayLocationClass", "CON", newToteInfo.AssignedPutawayLocationClass);
			AssertEquals("AssignedPutawayLocationString", "PUTAWAYLOCATION", newToteInfo.AssignedPutawayLocationString);
			AssertEquals("AssignedPutawayLocationStringUserFriendly", "PUTAWAYLOCATIONUSERFRIENDLY", newToteInfo.AssignedPutawayLocationStringUserFriendly);

			var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var newPackage = newFactory.Load<PkgPackage>(package.PK);
			AssertPackage(package, "TOTE01", Constants.PkgUnit.Carton,
				8m, Constants.Weight.Kilograms,
				10m, 5m, 2m, Constants.Length.Centimetres,
				false, true, false);
			AssertEquals("Should copy all fields.", true, newToteInfo.IsUsingCarrierLabelIntegration);

			var newOrder = newFactory.Load<WhsOrder>(order.PK);
			AssertNotNull(newOrder);
			AssertEquals("Should find out log", 1, Helper.FindLogs(newOrder.Logs, ZArchitecture.Business.Events.WarehouseOrderPacking, "Transferred items from Tote TOTE01 into Package O1-001.").Length);
			AssertEquals("Use Packing Weight and Volume should not have changed.", true, newOrder.UsePackingWeightAndVolume);
			AssertEquals("Packages Sent should not have changed.", 1, newOrder.WD_PackagesSent);
			AssertEquals("Weight Sent should be updated.", 8m, newOrder.WD_WeightSent);
			AssertEquals("Volume Sent should be updated.", 0.1m, newOrder.WD_CubicSent);
		}

		#endregion

		#region TestClosePackageForPacking_IsNotTote

		public void TestClosePackageForPacking_IsNotTote()
		{
			var packingHelper = new PackingTestHelper(Helper.Factory);
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			var printer = Helper.CreatePrintQueue("PrintQueue", "TestPrinter");

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			Helper.Factory.Save();

			var pick = Helper.CreatePickNew(order);
			Helper.Factory.Save();

			var package = order.PackageJob.Packages.AddNew();
			package.KP_PackageID = "PKG01";
			package.KP_F3_NKPackType = Constants.PkgUnit.Box;
			package.KP_Weight = 10m;
			package.KP_WeightUQ = Constants.Weight.Grams;
			package.KP_Length = 5m;
			package.KP_Width = 4m;
			package.KP_Height = 3m;
			package.KP_DimensionUQ = Constants.Length.Metres;
			package.Pack(orderLine.ReleaseLines[0], 5m);
			Helper.Factory.Save();

			AssertEquals("Precondition: Package is not Tote", false, package.GetIsTote());

			var packageInfo = new PackageForPackingInfo()
			{
				PK = package.PK.ToGuid(),
				ToteID = "PKG01",
				OrderReference = "O1",
				DocketID = order.WD_DocketID,
				Weight = 8m,
				Length = 10m,
				Width = 5m,
				Height = 2m
			};

			var webService = GetNewWebService();
			webService.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			var response = webService.ClosePackageForPacking(packageInfo, printer.PK.ToGuid());
			var newToteInfo = response.PackageForPackingInfo;
			AssertNotNull("PackageInfo should not be null", newToteInfo);
			AssertEquals("Should Error as Tote is not valid.", ErrorTypes.None, response.Error);
			AssertEquals("The new package id should not be empty", false, string.IsNullOrEmpty(newToteInfo.PackageID));

			var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var newPackage = newFactory.Load<PkgPackage>(package.PK);
			AssertPackage(package, "PKG01", Constants.PkgUnit.Box,
				8m, Constants.Weight.Grams,
				10m, 5m, 2m, Constants.Length.Metres,
				false, true, false, isPackageIDChanged: false);

			var newOrder = newFactory.Load<WhsOrder>(order.PK);
			AssertNotNull(newOrder);
			AssertEquals("Should not find out log", 0, Helper.FindLogs(newOrder.Logs, ZArchitecture.Business.Events.WarehouseOrderPacking).Length);
		}

		#endregion

		#region TestClosePackageForPacking_WithScannedCartonSize

		public void TestClosePackageForPacking_WithScannedCartonSize_PackToTote()
		{
			var (order, newPackage) = TestClosePackageForPacking_WithScannedCartonSizeCore(true, Constants.PkgUnit.Case);

			AssertPackage(newPackage, "TOTE01", Constants.PkgUnit.Case,
				8m, Constants.Weight.Kilograms,
				10m, 5m, 2m, Constants.Length.Centimetres,
				false, true, false, isPackageIDChanged: true);
		}

		public void TestClosePackageForPacking_WithScannedCartonSize_ShouldHaveCorrectPackType_WhenPackToCarton()
		{
			var (order, newPackage) = TestClosePackageForPacking_WithScannedCartonSizeCore(false, Constants.PkgUnit.Case);

			AssertPackage(newPackage, "TOTE01", Constants.PkgUnit.Case,
				8m, Constants.Weight.Kilograms,
				10m, 5m, 2m, Constants.Length.Centimetres,
				false, true, false, isPackageIDChanged: false);
		}

		public void TestClosePackageForPacking_WithScannedCartonSize_ShouldUpdatePackagesSentInfo_WhenPackToCarton()
		{
			var (order, newPackage) = TestClosePackageForPacking_WithScannedCartonSizeCore(false, Constants.PkgUnit.Case);

			AssertEquals("PackageSentInfo should be updated when processing 'Pack To Carton'", 1, order.WD_PackagesSent);
		}

		public void TestClosePackageForPacking_WithScannedCartonSize_ShouldUpdatePackagesSentInfo_WhenPackToCartonAndPackTypeIsNotChanged()
		{
			var (order, newPackage) = TestClosePackageForPacking_WithScannedCartonSizeCore(false, Constants.PkgUnit.Tote);

			AssertEquals("PackageSentInfo should be updated when processing 'Pack To Carton'", 1, order.WD_PackagesSent);
		}

		(WhsOrder, PkgPackage) TestClosePackageForPacking_WithScannedCartonSizeCore(bool isPackToTote, string packTypeLinkedToCartonSize)
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			Helper.SetProductWeightAndVolume(data.Part1, 0m, "", 0m, "");
			var printer = Helper.CreatePrintQueue("PrintQueue", "TestPrinter");

			var cartonSize = Helper.CreateWhsCartonSize("ABC", 1m, 1m, 1m, 0.1m, 2m, 1, 85, "M", "KG");
			cartonSize.WCS_F3_NKPackType = packTypeLinkedToCartonSize;

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			order.WD_TotalCubicUnit = Constants.Volume.Litre;
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			Helper.Factory.Save();

			var pick = Helper.CreatePickNew(order);
			Helper.Factory.Save();

			var package = order.PackageJob.Packages.AddNew("TOT");
			package.KP_PackageID = "TOTE01";
			package.KP_Weight = 10m;
			package.KP_WeightUQ = Constants.Weight.Grams;
			package.KP_VolumeUQ = Constants.Volume.Litre;
			package.KP_Length = 5m;
			package.KP_Width = 4m;
			package.KP_Height = 3m;
			package.KP_DimensionUQ = Constants.Length.Metres;
			package.SetIsTote(isPackToTote);
			package.Pack(orderLine.ReleaseLines[0], 5m);

			order.WD_PackagesSent = 0;
			Helper.Factory.Save();
			AssertEquals("Precondition: Package is Tote", isPackToTote, package.GetIsTote());

			var packageInfo = new PackageForPackingInfo()
			{
				PK = package.PK.ToGuid(),
				ToteID = "TOTE01",
				OrderReference = "O1",
				DocketID = order.WD_DocketID,
				IsUsingCarrierLabelIntegration = true,
				WeightUQ = Constants.Weight.Kilograms,
				Weight = 8m,
				Length = 10m,
				Width = 5m,
				Height = 2m,
				DimensionUQ = Constants.Length.Centimetres,
				IsUsingCartonSizes = true,
				CartonSize = "ABC",
				IsDirectedPacking = true,
				PackType = Constants.PkgUnit.Carton,
			};

			var webService = GetNewWebService();
			webService.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;

			var response = webService.ClosePackageForPacking(packageInfo, printer.PK.ToGuid());
			var newToteInfo = response.PackageForPackingInfo;
			AssertNotNull("PackageInfo should not be null", newToteInfo);
			AssertEquals("The new package id should not be empty", false, string.IsNullOrEmpty(newToteInfo.PackageID));

			var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var newPackage = newFactory.Load<PkgPackage>(package.PK);
			return (order, newPackage);
		}

		public void TestClosePackageForPacking_WithScannedCartonSize_InexistentCartonSize()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			Helper.SetProductWeightAndVolume(data.Part1, 0m, "", 0m, "");
			var printer = Helper.CreatePrintQueue("PrintQueue", "TestPrinter");

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			order.WD_TotalCubicUnit = Constants.Volume.Litre;
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			Helper.Factory.Save();

			var pick = Helper.CreatePickNew(order);
			Helper.Factory.Save();

			var package = order.PackageJob.Packages.AddNew("TOT");
			package.KP_PackageID = "TOTE01";
			package.KP_Weight = 10m;
			package.KP_WeightUQ = Constants.Weight.Grams;
			package.KP_VolumeUQ = Constants.Volume.Litre;
			package.KP_Length = 5m;
			package.KP_Width = 4m;
			package.KP_Height = 3m;
			package.KP_DimensionUQ = Constants.Length.Metres;
			package.SetIsTote(true);
			package.Pack(orderLine.ReleaseLines[0], 5m);
			Helper.Factory.Save();
			AssertEquals("Precondition: Package is Tote", true, package.GetIsTote());

			var packageInfo = new PackageForPackingInfo()
			{
				PK = package.PK.ToGuid(),
				ToteID = "TOTE01",
				OrderReference = "O1",
				DocketID = order.WD_DocketID,
				IsUsingCarrierLabelIntegration = true,
				WeightUQ = Constants.Weight.Kilograms,
				Weight = 8m,
				Length = 10m,
				Width = 5m,
				Height = 2m,
				DimensionUQ = Constants.Length.Centimetres,
				IsUsingCartonSizes = true,
				CartonSize = "DEF",
			};

			var webService = GetNewWebService();
			webService.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;

			var response = webService.ClosePackageForPacking(packageInfo, printer.PK.ToGuid());
			var newToteInfo = response.PackageForPackingInfo;
			AssertNotNull("PackageInfo should not be null", newToteInfo);
			AssertEquals("The new package id should not be empty", false, string.IsNullOrEmpty(newToteInfo.PackageID));

			var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var newPackage = newFactory.Load<PkgPackage>(package.PK);
			AssertPackage(newPackage, "TOTE01", Constants.PkgUnit.Carton,
				8m, Constants.Weight.Kilograms,
				10m, 5m, 2m, Constants.Length.Centimetres,
				false, true, false); // default to CTN
		}

		public void TestClosePackageForPacking_WithScannedCartonSize_IsNotUsingCartonSize()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			Helper.SetProductWeightAndVolume(data.Part1, 0m, "", 0m, "");
			var printer = Helper.CreatePrintQueue("PrintQueue", "TestPrinter");

			var cartonSize = Helper.CreateWhsCartonSize("ABC", 1m, 1m, 1m, 0.1m, 2m, 1, 85, "M", "KG");
			cartonSize.WCS_F3_NKPackType = Constants.PkgUnit.Case;

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			order.WD_TotalCubicUnit = Constants.Volume.Litre;
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			Helper.Factory.Save();

			var pick = Helper.CreatePickNew(order);
			Helper.Factory.Save();

			var package = order.PackageJob.Packages.AddNew("TOT");
			package.KP_PackageID = "TOTE01";
			package.KP_Weight = 10m;
			package.KP_WeightUQ = Constants.Weight.Grams;
			package.KP_VolumeUQ = Constants.Volume.Litre;
			package.KP_Length = 5m;
			package.KP_Width = 4m;
			package.KP_Height = 3m;
			package.KP_DimensionUQ = Constants.Length.Metres;
			package.SetIsTote(true);
			package.Pack(orderLine.ReleaseLines[0], 5m);
			Helper.Factory.Save();
			AssertEquals("Precondition: Package is Tote", true, package.GetIsTote());

			var packageInfo = new PackageForPackingInfo()
			{
				PK = package.PK.ToGuid(),
				ToteID = "TOTE01",
				OrderReference = "O1",
				DocketID = order.WD_DocketID,
				IsUsingCarrierLabelIntegration = true,
				WeightUQ = Constants.Weight.Kilograms,
				Weight = 8m,
				Length = 10m,
				Width = 5m,
				Height = 2m,
				DimensionUQ = Constants.Length.Centimetres,
				IsUsingCartonSizes = false,
			};

			var webService = GetNewWebService();
			webService.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;

			var response = webService.ClosePackageForPacking(packageInfo, printer.PK.ToGuid());
			var newToteInfo = response.PackageForPackingInfo;
			AssertNotNull("PackageInfo should not be null", newToteInfo);
			AssertEquals("The new package id should not be empty", false, string.IsNullOrEmpty(newToteInfo.PackageID));

			var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var newPackage = newFactory.Load<PkgPackage>(package.PK);
			AssertPackage(newPackage, "TOTE01", Constants.PkgUnit.Carton,
				8m, Constants.Weight.Kilograms,
				10m, 5m, 2m, Constants.Length.Centimetres,
				false, true, false);
		}

		#endregion

		#region TestClosePackageForPacking_IsNotTote_MismatchingUQs

		public void TestClosePackageForPacking_IsNotTote_MismatchingUQs()
		{
			var packingHelper = new PackingTestHelper(Helper.Factory);
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			var printer = Helper.CreatePrintQueue("PrintQueue", "TestPrinter");

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			Helper.Factory.Save();

			var pick = Helper.CreatePickNew(order);
			Helper.Factory.Save();

			var package = order.PackageJob.Packages.AddNew();
			package.KP_PackageID = "PKG";
			package.KP_F3_NKPackType = Constants.PkgUnit.Box;
			package.KP_Weight = 10m;
			package.KP_WeightUQ = Constants.Weight.Grams;
			package.KP_Length = 5m;
			package.KP_Width = 4m;
			package.KP_Height = 3m;
			package.KP_DimensionUQ = Constants.Length.Metres;
			package.Pack(orderLine.ReleaseLines[0], 5m);
			Helper.Factory.Save();

			AssertEquals("Precondition: Package is *not* a Tote", false, package.GetIsTote());

			var packageInfo = new PackageForPackingInfo()
			{
				PK = package.PK.ToGuid(),
				ToteID = "PKG",
				OrderReference = "O1",
				DocketID = order.WD_DocketID,
				IsUsingCarrierLabelIntegration = true,
				WeightUQ = Constants.Weight.MetricCarat,
				Weight = 8m,
				Length = 10m,
				Width = 5m,
				Height = 2m,
				DimensionUQ = Constants.Length.Yards
			};

			var webService = GetNewWebService();
			webService.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			var response = webService.ClosePackageForPacking(packageInfo, printer.PK.ToGuid());
			var newToteInfo = response.PackageForPackingInfo;
			AssertNotNull("PackageInfo should not be null", newToteInfo);
			AssertEquals("Should have no Errors.", ErrorTypes.None, response.Error);
			AssertEquals("The new package id should not be empty", false, string.IsNullOrEmpty(newToteInfo.PackageID));

			var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var newPackage = newFactory.Load<PkgPackage>(package.PK);
			AssertPackage(package, "PKG", Constants.PkgUnit.Box,
				8m, Constants.Weight.MetricCarat,
				10m, 5m, 2m, Constants.Length.Yards,
				false, true, false, isPackageIDChanged: false);
			AssertEquals("Should copy all fields.", true, newToteInfo.IsUsingCarrierLabelIntegration);
		}

		#endregion

		#region TestClosePackageForPacking_CannotFindPrinter_ButPackageShouldStillBeClosed

		public void TestClosePackageForPacking_CannotFindPrinter_ButPackageShouldStillBeClosed()
		{
			var packingHelper = new PackingTestHelper(Helper.Factory);
			var data = new TestDataSimpleEnvironment(Helper.Factory);

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var printer = Helper.CreatePrintQueue("PrintQueue", "TestPrinter");
			Helper.Factory.Save();

			var pick = Helper.CreatePickNew(order);
			Helper.Factory.Save();

			var package = order.PackageJob.Packages.AddNew();
			package.KP_PackageID = "TOTE01";
			package.KP_Weight = 10m;
			package.KP_WeightUQ = Constants.Weight.Grams;
			package.KP_Length = 5m;
			package.KP_Width = 4m;
			package.KP_Height = 3m;
			package.KP_DimensionUQ = Constants.Length.Metres;
			package.SetIsTote(true);
			package.Pack(orderLine.ReleaseLines[0], 5m);
			Helper.Factory.Save();

			AssertEquals("Precondition: Package is not Closed", false, package.KP_ClosedTimeUtc.IsValid);
			AssertEquals("Precondition: Package is Tote", true, package.GetIsTote());

			var packageInfo = new PackageForPackingInfo()
			{
				PK = package.PK.ToGuid(),
				ToteID = "TOTE01",
				OrderReference = "O1",
				DocketID = order.WD_DocketID,
				WeightUQ = Constants.Weight.Kilograms,
				Weight = 8m,
				Length = 10m,
				Width = 5m,
				Height = 2m,
				DimensionUQ = Constants.Length.Centimetres
			};

			var webService = GetNewWebService();
			webService.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			var pKNotExist = Guid.NewGuid();
			var response = webService.ClosePackageForPacking(packageInfo, pKNotExist);
			var newToteInfo = response.PackageForPackingInfo;
			AssertNotNull("PackageInfo should not be null", newToteInfo);
			AssertEquals("Should Error as Tote is not valid.", ErrorTypes.None, response.Error);
			AssertEquals("The new package id should not be empty", false, string.IsNullOrEmpty(newToteInfo.PackageID));

			var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var newPackage = newFactory.Load<PkgPackage>(package.PK);
			AssertPackage(package, "TOTE01", Constants.PkgUnit.Carton,
				8m, Constants.Weight.Kilograms,
				10m, 5m, 2m, Constants.Length.Centimetres,
				false, true, false);
			AssertEquals("Package is Closed, although Printer does not exist.", true, package.KP_ClosedTimeUtc.IsValid);

			var newOrder = newFactory.Load<WhsOrder>(order.PK);
			AssertNotNull(newOrder);
			AssertEquals("Should find out log", 1, Helper.FindLogs(newOrder.Logs, ZArchitecture.Business.Events.WarehouseOrderPacking, "Transferred items from Tote TOTE01 into Package O1-001.").Length);

			var pkcEvent = newPackage.Logs.Find(l => l.SL_SE_NKEvent == ZArchitecture.Business.Events.PackingCompleted.Code).First();
			AssertEquals("Event SL_Ref is correct.", "Packing Completed", pkcEvent.SL_Reference);
		}

		#endregion

		#region TestClosePackageForPacking_StorePrinterNameToPackingCompletedEvent

		public void TestClosePackageForPacking_StorePrinterNameToPackingCompletedEvent()
		{
			var packingHelper = new PackingTestHelper(Helper.Factory);
			var data = new TestDataSimpleEnvironment(Helper.Factory);

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var printer = Helper.CreatePrintQueue("PrintQueue", "TestPrinter");
			Helper.Factory.Save();

			var pick = Helper.CreatePickNew(order);
			Helper.Factory.Save();

			var package = order.PackageJob.Packages.AddNew();
			package.KP_PackageID = "TOTE01";
			package.KP_Weight = 10m;
			package.KP_WeightUQ = Constants.Weight.Grams;
			package.KP_Length = 5m;
			package.KP_Width = 4m;
			package.KP_Height = 3m;
			package.KP_DimensionUQ = Constants.Length.Metres;
			package.SetIsTote(true);
			package.Pack(orderLine.ReleaseLines[0], 5m);
			Helper.Factory.Save();

			AssertEquals("Precondition: Package is Tote", true, package.GetIsTote());

			var packageInfo = new PackageForPackingInfo()
			{
				PK = package.PK.ToGuid(),
				ToteID = "TOTE01",
				OrderReference = "O1",
				DocketID = order.WD_DocketID,
				WeightUQ = Constants.Weight.Kilograms,
				Weight = 8m,
				Length = 10m,
				Width = 5m,
				Height = 2m,
				DimensionUQ = Constants.Length.Centimetres
			};

			var webService = GetNewWebService();
			webService.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			var response = webService.ClosePackageForPacking(packageInfo, printer.PK.ToGuid());
			var newToteInfo = response.PackageForPackingInfo;
			AssertNotNull("PackageInfo should not be null", newToteInfo);
			AssertEquals("Should have no error.", ErrorTypes.None, response.Error);
			AssertEquals("The new package id should not be empty", false, string.IsNullOrEmpty(newToteInfo.PackageID));

			var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var newPackage = newFactory.Load<PkgPackage>(package.PK);
			AssertPackage(package, "TOTE01", Constants.PkgUnit.Carton,
				8m, Constants.Weight.Kilograms,
				10m, 5m, 2m, Constants.Length.Centimetres,
				false, true, false);

			var newOrder = newFactory.Load<WhsOrder>(order.PK);
			AssertNotNull(newOrder);
			AssertEquals("Should find out log", 1, Helper.FindLogs(newOrder.Logs, ZArchitecture.Business.Events.WarehouseOrderPacking, "Transferred items from Tote TOTE01 into Package O1-001.").Length);

			var pkcEvent = newPackage.Logs.Find(l => l.SL_SE_NKEvent == ZArchitecture.Business.Events.PackingCompleted.Code).First();
			AssertEquals("Event SL_Ref is correct.", "Packing Completed|EQN=TestPrinter", pkcEvent.SL_Reference);
		}

		#endregion

		#region TestClosePackageForPacking_SplitPackageForPacking

		public void TestClosePackageForPacking_SplitPackageForPacking()
		{
			var webService = GetNewWebService();
			var helper = new WhsTestHelperFunctions(webService.Factory);
			var data = new TestDataSimpleEnvironment(helper.Factory);
			webService.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			var printer = Helper.CreatePrintQueue("PrintQueue", "TestPrinter");

			helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			helper.Factory.Save();

			var order = helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine = helper.CreateWhsOrderLine(order, data.Part1, 10m);
			helper.Factory.Save();

			helper.CreatePickNew(order);

			var package = order.PackageJob.Packages.AddNew();
			package.KP_PackageID = "TOTE01";
			package.SetIsTote(true);
			package.Pack(orderLine.ReleaseLines[0], 10m);
			helper.Factory.Save();

			AssertEquals("Precondition: package is a tote.", true, package.GetIsTote());
			AssertEquals("Precondition: there is 1 package.", 1, order.PackageJob.Packages.Count);
			AssertEquals("Precondition: package divots is 1.", 1, package.PackedItemDivots.Count);
			AssertEquals("Precondition: package quantity is 10.", 10m, package.PackedItemDivots[0].KI_PackedQty);

			var productInfo = new WhsPackageProductInfo { ProductPK = data.Part1.PK.ToGuid(), ExpectedQty = 10m, Quantity = 6m };
			var packageInfo = new PackageForPackingInfo
			{
				DocketID = order.WD_DocketID,
				PK = package.PK.ToGuid(),
				WeightUQ = Constants.Weight.Kilograms,
				Weight = 8m,
				Length = 10m,
				Width = 5m,
				Height = 2m,
				DimensionUQ = Constants.Length.Centimetres,
				IsPackageSplitForPacking = true,
				ScannedProductInfos = new[] { productInfo }
			};

			var response = webService.ClosePackageForPacking(packageInfo, printer.PK.ToGuid());
			AssertEquals("There should be no error in the response.", true, string.IsNullOrEmpty(response.ErrorMessage));
			AssertEquals("There should be 2 packages.", 2, order.PackageJob.Packages.Count);
			AssertEquals("Quantity is 4 on original package.", 4m, package.PackedItemDivots[0].KI_PackedQty);
			AssertEquals("Package is not yet closed.", false, package.KP_ClosedTimeUtc.IsValid);

			var newPackage = helper.Factory.Load<PkgPackage>(response.PackageForPackingInfo.PK);
			AssertEquals("New package exists.", true, newPackage != null);
			AssertEquals("Quantity is 6 on the new package.", 6m, newPackage.PackedItemDivots[0].KI_PackedQty);
			AssertEquals("New package pack type is carton.", "CTN", newPackage.KP_F3_NKPackType);
			AssertEquals("New package is closed.", true, newPackage.KP_ClosedTimeUtc.IsValid);
		}

		public void TestClosePackageForPacking_SplitPackageForPacking_MultiplePicklines()
		{
			var webService = GetNewWebService();
			var helper = new WhsTestHelperFunctions(webService.Factory);
			var data = new TestDataSimpleEnvironment(helper.Factory);
			webService.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			var printer = Helper.CreatePrintQueue("PrintQueue", "TestPrinter");

			helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 6m);
			helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 4m);
			helper.Factory.Save();

			var order = helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine = helper.CreateWhsOrderLine(order, data.Part1, 10m);
			helper.Factory.Save();

			helper.CreatePickNew(order);

			var package = order.PackageJob.Packages.AddNew();
			package.KP_PackageID = "TOTE01";
			package.SetIsTote(true);
			package.Pack(orderLine.ReleaseLines[0], 10m);
			helper.Factory.Save();

			AssertEquals("Precondition: package is a tote.", true, package.GetIsTote());
			AssertEquals("Precondition: there is 1 package.", 1, order.PackageJob.Packages.Count);
			AssertEquals("Precondition: package divots is 2.", 2, package.PackedItemDivots.Count);
			AssertEquals("Precondition: package quantity is 10.", 10m, package.PackedItemDivots.Sum(divot => divot.KI_PackedQty));

			var productInfo = new WhsPackageProductInfo { ProductPK = data.Part1.PK.ToGuid(), ExpectedQty = 10m, Quantity = 4m };
			var packageInfo = new PackageForPackingInfo
			{
				DocketID = order.WD_DocketID,
				PK = package.PK.ToGuid(),
				WeightUQ = Constants.Weight.Kilograms,
				Weight = 8m,
				Length = 10m,
				Width = 5m,
				Height = 2m,
				DimensionUQ = Constants.Length.Centimetres,
				IsPackageSplitForPacking = true,
				ScannedProductInfos = new[] { productInfo }
			};

			var response = webService.ClosePackageForPacking(packageInfo, printer.PK.ToGuid());
			AssertEquals("There should be no error in the response.", true, string.IsNullOrEmpty(response.ErrorMessage));
			AssertEquals("There should be 2 packages.", 2, order.PackageJob.Packages.Count);
			AssertEquals("There are 2 package divots.", 2, package.PackedItemDivots.Count);
			AssertEquals("Quantity is 6 on original package.", 6m, package.PackedItemDivots.Sum(divot => divot.KI_PackedQty));
			AssertEquals("Quantity on packed item divots is correct.", true, package.PackedItemDivots.Any(divot => divot.PackedItem.Quantity == 2m));
			AssertEquals("Quantity on packed item divots is correct.", true, package.PackedItemDivots.Any(divot => divot.PackedItem.Quantity == 4m));

			var newPackage = helper.Factory.Load<PkgPackage>(response.PackageForPackingInfo.PK);
			AssertEquals("New package exists.", true, newPackage != null);
			AssertEquals("Quantity is 4 on the new package.", 4m, newPackage.PackedItemDivots.Sum(divot => divot.KI_PackedQty));
		}

		public void TestClosePackageForPacking_SplitPackageForPacking_MultipleOrderlines()
		{
			var webService = GetNewWebService();
			var helper = new WhsTestHelperFunctions(webService.Factory);
			var data = new TestDataSimpleEnvironment(helper.Factory);
			webService.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			var printer = Helper.CreatePrintQueue("PrintQueue", "TestPrinter");

			helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 6m);
			helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 4m);
			helper.Factory.Save();

			var order = helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine1 = helper.CreateWhsOrderLine(order, data.Part1, 5m);
			var orderLine2 = helper.CreateWhsOrderLine(order, data.Part1, 5m);
			helper.Factory.Save();

			helper.CreatePickNew(order);

			var package = order.PackageJob.Packages.AddNew();
			package.KP_PackageID = "TOTE01";
			package.SetIsTote(true);
			package.Pack(orderLine1.ReleaseLines[0], 5m);
			package.Pack(orderLine2.ReleaseLines[0], 5m);
			helper.Factory.Save();

			AssertEquals("Precondition: package is a tote.", true, package.GetIsTote());
			AssertEquals("Precondition: there is 1 package.", 1, order.PackageJob.Packages.Count);
			AssertEquals("Precondition: package divots is 3.", 3, package.PackedItemDivots.Count);
			AssertEquals("Precondition: package quantity is 10.", 10m, package.PackedItemDivots.Sum(divot => divot.KI_PackedQty));

			var productInfo = new WhsPackageProductInfo { ProductPK = data.Part1.PK.ToGuid(), ExpectedQty = 10m, Quantity = 4m };
			var packageInfo = new PackageForPackingInfo
			{
				DocketID = order.WD_DocketID,
				PK = package.PK.ToGuid(),
				WeightUQ = Constants.Weight.Kilograms,
				Weight = 8m,
				Length = 10m,
				Width = 5m,
				Height = 2m,
				DimensionUQ = Constants.Length.Centimetres,
				IsPackageSplitForPacking = true,
				ScannedProductInfos = new[] { productInfo }
			};

			var response = webService.ClosePackageForPacking(packageInfo, printer.PK.ToGuid());
			AssertEquals("There should be no error in the response.", true, string.IsNullOrEmpty(response.ErrorMessage));
			AssertEquals("There should be 2 packages.", 2, order.PackageJob.Packages.Count);
			AssertEquals("There are 3 package divots.", 3, package.PackedItemDivots.Count);
			AssertEquals("Quantity is 6 on original package.", 6m, package.PackedItemDivots.Sum(divot => divot.KI_PackedQty));
			AssertEquals("Quantity on packed item divots is correct.", true, package.PackedItemDivots.Any(divot => divot.PackedItem.Quantity == 1m));
			AssertEquals("Quantity on packed item divots is correct.", true, package.PackedItemDivots.Any(divot => divot.PackedItem.Quantity == 4m));
			AssertEquals("There are 2 divots with quantity 1.", 2, package.PackedItemDivots.Count(divot => divot.PackedItem.Quantity == 1m));

			var newPackage = helper.Factory.Load<PkgPackage>(response.PackageForPackingInfo.PK);
			AssertEquals("New package exists.", true, newPackage != null);
			AssertEquals("Quantity is 4 on the new package.", 4m, newPackage.PackedItemDivots.Sum(divot => divot.KI_PackedQty));
		}

		public void TestClosePackageForPacking_SplitPackageForPacking_MultipleProducts()
		{
			var webService = GetNewWebService();
			var helper = new WhsTestHelperFunctions(webService.Factory);
			var data = new TestDataSimpleEnvironment(helper.Factory);
			webService.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			var printer = Helper.CreatePrintQueue("PrintQueue", "TestPrinter");

			helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part2, 20m);
			helper.Factory.Save();

			var order = helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine1 = helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var orderLine2 = helper.CreateWhsOrderLine(order, data.Part2, 20m);
			helper.Factory.Save();

			helper.CreatePickNew(order);

			var package = order.PackageJob.Packages.AddNew();
			package.KP_PackageID = "TOTE01";
			package.SetIsTote(true);
			package.Pack(orderLine1.ReleaseLines[0], 10m);
			package.Pack(orderLine2.ReleaseLines[0], 20m);
			helper.Factory.Save();

			AssertEquals("Precondition: package is a tote.", true, package.GetIsTote());
			AssertEquals("Precondition: there is 1 package.", 1, order.PackageJob.Packages.Count);
			AssertEquals("Precondition: total quantity in the package is the sum of the 2 orderlines.", 30m, package.PackedItemDivots.Sum(divot => divot.KI_PackedQty));
			AssertEquals("Precondition: there are 2 packed items in the package.", 2, package.PackedItemDivots.Count);

			var productInfo1 = new WhsPackageProductInfo { ProductPK = data.Part1.PK.ToGuid(), ExpectedQty = 10m, Quantity = 6m };
			var productInfo2 = new WhsPackageProductInfo { ProductPK = data.Part2.PK.ToGuid(), ExpectedQty = 20m, Quantity = 1m };
			var packageInfo = new PackageForPackingInfo
			{
				DocketID = order.WD_DocketID,
				PK = package.PK.ToGuid(),
				WeightUQ = Constants.Weight.Kilograms,
				Weight = 8m,
				Length = 10m,
				Width = 5m,
				Height = 2m,
				DimensionUQ = Constants.Length.Centimetres,
				IsPackageSplitForPacking = true,
				ScannedProductInfos = new[] { productInfo1, productInfo2 }
			};

			var response = webService.ClosePackageForPacking(packageInfo, printer.PK.ToGuid());
			AssertEquals("There should be no errors in the response.", true, string.IsNullOrEmpty(response.ErrorMessage));
			AssertEquals("There should be 2 packages.", 2, order.PackageJob.Packages.Count);
			AssertEquals("Quantity is 23 on original package.", 23m, package.PackedItemDivots.Sum(divot => divot.KI_PackedQty));
			AssertEquals("Quantity on packed item divots is correct.", true, package.PackedItemDivots.Any(divot => divot.PackedItem.Quantity == 4m));
			AssertEquals("Quantity on packed item divots is correct.", true, package.PackedItemDivots.Any(divot => divot.PackedItem.Quantity == 19m));

			var newPackage = helper.Factory.Load<PkgPackage>(response.PackageForPackingInfo.PK);
			AssertEquals("New package exists.", true, newPackage != null);
			AssertEquals("Quantity is 7 on the new package.", 7m, newPackage.PackedItemDivots.Sum(divot => divot.KI_PackedQty));
			AssertEquals("There are 2 packed item divots in then new package.", 2, newPackage.PackedItemDivots.Count);
			AssertEquals("Quantity on packed item divots is correct.", true, newPackage.PackedItemDivots.Any(divot => divot.PackedItem.Quantity == 6m));
			AssertEquals("Quantity on packed item divots is correct.", true, newPackage.PackedItemDivots.Any(divot => divot.PackedItem.Quantity == 1m));
		}

		public void TestClosePackageForPacking_SplitPackageForPacking_SplitMultipleTimes()
		{
			var webService = GetNewWebService();
			var helper = new WhsTestHelperFunctions(webService.Factory);
			var data = new TestDataSimpleEnvironment(helper.Factory);
			webService.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			var printer = Helper.CreatePrintQueue("PrintQueue", "TestPrinter");

			helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			helper.Factory.Save();

			var order = helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine = helper.CreateWhsOrderLine(order, data.Part1, 10m);
			helper.Factory.Save();

			helper.CreatePickNew(order);

			var package = order.PackageJob.Packages.AddNew();
			package.KP_PackageID = "TOTE01";
			package.SetIsTote(true);
			package.Pack(orderLine.ReleaseLines[0], 10m);
			helper.Factory.Save();

			AssertEquals("Precondition: package is a tote.", true, package.GetIsTote());
			AssertEquals("Precondition: there is 1 package.", 1, order.PackageJob.Packages.Count);
			AssertEquals("Precondition: package divots is 1.", 1, package.PackedItemDivots.Count);
			AssertEquals("Precondition: package quantity is 10.", 10m, package.PackedItemDivots[0].KI_PackedQty);

			var productInfo1 = new WhsPackageProductInfo { ProductPK = data.Part1.PK.ToGuid(), ExpectedQty = 10m, Quantity = 6m };
			var packageInfo = new PackageForPackingInfo
			{
				DocketID = order.WD_DocketID,
				PK = package.PK.ToGuid(),
				WeightUQ = Constants.Weight.Kilograms,
				Weight = 8m,
				Length = 10m,
				Width = 5m,
				Height = 2m,
				DimensionUQ = Constants.Length.Centimetres,
				IsPackageSplitForPacking = true,
				ScannedProductInfos = new[] { productInfo1 }
			};

			var response1 = webService.ClosePackageForPacking(packageInfo, printer.PK.ToGuid());
			AssertEquals("There should be no error in the response.", true, string.IsNullOrEmpty(response1.ErrorMessage));
			AssertEquals("There should be 2 packages.", 2, order.PackageJob.Packages.Count);
			AssertEquals("Quantity is 4 on original package.", 4m, package.PackedItemDivots[0].KI_PackedQty);

			var newPackage1 = helper.Factory.Load<PkgPackage>(response1.PackageForPackingInfo.PK);
			AssertEquals("New package exists.", true, newPackage1 != null);
			AssertEquals("Quantity is 6 on the new package.", 6m, newPackage1.PackedItemDivots[0].KI_PackedQty);
			AssertEquals("New package pack type is carton.", "CTN", newPackage1.KP_F3_NKPackType);

			var productInfo2 = new WhsPackageProductInfo { ProductPK = data.Part1.PK.ToGuid(), ExpectedQty = 4m, Quantity = 3m };
			var packageInfo2 = new PackageForPackingInfo
			{
				DocketID = order.WD_DocketID,
				PK = package.PK.ToGuid(),
				WeightUQ = Constants.Weight.Kilograms,
				Weight = 8m,
				Length = 10m,
				Width = 5m,
				Height = 2m,
				DimensionUQ = Constants.Length.Centimetres,
				IsPackageSplitForPacking = true,
				ScannedProductInfos = new[] { productInfo2 }
			};
			var response2 = webService.ClosePackageForPacking(packageInfo2, printer.PK.ToGuid());

			AssertEquals("There should be no error in the response.", true, string.IsNullOrEmpty(response2.ErrorMessage));
			AssertEquals("There should be 3 packages.", 3, order.PackageJob.Packages.Count);
			AssertEquals("Quantity is 1 on original package.", 1m, package.PackedItemDivots[0].KI_PackedQty);

			var newPackage2 = helper.Factory.Load<PkgPackage>(response2.PackageForPackingInfo.PK);
			AssertEquals("New package exists.", true, newPackage2 != null);
			AssertEquals("Quantity is 3 on the new package.", 3m, newPackage2.PackedItemDivots[0].KI_PackedQty);
			AssertEquals("New package pack type is carton.", "CTN", newPackage2.KP_F3_NKPackType);
			AssertEquals("Quantity remains 6 on the first split package.", 6m, newPackage1.PackedItemDivots[0].KI_PackedQty);
		}

		public void TestClosePackageForPacking_SplitPackageForPacking_ProductsWithReleaseCapturedAttributes()
		{
			var webService = GetNewWebService();
			var helper = new WhsTestHelperFunctions(webService.Factory);
			var data = new TestDataSimpleEnvironment(webService.Factory);
			webService.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			var printer = Helper.CreatePrintQueue("PrintQueue", "TestPrinter");

			helper.SetClientAttributeType(data.Org1, AttributeNumber.One, PartAttributeTypeList.Codes.Mandatory);
			helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true, setReleaseCaptured: true);
			helper.SetClientAttributeType(data.Org1, AttributeNumber.One, PartAttributeTypeList.Codes.Mandatory);
			helper.SetProductAttributeUse(data.Org1, data.Part2, AttributeNumber.One, true, setReleaseCaptured: true);

			helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 4m);
			helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 6m);
			helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R3", data.Part2, 8m);
			helper.Factory.Save();

			var order = helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine1 = helper.CreateWhsOrderLine(order, data.Part1, 10);
			var orderLine2 = helper.CreateWhsOrderLine(order, data.Part2, 8);
			helper.CreatePickNew(order);
			helper.Factory.Save();

			orderLine1.ReleaseLines[0].PartAttribute1 = "123";
			orderLine2.ReleaseLines[0].PartAttribute1 = "456";

			var package = order.PackageJob.Packages.AddNew();
			package.KP_PackageID = "TOTE01";
			package.SetIsTote(true);
			package.Pack(orderLine1.ReleaseLines[0], 10m);
			package.Pack(orderLine2.ReleaseLines[0], 8m);
			helper.Factory.Save();

			AssertEquals("Prerequisite: Pick line for product1 has captured attributes.", true, orderLine1.PickLines.Any(l => l.HasReleaseCapturedAttribs));
			AssertEquals("Prerequisite: Pick line for product2 has captured attributes.", true, orderLine2.PickLines.Any(l => l.HasReleaseCapturedAttribs));

			var productInfo1 = new WhsPackageProductInfo { ProductPK = data.Part1.PK.ToGuid(), ExpectedQty = 10m, Quantity = 6m };
			var productInfo2 = new WhsPackageProductInfo { ProductPK = data.Part2.PK.ToGuid(), ExpectedQty = 8m, Quantity = 1m };
			var packageInfo = new PackageForPackingInfo
			{
				DocketID = order.WD_DocketID,
				PK = package.PK.ToGuid(),
				WeightUQ = Constants.Weight.Kilograms,
				Weight = 8m,
				Length = 10m,
				Width = 5m,
				Height = 2m,
				DimensionUQ = Constants.Length.Centimetres,
				IsPackageSplitForPacking = true,
				ScannedProductInfos = new[] { productInfo1, productInfo2 }
			};

			var response = webService.ClosePackageForPacking(packageInfo, printer.PK.ToGuid());
			AssertEquals("There should be no errors in the response.", true, string.IsNullOrEmpty(response.ErrorMessage));
			AssertEquals("There should be 2 packages.", 2, order.PackageJob.Packages.Count);
			AssertEquals("Quantity is 11 on original package.", 11m, package.PackedItemDivots.Sum(divot => divot.KI_PackedQty));
			AssertEquals("Quantity on packed item divots is correct.", true, package.PackedItemDivots.Any(divot => divot.PackedItem.Quantity == 4m));
			AssertEquals("Quantity on packed item divots is correct.", true, package.PackedItemDivots.Any(divot => divot.PackedItem.Quantity == 7m));

			var newPackage = helper.Factory.Load<PkgPackage>(response.PackageForPackingInfo.PK);
			AssertEquals("New package exists.", true, newPackage != null);
			AssertEquals("Quantity is 7 on the new package.", 7m, newPackage.PackedItemDivots.Sum(divot => divot.KI_PackedQty));
			AssertEquals("There are 2 packed item divots in then new package.", 2, newPackage.PackedItemDivots.Count);
			AssertEquals("Quantity on packed item divots is correct.", true, newPackage.PackedItemDivots.Any(divot => divot.PackedItem.Quantity == 6m));
			AssertEquals("Quantity on packed item divots is correct.", true, newPackage.PackedItemDivots.Any(divot => divot.PackedItem.Quantity == 1m));
		}

		public void TestClosePackageForPacking_SplitPackageForPacking_ProductsWithReleaseCapturedAttributes_Halved()
		{
			var webService = GetNewWebService();
			var helper = new WhsTestHelperFunctions(webService.Factory);
			var data = new TestDataSimpleEnvironment(webService.Factory);
			webService.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			var printer = Helper.CreatePrintQueue("PrintQueue", "TestPrinter");

			helper.SetClientAttributeType(data.Org1, AttributeNumber.One, PartAttributeTypeList.Codes.Mandatory);
			helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true, setReleaseCaptured: true);

			helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 4m);
			helper.Factory.Save();

			var order = helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine1 = helper.CreateWhsOrderLine(order, data.Part1, 4);
			helper.CreatePickNew(order);
			helper.Factory.Save();

			orderLine1.ReleaseLines[0].PartAttribute1 = "123";

			var package = order.PackageJob.Packages.AddNew();
			package.KP_PackageID = "TOTE01";
			package.SetIsTote(true);
			package.Pack(orderLine1.ReleaseLines[0], 4m);
			helper.Factory.Save();

			AssertEquals("Prerequisite: Pick line for product1 has captured attributes.", true, orderLine1.PickLines.Any(l => l.HasReleaseCapturedAttribs));

			var productInfo = new WhsPackageProductInfo { ProductPK = data.Part1.PK.ToGuid(), ExpectedQty = 4m, Quantity = 2m };
			var packageInfo = new PackageForPackingInfo
			{
				DocketID = order.WD_DocketID,
				PK = package.PK.ToGuid(),
				WeightUQ = Constants.Weight.Kilograms,
				Weight = 8m,
				Length = 10m,
				Width = 5m,
				Height = 2m,
				DimensionUQ = Constants.Length.Centimetres,
				IsPackageSplitForPacking = true,
				ScannedProductInfos = new[] { productInfo }
			};

			var response = webService.ClosePackageForPacking(packageInfo, printer.PK.ToGuid());
			AssertEquals("There should be no errors in the response.", true, string.IsNullOrEmpty(response.ErrorMessage));
			AssertEquals("There should be 2 packages.", 2, order.PackageJob.Packages.Count);
			AssertEquals("Quantity is 2 on original package.", 2m, package.PackedItemDivots.Sum(divot => divot.KI_PackedQty));

			var newPackage = helper.Factory.Load<PkgPackage>(response.PackageForPackingInfo.PK);
			AssertEquals("New package exists.", true, newPackage != null);
			AssertEquals("Quantity is 2 on the new package.", 2m, newPackage.PackedItemDivots.Sum(divot => divot.KI_PackedQty));
		}

		public void TestClosePackageForPacking_SplitPackageForPacking_DirectedPacking()
		{
			var webService = GetNewWebService();
			var helper = new WhsTestHelperFunctions(webService.Factory);
			var data = new TestDataSimpleEnvironment(helper.Factory);
			webService.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			var printer = Helper.CreatePrintQueue("PrintQueue", "TestPrinter");

			helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			helper.Factory.Save();

			var order = helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine = helper.CreateWhsOrderLine(order, data.Part1, 10m);
			helper.Factory.Save();

			helper.CreatePickNew(order);

			var package = order.PackageJob.Packages.AddNew();
			package.KP_PackageID = "TOTE01";
			package.SetIsTote(true);
			package.Pack(orderLine.ReleaseLines[0], 10m);
			helper.Factory.Save();

			AssertEquals("Precondition: package is a tote.", true, package.GetIsTote());
			AssertEquals("Precondition: there is 1 package.", 1, order.PackageJob.Packages.Count);
			AssertEquals("Precondition: package divots is 1.", 1, package.PackedItemDivots.Count);
			AssertEquals("Precondition: package quantity is 10.", 10m, package.PackedItemDivots[0].KI_PackedQty);

			var productInfo = new WhsPackageProductInfo { ProductPK = data.Part1.PK.ToGuid(), ExpectedQty = 10m, Quantity = 6m };
			var packageInfo = new PackageForPackingInfo
			{
				DocketID = order.WD_DocketID,
				PK = package.PK.ToGuid(),
				WeightUQ = Constants.Weight.Kilograms,
				Weight = 8m,
				Length = 10m,
				Width = 5m,
				Height = 2m,
				IsTote = true,
				DimensionUQ = Constants.Length.Centimetres,
				IsPackageSplitForPacking = true,
				IsDirectedPacking = true,
				ScannedProductInfos = new[] { productInfo }
			};

			var response = webService.ClosePackageForPacking(packageInfo, printer.PK.ToGuid());
			AssertEquals("There should be no error in the response.", true, string.IsNullOrEmpty(response.ErrorMessage));
			AssertEquals("There should be 1 package.", 1, order.PackageJob.Packages.Count);
			AssertEquals("Packed Quantity is 10 on package.", 10m, package.PackedItemDivots[0].KI_PackedQty);
			AssertEquals("Package is closed.", true, package.KP_ClosedTimeUtc.IsValid);
			AssertEquals("No new package is created.", packageInfo.PK, response.PackageForPackingInfo.PK);
		}

		#endregion

		#region TestClosePackageForPacking_SetPackTypes

		public void TestClosePackageForPacking_SetPackTypes()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			var printer = Helper.CreatePrintQueue("PrintQueue", "TestPrinter");

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			Helper.Factory.Save();

			Helper.CreatePickNew(order);
			Helper.Factory.Save();

			var package = order.PackageJob.Packages.AddNew("BOX");
			package.KP_PackageID = "PACK123";
			package.KP_Weight = 10m;
			package.KP_WeightUQ = Constants.Weight.Grams;
			package.KP_Length = 5m;
			package.KP_Width = 4m;
			package.KP_Height = 3m;
			package.KP_DimensionUQ = Constants.Length.Metres;
			package.Pack(orderLine.ReleaseLines[0], 5m);

			var casePackType = package.Lookups.PackTypes.Single(p => p.F3_Code == "CAS");
			casePackType.F3_UnitOfWeight = Constants.Weight.Kilograms;
			casePackType.F3_UnitOfDimension = Constants.Length.Centimetres;
			Helper.Factory.Save();

			var packageInfo = new PackageForPackingInfo()
			{
				PK = package.PK.ToGuid(),
				ToteID = "PACK123",
				PackType = "CAS",
				OrderReference = "O1",
				DocketID = order.WD_DocketID,
				WeightUQ = Constants.Weight.Grams,
				Weight = 8m,
				Length = 10m,
				Width = 5m,
				Height = 2m,
				DimensionUQ = Constants.Length.Metres
			};

			var webService = GetNewWebService();
			webService.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			var response = webService.ClosePackageForPacking(packageInfo, printer.PK.ToGuid());
			var newPackageInfo = response.PackageForPackingInfo;
			AssertNotNull("PackageInfo should not be null", newPackageInfo);
			AssertEquals("Should have no error when changing Pack Type to valid one.", ErrorTypes.None, response.Error);
			AssertEquals("New Pack Type should be set on returned Information.", "CAS", newPackageInfo.PackType);

			var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var newPackage = newFactory.Load<PkgPackage>(package.PK);
			AssertPackage(package, "PACK123", Constants.PkgUnit.Case,
				8m, Constants.Weight.Grams,
				10m, 5m, 2m, Constants.Length.Metres,
				false, true, false, isPackageIDChanged: false);
		}

		#endregion

		#region TestClosePackageForPacking_SetInvalidPackType

		public void TestClosePackageForPacking_SetInvalidPackType()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			var printer = Helper.CreatePrintQueue("PrintQueue", "TestPrinter");

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			Helper.Factory.Save();

			Helper.CreatePickNew(order);
			Helper.Factory.Save();

			var package = order.PackageJob.Packages.AddNew("BOX");
			package.KP_PackageID = "PACK123";
			package.KP_Weight = 10m;
			package.KP_WeightUQ = Constants.Weight.Grams;
			package.KP_Length = 5m;
			package.KP_Width = 4m;
			package.KP_Height = 3m;
			package.KP_DimensionUQ = Constants.Length.Metres;
			package.Pack(orderLine.ReleaseLines[0], 5m);

			var casePackType = package.Lookups.PackTypes.Single(p => p.F3_Code == "CAS");
			casePackType.F3_UnitOfWeight = Constants.Weight.Kilograms;
			casePackType.F3_UnitOfDimension = Constants.Length.Centimetres;
			Helper.Factory.Save();

			var packageInfo = new PackageForPackingInfo()
			{
				PK = package.PK.ToGuid(),
				ToteID = "PACK123",
				PackType = "XXX",
				OrderReference = "O1",
				DocketID = order.WD_DocketID,
				WeightUQ = Constants.Weight.Grams,
				Weight = 8m,
				Length = 10m,
				Width = 5m,
				Height = 2m,
				DimensionUQ = Constants.Length.Metres
			};

			var webService = GetNewWebService();
			webService.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			var response = webService.ClosePackageForPacking(packageInfo, printer.PK.ToGuid());
			var newPackageInfo = response.PackageForPackingInfo;
			AssertNull("PackageInfo should be null", newPackageInfo);
			AssertEquals("Should have error when changing Pack Type to invalid one.", ErrorTypes.BusinessValidationError, response.Error);
			AssertEquals("Should have error when changing Pack Type to invalid one.", "Attempt to set invalid Pack Type 'XXX'.", response.ErrorMessage);
		}

		#endregion

		public void TestClosePackageForPacking_PackageClosed()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			Helper.SetProductWeightAndVolume(data.Part1, 0m, "", 0m, "");
			var printer = Helper.CreatePrintQueue("PrintQueue", "TestPrinter");

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			order.WD_TotalCubicUnit = Constants.Volume.Litre;
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			Helper.Factory.Save();

			Helper.CreatePickNew(order);
			Helper.Factory.Save();

			var package = order.PackageJob.Packages.AddNew("TOT");
			package.KP_PackageID = "TOTE01";
			package.KP_Weight = 10m;
			package.KP_WeightUQ = Constants.Weight.Grams;
			package.KP_VolumeUQ = Constants.Volume.Litre;
			package.KP_Length = 5m;
			package.KP_Width = 4m;
			package.KP_Height = 3m;
			package.KP_DimensionUQ = Constants.Length.Metres;
			package.SetIsTote(true);
			package.Pack(orderLine.ReleaseLines[0], 5m);
			Helper.Factory.Save();
			AssertEquals("Precondition: Package is Tote", true, package.GetIsTote());
			AssertEquals("Precondition", true, order.UsePackingWeightAndVolume);
			AssertEquals("Precondition", 1, order.WD_PackagesSent);
			AssertEquals("Precondition", 0.01m, order.WD_WeightSent);
			AssertEquals("Precondition", 60000m, order.WD_CubicSent);

			var packageInfo = new PackageForPackingInfo()
			{
				PK = package.PK.ToGuid(),
				ToteID = "TOTE01",
				OrderReference = "O1",
				DocketID = order.WD_DocketID,
				IsUsingCarrierLabelIntegration = true,
				WeightUQ = Constants.Weight.Kilograms,
				Weight = 8m,
				Length = 10m,
				Width = 5m,
				Height = 2m,
				DimensionUQ = Constants.Length.Centimetres
			};

			package.KP_ClosedTimeUtc = ZDateTime.UtcNow;
			Helper.Factory.Save();

			var webService = GetNewWebService();
			webService.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;

			var response = webService.ClosePackageForPacking(packageInfo, printer.PK.ToGuid());
			AssertEquals(ErrorTypes.BusinessValidationError, response.Error);
			AssertEquals("Package with ID '' is already closed.", response.ErrorMessage);
			AssertNull(response.PackageForPackingInfo);
		}

		public void TestClosePackageForPacking_PackageValidationErrors()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			Helper.SetProductWeightAndVolume(data.Part1, 0m, "", 0m, "");
			var printer = Helper.CreatePrintQueue("PrintQueue", "TestPrinter");

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			order.WD_TotalCubicUnit = Constants.Volume.Litre;
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			Helper.Factory.Save();

			Helper.CreatePickNew(order);
			Helper.Factory.Save();

			var package = order.PackageJob.Packages.AddNew("TOT");
			package.KP_PackageID = "TOTE01";
			package.KP_Weight = 10m;
			package.KP_WeightUQ = Constants.Weight.Grams;
			package.KP_VolumeUQ = Constants.Volume.Litre;
			package.KP_Length = 5m;
			package.KP_Width = 4m;
			package.KP_Height = 3m;
			package.KP_DimensionUQ = Constants.Length.Metres;
			package.SetIsTote(true);
			package.Pack(orderLine.ReleaseLines[0], 5m);
			Helper.Factory.Save();
			AssertEquals("Precondition: Package is Tote", true, package.GetIsTote());
			AssertEquals("Precondition", true, order.UsePackingWeightAndVolume);
			AssertEquals("Precondition", 1, order.WD_PackagesSent);
			AssertEquals("Precondition", 0.01m, order.WD_WeightSent);
			AssertEquals("Precondition", 60000m, order.WD_CubicSent);

			var packageInfo = new PackageForPackingInfo()
			{
				PK = package.PK.ToGuid(),
				ToteID = "TOTE01",
				OrderReference = "O1",
				DocketID = order.WD_DocketID,
				IsUsingCarrierLabelIntegration = true,
				WeightUQ = Constants.Weight.Kilograms,
				Weight = 1000000m,
				Length = 10m,
				Width = 5m,
				Height = 2m,
				DimensionUQ = Constants.Length.Centimetres
			};

			Helper.Factory.Save();

			var webService = GetNewWebService();
			webService.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;

			SinglePackageForPackingWebServiceResponse response = null;
			AssertNoExceptionThrown(() => response = webService.ClosePackageForPacking(packageInfo, printer.PK.ToGuid()));
			AssertEquals(ErrorTypes.BusinessValidationError, response.Error);
			var expectedErrorMessage = @"There are errors that need to be corrected before this package can be closed.
Error - KP_Weight: The number 1,000,000 is too large, the maximum value allowed for Gross Weight is 999,999.999.
Error - WD_WeightSent: The number 1,000,000.000 is too large, the maximum value allowed for Weight Sent is 999,999.999.";
			AssertEquals(expectedErrorMessage, response.ErrorMessage.Trim());
			AssertNull(response.PackageForPackingInfo);
		}

		public void TestClosePackageForPacking_MultiplePackageValidationErrors()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			Helper.SetProductWeightAndVolume(data.Part1, 0m, "", 0m, "");
			var printer = Helper.CreatePrintQueue("PrintQueue", "TestPrinter");

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			order.WD_TotalCubicUnit = Constants.Volume.Litre;
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			Helper.Factory.Save();

			Helper.CreatePickNew(order);
			Helper.Factory.Save();

			var package = order.PackageJob.Packages.AddNew("TOT");
			package.KP_PackageID = "TOTE01";
			package.KP_Weight = 10m;
			package.KP_WeightUQ = Constants.Weight.Grams;
			package.KP_VolumeUQ = Constants.Volume.Litre;
			package.KP_Length = 5m;
			package.KP_Width = 4m;
			package.KP_Height = 3m;
			package.KP_DimensionUQ = Constants.Length.Metres;
			package.SetIsTote(true);
			package.Pack(orderLine.ReleaseLines[0], 5m);
			Helper.Factory.Save();
			AssertEquals("Precondition: Package is Tote", true, package.GetIsTote());
			AssertEquals("Precondition", true, order.UsePackingWeightAndVolume);
			AssertEquals("Precondition", 1, order.WD_PackagesSent);
			AssertEquals("Precondition", 0.01m, order.WD_WeightSent);
			AssertEquals("Precondition", 60000m, order.WD_CubicSent);

			var packageInfo = new PackageForPackingInfo()
			{
				PK = package.PK.ToGuid(),
				ToteID = "TOTE01",
				OrderReference = "O1",
				DocketID = order.WD_DocketID,
				IsUsingCarrierLabelIntegration = true,
				WeightUQ = Constants.Weight.Kilograms,
				Weight = 1000000m,
				Length = 2000000m,
				Width = 5m,
				Height = 2m,
				DimensionUQ = Constants.Length.Centimetres
			};

			Helper.Factory.Save();

			var webService = GetNewWebService();
			webService.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;

			SinglePackageForPackingWebServiceResponse response = null;
			AssertNoExceptionThrown(() => response = webService.ClosePackageForPacking(packageInfo, printer.PK.ToGuid()));
			AssertEquals(ErrorTypes.BusinessValidationError, response.Error);
			var expectedErrorMessage = @"There are errors that need to be corrected before this package can be closed.
Error - KP_Length: The number 2,000,000 is too large, the maximum value allowed for Length is 999,999.999.
Error - KP_Weight: The number 1,000,000 is too large, the maximum value allowed for Gross Weight is 999,999.999.
Error - WD_WeightSent: The number 1,000,000.000 is too large, the maximum value allowed for Weight Sent is 999,999.999.";
			AssertEquals(expectedErrorMessage, response.ErrorMessage.Trim());
			AssertNull(response.PackageForPackingInfo);
		}

		public void TestClosePackageForPacking_OrderValidationErrors_WeightSent()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			Helper.SetProductWeightAndVolume(data.Part1, 0m, "", 0m, "");
			var printer = Helper.CreatePrintQueue("PrintQueue", "TestPrinter");

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			order.WD_TotalCubicUnit = Constants.Volume.Litre;
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			Helper.Factory.Save();

			Helper.CreatePickNew(order);
			Helper.Factory.Save();

			var package1 = order.PackageJob.Packages.AddNew("CTN");
			package1.KP_PackageID = "CTN01";
			package1.KP_Weight = 999998m;
			package1.KP_WeightUQ = Constants.Weight.Kilograms;
			package1.KP_VolumeUQ = Constants.Volume.Litre;
			package1.KP_Length = 5m;
			package1.KP_Width = 4m;
			package1.KP_Height = 3m;
			package1.KP_DimensionUQ = Constants.Length.Metres;
			package1.Pack(orderLine.ReleaseLines[0], 5m);

			var package2 = order.PackageJob.Packages.AddNew("TOT");
			package2.KP_PackageID = "TOTE01";
			package2.KP_Weight = 1m;
			package2.KP_WeightUQ = Constants.Weight.Kilograms;
			package2.KP_VolumeUQ = Constants.Volume.Litre;
			package2.KP_Length = 5m;
			package2.KP_Width = 4m;
			package2.KP_Height = 3m;
			package2.KP_DimensionUQ = Constants.Length.Metres;
			package2.SetIsTote(true);
			package2.Pack(orderLine.ReleaseLines[0], 5m);

			Helper.Factory.Save();

			AssertEquals("Precondition: Package is Tote", true, package2.GetIsTote());
			AssertEquals("Precondition", true, order.UsePackingWeightAndVolume);
			AssertEquals("Precondition", 2, order.WD_PackagesSent);
			AssertEquals("Precondition", 999999m, order.WD_WeightSent);
			AssertEquals("Precondition", 120000m, order.WD_CubicSent);

			var packageInfo = new PackageForPackingInfo()
			{
				PK = package2.PK.ToGuid(),
				ToteID = "TOTE01",
				OrderReference = "O1",
				DocketID = order.WD_DocketID,
				IsUsingCarrierLabelIntegration = true,
				WeightUQ = Constants.Weight.Kilograms,
				Weight = 999999m,
				Length = 10m,
				Width = 5m,
				Height = 2m,
				DimensionUQ = Constants.Length.Metres
			};

			var webService = GetNewWebService();
			webService.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;

			SinglePackageForPackingWebServiceResponse response = null;
			AssertNoExceptionThrown(() => response = webService.ClosePackageForPacking(packageInfo, printer.PK.ToGuid()));
			AssertEquals(ErrorTypes.BusinessValidationError, response.Error);
			var expectedErrorMessage = @"There are errors that need to be corrected before this package can be closed.
Error - WD_WeightSent: The number 1,999,997.000 is too large, the maximum value allowed for Weight Sent is 999,999.999.";
			AssertEquals(expectedErrorMessage, response.ErrorMessage.Trim());
			AssertNull(response.PackageForPackingInfo);
		}

		public void TestClosePackageForPacking_OrderValidationErrors_CubicSent()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			Helper.SetProductWeightAndVolume(data.Part1, 0m, "", 0m, "");
			var printer = Helper.CreatePrintQueue("PrintQueue", "TestPrinter");

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			order.WD_TotalCubicUnit = Constants.Volume.Litre;
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			Helper.Factory.Save();

			Helper.CreatePickNew(order);
			Helper.Factory.Save();

			var package = order.PackageJob.Packages.AddNew("TOT");
			package.KP_PackageID = "TOTE01";
			package.KP_Weight = 1m;
			package.KP_WeightUQ = Constants.Weight.Kilograms;
			package.KP_VolumeUQ = Constants.Volume.Litre;
			package.KP_Length = 5m;
			package.KP_Width = 4m;
			package.KP_Height = 3m;
			package.KP_DimensionUQ = Constants.Length.Metres;
			package.SetIsTote(true);
			package.Pack(orderLine.ReleaseLines[0], 5m);

			Helper.Factory.Save();

			AssertEquals("Precondition: Package is Tote", true, package.GetIsTote());
			AssertEquals("Precondition", true, order.UsePackingWeightAndVolume);
			AssertEquals("Precondition", 1m, order.WD_WeightSent);
			AssertEquals("Precondition", 60000m, order.WD_CubicSent);

			var packageInfo = new PackageForPackingInfo()
			{
				PK = package.PK.ToGuid(),
				ToteID = "TOTE01",
				OrderReference = "O1",
				DocketID = order.WD_DocketID,
				IsUsingCarrierLabelIntegration = true,
				WeightUQ = Constants.Weight.Kilograms,
				Weight = 1m,
				Length = 100000m,
				Width = 500000m,
				Height = 200000m,
				DimensionUQ = Constants.Length.Metres
			};

			var webService = GetNewWebService();
			webService.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;

			SinglePackageForPackingWebServiceResponse response = null;
			AssertNoExceptionThrown(() => response = webService.ClosePackageForPacking(packageInfo, printer.PK.ToGuid()));
			AssertEquals(ErrorTypes.BusinessValidationError, response.Error);
			var expectedErrorMessage = @"There are errors that need to be corrected before this package can be closed.
Error - KP_Volume: The number 10,000,000,000,000,000,000 is too large, the maximum value allowed for Volume is 999,999.999.
Error - WD_CubicSent: The number 10,000,000,000,000,000,000 is too large, the maximum value allowed for Volume Sent is 999,999.999.";
			AssertEquals(expectedErrorMessage, response.ErrorMessage.Trim());
			AssertNull(response.PackageForPackingInfo);
		}

		public void TestClosePackageForPacking_UnrelatedOrderValidationError()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			Helper.SetProductWeightAndVolume(data.Part1, 0m, "", 0m, "");
			var printer = Helper.CreatePrintQueue("PrintQueue", "TestPrinter");

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			order.WD_TotalCubicUnit = Constants.Volume.Litre;
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			Helper.Factory.Save();

			Helper.CreatePickNew(order);
			Helper.Factory.Save();

			var package = order.PackageJob.Packages.AddNew("CTN");
			package.KP_PackageID = "TOTE01";
			package.KP_Weight = 10m;
			package.KP_WeightUQ = Constants.Weight.Kilograms;
			package.KP_VolumeUQ = Constants.Volume.Litre;
			package.KP_Length = 5m;
			package.KP_Width = 4m;
			package.KP_Height = 3m;
			package.KP_DimensionUQ = Constants.Length.Metres;
			package.SetIsTote(true);
			package.Pack(orderLine.ReleaseLines[0], 5m);

			Helper.Factory.Save();

			AssertEquals("Precondition: Package is Tote", true, package.GetIsTote());
			AssertEquals("Precondition", true, order.UsePackingWeightAndVolume);

			var packageInfo = new PackageForPackingInfo()
			{
				PK = package.PK.ToGuid(),
				ToteID = "TOTE01",
				OrderReference = "O1",
				DocketID = order.WD_DocketID,
				IsUsingCarrierLabelIntegration = true,
				WeightUQ = Constants.Weight.Kilograms,
				Weight = 15m,
				Length = 10m,
				Width = 5m,
				Height = 2m,
				DimensionUQ = Constants.Length.Metres
			};

			var newFactory = new BusinessObjectFactory();
			var orderInNewFactory = newFactory.Load<WhsOrder>(order.PK);
			orderInNewFactory.WD_TransportMode = "XXX";
			newFactory.Save();

			var webService = GetNewWebService();
			webService.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;

			SinglePackageForPackingWebServiceResponse response = null;
			AssertNoExceptionThrown(() => response = webService.ClosePackageForPacking(packageInfo, printer.PK.ToGuid()));
			AssertEquals(ErrorTypes.None, response.Error);
			Assert(string.IsNullOrEmpty(response.ErrorMessage));
			AssertNotNull(response.PackageForPackingInfo);
		}

		public void TestClosePackageForPacking_PackageTareWeightUpdated()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			Helper.SetProductWeightAndVolume(data.Part1, 0m, "", 0m, "");
			var printer = Helper.CreatePrintQueue("PrintQueue", "TestPrinter");

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			order.WD_TotalCubicUnit = Constants.Volume.Litre;
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			Helper.Factory.Save();

			Helper.CreatePickNew(order);
			Helper.Factory.Save();

			var package = order.PackageJob.Packages.AddNew("TOT");
			package.KP_PackageID = "TOTE01";
			package.KP_Weight = 10m;
			package.KP_TareWeight = 9m;
			package.KP_WeightUQ = Constants.Weight.Grams;
			package.KP_VolumeUQ = Constants.Volume.Litre;
			package.KP_Length = 5m;
			package.KP_Width = 4m;
			package.KP_Height = 3m;
			package.KP_DimensionUQ = Constants.Length.Metres;
			package.SetIsTote(true);
			package.Pack(orderLine.ReleaseLines[0], 5m);
			Helper.Factory.Save();

			var packageInfo = new PackageForPackingInfo()
			{
				PK = package.PK.ToGuid(),
				ToteID = "TOTE01",
				OrderReference = "O1",
				DocketID = order.WD_DocketID,
				IsUsingCarrierLabelIntegration = true,
				WeightUQ = Constants.Weight.Kilograms,
				Weight = 12m,
				EmptyWeight = 2m,
				Length = 10m,
				Width = 5m,
				Height = 2m,
				DimensionUQ = Constants.Length.Centimetres
			};

			Helper.Factory.Save();

			var webService = GetNewWebService();
			webService.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;

			SinglePackageForPackingWebServiceResponse response = null;
			AssertNoExceptionThrown(() => response = webService.ClosePackageForPacking(packageInfo, printer.PK.ToGuid()));
			AssertEquals(ErrorTypes.None, response.Error);
			Assert(string.IsNullOrEmpty(response.ErrorMessage));
			AssertNotNull(response.PackageForPackingInfo);

			var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var newPackage = newFactory.Load<PkgPackage>(package.PK);
			AssertEquals("Package tare weight got updated.", 2m, newPackage.KP_TareWeight);
			AssertEquals("Package weight is correct.", 12m, newPackage.KP_Weight);
		}

		#region AssertPackage

		void AssertPackage(PkgPackage package, ZString expectedPackageID, ZString expectedPackageType,
			ZDecimal expectedWeight, ZString expectedWeightUQ,
			ZDecimal expectedLength, ZDecimal expectedWidth, ZDecimal expectedHeight, ZString expectedDimensionUQ,
			ZBool expectedIsTote, ZBool expectedIsClosed, ZBool expectedIsHeld, bool isPackageIDChanged = true)
		{
			AssertNotNull("Package should not be null", package);
			if (isPackageIDChanged)
			{
				AssertNotEquals("PackageID should not equal to ToteID", expectedPackageID, package.KP_PackageID);
			}
			else
			{
				AssertEquals("PackageID should be equal", expectedPackageID, package.KP_PackageID);
			}
			AssertEquals("PackageType should be equal", expectedPackageType, package.KP_F3_NKPackType);
			AssertEquals("Weight should be equal", expectedWeight, package.KP_Weight);
			AssertEquals("WeightUQ", expectedWeightUQ, package.KP_WeightUQ);
			AssertEquals("Length should be equal", expectedLength, package.KP_Length);
			AssertEquals("Width should be equal", expectedWidth, package.KP_Width);
			AssertEquals("Height should be equal", expectedHeight, package.KP_Height);
			AssertEquals("DimensionUQ should be equal", expectedDimensionUQ, package.KP_DimensionUQ);
			AssertEquals("IsTote should be equal", expectedIsTote, package.GetIsTote());
			AssertEquals("IsClosed should be equal", expectedIsClosed, package.KP_ClosedTimeUtc.IsValid);
			AssertEquals("IsHeld should be equal", expectedIsHeld, package.KP_IsHeld);
		}

		#endregion
	}
}
