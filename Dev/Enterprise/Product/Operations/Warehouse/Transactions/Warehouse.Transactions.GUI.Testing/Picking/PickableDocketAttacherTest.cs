#region Test
#if DEBUG

namespace Enterprise.Warehouse.Transactions.GUI.Testing
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using CargoWise.EntityFramework;
	using Enterprise.MasterFiles.Business;
	using Enterprise.Packing.Business;
	using Enterprise.Warehouse.Transactions.Business;
	using Enterprise.Warehouse.Transactions.Business.Testing;
	using Enterprise.ZArchitecture.Environment;
	using Enterprise.ZArchitecture.GUI;
	using Enterprise.ZArchitecture.Modules;
	using Enterprise.ZArchitecture.Schema;

	public class PickableDocketAttacherTest : WhsGuiTestCaseWithFactory
	{
		#region TestAttach

		public void TestAttach()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var orders = new List<WhsOrder>();
			for (int i = 0; i < 11; i++)
			{
				orders.Add(Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O" + i, data.Part1, 1m));
			}

			var pick = Helper.CreatePickNew(orders[0]);

			var pickLineQuantityValidationCounter = 0;
			var quantityShortValidationCounter = 0;

			var orderedInventory = pick.OrderedInventories[0];
			orderedInventory.PickLineQuantityInfo.AdditionalValidation += () => pickLineQuantityValidationCounter++;
			orderedInventory.QuantityShortInfo.AdditionalValidation += () => quantityShortValidationCounter++;

			using (var form = new ZForm())
			{
				form.Show();
				var attacher = new PickableDocketAttacher(pick, pick.Orders, pick.Lookups.OrderFindBoxList, ModuleIDs.WhsOrder);
				attacher.Show(form);

				AttachOrders(orders.Skip(1), attacher);

				AssertContainsExactElementsInAnyOrder("Precondition: Should have attached orders.", orders, pick.Orders);
				AssertEquals("Should have validated PickLineQuantity once.", 1, pickLineQuantityValidationCounter);
				AssertEquals("Should have validated QuantityShort once.", 1, quantityShortValidationCounter);
			}
		}

		public void TestAttach_DbHits()
		{
			var data = new TestDataSimpleEnvironment(Factory, 10, 10);

			for (int i = 0; i < 10; i++)
			{
				var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R" + i);
				for (int j = 0; j < 10; j++)
				{
					Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m);
				}

				receive.AllocateLocationsWithMock();
				receive.FinaliseDocket();
				AssertIsFinalisedPrecondition(receive);
			}

			for (int i = 0; i < 10; i++)
			{
				var adjustment = Helper.CreateWhsAdjustment(data.Org1, data.Whs1, "A" + i);
				for (int j = 0; j < 10; j++)
				{
					Helper.CreateWhsAdjustmentLine(adjustment, data.Part1, 1m, data.Whs1.DefaultLocation);
				}

				adjustment.FinaliseDocket();
				AssertIsFinalisedPrecondition(adjustment);
			}

			var orders = new List<WhsOrder>();
			for (int i = 0; i < 10; i++)
			{
				orders.Add(Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O" + i, data.Part1, 1m));
			}

			Factory.Save();

			var otherFactory = new BusinessObjectFactory { RefreshEnabled = false };

			var pick = otherFactory.New<WhsPick>();

			using (var form = new ZForm())
			{
				form.Show();
				var attacher = new PickableDocketAttacher(pick, pick.Orders, pick.Lookups.OrderFindBoxList, ModuleIDs.WhsOrder);
				attacher.Show(form);

				using (RowFactory.SetCachedTables())
				{
					AttachOrders(orders, attacher);
				}

				var expectedDbHits = new Dictionary<string, int>()
				{
					{ OrgHeaderSchema.Constants.TableName, 1 },
					{ OrgMiscServSchema.Constants.TableName, 1 },
					{ OrgSupplierPartSchema.Constants.TableName, 1 },
					{ OrgPartUnitSchema.Constants.TableName, 1 },
					{ PkgPackageJobSchema.Constants.TableName, 1 },
					{ RefPackTypeSchema.Constants.TableName, 1 },
					{ WhsDocketSchema.Constants.TableName, 1 },
					{ WhsDocketLineSchema.Constants.TableName, 1 },
					{ WhsLocationTypeSchema.Constants.TableName, 1 },
					{ WhsLocationViewSchema.Constants.TableName, 1 },
					{ WhsPickLineSchema.Constants.TableName, 1 },
					{ WhsWarehouseSchema.Constants.TableName, 1 },
				};

				using (AssertDbHitsWithUsefulQueryInformation(expectedDbHits, otherFactory))
				{
				}
			}

			var ordersInOtherFactory = otherFactory.Load<WhsOrder>(new ZQuery(WhsDocketSchema.PK, orders.Select(o => o.PK)));
			AssertContainsExactElementsInAnyOrder("Precondition: Should have attached orders.", ordersInOtherFactory, pick.Orders);
		}

		#region TestTotals

		#region TestTotals_RecalculateAfter_AttachOrder

		public void TestTotals_RecalculateAfter_AttachOrder()
		{
			var (data, pick) = GetTestDataForTotals();
			var newOrder = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O4", data.Part2, 5m, WhsPickOption.Codes.Manual);
			Factory.Save();

			using (var form = new ZForm())
			{
				form.Show();
				var attacher = new PickableDocketAttacher(pick, pick.Orders, pick.Lookups.OrderFindBoxList, ModuleIDs.WhsOrder);
				attacher.Show(form);

				AttachOrders(new WhsOrder[] { newOrder }, attacher);
			}

			AssertEquals("NumberOfOrders should be 4.", 4, pick.NumberOfOrders);
			AssertEquals("TotalWeight should still be 12 KG since Attach will not allocate Orders.", 12m, pick.TotalWeight);
			AssertEquals("TotalVolume should be 12 M3 since Attach will not allocate Orders.", 12m, pick.TotalVolume);
			AssertEquals("TotalLineQty should be 12 since Attach will not allocate Orders.", 12m, pick.TotalLineQty);
			AssertEquals("TotalComponentQty should be 0 since Attach will not allocate Orders.", 0m, pick.TotalComponentQty);
		}

		#endregion

		#region TestTotals_RecalculateAfter_AttachOrders

		public void TestTotals_RecalculateAfter_AttachOrders()
		{
			var (data, pick) = GetTestDataForTotals();
			var newOrder1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O4", data.Part2, 5m, WhsPickOption.Codes.Manual);
			var newOrder2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O5", data.Part2, 5m, WhsPickOption.Codes.Manual);
			Factory.Save();

			using (var form = new ZForm())
			{
				form.Show();
				var attacher = new PickableDocketAttacher(pick, pick.Orders, pick.Lookups.OrderFindBoxList, ModuleIDs.WhsOrder);
				attacher.Show(form);

				AttachOrders(new WhsOrder[] { newOrder1, newOrder2 }, attacher);
			}

			AssertEquals("NumberOfOrders should be 5.", 5, pick.NumberOfOrders);
			AssertEquals("TotalWeight should be 12 KG since Attach will not allocate Orders.", 12m, pick.TotalWeight);
			AssertEquals("TotalVolume should be 12 M3 since Attach will not allocate Orders.", 12m, pick.TotalVolume);
			AssertEquals("TotalLineQty should be 12 since Attach will not allocate Orders.", 12m, pick.TotalLineQty);
			AssertEquals("TotalComponentQty should be 0 since Attach will not allocate Orders.", 0m, pick.TotalComponentQty);
		}

		#endregion

		#region GetTestDataForTotals

		(TestDataSimpleEnvironment, WhsPick) GetTestDataForTotals()
		{
			PackingRegistry.Instance.WeightUnit.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "KG");
			PackingRegistry.Instance.VolumeUnit.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "M3");
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetProductWeightAndVolume(data.Part1, 1, "KG", 1, "M3");
			Helper.SetProductWeightAndVolume(data.Part2, 100, "G", 10, "D3");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 50m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part2, 50m);
			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 3m, WhsPickOption.Codes.Manual);
			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O2", data.Part1, 4m, WhsPickOption.Codes.Manual);
			var order3 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O3", data.Part1, 5m, WhsPickOption.Codes.Manual);
			var pick = Helper.CreatePickNew(order1, order2, order3);
			pick.AutoAllocateItemsWithMock();
			Factory.Save();

			AssertEquals("NumberOfOrders should be 3.", 3, pick.NumberOfOrders);
			AssertEquals("TotalWeight should be 12 KG.", 12m, pick.TotalWeight);
			AssertEquals("TotalVolume should be 12 M3.", 12m, pick.TotalVolume);
			AssertEquals("TotalLineQty should be 12.", 12m, pick.TotalLineQty);
			AssertEquals("TotalComponentQty should be 0.", 0m, pick.TotalComponentQty);

			return (data, pick);
		}

		#endregion

		#endregion

		static void AttachOrders(IEnumerable<WhsOrder> orders, PickableDocketAttacher attacher)
		{
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			using (var popup = attacher.LastShownAttachPopupForTesting)
			{
				popup.EmbeddedModulePopupOKButtonStrategy.HandleFindBoxOKButton(orders.ToArray());
			}
		}

		#endregion
	}
}

#endif
#endregion
