// This file tests CargoWise.DbUpgrader\src\Scripts\Scripts.Definitions\Warehouse\WhsWarehouseUNDGTotals.sql
// If any changes are made to the SQL file, ensure they are tested here
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	class WhsWarehouseUNDGTotalsTest : WhsTestCaseWithFactory
	{
		#region TestQueryParameters

		public void TestQueryParameters_WithDocketToExcludeIsNotNull_AndIsNotExcluded()
		{
			// Arrange
			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WW_IsDangerousGoodsManagementEnabled = true;
			data.Whs1.WW_DGThresholdPercentage = 80;
			var substance = UNDGSubstanceLoader.LoadSubstances(Factory, "0004", "a", "IMO").FirstOrDefault();

			Helper.CreateUNDGDataItem(data.Part1, substance, 2m, "KG", 3m, "M3", substance.DG_Class);
			Helper.CreateWhsUNDGLimit(data.Whs1, substance, string.Empty, null, 30, "KG", 40, "M3");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);

			Factory.Save();

			// Act
			var results = LoadData(data.Whs1.PK, ZGuid.Empty);

			// Assert
			AssertEquals(1, results.Count);
			AssertQueryResult(results[0], substance.PK, null, null, 20m, 30m, "KG", 30m, 40m, "M3");
		}

		public void TestQueryParameters_WithDocketToExcludeIsNotNull_AndIsExcluded()
		{
			// Arrange
			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WW_IsDangerousGoodsManagementEnabled = true;
			data.Whs1.WW_DGThresholdPercentage = 80;
			var substance = UNDGSubstanceLoader.LoadSubstances(Factory, "0004", "a", "IMO").FirstOrDefault();
			AssertEquals("1.1D", substance.DG_Class);

			Helper.CreateUNDGDataItem(data.Part1, substance, 2m, "KG", 3m, "M3", substance.DG_Class);
			Helper.CreateWhsUNDGLimit(data.Whs1, substance, string.Empty, null, 30, "KG", 40, "M3");
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);

			Factory.Save();

			// Act
			var results = LoadData(data.Whs1.PK, receive.PK);

			// Assert
			AssertEquals(0, results.Count);
		}

		public void TestQueryParameters_WithDocketToExcludeIsNull()
		{
			// Arrange
			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WW_IsDangerousGoodsManagementEnabled = true;
			data.Whs1.WW_DGThresholdPercentage = 80;
			var substance = UNDGSubstanceLoader.LoadSubstances(Factory, "0004", "a", "IMO").FirstOrDefault();
			AssertEquals("1.1D", substance.DG_Class);

			Helper.CreateUNDGDataItem(data.Part1, substance, 2m, "KG", 3m, "M3", substance.DG_Class);
			Helper.CreateWhsUNDGLimit(data.Whs1, substance, string.Empty, null, 30, "KG", 40, "M3");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);

			Factory.Save();

			// Act
			var results = LoadData(data.Whs1.PK, null);

			// Assert
			AssertEquals(1, results.Count);
			AssertQueryResult(results[0], substance.PK, null, null, 20m, 30m,"KG", 30m, 40m, "M3");
		}

		#endregion

		#region TestUNDGStock_Receive

		public void TestUNDGStock_Receive_Entered()
		{
			// Arrange
			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WW_IsDangerousGoodsManagementEnabled = true;
			data.Whs1.WW_DGThresholdPercentage = 80;
			var substance = UNDGSubstanceLoader.LoadSubstances(Factory, "0004", "a", "IMO").FirstOrDefault();
			Helper.CreateUNDGDataItem(data.Part1, substance, 2m, "KG", 3m, "M3", substance.DG_Class);
			Helper.CreateWhsUNDGLimit(data.Whs1, substance, string.Empty, null, 30, "KG", 40, "M3");

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			receive.WD_ArrivalDate = ZDateTimeOffset.Empty;
			var receiveLine = Helper.CreateWhsReceiveLine(receive, data.Part1, 2m);
			Factory.Save();

			// Act
			AssertEquals(DocketStatus.Codes.Entered, receive.WD_DocketStatus);
			AssertEquals(InventoryStatus.Codes.Pending, receiveLine.WE_CurrentInventoryStatus);
			var results = LoadData(data.Whs1.PK, ZGuid.Empty);

			// Assert
			AssertEquals(0, results.Count);
		}

		public void TestUNDGStock_Receive_ArrivalDate_Entered()
		{
			// Arrange
			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WW_IsDangerousGoodsManagementEnabled = true;
			data.Whs1.WW_DGThresholdPercentage = 80;
			var substance = UNDGSubstanceLoader.LoadSubstances(Factory, "0004", "a", "IMO").FirstOrDefault();
			Helper.CreateUNDGDataItem(data.Part1, substance, 2m, "KG", 3m, "M3", substance.DG_Class);
			Helper.CreateWhsUNDGLimit(data.Whs1, substance, string.Empty, null, 30, "KG", 40, "M3");

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			receive.WD_ArrivalDate = ZDateTimeOffset.Now;
			var receiveLine = Helper.CreateWhsReceiveLine(receive, data.Part1, 2m);
			Factory.Save();

			// Act
			AssertEquals(DocketStatus.Codes.Entered, receive.WD_DocketStatus);
			AssertEquals(InventoryStatus.Codes.Arrived, receiveLine.WE_CurrentInventoryStatus);
			var results = LoadData(data.Whs1.PK, ZGuid.Empty);

			// Assert
			AssertEquals(0, results.Count);
		}

		public void TestUNDGStock_Receive_ReceiveToDockdoor()
		{
			// Arrange
			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WW_IsDangerousGoodsManagementEnabled = true;
			data.Whs1.WW_DGThresholdPercentage = 80;
			var substance = UNDGSubstanceLoader.LoadSubstances(Factory, "0004", "a", "IMO").FirstOrDefault();
			Helper.CreateUNDGDataItem(data.Part1, substance, 2m, "KG", 3m, "M3", substance.DG_Class);
			Helper.CreateWhsUNDGLimit(data.Whs1, substance, string.Empty, null, 30, "KG", 40, "M3");

			var receive = Helper.CreateWhsReceive(data.Org1.PK, data.Whs1.PK, "INW1", ZDateTimeOffset.Empty);
			receive.WD_ArrivalDate = ZDateTimeOffset.Now;
			var receiveLine = Helper.CreateWhsReceiveLine(receive, data.Part1, 2m, data.Whs1.DefaultInboundDockDoorLocation);
			receiveLine.WE_PalletID = "123";
			Factory.Save();

			// Act
			AssertEquals(DocketStatus.Codes.Entered, receive.WD_DocketStatus);
			AssertEquals(InventoryStatus.Codes.Received, receiveLine.WE_CurrentInventoryStatus);
			var results = LoadData(data.Whs1.PK, ZGuid.Empty);

			// Assert
			AssertEquals(0, results.Count);
		}

		public void TestUNDGStock_Receive_ReceiveToNormalLocation()
		{
			// Arrange
			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WW_IsDangerousGoodsManagementEnabled = true;
			data.Whs1.WW_DGThresholdPercentage = 80;
			var substance = UNDGSubstanceLoader.LoadSubstances(Factory, "0004", "a", "IMO").FirstOrDefault();
			Helper.CreateUNDGDataItem(data.Part1, substance, 2m, "KG", 3m, "M3", substance.DG_Class);
			Helper.CreateWhsUNDGLimit(data.Whs1, substance, string.Empty, null, 30, "KG", 40, "M3");
			var receive = Helper.CreateWhsReceiveWithInventory(
				data.Org1,
				data.Whs1,
				"R1",
				ZDateTimeOffset.Now,
				data.Part1,
				10m,
				"",
				data.Whs1.DefaultLocation,
				"PLT123",
				true,
				false);
			Factory.Save();

			AssertEquals(DocketStatus.Codes.Putaway, receive.WD_DocketStatus);

			var receiveLine = receive.Lines[0];
			AssertEquals(InventoryStatus.Codes.Putaway, receiveLine.WE_CurrentInventoryStatus);
			AssertEquals(data.Whs1.DefaultLocation, receiveLine.Location);

			// Act
			var results = LoadData(data.Whs1.PK, ZGuid.Empty);

			// Assert
			AssertEquals(1, results.Count);
			AssertQueryResult(results[0], substance.PK, null, null, 20m, 30m, "KG", 30m, 40m, "M3");
		}

		public void TestUNDGStock_Receive_ReceiveToDockdoor_ThenPutawayToLocation_Picked()
		{
			// Arrange
			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WW_IsDangerousGoodsManagementEnabled = true;
			data.Whs1.WW_DGThresholdPercentage = 80;
			var substance = UNDGSubstanceLoader.LoadSubstances(Factory, "0004", "a", "IMO").FirstOrDefault();
			Helper.CreateUNDGDataItem(data.Part1, substance, 2m, "KG", 3m, "M3", substance.DG_Class);
			Helper.CreateWhsUNDGLimit(data.Whs1, substance, string.Empty, null, 30, "KG", 40, "M3");

			var receive = Helper.CreateWhsReceive(data.Org1.PK, data.Whs1.PK, "INW1", ZDateTimeOffset.Empty);
			var inventory = Helper.CreateInventoryForDockDoorLocation(receive, data.Part1, data.Whs1.DefaultInboundDockDoorLocation, "PLT_1", 10m);
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR0");
			transfer.WD_IsPutawayTransfer = true;
			Helper.SetupTransferLineForDockDoorLocation(transfer, data.Part1, data.Whs1.DefaultInboundDockDoorLocation, data.Whs1.DefaultLocation, "PLT_1", 10m);
			transfer.RunPreSaveValidation();
			Factory.Save();

			// Act
			var results = LoadData(data.Whs1.PK, ZGuid.Empty);

			// Assert
			AssertEquals(0, results.Count);
		}

		public void TestUNDGStock_Receive_ReceiveToDockdoor_ThenPutawayToLocation_FinalisePick()
		{
			// Arrange
			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WW_IsDangerousGoodsManagementEnabled = true;
			data.Whs1.WW_DGThresholdPercentage = 80;
			var substance = UNDGSubstanceLoader.LoadSubstances(Factory, "0004", "a", "IMO").FirstOrDefault();
			Helper.CreateUNDGDataItem(data.Part1, substance, 2m, "KG", 3m, "M3", substance.DG_Class);
			Helper.CreateWhsUNDGLimit(data.Whs1, substance, string.Empty, null, 30, "KG", 40, "M3");

			var receive = Helper.CreateWhsReceive(data.Org1.PK, data.Whs1.PK, "INW1", ZDateTimeOffset.Empty);
			Helper.CreateInventoryForDockDoorLocation(receive, data.Part1, data.Whs1.DefaultInboundDockDoorLocation, "PLT_1", 10m);
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR0");
			transfer.WD_IsPutawayTransfer = true;
			Helper.SetupTransferLineForDockDoorLocation(transfer, data.Part1, data.Whs1.DefaultInboundDockDoorLocation, data.Whs1.DefaultLocation, "PLT_1", 10m);
			transfer.RunPreSaveValidation();
			transfer.FinaliseDocketWithoutUserConfirmation();
			Factory.Save();

			// Act
			var results = LoadData(data.Whs1.PK, ZGuid.Empty);

			// Assert
			AssertEquals(1, results.Count);
			AssertQueryResult(results[0], substance.PK, null, null, 20m, 30m, "KG", 30m, 40m, "M3");
		}

		public void TestUNDGStock_Receive_Finalised()
		{
			// Arrange
			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WW_IsDangerousGoodsManagementEnabled = true;
			data.Whs1.WW_DGThresholdPercentage = 80;
			var substance = UNDGSubstanceLoader.LoadSubstances(Factory, "0004", "a", "IMO").FirstOrDefault();
			Helper.CreateUNDGDataItem(data.Part1, substance, 2m, "KG", 3m, "M3", substance.DG_Class);
			Helper.CreateWhsUNDGLimit(data.Whs1, substance, string.Empty, null, 30, "KG", 40, "M3");

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var receiveLine = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m, data.Whs1.DefaultLocation);
			receive.FinaliseDocketWithoutUserConfirmation();
			Factory.Save();

			// Act
			AssertEquals(DocketStatus.Codes.Finalised, receive.WD_DocketStatus);
			AssertEquals(InventoryStatus.Codes.Available, receiveLine.WE_CurrentInventoryStatus);
			var results = LoadData(data.Whs1.PK, ZGuid.Empty);

			// Assert
			AssertEquals(1, results.Count);
			AssertQueryResult(results[0], substance.PK, null, null, 20m, 30m, "KG", 30m, 40m, "M3");
		}

		#endregion

		#region TestUNDGStock_Transfer

		public void TestUNDGStock_InnerTransfer_Entered()
		{
			// Arrange
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			data.Whs1.WW_IsDangerousGoodsManagementEnabled = true;
			data.Whs1.WW_DGThresholdPercentage = 80;
			var substance = UNDGSubstanceLoader.LoadSubstances(Factory, "0004", "a", "IMO").FirstOrDefault();
			Helper.CreateUNDGDataItem(data.Part1, substance, 2m, "KG", 3m, "M3", substance.DG_Class);
			Helper.CreateWhsUNDGLimit(data.Whs1, substance, string.Empty, null, 30, "KG", 40, "M3");

			var locationA1 = data.Whs1.FindLocation("A-1");
			var locationA2 = data.Whs1.FindLocation("A-2");
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m, locationA1, "");
			Factory.Save();

			var resultBeforeTransfer = LoadData(data.Whs1.PK, ZGuid.Empty);
			AssertEquals(1, resultBeforeTransfer.Count);
			AssertQueryResult(resultBeforeTransfer[0], substance.PK, null, null, 20m, 30m, "KG", 30m, 40m, "M3");

			// Act
			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, locationA1, locationA2);
			transferLine.RunPreSaveValidation();
			Factory.Save();

			// Assert
			AssertEquals(DocketLineStatus.Codes.Entered, transferLine.WE_DocketLineStatus);

			var resultAfterTransfer = LoadData(data.Whs1.PK, ZGuid.Empty);
			AssertEquals(1, resultAfterTransfer.Count);
			AssertQueryResult(resultAfterTransfer[0], substance.PK, null, null, 20m, 30m, "KG", 30m, 40m, "M3");
		}

		public void TestUNDGStock_InnerTransfer_Picked()
		{
			// Arrange
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			data.Whs1.WW_IsDangerousGoodsManagementEnabled = true;
			data.Whs1.WW_DGThresholdPercentage = 80;
			var substance = UNDGSubstanceLoader.LoadSubstances(Factory, "0004", "a", "IMO").FirstOrDefault();
			Helper.CreateUNDGDataItem(data.Part1, substance, 2m, "KG", 3m, "M3", substance.DG_Class);
			Helper.CreateWhsUNDGLimit(data.Whs1, substance, string.Empty, null, 30, "KG", 40, "M3");

			var locationA1 = data.Whs1.FindLocation("A-1");
			var locationA2 = data.Whs1.FindLocation("A-2");
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m, locationA1, "");
			Factory.Save();

			var resultBeforeTransfer = LoadData(data.Whs1.PK, ZGuid.Empty);
			AssertEquals(1, resultBeforeTransfer.Count);
			AssertQueryResult(resultBeforeTransfer[0], substance.PK, null, null, 20m, 30m, "KG", 30m, 40m, "M3");

			// Act
			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, locationA1, locationA2);
			transferLine.PickedTime = ZDateTimeOffset.Now;
			Factory.Save();

			// Assert
			AssertEquals(DocketLineStatus.Codes.HeldForTransfer, transferLine.WE_DocketLineStatus);

			var resultAfterTransfer = LoadData(data.Whs1.PK, ZGuid.Empty);
			AssertEquals(1, resultAfterTransfer.Count);
			AssertQueryResult(resultAfterTransfer[0], substance.PK, null, null, 20m, 30m, "KG", 30m, 40m, "M3");
		}

		public void TestUNDGStock_InnerTransfer_Finalised()
		{
			// Arrange
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			data.Whs1.WW_IsDangerousGoodsManagementEnabled = true;
			data.Whs1.WW_DGThresholdPercentage = 80;
			var substance = UNDGSubstanceLoader.LoadSubstances(Factory, "0004", "a", "IMO").FirstOrDefault();
			Helper.CreateUNDGDataItem(data.Part1, substance, 2m, "KG", 3m, "M3", substance.DG_Class);
			Helper.CreateWhsUNDGLimit(data.Whs1, substance, string.Empty, null, 30, "KG", 40, "M3");

			var locationA1 = data.Whs1.FindLocation("A-1");
			var locationA2 = data.Whs1.FindLocation("A-2");
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m, locationA1, "");
			Factory.Save();

			var resultBeforeTransfer = LoadData(data.Whs1.PK, ZGuid.Empty);
			AssertEquals(1, resultBeforeTransfer.Count);
			AssertQueryResult(resultBeforeTransfer[0], substance.PK, null, null, 20m, 30m, "KG", 30m, 40m, "M3");

			// Act
			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, locationA1, locationA2);
			transferLine.FinaliseDocketLine();
			transfer.FinaliseDocketWithoutUserConfirmation();
			Factory.Save();

			// Assert
			var resultAfterTransfer = LoadData(data.Whs1.PK, ZGuid.Empty);
			AssertEquals(1, resultAfterTransfer.Count);
			AssertQueryResult(resultAfterTransfer[0], substance.PK, null, null, 20m, 30m, "KG", 30m, 40m, "M3");
		}

		public void TestUNDGStock_InterWarehouseTransferSource_Entered()
		{
			// Arrange
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var whs1 = data.Whs1;
			var whs2 = Helper.CreateWarehouse("WH2", "B");
			whs1.WW_IsDangerousGoodsManagementEnabled = true;
			whs1.WW_DGThresholdPercentage = 80;
			whs2.WW_IsDangerousGoodsManagementEnabled = true;
			whs2.WW_DGThresholdPercentage = 80;
			var substance = UNDGSubstanceLoader.LoadSubstances(Factory, "0004", "a", "IMO").FirstOrDefault();
			Helper.CreateUNDGDataItem(data.Part1, substance, 2m, "KG", 3m, "M3", substance.DG_Class);
			Helper.CreateWhsUNDGLimit(whs1, substance, string.Empty, null, 30, "KG", 40, "M3");
			Helper.CreateWhsUNDGLimit(whs2, substance, string.Empty, null, 50, "KG", 60, "M3");

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, whs1, "R1", data.Part1, 10m, whs1.DefaultLocation, "");
			Factory.Save();

			var whs1ResultBeforeTransfer = LoadData(whs1.PK, ZGuid.Empty);
			AssertEquals(1, whs1ResultBeforeTransfer.Count);
			AssertQueryResult(whs1ResultBeforeTransfer[0], substance.PK, null, null, 20m, 30m, "KG", 30m, 40m, "M3");

			var whs2ResultBeforeTransfer = LoadData(whs2.PK, ZGuid.Empty);
			AssertEquals(0, whs2ResultBeforeTransfer.Count);

			// Act
			var transfer = Helper.CreateWhsTransfer(data.Org1, whs1, "TR1", Notify);
			transfer.WD_DocketSubType = TransferType.Codes.InterWhsSource;
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, "A", whs2.PK, "B");
			transferLine.RunPreSaveValidation();
			Factory.Save();

			// Assert
			AssertEquals(DocketLineStatus.Codes.Entered, transferLine.WE_DocketLineStatus);

			var whs1ResultAfterTransfer = LoadData(whs1.PK, ZGuid.Empty);
			AssertEquals(1, whs1ResultAfterTransfer.Count);
			AssertQueryResult(whs1ResultAfterTransfer[0], substance.PK, null, null, 20m, 30m, "KG", 30m, 40m, "M3");

			var whs2ResultAfterTransfer = LoadData(whs2.PK, ZGuid.Empty);
			AssertEquals(0, whs2ResultAfterTransfer.Count);
		}

		public void TestUNDGStock_InterWarehouseTransferSource_Picked()
		{
			// Arrange
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var whs1 = data.Whs1;
			var whs2 = Helper.CreateWarehouse("WH2", "B");
			whs1.WW_IsDangerousGoodsManagementEnabled = true;
			whs1.WW_DGThresholdPercentage = 80;
			whs2.WW_IsDangerousGoodsManagementEnabled = true;
			whs2.WW_DGThresholdPercentage = 80;
			var substance = UNDGSubstanceLoader.LoadSubstances(Factory, "0004", "a", "IMO").FirstOrDefault();
			Helper.CreateUNDGDataItem(data.Part1, substance, 2m, "KG", 3m, "M3", substance.DG_Class);
			Helper.CreateWhsUNDGLimit(whs1, substance, string.Empty, null, 30, "KG", 40, "M3");
			Helper.CreateWhsUNDGLimit(whs2, substance, string.Empty, null, 50, "KG", 60, "M3");

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, whs1, "R1", data.Part1, 10m, whs1.DefaultLocation, "");
			Factory.Save();

			var whs1ResultBeforeTransfer = LoadData(whs1.PK, ZGuid.Empty);
			AssertEquals(1, whs1ResultBeforeTransfer.Count);
			AssertQueryResult(whs1ResultBeforeTransfer[0], substance.PK, null, null, 20m, 30m, "KG", 30m, 40m, "M3");

			var whs2ResultBeforeTransfer = LoadData(whs2.PK, ZGuid.Empty);
			AssertEquals(0, whs2ResultBeforeTransfer.Count);

			// Act
			var transfer = Helper.CreateWhsTransfer(data.Org1, whs1, "TR1", Notify);
			transfer.WD_DocketSubType = TransferType.Codes.InterWhsSource;
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, "A", whs2.PK, "B");
			transferLine.PickedTime = ZDateTimeOffset.Now;
			transferLine.RunPreSaveValidation();
			Factory.Save();

			// Assert
			AssertEquals(DocketLineStatus.Codes.HeldForTransfer, transferLine.WE_DocketLineStatus);

			var whs1ResultAfterTransfer = LoadData(whs1.PK, ZGuid.Empty);
			AssertEquals(1, whs1ResultAfterTransfer.Count);
			AssertQueryResult(whs1ResultAfterTransfer[0], substance.PK, null, null, 20m, 30m, "KG", 30m, 40m, "M3");

			var whs2ResultAfterTransfer = LoadData(whs2.PK, ZGuid.Empty);
			AssertEquals(0, whs2ResultAfterTransfer.Count);
		}

		public void TestUNDGStock_InterWarehouseTransferSource_Finalised()
		{
			// Arrange
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var whs1 = data.Whs1;
			var whs2 = Helper.CreateWarehouse("WH2", "B");
			whs1.WW_IsDangerousGoodsManagementEnabled = true;
			whs1.WW_DGThresholdPercentage = 80;
			whs2.WW_IsDangerousGoodsManagementEnabled = true;
			whs2.WW_DGThresholdPercentage = 80;
			var substance = UNDGSubstanceLoader.LoadSubstances(Factory, "0004", "a", "IMO").FirstOrDefault();
			Helper.CreateUNDGDataItem(data.Part1, substance, 2m, "KG", 3m, "M3", substance.DG_Class);
			Helper.CreateWhsUNDGLimit(whs1, substance, string.Empty, null, 30, "KG", 40, "M3");
			Helper.CreateWhsUNDGLimit(whs2, substance, string.Empty, null, 50, "KG", 60, "M3");

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, whs1, "R1", data.Part1, 10m, whs1.DefaultLocation, "");
			Factory.Save();

			var whs1ResultBeforeTransfer = LoadData(whs1.PK, ZGuid.Empty);
			AssertEquals(1, whs1ResultBeforeTransfer.Count);
			AssertQueryResult(whs1ResultBeforeTransfer[0], substance.PK, null, null, 20m, 30m, "KG", 30m, 40m, "M3");

			// Act
			var transfer = Helper.CreateWhsTransfer(data.Org1, whs1, "TR1", Notify);
			transfer.WD_DocketSubType = TransferType.Codes.InterWhsSource;
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, "A", whs2.PK, "B");
			transferLine.RunPreSaveValidation();
			transferLine.PickedTime = ZDateTimeOffset.Now;
			transfer.FinaliseDocket();
			Factory.Save();

			// Assert
			AssertEquals(DocketLineStatus.Codes.Finalised, transferLine.WE_DocketLineStatus);

			var whs1ResultAfterTransfer = LoadData(whs1.PK, ZGuid.Empty);
			AssertEquals(0, whs1ResultAfterTransfer.Count);

			var whs2ResultAfterTransfer = LoadData(whs2.PK, ZGuid.Empty);
			AssertEquals(1, whs2ResultAfterTransfer.Count);
			AssertQueryResult(whs2ResultAfterTransfer[0], substance.PK, null, null, 20m, 50m, "KG", 30m, 60m, "M3");
		}

		public void TestUNDGStock_InterWarehouseTransferDestination_Entered()
		{
			// Arrange
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var whs1 = data.Whs1;
			var whs2 = Helper.CreateWarehouse("WH2", "B");
			whs1.WW_IsDangerousGoodsManagementEnabled = true;
			whs1.WW_DGThresholdPercentage = 80;
			whs2.WW_IsDangerousGoodsManagementEnabled = true;
			whs2.WW_DGThresholdPercentage = 80;
			var substance = UNDGSubstanceLoader.LoadSubstances(Factory, "0004", "a", "IMO").FirstOrDefault();
			Helper.CreateUNDGDataItem(data.Part1, substance, 2m, "KG", 3m, "M3", substance.DG_Class);
			Helper.CreateWhsUNDGLimit(whs1, substance, string.Empty, null, 30, "KG", 40, "M3");
			Helper.CreateWhsUNDGLimit(whs2, substance, string.Empty, null, 50, "KG", 60, "M3");

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, whs2, "R1", data.Part1, 10m, whs2.DefaultLocation, "");
			Factory.Save();

			var whs1ResultBeforeTransfer = LoadData(whs1.PK, ZGuid.Empty);
			AssertEquals(0, whs1ResultBeforeTransfer.Count);

			var whs2ResultBeforeTransfer = LoadData(whs2.PK, ZGuid.Empty);
			AssertEquals(1, whs2ResultBeforeTransfer.Count);
			AssertQueryResult(whs2ResultBeforeTransfer[0], substance.PK, null, null, 20m, 50m, "KG", 30m, 60m, "M3");

			// Act
			var transfer = Helper.CreateWhsTransfer(data.Org1, whs1, "TR1", Notify);
			transfer.WD_DocketSubType = TransferType.Codes.InterWhsDest;
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, "B", whs2.PK, "A");
			transferLine.RunPreSaveValidation();
			Factory.Save();

			// Assert
			AssertEquals(DocketLineStatus.Codes.Entered, transferLine.WE_DocketLineStatus);

			var whs1ResultAfterTransfer = LoadData(whs1.PK, ZGuid.Empty);
			AssertEquals(0, whs1ResultAfterTransfer.Count);

			var whs2ResultAfterTransfer = LoadData(whs2.PK, ZGuid.Empty);
			AssertEquals(1, whs2ResultAfterTransfer.Count);
			AssertQueryResult(whs2ResultAfterTransfer[0], substance.PK, null, null, 20m, 50m, "KG", 30m, 60m, "M3");
		}

		public void TestUNDGStock_InterWarehouseTransferDestination_Picked()
		{
			// Arrange
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var whs1 = data.Whs1;
			var whs2 = Helper.CreateWarehouse("WH2", "B");
			whs1.WW_IsDangerousGoodsManagementEnabled = true;
			whs1.WW_DGThresholdPercentage = 80;
			whs2.WW_IsDangerousGoodsManagementEnabled = true;
			whs2.WW_DGThresholdPercentage = 80;
			var substance = UNDGSubstanceLoader.LoadSubstances(Factory, "0004", "a", "IMO").FirstOrDefault();
			Helper.CreateUNDGDataItem(data.Part1, substance, 2m, "KG", 3m, "M3", substance.DG_Class);
			Helper.CreateWhsUNDGLimit(whs1, substance, string.Empty, null, 30, "KG", 40, "M3");
			Helper.CreateWhsUNDGLimit(whs2, substance, string.Empty, null, 50, "KG", 60, "M3");

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, whs2, "R1", data.Part1, 10m, whs2.DefaultLocation, "");
			Factory.Save();

			var whs1ResultBeforeTransfer = LoadData(whs1.PK, ZGuid.Empty);
			AssertEquals(0, whs1ResultBeforeTransfer.Count);

			var whs2ResultBeforeTransfer = LoadData(whs2.PK, ZGuid.Empty);
			AssertEquals(1, whs2ResultBeforeTransfer.Count);
			AssertQueryResult(whs2ResultBeforeTransfer[0], substance.PK, null, null, 20m, 50m, "KG", 30m, 60m, "M3");

			// Act
			var transfer = Helper.CreateWhsTransfer(data.Org1, whs1, "TR1", Notify);
			transfer.WD_DocketSubType = TransferType.Codes.InterWhsDest;
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, "B", whs2.PK, "A");
			transferLine.RunPreSaveValidation();
			transferLine.PickedTime = ZDateTimeOffset.Now;
			Factory.Save();

			// Assert
			AssertEquals(DocketLineStatus.Codes.HeldForTransfer, transferLine.WE_DocketLineStatus);

			var whs1ResultAfterTransfer = LoadData(whs1.PK, ZGuid.Empty);
			AssertEquals(0, whs1ResultAfterTransfer.Count);

			var whs2ResultAfterTransfer = LoadData(whs2.PK, ZGuid.Empty);
			AssertEquals(1, whs2ResultAfterTransfer.Count);
			AssertQueryResult(whs2ResultAfterTransfer[0], substance.PK, null, null, 20m, 50m, "KG", 30m, 60m, "M3");
		}

		public void TestUNDGStock_InterWarehouseTransferDestination_Finalised()
		{
			// Arrange
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var whs1 = data.Whs1;
			var whs2 = Helper.CreateWarehouse("WH2", "B");
			whs1.WW_IsDangerousGoodsManagementEnabled = true;
			whs1.WW_DGThresholdPercentage = 80;
			whs2.WW_IsDangerousGoodsManagementEnabled = true;
			whs2.WW_DGThresholdPercentage = 80;
			var substance = UNDGSubstanceLoader.LoadSubstances(Factory, "0004", "a", "IMO").FirstOrDefault();
			Helper.CreateUNDGDataItem(data.Part1, substance, 2m, "KG", 3m, "M3", substance.DG_Class);
			Helper.CreateWhsUNDGLimit(whs1, substance, string.Empty, null, 30, "KG", 40, "M3");
			Helper.CreateWhsUNDGLimit(whs2, substance, string.Empty, null, 50, "KG", 60, "M3");

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, whs2, "R1", data.Part1, 10m, whs2.DefaultLocation, "");
			Factory.Save();

			var whs1ResultBeforeTransfer = LoadData(whs1.PK, ZGuid.Empty);
			AssertEquals(0, whs1ResultBeforeTransfer.Count);

			var whs2ResultBeforeTransfer = LoadData(whs2.PK, ZGuid.Empty);
			AssertEquals(1, whs2ResultBeforeTransfer.Count);
			AssertQueryResult(whs2ResultBeforeTransfer[0], substance.PK, null, null, 20m, 50m, "KG", 30m, 60m, "M3");

			// Act
			var transfer = Helper.CreateWhsTransfer(data.Org1, whs1, "TR1", Notify);
			transfer.WD_DocketSubType = TransferType.Codes.InterWhsDest;
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, "B", whs2.PK, "A");
			transferLine.RunPreSaveValidation();
			transferLine.PickedTime = ZDateTimeOffset.Now;
			transfer.FinaliseDocket();
			Factory.Save();

			// Assert
			AssertEquals(DocketLineStatus.Codes.Finalised, transferLine.WE_DocketLineStatus);

			var whs1ResultAfterTransfer = LoadData(whs1.PK, ZGuid.Empty);
			AssertEquals(1, whs1ResultAfterTransfer.Count);
			AssertQueryResult(whs1ResultAfterTransfer[0], substance.PK, null, null, 20m, 30m, "KG", 30m, 40m, "M3");

			var whs2ResultAfterTransfer = LoadData(whs2.PK, ZGuid.Empty);
			AssertEquals(0, whs2ResultAfterTransfer.Count);
		}

		#endregion

		#region TestUNDGStock_Adjustment

		public void TestUNDGStock_AdjustmentIn_Entered()
		{
			// Arrange
			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WW_IsDangerousGoodsManagementEnabled = true;
			data.Whs1.WW_DGThresholdPercentage = 80;
			var substance = UNDGSubstanceLoader.LoadSubstances(Factory, "0004", "a", "IMO").FirstOrDefault();
			Helper.CreateUNDGDataItem(data.Part1, substance, 2m, "KG", 3m, "M3", substance.DG_Class);
			Helper.CreateWhsUNDGLimit(data.Whs1, substance, string.Empty, null, 30, "KG", 40, "M3");

			var adjustment = Helper.CreateWhsAdjustment(data.Org1, data.Whs1);
			var adjustmentLine = Helper.CreateWhsAdjustmentLine(adjustment, data.Part1, 2m, data.Whs1.DefaultLocation);
			Factory.Save();

			// Act
			AssertEquals(DocketStatus.Codes.Entered, adjustment.WD_DocketStatus);
			AssertEquals(InventoryStatus.Codes.Available, adjustmentLine.WE_CurrentInventoryStatus);
			var results = LoadData(data.Whs1.PK, ZGuid.Empty);

			// Assert
			AssertEquals(0, results.Count);
		}

		public void TestUNDGStock_AdjustmentIn_Finalised()
		{
			// Arrange
			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WW_IsDangerousGoodsManagementEnabled = true;
			data.Whs1.WW_DGThresholdPercentage = 80;
			var substance = UNDGSubstanceLoader.LoadSubstances(Factory, "0004", "a", "IMO").FirstOrDefault();
			Helper.CreateUNDGDataItem(data.Part1, substance, 2m, "KG", 3m, "M3", substance.DG_Class);
			Helper.CreateWhsUNDGLimit(data.Whs1, substance, string.Empty, null, 30, "KG", 40, "M3");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var resultBeforeAdjustment = LoadData(data.Whs1.PK, ZGuid.Empty);
			AssertEquals(1, resultBeforeAdjustment.Count);
			AssertQueryResult(resultBeforeAdjustment[0], substance.PK, null, null, 20m, 30m, "KG", 30m, 40m, "M3");

			// Act
			var adjustment = Helper.CreateWhsAdjustment(data.Org1, data.Whs1, "111", Notify);
			Helper.CreateWhsAdjustmentLine(adjustment, data.Part1, 2m, data.Whs1.DefaultLocation);
			adjustment.FinaliseDocket();
			Factory.Save();

			// Assert
			var resultAfterAdjustment = LoadData(data.Whs1.PK, ZGuid.Empty);
			AssertEquals(1, resultAfterAdjustment.Count);
			AssertQueryResult(resultAfterAdjustment[0], substance.PK, null, null, 24m, 30m, "KG", 36m, 40m, "M3");
		}

		public void TestUNDGStock_AdjustmentOut_Entered()
		{
			// Arrange
			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WW_IsDangerousGoodsManagementEnabled = true;
			data.Whs1.WW_DGThresholdPercentage = 80;
			var substance = UNDGSubstanceLoader.LoadSubstances(Factory, "0004", "a", "IMO").FirstOrDefault();
			Helper.CreateUNDGDataItem(data.Part1, substance, 2m, "KG", 3m, "M3", substance.DG_Class);
			Helper.CreateWhsUNDGLimit(data.Whs1, substance, string.Empty, null, 30, "KG", 40, "M3");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var resultBeforeAdjustment = LoadData(data.Whs1.PK, ZGuid.Empty);
			AssertEquals(1, resultBeforeAdjustment.Count);
			AssertQueryResult(resultBeforeAdjustment[0], substance.PK, null, null, 20m, 30m, "KG", 30m, 40m, "M3");

			// Act
			var adjustment = Helper.CreateWhsAdjustment(data.Org1, data.Whs1, "111", Notify);
			var adjustmentLine = Helper.CreateWhsAdjustmentLine(adjustment, data.Part1, -10m, data.Whs1.DefaultLocation);
			adjustment.RunPreSaveValidation();
			Factory.Save();

			AssertEquals(DocketStatus.Codes.Entered, adjustment.WD_DocketStatus);
			AssertEquals(InventoryStatus.Codes.Available, adjustmentLine.WE_CurrentInventoryStatus);
			var resultAfterAdjustment = LoadData(data.Whs1.PK, ZGuid.Empty);

			// Assert
			AssertEquals(1, resultAfterAdjustment.Count);
			AssertQueryResult(resultAfterAdjustment[0], substance.PK, null, null, 20m, 30m, "KG", 30m, 40m, "M3");
		}

		public void TestUNDGStock_AdjustmentOut_Finalised()
		{
			// Arrange
			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WW_IsDangerousGoodsManagementEnabled = true;
			data.Whs1.WW_DGThresholdPercentage = 80;
			var substance = UNDGSubstanceLoader.LoadSubstances(Factory, "0004", "a", "IMO").FirstOrDefault();
			Helper.CreateUNDGDataItem(data.Part1, substance, 2m, "KG", 3m, "M3", substance.DG_Class);
			Helper.CreateWhsUNDGLimit(data.Whs1, substance, string.Empty, null, 30, "KG", 40, "M3");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var resultBeforeAdjustment = LoadData(data.Whs1.PK, ZGuid.Empty);
			AssertEquals(1, resultBeforeAdjustment.Count);
			AssertQueryResult(resultBeforeAdjustment[0], substance.PK, null, null, 20m, 30m, "KG", 30m, 40m, "M3");

			// Act
			var adjustment = Helper.CreateWhsAdjustment(data.Org1, data.Whs1, "111", Notify);
			Helper.CreateWhsAdjustmentLine(adjustment, data.Part1, -10m, data.Whs1.DefaultLocation);
			adjustment.FinaliseDocket();
			Factory.Save();

			// Assert
			var resultAfterAdjustment = LoadData(data.Whs1.PK, ZGuid.Empty);
			AssertEquals(0, resultAfterAdjustment.Count);
		}

		#endregion

		#region TestUNDGStock_Order

		public void TestUNDGStock_Order_Entered()
		{
			// Arrange
			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WW_IsDangerousGoodsManagementEnabled = true;
			data.Whs1.WW_DGThresholdPercentage = 80;
			var substance = UNDGSubstanceLoader.LoadSubstances(Factory, "0004", "a", "IMO").FirstOrDefault();
			Helper.CreateUNDGDataItem(data.Part1, substance, 2m, "KG", 3m, "M3", substance.DG_Class);
			Helper.CreateWhsUNDGLimit(data.Whs1, substance, string.Empty, null, 30, "KG", 40, "M3");
			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 5m);
			Factory.Save();

			// Act
			AssertEquals(DocketStatus.Codes.Entered, order.WD_DocketStatus);
			AssertEquals(string.Empty, orderLine.WE_CurrentInventoryStatus);
			var results = LoadData(data.Whs1.PK, ZGuid.Empty);

			// Assert
			AssertEquals(0, results.Count);
		}

		public void TestUNDGStock_Order_Picked()
		{
			// Arrange
			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WW_IsDangerousGoodsManagementEnabled = true;
			data.Whs1.WW_DGThresholdPercentage = 80;
			var substance = UNDGSubstanceLoader.LoadSubstances(Factory, "0004", "a", "IMO").FirstOrDefault();
			Helper.CreateUNDGDataItem(data.Part1, substance, 2m, "KG", 3m, "M3", substance.DG_Class);
			Helper.CreateWhsUNDGLimit(data.Whs1, substance, string.Empty, null, 30, "KG", 40, "M3");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			Helper.CreateWhsOrderLine(order, data.Part1, 5m);

			var pick = Helper.CreatePickNew(order);
			Factory.Save();

			// Act
			var results = LoadData(data.Whs1.PK, ZGuid.Empty);

			// Assert
			AssertEquals(1, results.Count);
			AssertQueryResult(results[0], substance.PK, null, null, 20m, 30m, "KG", 30m, 40m, "M3");
		}

		public void TestUNDGStock_Order_Finalised()
		{
			// Arrange
			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WW_IsDangerousGoodsManagementEnabled = true;
			data.Whs1.WW_DGThresholdPercentage = 80;
			var substance = UNDGSubstanceLoader.LoadSubstances(Factory, "0004", "a", "IMO").FirstOrDefault();
			Helper.CreateUNDGDataItem(data.Part1, substance, 2m, "KG", 3m, "M3", substance.DG_Class);
			Helper.CreateWhsUNDGLimit(data.Whs1, substance, string.Empty, null, 30, "KG", 40, "M3");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 20m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			Helper.CreateWhsOrderLine(order, data.Part1, 20m);

			var pick = Helper.CreatePickNew(order);
			pick.FinaliseAllOrders();
			pick.FinalisePick();
			Factory.Save();

			// Act
			var results = LoadData(data.Whs1.PK, ZGuid.Empty);

			// Assert
			AssertEquals(0, results.Count);
		}

		#endregion

		#region TestUNDGStock_InventoryInTransit

		public void TestUNDGStock_InventoryInTransit()
		{
			// Arrange
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			data.Whs1.WW_IsDangerousGoodsManagementEnabled = true;
			data.Whs1.WW_DGThresholdPercentage = 80;
			var substance = UNDGSubstanceLoader.LoadSubstances(Factory, "0004", "a", "IMO").FirstOrDefault();
			Helper.CreateUNDGDataItem(data.Part1, substance, 2m, "KG", 3m, "M3", substance.DG_Class);
			Helper.CreateWhsUNDGLimit(data.Whs1, substance, string.Empty, null, 30, "KG", 40, "M3");

			var locationA1 = data.Whs1.FindLocation("A-1");
			var locationA2 = data.Whs1.FindLocation("A-2");
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m, locationA1, "");
			Factory.Save();

			var resultBeforeTransfer = LoadData(data.Whs1.PK, ZGuid.Empty);
			AssertEquals(1, resultBeforeTransfer.Count);
			AssertQueryResult(resultBeforeTransfer[0], substance.PK, null, null, 20m, 30m, "KG", 30m, 40m, "M3");

			// Act
			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, locationA1, locationA2);
			transferLine.RunPreSaveValidation();
			transferLine.PickedTime = ZDateTimeOffset.Today;
			Factory.Save();
			AssertEquals("Inventory In Transit", InventoryStatus.Codes.InTransit, transferLine.WE_OriginalInventoryStatus);

			// Assert
			var resultAfterTransfer = LoadData(data.Whs1.PK, ZGuid.Empty);
			AssertEquals(1, resultAfterTransfer.Count);
			AssertQueryResult(resultAfterTransfer[0], substance.PK, null, null, 20m, 30m, "KG", 30m, 40m, "M3");
		}

		#endregion

		#region TestUNDGStock_InventoryHeld

		public void TestUNDGStock_InventoryHeld()
		{
			// Arrange
			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WW_IsDangerousGoodsManagementEnabled = true;
			data.Whs1.WW_DGThresholdPercentage = 80;
			var substance = UNDGSubstanceLoader.LoadSubstances(Factory, "0004", "a", "IMO").FirstOrDefault();
			Helper.CreateUNDGDataItem(data.Part1, substance, 2m, "KG", 3m, "M3", substance.DG_Class);
			Helper.CreateWhsUNDGLimit(data.Whs1, substance, string.Empty, null, 30, "KG", 40, "M3");
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			AssertEquals(DocketStatus.Codes.Finalised, receive.WD_DocketStatus);
			Factory.Save();

			var inventoryLine = receive.Lines[0];
			inventoryLine.HeldCodeChangeQuantity = 3m;
			inventoryLine.HeldCodeToChangeTo = InventoryHoldCodes.Codes.Held;
			inventoryLine.ChangeInventoryHeldCode(true);
			AssertEquals(7m, inventoryLine.WE_StockOnHand);
			Factory.Save();

			// Act
			var results = LoadData(data.Whs1.PK, ZGuid.Empty);

			// Assert
			AssertEquals(1, results.Count);
			AssertQueryResult(results[0], substance.PK, null, null, 20m, 30m, "KG", 30m, 40m, "M3");
		}

		#endregion

		#region TestUNDGLimitOnGoods

		public void TestLimitOnGoods_BySubstance_WithSubstanceUnderLimit()
		{
			// Arrange
			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WW_IsDangerousGoodsManagementEnabled = true;
			data.Whs1.WW_DGThresholdPercentage = 80;
			var substance = UNDGSubstanceLoader.LoadSubstances(Factory, "0004", "a", "IMO").FirstOrDefault();
			Helper.CreateUNDGDataItem(data.Part1, substance, 2m, "KG", 3m, "M3", substance.DG_Class);
			Helper.CreateWhsUNDGLimit(data.Whs1, substance, string.Empty, null, 30, "KG", 40, "M3");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);

			Factory.Save();

			// Act
			var results = LoadData(data.Whs1.PK, ZGuid.Empty);

			// Assert
			AssertEquals(1, results.Count);
			AssertQueryResult(results[0], substance.PK, null, null, 20m, 30m, "KG", 30m, 40m, "M3");
		}

		public void TestLimitOnGoods_BySubstance_WithMultipleSubstanceLimits()
		{
			// Arrange
			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WW_IsDangerousGoodsManagementEnabled = true;
			data.Whs1.WW_DGThresholdPercentage = 80;
			var substance1 = UNDGSubstanceLoader.LoadSubstances(Factory, "0004", "a", "IMO").FirstOrDefault();
			var substance2 = UNDGSubstanceLoader.LoadSubstances(Factory, "0004", "b", "IMO").FirstOrDefault();

			Helper.CreateUNDGDataItem(data.Part1, substance1, 2m, "KG", 3m, "M3", substance1.DG_Class);
			Helper.CreateUNDGDataItem(data.Part1, substance2, 3m, "KG", 4m, "M3", substance2.DG_Class);

			Helper.CreateWhsUNDGLimit(data.Whs1, substance1, string.Empty, null, 30, "KG", 40, "M3");
			Helper.CreateWhsUNDGLimit(data.Whs1, substance2, string.Empty, null, 32, "KG", 42, "M3");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);

			Factory.Save();

			// Act
			var results = LoadData(data.Whs1.PK, ZGuid.Empty);

			// Assert
			AssertEquals(2, results.Count);

			var result1 = results.Single(r => r["UNDGSubstance"].Equals(substance1.PK));
			AssertQueryResult(result1, substance1.PK, null, null, 20m, 30m, "KG", 30m, 40m, "M3");

			var result2 = results.Single(r =>  r["UNDGSubstance"].Equals(substance2.PK));
			AssertQueryResult(result2, substance2.PK, null, null, 30m, 32m, "KG", 40m, 42m, "M3");
		}

		public void TestLimitOnGoods_BySubstance_WithOtherSubstanceNotUnderLimit()
		{
			// Arrange
			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WW_IsDangerousGoodsManagementEnabled = true;
			data.Whs1.WW_DGThresholdPercentage = 80;
			var substance1 = UNDGSubstanceLoader.LoadSubstances(Factory, "0004", "a", "IMO").FirstOrDefault();
			var substance2 = UNDGSubstanceLoader.LoadSubstances(Factory, "0004", "b", "IMO").FirstOrDefault();

			Helper.CreateUNDGDataItem(data.Part1, substance1, 2m, "KG", 3m, "M3", substance1.DG_Class);
			Helper.CreateUNDGDataItem(data.Part1, substance2, 3m, "KG", 4m, "M3", substance2.DG_Class);
			Helper.CreateWhsUNDGLimit(data.Whs1, substance1, string.Empty, null, 30, "KG", 40, "M3");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);

			Factory.Save();

			// Act
			var results = LoadData(data.Whs1.PK, ZGuid.Empty);

			// Assert
			AssertEquals(1, results.Count);
			AssertQueryResult(results[0], substance1.PK, null, null, 20m, 30m, "KG", 30m, 40m, "M3");
		}

		public void TestLimitOnGoods_BySubstance_WithNoSubstanceUnderLimit()
		{
			// Arrange
			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WW_IsDangerousGoodsManagementEnabled = true;
			data.Whs1.WW_DGThresholdPercentage = 80;
			var substance = UNDGSubstanceLoader.LoadSubstances(Factory, "0004", "a", "IMO").FirstOrDefault();
			Helper.CreateWhsUNDGLimit(data.Whs1, substance, string.Empty, null, 30, "KG", 40, "M3");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);

			Factory.Save();

			// Act
			var results = LoadData(data.Whs1.PK, ZGuid.Empty);

			// Assert
			AssertEquals(0, results.Count);
		}

		public void TestLimitOnGoods_ByCountryReference_WithSubstanceUnderLimit()
		{
			// Arrange
			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WW_IsDangerousGoodsManagementEnabled = true;
			data.Whs1.WW_DGThresholdPercentage = 80;
			var substance = UNDGSubstanceLoader.LoadSubstances(Factory, "0004", "a", "IMO").FirstOrDefault();
			var countryReference = Helper.CreateCountryReference();
			Helper.CreateCountryReferencePivot(countryReference, substance);
			Helper.CreateUNDGDataItem(data.Part1, substance, 2m, "KG", 3m, "M3", substance.DG_Class);
			Helper.CreateWhsUNDGLimit(data.Whs1, "", "", countryReference, 30, "KG", 40, "M3");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);

			Factory.Save();

			// Act
			var results = LoadData(data.Whs1.PK, ZGuid.Empty);

			// Assert
			AssertEquals(1, results.Count);
			AssertQueryResult(results[0], null, countryReference.PK, null, 20m, 30m, "KG", 30m, 40m, "M3");
		}

		public void TestLimitOnGoods_ByCountryReference_WithMultipleSubstancesUnderOneCountryReference()
		{
			// Arrange
			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WW_IsDangerousGoodsManagementEnabled = true;
			data.Whs1.WW_DGThresholdPercentage = 80;
			var substance1 = UNDGSubstanceLoader.LoadSubstances(Factory, "0004", "a", "IMO").FirstOrDefault();
			var substance2 = UNDGSubstanceLoader.LoadSubstances(Factory, "0004", "b", "IMO").FirstOrDefault();

			var countryReference = Helper.CreateCountryReference();
			Helper.CreateCountryReferencePivot(countryReference, substance1);
			Helper.CreateCountryReferencePivot(countryReference, substance2);

			Helper.CreateUNDGDataItem(data.Part1, substance1, 2m, "KG", 3m, "M3", substance1.DG_Class);
			Helper.CreateUNDGDataItem(data.Part1, substance2, 3m, "KG", 4m, "M3", substance2.DG_Class);
			Helper.CreateWhsUNDGLimit(data.Whs1, "", "", countryReference, 30, "KG", 40, "M3");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);

			Factory.Save();

			// Act
			var results = LoadData(data.Whs1.PK, ZGuid.Empty);

			// Assert
			AssertEquals(1, results.Count);
			AssertQueryResult(results[0], null, countryReference.PK, null, 50m, 30m, "KG", 70m, 40m, "M3");
		}

		public void TestLimitOnGoods_ByCountryReference_WithOtherSubstanceNotUnderLimit()
		{
			// Arrange
			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WW_IsDangerousGoodsManagementEnabled = true;
			data.Whs1.WW_DGThresholdPercentage = 80;
			var substance1 = UNDGSubstanceLoader.LoadSubstances(Factory, "0004", "a", "IMO").FirstOrDefault();
			var substance2 = UNDGSubstanceLoader.LoadSubstances(Factory, "0004", "b", "IMO").FirstOrDefault();

			var countryReference1 = Helper.CreateCountryReference(referenceCode: "1234");
			Helper.CreateCountryReferencePivot(countryReference1, substance1);

			var countryReference2 = Helper.CreateCountryReference(referenceCode: "2345");
			Helper.CreateCountryReferencePivot(countryReference2, substance2);

			Helper.CreateUNDGDataItem(data.Part1, substance1, 2m, "KG", 3m, "M3", substance1.DG_Class);
			Helper.CreateUNDGDataItem(data.Part1, substance2, 4m, "KG", 5m, "M3", substance2.DG_Class);
			Helper.CreateWhsUNDGLimit(data.Whs1, "", "", countryReference1, 30, "KG", 40, "M3");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);

			Factory.Save();

			// Act
			var results = LoadData(data.Whs1.PK, ZGuid.Empty);

			// Assert
			AssertEquals(1, results.Count);
			AssertQueryResult(results[0], null, countryReference1.PK, null, 20m, 30m, "KG", 30m, 40m, "M3");
		}

		public void TestLimitOnGoods_ByCountryReference_WithNoSubstanceUnderLimit()
		{
			// Arrange
			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WW_IsDangerousGoodsManagementEnabled = true;
			data.Whs1.WW_DGThresholdPercentage = 80;
			var substance = UNDGSubstanceLoader.LoadSubstances(Factory, "0004", "a", "IMO").FirstOrDefault();

			var countryReference = Helper.CreateCountryReference();
			Helper.CreateCountryReferencePivot(countryReference, substance);

			Helper.CreateWhsUNDGLimit(data.Whs1, "", "", countryReference, 30, "KG", 40, "M3");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);

			Factory.Save();

			// Act
			var results = LoadData(data.Whs1.PK, ZGuid.Empty);

			// Assert
			AssertEquals(0, results.Count);
		}

		public void TestLimitOnGoods_ByCountryReference_SubstanceNotInTableZZUNDGSubstance()
		{
			// Arrange
			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WW_IsDangerousGoodsManagementEnabled = true;
			data.Whs1.WW_DGThresholdPercentage = 80;
			var substanceRID = Factory.New<UNDGSubstanceRID>();
			substanceRID.RID_UNNO = "9999";
			Factory.Save();

			var substance = UNDGSubstanceLoader.LoadSubstances(Factory, "9999", "", "RID").First();
			var countryReference = Helper.CreateCountryReference();
			Helper.CreateCountryReferencePivot(countryReference, substance);
			Helper.CreateUNDGDataItem(data.Part1, substance, 2m, "KG", 3m, "M3", substance.DG_Class);
			Helper.CreateWhsUNDGLimit(data.Whs1, "", "", countryReference, 30, "KG", 40, "M3");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			// Act
			var results = LoadData(data.Whs1.PK, ZGuid.Empty);

			// Assert
			AssertEquals(1, results.Count);
			AssertQueryResult(results[0], null, countryReference.PK, null, 20m, 30m, "KG", 30m, 40m, "M3");
		}

		public void TestLimitOnGoods_ByClass_WithSubstanceUnderLimit()
		{
			// Arrange
			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WW_IsDangerousGoodsManagementEnabled = true;
			data.Whs1.WW_DGThresholdPercentage = 80;
			var substance = UNDGSubstanceLoader.LoadSubstances(Factory, "0004", "a", "IMO").FirstOrDefault();

			Helper.CreateUNDGDataItem(data.Part1, substance, 2m, "KG", 3m, "M3", substance.DG_Class);
			Helper.CreateWhsUNDGLimit(data.Whs1, "", "1", null, 30, "KG", 40, "M3");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);

			Factory.Save();

			// Act
			var results = LoadData(data.Whs1.PK, ZGuid.Empty);

			// Assert
			AssertEquals(1, results.Count);
			AssertQueryResult(results[0], null, null, "1", 20m, 30m, "KG", 30m, 40m, "M3");
		}

		public void TestLimitOnGoods_ByClass_WithMultipleSubstancesUnderOneClass()
		{
			// Arrange
			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WW_IsDangerousGoodsManagementEnabled = true;
			data.Whs1.WW_DGThresholdPercentage = 80;
			var substance1 = UNDGSubstanceLoader.LoadSubstances(Factory, "0004", "a", "IMO").FirstOrDefault();
			var substance2 = UNDGSubstanceLoader.LoadSubstances(Factory, "0004", "b", "IMO").FirstOrDefault();
			AssertEquals("1.1D", substance1.DG_Class);
			AssertEquals("1.1D", substance2.DG_Class);

			Helper.CreateUNDGDataItem(data.Part1, substance1, 2m, "KG", 3m, "M3", substance1.DG_Class);
			Helper.CreateUNDGDataItem(data.Part1, substance2, 3m, "KG", 4m, "M3", substance2.DG_Class);
			Helper.CreateWhsUNDGLimit(data.Whs1, "", "1", null, 30, "KG", 40, "M3");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);

			Factory.Save();

			// Act
			var results = LoadData(data.Whs1.PK, ZGuid.Empty);

			// Assert
			AssertEquals(1, results.Count);
			AssertQueryResult(results[0], null, null, "1", 50m, 30m, "KG", 70m, 40m, "M3");
		}

		public void TestLimitOnGoods_ByClass_WithOtherSubstanceNotUnderLimit()
		{
			// Arrange
			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WW_IsDangerousGoodsManagementEnabled = true;
			data.Whs1.WW_DGThresholdPercentage = 80;
			var substance1 = UNDGSubstanceLoader.LoadSubstances(Factory, "0004", "a", "IMO").FirstOrDefault();
			var substance2 = UNDGSubstanceLoader.LoadSubstances(Factory, "1001", "", "IMO").FirstOrDefault();
			AssertEquals("1.1D", substance1.DG_Class);
			AssertEquals("2.1", substance2.DG_Class);

			Helper.CreateUNDGDataItem(data.Part1, substance1, 2m, "KG", 3m, "M3", substance1.DG_Class);
			Helper.CreateUNDGDataItem(data.Part1, substance2, 3m, "KG", 4m, "M3", substance2.DG_Class);
			Helper.CreateWhsUNDGLimit(data.Whs1, "", "1", null, 30, "KG", 40, "M3");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);

			Factory.Save();

			// Act
			var results = LoadData(data.Whs1.PK, ZGuid.Empty);

			// Assert
			AssertEquals(1, results.Count);
			AssertQueryResult(results[0], null, null, "1", 20m, 30m, "KG", 30m, 40m, "M3");
		}

		public void TestLimitOnGoods_ByClass_WithNoSubstanceUnderLimit()
		{
			// Arrange
			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WW_IsDangerousGoodsManagementEnabled = true;
			data.Whs1.WW_DGThresholdPercentage = 80;
			var substance = UNDGSubstanceLoader.LoadSubstances(Factory, "0004", "a", "IMO").FirstOrDefault();
			AssertEquals("1.1D", substance.DG_Class);

			Helper.CreateWhsUNDGLimit(data.Whs1, "", "1", null, 30, "KG", 40, "M3");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);

			Factory.Save();

			// Act
			var results = LoadData(data.Whs1.PK, ZGuid.Empty);

			// Assert
			AssertEquals(0, results.Count);
		}

		public void TestLimitOnGoods_ByClass_ClassComb()
		{
			// Arrange
			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WW_IsDangerousGoodsManagementEnabled = true;
			data.Whs1.WW_DGThresholdPercentage = 80;

			var substance = Factory.New<UNDGSubstance>();
			substance.DG_UNNO = "2024";
			substance.DG_Variant = "xx";
			substance.DG_Standard = "IMO";
			substance.DG_Class = "Comb";

			Helper.CreateUNDGDataItem(data.Part1, substance, 2m, "KG", 3m, "M3", substance.DG_Class);
			Helper.CreateWhsUNDGLimit(data.Whs1, "", "Comb", null, 30, "KG", 40, "M3");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);

			Factory.Save();

			// Act
			var results = LoadData(data.Whs1.PK, ZGuid.Empty);

			// Assert
			AssertEquals(1, results.Count);
			AssertQueryResult(results[0], null, null, "Comb", 20m, 30m, "KG", 30m, 40m, "M3");
		}

		public void TestLimitOnGoods_BySubstance_AndCountryReference_AndClass()
		{
			// Arrange
			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WW_IsDangerousGoodsManagementEnabled = true;
			data.Whs1.WW_DGThresholdPercentage = 65;
			var substance = UNDGSubstanceLoader.LoadSubstances(Factory, "0004", "a", "IMO").FirstOrDefault();
			AssertEquals("1.1D", substance.DG_Class);

			Helper.CreateUNDGDataItem(data.Part1, substance, 2m, "KG", 3m, "M3", substance.DG_Class);
			Helper.CreateWhsUNDGLimit(data.Whs1, substance, string.Empty, null, 30, "KG", 40, "M3");
			var countryReference = Helper.CreateCountryReference();
			Helper.CreateCountryReferencePivot(countryReference, substance);
			Helper.CreateWhsUNDGLimit(data.Whs1, "", "", countryReference, 40, "KG", 50, "M3");
			Helper.CreateWhsUNDGLimit(data.Whs1, "", "1", null, 50, "KG", 60, "M3");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);

			Factory.Save();

			// Act
			var results = LoadData(data.Whs1.PK, ZGuid.Empty);

			// Assert
			AssertEquals(3, results.Count);

			var result1 = results.Single(r => r["UNDGSubstance"].Equals(substance.PK));
			AssertQueryResult(result1, substance.PK, null, null, 20m, 30m, "KG", 30m, 40m, "M3");

			var result2 = results.Single(r => r["UNDGCountryReference"].Equals(countryReference.PK));
			AssertQueryResult(result2, null, countryReference.PK, null, 20m, 40m, "KG", 30m, 50m, "M3");

			var result3 = results.Single(r => r["UNDGClass"].Equals("1"));
			AssertQueryResult(result3, null, null, "1", 20m, 50m, "KG", 30m, 60m, "M3");
		}

		#endregion

		#region TestUQConversion

		public void TestUQConversion_WarehouseLimit()
		{
			// Arrange
			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WW_IsDangerousGoodsManagementEnabled = true;
			data.Whs1.WW_DGThresholdPercentage = 80;
			var substance1 = UNDGSubstanceLoader.LoadSubstances(Factory, "0004", "a", "IMO").FirstOrDefault();
			var substance2 = UNDGSubstanceLoader.LoadSubstances(Factory, "0004", "b", "IMO").FirstOrDefault();
			AssertEquals("1.1D", substance1.DG_Class);
			AssertEquals("1.1D", substance2.DG_Class);

			Helper.CreateUNDGDataItem(data.Part1, substance1, 2m, "KG", 3m, "M3", substance1.DG_Class);
			Helper.CreateUNDGDataItem(data.Part1, substance2, 4m, "KG", 5m, "M3", substance2.DG_Class);
			Helper.CreateWhsUNDGLimit(data.Whs1, substance1, string.Empty, null, 30m, "LB", 4000m, "L");
			Helper.CreateWhsUNDGLimit(data.Whs1, substance2, string.Empty, null, 300m, "OZ", 4000m, "GA");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 1m);

			Factory.Save();

			// Act
			var results = LoadData(data.Whs1.PK, ZGuid.Empty);

			// Assert
			AssertEquals(2, results.Count);

			var result1 = results.Single(r => r["UNDGSubstance"].Equals(substance1.PK));
			AssertQueryResult(result1, substance1.PK, null, null, 4.409m, 30m, "LB", 3000m, 4000m, "L");

			var result2 = results.Single(r => r["UNDGSubstance"].Equals(substance2.PK));
			AssertQueryResult(result2, substance2.PK, null, null, 141.096m, 300m, "OZ", 1320.86m, 4000m, "GA");
		}

		#endregion

		DynamicBusinessObjectCollection LoadData(ZGuid warehousePK, ZGuid? docketToExcludePK = null)
		{
			var rawSql = @"SELECT UNDGSubstance, UNDGCountryReference, UNDGClass, TotalWeight, TotalWeightLimit, TotalWeightLimitUQ, TotalVolume, TotalVolumeLimit, TotalVolumeLimitUQ
FROM dbo.WhsWarehouseUNDGTotals(@WarehousePK, @DocketPK)";

			var collection = new DynamicBusinessObjectCollection(Factory);
			collection.Load(rawSql, new[]
			{
				ZSqlParameter.New("@WarehousePK", warehousePK, WhsWarehouseSchema.PK),
				docketToExcludePK is null ? ZSqlParameter.New("@DocketPK", null, DummyBizoSchema.Z0_Guid) : ZSqlParameter.New("@DocketPK", docketToExcludePK, WhsDocketSchema.PK),
			});

			return collection;
		}

		void AssertQueryResult(DynamicBusinessObject result, ZGuid? substancePK, ZGuid? countryReferencePK, string undgClass, decimal totalWeight, decimal weightLimit, string totalWeightUQ, decimal totalVolume, decimal volumeLimit, string totalVolumeUQ)
		{
			if (substancePK != null)
			{
				AssertEquals(substancePK, result["UNDGSubstance"]);
				AssertEquals(ZGuid.Empty, result["UNDGCountryReference"]);
				AssertEquals(ZString.Empty, result["UNDGClass"]);
			}
			else if (countryReferencePK != null)
			{
				AssertEquals(ZGuid.Empty, result["UNDGSubstance"]);
				AssertEquals(countryReferencePK, result["UNDGCountryReference"]);
				AssertEquals(ZString.Empty, result["UNDGClass"]);
			}
			else if(undgClass != null)
			{
				AssertEquals(ZGuid.Empty, result["UNDGSubstance"]);
				AssertEquals(ZGuid.Empty, result["UNDGCountryReference"]);
				AssertEquals(undgClass, result["UNDGClass"]);
			}

			AssertEquals("Total weight:", totalWeight, result["TotalWeight"]);
			AssertEquals("Weight limit:", weightLimit, result["TotalWeightLimit"]);
			AssertEquals("Total weight UQ:", totalWeightUQ, result["TotalWeightLimitUQ"]);
			AssertEquals("Total volume:", totalVolume, result["TotalVolume"]);
			AssertEquals("Volume limit:", volumeLimit, result["TotalVolumeLimit"]);
			AssertEquals("Total volume UQ:", totalVolumeUQ, result["TotalVolumeLimitUQ"]);
		}
	}
}
