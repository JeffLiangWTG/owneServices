using System;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Registry.Business;
using Moq;
using NUnit.Framework;
using ServiceManager.Integration.Abstractions;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	class PartiallyReplenishedPickSplitterTest : WhsTestCaseWithFactory
	{
		#region TestSplitOrdersFromPartiallyReplenishedPicks_ObjectFactoryConfiguration

		public void TestSplitOrdersFromPartiallyReplenishedPicks_ObjectFactoryConfiguration()
		{
			AssertType<PartiallyReplenishedPickSplitter>(ObjectFactory.Get<IPartiallyReplenishedPickSplitter>());
		}

		#endregion

		#region TestSplitOrdersFromPartiallyReplenishedPicks_NullPicks

		public void TestSplitOrdersFromPartiallyReplenishedPicks_NullPicks()
		{
			var splitter = ObjectFactory.Get<IPartiallyReplenishedPickSplitter>();
			AssertExceptionThrown<ArgumentNullException>(() => splitter.SplitOrdersFromPartiallyReplenishedPick(null));
		}

		#endregion

		#region TestSplitOrdersFromPartiallyReplenishedPicks_SinglePick_SingleOrder

		public void TestSplitOrdersFromPartiallyReplenishedPicks_SinglePick_SingleOrder_ParamOn()
		{
			TestSplitOrdersFromPartiallyReplenishedPicks_SinglePick_SingleOrderCore(paramOn: true);
		}

		public void TestSplitOrdersFromPartiallyReplenishedPicks_SinglePick_SingleOrder_ParamOff()
		{
			TestSplitOrdersFromPartiallyReplenishedPicks_SinglePick_SingleOrderCore(paramOn: false);
		}

		void TestSplitOrdersFromPartiallyReplenishedPicks_SinglePick_SingleOrderCore(bool paramOn)
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var pickParams = WhsClientPickingParams.GetClientPickingParams(data.Org1).WarehousePickPackParams.AddNew();
			pickParams.WPP_WW_Warehouse = data.Whs1.PK;
			pickParams.WPP_SplitOrdersFromPartiallyReplenishedPicks = paramOn;

			var pickFaceLocation = data.Whs1.FindLocation("A-1");
			var bulkLocation = data.Whs1.FindLocation("A-2");
			Helper.CreateProductPickFace(data.Part1, data.Org1, pickFaceLocation);
			Helper.CreateProductPickFace(data.Part2, data.Org1, pickFaceLocation);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m, bulkLocation, "");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part2, 10m, bulkLocation, "");
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			var transferLine1 = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, bulkLocation, pickFaceLocation);
			transferLine1.RunPreSaveValidation();
			transferLine1.FinaliseDocketLine();

			var transferLine2 = Helper.CreateWhsTransferLine(transfer, data.Part2, 10m, bulkLocation, pickFaceLocation);
			transferLine2.RunPreSaveValidation();
			Factory.Save();
			AssertEquals("Precondition", false, transfer.IsFinalised);

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "01");
			Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			Helper.CreateWhsOrderLine(order, data.Part2, 10m);
			Factory.Save();

			var pick = Helper.CreatePickNew(order);
			AssertEquals("Precondition: pick is waiting replenishment.", true, pick.WP_IsAwaitingReplenishment);
			Factory.Save();

			var splitter = ObjectFactory.Get<IPartiallyReplenishedPickSplitter>();
			AssertNull("Should return no picks", splitter.SplitOrdersFromPartiallyReplenishedPick(pick));
			AssertEquals("Expect no new picks created", 1, Factory.Load<WhsPick>(new ZQuery()).Length);
		}

		#endregion

		#region TestSplitOrdersFromPartiallyReplenishedPicks_ReplenishedPicks

		public void TestSplitOrdersFromPartiallyReplenishedPicks_ReplenishedPicks_ParamOn()
		{
			TestSplitOrdersFromPartiallyReplenishedPicks_ReplenishedPicksCore(paramOn: true);
		}

		public void TestSplitOrdersFromPartiallyReplenishedPicks_ReplenishedPicks_ParamOff()
		{
			TestSplitOrdersFromPartiallyReplenishedPicks_ReplenishedPicksCore(paramOn: false);
		}

		void TestSplitOrdersFromPartiallyReplenishedPicks_ReplenishedPicksCore(bool paramOn)
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var pickParams = WhsClientPickingParams.GetClientPickingParams(data.Org1).WarehousePickPackParams.AddNew();
			pickParams.WPP_WW_Warehouse = data.Whs1.PK;
			pickParams.WPP_SplitOrdersFromPartiallyReplenishedPicks = paramOn;

			var pickFaceLocation = data.Whs1.FindLocation("A-1");
			var bulkLocation = data.Whs1.FindLocation("A-2");
			Helper.CreateProductPickFace(data.Part1, data.Org1, pickFaceLocation);
			Helper.CreateProductPickFace(data.Part2, data.Org1, pickFaceLocation);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m, bulkLocation, "");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part2, 10m, bulkLocation, "");
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			var transferLine1 = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, bulkLocation, pickFaceLocation);
			transferLine1.RunPreSaveValidation();
			transferLine1.FinaliseDocketLine();

			var transferLine2 = Helper.CreateWhsTransferLine(transfer, data.Part2, 10m, bulkLocation, pickFaceLocation);
			transferLine2.RunPreSaveValidation();
			transferLine2.FinaliseDocketLine();
			Factory.Save();

			var order1 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "01");
			Helper.CreateWhsOrderLine(order1, data.Part1, 10m);

			var order2 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "02");
			Helper.CreateWhsOrderLine(order2, data.Part2, 10m);
			Factory.Save();

			var pick = Helper.CreatePickNew(order1, order2);
			AssertEquals("Precondition: pick is NOT waiting replenishment.", false, pick.WP_IsAwaitingReplenishment);
			Factory.Save();

			AssertNull("Should return no picks", RunSplitterAndGatherResults(pick));
			AssertEquals("Expect no new picks created", 1, Factory.Load<WhsPick>(new ZQuery()).Length);
		}

		#endregion

		#region TestSplitOrdersFromPartiallyReplenishedPicks_SinglePick_MultipleOrders

		public void TestSplitOrdersFromPartiallyReplenishedPicks_SinglePick_MultipleOrders_ParamOn()
		{
			TestSplitOrdersFromPartiallyReplenishedPicks_SinglePick_MultipleOrdersCore(paramOn: true);
		}

		public void TestSplitOrdersFromPartiallyReplenishedPicks_SinglePick_MultipleOrders_ParamOff()
		{
			TestSplitOrdersFromPartiallyReplenishedPicks_SinglePick_MultipleOrdersCore(paramOn: false);
		}

		void TestSplitOrdersFromPartiallyReplenishedPicks_SinglePick_MultipleOrdersCore(bool paramOn)
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			data.Whs1.WW_IsPickByUOMEnabled = true;

			var whsGroup = Helper.CreateWhsCartonGroup("1", "WhsOrg");
			var whsSize = Helper.CreateWhsCartonSize("WH", 10, 20, 40, 80, 160, 320, new ZByte(100), Constants.Length.Metres, Constants.Weight.Kilograms);
			whsGroup.CartonSizes.Add(whsSize);
			data.Whs1.WarehouseAddress.Header.MiscServ.OM_WCG_CartonGroup = whsGroup.PK;

			var packType = data.Part1.Lookups.PackTypes.Single(p => p.F3_Code == data.Part1.OP_StockKeepingUnit);
			packType.F3_UOMType = UOMPackTypesList.Codes.SplitCase;

			var pickParams = WhsClientPickingParams.GetClientPickingParams(data.Org1).WarehousePickPackParams.AddNew();
			pickParams.WPP_WW_Warehouse = data.Whs1.PK;
			pickParams.WPP_SplitOrdersFromPartiallyReplenishedPicks = paramOn;

			var pickFaceLocation = data.Whs1.FindLocation("A-1");
			var bulkLocation = data.Whs1.FindLocation("A-2");
			Helper.CreateProductPickFace(data.Part1, data.Org1, pickFaceLocation);
			Helper.CreateProductPickFace(data.Part2, data.Org1, pickFaceLocation);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m, bulkLocation, "");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part2, 10m, bulkLocation, "");
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			var transferLine1 = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, bulkLocation, pickFaceLocation);
			transferLine1.RunPreSaveValidation();
			transferLine1.FinaliseDocketLine();

			var transferLine2 = Helper.CreateWhsTransferLine(transfer, data.Part2, 10m, bulkLocation, pickFaceLocation);
			transferLine2.RunPreSaveValidation();
			Factory.Save();
			AssertEquals("Precondition", false, transfer.IsFinalised);

			var order1 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "01");
			Helper.CreateWhsOrderLine(order1, data.Part1, 10m);

			var order2 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "02");
			Helper.CreateWhsOrderLine(order2, data.Part2, 10m);
			Factory.Save();

			var pick = Helper.CreatePickNew(order1, order2);
			pick.WP_WL_DockDoor = data.Whs1.DefaultOutboundDockDoorLocation.PK;
			pick.PickPriority = 5;
			pick.WP_PickPalletsByLabel = false;
			pick.WP_PickCasesByLabel = false;
			pick.WP_CartoniseSplitCases = true;
			pick.WP_ForcePickByCaseUOMTypeAllocation = false;
			pick.WP_ForceSplitCaseUOMTypeAllocation = true;

			AssertEquals("Precondition: pick is waiting replenishment.", true, pick.WP_IsAwaitingReplenishment);
			Factory.Save();

			var pickReturned = RunSplitterAndGatherResults(pick);

			if (paramOn)
			{
				AssertEquals("Expect new pick created", 2, Factory.Load<WhsPick>(new ZQuery()).Length);

				AssertEquals("Original pick is still waiting replenishment.", true, pick.WP_IsAwaitingReplenishment);
				AssertEquals("Original pick is not cartonized.", false, pick.WP_IsCartonised);
				AssertEquals("Original Pick should have 1 order", 1, pick.Orders.Count);
				AssertEquals("Original Pick should have not replenished order", order2.PK, pick.Orders[0].PK);
				AssertEquals("Original pick WP_PercentageComplete updated.", (ZByte)0, pick.WP_PercentageComplete);

				AssertNotNull("Should return 1 pick", pickReturned);
				AssertNotEquals("New pick is not original pick", pick.PK, pickReturned.PK);
				AssertNewPickMatchesOriginalPicksSettings(pick, pickReturned);

				AssertEquals("New pick is NOT waiting replenishment.", false, pickReturned.WP_IsAwaitingReplenishment);
				AssertEquals("New pick is cartonized.", true, pickReturned.WP_IsCartonised);
				AssertEquals("New Pick should have 1 order", 1, pickReturned.Orders.Count);
				AssertEquals("New Pick should have replenished order", order1.PK, pickReturned.Orders[0].PK);
				AssertEquals("New pick WP_PercentageComplete updated.", (ZByte)0, pickReturned.WP_PercentageComplete);
			}
			else
			{
				AssertNull("Should return no picks", pickReturned);
				AssertEquals("Original pick is still waiting replenishment.", true, pick.WP_IsAwaitingReplenishment);
				AssertEquals("Original pick is not cartonized.", false, pick.WP_IsCartonised);
				AssertEquals("Expect no new picks created", 1, Factory.Load<WhsPick>(new ZQuery()).Length);
			}
		}

		#endregion

		#region TestSplitOrdersFromPartiallyReplenishedPicks_Unallocated

		public void TestSplitOrdersFromPartiallyReplenishedPicks_Unallocated_ParamOn()
		{
			TestSplitOrdersFromPartiallyReplenishedPicks_UnallocatedCore(paramOn: true);
		}

		public void TestSplitOrdersFromPartiallyReplenishedPicks_Unallocated_ParamOff()
		{
			TestSplitOrdersFromPartiallyReplenishedPicks_UnallocatedCore(paramOn: false);
		}

		void TestSplitOrdersFromPartiallyReplenishedPicks_UnallocatedCore(bool paramOn)
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var pickParams = WhsClientPickingParams.GetClientPickingParams(data.Org1).WarehousePickPackParams.AddNew();
			pickParams.WPP_WW_Warehouse = data.Whs1.PK;
			pickParams.WPP_SplitOrdersFromPartiallyReplenishedPicks = paramOn;

			var pickFaceLocation = data.Whs1.FindLocation("A-1");
			var bulkLocation = data.Whs1.FindLocation("A-2");
			Helper.CreateProductPickFace(data.Part1, data.Org1, pickFaceLocation);
			Helper.CreateProductPickFace(data.Part2, data.Org1, pickFaceLocation);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m, bulkLocation, "");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part2, 10m, bulkLocation, "");
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			var transferLine1 = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, bulkLocation, pickFaceLocation);
			transferLine1.RunPreSaveValidation();
			transferLine1.FinaliseDocketLine();

			var transferLine2 = Helper.CreateWhsTransferLine(transfer, data.Part2, 10m, bulkLocation, pickFaceLocation);
			transferLine2.RunPreSaveValidation();
			Factory.Save();
			AssertEquals("Precondition", false, transfer.IsFinalised);

			var order1 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "01");
			Helper.CreateWhsOrderLine(order1, data.Part1, 10m);

			var order2 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "02");
			Helper.CreateWhsOrderLine(order2, data.Part2, 10m);
			Factory.Save();

			var pick = Factory.New<WhsPick>();
			pick.Orders.AddRange(new[] { order1, order2 });
			pick.RunPreSaveValidation();
			AssertEquals("Precondition: pick is NOT waiting replenishment.", false, pick.WP_IsAwaitingReplenishment);
			Factory.Save();

			var splitter = ObjectFactory.Get<IPartiallyReplenishedPickSplitter>();
			AssertNull("Should return no picks", splitter.SplitOrdersFromPartiallyReplenishedPick(pick));
			AssertEquals("Expect no new picks created", 1, Factory.Load<WhsPick>(new ZQuery()).Length);
		}

		#endregion

		#region TestSplitOrdersFromPartiallyReplenishedPicks_SinglePick_SplitEnabledForSome

		public void TestSplitOrdersFromPartiallyReplenishedPicks_SinglePick_SplitEnabledForSome()
		{
			var data = new TestDataSimpleEnvironment(Factory, 3, 1);
			var pickParams1 = WhsClientPickingParams.GetClientPickingParams(data.Org1).WarehousePickPackParams.AddNew();
			pickParams1.WPP_WW_Warehouse = data.Whs1.PK;
			pickParams1.WPP_SplitOrdersFromPartiallyReplenishedPicks = true;

			var client2 = Helper.CreateClient("CL2");
			Helper.CreateProductClientRelationShip(client2, data.Part1);
			Helper.CreateProductClientRelationShip(client2, data.Part2);
			var pickParams2 = WhsClientPickingParams.GetClientPickingParams(client2).WarehousePickPackParams.AddNew();
			pickParams2.WPP_WW_Warehouse = data.Whs1.PK;
			pickParams2.WPP_SplitOrdersFromPartiallyReplenishedPicks = false;

			var pickFaceLocation = data.Whs1.FindLocation("A-1");
			var bulkLocation = data.Whs1.FindLocation("A-2");
			Helper.CreateProductPickFace(data.Part1, data.Org1, pickFaceLocation);
			Helper.CreateProductPickFace(data.Part2, data.Org1, pickFaceLocation);
			Helper.CreateProductPickFace(data.Part1, client2, pickFaceLocation);
			Helper.CreateProductPickFace(data.Part2, client2, pickFaceLocation);

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m, bulkLocation, "");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part2, 10m, bulkLocation, "");
			Helper.CreateWhsReceiveWithInventory(client2, data.Whs1, "R3", data.Part1, 10m, bulkLocation, "");
			Helper.CreateWhsReceiveWithInventory(client2, data.Whs1, "R4", data.Part2, 10m, bulkLocation, "");
			Factory.Save();

			var transfer1 = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			var transferLine1 = Helper.CreateWhsTransferLine(transfer1, data.Part1, 10m, bulkLocation, pickFaceLocation);
			transferLine1.RunPreSaveValidation();
			transferLine1.FinaliseDocketLine();

			var transferLine2 = Helper.CreateWhsTransferLine(transfer1, data.Part2, 10m, bulkLocation, pickFaceLocation);
			transferLine2.RunPreSaveValidation();

			var transfer2 = Helper.CreateWhsTransfer(client2, data.Whs1);
			var transferLine3 = Helper.CreateWhsTransferLine(transfer2, data.Part1, 10m, bulkLocation, pickFaceLocation);
			transferLine3.RunPreSaveValidation();
			transferLine3.FinaliseDocketLine();

			var transferLine4 = Helper.CreateWhsTransferLine(transfer2, data.Part2, 10m, bulkLocation, pickFaceLocation);
			transferLine4.RunPreSaveValidation();
			Factory.Save();
			AssertEquals("Precondition", false, transfer1.IsFinalised);
			AssertEquals("Precondition", false, transfer2.IsFinalised);

			var order1 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "01");
			Helper.CreateWhsOrderLine(order1, data.Part1, 10m);

			var order2 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "02");
			Helper.CreateWhsOrderLine(order2, data.Part2, 10m);

			var order3 = Helper.CreateWhsOrder(client2, data.Whs1, "03");
			Helper.CreateWhsOrderLine(order3, data.Part1, 10m);

			var order4 = Helper.CreateWhsOrder(client2, data.Whs1, "04");
			Helper.CreateWhsOrderLine(order4, data.Part2, 10m);
			Factory.Save();

			var pick = Helper.CreatePickNew(order1, order2, order3, order4);
			AssertEquals("Precondition: pick is waiting replenishment.", true, pick.WP_IsAwaitingReplenishment);
			Factory.Save();

			var pickReturned = RunSplitterAndGatherResults(pick);

			AssertEquals("Expect new pick created", 2, Factory.Load<WhsPick>(new ZQuery()).Length);

			AssertEquals("Original pick is still waiting replenishment.", true, pick.WP_IsAwaitingReplenishment);
			AssertEquals("Original Pick should have 3 orders", 3, pick.Orders.Count);
			AssertContainsExactElementsInAnyOrder("Original Pick should have not replenished orders", new[] { order2.PK, order3.PK, order4.PK }, pick.Orders.Select(o => o.PK));
			AssertEquals("Original pick WP_PercentageComplete updated.", (ZByte)0, pick.WP_PercentageComplete);

			AssertNotNull("Should return pick", pickReturned);
			AssertEquals("Expect new pick created", 2, Factory.Load<WhsPick>(new ZQuery()).Length);
			AssertNotEquals("New pick is not original pick", pick.PK, pickReturned.PK);
			AssertNewPickMatchesOriginalPicksSettings(pick, pickReturned);

			AssertEquals("New pick is NOT waiting replenishment.", false, pickReturned.WP_IsAwaitingReplenishment);
			AssertEquals("New Pick should have 1 order", 1, pickReturned.Orders.Count);
			AssertEquals("New Pick should have replenished order", order1.PK, pickReturned.Orders[0].PK);
			AssertEquals("New pick WP_PercentageComplete updated.", (ZByte)0, pickReturned.WP_PercentageComplete);
		}

		#endregion

		#region TestSplitOrdersFromPartiallyReplenishedPicks_MultiplePicks

		public void TestSplitOrdersFromPartiallyReplenishedPicks_MultiplePicks_ParamOn()
		{
			TestSplitOrdersFromPartiallyReplenishedPicks_MultiplePicksCore(paramOn: true);
		}

		public void TestSplitOrdersFromPartiallyReplenishedPicks_MultiplePicks_ParamOff()
		{
			TestSplitOrdersFromPartiallyReplenishedPicks_MultiplePicksCore(paramOn: false);
		}

		void TestSplitOrdersFromPartiallyReplenishedPicks_MultiplePicksCore(bool paramOn)
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var pickParams = WhsClientPickingParams.GetClientPickingParams(data.Org1).WarehousePickPackParams.AddNew();
			pickParams.WPP_WW_Warehouse = data.Whs1.PK;
			pickParams.WPP_SplitOrdersFromPartiallyReplenishedPicks = paramOn;

			var pickFaceLocation = data.Whs1.FindLocation("A-1");
			var bulkLocation = data.Whs1.FindLocation("A-2");
			Helper.CreateProductPickFace(data.Part1, data.Org1, pickFaceLocation);
			Helper.CreateProductPickFace(data.Part2, data.Org1, pickFaceLocation);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m, bulkLocation, "");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part2, 10m, bulkLocation, "");
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			var transferLine1 = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, bulkLocation, pickFaceLocation);
			transferLine1.RunPreSaveValidation();
			transferLine1.FinaliseDocketLine();

			var transferLine2 = Helper.CreateWhsTransferLine(transfer, data.Part2, 10m, bulkLocation, pickFaceLocation);
			transferLine2.RunPreSaveValidation();
			Factory.Save();
			AssertEquals("Precondition", false, transfer.IsFinalised);

			var order1 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			Helper.CreateWhsOrderLine(order1, data.Part1, 5m);

			var order2 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O2");
			Helper.CreateWhsOrderLine(order2, data.Part2, 5m);

			var order3 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O3");
			Helper.CreateWhsOrderLine(order3, data.Part1, 5m);
			Factory.Save();

			var pick1 = Helper.CreatePickNew(order1, order2);
			AssertEquals("Precondition: pick is waiting replenishment.", true, pick1.WP_IsAwaitingReplenishment);

			var pick2 = Helper.CreatePickNew(order3);
			AssertEquals("Precondition: pick is waiting replenishment.", false, pick2.WP_IsAwaitingReplenishment);
			Factory.Save();

			var pickReturned = RunSplitterAndGatherResults(pick1);
			if (paramOn)
			{
				AssertNotNull("Should return pick", pickReturned);
				var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
				AssertEquals("Expect new pick created", 3, Factory.Load<WhsPick>(new ZQuery()).Length);

				Assertion.AssertEquals("Original pick1 is still waiting replenishment.", true, pick1.WP_IsAwaitingReplenishment);
				AssertEquals("Original Pick1 should have 1 order", 1, pick1.Orders.Count);
				AssertEquals("Original pick1 should have not replenished order", order2.PK, pick1.Orders[0].PK);
				Assertion.AssertEquals("Original pick1 WP_PercentageComplete updated.", (ZByte)0, pick1.WP_PercentageComplete);

				Assertion.AssertEquals("Original pick2 is still not waiting replenishment.", false, pick2.WP_IsAwaitingReplenishment);
				AssertEquals("Original Pick2 should have 1 order", 1, pick2.Orders.Count);
				AssertEquals("Original pick2 should have correct order", order3.PK, pick2.Orders[0].PK);
				Assertion.AssertEquals("Original pick2 WP_PercentageComplete updated.", (ZByte)0, pick2.WP_PercentageComplete);

				AssertEquals("New pick is not any of the original picks", true, pickReturned.PK != pick1.PK && pickReturned.PK != pick2.PK);
				AssertNewPickMatchesOriginalPicksSettings(pick1, pickReturned);

				AssertEquals("New pick is NOT waiting replenishment.", false, pickReturned.WP_IsAwaitingReplenishment);
				AssertEquals("New Pick should have 1 order", 1, pickReturned.Orders.Count);
				AssertEquals("New Pick should have replenished order", order1.PK, pickReturned.Orders[0].PK);
				AssertEquals("New pick WP_PercentageComplete updated.", (ZByte)0, pickReturned.WP_PercentageComplete);
			}
			else
			{
				AssertNull("Should return no picks", pickReturned);
				AssertEquals("Expect no new picks created", 2, Factory.Load<WhsPick>(new ZQuery()).Length);
			}
		}

		#endregion

		#region TestSplitOrdersFromPartiallyReplenishedPicks_MultiplePicks_SplitEnabledForSome

		public void TestSplitOrdersFromPartiallyReplenishedPicks_MultiplePicks_SplitEnabledForSome()
		{
			var data = new TestDataSimpleEnvironment(Factory, 3, 1);
			var pickParams1 = WhsClientPickingParams.GetClientPickingParams(data.Org1).WarehousePickPackParams.AddNew();
			pickParams1.WPP_WW_Warehouse = data.Whs1.PK;
			pickParams1.WPP_SplitOrdersFromPartiallyReplenishedPicks = true;

			var client2 = Helper.CreateClient("CL2");
			Helper.CreateProductClientRelationShip(client2, data.Part1);
			Helper.CreateProductClientRelationShip(client2, data.Part2);
			var pickParams2 = WhsClientPickingParams.GetClientPickingParams(client2).WarehousePickPackParams.AddNew();
			pickParams2.WPP_WW_Warehouse = data.Whs1.PK;
			pickParams2.WPP_SplitOrdersFromPartiallyReplenishedPicks = false;

			var pickFaceLocation = data.Whs1.FindLocation("A-1");
			var bulkLocation = data.Whs1.FindLocation("A-2");
			Helper.CreateProductPickFace(data.Part1, data.Org1, pickFaceLocation);
			Helper.CreateProductPickFace(data.Part2, data.Org1, pickFaceLocation);
			Helper.CreateProductPickFace(data.Part1, client2, pickFaceLocation);
			Helper.CreateProductPickFace(data.Part2, client2, pickFaceLocation);

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m, bulkLocation, "");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part2, 10m, bulkLocation, "");
			Helper.CreateWhsReceiveWithInventory(client2, data.Whs1, "R3", data.Part1, 10m, bulkLocation, "");
			Helper.CreateWhsReceiveWithInventory(client2, data.Whs1, "R4", data.Part2, 10m, bulkLocation, "");
			Factory.Save();

			var transfer1 = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "T1");
			var transferLine1 = Helper.CreateWhsTransferLine(transfer1, data.Part1, 10m, bulkLocation, pickFaceLocation);
			transferLine1.RunPreSaveValidation();
			transferLine1.FinaliseDocketLine();

			var transferLine2 = Helper.CreateWhsTransferLine(transfer1, data.Part2, 10m, bulkLocation, pickFaceLocation);
			transferLine2.RunPreSaveValidation();

			var transfer2 = Helper.CreateWhsTransfer(client2, data.Whs1, "T2");
			var transferLine3 = Helper.CreateWhsTransferLine(transfer2, data.Part1, 10m, bulkLocation, pickFaceLocation);
			transferLine3.RunPreSaveValidation();
			transferLine3.FinaliseDocketLine();

			var transferLine4 = Helper.CreateWhsTransferLine(transfer2, data.Part2, 10m, bulkLocation, pickFaceLocation);
			transferLine4.RunPreSaveValidation();
			Factory.Save();
			AssertEquals("Precondition", false, transfer1.IsFinalised);
			AssertEquals("Precondition", false, transfer2.IsFinalised);

			var order1 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			Helper.CreateWhsOrderLine(order1, data.Part1, 10m);

			var order2 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O2");
			Helper.CreateWhsOrderLine(order2, data.Part2, 10m);

			var order3 = Helper.CreateWhsOrder(client2, data.Whs1, "O3");
			Helper.CreateWhsOrderLine(order3, data.Part1, 10m);

			var order4 = Helper.CreateWhsOrder(client2, data.Whs1, "O4");
			Helper.CreateWhsOrderLine(order4, data.Part2, 10m);
			Factory.Save();

			var pick1 = Helper.CreatePickNew(order1, order2);
			AssertEquals("Precondition: pick is waiting replenishment.", true, pick1.WP_IsAwaitingReplenishment);
			var pick2 = Helper.CreatePickNew(order3, order4);
			AssertEquals("Precondition: pick is waiting replenishment.", true, pick2.WP_IsAwaitingReplenishment);
			Factory.Save();

			var pickReturned1 = RunSplitterAndGatherResults(pick1);
			var pickReturned2 = RunSplitterAndGatherResults(pick2);

			AssertEquals("Expect new pick created", 3, Factory.Load<WhsPick>(new ZQuery()).Length);

			AssertEquals("Original pick1 is still waiting replenishment.", true, pick1.WP_IsAwaitingReplenishment);
			AssertEquals("Original Pick1 should have 1 order", 1, pick1.Orders.Count);
			AssertEquals("Original pick1 should have not replenished order", order2.PK, pick1.Orders[0].PK);
			AssertEquals("Original pick1 WP_PercentageComplete updated.", (ZByte)0, pick1.WP_PercentageComplete);

			AssertEquals("Original pick2 is still waiting replenishment.", true, pick2.WP_IsAwaitingReplenishment);
			AssertEquals("Original Pick2 should have 2 orders", 2, pick2.Orders.Count);
			AssertContainsExactElementsInAnyOrder("Original pick2 should have correct orders", new[] { order3.PK, order4.PK }, pick2.Orders.Select(o => o.PK));
			AssertEquals("Original pick2 WP_PercentageComplete updated.", (ZByte)0, pick2.WP_PercentageComplete);

			AssertNotNull("Should return new pick", pickReturned1);
			AssertNull("Should return NO new pick", pickReturned2);

			AssertEquals("New pick is not any of the original picks", true, pickReturned1.PK != pick1.PK && pickReturned1.PK != pick2.PK);
			AssertNewPickMatchesOriginalPicksSettings(pick1, pickReturned1);

			AssertEquals("New pick is NOT waiting replenishment.", false, pickReturned1.WP_IsAwaitingReplenishment);
			AssertEquals("New Pick should have 1 order", 1, pickReturned1.Orders.Count);
			AssertEquals("New Pick should have replenished order", order1.PK, pickReturned1.Orders[0].PK);
			AssertEquals("New pick WP_PercentageComplete updated.", (ZByte)0, pickReturned1.WP_PercentageComplete);
		}

		#endregion

		#region TestSplitOrdersFromPartiallyReplenishedPicks_SalesChannel

		public void TestSplitOrdersFromPartiallyReplenishedPicks_SalesChannel()
		{
			var data = new TestDataSimpleEnvironment(Factory, 3, 1);
			var salesChannel = Helper.CreateWhsSalesChannel("ECO", "eCommerce");
			var otherSalesChannel = Helper.CreateWhsSalesChannel("DBT", "Distribution");

			var pickParams1 = WhsClientPickingParams.GetClientPickingParams(data.Org1).WarehousePickPackParams.AddNew();
			pickParams1.WPP_WW_Warehouse = data.Whs1.PK;
			pickParams1.WPP_WSH_SalesChannel = salesChannel.PK;
			pickParams1.WPP_SplitOrdersFromPartiallyReplenishedPicks = true;

			var pickParams2 = WhsClientPickingParams.GetClientPickingParams(data.Org1).WarehousePickPackParams.AddNew();
			pickParams2.WPP_WW_Warehouse = data.Whs1.PK;
			pickParams2.WPP_WSH_SalesChannel = otherSalesChannel.PK;
			pickParams2.WPP_SplitOrdersFromPartiallyReplenishedPicks = false;

			var pickParams3 = WhsClientPickingParams.GetClientPickingParams(data.Org1).WarehousePickPackParams.AddNew();
			pickParams3.WPP_WW_Warehouse = data.Whs1.PK;
			pickParams3.WPP_SplitOrdersFromPartiallyReplenishedPicks = false;

			var pickFaceLocation = data.Whs1.FindLocation("A-1");
			var bulkLocation = data.Whs1.FindLocation("A-2");
			Helper.CreateProductPickFace(data.Part1, data.Org1, pickFaceLocation);
			Helper.CreateProductPickFace(data.Part2, data.Org1, pickFaceLocation);

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 30m, bulkLocation, "");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part2, 10m, bulkLocation, "");
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "T1");
			var transferLine1 = Helper.CreateWhsTransferLine(transfer, data.Part1, 30m, bulkLocation, pickFaceLocation);
			transferLine1.RunPreSaveValidation();
			transferLine1.FinaliseDocketLine();

			var transferLine2 = Helper.CreateWhsTransferLine(transfer, data.Part2, 10m, bulkLocation, pickFaceLocation);
			transferLine2.RunPreSaveValidation();
			Factory.Save();
			AssertEquals("Precondition", false, transfer.IsFinalised);

			var order1 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			order1.WD_WSH_SalesChannel = salesChannel.PK;
			Helper.CreateWhsOrderLine(order1, data.Part1, 10m);

			var order2 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O2");
			order2.WD_WSH_SalesChannel = otherSalesChannel.PK;
			Helper.CreateWhsOrderLine(order2, data.Part1, 10m);

			var order3 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O3");
			Helper.CreateWhsOrderLine(order3, data.Part1, 10m);

			var order4 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O4");
			Helper.CreateWhsOrderLine(order4, data.Part2, 10m);
			Factory.Save();

			var pick = Helper.CreatePickNew(order1, order2, order3, order4);
			AssertEquals("Precondition: pick is waiting replenishment.", true, pick.WP_IsAwaitingReplenishment);
			Factory.Save();

			var pickReturned = RunSplitterAndGatherResults(pick);

			var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			AssertEquals("Expect new pick created", 2, newFactory.Load<WhsPick>(new ZQuery()).Length);

			var oldPick1 = newFactory.Load<WhsPick>(pick.PK);
			AssertEquals("Original pick1 is still waiting replenishment.", true, oldPick1.WP_IsAwaitingReplenishment);
			AssertContainsExactElementsInAnyOrder("Original pick1 should have correct orders", new[] { order2.PK, order3.PK, order4.PK }, oldPick1.Orders.Select(o => o.PK));
			AssertEquals("Original pick1 WP_PercentageComplete updated.", (ZByte)0, oldPick1.WP_PercentageComplete);

			AssertNotNull("Should return new pick", pickReturned);

			var newPick = newFactory.Load<WhsPick>(pickReturned.PK);
			AssertEquals("New pick is not any of the original picks", true, newPick.PK != pick.PK);
			AssertNewPickMatchesOriginalPicksSettings(pick, newPick);

			AssertEquals("New pick is NOT waiting replenishment.", false, newPick.WP_IsAwaitingReplenishment);
			AssertEquals("New Pick should have 1 order", 1, newPick.Orders.Count);
			AssertEquals("New Pick should have replenished order", order1.PK, newPick.Orders[0].PK);
			AssertEquals("New pick WP_PercentageComplete updated.", (ZByte)0, newPick.WP_PercentageComplete);
		}

		#endregion

		#region TestSplitOrdersFromPartiallyReplenishedPicks_MaintainsAllocations

		public void TestSplitOrdersFromPartiallyReplenishedPicks_MaintainsAllocations()
		{
			var data = new TestDataSimpleEnvironment(Factory, 4, 1);
			var pickParams = WhsClientPickingParams.GetClientPickingParams(data.Org1).WarehousePickPackParams.AddNew();
			pickParams.WPP_WW_Warehouse = data.Whs1.PK;
			pickParams.WPP_SplitOrdersFromPartiallyReplenishedPicks = true;

			var pickFaceLocation1 = data.Whs1.FindLocation("A-1");
			var pickFaceLocation2 = data.Whs1.FindLocation("A-2");
			var bulkLocation1 = data.Whs1.FindLocation("A-3");
			var bulkLocation2 = data.Whs1.FindLocation("A-4");
			Helper.CreateProductPickFace(data.Part1, data.Org1, pickFaceLocation1);
			Helper.CreateProductPickFace(data.Part2, data.Org1, pickFaceLocation1);
			Helper.CreateProductPickFace(data.Part1, data.Org1, pickFaceLocation2);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m, bulkLocation1, "");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 10m, bulkLocation2, "");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R3", data.Part2, 10m, bulkLocation1, "");
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			var transferLine1 = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, bulkLocation1, pickFaceLocation1);
			transferLine1.RunPreSaveValidation();
			transferLine1.FinaliseDocketLine();

			var transferLine2 = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, bulkLocation2, pickFaceLocation2);
			transferLine2.RunPreSaveValidation();
			transferLine2.FinaliseDocketLine();

			var transferLine3 = Helper.CreateWhsTransferLine(transfer, data.Part2, 10m, bulkLocation1, pickFaceLocation1);
			transferLine3.RunPreSaveValidation();
			Factory.Save();
			AssertEquals("Precondition", false, transfer.IsFinalised);

			var order1 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine1 = Helper.CreateWhsOrderLine(order1, data.Part1, 10m);

			var order2 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O2");
			Helper.CreateWhsOrderLine(order2, data.Part2, 5m);
			Factory.Save();

			var pick = Helper.CreatePickNew(order1, order2);

			var orderPickLine = orderLine1.PickLines[0];
			orderPickLine.WZ_WE_InventoryLine = transferLine2.PK;
			AssertEquals("Precondition: pick is waiting replenishment.", true, pick.WP_IsAwaitingReplenishment);
			Factory.Save();
			AssertEquals("OrderLine should pick from transferLine2.", transferLine2.PK, orderPickLine.WZ_WE_InventoryLine);

			var pickReturned = RunSplitterAndGatherResults(pick);

			AssertEquals("Expect new pick created", 2, Factory.Load<WhsPick>(new ZQuery()).Length);

			AssertEquals("Original pick is still waiting replenishment.", true, pick.WP_IsAwaitingReplenishment);
			AssertEquals("Original Pick should have 1 order", 1, pick.Orders.Count);
			AssertEquals("Original pick should have not replenished order", order2.PK, pick.Orders[0].PK);
			AssertEquals("Original pick WP_PercentageComplete updated.", (ZByte)0, pick.WP_PercentageComplete);

			AssertNotNull("Should return new pick", pickReturned);
			AssertEquals("New pick is not any of the original pick", true, pickReturned.PK != pick.PK);
			AssertNewPickMatchesOriginalPicksSettings(pick, pickReturned);

			AssertEquals("New pick is NOT waiting replenishment.", false, pickReturned.WP_IsAwaitingReplenishment);
			AssertEquals("New Pick should have 1 order", 1, pickReturned.Orders.Count);
			AssertEquals("New Pick should have replenished order", order1.PK, pickReturned.Orders[0].PK);
			AssertEquals("New pick WP_PercentageComplete updated.", (ZByte)0, pickReturned.WP_PercentageComplete);

			AssertEquals("OrderLine1 should STILL pick from transferLine2.", transferLine2.PK, orderLine1.PickLines[0].WZ_WE_InventoryLine);
		}

		#endregion

		#region TestSplitOrdersFromPartiallyReplenishedPicks_WorkOrder

		public void TestSplitOrdersFromPartiallyReplenishedPicks_WorkOrder()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var pickParams = WhsClientPickingParams.GetClientPickingParams(data.Org1).WarehousePickPackParams.AddNew();
			pickParams.WPP_WW_Warehouse = data.Whs1.PK;
			pickParams.WPP_SplitOrdersFromPartiallyReplenishedPicks = true;

			var mainProduct = Helper.CreateProduct(data.Org1, "MP1");
			var bomComponentProduct = Helper.CreateProduct(data.Org1, "BOM1");
			Helper.CreateProductBOM(mainProduct, bomComponentProduct, 1m, Constants.PkgUnit.Unit);

			var pickFaceLocation = data.Whs1.FindLocation("A-1");
			var bulkLocation = data.Whs1.FindLocation("A-2");
			Helper.CreateProductPickFace(bomComponentProduct, data.Org1, pickFaceLocation);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", bomComponentProduct, 10m, bulkLocation, "");
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			var transferLine = Helper.CreateWhsTransferLine(transfer, bomComponentProduct, 10m, bulkLocation, pickFaceLocation);
			transferLine.RunPreSaveValidation();
			transferLine.FinaliseDocketLine();
			Factory.Save();
			AssertEquals("Precondition", false, transfer.IsFinalised);

			var workOrder = Helper.CreateWhsWorkOrder(data.Org1, data.Whs1, "W01");
			Helper.CreateWhsWorkOrderLine(workOrder, mainProduct, 10m);
			Factory.Save();

			var pick = Helper.CreatePickNew(workOrder);
			pick.WP_IsAwaitingReplenishment = true;
			AssertEquals("Precondition: pick is waiting replenishment.", true, pick.WP_IsAwaitingReplenishment);
			Factory.Save();

			var pickReturned = RunSplitterAndGatherResults(pick);

			AssertNull("Should return no picks", pickReturned);
			AssertEquals("Expect no new picks created", 1, new BusinessObjectFactory().Load<WhsPick>(new ZQuery()).Length);
		}

		#endregion

		#region TestSplitOrdersFromPartiallyReplenishedPicks_PickByBOM

		public void TestSplitOrdersFromPartiallyReplenishedPicks_PickByBOM_NonBOMIsAwaiting()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var pickParams = WhsClientPickingParams.GetClientPickingParams(data.Org1).WarehousePickPackParams.AddNew();
			pickParams.WPP_WW_Warehouse = data.Whs1.PK;
			pickParams.WPP_SplitOrdersFromPartiallyReplenishedPicks = true;

			var mainProduct = Helper.CreateProduct(data.Org1, "MP1");
			mainProduct.OP_IsComponentPickedOnSalesOrder = true;
			var bomComponentProduct = Helper.CreateProduct(data.Org1, "BOM1");
			Helper.CreateProductBOM(mainProduct, bomComponentProduct, 1m, Constants.PkgUnit.Unit);

			var pickFaceLocation = data.Whs1.FindLocation("A-1");
			var bulkLocation = data.Whs1.FindLocation("A-2");
			Helper.CreateProductPickFace(bomComponentProduct, data.Org1, pickFaceLocation);
			Helper.CreateProductPickFace(data.Part1, data.Org1, pickFaceLocation);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", bomComponentProduct, 10m, bulkLocation, "");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 10m, bulkLocation, "");
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			var transferLine1 = Helper.CreateWhsTransferLine(transfer, bomComponentProduct, 10m, bulkLocation, pickFaceLocation);
			transferLine1.RunPreSaveValidation();
			transferLine1.FinaliseDocketLine();

			var transferLine2 = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, bulkLocation, pickFaceLocation);
			transferLine2.RunPreSaveValidation();
			Factory.Save();
			AssertEquals("Precondition", false, transfer.IsFinalised);

			var order1 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "01");
			Helper.CreateWhsOrderLine(order1, mainProduct, 10m);

			var order2 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "02");
			Helper.CreateWhsOrderLine(order2, data.Part1, 10m);
			Factory.Save();

			var pick = Helper.CreatePickNew(order1, order2);
			AssertEquals("Precondition: pick is waiting replenishment.", true, pick.WP_IsAwaitingReplenishment);
			Factory.Save();

			var pickReturned = RunSplitterAndGatherResults(pick);

			var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			AssertEquals("Expect new pick created", 2, newFactory.Load<WhsPick>(new ZQuery()).Length);

			var oldPick = newFactory.Load<WhsPick>(pick.PK);
			AssertEquals("Original pick is still waiting replenishment.", true, oldPick.WP_IsAwaitingReplenishment);
			AssertEquals("Original Pick should have 1 order", 1, oldPick.Orders.Count);
			AssertEquals("Original pick should have not replenished order", order2.PK, oldPick.Orders[0].PK);
			AssertEquals("Original pick WP_PercentageComplete updated.", (ZByte)0, oldPick.WP_PercentageComplete);

			AssertNotNull("Should return new pick", pickReturned);
			var newPick = newFactory.Load<WhsPick>(pickReturned.PK);
			AssertEquals("New pick is not any of the original pick", true, newPick.PK != pick.PK);
			AssertNewPickMatchesOriginalPicksSettings(pick, newPick);

			AssertEquals("New pick is NOT waiting replenishment.", false, newPick.WP_IsAwaitingReplenishment);
			AssertEquals("New Pick should have 1 order", 1, newPick.Orders.Count);
			AssertEquals("New Pick should have replenished order", order1.PK, newPick.Orders[0].PK);
			AssertEquals("New pick WP_PercentageComplete updated.", (ZByte)0, newPick.WP_PercentageComplete);
		}

		[GuiTest]
		public void TestSplitOrdersFromPartiallyReplenishedPicks_PickByBOM_IsAwaitingReplenishment()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var pickParams = WhsClientPickingParams.GetClientPickingParams(data.Org1).WarehousePickPackParams.AddNew();
			pickParams.WPP_WW_Warehouse = data.Whs1.PK;
			pickParams.WPP_SplitOrdersFromPartiallyReplenishedPicks = true;

			var ruleSet = AllocationRulesHelper.MakeNewRuleSetAndDeactivateSystemAllocationRuleSet(Factory);
			AllocationRulesHelper.AddFifoRule(ruleSet, 200, preventPickingPickFacesFromBulk: true);
			AllocationRulesHelper.AddPickFaceRules(ruleSet, 100);

			var mainProduct = Helper.CreateProduct(data.Org1, "MP1");
			mainProduct.OP_IsComponentPickedOnSalesOrder = true;
			var bomComponentProduct = Helper.CreateProduct(data.Org1, "BOM1");
			Helper.CreateProductBOM(mainProduct, bomComponentProduct, 1m, Constants.PkgUnit.Unit);

			var pickFaceLocation = data.Whs1.FindLocation("A-1");
			var bulkLocation = data.Whs1.FindLocation("A-2");
			Helper.CreateProductPickFace(bomComponentProduct, data.Org1, pickFaceLocation);
			Helper.CreateProductPickFace(mainProduct, data.Org1, pickFaceLocation);
			Helper.CreateProductPickFace(data.Part1, data.Org1, pickFaceLocation);
			Factory.Save();

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", bomComponentProduct, 15m, bulkLocation, "");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", mainProduct, 2m, pickFaceLocation, "");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R3", data.Part1, 10m, bulkLocation, "");
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			var transferLine1 = Helper.CreateWhsTransferLine(transfer, bomComponentProduct, 5m, bulkLocation, pickFaceLocation);
			transferLine1.RunPreSaveValidation();

			var transferLine2 = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, bulkLocation, pickFaceLocation);
			transferLine2.RunPreSaveValidation();
			transferLine2.FinaliseDocketLine();
			Factory.Save();
			AssertEquals("Precondition", false, transfer.IsFinalised);

			var order1 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "01");
			Helper.CreateWhsOrderLine(order1, mainProduct, 15m);

			var order2 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "02");
			Helper.CreateWhsOrderLine(order2, data.Part1, 10m);
			Factory.Save();

			var pick = Helper.CreatePickNew_WithoutAllocationEngineMock(order1, order2);
			Factory.Save();
			AssertEquals("Precondition: pick is waiting replenishment.", true, pick.WP_IsAwaitingReplenishment);

			var pickReturned = RunSplitterAndGatherResults(pick);

			var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			AssertEquals("Expect new pick created", 2, newFactory.Load<WhsPick>(new ZQuery()).Length);

			var oldPick = newFactory.Load<WhsPick>(pick.PK);
			AssertEquals("Original pick is still waiting replenishment.", true, oldPick.WP_IsAwaitingReplenishment);
			AssertEquals("Original Pick should have 1 order", 1, oldPick.Orders.Count);
			AssertEquals("Original pick should have not replenished order", order1.PK, oldPick.Orders[0].PK);
			AssertEquals("Original pick WP_PercentageComplete updated.", (ZByte)0, oldPick.WP_PercentageComplete);

			AssertNotNull("Should return new pick", pickReturned);
			var newPick = newFactory.Load<WhsPick>(pickReturned.PK);
			AssertEquals("New pick is not any of the original pick", true, newPick.PK != pick.PK);
			AssertNewPickMatchesOriginalPicksSettings(pick, newPick);

			AssertEquals("New pick is NOT waiting replenishment.", false, newPick.WP_IsAwaitingReplenishment);
			AssertEquals("New Pick should have 1 order", 1, newPick.Orders.Count);
			AssertEquals("New Pick should have replenished order", order2.PK, newPick.Orders[0].PK);
			AssertEquals("New pick WP_PercentageComplete updated.", (ZByte)0, newPick.WP_PercentageComplete);
		}

		#endregion

		#region TestSplitOrdersFromPartiallyReplenishedPicks_PickedAlready

		public void TestSplitOrdersFromPartiallyReplenishedPicks_PickedAlready()
		{
			using (ObjectFactory.Substitute(Mock.Of<INudgingController>()))
			{
				var data = new TestDataSimpleEnvironment(Factory, 2, 1);
				var pickParams = WhsClientPickingParams.GetClientPickingParams(data.Org1).WarehousePickPackParams.AddNew();
				pickParams.WPP_WW_Warehouse = data.Whs1.PK;
				pickParams.WPP_SplitOrdersFromPartiallyReplenishedPicks = true;
				var part3 = Helper.CreateProduct(data.Org1, "MP1");

				var pickFaceLocation = data.Whs1.FindLocation("A-1");
				var bulkLocation = data.Whs1.FindLocation("A-2");
				Helper.CreateProductPickFace(data.Part1, data.Org1, pickFaceLocation);
				Helper.CreateProductPickFace(data.Part2, data.Org1, pickFaceLocation);
				Helper.CreateProductPickFace(part3, data.Org1, pickFaceLocation);
				Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m, bulkLocation, "");
				Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part2, 10m, bulkLocation, "");
				Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R3", part3, 20m, bulkLocation, "");
				Factory.Save();

				var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
				var transferLine1 = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, bulkLocation, pickFaceLocation);
				transferLine1.RunPreSaveValidation();
				transferLine1.FinaliseDocketLine();

				var transferLine2 = Helper.CreateWhsTransferLine(transfer, data.Part2, 10m, bulkLocation, pickFaceLocation);
				transferLine2.RunPreSaveValidation();

				var transferLine3 = Helper.CreateWhsTransferLine(transfer, part3, 20m, bulkLocation, pickFaceLocation);
				transferLine3.RunPreSaveValidation();
				transferLine3.FinaliseDocketLine();
				Factory.Save();
				AssertEquals("Precondition", false, transfer.IsFinalised);

				var order1 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "01");
				Helper.CreateWhsOrderLine(order1, data.Part1, 10m);
				var orderLine2 = Helper.CreateWhsOrderLine(order1, part3, 10m);

				var order2 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "02");
				Helper.CreateWhsOrderLine(order2, data.Part2, 10m);
				var orderLine4 = Helper.CreateWhsOrderLine(order2, part3, 10m);
				Factory.Save();

				var pick = Helper.CreatePickNew(order1, order2);

				var pickLine1 = orderLine2.PickLines[0];
				pickLine1.WZ_PickedDateTime = ZDateTimeOffset.Now;

				var pickLine2 = orderLine4.PickLines[0];
				pickLine2.WZ_PickedDateTime = ZDateTimeOffset.Now;
				Factory.Save();

				AssertEquals("Precondition: pick is waiting replenishment.", true, pick.WP_IsAwaitingReplenishment);

				RunSplitterAndGatherResults(pick);

				var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
				AssertEquals("Expect new pick created", 1, newFactory.Load<WhsPick>(new ZQuery()).Length);

				var oldPick = newFactory.Load<WhsPick>(pick.PK);
				AssertEquals("Original pick is still waiting replenishment.", true, oldPick.WP_IsAwaitingReplenishment);
				AssertEquals("Original Pick should have 2 orders", 2, oldPick.Orders.Count);
				AssertEquals("Original pick WP_PercentageComplete is correct.", (ZByte)0, oldPick.WP_PercentageComplete);
			}
		}

		#endregion

		#region TestSplitOrdersFromPartiallyReplenishedPicks_SinglePick_MultipleOrders_NoSetPickParams

		public void TestSplitOrdersFromPartiallyReplenishedPicks_SinglePick_MultipleOrders_NoSetPickParams()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);

			var pickFaceLocation = data.Whs1.FindLocation("A-1");
			var bulkLocation = data.Whs1.FindLocation("A-2");
			Helper.CreateProductPickFace(data.Part1, data.Org1, pickFaceLocation);
			Helper.CreateProductPickFace(data.Part2, data.Org1, pickFaceLocation);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m, bulkLocation, "");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part2, 10m, bulkLocation, "");
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			var transferLine1 = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, bulkLocation, pickFaceLocation);
			transferLine1.RunPreSaveValidation();
			transferLine1.FinaliseDocketLine();

			var transferLine2 = Helper.CreateWhsTransferLine(transfer, data.Part2, 10m, bulkLocation, pickFaceLocation);
			transferLine2.RunPreSaveValidation();
			Factory.Save();
			AssertEquals("Precondition", false, transfer.IsFinalised);

			var order1 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "01");
			Helper.CreateWhsOrderLine(order1, data.Part1, 10m);

			var order2 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "02");
			Helper.CreateWhsOrderLine(order2, data.Part2, 10m);
			Factory.Save();

			var pick = Helper.CreatePickNew(order1, order2);

			AssertEquals("Precondition: pick is waiting replenishment.", true, pick.WP_IsAwaitingReplenishment);
			Factory.Save();

			var pickReturned = RunSplitterAndGatherResults(pick);
			AssertNull("Should return no picks", pickReturned);
			AssertEquals("Expect no new picks created", 1, Factory.Load<WhsPick>(new ZQuery()).Length);
		}

		#endregion

		#region Implementation

		static WhsPick RunSplitterAndGatherResults(WhsPick pick)
		{
			var splitter = ObjectFactory.Get<IPartiallyReplenishedPickSplitter>();
			var result = splitter.SplitOrdersFromPartiallyReplenishedPick(pick);
			pick.Factory.Save();

			return result;
		}

		#endregion

		#region Assertion

		void AssertNewPickMatchesOriginalPicksSettings(WhsPick pick, WhsPick newPick)
		{
			AssertEquals("Whs matches", pick.WP_WW_Whs, newPick.WP_WW_Whs);
			AssertEquals("DDL matches", pick.WP_WL_DockDoor, newPick.WP_WL_DockDoor);
			AssertEquals("PickPriority matches", pick.PickPriority, newPick.PickPriority);
			AssertEquals("WP_PickPalletsByLabel matches", pick.WP_PickPalletsByLabel, newPick.WP_PickPalletsByLabel);
			AssertEquals("WP_PickCasesByLabel matches", pick.WP_PickCasesByLabel, newPick.WP_PickCasesByLabel);
			AssertEquals("WP_CartoniseSplitCases matches", pick.WP_CartoniseSplitCases, newPick.WP_CartoniseSplitCases);
			AssertEquals("WP_ForcePickByCaseUOMTypeAllocation matches", pick.WP_ForcePickByCaseUOMTypeAllocation, newPick.WP_ForcePickByCaseUOMTypeAllocation);
			AssertEquals("WP_ForceSplitCaseUOMTypeAllocation matches", pick.WP_ForceSplitCaseUOMTypeAllocation, newPick.WP_ForceSplitCaseUOMTypeAllocation);
		}

		#endregion
	}
}
