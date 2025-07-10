// This file tests CargoWise.DbUpgrader\src\Scripts\Scripts.Definitions\Warehouse\WhsWarehouseUNDGTotalsBySubstance.sql
// If any changes are made to the SQL file, ensure they are tested here
using System.Linq;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Transactions.CodeLists;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	class WhsWarehouseUNDGTotalsBySubstanceTest : WhsTestCaseWithFactory
	{
		#region TestUNDGStockAndWarehouseLimit

		public void TestNoUNDGStock()
		{
			// Arrange
			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WW_IsDangerousGoodsManagementEnabled = true;
			data.Whs1.WW_DGThresholdPercentage = 80;
			var substance = UNDGSubstanceLoader.LoadSubstances(Factory, "0004", "a", "IMO").FirstOrDefault();
			Factory.Save();

			// Act
			var results = WhsWarehouseUNDGTotalsHelper.LoadWhsWarehouseUNDGTotalsBySubstance(Factory, data.Whs1.PK, substance.PK);

			// Assert
			AssertEquals(0, results.Count);
		}

		public void TestNoUNDGStock_OtherSubstanceHasStock()
		{
			// Arrange
			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WW_IsDangerousGoodsManagementEnabled = true;
			data.Whs1.WW_DGThresholdPercentage = 80;
			var substance1 = UNDGSubstanceLoader.LoadSubstances(Factory, "0004", "a", "IMO").FirstOrDefault();
			var substance2 = UNDGSubstanceLoader.LoadSubstances(Factory, "0004", "b", "IMO").FirstOrDefault();
			Helper.CreateUNDGDataItem(data.Part1, substance1, 2m, "KG", 3m, "M3", substance1.DG_Class);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			// Act
			var results = WhsWarehouseUNDGTotalsHelper.LoadWhsWarehouseUNDGTotalsBySubstance(Factory, data.Whs1.PK, substance2.PK);

			// Assert
			AssertEquals(0, results.Count);
		}

		public void TestHasUNDGStock()
		{
			// Arrange
			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WW_IsDangerousGoodsManagementEnabled = true;
			data.Whs1.WW_DGThresholdPercentage = 80;
			var substance = UNDGSubstanceLoader.LoadSubstances(Factory, "0004", "a", "IMO").FirstOrDefault();
			Helper.CreateUNDGDataItem(data.Part1, substance, 2m, "KG", 3m, "M3", substance.DG_Class);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			// Act
			var results = WhsWarehouseUNDGTotalsHelper.LoadWhsWarehouseUNDGTotalsBySubstance(Factory, data.Whs1.PK, substance.PK);

			// Assert
			AssertEquals(1, results.Count);
			AssertQueryResult(results.ElementAt(0), 20m, "KG", 30m, "M3");
		}

		public void TestHasUNDGStock_MultipleSubstance_SameUQ()
		{
			// Arrange
			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WW_IsDangerousGoodsManagementEnabled = true;
			data.Whs1.WW_DGThresholdPercentage = 80;
			var substance = UNDGSubstanceLoader.LoadSubstances(Factory, "0004", "a", "IMO").FirstOrDefault();
			Helper.CreateUNDGDataItem(data.Part1, substance, 1m, "DT", 2m, "CC", substance.DG_Class);
			Helper.CreateUNDGDataItem(data.Part2, substance, 3m, "DT", 4m, "CC", substance.DG_Class);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part2, 10m);
			Factory.Save();

			// Act
			var results = WhsWarehouseUNDGTotalsHelper.LoadWhsWarehouseUNDGTotalsBySubstance(Factory, data.Whs1.PK, substance.PK);

			// Assert
			AssertEquals(1, results.Count);
			AssertQueryResult(results.ElementAt(0), 40m, "DT", 60m, "CC");
		}

		public void TestHasUNDGStock_MultipleSubstance_DifferentUQ()
		{
			// Arrange
			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WW_IsDangerousGoodsManagementEnabled = true;
			data.Whs1.WW_DGThresholdPercentage = 80;
			var substance = UNDGSubstanceLoader.LoadSubstances(Factory, "0004", "a", "IMO").FirstOrDefault();
			Helper.CreateUNDGDataItem(data.Part1, substance, 1m, "DT", 2m, "CC", substance.DG_Class);
			Helper.CreateUNDGDataItem(data.Part2, substance, 3m, "G", 4m, "CF", substance.DG_Class);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part2, 10m);
			Factory.Save();

			// Act
			var results = WhsWarehouseUNDGTotalsHelper.LoadWhsWarehouseUNDGTotalsBySubstance(Factory, data.Whs1.PK, substance.PK);

			// Assert
			AssertEquals(2, results.Count);

			var result1 = results.Single(r => r.TotalWeightUQ.Equals("DT"));
			AssertQueryResult(result1, 10m, "DT", 20m, "CC");

			var result2 = results.Single(r => r.TotalWeightUQ.Equals("G"));
			AssertQueryResult(result2, 30m, "G", 40m, "CF");
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

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			receive.WD_ArrivalDate = ZDateTimeOffset.Empty;
			var receiveLine = Helper.CreateWhsReceiveLine(receive, data.Part1, 2m);
			Factory.Save();

			// Act
			AssertEquals(DocketStatus.Codes.Entered, receive.WD_DocketStatus);
			AssertEquals(InventoryStatus.Codes.Pending, receiveLine.WE_CurrentInventoryStatus);
			var results = WhsWarehouseUNDGTotalsHelper.LoadWhsWarehouseUNDGTotalsBySubstance(Factory, data.Whs1.PK, substance.PK);

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

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			receive.WD_ArrivalDate = ZDateTimeOffset.Now;
			var receiveLine = Helper.CreateWhsReceiveLine(receive, data.Part1, 2m);
			Factory.Save();

			// Act
			AssertEquals(DocketStatus.Codes.Entered, receive.WD_DocketStatus);
			AssertEquals(InventoryStatus.Codes.Arrived, receiveLine.WE_CurrentInventoryStatus);
			var results = WhsWarehouseUNDGTotalsHelper.LoadWhsWarehouseUNDGTotalsBySubstance(Factory, data.Whs1.PK, substance.PK);

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

			var receive = Helper.CreateWhsReceive(data.Org1.PK, data.Whs1.PK, "INW1", ZDateTimeOffset.Empty);
			receive.WD_ArrivalDate = ZDateTimeOffset.Now;
			var receiveLine = Helper.CreateWhsReceiveLine(receive, data.Part1, 2m, data.Whs1.DefaultInboundDockDoorLocation);
			receiveLine.WE_PalletID = "123";
			Factory.Save();

			// Act
			AssertEquals(DocketStatus.Codes.Entered, receive.WD_DocketStatus);
			AssertEquals(InventoryStatus.Codes.Received, receiveLine.WE_CurrentInventoryStatus);
			var results = WhsWarehouseUNDGTotalsHelper.LoadWhsWarehouseUNDGTotalsBySubstance(Factory, data.Whs1.PK, substance.PK);

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
			var results = WhsWarehouseUNDGTotalsHelper.LoadWhsWarehouseUNDGTotalsBySubstance(Factory, data.Whs1.PK, substance.PK);

			// Assert
			AssertEquals(1, results.Count);
			AssertQueryResult(results.ElementAt(0), 20, "KG", 30, "M3");
		}

		public void TestUNDGStock_Receive_ReceiveToDockdoor_ThenPutawayToLocation_Picked()
		{
			// Arrange
			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WW_IsDangerousGoodsManagementEnabled = true;
			data.Whs1.WW_DGThresholdPercentage = 80;
			var substance = UNDGSubstanceLoader.LoadSubstances(Factory, "0004", "a", "IMO").FirstOrDefault();
			Helper.CreateUNDGDataItem(data.Part1, substance, 2m, "KG", 3m, "M3", substance.DG_Class);

			var receive = Helper.CreateWhsReceive(data.Org1.PK, data.Whs1.PK, "INW1", ZDateTimeOffset.Empty);
			var inventory = Helper.CreateInventoryForDockDoorLocation(receive, data.Part1, data.Whs1.DefaultInboundDockDoorLocation, "PLT_1", 10m);
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR0");
			transfer.WD_IsPutawayTransfer = true;
			Helper.SetupTransferLineForDockDoorLocation(transfer, data.Part1, data.Whs1.DefaultInboundDockDoorLocation, data.Whs1.DefaultLocation, "PLT_1", 10m);
			transfer.RunPreSaveValidation();
			Factory.Save();

			// Act
			var results = WhsWarehouseUNDGTotalsHelper.LoadWhsWarehouseUNDGTotalsBySubstance(Factory, data.Whs1.PK, substance.PK);

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
			var results = WhsWarehouseUNDGTotalsHelper.LoadWhsWarehouseUNDGTotalsBySubstance(Factory, data.Whs1.PK, substance.PK);

			// Assert
			AssertEquals(1, results.Count);
			AssertQueryResult(results.ElementAt(0), 20, "KG", 30, "M3");
		}

		public void TestUNDGStock_Receive_Finalised()
		{
			// Arrange
			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WW_IsDangerousGoodsManagementEnabled = true;
			data.Whs1.WW_DGThresholdPercentage = 80;
			var substance = UNDGSubstanceLoader.LoadSubstances(Factory, "0004", "a", "IMO").FirstOrDefault();
			Helper.CreateUNDGDataItem(data.Part1, substance, 2m, "KG", 3m, "M3", substance.DG_Class);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var receiveLine = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m, data.Whs1.DefaultLocation);
			receive.FinaliseDocketWithoutUserConfirmation();
			Factory.Save();

			// Act
			AssertEquals(DocketStatus.Codes.Finalised, receive.WD_DocketStatus);
			AssertEquals(InventoryStatus.Codes.Available, receiveLine.WE_CurrentInventoryStatus);
			var results = WhsWarehouseUNDGTotalsHelper.LoadWhsWarehouseUNDGTotalsBySubstance(Factory, data.Whs1.PK, substance.PK);

			// Assert
			AssertEquals(1, results.Count);
			AssertQueryResult(results.ElementAt(0), 20, "KG", 30, "M3");
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

			var locationA1 = data.Whs1.FindLocation("A-1");
			var locationA2 = data.Whs1.FindLocation("A-2");
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m, locationA1, "");
			Factory.Save();

			var resultBeforeTransfer = WhsWarehouseUNDGTotalsHelper.LoadWhsWarehouseUNDGTotalsBySubstance(Factory, data.Whs1.PK, substance.PK);
			AssertEquals(1, resultBeforeTransfer.Count);
			AssertQueryResult(resultBeforeTransfer.ElementAt(0), 20, "KG", 30, "M3");

			// Act
			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, locationA1, locationA2);
			transferLine.RunPreSaveValidation();
			Factory.Save();

			// Assert
			AssertEquals(DocketLineStatus.Codes.Entered, transferLine.WE_DocketLineStatus);

			var resultAfterTransfer = WhsWarehouseUNDGTotalsHelper.LoadWhsWarehouseUNDGTotalsBySubstance(Factory, data.Whs1.PK, substance.PK);
			AssertEquals(1, resultAfterTransfer.Count);
			AssertQueryResult(resultAfterTransfer.ElementAt(0), 20, "KG", 30, "M3");
		}

		public void TestUNDGStock_InnerTransfer_Picked()
		{
			// Arrange
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			data.Whs1.WW_IsDangerousGoodsManagementEnabled = true;
			data.Whs1.WW_DGThresholdPercentage = 80;
			var substance = UNDGSubstanceLoader.LoadSubstances(Factory, "0004", "a", "IMO").FirstOrDefault();
			Helper.CreateUNDGDataItem(data.Part1, substance, 2m, "KG", 3m, "M3", substance.DG_Class);

			var locationA1 = data.Whs1.FindLocation("A-1");
			var locationA2 = data.Whs1.FindLocation("A-2");
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m, locationA1, "");
			Factory.Save();

			var resultBeforeTransfer = WhsWarehouseUNDGTotalsHelper.LoadWhsWarehouseUNDGTotalsBySubstance(Factory, data.Whs1.PK, substance.PK);
			AssertEquals(1, resultBeforeTransfer.Count);
			AssertQueryResult(resultBeforeTransfer.ElementAt(0), 20, "KG", 30, "M3");

			// Act
			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, locationA1, locationA2);
			transferLine.PickedTime = ZDateTimeOffset.Now;
			Factory.Save();

			// Assert
			AssertEquals(DocketLineStatus.Codes.HeldForTransfer, transferLine.WE_DocketLineStatus);

			var resultAfterTransfer = WhsWarehouseUNDGTotalsHelper.LoadWhsWarehouseUNDGTotalsBySubstance(Factory, data.Whs1.PK, substance.PK);
			AssertEquals(1, resultAfterTransfer.Count);
			AssertQueryResult(resultAfterTransfer.ElementAt(0), 20, "KG", 30, "M3");
		}

		public void TestUNDGStock_InnerTransfer_Finalised()
		{
			// Arrange
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			data.Whs1.WW_IsDangerousGoodsManagementEnabled = true;
			data.Whs1.WW_DGThresholdPercentage = 80;
			var substance = UNDGSubstanceLoader.LoadSubstances(Factory, "0004", "a", "IMO").FirstOrDefault();
			Helper.CreateUNDGDataItem(data.Part1, substance, 2m, "KG", 3m, "M3", substance.DG_Class);

			var locationA1 = data.Whs1.FindLocation("A-1");
			var locationA2 = data.Whs1.FindLocation("A-2");
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m, locationA1, "");
			Factory.Save();

			var resultBeforeTransfer = WhsWarehouseUNDGTotalsHelper.LoadWhsWarehouseUNDGTotalsBySubstance(Factory, data.Whs1.PK, substance.PK);
			AssertEquals(1, resultBeforeTransfer.Count);
			AssertQueryResult(resultBeforeTransfer.ElementAt(0), 20, "KG", 30, "M3");

			// Act
			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, locationA1, locationA2);
			transferLine.FinaliseDocketLine();
			transfer.FinaliseDocketWithoutUserConfirmation();
			Factory.Save();

			// Assert
			var resultAfterTransfer = WhsWarehouseUNDGTotalsHelper.LoadWhsWarehouseUNDGTotalsBySubstance(Factory, data.Whs1.PK, substance.PK);
			AssertEquals(1, resultAfterTransfer.Count);
			AssertQueryResult(resultAfterTransfer.ElementAt(0), 20, "KG", 30, "M3");
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

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, whs1, "R1", data.Part1, 10m, whs1.DefaultLocation, "");
			Factory.Save();

			var whs1ResultBeforeTransferEntered = WhsWarehouseUNDGTotalsHelper.LoadWhsWarehouseUNDGTotalsBySubstance(Factory, whs1.PK, substance.PK);
			AssertEquals(1, whs1ResultBeforeTransferEntered.Count);
			AssertQueryResult(whs1ResultBeforeTransferEntered.ElementAt(0), 20, "KG", 30, "M3");

			var whs2ResultBeforeTransferEntered = WhsWarehouseUNDGTotalsHelper.LoadWhsWarehouseUNDGTotalsBySubstance(Factory, whs2.PK, substance.PK);
			AssertEquals(0, whs2ResultBeforeTransferEntered.Count);

			// Act
			var transfer = Helper.CreateWhsTransfer(data.Org1, whs1, "TR1", Notify);
			transfer.WD_DocketSubType = TransferType.Codes.InterWhsSource;
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, "A", whs2.PK, "B");
			transferLine.RunPreSaveValidation();
			transferLine.PickedTime = ZDateTimeOffset.Now;
			Factory.Save();

			// Assert
			var whs1ResultAfterTransfer = WhsWarehouseUNDGTotalsHelper.LoadWhsWarehouseUNDGTotalsBySubstance(Factory, whs1.PK, substance.PK);
			AssertEquals(1, whs1ResultAfterTransfer.Count);
			AssertQueryResult(whs1ResultBeforeTransferEntered.ElementAt(0), 20, "KG", 30, "M3");

			var whs2ResultAfterTransfer = WhsWarehouseUNDGTotalsHelper.LoadWhsWarehouseUNDGTotalsBySubstance(Factory, whs2.PK, substance.PK);
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

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, whs1, "R1", data.Part1, 10m, whs1.DefaultLocation, "");
			Factory.Save();

			var whs1ResultBeforeTransfer = WhsWarehouseUNDGTotalsHelper.LoadWhsWarehouseUNDGTotalsBySubstance(Factory, whs1.PK, substance.PK);
			AssertEquals(1, whs1ResultBeforeTransfer.Count);
			AssertQueryResult(whs1ResultBeforeTransfer.ElementAt(0), 20, "KG", 30, "M3");

			var whs2ResultBeforeTransfer = WhsWarehouseUNDGTotalsHelper.LoadWhsWarehouseUNDGTotalsBySubstance(Factory, whs2.PK, substance.PK);
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

			var whs1ResultAfterTransfer = WhsWarehouseUNDGTotalsHelper.LoadWhsWarehouseUNDGTotalsBySubstance(Factory, whs1.PK, substance.PK);
			AssertEquals(1, whs1ResultAfterTransfer.Count);
			AssertQueryResult(whs1ResultAfterTransfer.ElementAt(0), 20, "KG", 30, "M3");

			var whs2ResultAfterTransfer = WhsWarehouseUNDGTotalsHelper.LoadWhsWarehouseUNDGTotalsBySubstance(Factory, whs2.PK, substance.PK);
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

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, whs1, "R1", data.Part1, 10m, whs1.DefaultLocation, "");
			Factory.Save();

			var whs1ResultBeforeTransfer = WhsWarehouseUNDGTotalsHelper.LoadWhsWarehouseUNDGTotalsBySubstance(Factory, whs1.PK, substance.PK);
			AssertEquals(1, whs1ResultBeforeTransfer.Count);
			AssertQueryResult(whs1ResultBeforeTransfer.ElementAt(0), 20, "KG", 30, "M3");

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

			var whs1ResultAfterTransfer = WhsWarehouseUNDGTotalsHelper.LoadWhsWarehouseUNDGTotalsBySubstance(Factory, whs1.PK, substance.PK);
			AssertEquals(0, whs1ResultAfterTransfer.Count);

			var whs2ResultAfterTransfer = WhsWarehouseUNDGTotalsHelper.LoadWhsWarehouseUNDGTotalsBySubstance(Factory, whs2.PK, substance.PK);
			AssertEquals(1, whs2ResultAfterTransfer.Count);
			AssertQueryResult(whs2ResultAfterTransfer.ElementAt(0), 20, "KG", 30, "M3");
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

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, whs2, "R1", data.Part1, 10m, whs2.DefaultLocation, "");
			Factory.Save();

			var whs1ResultBeforeTransfer = WhsWarehouseUNDGTotalsHelper.LoadWhsWarehouseUNDGTotalsBySubstance(Factory, whs1.PK, substance.PK);
			AssertEquals(0, whs1ResultBeforeTransfer.Count);

			var whs2ResultBeforeTransfer = WhsWarehouseUNDGTotalsHelper.LoadWhsWarehouseUNDGTotalsBySubstance(Factory, whs2.PK, substance.PK);
			AssertEquals(1, whs2ResultBeforeTransfer.Count);
			AssertQueryResult(whs2ResultBeforeTransfer.ElementAt(0), 20, "KG", 30, "M3");

			// Act
			var transfer = Helper.CreateWhsTransfer(data.Org1, whs1, "TR1", Notify);
			transfer.WD_DocketSubType = TransferType.Codes.InterWhsDest;
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, "B", whs2.PK, "A");
			transferLine.RunPreSaveValidation();
			Factory.Save();

			// Assert
			AssertEquals(DocketLineStatus.Codes.Entered, transferLine.WE_DocketLineStatus);

			var whs1ResultAfterTransfer = WhsWarehouseUNDGTotalsHelper.LoadWhsWarehouseUNDGTotalsBySubstance(Factory, whs1.PK, substance.PK);
			AssertEquals(0, whs1ResultAfterTransfer.Count);

			var whs2ResultAfterTransfer = WhsWarehouseUNDGTotalsHelper.LoadWhsWarehouseUNDGTotalsBySubstance(Factory, whs2.PK, substance.PK);
			AssertEquals(1, whs2ResultAfterTransfer.Count);
			AssertQueryResult(whs2ResultAfterTransfer.ElementAt(0), 20, "KG", 30, "M3");
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

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, whs2, "R1", data.Part1, 10m, whs2.DefaultLocation, "");
			Factory.Save();

			var whs1ResultBeforeTransfer = WhsWarehouseUNDGTotalsHelper.LoadWhsWarehouseUNDGTotalsBySubstance(Factory, whs1.PK, substance.PK);
			AssertEquals(0, whs1ResultBeforeTransfer.Count);

			var whs2ResultBeforeTransfer = WhsWarehouseUNDGTotalsHelper.LoadWhsWarehouseUNDGTotalsBySubstance(Factory, whs2.PK, substance.PK);
			AssertEquals(1, whs2ResultBeforeTransfer.Count);
			AssertQueryResult(whs2ResultBeforeTransfer.ElementAt(0), 20, "KG", 30, "M3");

			// Act
			var transfer = Helper.CreateWhsTransfer(data.Org1, whs1, "TR1", Notify);
			transfer.WD_DocketSubType = TransferType.Codes.InterWhsDest;
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, "B", whs2.PK, "A");
			transferLine.RunPreSaveValidation();
			transferLine.PickedTime = ZDateTimeOffset.Now;
			Factory.Save();

			// Assert
			AssertEquals(DocketLineStatus.Codes.HeldForTransfer, transferLine.WE_DocketLineStatus);

			var whs1ResultAfterTransfer = WhsWarehouseUNDGTotalsHelper.LoadWhsWarehouseUNDGTotalsBySubstance(Factory, whs1.PK, substance.PK);
			AssertEquals(0, whs1ResultAfterTransfer.Count);

			var whs2ResultAfterTransfer = WhsWarehouseUNDGTotalsHelper.LoadWhsWarehouseUNDGTotalsBySubstance(Factory, whs2.PK, substance.PK);
			AssertEquals(1, whs2ResultAfterTransfer.Count);
			AssertQueryResult(whs2ResultAfterTransfer.ElementAt(0), 20, "KG", 30, "M3");
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

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, whs2, "R1", data.Part1, 10m, whs2.DefaultLocation, "");
			Factory.Save();

			var whs1ResultBeforeTransfer = WhsWarehouseUNDGTotalsHelper.LoadWhsWarehouseUNDGTotalsBySubstance(Factory, whs1.PK, substance.PK);
			AssertEquals(0, whs1ResultBeforeTransfer.Count);

			var whs2ResultBeforeTransfer = WhsWarehouseUNDGTotalsHelper.LoadWhsWarehouseUNDGTotalsBySubstance(Factory, whs2.PK, substance.PK);
			AssertEquals(1, whs2ResultBeforeTransfer.Count);
			AssertQueryResult(whs2ResultBeforeTransfer.ElementAt(0), 20, "KG", 30, "M3");

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

			var whs1ResultAfterTransfer = WhsWarehouseUNDGTotalsHelper.LoadWhsWarehouseUNDGTotalsBySubstance(Factory, whs1.PK, substance.PK);
			AssertEquals(1, whs1ResultAfterTransfer.Count);
			AssertQueryResult(whs1ResultAfterTransfer.ElementAt(0), 20, "KG", 30, "M3");

			var whs2ResultAfterTransfer = WhsWarehouseUNDGTotalsHelper.LoadWhsWarehouseUNDGTotalsBySubstance(Factory, whs2.PK, substance.PK);
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

			var adjustment = Helper.CreateWhsAdjustment(data.Org1, data.Whs1);
			var adjustmentLine = Helper.CreateWhsAdjustmentLine(adjustment, data.Part1, 2m, data.Whs1.DefaultLocation);
			Factory.Save();

			// Act
			AssertEquals(DocketStatus.Codes.Entered, adjustment.WD_DocketStatus);
			AssertEquals(InventoryStatus.Codes.Available, adjustmentLine.WE_CurrentInventoryStatus);
			var results = WhsWarehouseUNDGTotalsHelper.LoadWhsWarehouseUNDGTotalsBySubstance(Factory, data.Whs1.PK, substance.PK);

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
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var resultBeforeAdjustment = WhsWarehouseUNDGTotalsHelper.LoadWhsWarehouseUNDGTotalsBySubstance(Factory, data.Whs1.PK, substance.PK);
			AssertEquals(1, resultBeforeAdjustment.Count);
			AssertQueryResult(resultBeforeAdjustment.ElementAt(0), 20, "KG", 30, "M3");

			// Act
			var adjustment = Helper.CreateWhsAdjustment(data.Org1, data.Whs1, "111", Notify);
			Helper.CreateWhsAdjustmentLine(adjustment, data.Part1, 2m, data.Whs1.DefaultLocation);
			adjustment.FinaliseDocket();
			Factory.Save();

			// Assert
			var resultAfterAdjustment = WhsWarehouseUNDGTotalsHelper.LoadWhsWarehouseUNDGTotalsBySubstance(Factory, data.Whs1.PK, substance.PK);
			AssertEquals(1, resultAfterAdjustment.Count);
			AssertQueryResult(resultAfterAdjustment.ElementAt(0), 24, "KG", 36, "M3");
		}

		public void TestUNDGStock_AdjustmentOut_Entered()
		{
			// Arrange
			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WW_IsDangerousGoodsManagementEnabled = true;
			data.Whs1.WW_DGThresholdPercentage = 80;
			var substance = UNDGSubstanceLoader.LoadSubstances(Factory, "0004", "a", "IMO").FirstOrDefault();
			Helper.CreateUNDGDataItem(data.Part1, substance, 2m, "KG", 3m, "M3", substance.DG_Class);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var resultBeforeAdjustment = WhsWarehouseUNDGTotalsHelper.LoadWhsWarehouseUNDGTotalsBySubstance(Factory, data.Whs1.PK, substance.PK);
			AssertEquals(1, resultBeforeAdjustment.Count);
			AssertQueryResult(resultBeforeAdjustment.ElementAt(0), 20, "KG", 30, "M3");

			// Act
			var adjustment = Helper.CreateWhsAdjustment(data.Org1, data.Whs1, "111", Notify);
			var adjustmentLine = Helper.CreateWhsAdjustmentLine(adjustment, data.Part1, -10m, data.Whs1.DefaultLocation);
			adjustment.RunPreSaveValidation();
			Factory.Save();

			AssertEquals(DocketStatus.Codes.Entered, adjustment.WD_DocketStatus);
			AssertEquals(InventoryStatus.Codes.Available, adjustmentLine.WE_CurrentInventoryStatus);
			var resultAfterAdjustment = WhsWarehouseUNDGTotalsHelper.LoadWhsWarehouseUNDGTotalsBySubstance(Factory, data.Whs1.PK, substance.PK);

			// Assert
			AssertEquals(1, resultAfterAdjustment.Count);
			AssertQueryResult(resultBeforeAdjustment.ElementAt(0), 20, "KG", 30, "M3");
		}

		public void TestUNDGStock_AdjustmentOut_Finalised()
		{
			// Arrange
			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WW_IsDangerousGoodsManagementEnabled = true;
			data.Whs1.WW_DGThresholdPercentage = 80;
			var substance = UNDGSubstanceLoader.LoadSubstances(Factory, "0004", "a", "IMO").FirstOrDefault();
			Helper.CreateUNDGDataItem(data.Part1, substance, 2m, "KG", 3m, "M3", substance.DG_Class);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var resultBeforeAdjustment = WhsWarehouseUNDGTotalsHelper.LoadWhsWarehouseUNDGTotalsBySubstance(Factory, data.Whs1.PK, substance.PK);
			AssertEquals(1, resultBeforeAdjustment.Count);
			AssertQueryResult(resultBeforeAdjustment.ElementAt(0), 20, "KG", 30, "M3");

			// Act
			var adjustment = Helper.CreateWhsAdjustment(data.Org1, data.Whs1, "111", Notify);
			Helper.CreateWhsAdjustmentLine(adjustment, data.Part1, -10m, data.Whs1.DefaultLocation);
			adjustment.FinaliseDocket();
			Factory.Save();

			// Assert
			var resultAfterAdjustment = WhsWarehouseUNDGTotalsHelper.LoadWhsWarehouseUNDGTotalsBySubstance(Factory, data.Whs1.PK, substance.PK);
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
			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 5m);
			Factory.Save();

			// Act
			AssertEquals(DocketStatus.Codes.Entered, order.WD_DocketStatus);
			AssertEquals(string.Empty, orderLine.WE_CurrentInventoryStatus);
			var results = WhsWarehouseUNDGTotalsHelper.LoadWhsWarehouseUNDGTotalsBySubstance(Factory, data.Whs1.PK, substance.PK);

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
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			Helper.CreateWhsOrderLine(order, data.Part1, 5m);

			var pick = Helper.CreatePickNew(order);
			Factory.Save();

			// Act
			var results = WhsWarehouseUNDGTotalsHelper.LoadWhsWarehouseUNDGTotalsBySubstance(Factory, data.Whs1.PK, substance.PK);

			// Assert
			AssertEquals(1, results.Count);
			AssertQueryResult(results.ElementAt(0), 20, "KG", 30, "M3");
		}

		public void TestUNDGStock_Order_Finalised()
		{
			// Arrange
			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WW_IsDangerousGoodsManagementEnabled = true;
			data.Whs1.WW_DGThresholdPercentage = 80;
			var substance = UNDGSubstanceLoader.LoadSubstances(Factory, "0004", "a", "IMO").FirstOrDefault();
			Helper.CreateUNDGDataItem(data.Part1, substance, 2m, "KG", 3m, "M3", substance.DG_Class);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 20m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			Helper.CreateWhsOrderLine(order, data.Part1, 20m);

			var pick = Helper.CreatePickNew(order);
			pick.FinaliseAllOrders();
			pick.FinalisePick();
			Factory.Save();

			// Act
			var results = WhsWarehouseUNDGTotalsHelper.LoadWhsWarehouseUNDGTotalsBySubstance(Factory, data.Whs1.PK, substance.PK);

			// Assert
			AssertEquals(0, results.Count);
		}

		#endregion

		#region Implementation

		void AssertQueryResult(WhsWarehouseUNDGWeightAndVolume result, decimal totalWeight, ZString totalWeightUQ, decimal totalVolume, ZString totalVolumeUQ)
		{
			AssertEquals("Total weight:", totalWeight, result.TotalWeight);
			AssertEquals("Total weight UQ:", totalWeightUQ, result.TotalWeightUQ);
			AssertEquals("Total volume:", totalVolume, result.TotalVolume);
			AssertEquals("Total volume UQ:", totalVolumeUQ, result.TotalVolumeUQ);
		}

		#endregion
	}
}
