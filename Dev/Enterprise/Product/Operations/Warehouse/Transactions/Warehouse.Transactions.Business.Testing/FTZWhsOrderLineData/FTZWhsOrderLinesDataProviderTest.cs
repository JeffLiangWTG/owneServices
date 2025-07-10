using System;
using System.Linq;
using CargoWise.Application;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.MasterFiles.Integration.Customs.PermitService;
using Enterprise.Warehouse.Environment.Business.Testing;
using Enterprise.Warehouse.Integration;
using Enterprise.Warehouse.Integration.CodeLists;
using Enterprise.Warehouse.Transactions.CodeLists;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	public class FTZWhsOrderLinesDataProviderTest : WhsTestCaseWithFactory
	{
		#region TestFTZWhsOrderLinesDataProviderInstantiatedByObjectFactory

		public void TestFTZWhsOrderLinesDataProviderInstantiatedByObjectFactory()
		{
			var result = ObjectFactory.Get<IFTZWhsOrderLinesDataProvider>();
			AssertEquals(typeof(FTZWhsOrderLinesDataProvider), result.GetType());
		}

		#endregion

		#region TestGetFTZWhsOrderLinesData_ImporterIsNull

		public void TestGetFTZWhsOrderLinesData_ImporterIsNull()
		{
			var ftzWhs = Helper.CreateFTZWarehouseInUS();
			var provider = new FTZWhsOrderLinesDataProvider();
			AssertExceptionThrown(typeof(ArgumentNullException), @"Value cannot be null.
Parameter name: importer", () => provider.GetFTZWhsOrderLinesData(null, ftzWhs.WarehouseAddress, "DummyOutward"));
		}

		#endregion

		#region TestGetFTZWhsOrderLinesData_ImportIsInactiveOrNotWarehouseClient

		public void TestGetFTZWhsOrderLinesData_ImportIsInactive()
		{
			var client = Helper.CreateClient("ORG");
			client.OH_IsActive = false;
			TestGetFTZWhsOrderLinesData_ImportIsInactiveOrNotWarehouseClientCore(client);
		}

		public void TestGetFTZWhsOrderLinesData_ImporterIsNotWarehouseClient()
		{
			var client = Helper.CreateClient("ORG");
			client.OH_IsWarehouseClient = false;
			TestGetFTZWhsOrderLinesData_ImportIsInactiveOrNotWarehouseClientCore(client);
		}

		void TestGetFTZWhsOrderLinesData_ImportIsInactiveOrNotWarehouseClientCore(IOrgHeader importer)
		{
			var ftzWhs = Helper.CreateFTZWarehouseInUS();
			var provider = new FTZWhsOrderLinesDataProvider();
			AssertExceptionThrown(typeof(ArgumentException), "Importer must be active and must be a Warehouse Client.",
				() => provider.GetFTZWhsOrderLinesData(importer, ftzWhs.WarehouseAddress, "DummyOutward"));
		}

		#endregion

		#region TestGetFTZWhsOrderLinesData_WarehouseAddressIsNull

		public void TestGetFTZWhsOrderLinesData_WarehouseAddressIsNull()
		{
			var client = Helper.CreateClient("ORG");
			var provider = new FTZWhsOrderLinesDataProvider();
			AssertExceptionThrown(typeof(ArgumentNullException), @"Value cannot be null.
Parameter name: warehouseAddress", () => provider.GetFTZWhsOrderLinesData(client, null, "DummyOutward"));
		}

		#endregion

		#region TestGetFTZWhsOrderLinesData_WarehouseIsInactive

		public void TestGetFTZWhsOrderLinesData_WarehouseIsInactive()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var inactiveWhs = Helper.CreateFTZWarehouseInUS();
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, inactiveWhs, "REC", data.Part1, 10m);
			var order = Helper.CreateWhsOrder(data.Org1, inactiveWhs);
			order.WD_DocketSubType = OrderType.Codes.CustomsReleaseWithPermit;
			var orderLine = CreateOrderLine(order, data.Part1);

			using (PermitServiceTestHelper.MockServiceToGetPermits())
			{
				var pick = Helper.CreatePickNew(order);
				pick.FinaliseAllOrders();
				pick.FinalisePick();
				AssertIsFinalisedPrecondition(pick);
				AssertNoExceptionThrown("Save Pick With No Exception.", Factory.Save);
			}

			inactiveWhs.WW_IsActive = false;
			Factory.Save();
			AssertEquals("Precondition: Warehouse is inactive", false, inactiveWhs.WW_IsActive);

			var provider = new FTZWhsOrderLinesDataProvider();
			AssertEquals("When Warehouse is inactive, should not find out any lines", false,
				provider.GetFTZWhsOrderLinesData(data.Org1, inactiveWhs.WarehouseAddress, "DummyOutwards").Any());
		}

		#endregion

		#region TestGetFTZWhsOrderLinesData_NonFTZWarehouse

		public void TestGetFTZWhsOrderLinesData_NonFTZWarehouse()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var nonFTZWhs = data.Whs1;
			Helper.EnableWarehouseForBond(nonFTZWhs, true);

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, nonFTZWhs, "REC", data.Part1, 10m);
			var order = Helper.CreateWhsOrder(data.Org1, nonFTZWhs);
			order.WD_DocketSubType = OrderType.Codes.Customs;
			var orderLine = CreateOrderLine(order, data.Part1);
			var pick = Helper.CreatePickNew(order);
			pick.FinaliseAllOrders();
			pick.FinalisePick();
			Factory.Save();

			AssertIsFinalisedPrecondition(pick);
			AssertEquals("Precondition: WarehouseType is not FTZ", false,
				nonFTZWhs.WW_WarehouseType == WarehouseTypes.Codes.FreeTradeZone);

			var provider = new FTZWhsOrderLinesDataProvider();
			AssertEquals("When WarehouseType is not FTZ, should not find out any lines", false,
				provider.GetFTZWhsOrderLinesData(data.Org1, nonFTZWhs.WarehouseAddress, "DummyOutwards").Any());
		}

		#endregion

		#region TestGetFTZWhsOrderLinesData_OrderWithoutPick

		public void TestGetFTZWhsOrderLinesData_OrderWithoutPick()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var ftzWhs = Helper.CreateFTZWarehouseInUS();
			var order = Helper.CreateWhsOrder(data.Org1, ftzWhs);
			order.WD_DocketSubType = OrderType.Codes.CustomsReleaseWithPermit;
			var orderLine = CreateOrderLine(order, data.Part1);
			Factory.Save();

			AssertNull("Precondition: Order has no Pick", order.Pick);

			var provider = new FTZWhsOrderLinesDataProvider();
			AssertEquals("When Order has no Pick, should not find out any lines", false,
				provider.GetFTZWhsOrderLinesData(data.Org1, ftzWhs.WarehouseAddress, "DummyOutwards").Any());
		}

		#endregion

		#region TestGetFTZWhsOrderLinesData_PickIsNotFinalised

		public void TestGetFTZWhsOrderLinesData_PickIsNotFinalised()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var ftzWhs = Helper.CreateFTZWarehouseInUS();
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, ftzWhs, "REC", data.Part1, 10m);
			var order = Helper.CreateWhsOrder(data.Org1, ftzWhs);
			order.WD_DocketSubType = OrderType.Codes.CustomsReleaseWithPermit;
			var orderLine = CreateOrderLine(order, data.Part1);

			using (PermitServiceTestHelper.MockServiceToGetPermits())
			{
				var pick = Helper.CreatePickNew(order);
				pick.FinaliseAllOrders();
				AssertEquals("Precondition: Pick is not finalised", false, pick.IsFinalised);
				AssertNoExceptionThrown("Save Pick With No Exception.", Factory.Save);
			}

			var provider = new FTZWhsOrderLinesDataProvider();
			AssertEquals("When Pick is not finalised, should not find out any lines", false,
				provider.GetFTZWhsOrderLinesData(data.Org1, ftzWhs.WarehouseAddress, "DummyOutwards").Any());
		}

		#endregion

		#region TestGetFTZWhsOrderLinesData_NotCustomsReleaseOrder

		public void TestGetFTZWhsOrderLinesData_NotCustomsReleaseOrder()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var ftzWhs = Helper.CreateFTZWarehouse();
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, ftzWhs, "REC", data.Part1, 10m);
			var order = Helper.CreateWhsOrder(data.Org1, ftzWhs);
			var orderLine = CreateOrderLine(order, data.Part1);
			var pick = Helper.CreatePickNew(order);
			pick.FinaliseAllOrders();
			pick.FinalisePick();
			Factory.Save();

			AssertIsFinalisedPrecondition(pick);
			AssertEquals("Precondition: Order.SubType is not Customs Release", false,
				order.WD_DocketSubType == OrderType.Codes.Customs);

			var provider = new FTZWhsOrderLinesDataProvider();
			AssertEquals("When Order.SubType is not Customs Release, should not find out any lines", false,
				provider.GetFTZWhsOrderLinesData(data.Org1, ftzWhs.WarehouseAddress, "DummyOutwards").Any());
		}

		#endregion

		#region TestGetFTZWhsOrderLinesData_OutwardsEntryNumberIsNullOrEmpty

		public void TestGetFTZWhsOrderLinesData_OutwardsEntryNumberIsNull()
		{
			TestGetFTZWhsOrderLinesData_OutwardsEntryNumberIsNullOrEmptyCore(null);
		}

		public void TestGetFTZWhsOrderLinesData_OutwardsEntryNumberIsEmpty()
		{
			TestGetFTZWhsOrderLinesData_OutwardsEntryNumberIsNullOrEmptyCore("");
		}

		void TestGetFTZWhsOrderLinesData_OutwardsEntryNumberIsNullOrEmptyCore(string outwardsEntryNumber)
		{
			var client = Helper.CreateClient("ORG");
			var ftzWhs = Helper.CreateFTZWarehouseInUS();
			Factory.Save();

			var provider = new FTZWhsOrderLinesDataProvider();
			AssertExceptionThrown(typeof(ArgumentException), @"Value cannot be empty string ("""").
Parameter name: outwardEntryNumber",
				() => provider.GetFTZWhsOrderLinesData(client, ftzWhs.WarehouseAddress, outwardsEntryNumber));
		}

		#endregion

		#region TestGetFTZWhsOrderLinesData_OutwardsEntryNumberIsNotNullOrEmpty

		public void TestGetFTZWhsOrderLinesData_OutwardsEntryNumberIsNotNullOrEmpty()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var ftzWhs = Helper.CreateFTZWarehouseInUS();
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, ftzWhs, "REC", data.Part1, 15m);
			var order = Helper.CreateWhsOrder(data.Org1, ftzWhs);
			order.WD_DocketSubType = OrderType.Codes.CustomsReleaseWithPermit;
			Factory.Save();

			var orderLine1 = CreateOrderLine(order, data.Part1, "DummyOutwards1");
			var orderLine2 = CreateOrderLine(order, data.Part1, "DummyOutwards2");
			var orderLine3 = CreateOrderLine(order, data.Part1, "DummyOutwards3");

			using (PermitServiceTestHelper.MockServiceToGetPermits(new[]
				   {
					   new WhsPermitWithdrawRequestResponseForTest(orderLine1, SuccessOrFailure.Success, 1000m,
						   "DummyOutwards1"),
					   new WhsPermitWithdrawRequestResponseForTest(orderLine2, SuccessOrFailure.Success, 1000m,
						   "DummyOutwards2"),
					   new WhsPermitWithdrawRequestResponseForTest(orderLine3, SuccessOrFailure.Success, 1000m,
						   "DummyOutwards3")
				   }))
			{
				var pick = Helper.CreatePickNew(order);
				pick.FinaliseAllOrders();
				pick.FinalisePick();
				AssertIsFinalisedPrecondition(pick);
				AssertNoExceptionThrown("Save Pick With No Exception.", Factory.Save);
			}

			var provider = new FTZWhsOrderLinesDataProvider();
			var ftzLines = provider.GetFTZWhsOrderLinesData(data.Org1, ftzWhs.WarehouseAddress, "DummyOutwards2");
			AssertEquals("When OutwardsEntryNumber is not null or empty, should find out 1 line", 1, ftzLines.Count());
			AssertFTZOrderLine(orderLine2, ftzLines.Single());
		}

		#endregion

		#region TestGetFTZWhsOrderLinesData_OrderTypeIsCUS

		public void TestGetFTZWhsOrderLinesData_OrderTypeIsCUS()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var ftzWhs = Helper.CreateFTZWarehouseInUS();
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, ftzWhs, "REC", data.Part1, 15m);
			var order = Helper.CreateWhsOrder(data.Org1, ftzWhs);
			order.WD_DocketSubType = OrderType.Codes.Customs;
			Factory.Save();

			var orderLine1 = CreateOrderLine(order, data.Part1, "DummyOutwards1");
			var orderLine2 = CreateOrderLine(order, data.Part1, "DummyOutwards2");
			var orderLine3 = CreateOrderLine(order, data.Part1, "DummyOutwards3");

			var pick = Helper.CreatePickNew(order);
			pick.FinaliseAllOrders();
			pick.FinalisePick();
			AssertIsFinalisedPrecondition(pick);
			AssertNoExceptionThrown("Save Pick With No Exception.", Factory.Save);

			var provider = new FTZWhsOrderLinesDataProvider();
			AssertEquals("When Warehouse Order is CUS, should not find any lines.", 0,
				provider.GetFTZWhsOrderLinesData(data.Org1, ftzWhs.WarehouseAddress, "DummyOutwards2").Count());
		}

		#endregion

		#region TestGetFTZWhsOrderLinesData_DifferentWarehouses

		public void TestGetFTZWhsOrderLinesData_DifferentWarehouses()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var ftzWhs1 = Helper.CreateFTZWarehouseInUS(warehouseCode: "AAA");
			var ftzWhs2 = Helper.CreateFTZWarehouseInUS(warehouseCode: "BBB");

			var receive1 = Helper.CreateWhsReceiveWithInventory(data.Org1, ftzWhs1, "REC1", data.Part1, 10m);
			var order1 = Helper.CreateWhsOrder(data.Org1, ftzWhs1, "ORD1");
			order1.WD_DocketSubType = OrderType.Codes.CustomsReleaseWithPermit;
			Factory.Save();

			var orderLine1 = CreateOrderLine(order1, data.Part1);

			using (PermitServiceTestHelper.MockServiceToGetPermits(new[]
				   {
					   new WhsPermitWithdrawRequestResponseForTest(orderLine1, SuccessOrFailure.Success, 1000m,
						   "DummyOutwards")
				   }))
			{
				var pick1 = Helper.CreatePickNew(order1);
				pick1.FinaliseAllOrders();
				pick1.FinalisePick();
				AssertIsFinalisedPrecondition(pick1);
				AssertNoExceptionThrown("Save Pick With No Exception.", Factory.Save);
			}

			var receive2 = Helper.CreateWhsReceiveWithInventory(data.Org1, ftzWhs2, "REC2", data.Part1, 10m);
			var order2 = Helper.CreateWhsOrder(data.Org1, ftzWhs2, "ORD2");
			order2.WD_DocketSubType = OrderType.Codes.CustomsReleaseWithPermit;
			Factory.Save();

			var orderLine2 = CreateOrderLine(order2, data.Part1);

			using (PermitServiceTestHelper.MockServiceToGetPermits(new[]
				   {
					   new WhsPermitWithdrawRequestResponseForTest(orderLine2, SuccessOrFailure.Success, 1000m,
						   "DummyOutwards")
				   }))
			{
				var pick2 = Helper.CreatePickNew(order2);
				pick2.FinaliseAllOrders();
				pick2.FinalisePick();
				AssertIsFinalisedPrecondition(pick2);
				AssertNoExceptionThrown("Save Pick With No Exception.", Factory.Save);
			}

			var provider = new FTZWhsOrderLinesDataProvider();
			var ftzLines = provider.GetFTZWhsOrderLinesData(data.Org1, ftzWhs2.WarehouseAddress, "DummyOutwards");
			AssertEquals("Should only find out 1 line", 1, ftzLines.Count());
			AssertFTZOrderLine(orderLine2, ftzLines.Single());
		}

		#endregion

		#region TestGetFTZWhsOrderLinesData_DifferentImporters

		public void TestGetFTZWhsOrderLinesData_DifferentImporters()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var org2 = Helper.CreateClient("ORG2");
			Helper.CreateProductClientRelationShip(org2, data.Part1);
			var ftzWhs = Helper.CreateFTZWarehouseInUS();

			var receive1 = Helper.CreateWhsReceiveWithInventory(data.Org1, ftzWhs, "REC1", data.Part1, 10m);
			var order1 = Helper.CreateWhsOrder(data.Org1, ftzWhs, "ORD1");
			order1.WD_DocketSubType = OrderType.Codes.CustomsReleaseWithPermit;
			Factory.Save();

			var orderLine1 = CreateOrderLine(order1, data.Part1);

			using (PermitServiceTestHelper.MockServiceToGetPermits(new[]
				   {
					   new WhsPermitWithdrawRequestResponseForTest(orderLine1, SuccessOrFailure.Success, 1000m,
						   "DummyOutwards")
				   }))
			{
				var pick1 = Helper.CreatePickNew(order1);
				pick1.FinaliseAllOrders();
				pick1.FinalisePick();
				AssertIsFinalisedPrecondition(pick1);
				AssertNoExceptionThrown("Save Pick With No Exception.", Factory.Save);
			}

			var receive2 = Helper.CreateWhsReceiveWithInventory(org2, ftzWhs, "REC2", data.Part1, 10m);
			var order2 = Helper.CreateWhsOrder(org2, ftzWhs, "ORD2");
			order2.WD_DocketSubType = OrderType.Codes.CustomsReleaseWithPermit;
			Factory.Save();

			var orderLine2 = CreateOrderLine(order2, data.Part1);

			using (PermitServiceTestHelper.MockServiceToGetPermits(new[]
				   {
					   new WhsPermitWithdrawRequestResponseForTest(orderLine2, SuccessOrFailure.Success, 1000m,
						   "DummyOutwards")
				   }))
			{
				var pick2 = Helper.CreatePickNew(order2);
				pick2.FinaliseAllOrders();
				pick2.FinalisePick();
				AssertIsFinalisedPrecondition(pick2);
				AssertNoExceptionThrown("Save Pick With No Exception.", Factory.Save);
			}

			var provider = new FTZWhsOrderLinesDataProvider();
			var ftzLines = provider.GetFTZWhsOrderLinesData(org2, ftzWhs.WarehouseAddress, "DummyOutwards");
			AssertEquals("Should only find out 1 line", 1, ftzLines.Count());
			AssertFTZOrderLine(orderLine2, ftzLines.Single());
		}

		#endregion

		#region Implementation

		WhsOrderLine CreateOrderLine(WhsOrder order, OrgSupplierPart part, string outwardsEntryNumber = "DummyOutwards")
		{
			var orderLine = Helper.CreateWhsOrderLine(order, part, 5m, "123", outwardsEntryNumber);
			orderLine.CustomsData.WB_Tariff = "AAA";
			orderLine.CustomsData.WB_RN_NKCountryOfOrigin = "US";
			orderLine.CustomsData.WB_PrimaryPreference = "BBB";
			orderLine.CustomsData.WB_BondedWhsQty = 25m;
			orderLine.CustomsData.WB_ValueForDuty = 10m;
			orderLine.CustomsData.WB_CustomsQty = 15m;
			orderLine.CustomsData.WB_CustomsUnitOfQty = "BOX";
			orderLine.CustomsData.WB_CustomsSecondQuantity = 20m;
			orderLine.CustomsData.WB_CustomsSecondUnitQty = "PLT";
			orderLine.CustomsData.WB_CustomsThirdQuantity = 10m;
			orderLine.CustomsData.WB_CustomsThirdUnitQty = "CU";
			orderLine.CustomsData.WB_AddInfo = outwardsEntryNumber;

			return orderLine;
		}

		void AssertFTZOrderLine(WhsOrderLine expectedOrderLine, IFTZWhsOrderLineData ftzOrderLine)
		{
			AssertEquals(expectedOrderLine.ProductCode, ftzOrderLine.ProductCode);
			AssertEquals(expectedOrderLine.ProductDesc, ftzOrderLine.ProductDesc);
			AssertEquals(expectedOrderLine.WE_TransactionQuantity, ftzOrderLine.Quantity);
			AssertEquals(expectedOrderLine.ProductUQ, ftzOrderLine.QuantityUnit);
			AssertEquals(expectedOrderLine.CustomsData.WB_Tariff, ftzOrderLine.Tariff);
			AssertEquals(expectedOrderLine.CustomsData.WB_RN_NKCountryOfOrigin, ftzOrderLine.CountryOfOriginCode);
			AssertEquals(expectedOrderLine.CustomsData.WB_PrimaryPreference, ftzOrderLine.PrimaryPreference);
			AssertEquals(expectedOrderLine.CustomsData.WB_BondedWhsQty, ftzOrderLine.TotalReceiveQuantity);
			AssertEquals(expectedOrderLine.CustomsData.WB_ValueForDuty, ftzOrderLine.TotalReceiveValueForDuty);
			AssertEquals(expectedOrderLine.CustomsData.WB_CustomsQty, ftzOrderLine.TotalReceiveCustomsQty);
			AssertEquals(expectedOrderLine.CustomsData.WB_CustomsUnitOfQty, ftzOrderLine.ReceiveCustomsQtyUnit);
			AssertEquals(expectedOrderLine.CustomsData.WB_CustomsSecondQuantity,
				ftzOrderLine.TotalReceiveCustomsSecondQty);
			AssertEquals(expectedOrderLine.CustomsData.WB_CustomsSecondUnitQty,
				ftzOrderLine.ReceiveCustomsSecondQtyUnit);
			AssertEquals(expectedOrderLine.CustomsData.WB_CustomsThirdQuantity,
				ftzOrderLine.TotalReceiveCustomsThirdQty);
			AssertEquals(expectedOrderLine.CustomsData.WB_CustomsThirdUnitQty, ftzOrderLine.ReceiveCustomsThirdQtyUnit);
			AssertEquals(expectedOrderLine.CustomsData.WB_AddInfo, ftzOrderLine.ReceiveCustomsAddInfo);
		}

		#endregion
	}
}
