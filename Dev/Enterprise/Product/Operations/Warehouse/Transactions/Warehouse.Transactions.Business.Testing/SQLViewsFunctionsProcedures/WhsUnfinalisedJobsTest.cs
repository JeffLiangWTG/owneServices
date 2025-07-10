using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Environment.CodeLists;
using Enterprise.Warehouse.Integration.CodeLists;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.Schema;

// Extracted from Warehouse\Transactions\Warehouse.Transactions.Business.Testing\SQLViewsFunctionsProcedures\WhsProductCategoryAndChildCategoriesTest.cs - refer to the original file for checking history
namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	internal class WhsUnfinalisedJobsTest : WhsTestCaseWithFactory
	{
		#region TestView

		public void TestView()
		{
			Assert("incomplete test", true);
		}

		#endregion

		#region TestWhereClause

		public void TestWhereClause()
		{
			var whs1 = Helper.CreateWarehouse("1", "A", 2, 2);
			var whs2 = Helper.CreateWarehouse("2", "A", 2, 2);
			var whs = new WhsWarehouseCollection(Factory);
			whs.Add(whs1);
			whs.Add(whs2);

			var client1 = Helper.CreateClient("TESFIR", "Test First Client");
			var client2 = Helper.CreateClient("TESSEC", "Test Second Client");
			var clients = new OrgHeaderCollection(Factory);
			clients.Add(client1);
			clients.Add(client2);

			SetupData(whs, clients);

			LoadAndAssertViewResults(whs1, client1, 4);
			LoadAndAssertViewResults(whs1, client2, 0);
			LoadAndAssertViewResults(whs2, client1, 1);
			LoadAndAssertViewResults(whs2, client2, 1);
		}

		void LoadAndAssertViewResults(WhsWarehouse whs, OrgHeader client, int expectedNoOfResultsLoaded)
		{
			var result = LoadView(whs, client);

			AssertEquals(expectedNoOfResultsLoaded, result.Count);
			foreach (DynamicBusinessObject bizO in result)
			{
				AssertEquals(client.PK, bizO["ClientPK"]);
				AssertEquals(whs.PK, bizO["WarehousePK"]);
			}
		}

		DynamicBusinessObjectCollection LoadView(WhsWarehouse whs, OrgHeader client)
		{
			var result = new DynamicBusinessObjectCollection(Factory);

			var sql = @"
SELECT
	*
FROM
	dbo.WhsUnfinalisedJobsReport
WHERE
	WarehousePK = @WarehousePK AND
	ClientPK = @ClientPK
ORDER BY
	reference";

			var parameters = new ZSqlParameterCollection();
			parameters.Add("@ClientPK", client.PK, OrgHeaderSchema.PK);
			parameters.Add("@WarehousePK", whs.PK, WhsWarehouseSchema.PK);

			result.Load(sql, parameters);
			return result;
		}

		#region SetupData

		void SetupData(WhsWarehouseCollection whs, OrgHeaderCollection clients)
		{
			var part1 = Helper.CreateProduct(clients[0], "1");
			Helper.CreateProduct(clients[1], "2");

			var receive1 = SetupReceive(clients[0], whs[0], "I1", part1, 111);
			SetupReceive(clients[0], whs[0], "I2", part1, 121);
			SetupReceive(clients[0], whs[0], "I3", part1, 211);
			SetupReceive(clients[1], whs[1], "I4", part1, 221);
			var receive5 = SetupReceive(clients[0], whs[1], "I5", part1, 311);

			var order1 = SetupOrder(clients[0], whs[0], "O1", part1, 101);
			var order2 = SetupOrder(clients[1], whs[1], "O2", part1, 20); // client2
			SetupOrder(clients[0], whs[1], "O3", part1, 30);
			SetupOrder(clients[0], whs[0], "O4", part1, 40);
			SetupOrder(clients[0], whs[0], "O5", part1, 50);

			Factory.Save();

			receive1.AllocateLocationsWithMock();
			receive1.FinaliseDocket();
			Helper.CreatePickNew(order1);
			order1.WD_FinalisedDate = ZDateTimeOffset.Now;

			CancelDocket(receive5);
			CancelDocket(order2);

			Factory.Save();
		}

		void CancelDocket(WhsDocket docket)
		{
			docket.CancelReactivateDocket();
			AssertEquals(true, docket.IsCancelled);
		}

		WhsOrder SetupOrder(OrgHeader client, WhsWarehouse warehouse, ZString reference,
			OrgSupplierPart warehouseProduct, ZDecimal units)
		{
			var order = Helper.CreateWhsOrder(client, warehouse, reference, Notify);
			Helper.CreateWhsOrderLine(order, warehouseProduct, units);
			return order;
		}

		WhsReceive SetupReceive(OrgHeader client, WhsWarehouse warehouse, ZString reference,
			OrgSupplierPart warehouseProduct, ZDecimal units)
		{
			var receive = Helper.CreateWhsReceive(client, warehouse, reference, Notify);
			Helper.CreateWhsReceiveInventoryLine(receive, warehouseProduct, units);
			return receive;
		}

		#endregion

		#endregion

		#region TestDocketSubType

		public void TestDocketSubType()
		{
			var whs = Helper.CreateWarehouse("1", "A", 2, 2);
			var client = Helper.CreateClient("TESFIR", "Test First Client");
			SetupDataForTestingOrderSubType(whs, client);

			// receives
			LoadAndAssertViewResultsWithDocketTypeSubType(whs, client, DocketType.Codes.Receive,
				ReceiveType.Codes.Receipt, 2);
			LoadAndAssertViewResultsWithDocketTypeSubType(whs, client, DocketType.Codes.Receive,
				ReceiveType.Codes.Customs, 1);
			LoadAndAssertViewResultsWithDocketTypeSubType(whs, client, DocketType.Codes.Receive,
				ReceiveType.Codes.Returns, 0);

			// orders
			LoadAndAssertViewResultsWithDocketTypeSubType(whs, client, DocketType.Codes.Order, OrderType.Codes.Order,
				2);
			LoadAndAssertViewResultsWithDocketTypeSubType(whs, client, DocketType.Codes.Order,
				OrderType.Codes.RepeatOrder, 1);
			LoadAndAssertViewResultsWithDocketTypeSubType(whs, client, DocketType.Codes.Order,
				OrderType.Codes.BackOrder, 0);
			LoadAndAssertViewResultsWithDocketTypeSubType(whs, client, DocketType.Codes.Order, OrderType.Codes.Customs,
				0);
		}

		public void TestDocketSubType_Transfers()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var dockDoorLocationType = Helper.CreateLocationType("XYZ", "Test",
				false, 0, LocationClasses.Codes.DDL);
			var nonDockDoorLocation = data.Whs1.FindLocation("A-1");
			var dockDoorLocation = data.Whs1.FindLocation("A-2");
			dockDoorLocation.WLV_WLT_LocationType = dockDoorLocationType.PK; // dock door location

			var receiveWithDockDoor = Helper.CreateWhsReceive(data.Org1.PK, data.Whs1.PK, "R1");
			var receiveLineWithDDL =
				Helper.CreateWhsReceiveLine(receiveWithDockDoor, data.Part1, 10m, dockDoorLocation, "12345");

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 100m, nonDockDoorLocation,
				"1");
			Factory.Save();

			var putawayTransfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "T1");
			putawayTransfer.WD_IsPutawayTransfer = true;
			putawayTransfer.CreateDocketLineFromInventory(receiveLineWithDDL.Inventory[0]);
			putawayTransfer.RunPreSaveValidation(); // to commit inventory

			var nonPutawayTransfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "T2");
			Helper.CreateWhsTransferLine(nonPutawayTransfer, data.Part1, 10m, "A-1", "1", "", "");
			nonPutawayTransfer.RunPreSaveValidation(); // to commit inventory
			AssertEquals("Precondition", false, nonPutawayTransfer.WD_IsPutawayTransfer);
			Factory.Save();

			var result = LoadView(data.Whs1, data.Org1);
			AssertEquals("Precondition", 3, result.Count);
			AssertDocketSubType(
				result.Single(b =>
					b["TranType"].ToString() == DocketType.Codes.Transfer && b["SubType"].ToString() == "PUT"),
				putawayTransfer.WD_DocketID, "TFRTFR");
			AssertDocketSubType(
				result.Single(b =>
					b["TranType"].ToString() == DocketType.Codes.Transfer &&
					b["SubType"].ToString() == TransferType.Codes.Internal), nonPutawayTransfer.WD_DocketID, "TFRTFR");
			AssertDocketSubType(result.Single(b => b["TranType"].ToString() == DocketType.Codes.Receive),
				receiveWithDockDoor.WD_DocketID, "INWREC");
		}

		void AssertDocketSubType(DynamicBusinessObject bizO, string expectedReference, string expectedDocketTypeSubType)
		{
			AssertEquals(expectedReference, bizO["DocketID"].ToString());
			AssertEquals(expectedDocketTypeSubType, bizO["DocketTypeSubType"].ToString());
		}

		void SetupDataForTestingOrderSubType(WhsWarehouse whs1, OrgHeader client1)
		{
			var part1 = Helper.CreateProduct(client1, "1");

			var receive1 = SetupReceive(client1, whs1, "I1", part1, 111);
			var receive2 = SetupReceive(client1, whs1, "I2", part1, 121);
			var receive3 = SetupReceive(client1, whs1, "I3", part1, 211);
			var receive4 = SetupReceive(client1, whs1, "I4", part1, 221);
			var receive5 = SetupReceive(client1, whs1, "I5", part1, 311);

			receive1.WD_DocketSubType = ReceiveType.Codes.Receipt; // finalised
			receive2.WD_DocketSubType = ReceiveType.Codes.Receipt;
			receive3.WD_DocketSubType = ReceiveType.Codes.Customs;
			receive4.WD_DocketSubType = ReceiveType.Codes.Receipt;
			receive5.WD_DocketSubType = ReceiveType.Codes.Receipt; // cancelled

			var order1 = SetupOrder(client1, whs1, "O1", part1, 101);
			var order2 = SetupOrder(client1, whs1, "O2", part1, 20);
			var order3 = SetupOrder(client1, whs1, "O3", part1, 30);
			var order4 = SetupOrder(client1, whs1, "O4", part1, 40);
			var order5 = SetupOrder(client1, whs1, "O5", part1, 50);

			order1.WD_DocketSubType = OrderType.Codes.Order; // finalised
			order2.WD_DocketSubType = OrderType.Codes.BackOrder; // cancelled
			order3.WD_DocketSubType = OrderType.Codes.Order;
			order4.WD_DocketSubType = OrderType.Codes.RepeatOrder;
			order5.WD_DocketSubType = OrderType.Codes.Order;

			Factory.Save();

			receive1.AllocateLocationsWithMock();
			receive1.FinaliseDocket();
			Helper.CreatePickNew(order1);
			order1.WD_FinalisedDate = ZDateTimeOffset.Now;

			CancelDocket(receive5);
			CancelDocket(order2);

			Factory.Save();
		}

		void LoadAndAssertViewResultsWithDocketTypeSubType(WhsWarehouse whs, OrgHeader client, string docketType,
			string docketSubType, int expectedNoOfResultsLoaded)
		{
			var result = LoadView(whs, client);

			var loadedCount = 0;
			foreach (DynamicBusinessObject bizO in result)
			{
				AssertEquals(client.PK, bizO["ClientPK"]);
				AssertEquals(whs.PK, bizO["WarehousePK"]);

				if (bizO["TranType"].ToString() == docketType && bizO["SubType"].ToString() == docketSubType)
				{
					loadedCount++;
				}
			}

			AssertEquals(expectedNoOfResultsLoaded, loadedCount);
		}

		#endregion

		#region TestView_LoadingStatus

		public void TestView_LoadedStatus()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var carrierServicelevel = data.Org1.MiscServ.CarrierServiceLevels.AddNew();
			carrierServicelevel.PL_Code = "RD";
			carrierServicelevel.PL_CarrierServiceLevelDescription = "Road";
			var truck = Helper.CreateEquipment("T001", 1m, Core.Constants.Weight.Kilograms, 1m, Core.Constants.Volume.CubicMetres);
			var load = Helper.CreateWhsLoad(data.Org1, data.Whs1.DefaultOutboundDockDoorLocation, transportUnit: truck, startTime: DateTimeOffset.Now);

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 5m);
			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O2", data.Part1, 5m);
			Factory.Save();

			Helper.CreatePickNew(order1, order2);

			var pickLine1 = order1.Lines.Single().PickLines.Single();
			pickLine1.WZ_PickedDateTime = ZDateTimeOffset.Now;

			var pickLine2 = order1.Lines.Single().PickLines.Single();
			pickLine2.WZ_PickedDateTime = ZDateTimeOffset.Now;
			Factory.Save();

			var package1 = order1.PackageJob.Packages.AddNew("CTN");
			package1.Pack(order1.Lines[0].ReleaseLines[0], 5m);
			var loadPkgPackagePivot = Helper.CreateLoadPkgPackagePivot(package1.PK, load);
			loadPkgPackagePivot.WLP_LoadedTime = ZDateTimeOffset.Now;
			loadPkgPackagePivot.WLP_GS_NKLoadingUser = "E";

			var package2 = order2.PackageJob.Packages.AddNew("CTN");
			package2.Pack(order2.Lines[0].ReleaseLines[0], 5m);
			Helper.CreateLoadPkgPackagePivot(package2.PK, load);
			Factory.Save();

			var results = LoadView(data.Whs1, data.Org1);
			AssertEquals("Should return 2 records.", 3, results.Count); // 3rd is receive
			var orderRecord1 = results[0];
			AssertEquals("Order1 status should LOA.", WhsOrderStatus.Codes.Loaded, orderRecord1["Status"]);

			var orderRecord2 = results[1];
			AssertEquals("Order1 status should ATP.", DocketStatus.Codes.AttachedToPick, orderRecord2["Status"]);
		}

		#endregion

		#region TestAdHocJobs

		public void TestAdHocJobs()
		{
			var whs1 = Helper.CreateWarehouse("Whs1", "A", 2, 2);
			var whs2 = Helper.CreateWarehouse("Whs2", "A", 2, 2);

			var client1 = Helper.CreateClient("Client1", "Client1");
			var client2 = Helper.CreateClient("Client2", "Client2");

			var today = ZDateTime.Today;

			var adHocJob1 = Helper.CreateWhsAdHocServiceJob(whs1, client1, today, "ADH1", finalised: false);
			var adHocJob2 = Helper.CreateWhsAdHocServiceJob(whs1, client1, today, "ADH2", finalised: false);
			var adHocJob3 = Helper.CreateWhsAdHocServiceJob(whs1, client1, today, "ADH3", finalised: true);
			var adHocJob4 = Helper.CreateWhsAdHocServiceJob(whs2, client1, today, "ADH4", finalised: false);
			var adHocJob5 = Helper.CreateWhsAdHocServiceJob(whs1, client2, today, "ADH5", finalised: false);

			Factory.Save();

			LoadAndAssertViewResultsWithAdHocJob(whs1, client1, adHocJob1, adHocJob2);
			LoadAndAssertViewResultsWithAdHocJob(whs2, client1, adHocJob4);
			LoadAndAssertViewResultsWithAdHocJob(whs1, client2, adHocJob5);
		}

		public void TestAdHocJobs_DifferentWarehouses()
		{
			var whs1 = CreateTestWarehouse("whs1", true, false, WarehouseTypes.Codes.Product);
			var whs2 = CreateTestWarehouse("whs2", false, false, WarehouseTypes.Codes.Product);
			var whs6 = CreateTestWarehouse("whs3", true, true, WarehouseTypes.Codes.Product);
			var whs3 = CreateTestWarehouse("whs4", true, false, WarehouseTypes.Codes.ContainerYard);
			var whs4 = CreateTestWarehouse("whs5", true, false, WarehouseTypes.Codes.FreeTradeZone);
			var whs5 = CreateTestWarehouse("whs6", true, false, WarehouseTypes.Codes.Transit);

			var client1 = Helper.CreateClient("Client1", "Client1");

			var today = ZDateTime.Today;

			var adHocJob1 = Helper.CreateWhsAdHocServiceJob(whs1, client1, today, "ADH1", finalised: false);
			var adHocJob2 = Helper.CreateWhsAdHocServiceJob(whs2, client1, today, "ADH2", finalised: false);
			var adHocJob3 = Helper.CreateWhsAdHocServiceJob(whs3, client1, today, "ADH3", finalised: false);
			var adHocJob4 = Helper.CreateWhsAdHocServiceJob(whs4, client1, today, "ADH4", finalised: false);
			var adHocJob5 = Helper.CreateWhsAdHocServiceJob(whs5, client1, today, "ADH5", finalised: false);
			var adHocJob6 = Helper.CreateWhsAdHocServiceJob(whs6, client1, today, "ADH6", finalised: false);

			Factory.Save();

			LoadAndAssertViewResultsWithAdHocJob(whs1, client1, adHocJob1);
			LoadAndAssertViewResultsWithAdHocJob(whs2, client1, adHocJob2);
			LoadAndAssertViewResultsWithAdHocJob(whs3, client1, adHocJob3);
			LoadAndAssertViewResultsWithAdHocJob(whs4, client1, adHocJob4);
			LoadAndAssertViewResultsWithAdHocJob(whs5, client1, adHocJob5);
			LoadAndAssertViewResultsWithAdHocJob(whs6, client1, adHocJob6);
		}

		#endregion

		#region TestVASOrder

		public void TestVASOrderFilter()
		{
			var whs1 = Helper.CreateWarehouse("Whs1", "A", 2, 2);
			var serviceArea1 = Helper.CreateServiceAreaForVASOrder(whs1);
			var client1 = Helper.CreateClient("Client1", "Client1");
			var product1 = Helper.CreateProduct("Product1", client1);
			var whs2 = Helper.CreateWarehouse("Whs2", "A", 2, 2);
			var serviceArea2 = Helper.CreateServiceAreaForVASOrder(whs2);
			var client2 = Helper.CreateClient("Client2", "Client2");
			var product2 = Helper.CreateProduct("Product2", client2);
			Helper.CreateWhsReceiveWithInventory(client1, whs1, "R1", product1, 10m);
			Helper.CreateWhsReceiveWithInventory(client1, whs2, "R2", product1, 10m);
			Helper.CreateWhsReceiveWithInventory(client2, whs1, "R3", product2, 10m);
			Factory.Save();

			Helper.CreateWhsVASOrderWithLine(serviceArea1, client1, product1, 1m, createTransferAndFinalise: true);
			var vasOrder1 = Helper.CreateWhsVASOrderWithLine(serviceArea1, client1, product1, 1m,
				createTransferAndFinalise: false);
			var vasOrder2 = Helper.CreateWhsVASOrderWithLine(serviceArea1, client1, product1, 1m,
				createTransferAndFinalise: false);
			var vasOrder3 = Helper.CreateWhsVASOrderWithLine(serviceArea2, client1, product1, 1m,
				createTransferAndFinalise: false);
			var vasOrder4 = Helper.CreateWhsVASOrderWithLine(serviceArea1, client2, product2, 1m,
				createTransferAndFinalise: false);

			LoadAndAssertViewResultsWithVASOrderJob(whs1, client1, vasOrder1, vasOrder2);
			LoadAndAssertViewResultsWithVASOrderJob(whs2, client1, vasOrder3);
			LoadAndAssertViewResultsWithVASOrderJob(whs1, client2, vasOrder4);
			LoadAndAssertViewNoResults(whs2, client2);
		}

		public void TestWhsUnfinalisedJobs_VASOrder_WarehouseTypes_FreeTradeZone()
		{
			TestWhsUnfinalisedJobs_VASOrder_WarehouseTypesCore(WarehouseTypes.Codes.FreeTradeZone, true);
		}

		public void TestWhsUnfinalisedJobs_VASOrder_WarehouseTypes_Product()
		{
			TestWhsUnfinalisedJobs_VASOrder_WarehouseTypesCore(WarehouseTypes.Codes.Product, true);
		}

		public void TestWhsUnfinalisedJobs_VASOrder_WarehouseTypes_Transit()
		{
			TestWhsUnfinalisedJobs_VASOrder_WarehouseTypesCore(WarehouseTypes.Codes.Transit, true);
		}

		public void TestWhsUnfinalisedJobs_VASOrder_WarehouseTypes_Product_IsVirtualWarehouse()
		{
			TestWhsUnfinalisedJobs_VASOrder_WarehouseTypesCore(WarehouseTypes.Codes.Product, true,
				isVirtualWarehouse: true);
		}

		public void TestWhsUnfinalisedJobs_VASOrder_WarehouseTypes_Product_IsInActiveWarehouse()
		{
			TestWhsUnfinalisedJobs_VASOrder_WarehouseTypesCore(WarehouseTypes.Codes.Product, true,
				isActiveWarehouse: false);
		}

		void TestWhsUnfinalisedJobs_VASOrder_WarehouseTypesCore(string warehouseType, bool expectedHaveResult,
			bool isVirtualWarehouse = false, bool isActiveWarehouse = true)
		{
			var whs = Helper.CreateWarehouse("Whs");
			whs.WW_WarehouseType = warehouseType;
			whs.WW_IsVirtualWarehouse = isVirtualWarehouse;
			var client = Helper.CreateClient("Client");
			var product = Helper.CreateProduct("Product", client);
			var serviceArea = Helper.CreateServiceAreaForVASOrder(whs);
			Helper.CreateWhsReceiveWithInventory(client, whs, "R1", product, 1m);

			whs.WW_IsActive = isActiveWarehouse;
			var vasOrder =
				Helper.CreateWhsVASOrderWithLine(serviceArea, client, product, 1m, createTransferAndFinalise: false);

			if (expectedHaveResult)
			{
				LoadAndAssertViewResultsWithVASOrderJob(whs, client, vasOrder);
			}
			else
			{
				LoadAndAssertViewNoResults(whs, client);
			}
		}

		#endregion

		#region Helper

		WhsWarehouse CreateTestWarehouse(ZString name, bool isActive, bool isVirtual,
			string typeCode = WarehouseTypes.Codes.Product)
		{
			var whs = Helper.CreateWarehouse(name, "A", 2, 2);
			whs.WW_GB_RelatedCompanyBranch = GlbBranch.CurrentBranch.PK;
			whs.WW_IsActive = isActive;
			whs.WW_IsVirtualWarehouse = isVirtual;
			whs.WW_WarehouseType = typeCode;
			return whs;
		}

		void AssertJob(WhsWarehouse whs, OrgHeader client, DynamicBusinessObject bizO, string tranType,
			string reference, string docketID)
		{
			AssertEquals("Incorrect Warehouse.", whs.PK, bizO["WarehousePK"]);
			AssertEquals("Incorrect Warehouse.", whs.WW_WarehouseName, bizO["WarehouseName"]);
			AssertEquals("Incorrect Client.", client.PK, bizO["ClientPK"]);
			AssertEquals("Incorrect Client Code.", client.OH_Code, bizO["ClientCode"]);
			AssertEquals("Incorrect Client Name.", client.OH_FullName, bizO["Client"]);
			AssertEquals("Incorrct Job Type.", tranType, bizO["TranType"]);
			AssertEquals("Incorrect Subtype.", "", bizO["SubType"]);
			AssertEquals("Incorrect Reference.", reference, bizO["Reference"]);
			AssertEquals("Incorrect Docket ID.", docketID, bizO["DocketID"]);
			AssertEquals("PickPriority should be empty.", ZInt.Zero, bizO["PickPriority"]);
		}

		void LoadAndAssertViewNoResults(WhsWarehouse whs, OrgHeader client)
		{
			AssertEquals("Only unfinalised jobs should be loaded.", 0, LoadView(whs, client).Count);
		}

		void LoadAndAssertViewResultsWithAdHocJob(WhsWarehouse whs, OrgHeader client,
			params WhsAdHocServiceJob[] expectedJobs)
		{
			var result = LoadView(whs, client);

			AssertEquals("Only unfinalised AdHocJobs should be loaded.", expectedJobs.Length, result.Count);
			for (var index = 0; index < result.Count; index++)
			{
				AssertJob(whs, client, result[index], "ADH", expectedJobs[index].WSJ_CustomerReference,
					expectedJobs[index].WSJ_JobNumber);
			}
		}

		void LoadAndAssertViewResultsWithVASOrderJob(WhsWarehouse whs, OrgHeader client,
			params WhsVASOrder[] expectedJobs)
		{
			var result = LoadView(whs, client);

			AssertEquals("Only unfinalised VASOrders should be loaded.", expectedJobs.Length, result.Count);
			for (var index = 0; index < result.Count; index++)
			{
				AssertJob(whs, client, result[index], "WVO", expectedJobs[index].WVO_CustomerReferenceNo,
					expectedJobs[index].WVO_JobID);
			}
		}
	}

	#endregion
}
