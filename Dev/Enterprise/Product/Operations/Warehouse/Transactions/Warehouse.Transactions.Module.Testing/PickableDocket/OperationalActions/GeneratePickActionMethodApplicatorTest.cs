using System.Linq;
using CargoWise.Application;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.ProductionRules.Business;
using Enterprise.Services.OperationalActions.Support.Testing;
using Enterprise.Warehouse.Environment.Business.Testing;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Module.Testing
{
	[TestedType(typeof(GeneratePickActionMethodApplicator))]
	public class GeneratePickActionMethodApplicatorForOrderTest : GeneratePickActionMethodApplicatorTest<WhsOrder>
	{
		protected override ZString GetNoStockErrorMsg(WhsOrder docket)
		{
			return string.Format("ERROR: Attempted Picking of {0} [HL {1}] failed -- No Stock was allocated.\n\r", docket.Description, docket.WD_DocketID);
		}

		#region TestPickOrders_ConcurrencyTest

		public void TestPickOrders_ConcurrencyTest()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var newUser = Helper.CreateGlbStaff("S1", "S1");
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", new TestNotificationBuffer());
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 5m, data.Whs1.DefaultLocation);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 5m, data.Whs1.DefaultLocation);
			receive.FinaliseDocket();
			Factory.Save();
			WhsTestCaseWithFactory.AssertIsFinalisedPrecondition(receive);

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			Helper.CreateWhsOrderLine(order, data.Part1, 1m);
			Factory.Save();
			AssertEquals(false, order.HasChanges);

			order.WD_UnitsSent = 2m;
			AssertEquals(true, order.HasChanges);

			var applictor = new GeneratePickActionMethodApplicator();
			DummyOperationalActionSectionLog log = new DummyOperationalActionSectionLog();

			applictor.InitialiseBeforeIndividiualBatchRun();
			applictor.NewFactorySaving += delegate
			{
				CargoWise.Database.TestFramework.ObjectModel.WhsDocketLine.DeleteInDB(Db.Connection, order.Lines[0].PK.ToGuid());
			};

			using (ObjectFactory.Substitute<IAllocationEngineManager>(new AllocateFIFOLegacyMock()))
			{
				applictor.Apply(log, new[] { order });
			}
			log.Verify();

			if (applictor.SupportsSummary)
			{
				log.messages.Add("<-- Summary -->");
				applictor.SummaryLog(log);
			}

			AssertMultilineASCIIEquals("", string.Format("ERROR: Attempted Picking of Order [HL {0}] failed -- Order: {0} was modified by another user while you are running operational action. Please run the operational action on this order again.",
				order.WD_DocketID), log.MessagesString());
		}

		#endregion
	}

	[TestedType(typeof(GeneratePickActionMethodApplicator))]
	public class PickableDocketGeneratePickActionMethodApplicatorForWorkOrderTest : GeneratePickActionMethodApplicatorTest<WhsWorkOrder>
	{
		protected override ZString GetNoStockErrorMsg(WhsWorkOrder docket)
		{
			return string.Format("ERROR: Attempted Picking of {0} [HL {1}] failed -- No Stock can be allocated to this {0} due to shortfalls.\n\r", docket.Description, docket.WD_DocketID);
		}
	}

	public abstract class GeneratePickActionMethodApplicatorTest<TDocket> : OperationalActionMethodApplicatorTest
		where TDocket : WhsPickableDocket
	{
		#region TestAction

		public void TestAction()
		{
			Data.CreateBOMProducts();
			Data.CreateProductInInventory("Bike Wheel", Data.BOM.BikeWheel, 10); // order will pick these
			Data.CreateProductInInventory("Wheel Rim", Data.BOM.WheelRim, 8);
			Data.CreateProductInInventory("Wheel Tyre", Data.BOM.WheelTyre, 8);
			Data.CreateProductInInventory("Polish", Data.BOM.Polish, 8);
			Factory.Save();

			var order_ENT = CreateOrderWithLine("Docket OK", DocketStatus.Codes.Entered, Data.BOM.BikeWheel, 1m);
			var order_ENT_NoLines = CreateOrder("Docket NoLines", DocketStatus.Codes.Entered);
			var order_ENT_NoStock = CreateOrderWithLine("Docket NoStock", DocketStatus.Codes.Entered, Data.BOM.BikeEngine, 1m);
			var order_ATP = CreateOrderWithLine("Docket ATP/PIC", DocketStatus.Codes.Entered, Data.BOM.BikeWheel, 1m);
			Helper.CreatePickNew(order_ATP);
			order_ATP.WD_DocketStatus = order_ATP.WD_DocketType == DocketType.Codes.WorkOrder ? DocketStatus.Codes.Picking : DocketStatus.Codes.AttachedToPick;
			Factory.Save();

			var order_PIC = CreateOrderWithLine("Docket PIC", DocketStatus.Codes.Entered, Data.BOM.BikeWheel, 1m);
			Helper.CreatePickNew(order_PIC);
			order_PIC.WD_DocketStatus = DocketStatus.Codes.Picking;
			Factory.Save();

			var order_FIN = CreateOrderWithLine("Docket FIN", DocketStatus.Codes.Entered, Data.BOM.BikeWheel, 1m);
			var order_CAN = CreateOrderWithLine("Docket CAN", DocketStatus.Codes.Cancelled, Data.BOM.BikeWheel, 1m);
			order_CAN.AllLines.ToList().ForEach(l => l.WE_DocketLineStatus = DocketLineStatus.Codes.Cancelled);
			var order_ENT_ManPick = CreateOrderWithLine("Docket Man", DocketStatus.Codes.Entered, Data.BOM.BikeWheel, 1m);
			var order_ENT_ManWithAutoAllocatePick = CreateOrderWithLine("Docket Man+Auto", DocketStatus.Codes.Entered, Data.BOM.BikeWheel, 1m);
			Factory.Save();

			var pick = Helper.CreatePickNew(finaliseOrders: true, finalisePick: false, pickableDockets: order_FIN);
			order_ENT_ManPick.WD_PickOption = WhsPickOption.Codes.Manual;
			order_ENT_ManWithAutoAllocatePick.WD_PickOption = WhsPickOption.Codes.ManualWithAutoAllocate;

			var orders = new[]
			{
				order_ENT,
				order_ENT_NoLines,
				order_ENT_NoStock,
				order_ATP,
				order_PIC,
				order_FIN,
				order_CAN,
				order_ENT_ManPick,
				order_ENT_ManWithAutoAllocatePick
			};

			var orderExpectedLog = string.Format(
				"ERROR: Attempted Picking of {0} [HL Docket NoLines] failed -- This {0} has no Lines.\n\r" +
				GetNoStockErrorMsg(order_ENT_NoStock) +
				"ERROR: {0} [HL Docket ATP/PIC] is Attached To Pick (ATP).\n\r" +
				"ERROR: {0} [HL Docket PIC] is Picking (PIC).\n\r" +
				"ERROR: {0} [HL Docket FIN] is Attached To Pick (ATP).\n\r" +
				"ERROR: {0} [HL Docket CAN] is Canceled (CAN).\n\r" +
				"ERROR: {0} [HL Docket Man] does not have Pick Option AUT.\n\r" +
				"ERROR: {0} [HL Docket Man+Auto] does not have Pick Option AUT.",
				order_ENT.Description);

			var workOrderExpectedLog = string.Format(
				"ERROR: Attempted Picking of {0} [HL Docket NoLines] failed -- This {0} has no Lines.\n\r" +
				GetNoStockErrorMsg(order_ENT_NoStock) +
				"ERROR: {0} [HL Docket ATP/PIC] is Picking (PIC).\n\r" +
				"ERROR: {0} [HL Docket PIC] is Picking (PIC).\n\r" +
				"ERROR: {0} [HL Docket FIN] is Finalized (FIN).\n\r" +
				"ERROR: {0} [HL Docket CAN] is Canceled (CAN).\n\r" +
				"ERROR: {0} [HL Docket Man] does not have Pick Option AUT.\n\r" +
				"ERROR: {0} [HL Docket Man+Auto] does not have Pick Option AUT.",
				order_ENT.Description);

			var expectedLog = order_ENT.WD_DocketType == DocketType.Codes.WorkOrder ? workOrderExpectedLog : orderExpectedLog;
			using (ObjectFactory.Substitute<IAllocationEngineManager>(new AllocateFIFOLegacyMock()))
			{
				ApplyApplicator(orders, expectedLog);
			}
			AssertNotNull(order_ENT.Pick);
			AssertNull(order_ENT_NoLines.Pick);
			AssertNull(order_ENT_NoStock.Pick);
			AssertNotNull(order_ATP.Pick);
			AssertNotNull(order_PIC.Pick);
			AssertEquals(pick, order_FIN.Pick); // Finalised order must have been picked to be finalised. Assert no new pick created.
			AssertNull(order_CAN.Pick);
			AssertNull(order_ENT_ManPick.Pick);
			AssertNull(order_ENT_ManWithAutoAllocatePick.Pick);
		}

		protected abstract ZString GetNoStockErrorMsg(TDocket docket);

		protected TDocket CreateOrder(ZString id, ZString status)
		{
			var docket = Factory.New<TDocket>();
			docket.WD_OH_Client = Data.Org1.PK;
			docket.WD_WW_Whs = Data.Whs1.PK;
			docket.WD_DocketID = id;
			docket.WD_DocketStatus = status;
			docket.WD_RequiredDate = ZDateTimeOffset.Now;
			docket.ConsigneeAddressPK = Data.Org1.MainAddress.PK;

			return docket;
		}

		protected TDocket CreateOrderWithLine(ZString id, ZString status, OrgSupplierPart part, ZDecimal units)
		{
			var docket = CreateOrder(id, status);
			Helper.CreateWhsPickableDocketLine(docket, part, units);

			return docket;
		}

		#endregion

		#region TestAction_GeneratesPickUsingOrdersPickPriority

		public void TestAction_GeneratesPickUsingOrdersPickPriority()
		{
			Data.CreateBOMProducts();
			Data.CreateProductInInventory("Bike Wheel", Data.BOM.BikeWheel, 2); // orders will pick wheels
			Data.CreateProductInInventory("Wheel Rim", Data.BOM.WheelRim, 2);   //
			Data.CreateProductInInventory("Wheel Tyre", Data.BOM.WheelTyre, 2); //
			Data.CreateProductInInventory("Polish", Data.BOM.Polish, 2);        // work orders will pick rims + tyres + polish
			Factory.Save();

			var order2 = CreateOrderWithLine("Docket 2", DocketStatus.Codes.Entered, Data.BOM.BikeWheel, 1m);
			var order3 = CreateOrderWithLine("Docket 3", DocketStatus.Codes.Entered, Data.BOM.BikeWheel, 1m);
			var order1 = CreateOrderWithLine("Docket 1", DocketStatus.Codes.Entered, Data.BOM.BikeWheel, 1m);
			order1.Lines[0].ClearWE_ShortfallQuantityCached();
			order2.Lines[0].ClearWE_ShortfallQuantityCached();
			order3.Lines[0].ClearWE_ShortfallQuantityCached();

			order1.WD_PickPriority = 1;
			order2.WD_PickPriority = 2;
			order3.WD_PickPriority = 3;
			Factory.Save();

			using (ObjectFactory.Substitute<IAllocationEngineManager>(new AllocateFIFOLegacyMock()))
			{
				ApplyApplicator(new[] { order1, order2, order3 }, GetNoStockErrorMsg(order3));
			}
			AssertEquals("P00000001", order1.Pick.WP_PickNo);
			AssertEquals("P00000002", order2.Pick.WP_PickNo);
			AssertNull("Order 3 should have no Pick, because it was processed last and there was no stock left.", order3.Pick);
		}

		#endregion

		#region TestGenerateErrorMessageForPick

		public void TestGenerateErrorMessageForPick()
		{
			Data.CreateBOMProducts();
			Data.CreateProductInInventory("Bike Wheel", Data.BOM.BikeWheel, 10); // order will pick these
			Data.CreateProductInInventory("Wheel Rim", Data.BOM.WheelRim, 8);
			Data.CreateProductInInventory("Wheel Tyre", Data.BOM.WheelTyre, 8);
			Data.CreateProductInInventory("Polish", Data.BOM.Polish, 8);

			var partWithExcessiveWeight = Data.BOM.BikeWheel;
			var partWithExcessiveVolume = Data.BOM.WheelRim;
			Helper.SetProductWeightAndVolume(partWithExcessiveWeight, 999999.99m, Constants.Weight.Kilograms, 0.2m, Constants.Volume.CubicMetres);
			Helper.SetProductWeightAndVolume(Data.BOM.WheelTyre, 999999.99m, Constants.Weight.Kilograms, 0.2m, Constants.Volume.CubicMetres);
			Helper.SetProductWeightAndVolume(partWithExcessiveVolume, 99.0m, Constants.Weight.Kilograms, 999999.99m, Constants.Volume.CubicMetres);
			Factory.Save();

			var orderWithExcessiveWeight = CreateOrderWithLine("Docket OK", DocketStatus.Codes.Entered, Data.BOM.BikeWheel, 5m);

			var orders = new TDocket[]
			{
				orderWithExcessiveWeight
			};

			string expectedLogText = string.Empty;
			if (orders[0] is WhsWorkOrder)
			{
				// Work Orders does not use Weight/Volume Sent
				expectedLogText = string.Format(
				@"ERROR: Attempted Picking of {1} [HL Docket OK] failed -- Please check your Order Lines.
  {0} Volume: The number 5,000,001.05 is too large, the maximum value allowed for Volume is 999,999.999.
  {0} Weight: The number 5,000,504.95 is too large, the maximum value allowed for Weight is 999,999.999.

", (char)8226, orderWithExcessiveWeight.Description);
			}
			else if (orders[0] is WhsOrder)
			{
				expectedLogText = string.Format(
				@"ERROR: Attempted Picking of {1} [HL Docket OK] failed -- Please check your order lines. One or multiple products have excessive Weight / Volume.
To fix the Weight / Volume go to Maintain > Warehouse > Products and amend the gross Weight / Volume for these products.
  {0} Weight: The number 4,999,999.95 is too large, the maximum value allowed for Weight is 999,999.999.
  {0} Weight Sent: The number 4,999,999.95 is too large, the maximum value allowed for Weight Sent is 999,999.999.
  {0} Net Weight: The number 4,999,999.95 is too large, the maximum value allowed for Net Weight is 999,999.999.

", (char)8226, orderWithExcessiveWeight.Description);
			}

			using (ObjectFactory.Substitute<IAllocationEngineManager>(new AllocateFIFOLegacyMock()))
			{
				ApplyApplicator(orders, expectedLogText);
			}
		}

		#endregion

		#region TestGenerateErrorMessageForPick_NoAllocationRules

		public void TestGenerateErrorMessageForPick_NoAllocationRules()
		{
			var warehouse1 = Helper.CreateWarehouse("WHS1", "Test Warehouse 1");
			var client = Helper.CreateClient("CQTC", "QTC Client", "QTC", "WKY");
			var product1 = Helper.CreateProduct(client, "P1");

			var receive = Helper.CreateWhsReceiveWithInventory(client, warehouse1, "R1", product1, 5);
			receive.FinaliseDocketWithoutUserConfirmation();
			WhsTestCaseWithFactory.AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(client, warehouse1, "O1", product1, 6);
			Factory.Save();

			var allocationRuleSetQuery = new ZQuery(ProductionRuleSetSchema.PRS_Context, "PWA");
			allocationRuleSetQuery.AddToFilter(ProductionRuleSetSchema.PRS_IsLive, true);

			var allocationRuleSet = Factory.LoadTop1<ProductionRuleSet>(allocationRuleSetQuery);
			allocationRuleSet.PRS_IsLive = false;
			Factory.Save();

			var expectedLogText = @"ERROR: Attempted Picking of Order [HL W00000002] failed -- No Stock was allocated. Issue occurred during allocation: No rules found for context: 'PWA'";

			ApplyApplicator(new[] { order }, expectedLogText);
		}

		#endregion

		#region Implementation

		protected TestDataForBOM Data => data ?? (data = new TestDataForBOM(Factory));
		TestDataForBOM data;

		protected WhsTestHelperFunctions Helper => helper ?? (helper = new WhsTestHelperFunctions(Factory));
		WhsTestHelperFunctions helper;

		#endregion
	}
}
