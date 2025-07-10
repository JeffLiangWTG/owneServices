using System;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	class ConsigneeECommerceScoreCacheTest : WhsTestCaseWithFactory
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentNullException>(() => new ConsigneeECommerceScoreCache(null));
		}

		public void TestIsConsigneeOnOrderUsedOnOtherWarehouseJobs_DoesNotAcceptNull()
		{
			var pick = Factory.New<WhsPick>();
			AssertExceptionThrown<ArgumentNullException>(() =>
				new ConsigneeECommerceScoreCache(pick).IsConsigneeOnOrderUsedOnOtherWarehouseJobs(null));
		}

		public void TestIsConsigneeOnOrderUsedOnOtherWarehouseJobs()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 100m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 5m);
			var pick = Helper.CreatePickNew(order);
			AssertEquals(false,
				new ConsigneeECommerceScoreCache(pick).IsConsigneeOnOrderUsedOnOtherWarehouseJobs(order));
		}

		public void TestIsConsigneeOnOrderUsedOnOtherWarehouseJobs_OrderOnOtherPick()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 100m);
			Factory.Save();

			var consignee = Helper.CreateClient("CON");
			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 5m);
			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O2", data.Part1, 5m);
			order1.ConsigneeAddressPK = consignee.MainAddress.PK;
			order2.ConsigneeAddressPK = consignee.MainAddress.PK;
			var pick = Helper.CreatePickNew(order1);
			AssertEquals(true,
				new ConsigneeECommerceScoreCache(pick).IsConsigneeOnOrderUsedOnOtherWarehouseJobs(order1));
			AssertEquals(false,
				new ConsigneeECommerceScoreCache(pick).IsConsigneeOnOrderUsedOnOtherWarehouseJobs(order2));
		}

		public void TestIsConsigneeOnOrderUsedOnOtherWarehouseJobs_MultipleOrders()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 100m);
			Factory.Save();

			var consignee = Helper.CreateClient("CON");
			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 5m);
			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O2", data.Part1, 5m);
			order1.ConsigneeAddressPK = consignee.MainAddress.PK;
			order2.ConsigneeAddressPK = consignee.MainAddress.PK;
			var pick = Helper.CreatePickNew(order1, order2);
			AssertEquals(true,
				new ConsigneeECommerceScoreCache(pick).IsConsigneeOnOrderUsedOnOtherWarehouseJobs(order1));
			AssertEquals(true,
				new ConsigneeECommerceScoreCache(pick).IsConsigneeOnOrderUsedOnOtherWarehouseJobs(order2));
		}

		public void TestIsConsigneeOnOrderUsedOnOtherWarehouseJobs_OverriddenAddress()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 100m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 5m);
			order.ConsigneeDocAddress.E2_AddressOverride = true;
			order.ConsigneeDocAddress.E2_City = "Sydney";
			order.ConsigneeDocAddress.E2_Postcode = "2000";
			order.ConsigneeDocAddress.E2_State = "NSW";
			order.ConsigneeDocAddress.E2_RN_NKCountryCode = "AU";
			var pick = Helper.CreatePickNew(order);
			AssertEquals(false,
				new ConsigneeECommerceScoreCache(pick).IsConsigneeOnOrderUsedOnOtherWarehouseJobs(order));
		}

		public void TestIsConsigneeOnOrderUsedOnOtherWarehouseJobs_NonConsigneeAddressIsDuplicated()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 100m);
			Factory.Save();

			var consignee1 = Helper.CreateClient("CON1");
			var consignee2 = Helper.CreateClient("CON2");
			var distributionCentre = Helper.CreateClient("DST1");
			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 5m);
			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O2", data.Part1, 5m);
			order1.ConsigneeAddressPK = consignee1.MainAddress.PK;
			order2.ConsigneeAddressPK = consignee2.MainAddress.PK;
			order1.DistributionCentreDocAddress.E2_OA_Address = distributionCentre.MainAddress.PK;
			order2.DistributionCentreDocAddress.E2_OA_Address = distributionCentre.MainAddress.PK;
			var pick = Helper.CreatePickNew(order1, order2);
			AssertEquals(false,
				new ConsigneeECommerceScoreCache(pick).IsConsigneeOnOrderUsedOnOtherWarehouseJobs(order1));
			AssertEquals(false,
				new ConsigneeECommerceScoreCache(pick).IsConsigneeOnOrderUsedOnOtherWarehouseJobs(order2));
		}
	}
}
