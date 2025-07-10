using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.Services.OperationalActions.Support.Testing;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Module.Testing
{
	[TestedType(typeof(FinalisePicksActionMethodApplicator))]
	public class FinalisePicksActionMethodApplicatorTest : OperationalActionMethodApplicatorTest
	{
		#region TestApplyApplicator

		#region TestApplyApplicator_AutomaticallyFinaliseOrders

		public void TestApplyApplicator_AutomaticallyFinaliseOrdersRegistryItemTurnedOff()
		{
			AssertOperationalAction(false);
		}

		public void TestApplyApplicator_AutomaticallyFinaliseOrdersRegistryItemTurnedOn()
		{
			AssertOperationalAction(true);
		}

		void AssertOperationalAction(bool shouldAutoFinaliseOrders)
		{
			WarehouseDataRegistry.Instance.AutoFinalizeOrders.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, shouldAutoFinaliseOrders);

			var data = new TestDataSimpleEnvironment(Factory);
			var receive1 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);

			var order_NotFinalised = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 1m);
			var order_Finalised1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O2", data.Part1, 2m);
			var order_Finalised2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O3", data.Part1, 3m);
			var order_Finalised3 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O4", data.Part1, 4m);
			Factory.Save();

			var finaliseOrder = true;
			var finalisePick = true;
			var pick_Canceled = Factory.New<WhsPick>();
			var pick_WithoutOrders = Factory.New<WhsPick>();
			var pick_WithUnfinalisedOrder = Helper.CreatePickNew(!finaliseOrder, !finalisePick, order_NotFinalised);
			var pick_WithFinalisedOrderAndFinaliseError = Helper.CreatePickNew(finaliseOrder, !finalisePick, order_Finalised1);
			var pick_CanBeFinalised = Helper.CreatePickNew(finaliseOrder, !finalisePick, order_Finalised2);
			var pick_AlreadyFinalised = Helper.CreatePickNew(finaliseOrder, finalisePick, order_Finalised3);
			pick_Canceled.WP_PickStatus = PickStatus.Codes.Cancelled;

			// Orders preconditions
			AssertEquals("Precondition", false, order_NotFinalised.IsFinalised);
			WhsTestCaseWithFactory.AssertIsFinalisedPrecondition(order_Finalised1);
			WhsTestCaseWithFactory.AssertIsFinalisedPrecondition(order_Finalised2);
			WhsTestCaseWithFactory.AssertIsFinalisedPrecondition(order_Finalised3);

			// Picks preconditions
			AssertEquals("Precondition", true, pick_Canceled.IsCancelled);
			AssertEquals("Precondition", 0, pick_WithoutOrders.Orders.Count);
			AssertEquals("Precondition", true, pick_WithUnfinalisedOrder.Orders.Cast<WhsPickableDocket>().All(o => !o.IsFinalised));
			AssertEquals("Precondition", true, pick_CanBeFinalised.Orders.Cast<WhsPickableDocket>().All(o => o.IsFinalised));
			WhsTestCaseWithFactory.AssertIsFinalisedPrecondition(pick_AlreadyFinalised);

			order_Finalised1.ConsigneePK = CargoWise.Types.ZGuid.Empty; // Hack to cause validation failure on finalising pick
			Factory.Save();

			var picks = new[] { pick_Canceled, pick_WithoutOrders, pick_WithUnfinalisedOrder, pick_WithFinalisedOrderAndFinaliseError, pick_CanBeFinalised, pick_AlreadyFinalised };
			if (!shouldAutoFinaliseOrders)
			{
				ApplyApplicator(picks, @"
WARNING: Pick P00000001 [HL P00000001] - is canceled and cannot be finalized.
WARNING: Pick P00000002 [HL P00000002] - has no Orders attached and cannot be finalized.
WARNING: Pick P00000003 [HL P00000003] - has non-finalized Orders. Finalize the Orders before trying to finalize the Pick.
WARNING: Release P00000004 [HL P00000004] - could not be auto-finalized. It must be manually finalized.
Failed to Finalize Pick.
Error - OrganisationPK: The Consignee has no address entered
Error - OrganisationNameOrPK: The Consignee has no address entered
INFO: Release P00000005 [HL P00000005] - was successfully finalized.
WARNING: Pick P00000006 [HL P00000006] - is already finalized.".Trim());
			}
			else
			{
				ApplyApplicator(picks, @"
WARNING: Pick P00000001 [HL P00000001] - is canceled and cannot be finalized.
WARNING: Pick P00000002 [HL P00000002] - has no Orders attached and cannot be finalized.
INFO: Release P00000003 [HL P00000003] - was successfully finalized.
WARNING: Release P00000004 [HL P00000004] - could not be auto-finalized. It must be manually finalized.
Failed to Finalize Pick.
Error - OrganisationPK: The Consignee has no address entered
Error - OrganisationNameOrPK: The Consignee has no address entered
INFO: Release P00000005 [HL P00000005] - was successfully finalized.
WARNING: Pick P00000006 [HL P00000006] - is already finalized.".Trim());
			}
		}

		#endregion

		#region TestApplyApplicator_AutomaticallyFinaliseOrdersRegistryItemTurnedOn_OrdersWithErrors

		public void TestApplyApplicator_AutomaticallyFinaliseOrdersRegistryItemTurnedOn_OrdersWithErrors()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, PartAttributeTypeList.Codes.VIN);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true, true);
			Factory.Save();

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 3m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part2, 3m);
			Factory.Save();

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 2m);
			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O2", data.Part2, 2m);

			WarehouseDataRegistry.Instance.AutoFinalizeOrders.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var pick = Helper.CreatePickNew(order1, order2);

			Assert("Precondition", !order1.IsFinalised);
			Assert("Precondition", !order2.IsFinalised);
			Assert("Precondition", !pick.IsFinalised);

			Factory.Save();

			var picks = new[] { pick };
			ApplyApplicator(picks, @"
WARNING: Release P00000001 [HL P00000001] - could not be auto-finalized. It must be manually finalized.
Failed to Finalize Pick.
Error Message: Please finalize all the orders on the pick before finalizing the pick.
Order Line Product: P1
Error - PartAttribute1: Please enter a Part Attrib. 1.
".Trim());
		}

		#endregion

		#region TestApplyApplicator_AutomaticallyFinaliseOrders_WithReleaseCapturedAttribs

		public void TestApplyApplicator_AutomaticallyFinaliseOrders_WithReleaseCapturedAttribs()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true, setReleaseCaptured: true);
			Factory.Save();

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 4m);
			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O2", data.Part1, 6m);

			using (WarehouseDataRegistry.Instance.AutoFinalizeOrders.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var pick = Helper.CreatePickNew(order1, order2);
				Assert("Precondition", !order1.IsFinalised);
				Assert("Precondition", !order2.IsFinalised);
				Assert("Precondition", !pick.IsFinalised);

				var picks = new[] { pick };
				ApplyApplicator(picks, @"
WARNING: Release P00000001 [HL P00000001] - could not be auto-finalized. It must be manually finalized.
Failed to Finalize Pick.
Error Message: Please finalize all the orders on the pick before finalizing the pick.
Order Line Product: P1
Order Line Product: P1
Error - PartAttribute1: Please enter a Part Attrib. 1.
".Trim());
			}
		}

		#endregion

		#region TestApplyApplicator_AutomaticallyFinaliseOrders_WithReleaseCapturedAttribs_WhenOrdersAreFinalised

		public void TestApplyApplicator_AutomaticallyFinaliseOrders_WithReleaseCapturedAttribs_WhenOrdersAreFinalised()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true, setReleaseCaptured: true);
			Factory.Save();

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 4m);
			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O2", data.Part1, 6m);

			using (WarehouseDataRegistry.Instance.AutoFinalizeOrders.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var pick = Helper.CreatePickNew(order1, order2);
				Assert("Precondition", !order1.IsFinalised);
				Assert("Precondition", !order2.IsFinalised);
				Assert("Precondition", !pick.IsFinalised);

				var releaseLine1 = order1.Lines[0].ReleaseLines[0];
				releaseLine1.PartAttribute1 = "RED";
				var releaseLine2 = order2.Lines[0].ReleaseLines[0];
				releaseLine2.PartAttribute1 = "BLUE";

				pick.FinaliseAllOrders();
				WhsTestCaseWithFactory.AssertIsFinalisedPrecondition(order1);
				WhsTestCaseWithFactory.AssertIsFinalisedPrecondition(order2);

				releaseLine1.PartAttribute1 = "GREEN";
				releaseLine1.Quantity = 1m;
				releaseLine2.PartAttribute1 = "YELLOW";
				releaseLine2.Quantity = 1m;
				Factory.Save(); // In practice, this should fail due to row errors on the pick

				var picks = new[] { pick };
				ApplyApplicator(picks, @"
WARNING: Release P00000001 [HL P00000001] - could not be auto-finalized. It must be manually finalized.
Failed to Finalize Pick.
Order Line Product: P1
Order Line Product: P1
Error - PartAttribute1: Please enter a Part Attrib. 1.
".Trim());
			}
		}

		#endregion

		#region TestApplyApplicator_EachPickIsFinalisedInSeparateFactory

		public void TestApplyApplicator_EachPickIsFinalisedInSeparateFactory()
		{
			var otherFactory = new BusinessObjectFactory();
			var helper = new WhsTestHelperFunctions(otherFactory);
			var data = new TestDataSimpleEnvironment(otherFactory);
			helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part2, 10m);
			otherFactory.Save();

			var order1InOtherFactory = helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 10m);
			var order2InOtherFactory = helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O2", data.Part2, 10m);
			var pick1InOtherFactory = helper.CreatePickNew(order1InOtherFactory);
			var pick2InOtherFactory = helper.CreatePickNew(order2InOtherFactory);
			var totalPickLineQuantity1InOtherFactory = helper.GetTotalPickLineQuantity(pick1InOtherFactory);
			var totalPickLineQuantity2InOtherFactory = helper.GetTotalPickLineQuantity(pick2InOtherFactory);
			AssertEquals("Precondition", 10m, totalPickLineQuantity1InOtherFactory);
			AssertEquals("Precondition", 10m, totalPickLineQuantity2InOtherFactory);

			pick1InOtherFactory.FinaliseAllOrders();
			pick2InOtherFactory.FinaliseAllOrders();
			WhsTestCaseWithFactory.AssertIsFinalisedPrecondition(order1InOtherFactory);
			WhsTestCaseWithFactory.AssertIsFinalisedPrecondition(order2InOtherFactory);
			otherFactory.Save();

			var pick1 = Factory.Load<WhsPick>(pick1InOtherFactory.PK);
			var pick2 = Factory.Load<WhsPick>(pick2InOtherFactory.PK);
			ApplyApplicator(new[] { pick1, pick2 }, @"
INFO: Release P00000001 [HL P00000001] - was successfully finalized.
INFO: Release P00000002 [HL P00000002] - was successfully finalized.".Trim());
			WhsTestCaseWithFactory.AssertIsFinalisedPrecondition(pick1); // this should show as finalised due to data refresh
			WhsTestCaseWithFactory.AssertIsFinalisedPrecondition(pick2);

			var query = new ZQuery();
			query.FetchOnlyFromLocalCache = true;
			AssertEquals("No Order or Inventory Lines should be loaded in Operational Actions Factory. Separate factory should be used for finalisation.", 0, Factory.Load<WhsDocketLine>(query).Length);
			AssertEquals("No Pick Lines should be loaded in Operational Actions Factory. Separate factory should be used for finalisation.", 0, Factory.Load<WhsPickLine>(query).Length);
		}

		#endregion

		#region PerformanceTesting_ApplyApplicator_PerformanceOfFinalising20PicksAnd400Orders

		public void TestDbHits_ApplyApplicator_PerformanceOfFinalising10PicksAnd100Orders()
		{
			var numberOfInventories = 22;
			var numberOfPicks = 10;
			var numberOfOrdersPerPick = 6;
			var otherFactory = new BusinessObjectFactory();
			var helper = new WhsTestHelperFunctions(otherFactory);
			var data = new TestDataSimpleEnvironment_ForFatDatTestsRequiringDataSetup(otherFactory);
			var receive1InOtherFactory = helper.CreateWhsReceive(data.Org1, data.Whs1, "R35", Helper.Notify);

			for (var i = 0; i < numberOfInventories; i++)
			{
				helper.CreateWhsReceiveInventoryLine(receive1InOtherFactory, data.Part1, 10m, data.Whs1.DefaultLocation);
			}

			receive1InOtherFactory.FinaliseDocket();
			AssertEquals(true, receive1InOtherFactory.IsFinalised);
			otherFactory.Save();

			for (var i = 0; i < numberOfPicks; i++)
			{
				var listOfOrders = new List<WhsOrder>();
				for (var j = 0; j < numberOfOrdersPerPick; j++)
				{
					listOfOrders.Add(helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, $"O{i}{j + 25}", data.Part1, 2m));
				}
				helper.CreatePickNew(listOfOrders.ToArray());
			}
			otherFactory.Save();

			var expectedDbHits = new Dictionary<string, int>()
			{
				{ WhsDocketSchema.Constants.TableName, 10 },
				{ WhsPickSchema.Constants.TableName, 1 },
			};

			var newFactory = new BusinessObjectFactory();
			var picksInNewFactory = newFactory.Load<WhsPick>(new ZQuery());

			using (WarehouseDataRegistry.Instance.AutoFinalizeOrders.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (AssertDbHitsWithUsefulQueryInformation(expectedDbHits, newFactory))
			{
				SimulateRun(picksInNewFactory, saveOnSuccess: false);
			}
		}

		#endregion

		#endregion

		#region Implementation

		protected WhsTestHelperFunctions Helper => helper ?? (helper = new WhsTestHelperFunctions(Factory));
		WhsTestHelperFunctions helper;

		#endregion

		#region TestConcurrencyError

		public void TestWhsDocketLineConcurrencyError()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order_NotFinalised = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 2m);
			var order_PickFinalised = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O2", data.Part1, 3m);

			var pick_First = Helper.CreatePickNew(order_NotFinalised);
			var pick_Second = Helper.CreatePickNew(order_PickFinalised);
			pick_First.FinaliseOrder(order_NotFinalised);
			Factory.Save();

			var newFactory = new BusinessObjectFactory { RefreshEnabled = false };
			var pickInNewFactory = newFactory.Load<WhsPick>(pick_Second.PK);
			var orderInNewFactory = newFactory.Load<WhsOrder>(order_PickFinalised.PK);

			Applicator.FinalizeOrderAndPickForTest += () =>
			{
				pickInNewFactory.FinaliseOrder(orderInNewFactory);
				pickInNewFactory.FinalisePick();
				newFactory.Save();
			};

			ApplyApplicator(new[] { pick_First }, @"INFO: Release P00000001 [HL P00000001] - was successfully finalized.
WARNING: Release P00000001 [HL P00000001] - While you were editing your data, another user modified it. Your changes cannot be saved because they may conflict with the other user's changes. Please try running this operational action again.".Trim());
		}

		#endregion

		#region TestCannotSaveExceptionError

		public void TestCannotSaveExceptionError()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 100m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);
			pick.FinaliseOrder(order);

			Factory.Save();

			var newFactory = new BusinessObjectFactory { RefreshEnabled = false };
			var pickInNewFactory = newFactory.Load<WhsPick>(pick.PK);
			var orderInNewFactory = newFactory.Load<WhsOrder>(order.PK);

			Applicator.FinalizeOrderAndPickForTest += () =>
			{
				pickInNewFactory.FinalisePick();
				newFactory.Save();
			};

			var expectedLogMessage = @"INFO: Release P00000001 [HL P00000001] - was successfully finalized.
WARNING: Release P00000001 [HL P00000001] - While you were editing your data, another user modified it. Your changes cannot be saved because they may conflict with the other user's changes. Please try running this operational action again.";
			AssertNoExceptionThrown(() =>
			{
				ApplyApplicator(new[] { pick }, expectedLogMessage);
			});
		}

		#endregion

		#region TestZSaveExceptionError

		public void TestZSaveExceptionError()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 100m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			var orderLine = order.Lines[0];
			var pick = Helper.CreatePickNew(order);
			pick.FinaliseOrder(order);
			Factory.Save();

			Applicator.SetupOrderAndPickWithFactoryForTest += (factory) =>
			{
				var orderLineInFactory = factory.Load<WhsOrderLine>(orderLine.PK);
				orderLineInFactory.WE_TransactionQuantity = 9m;
			};

			var expectedLogMessage = @"INFO: Release P00000001 [HL P00000001] - was successfully finalized.
WARNING: Release P00000001 [HL P00000001] - Attempt to set Transaction and Pick qty out of sync by the current process.";

			ApplyApplicator(new[] { pick }, expectedLogMessage);
		}

		#endregion

		new FinalisePicksActionMethodApplicator Applicator => (FinalisePicksActionMethodApplicator)base.Applicator;
	}
}
