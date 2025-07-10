using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Registry.Business.Warehouse;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	public class WhsTransferTestInTestProject : WhsTestCaseWithFactory
	{
		public void TestLocationStockOnHandCache()
		{
			using (WarehouseDataRegistry.Instance.SOHLocationWarning.SetTemporaryValue(
					   Enterprise.Environment.Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true))
			{
				var data = new TestDataSimpleEnvironment(Factory, 3, 1);
				var locationA1 = data.Whs1.FindLocation("A-1");
				var locationA2 = data.Whs1.FindLocation("A-2");

				var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
				Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, locationA1);
				Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 10m, locationA2);
				receive.FinaliseDocket();
				AssertIsFinalisedPrecondition(receive);
				Factory.Save();

				var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
				var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, locationA1, locationA2);
				transfer.RunPreSaveValidation();

				var key = GetStockOnHandLocationCacheKey(locationA2.PK, transfer.PK);
				AssertNotNull("Cache should exist on factory",
					Factory.GetCachedValue<IEnumerable<LocationStockOnHandInfo>>(key, () => null));

				Factory.Save();
				AssertNull("Cache should not exist on factory after factory save",
					Factory.GetCachedValue<IEnumerable<LocationStockOnHandInfo>>(key, () => null));
			}
		}

		public void TestLocationStockOnHandCache_MultipleLocations()
		{
			using (WarehouseDataRegistry.Instance.SOHLocationWarning.SetTemporaryValue(
					   Enterprise.Environment.Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true))
			{
				var data = new TestDataSimpleEnvironment(Factory, 3, 1);
				var locationA1 = data.Whs1.FindLocation("A-1");
				var locationA2 = data.Whs1.FindLocation("A-2");
				var locationA3 = data.Whs1.FindLocation("A-3");

				var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
				Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 20m, locationA1);
				Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 10m, locationA2, "PLT2");
				receive.FinaliseDocket();
				AssertIsFinalisedPrecondition(receive);
				Factory.Save();

				var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
				Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, locationA1, locationA2);
				Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, locationA1, locationA3);
				transfer.RunPreSaveValidation();

				var key1 = GetStockOnHandLocationCacheKey(locationA2.PK, transfer.PK);
				AssertNotNull("Cache should exist on factory",
					Factory.GetCachedValue<IEnumerable<LocationStockOnHandInfo>>(key1, () => null));

				var key2 = GetStockOnHandLocationCacheKey(locationA3.PK, transfer.PK);
				AssertNotNull("Cache should exist on factory",
					Factory.GetCachedValue<IEnumerable<LocationStockOnHandInfo>>(key2, () => null));

				Factory.Save();
				AssertNull("Cache should not exist on factory after factory save",
					Factory.GetCachedValue<IEnumerable<LocationStockOnHandInfo>>(key1, () => null));
				AssertNull("Cache should not exist on factory after factory save",
					Factory.GetCachedValue<IEnumerable<LocationStockOnHandInfo>>(key2, () => null));
			}
		}

		public void TestLocationStockOnHandCache_MultiplePallets()
		{
			using (WarehouseDataRegistry.Instance.SOHLocationWarning.SetTemporaryValue(
					   Enterprise.Environment.Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true))
			{
				var data = new TestDataSimpleEnvironment(Factory, 3, 1);
				var locationA1 = data.Whs1.FindLocation("A-1");
				var locationA2 = data.Whs1.FindLocation("A-2");
				var locationA3 = data.Whs1.FindLocation("A-3");

				var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
				Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, locationA1, "PLT1");
				Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, locationA1, "PLT2");
				Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, locationA2, "PLT3");
				Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 10m, locationA2, "PLT4");
				receive.FinaliseDocket();
				AssertIsFinalisedPrecondition(receive);
				Factory.Save();

				var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
				Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, "A-1", "PLT1", "A-2", "PLT1");
				Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, "A-1", "PLT2", "A-2", "PLT2");
				transfer.RunPreSaveValidation();

				var key = GetStockOnHandLocationCacheKey(locationA2.PK, transfer.PK);
				AssertNotNull("Cache should exist on factory",
					Factory.GetCachedValue<IEnumerable<LocationStockOnHandInfo>>(key, () => null));

				Factory.Save();
				AssertNull("Cache should not exist on factory after factory save",
					Factory.GetCachedValue<IEnumerable<LocationStockOnHandInfo>>(key, () => null));
			}
		}

		public void TestLocationStockOnHandCache_DockdoorLocation()
		{
			using (WarehouseDataRegistry.Instance.SOHLocationWarning.SetTemporaryValue(
					   Enterprise.Environment.Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true))
			{
				var data = new TestDataSimpleEnvironment(Factory, 2, 1);
				var locationA1 = data.Whs1.FindLocation("A-1");
				Factory.Save();

				var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
				Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, locationA1);
				receive.FinaliseDocket();
				AssertIsFinalisedPrecondition(receive);

				Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 10m,
					data.Whs1.DefaultOutboundDockDoorLocation, "ABC", false, false);

				Factory.Save();

				var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
				Helper.CreatePickNew(order);

				var pickLine = order.Lines[0].PickLines.Single();
				var transferLine = Helper.PickAndMakeInTransitTransfer(pickLine, ZDateTimeOffset.Now);
				var transfer = transferLine.Docket;
				transfer.RunPreSaveValidation();

				AssertNull("Cache should not exist on factory",
					Factory.GetCachedValue<IEnumerable<LocationStockOnHandInfo>>(
						GetStockOnHandLocationCacheKey(transferLine.Location.PK, transfer.PK), () => null));
			}
		}

		public void TestLocationStockOnHandCache_SOHLocationWarningDisabled()
		{
			using (WarehouseDataRegistry.Instance.SOHLocationWarning.SetTemporaryValue(
					   Enterprise.Environment.Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, false))
			{
				var data = new TestDataSimpleEnvironment(Factory, 3, 1);
				var locationA1 = data.Whs1.FindLocation("A-1");
				var locationA2 = data.Whs1.FindLocation("A-2");

				var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
				Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, locationA1, "PLT1");
				Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 10m, locationA2, "PLT2");
				receive.FinaliseDocket();
				AssertIsFinalisedPrecondition(receive);
				Factory.Save();

				var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
				var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, locationA1, locationA2);
				transfer.RunPreSaveValidation();

				AssertNull("Cache should not exist on factory after factory save",
					Factory.GetCachedValue<IEnumerable<LocationStockOnHandInfo>>(
						GetStockOnHandLocationCacheKey(locationA2.PK, transfer.PK), () => null));
			}
		}

		static string GetStockOnHandLocationCacheKey(ZGuid locationPK, ZGuid docketPK)
		{
			return "LocationStockOnHandInfoCache|" + docketPK + "|" + locationPK;
		}
	}
}
