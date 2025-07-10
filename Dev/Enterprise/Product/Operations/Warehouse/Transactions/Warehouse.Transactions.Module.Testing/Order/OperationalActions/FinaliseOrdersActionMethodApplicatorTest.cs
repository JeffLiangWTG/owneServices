using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.Services.OperationalActions.Support.Testing;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Module.Testing
{
	[TestedType(typeof(FinaliseOrdersActionMethodApplicator))]
	public class FinaliseOrdersActionMethodApplicatorTest : FinaliseDocketActionMethodApplicatorTest<WhsOrder, FinaliseOrdersActionMethodApplicator>
	{
		#region TestApplyApplicator_MultipleDockets

		protected override void TestApplyApplicator_MultipleDocketsCore()
		{
			var data = new TestDataSimpleEnvironment_ForFatDatTestsRequiringDataSetup(Factory);

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);

			var order_NotPicked = Helper.CreateWhsOrder(data.Org1, data.Whs1, data.GetUniqueNameForFatDat(WhsDocketSchema.WD_ExternalReference));
			var orderLine_NotPicked = Helper.CreateWhsOrderLine(order_NotPicked, data.Part1, 1m);
			var order_PickedWithError = Helper.CreateWhsOrder(data.Org1, data.Whs1, data.GetUniqueNameForFatDat(WhsDocketSchema.WD_ExternalReference)); // different product + fulfilment rule.
			var orderline_PickedWithError = Helper.CreateWhsOrderLine(order_PickedWithError, data.Part2, 2m);

			order_PickedWithError.WD_WhsOrderFulfillmentRule = WhsOrderFulfillmentRuleList.Codes.All;

			var order_PickedCanBeFinalised = Helper.CreateWhsOrder(data.Org1, data.Whs1, data.GetUniqueNameForFatDat(WhsDocketSchema.WD_ExternalReference));
			var orderLine_PickedCanBeFinalised = Helper.CreateWhsOrderLine(order_PickedCanBeFinalised, data.Part1, 3m);
			var order_Finalised = Helper.CreateWhsOrder(data.Org1, data.Whs1, data.GetUniqueNameForFatDat(WhsDocketSchema.WD_ExternalReference));
			var orderLine_Finalised = Helper.CreateWhsOrderLine(order_Finalised, data.Part1, 4m);

			var pick = Helper.CreatePickNew(order_PickedWithError, order_PickedCanBeFinalised, order_Finalised);
			pick.FinaliseOrder(order_Finalised);
			AssertEquals("Precondition", false, order_NotPicked.IsAttachedToPickButNotFinalised);
			AssertEquals("Precondition", true, order_PickedWithError.IsAttachedToPickButNotFinalised);
			AssertEquals("Precondition", true, order_PickedCanBeFinalised.IsAttachedToPickButNotFinalised);
			WhsTestCaseWithFactory.AssertIsFinalisedPrecondition(order_Finalised);

			Factory.Save();

			var orders = new[] { order_NotPicked, order_PickedWithError, order_PickedCanBeFinalised, order_Finalised };
			ApplyApplicator(orders, @"
WARNING: Order [HL W00000002] - is not picked. It must be picked before it can be finalized.
WARNING: Order [HL W00000003] - could not be auto-finalized. It must be manually finalized.
INFO: Order [HL W00000004] - was successfully finalized.
WARNING: Order [HL W00000005] - is already finalized.".Trim());
		}

		#endregion

		#region TestApplyApplicator_CannotFinalise

		protected override void TestApplyApplicator_CannotFinaliseCore()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);

			var order_PickedWithError = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1"); // different product + fulfilment rule.
			var orderline_PickedWithError = Helper.CreateWhsOrderLine(order_PickedWithError, data.Part2, 2m);

			order_PickedWithError.WD_WhsOrderFulfillmentRule = WhsOrderFulfillmentRuleList.Codes.All;

			var pick = Helper.CreatePickNew(order_PickedWithError);

			Factory.Save();
			ApplyApplicator(new[] { order_PickedWithError }, $@"
WARNING: {order_PickedWithError.Description} [HL W00000002] - could not be auto-finalized. It must be manually finalized.".Trim());
			var docketInNewFactory = new BusinessObjectFactory().Load<WhsDocket>(order_PickedWithError.PK);
			Assert("Docket is not finalized.", !docketInNewFactory.IsFinalised);
		}

		#endregion

		#region TestApplyApplicator_EachOrderIsFinalisedInSeparateFactory

		public void TestApplyApplicator_EachOrderIsFinalisedInSeparateFactory()
		{
			WarehouseDataRegistry.Instance.AutoFinalizePick.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var otherFactory = new BusinessObjectFactory();
			var helperInOtherFactory = new WhsTestHelperFunctions(otherFactory);
			var data = new TestDataSimpleEnvironment_ForFatDatTestsRequiringDataSetup(otherFactory);

			var receive1InOtherFactory = helperInOtherFactory.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			var receive2InOtherFactory = helperInOtherFactory.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part2, 10m);

			otherFactory.Save();

			var order1InOtherFactory = helperInOtherFactory.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderline1InOtherFactory = helperInOtherFactory.CreateWhsOrderLine(order1InOtherFactory, data.Part1, 10m);
			var order2InOtherFactory = helperInOtherFactory.CreateWhsOrder(data.Org1, data.Whs1, "O2");
			var orderline2InOtherFactory = helperInOtherFactory.CreateWhsOrderLine(order2InOtherFactory, data.Part2, 10m);

			var pick1InOtherFactory = helperInOtherFactory.CreatePickNew(order1InOtherFactory);
			var pick2InOtherFactory = helperInOtherFactory.CreatePickNew(order2InOtherFactory);
			var totalPickLineQuantity1InOtherFactory = helperInOtherFactory.GetTotalPickLineQuantity(pick1InOtherFactory);
			var totalPickLineQuantity2InOtherFactory = helperInOtherFactory.GetTotalPickLineQuantity(pick2InOtherFactory);

			AssertEquals("Precondition", 10m, totalPickLineQuantity1InOtherFactory);
			AssertEquals("Precondition", 10m, totalPickLineQuantity2InOtherFactory);

			otherFactory.Save();

			var order1 = Factory.Load<WhsOrder>(order1InOtherFactory.PK);
			var order2 = Factory.Load<WhsOrder>(order2InOtherFactory.PK);
			ApplyApplicator(new[] { order1, order2 }, @"
INFO: Order [HL W00000003] - was successfully finalized.
INFO: Order [HL W00000004] - was successfully finalized.".Trim());
			WhsTestCaseWithFactory.AssertIsFinalisedPrecondition(order1); // this should show as finalised due to data refresh
			WhsTestCaseWithFactory.AssertIsFinalisedPrecondition(order2);

			var query = new ZQuery();
			query.FetchOnlyFromLocalCache = true;
			AssertEquals("No Order or Inventory Lines should be loaded in Operational Actions Factory. Separate factory should be used for finalisation.", 0, Factory.Load<WhsDocketLine>(query).Length);
			AssertEquals("No Pick Lines should be loaded in Operational Actions Factory. Separate factory should be used for finalisation.", 0, Factory.Load<WhsPickLine>(query).Length);
		}

		#endregion

		#region TestApplyApplicator_WithReleaseCapturedAttributes

		public void TestApplyApplicator_WithReleaseCapturedAttributes()
		{
			var otherFactory = new BusinessObjectFactory();
			var helperInOtherFactory = new WhsTestHelperFunctions(otherFactory);
			var data = new TestDataSimpleEnvironment_ForFatDatTestsRequiringDataSetup(otherFactory);
			helperInOtherFactory.SetClientAttributeType(data.Org1, AttributeNumber.One, true);
			helperInOtherFactory.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true, setReleaseCaptured: true);

			helperInOtherFactory.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			helperInOtherFactory.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part2, 10m);

			otherFactory.Save();

			var order1InOtherFactory = helperInOtherFactory.CreateWhsOrder(data.Org1, data.Whs1, data.GetUniqueNameForFatDat(WhsDocketSchema.WD_ExternalReference));
			var orderline1InOtherFactory = helperInOtherFactory.CreateWhsOrderLine(order1InOtherFactory, data.Part1, 10m);
			var order2InOtherFactory = helperInOtherFactory.CreateWhsOrder(data.Org1, data.Whs1, data.GetUniqueNameForFatDat(WhsDocketSchema.WD_ExternalReference));
			var orderline2InOtherFactory = helperInOtherFactory.CreateWhsOrderLine(order2InOtherFactory, data.Part2, 10m);

			var pick1InOtherFactory = helperInOtherFactory.CreatePickNew(order1InOtherFactory);
			var pick2InOtherFactory = helperInOtherFactory.CreatePickNew(order2InOtherFactory);
			var totalPickLineQuantity1InOtherFactory = helperInOtherFactory.GetTotalPickLineQuantity(pick1InOtherFactory);
			var totalPickLineQuantity2InOtherFactory = helperInOtherFactory.GetTotalPickLineQuantity(pick2InOtherFactory);

			AssertEquals("Precondition", 10m, totalPickLineQuantity1InOtherFactory);
			AssertEquals("Precondition", 10m, totalPickLineQuantity2InOtherFactory);

			otherFactory.Save();

			var order1 = Factory.Load<WhsOrder>(order1InOtherFactory.PK);
			var order2 = Factory.Load<WhsOrder>(order2InOtherFactory.PK);
			ApplyApplicator(new[] { order1, order2 }, @"
WARNING: Order [HL W00000003] - could not be auto-finalized. It must be manually finalized.
INFO: Order [HL W00000004] - was successfully finalized.".Trim());
			AssertEquals("Orders should not be finalised, Release Captured Attributes must be entered.", false, order1.IsFinalised);
			AssertEquals("Orders should be finalised.", true, order2.IsFinalised);
		}

		#endregion

		#region TestConcurrency

		protected override bool ShouldRunTestApplicator_ConcurrencyError => false; // Tested more specific use cases below.

		public void TestConcurrency_FinaliseOrderWhileAutoPickTurnedOnAndMeanwhileAnotherUserDetachedTheOrder()
		{
			WarehouseDataRegistry.Instance.AutoFinalizePick.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var factory = new BusinessObjectFactory();
			var helperInOtherFactory = new WhsTestHelperFunctions(factory);
			var data = new TestDataSimpleEnvironment_ForFatDatTestsRequiringDataSetup(factory);
			helperInOtherFactory.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			factory.Save();

			var orderInFactory = helperInOtherFactory.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderlineInFactory = helperInOtherFactory.CreateWhsOrderLine(orderInFactory, data.Part1, 10m);

			var pickInOtherFactory = helperInOtherFactory.CreatePickNew(orderInFactory);

			var applicator = new FinaliseOrdersActionMethodApplicator(Factory);
			applicator.UserModifyDataInDBAction += (f) =>
			{
				var otherFactory = new BusinessObjectFactory() { RefreshEnabled = false };
				var newPick = otherFactory.Load<WhsPick>(pickInOtherFactory.PK);
				newPick.Orders.RemoveAll();
				otherFactory.Save();
			};

			var orders = new[] { orderInFactory };
			var log = new DummyOperationalActionSectionLog();

			AssertNoExceptionThrown("Should not throw exception", () => applicator.Apply(log, orders));

			AssertEquals($"WARNING: Order [HL W00000002] - While you were editing your data, another user modified it. Your changes cannot be saved because they may conflict with the other user's changes. Please close and open this form to try again.",
				log.MessagesString().Trim());
		}

		public void TestConcurrency_FinaliseOrderWhileAutoPickTurnedOnAndMeanwhileAnotherUserAttachedAnotherOrder()
		{
			WarehouseDataRegistry.Instance.AutoFinalizePick.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var factory = new BusinessObjectFactory();
			var helperInOtherFactory = new WhsTestHelperFunctions(factory);
			var data = new TestDataSimpleEnvironment_ForFatDatTestsRequiringDataSetup(factory);
			helperInOtherFactory.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			factory.Save();

			var orderInFactory = helperInOtherFactory.CreateWhsOrder(data.Org1, data.Whs1, data.GetUniqueNameForFatDat(WhsDocketSchema.WD_ExternalReference));
			var orderlineInFactory = helperInOtherFactory.CreateWhsOrderLine(orderInFactory, data.Part1, 1m);

			var order2InFactory = helperInOtherFactory.CreateWhsOrder(data.Org1, data.Whs1, data.GetUniqueNameForFatDat(WhsDocketSchema.WD_ExternalReference));
			var orderline2InFactory = helperInOtherFactory.CreateWhsOrderLine(order2InFactory, data.Part1, 1m);

			factory.Save();
			var pickInOtherFactory = helperInOtherFactory.CreatePickNew(orderInFactory);

			var applicator = new FinaliseOrdersActionMethodApplicator(Factory);
			applicator.UserModifyDataInDBAction += (f) =>
			{
				var otherFactory = new BusinessObjectFactory() { RefreshEnabled = false };
				var newPick = otherFactory.Load<WhsPick>(pickInOtherFactory.PK);
				var order2 = otherFactory.Load<WhsOrder>(order2InFactory.PK);
				newPick.Orders.Add(order2);
				otherFactory.Save();
			};

			var orders = new[] { orderInFactory };
			var log = new DummyOperationalActionSectionLog();

			AssertNoExceptionThrown("Should not throw exception", () => applicator.Apply(log, orders));

			AssertEquals("WARNING: Order [HL W00000002] - While you were editing your data, another user modified it. Your changes cannot be saved because they may conflict with the other user's changes. Please close and open this form to try again.",
				log.MessagesString().Trim());
		}

		public void TestConcurrency_FinaliseOrderWhileInventoryWasChanged()
		{
			WarehouseDataRegistry.Instance.AutoFinalizePick.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var data = new TestDataSimpleEnvironment_ForFatDatTestsRequiringDataSetup(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", ZDateTimeOffset.Today, data.Part1, 10m, data.Whs1.DefaultLocation, "");
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderline = Helper.CreateWhsOrderLine(order, data.Part1, 1m);

			var pick = Helper.CreatePickNew(order);
			Factory.Save();

			Applicator.UserModifyDataInDBAction += (f) =>
			{
				var otherFactory = new BusinessObjectFactory() { RefreshEnabled = false };
				var helperForOtherFactory = new WhsTestHelperFunctions(otherFactory);
				var adjustment = helperForOtherFactory.CreateWhsAdjustment(data.Org1, data.Whs1, "ADJ");
				var adjustmentLine = helperForOtherFactory.CreateWhsAdjustmentLine(adjustment, data.Part1, -9m, data.Whs1.DefaultLocation);
				adjustment.FinaliseDocketWithoutUserConfirmation();

				otherFactory.Save();

				WhsTestCaseWithFactory.AssertIsFinalisedPrecondition(adjustment);
			};

			var expectedMessage = "WARNING: Order [HL W00000002] - While you were editing your data, another user modified it. Your changes cannot be saved because they may conflict with the other user's changes. Please close and open this form to try again.";
			AssertNoExceptionThrown("Should not throw exception", () =>
			{
				using (WarehouseDataRegistry.Instance.AutoFinalizePick.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
				{
					ApplyApplicator(new[] { order }, expectedMessage);
				}
			});
		}

		#endregion

		#region TestZSaveExceptionError

		public void TestZSaveExceptionError()
		{
			var data = new TestDataSimpleEnvironment_ForFatDatTestsRequiringDataSetup(Factory);

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 100m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderline = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var orderLine = order.Lines[0];
			var pick = Helper.CreatePickNew(order);
			Factory.Save();

			Applicator.UserModifyDataInDBAction += (factory) =>
			{
				var orderLineInFactory = factory.Load<WhsOrderLine>(orderLine.PK);
				orderLineInFactory.WE_TransactionQuantity = 9m;
			};

			var expectedLogMessage = @"WARNING: Order [HL W00000002] - Attempt to set Transaction and Pick qty out of sync by the current process.";
			using (WarehouseDataRegistry.Instance.AutoFinalizePick.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				ApplyApplicator(new[] { order }, expectedLogMessage);
			}
		}

		#endregion

		#region Implementation
		protected override WhsOrder GetNewDocket(WhsTestHelperFunctions helper, OrgHeader client, WhsWarehouse warehouse, string reference = "1")
		{
			return helper.CreateWhsOrder(client, warehouse, reference);
		}

		protected override WhsDocketLine GetNewDocketLine(WhsTestHelperFunctions helper, WhsOrder docket, OrgSupplierPart part, ZDecimal quantity)
		{
			return helper.CreateWhsOrderLine(docket, part, quantity);
		}

		protected override void PrepareDocketsForFinalisation(WhsTestHelperFunctions helper, params WhsOrder[] dockets)
		{
			helper.CreatePickNew(dockets);
		}

		#endregion

		new FinaliseOrdersActionMethodApplicator Applicator => (FinaliseOrdersActionMethodApplicator)base.Applicator;
	}
}
