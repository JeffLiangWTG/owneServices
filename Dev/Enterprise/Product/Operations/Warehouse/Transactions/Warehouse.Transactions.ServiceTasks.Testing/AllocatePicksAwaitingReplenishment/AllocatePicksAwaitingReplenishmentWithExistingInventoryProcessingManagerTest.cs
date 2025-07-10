using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CargoWise.Application;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Packing.Business.Testing;
using Enterprise.ProductionRules.Business;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.Warehouse.Transactions.ServiceTasks.Testing
{
	public class AllocatePicksAwaitingReplenishmentWithExistingInventoryProcessingManagerTest : WhsTestCaseWithFactory
	{
		#region TestAllocateAwaitingReplenishmentPicks

		public void TestAllocateAwaitingReplenishmentPicks_Orders()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var ruleSet = AllocationRulesHelper.MakeNewRuleSetAndDeactivateSystemAllocationRuleSet(Factory);
			AllocationRulesHelper.AddFullPalletRule(ruleSet, 5);
			AllocationRulesHelper.AddPickFaceRules(ruleSet, 10);
			AllocationRulesHelper.AddFifoRule(ruleSet, 20, preventPickingPickFacesFromBulk: true);

			Helper.CreateProductUnit(data.Part1, Constants.PkgUnit.Pallet, 2m); // Part1 : 1PLT = 2 UNT

			var pickFaceLocation = data.Whs1.FindLocation("A-1");
			var bulkLocation = data.Whs1.FindLocation("A-2");
			Helper.CreateProductPickFace(data.Part1, data.Org1, pickFaceLocation);
			Helper.CreateProductPickFace(data.Part2, data.Org1, pickFaceLocation);

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", ZDateTimeOffset.Today.AddDays(-1), data.Part1, 1m, bulkLocation, "");

			var orderForWaitingReplenishmentPick = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 1m, WhsPickOption.Codes.Manual);
			var orderForWaitingReplenishmentPickWithoutExistingInventory = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O3", data.Part2, 1m, WhsPickOption.Codes.Manual);
			var orderForNonWaitingReplenishmentPick = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O2", data.Part1, 1m, WhsPickOption.Codes.Manual);
			Factory.Save();

			var waitingReplenihsmentPick = Helper.CreatePickNew(orderForWaitingReplenishmentPick);
			var waitingReplenihsmentPickWithoutExistingInventory = Helper.CreatePickNew(orderForWaitingReplenishmentPickWithoutExistingInventory);
			var nonWaitingReplenihsmentPick = Helper.CreatePickNew(orderForNonWaitingReplenishmentPick);

			waitingReplenihsmentPick.WP_IsAwaitingReplenishment = true;
			waitingReplenihsmentPickWithoutExistingInventory.WP_IsAwaitingReplenishment = true;
			AssertEquals("Precondition", PickStatus.Codes.Created, nonWaitingReplenihsmentPick.WP_PickStatus);
			Factory.Save();

			AllocateAwaitingReplenishmentPicksForTest();

			AssertEquals("Since there are existing inventory, pick should be waiting for replenishment.",
				true, waitingReplenihsmentPick.WP_IsAwaitingReplenishment);
			AssertEquals("Since there are no existing inventory, pick status is updated to created.",
				PickStatus.Codes.Created, waitingReplenihsmentPickWithoutExistingInventory.WP_PickStatus);
			AssertEquals("Since status wasn't waiting replenishement, status shouldn't be changed.",
				PickStatus.Codes.Created, nonWaitingReplenihsmentPick.WP_PickStatus);

			AssertPickAvailableInventory(waitingReplenihsmentPick, 0m);
			AssertPickAvailableInventory(nonWaitingReplenihsmentPick, 0m);
		}

		void AssertPickAvailableInventory(WhsPick pick, decimal expectedQuantity)
		{
			var orderedInventory = (WhsPickOrderedInventory)pick.OrderedInventories.Single();
			var availableInventory = (WhsPickAvailableInventory)orderedInventory.AvailableInventories.Single();
			AssertEquals(expectedQuantity, availableInventory.PickLineQuantity);
		}

		#endregion

		#region TestAllocateAwaitingReplenishmentPicks_WorkOrders_ShouldNotThrowException

		public void TestAllocateAwaitingReplenishmentPicks_WorkOrders_ShouldNotThrowException()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var ruleSet = AllocationRulesHelper.MakeNewRuleSetAndDeactivateSystemAllocationRuleSet(Factory);
			AllocationRulesHelper.AddFullPalletRule(ruleSet, 5);
			AllocationRulesHelper.AddPickFaceRules(ruleSet, 10);
			AllocationRulesHelper.AddFifoRule(ruleSet, 20, preventPickingPickFacesFromBulk: true);

			var mainProduct = Helper.CreateProduct(data.Org1, $"P111");
			var subProduct = Helper.CreateProduct(data.Org1, $"P211");
			var bom = Helper.CreateProductBOM(mainProduct, subProduct, 2m, "UNT");
			Helper.CreateProductUnit(subProduct, Constants.PkgUnit.Pallet, 2m);

			var pickFaceLocation = data.Whs1.FindLocation("A-1");
			var bulkLocation = data.Whs1.FindLocation("A-2");
			var pickface = Helper.CreateProductPickFace(subProduct, data.Org1, pickFaceLocation, 0m, 6m, 1m);

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", ZDateTimeOffset.Today.AddDays(-1), subProduct, 20m, bulkLocation, "");
			Factory.Save();

			var workOrderWaitingReplenishmentPick = Helper.CreateWhsWorkOrderWithLine(data.Org1, data.Whs1, "WO1", mainProduct, 5m);
			var waitingReplenishmentPick = Helper.CreatePickNew(workOrderWaitingReplenishmentPick);
			waitingReplenishmentPick.WP_IsAwaitingReplenishment = true; // Force allocation which added fetch hint.
			Factory.Save();

			AssertNoExceptionThrown(AllocateAwaitingReplenishmentPicksForTest);
		}

		#endregion

		#region TestAllocateAwaitingReplenishmentPicks_WaitingReplenishmentPicksWithNotEnoughStock

		public void TestAllocateAwaitingReplenishmentPicks_WaitingReplenishmentPicksWithNotEnoughStock()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var ruleSet = AllocationRulesHelper.MakeNewRuleSetAndDeactivateSystemAllocationRuleSet(Factory);
			AllocationRulesHelper.AddFullPalletRule(ruleSet, 5);
			AllocationRulesHelper.AddPickFaceRules(ruleSet, 10);
			AllocationRulesHelper.AddFifoRule(ruleSet, 20, preventPickingPickFacesFromBulk: true);

			Helper.CreateProductUnit(data.Part1, Constants.PkgUnit.Pallet, 2m); // Part1 : 1PLT = 2 UNT

			var pickFaceLocation = data.Whs1.FindLocation("A-1");
			var bulkLocation = data.Whs1.FindLocation("A-2");
			Helper.CreateProductPickFace(data.Part1, data.Org1, pickFaceLocation);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", ZDateTimeOffset.Today, data.Part1, 1m, bulkLocation, "");

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O3", data.Part1, 1m, WhsPickOption.Codes.Manual);
			Factory.Save();

			var pick = Helper.CreatePickNew(order);
			pick.WP_IsAwaitingReplenishment = true;
			Factory.Save();

			AllocateAwaitingReplenishmentPicksForTest();
			AssertEquals("Information|Pick No: P00000001 did not find existing stock that could be allocated.", Logger.ToString().Trim());
		}

		#endregion

		#region TestAllocateAwaitingReplenishmentPicks_AutoAllocatePicksAlthoughNoneOfThemHasEnoughToFullfill

		public void TestAllocateAwaitingReplenishmentPicks_AutoAllocatePicksAlthoughNoneOfThemHasEnoughToFullfill()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var ruleSet = AllocationRulesHelper.MakeNewRuleSetAndDeactivateSystemAllocationRuleSet(Factory);
			AllocationRulesHelper.AddFullPalletRule(ruleSet, 5);
			AllocationRulesHelper.AddPickFaceRules(ruleSet, 10);
			AllocationRulesHelper.AddFifoRule(ruleSet, 20, preventPickingPickFacesFromBulk: true);

			Helper.CreateProductUnit(data.Part1, Constants.PkgUnit.Pallet, 3m); // Part1 : 1PLT = 2 UNT

			var pickFaceLocation = data.Whs1.FindLocation("A-1");
			var bulkLocation = data.Whs1.FindLocation("A-2");
			Helper.CreateProductPickFace(data.Part1, data.Org1, pickFaceLocation);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", ZDateTimeOffset.Today, data.Part1, 2m, bulkLocation, ""); // broken pallet
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", ZDateTimeOffset.Today, data.Part1, 3m, pickFaceLocation, "");
			Factory.Save();

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 5m, WhsPickOption.Codes.Manual);
			var pick1 = Helper.CreatePickNew(order1);
			pick1.AutoAllocateItems();
			AssertEquals(true, pick1.WP_IsAwaitingReplenishment);
			Factory.Save();

			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O2", data.Part1, 2m, WhsPickOption.Codes.Manual);
			var pick2 = Helper.CreatePickNew(order2);
			pick2.AutoAllocateItems();
			AssertEquals(true, pick2.WP_IsAwaitingReplenishment);
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "T1", Notify);
			Helper.CreateWhsTransferLine(transfer, data.Part1, 1m, bulkLocation, pickFaceLocation);
			transfer.FinaliseDocket();
			AssertEquals(true, transfer.IsFinalised);
			Factory.Save();

			AllocateAwaitingReplenishmentPicksForTest();
			AssertEquals("No errors should be shown.", @"Information|Pick No: P00000001 has successfully allocated stock.
Pick No: P00000002 did not find existing stock that could be allocated.".Trim(), Logger.ToString().Trim());

			var newFactoryToLoadPicks = new BusinessObjectFactory();
			var pick1InNewFactory = newFactoryToLoadPicks.Load<WhsPick>(pick1.PK);
			var pick2InNewFactory = newFactoryToLoadPicks.Load<WhsPick>(pick2.PK);
			AssertEquals(true, pick1InNewFactory.WP_IsAwaitingReplenishment);
			AssertEquals(true, pick2InNewFactory.WP_IsAwaitingReplenishment);

			var avaialbleInventoriesForPick1 = pick1InNewFactory.OrderedInventories.Cast<WhsPickOrderedInventory>().Single().AvailableInventories.Cast<WhsPickAvailableInventory>();
			AssertEquals(false, avaialbleInventoriesForPick1.Single(a => a.LocationPK == bulkLocation.PK).Allocate);
			var availableInventoryLineForPick1InPickface = avaialbleInventoriesForPick1.Single(a => a.LocationPK == pickFaceLocation.PK);
			AssertEquals(true, availableInventoryLineForPick1InPickface.Allocate);
			AssertEquals(4m, availableInventoryLineForPick1InPickface.PickLineQuantity);
			AssertEquals(4m, availableInventoryLineForPick1InPickface.QuantityAvailableToPick);
			AssertEquals(4m, availableInventoryLineForPick1InPickface.QuantityCommitted);

			var avaialbleInventoriesForPick2 = pick2InNewFactory.OrderedInventories.Cast<WhsPickOrderedInventory>().Single().AvailableInventories.Cast<WhsPickAvailableInventory>();
			AssertEquals(false, avaialbleInventoriesForPick2.Single(a => a.LocationPK == bulkLocation.PK).Allocate);
			var availableInventoryLineForPick2InPickface = avaialbleInventoriesForPick2.Single(a => a.LocationPK == pickFaceLocation.PK);
			AssertEquals(false, availableInventoryLineForPick2InPickface.Allocate);
			AssertEquals(0m, availableInventoryLineForPick2InPickface.PickLineQuantity);
			AssertEquals(0m, availableInventoryLineForPick2InPickface.QuantityAvailableToPick);
			AssertEquals(4m, availableInventoryLineForPick2InPickface.QuantityCommitted);
		}

		#endregion

		#region TestAllocateAwaitingReplenishmentPicks_AutoAllocatePicksWithPickPriority

		public void TestAllocateAwaitingReplenishmentPicks_AutoAllocatePicksWithPickPriority()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var ruleSet = AllocationRulesHelper.MakeNewRuleSetAndDeactivateSystemAllocationRuleSet(Factory);
			AllocationRulesHelper.AddPickFaceRules(ruleSet, 10);
			AllocationRulesHelper.AddFifoRule(ruleSet, 20, preventPickingPickFacesFromBulk: true);

			Helper.CreateProductUnit(data.Part1, Constants.PkgUnit.Pallet, 3m); // Part1 : 1PLT = 3 UNT

			var pickFaceLocation = data.Whs1.FindLocation("A-1");
			var bulkLocation = data.Whs1.FindLocation("A-2");
			Helper.CreateProductPickFace(data.Part1, data.Org1, pickFaceLocation);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", ZDateTimeOffset.Today, data.Part1, 40m, bulkLocation, "");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", ZDateTimeOffset.Today, data.Part1, 15m, pickFaceLocation, "");
			Factory.Save();

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 16m, WhsPickOption.Codes.Manual);
			order1.WD_PickPriority = 0;
			var pick1 = Helper.CreatePickNew(order1);
			pick1.AutoAllocateItems();
			AssertEquals(true, pick1.WP_IsAwaitingReplenishment);
			Factory.Save();

			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O2", data.Part1, 16m, WhsPickOption.Codes.Manual);
			order2.WD_PickPriority = 5;
			var pick2 = Helper.CreatePickNew(order2);
			pick2.AutoAllocateItems();
			AssertEquals(true, pick2.WP_IsAwaitingReplenishment);
			Factory.Save();

			var order3 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O3", data.Part1, 16m, WhsPickOption.Codes.Manual);
			var pick3 = Helper.CreatePickNew(order3);
			order3.WD_PickPriority = 3;
			pick3.AutoAllocateItems();
			AssertEquals(true, pick3.WP_IsAwaitingReplenishment);
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "T1", Notify);
			Helper.CreateWhsTransferLine(transfer, data.Part1, 16m, bulkLocation, pickFaceLocation);
			transfer.FinaliseDocket();
			AssertEquals(true, transfer.IsFinalised);
			Factory.Save();

			using (WarehouseDataRegistry.Instance.DeallocateLowerPriorityPicksWhenAllocatingWaitingReplenishmentPicks.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				AllocateAwaitingReplenishmentPicksForTest();
				AssertEquals("No errors should be shown.", @"Information|Pick No: P00000003 has successfully allocated stock.
Pick No: P00000002 did not find existing stock that could be allocated.
Pick No: P00000001 did not find existing stock that could be allocated.".Trim(), Logger.ToString().Trim());

				var newFactoryToLoadPicks = new BusinessObjectFactory();
				var pick1InNewFactory = newFactoryToLoadPicks.Load<WhsPick>(pick1.PK);
				var pick2InNewFactory = newFactoryToLoadPicks.Load<WhsPick>(pick2.PK);
				var pick3InNewFactory = newFactoryToLoadPicks.Load<WhsPick>(pick3.PK);
				AssertEquals(true, pick1InNewFactory.WP_IsAwaitingReplenishment);
				AssertEquals(true, pick2InNewFactory.WP_IsAwaitingReplenishment);
				AssertEquals(false, pick3InNewFactory.WP_IsAwaitingReplenishment);
				AssertEquals(PickStatus.Codes.Created, pick3InNewFactory.WP_PickStatus);

				var avaialbleInventoriesForPick1 = pick1InNewFactory.OrderedInventories.Cast<WhsPickOrderedInventory>().Single().AvailableInventories.Cast<WhsPickAvailableInventory>();
				AssertEquals(false, avaialbleInventoriesForPick1.Single(a => a.LocationPK == bulkLocation.PK).Allocate);
				var availableInventoryLineForPick1InPickface = avaialbleInventoriesForPick1.Single(a => a.LocationPK == pickFaceLocation.PK);
				AssertEquals(true, availableInventoryLineForPick1InPickface.Allocate);
				AssertEquals(15m, availableInventoryLineForPick1InPickface.PickLineQuantity);
				AssertEquals(15m, availableInventoryLineForPick1InPickface.QuantityAvailableToPick);
				AssertEquals(31m, availableInventoryLineForPick1InPickface.QuantityCommitted);

				var avaialbleInventoriesForPick2 = pick2InNewFactory.OrderedInventories.Cast<WhsPickOrderedInventory>().Single().AvailableInventories.Cast<WhsPickAvailableInventory>();
				AssertEquals(false, avaialbleInventoriesForPick2.Single(a => a.LocationPK == bulkLocation.PK).Allocate);
				var availableInventoryLineForPick2InPickface = avaialbleInventoriesForPick2.Single(a => a.LocationPK == pickFaceLocation.PK);
				AssertEquals(false, availableInventoryLineForPick2InPickface.Allocate);
				AssertEquals(0m, availableInventoryLineForPick2InPickface.PickLineQuantity);
				AssertEquals(0m, availableInventoryLineForPick2InPickface.QuantityAvailableToPick);
				AssertEquals(31m, availableInventoryLineForPick2InPickface.QuantityCommitted);

				var avaialbleInventoriesForPick3 = pick3InNewFactory.OrderedInventories.Cast<WhsPickOrderedInventory>().Single().AvailableInventories.Cast<WhsPickAvailableInventory>();
				AssertEquals(false, avaialbleInventoriesForPick3.Single(a => a.LocationPK == bulkLocation.PK).Allocate);
				var availableInventoryLineForPick3InPickface = avaialbleInventoriesForPick3.Single(a => a.LocationPK == pickFaceLocation.PK);
				AssertEquals(true, availableInventoryLineForPick3InPickface.Allocate);
				AssertEquals(16m, availableInventoryLineForPick3InPickface.PickLineQuantity);
				AssertEquals(16m, availableInventoryLineForPick3InPickface.QuantityAvailableToPick);
				AssertEquals(31m, availableInventoryLineForPick3InPickface.QuantityCommitted);
			}
		}

		#endregion

		#region TestDBHitsForTransferCreation

		[GuiTest]
		[StressTest]
		public void TestDBHitsForPickAllocation_Order()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var ruleSet = AllocationRulesHelper.MakeNewRuleSetAndDeactivateSystemAllocationRuleSet(Factory);
			AllocationRulesHelper.AddPickFaceRules(ruleSet, 10);
			AllocationRulesHelper.AddFifoRule(ruleSet, 20, preventPickingPickFacesFromBulk: true);

			for (var pickCount = 0; pickCount < 5; pickCount++)
			{
				var orderList = new List<WhsOrder>();
				for (var orderCount = 0; orderCount < 5; orderCount++)
				{
					var client = Helper.CreateClient("C1" + pickCount.ToString() + orderCount.ToString());
					var orderForProduct = Helper.CreateWhsOrder(client, data.Whs1, "O" + pickCount.ToString() + orderCount.ToString(), WhsPickOption.Codes.Manual);
					for (var orderLineCount = 0; orderLineCount < 5; orderLineCount++)
					{
						var product = Helper.CreateProduct(client, "PR1" + pickCount.ToString() + orderCount.ToString() + orderLineCount.ToString());
						Helper.CreateProductUnit(product, Constants.PkgUnit.Pallet, 2m); // Part1 : 1PLT = 2 UNT

						var pickFaceLocation = Helper.CreateRowAndGenerateLocations(data.Whs1,
							"R" + pickCount.ToString() + orderCount.ToString() + orderLineCount.ToString(), 1, 1).Locations.Single();
						Helper.CreateProductPickFace(product, client, pickFaceLocation, 0m, 6m, 1m); // Min: 0 so it needs to get replenished
						Helper.CreateWhsReceiveWithInventory(client, data.Whs1, "R" + pickCount.ToString() + orderCount.ToString() + orderLineCount.ToString(),
							ZDateTimeOffset.Today.AddDays(-1), product, 2m, pickFaceLocation, "", false);

						Helper.CreateWhsOrderLine(orderForProduct, product, 1m);
					}

					orderList.Add(orderForProduct);
				}

				var waitingReplenishmentPick = Helper.CreatePickNew(orderList.ToArray()); // 1 pick should have 50 ordered inventory lines
				waitingReplenishmentPick.WP_IsAwaitingReplenishment = true;
			}
			Factory.Save();

			var hits = new Dictionary<string, int>
			{
				{ AccAllowedBranchDepartmentComboSchema.Constants.TableName, 1 },
				{ CusRefTradeGroupViewSchema.Constants.TableName, 1 },
				{ JobDocAddressSchema.Constants.TableName, 5 },
				{ OrgAddressSchema.Constants.TableName, 10 },
				{ OrgAddressCapabilitySchema.Constants.TableName, 5 },
				{ OrgCompanyDataSchema.Constants.TableName, 5 },
				{ OrgContactSchema.Constants.TableName, 5 },
				{ OrgCusCodeSchema.Constants.TableName, 5 },
				{ OrgCustomLabelsSchema.Constants.TableName, 6 },
				{ OrgHeaderSchema.Constants.TableName, 6 },
				{ OrgMiscServSchema.Constants.TableName, 5 },
				{ OrgPartRelationSchema.Constants.TableName, 5 },
				{ OrgPartUnitSchema.Constants.TableName, 5 },
				{ OrgSupplierPartSchema.Constants.TableName, 5 },
				{ ProcessTasksSchema.Constants.TableName, 10 },
				{ ProcessCompanyLinkRuleSchema.Constants.TableName, 5 },
				{ ProcessTaskTemplateSchema.Constants.TableName, 10 },
				{ ProductionRuleSchema.Constants.TableName, 5 },
				{ RefPacksSchema.Constants.TableName, 5 },
				{ StmALogSchema.Constants.TableName, 5 },
				{ StmEventSchema.Constants.TableName, 10 },
				{ StmNoteSchema.Constants.TableName, 10 },
				{ WhsAreaSchema.Constants.TableName, 5 },
				{ WhsDocketSchema.Constants.TableName, 11 },
				{ WhsDocketContainerSchema.Constants.TableName, 5 },
				{ WhsDocketLineSchema.Constants.TableName, 10 },
				{ WhsInventoryViewSchema.Constants.TableName, 5 },
				{ WhsLocationViewSchema.Constants.TableName, 10 }, // One for loading avail invs, one for validating WP_WL_DockDoor
				{ WhsPickSchema.Constants.TableName, 6 },
				{ WhsPickLineSchema.Constants.TableName, 20 }, // One for loading picklines for committed qty, one for loading picklines for available qty and then factory cache is reset on pick validation which reloads both again during validation
				{ WhsWarehouseSchema.Constants.TableName, 5 },
				{ PkgPackageJobSchema.Constants.TableName, 5 }, // Due to access to package job outer package to calculate total sent quantities
				{ PkgPackageSchema.Constants.TableName, 5 },
				{ WhsLocationTypeSchema.Constants.TableName, 10 },
				{ WhsProductParamsByWhsAndClientSchema.Constants.TableName, 5 }
			};

			using (AssertDbHitsForAllFactories(hits, useOnlyNewFactories: true, ignoreHitsFromTablesCachedInUberFactory: true))
			{
				AllocateAwaitingReplenishmentPicksForTest();
			}
		}

		[StressTest]
		public void TestDBHitsForPickAllocation_WorkOrder()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var ruleSet = AllocationRulesHelper.MakeNewRuleSetAndDeactivateSystemAllocationRuleSet(Factory);
			AllocationRulesHelper.AddFullPalletRule(ruleSet, 5);
			AllocationRulesHelper.AddPickFaceRules(ruleSet, 10);
			AllocationRulesHelper.AddFifoRule(ruleSet, 20, preventPickingPickFacesFromBulk: true);

			for (int pickCount = 0; pickCount < 5; pickCount++)
			{
				var workOrderList = new List<WhsWorkOrder>();

				var client = Helper.CreateClient("C1" + pickCount.ToString());
				var mainProduct = Helper.CreateProduct(client, $"P1{pickCount}");
				var subProduct = Helper.CreateProduct(client, $"P2{pickCount}");
				var bom = Helper.CreateProductBOM(mainProduct, subProduct, 2m, "UNT");
				Helper.CreateProductUnit(subProduct, Constants.PkgUnit.Pallet, 2m);

				var pickFaceLocation = Helper.CreateRowAndGenerateLocations(data.Whs1, $"R{pickCount}", 1, 1).Locations.Single();
				Helper.CreateProductPickFace(subProduct, client, pickFaceLocation, 0m, 6m, 1m); // Min: 0 so it needs to get replenished

				var receive = Helper.CreateWhsReceiveWithInventory(client, data.Whs1, $"R{pickCount}", subProduct, 50m, pickFaceLocation, "");
				AssertIsFinalisedPrecondition(receive);
				Factory.Save();

				var workOrderForProduct = Helper.CreateWhsWorkOrder(client, data.Whs1, $"O{pickCount}");
				for (int workOrderLineCount = 0; workOrderLineCount < 25; workOrderLineCount++)
				{
					Helper.CreateWhsWorkOrderLine(workOrderForProduct, mainProduct, 1m);
				}

				workOrderList.Add(workOrderForProduct);

				var waitingReplenishmentPick = Helper.CreatePickNew(workOrderList.ToArray()); // 1 pick should have 1 ordered inventory line
				waitingReplenishmentPick.WP_IsAwaitingReplenishment = true;
			}
			Factory.Save();

			var hits = new Dictionary<string, int>
			{
				{ AccAllowedBranchDepartmentComboSchema.Constants.TableName, 1 },
				{ ProcessTasksSchema.Constants.TableName, 5 },
				{ ProcessTaskTemplateSchema.Constants.TableName, 5 },
				{ OrgAddressSchema.Constants.TableName, 10 },
				{ OrgCompanyDataSchema.Constants.TableName, 5 },
				{ OrgHeaderSchema.Constants.TableName, 6 },
				{ OrgMiscServSchema.Constants.TableName, 5 },
				{ OrgPartRelationSchema.Constants.TableName, 5 },
				{ OrgPartBOMSchema.Constants.TableName, 5 },
				{ OrgPartUnitSchema.Constants.TableName, 5 },
				{ OrgSupplierPartSchema.Constants.TableName, 5 },
				{ OrgCustomLabelsSchema.Constants.TableName, 6 },
				{ WhsAreaSchema.Constants.TableName, 5 },
				{ WhsDocketSchema.Constants.TableName, 11 },
				{ WhsDocketLineSchema.Constants.TableName, 15 },
				{ WhsInventoryViewSchema.Constants.TableName, 5 },
				{ WhsLocationViewSchema.Constants.TableName, 5 }, // Once for loading avail invs, the other for checking warehouse on pick faces
				{ WhsPickSchema.Constants.TableName, 6 },
				{ WhsPickLineSchema.Constants.TableName, 25 }, // One for loading picklines for committed qty, one for loading picklines for available qty then on pick validation factory cache is reset which reloads both again during validation and one more hit for re-synced existing picklines prior to pick validation
				{ WhsWarehouseSchema.Constants.TableName, 5 },
				{ StmEventSchema.Constants.TableName, 5 },
			};

			QueryStackTraceRecorder.Instance.Enabled = true;
			using (AssertDbHitsForAllFactories(hits, useOnlyNewFactories: true, ignoreHitsFromTablesCachedInUberFactory: true))
			{
				AllocateAwaitingReplenishmentPicksForTest();
			}
		}

		#endregion

		#region TestAllocateAwaitingReplenishmentPicks_PicksWithValidationErrors

		public void TestAllocateAwaitingReplenishmentPicks_PicksWithValidationErrors()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var ruleSet = AllocationRulesHelper.MakeNewRuleSetAndDeactivateSystemAllocationRuleSet(Factory);
			AllocationRulesHelper.AddFullPalletRule(ruleSet, 5);
			AllocationRulesHelper.AddPickFaceRules(ruleSet, 10);
			AllocationRulesHelper.AddFifoRule(ruleSet, 20, preventPickingPickFacesFromBulk: true);

			Helper.CreateProductUnit(data.Part1, Constants.PkgUnit.Pallet, 3m); // Part1 : 1PLT = 2 UNT
			Helper.CreateProductUnit(data.Part2, Constants.PkgUnit.Pallet, 3m); // Part1 : 1PLT = 2 UNT

			var pickFaceLocation = data.Whs1.FindLocation("A-1");
			var bulkLocation = data.Whs1.FindLocation("A-2");
			Helper.CreateProductPickFace(data.Part1, data.Org1, pickFaceLocation);
			Helper.CreateProductPickFace(data.Part2, data.Org1, pickFaceLocation);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", ZDateTimeOffset.Today, data.Part1, 2m, bulkLocation, ""); // broken pallet
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", ZDateTimeOffset.Today, data.Part1, 3m, pickFaceLocation, "");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R3", ZDateTimeOffset.Today, data.Part2, 1m, bulkLocation, ""); // broken pallet
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R4", ZDateTimeOffset.Today, data.Part2, 3m, pickFaceLocation, "");
			Factory.Save();

			var orderWithErrors1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 5m, WhsPickOption.Codes.Manual);
			var pickWithErrors1 = Helper.CreatePickNew(orderWithErrors1);
			pickWithErrors1.AutoAllocateItems();
			orderWithErrors1.WD_DropMode = "SSS"; // hack into the system to make a validation error
			AssertEquals("Precondition", true, pickWithErrors1.WP_IsAwaitingReplenishment);
			Factory.Save();

			var orderWithErrors2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O2", data.Part2, 5m, WhsPickOption.Codes.Manual);
			var pickWithErrors2 = Helper.CreatePickNew(orderWithErrors2);
			pickWithErrors2.AutoAllocateItems();
			orderWithErrors2.WD_DropMode = "SSS"; // hack into the system to make a validation error
			AssertEquals("Precondition", true, pickWithErrors2.WP_IsAwaitingReplenishment);
			Factory.Save();

			var orderWithoutErrors = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O3", data.Part1, 2m, WhsPickOption.Codes.Manual);
			var pickWithoutErrors = Helper.CreatePickNew(orderWithoutErrors);
			pickWithoutErrors.AutoAllocateItems();
			AssertEquals("Precondition", true, pickWithoutErrors.WP_IsAwaitingReplenishment);
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "T1", Notify);
			Helper.CreateWhsTransferLine(transfer, data.Part1, 1m, bulkLocation, pickFaceLocation);
			transfer.FinaliseDocket();
			AssertEquals(true, transfer.IsFinalised);
			Factory.Save();

			AllocateAwaitingReplenishmentPicksForTest();

			AssertMultilineASCIIEquals("There should be one pick with validation errors",
@"Information|Pick No: P00000002 did not find existing stock that could be allocated.
Pick No: P00000003 has successfully allocated stock.
Error|Pick No: P00000001 could not be allocated due to following errors:
Error - WD_DropMode: Enter a valid Drop Mode.",
			Logger.ToString());
		}

		#endregion

		#region TestAllocateAwaitingReplenishmentPicks_WaitingReplenishmentPicksWithNotEnoughStock

		public void TestAllocateAwaitingReplenishmentPicks_WaitingReplenishmentPicksWithConcurrencyIssues()
		{
			// Setup data and pick algorithm
			var data = new TestDataSimpleEnvironment(Factory);
			var ruleSet = AllocationRulesHelper.MakeNewRuleSetAndDeactivateSystemAllocationRuleSet(Factory);
			AllocationRulesHelper.AddFullPalletRule(ruleSet, 5);
			AllocationRulesHelper.AddPickFaceRules(ruleSet, 10);
			AllocationRulesHelper.AddFifoRule(ruleSet, 20, preventPickingPickFacesFromBulk: true);

			Helper.CreateProductUnit(data.Part1, Constants.PkgUnit.Pallet, 2m); // Part1 : 1PLT = 2 UNT

			var pickFaceLocation = data.Whs1.FindLocation("A");
			Helper.CreateProductPickFace(data.Part1, data.Org1, pickFaceLocation);
			Helper.CreateProductPickFace(data.Part2, data.Org1, pickFaceLocation);

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", ZDateTimeOffset.Today, data.Part1, 2m, pickFaceLocation, "");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", ZDateTimeOffset.Today, data.Part2, 2m, pickFaceLocation, "");

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O3", data.Part1, 1m, WhsPickOption.Codes.Manual);
			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O4", data.Part2, 1m, WhsPickOption.Codes.Manual);
			var pick1 = Helper.CreatePickNew(order1);
			var pick2 = Helper.CreatePickNew(order2);
			pick1.WP_IsAwaitingReplenishment = true;
			pick2.WP_IsAwaitingReplenishment = true;
			Factory.Save();

			var manager = new AllocatePicksAwaitingReplenishmentWithExistingInventoryProcessingManager(ObjectFactory.Get<IPartiallyReplenishedPickSplitter>());
			var shouldModifyData = true;
			manager.OnNewFactorySaving += delegate
			{
				if (shouldModifyData)
				{
					CargoWise.Database.TestFramework.ObjectModel.WhsDocket
						.UpdateWhere(l => l.PK == order1.PK.ToGuid())
						.Set(l => l.WD_SystemLastEditUser, "S1")
						.Set(l => l.WD_UnitsSent, 3).Post(Db.Connection); // For a unit test
				}
				shouldModifyData = false;
			};

			AssertNoExceptionThrown(() => manager.AllocateAwaitingReplenishmentPicks(Logger, new CancellationToken()));

			AssertEquals(
@"Information|Pick No: P00000002 has successfully allocated stock.
Error|Pick No: P00000001 could not be allocated. While service task was allocating inventory someone else has changed Pick or Order(s). This pick would be allocated when service task run next time.", Logger.ToString().Trim());
		}

		#endregion

		#region TestAllocateAwaitingReplenishmentPicks_UsesWarehouseBranchTimeZoneForReceives

		public void TestAllocateAwaitingReplenishmentPicks_UsesWarehouseBranchTimeZoneForReceives()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var aucklandPort = new RefUNLOCO.Loader(Factory).Load("NZAKL");
			var utcNow = ZDateTime.UtcNow;

			// create a branch with Auckland time zone and set the Whs to this branch / time zone
			var branch = Factory.NewWithValidTestData<GlbCompany>().Branches.AddNew();
			branch.GB_RL_NKHomePort = aucklandPort.RL_Code;
			data.Whs1.WW_GB_RelatedCompanyBranch = branch.PK;

			var localNZTime = data.Whs1.GetWarehouseBranchDateTimeOffset(utcNow);
			AssertNotEquals("UTC Now and Local NZ Time must differ to ensure the integrity of this test.", utcNow, localNZTime);
			AssertNotEquals("Local Time and Local NZ Time must differ to ensure the integrity of this test.", utcNow.ToDateTime().ToLocalTime(), localNZTime); // test must be run under non-NZ branch

			// create an order for stock that does not yet exist
			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 3, WhsPickOption.Codes.Manual);
			var pick = Helper.CreatePickNew(order);
			pick.WP_IsAwaitingReplenishment = true;

			// create inventory
			var receiveNow = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 1m);
			var receive5MinsAgo = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 1m);
			var receive5MinsInFuture = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R3", data.Part1, 1m);
			receiveNow.WD_FinalisedDate = localNZTime;
			receive5MinsAgo.WD_FinalisedDate = localNZTime.AddMinutes(-5);
			receive5MinsInFuture.WD_FinalisedDate = localNZTime.AddMinutes(5);

			// save order and inventory
			Factory.Save();

			// run the allocation and ensure we only allocate stock from receives no later than the current date/time
			AllocateAwaitingReplenishmentPicksForTest();
			var newFactory = new BusinessObjectFactory();
			var orderInNewFactory = newFactory.Load<WhsOrder>(order.PK);
			var receiveLinesUsedForPick = orderInNewFactory.Lines[0].PickLines.Select(pl => pl.Inventory.InDocketLine);
			AssertContainsExactElementsInAnyOrder(new[] { receiveNow.Lines.Single().PK, receive5MinsAgo.Lines.Single().PK }, receiveLinesUsedForPick.Select(l => l.PK));
		}

		#endregion

		#region TestAllocateAwaitingReplenishmentPicks_CartonisationSelected

		public void TestAllocateAwaitingReplenishmentPicks_CartonisationSelected_CartoniseSplitCases()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var ruleSet = AllocationRulesHelper.MakeNewRuleSetAndDeactivateSystemAllocationRuleSet(Factory);
			AllocationRulesHelper.AddPickFaceRules(ruleSet, 10);
			AllocationRulesHelper.AddFifoRule(ruleSet, 20, preventPickingPickFacesFromBulk: true);

			data.Whs1.WW_IsPickByUOMEnabled = true;
			var whsGroup = Helper.CreateWhsCartonGroup("1", "WhsOrg");
			var whsSize1 = Helper.CreateWhsCartonSize("WH", 1, 2, 4, 8, 16, 32, new ZByte(80), Constants.Length.Metres, Constants.Weight.Kilograms);
			whsGroup.CartonSizes.Add(whsSize1);
			data.Whs1.WarehouseAddress.Header.MiscServ.OM_WCG_CartonGroup = whsGroup.PK;

			var refType = PackingHelper.CreateRefPackType("1", "1", 2m, 4m, 8m, Constants.Length.Metres, 3, Constants.Weight.Kilograms, UOMPackTypesList.Codes.SplitCase);
			data.Part1.OP_StockKeepingUnit = refType.F3_Code;

			var pickFaceLocation = data.Whs1.FindLocation("A-1");
			var bulkLocation = data.Whs1.FindLocation("A-2");
			Helper.CreateProductPickFace(data.Part1, data.Org1, pickFaceLocation);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", ZDateTimeOffset.Today, data.Part1, 40m, bulkLocation, "");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", ZDateTimeOffset.Today, data.Part1, 15m, pickFaceLocation, "");
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 16m, WhsPickOption.Codes.Manual);
			var pick = Helper.CreatePickNew(order);
			pick.AutoAllocateItems();
			AssertEquals("Precondition", true, pick.WP_IsAwaitingReplenishment);
			AssertEquals("Precondition", false, pick.WP_IsCartonised);
			pick.WP_CartoniseSplitCases = true;
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "T1", Notify);
			Helper.CreateWhsTransferLine(transfer, data.Part1, 16m, bulkLocation, pickFaceLocation);
			transfer.FinaliseDocket();
			AssertEquals(true, transfer.IsFinalised);
			Factory.Save();

			AllocateAwaitingReplenishmentPicksForTest();

			AssertEquals("Pick is not waiting replenishment anymore.", PickStatus.Codes.Created, pick.WP_PickStatus);
			AssertEquals(true, pick.WP_IsCartonised);
		}

		public void TestAllocateAwaitingReplenishmentPicks_CartonisationSelected_PickCasesByLabel()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var ruleSet = AllocationRulesHelper.MakeNewRuleSetAndDeactivateSystemAllocationRuleSet(Factory);
			AllocationRulesHelper.AddPickFaceRules(ruleSet, 10);
			AllocationRulesHelper.AddFifoRule(ruleSet, 20, preventPickingPickFacesFromBulk: true);

			data.Whs1.WW_IsPickByUOMEnabled = true;
			var refType = PackingHelper.CreateRefPackType("1", "1", 2m, 4m, 8m, Constants.Length.Metres, 3, Constants.Weight.Kilograms, UOMPackTypesList.Codes.Case);
			data.Part1.OP_StockKeepingUnit = refType.F3_Code;

			var pickFaceLocation = data.Whs1.FindLocation("A-1");
			var bulkLocation = data.Whs1.FindLocation("A-2");
			Helper.CreateProductPickFace(data.Part1, data.Org1, pickFaceLocation);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", ZDateTimeOffset.Today, data.Part1, 40m, bulkLocation, "");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", ZDateTimeOffset.Today, data.Part1, 15m, pickFaceLocation, "");
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 16m, WhsPickOption.Codes.Manual);
			var pick = Helper.CreatePickNew(order);
			pick.AutoAllocateItems();
			AssertEquals("Precondition", true, pick.WP_IsAwaitingReplenishment);
			AssertEquals("Precondition", false, pick.WP_IsCartonised);
			pick.WP_PickCasesByLabel = true;
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "T1", Notify);
			Helper.CreateWhsTransferLine(transfer, data.Part1, 16m, bulkLocation, pickFaceLocation);
			transfer.FinaliseDocket();
			AssertEquals(true, transfer.IsFinalised);
			Factory.Save();

			AllocateAwaitingReplenishmentPicksForTest();

			AssertEquals("Pick is not waiting replenishment anymore.", PickStatus.Codes.Created, pick.WP_PickStatus);
			AssertEquals(true, pick.WP_IsCartonised);
		}

		public void TestAllocateAwaitingReplenishmentPicks_CartonisationSelected_PickPalletsByLabel()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var ruleSet = AllocationRulesHelper.MakeNewRuleSetAndDeactivateSystemAllocationRuleSet(Factory);
			AllocationRulesHelper.AddPickFaceRules(ruleSet, 10);
			AllocationRulesHelper.AddFifoRule(ruleSet, 20, preventPickingPickFacesFromBulk: true);

			data.Whs1.WW_IsPickByUOMEnabled = true;
			var refType = PackingHelper.CreateRefPackType("1", "1", 2m, 4m, 8m, Constants.Length.Metres, 3, Constants.Weight.Kilograms, UOMPackTypesList.Codes.Pallet);
			data.Part1.OP_StockKeepingUnit = refType.F3_Code;

			var pickFaceLocation = data.Whs1.FindLocation("A-1");
			var bulkLocation = data.Whs1.FindLocation("A-2");
			Helper.CreateProductPickFace(data.Part1, data.Org1, pickFaceLocation);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", ZDateTimeOffset.Today, data.Part1, 40m, bulkLocation, "");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", ZDateTimeOffset.Today, data.Part1, 15m, pickFaceLocation, "");
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 16m, WhsPickOption.Codes.Manual);
			var pick = Helper.CreatePickNew(order);
			pick.AutoAllocateItems();
			AssertEquals("Precondition", true, pick.WP_IsAwaitingReplenishment);
			AssertEquals("Precondition", false, pick.WP_IsCartonised);
			pick.WP_PickPalletsByLabel = true;
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "T1", Notify);
			Helper.CreateWhsTransferLine(transfer, data.Part1, 16m, bulkLocation, pickFaceLocation);
			transfer.FinaliseDocket();
			AssertEquals(true, transfer.IsFinalised);
			Factory.Save();

			AllocateAwaitingReplenishmentPicksForTest();

			AssertEquals("Pick is not waiting replenishment anymore.", PickStatus.Codes.Created, pick.WP_PickStatus);
			AssertEquals(true, pick.WP_IsCartonised);
		}

		#endregion

		#region TestAllocateAwaitingReplenishmentPicks_CartonisationSelected_ErrorDuringAllocatingAwaitingReplenishmentPicks

		public void TestAllocateAwaitingReplenishmentPicks_CartonisationSelected_ErrorDuringAllocatingAwaitingReplenishmentPicks()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var ruleSet = AllocationRulesHelper.MakeNewRuleSetAndDeactivateSystemAllocationRuleSet(Factory);
			AllocationRulesHelper.AddPickFaceRules(ruleSet, 10);
			AllocationRulesHelper.AddFifoRule(ruleSet, 20, preventPickingPickFacesFromBulk: true);

			data.Whs1.WW_IsPickByUOMEnabled = true;
			var refType = PackingHelper.CreateRefPackType("1", "1", 2m, 4m, 8m, Constants.Length.Metres, 3, Constants.Weight.Kilograms, UOMPackTypesList.Codes.Pallet);
			data.Part1.OP_StockKeepingUnit = refType.F3_Code;

			var pickFaceLocation = data.Whs1.FindLocation("A-1");
			var bulkLocation = data.Whs1.FindLocation("A-2");
			Helper.CreateProductPickFace(data.Part1, data.Org1, pickFaceLocation);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", ZDateTimeOffset.Today, data.Part1, 40m, bulkLocation, "");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", ZDateTimeOffset.Today, data.Part1, 15m, pickFaceLocation, "");
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 16m, WhsPickOption.Codes.Manual);
			var pick = Helper.CreatePickNew(order);
			pick.AutoAllocateItems();
			AssertEquals("Precondition", true, pick.WP_IsAwaitingReplenishment);
			AssertEquals("Precondition", false, pick.WP_IsCartonised);
			pick.WP_PickPalletsByLabel = true;
			order.WD_DropMode = "SSS"; // force a validation error on pick
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "T1", Notify);
			Helper.CreateWhsTransferLine(transfer, data.Part1, 16m, bulkLocation, pickFaceLocation);
			transfer.FinaliseDocket();
			AssertEquals(true, transfer.IsFinalised);
			Factory.Save();

			AllocateAwaitingReplenishmentPicksForTest();

			AssertEquals(false, pick.WP_IsCartonised);
			AssertMultilineASCIIEquals("There should be an error on the logs",
@"Error|Pick No: P00000001 could not be allocated due to following errors:
Error - WD_DropMode: Enter a valid Drop Mode.",
			Logger.ToString());
		}

		#endregion

		#region TestAllocateAwaitingReplenishmentPicks_CartonisationSelected_PickStillAwaitingReplenishmentAfterAllocatingPick

		public void TestAllocateAwaitingReplenishmentPicks_CartonisationSelected_PickStillAwaitingReplenishmentAfterAllocatingPick()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var ruleSet = AllocationRulesHelper.MakeNewRuleSetAndDeactivateSystemAllocationRuleSet(Factory);
			AllocationRulesHelper.AddPickFaceRules(ruleSet, 10);
			AllocationRulesHelper.AddFifoRule(ruleSet, 20, preventPickingPickFacesFromBulk: true);

			data.Whs1.WW_IsPickByUOMEnabled = true;
			var refType = PackingHelper.CreateRefPackType("1", "1", 2m, 4m, 8m, Constants.Length.Metres, 3, Constants.Weight.Kilograms, UOMPackTypesList.Codes.Pallet);
			data.Part1.OP_StockKeepingUnit = refType.F3_Code;

			var pickFaceLocation = data.Whs1.FindLocation("A-1");
			var bulkLocation = data.Whs1.FindLocation("A-2");
			Helper.CreateProductPickFace(data.Part1, data.Org1, pickFaceLocation);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", ZDateTimeOffset.Today, data.Part1, 40m, bulkLocation, "");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", ZDateTimeOffset.Today, data.Part1, 15m, pickFaceLocation, "");
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 16m, WhsPickOption.Codes.Manual);
			var pick = Helper.CreatePickNew(order);
			pick.AutoAllocateItems();
			AssertEquals("Precondition", true, pick.WP_IsAwaitingReplenishment);
			AssertEquals("Precondition", false, pick.WP_IsCartonised);
			pick.WP_PickPalletsByLabel = true;
			Factory.Save();

			AllocateAwaitingReplenishmentPicksForTest();
			AssertEquals("Pick is still waiting replenishment.", true, pick.WP_IsAwaitingReplenishment);
			AssertEquals(false, pick.WP_IsCartonised);
			AssertEquals("Information|Pick No: P00000001 did not find existing stock that could be allocated.", Logger.ToString().Trim());
		}

		#endregion

		#region TestAllocateAwaitingReplenishmentPicks_CartonisationSelected_PickHasChangesButStillAwaitingReplenishment

		public void TestAllocateAwaitingReplenishmentPicks_CartonisationSelected_PickHasChangesButStillAwaitingReplenishment()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var ruleSet = AllocationRulesHelper.MakeNewRuleSetAndDeactivateSystemAllocationRuleSet(Factory);
			AllocationRulesHelper.AddPickFaceRules(ruleSet, 10);
			AllocationRulesHelper.AddFifoRule(ruleSet, 20, preventPickingPickFacesFromBulk: true);

			data.Whs1.WW_IsPickByUOMEnabled = true;
			var refType = PackingHelper.CreateRefPackType("1", "1", 2m, 4m, 8m, Constants.Length.Metres, 3, Constants.Weight.Kilograms, UOMPackTypesList.Codes.Pallet);
			data.Part1.OP_StockKeepingUnit = refType.F3_Code;

			var pickFaceLocation = data.Whs1.FindLocation("A-1");
			var bulkLocation = data.Whs1.FindLocation("A-2");
			Helper.CreateProductPickFace(data.Part1, data.Org1, pickFaceLocation);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", ZDateTimeOffset.Today, data.Part1, 50m, bulkLocation, "");
			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 50m, bulkLocation, pickFaceLocation);
			transferLine.RunPreSaveValidation();
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 100m, WhsPickOption.Codes.Auto);
			var pick = Helper.CreatePickNew(order);
			pick.WP_PickPalletsByLabel = true;
			AssertEquals("Precondition", true, pick.WP_IsAwaitingReplenishment);
			AssertEquals("Precondition", false, pick.WP_IsCartonised);
			Factory.Save();

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", ZDateTimeOffset.Today, data.Part1, 50m, pickFaceLocation, "");
			Factory.Save();

			AllocateAwaitingReplenishmentPicksForTest();
			AssertEquals("Pick is still awaiting replenishment.", true, pick.WP_IsAwaitingReplenishment);
			AssertEquals(false, pick.WP_IsCartonised);
			AssertNotContains("You cannot Allocate Package Labels if the pick is awaiting replenishment", Logger.ToString().Trim());
		}

		#endregion

		#region TestAllocateAwaitingReplenishmentPicks_CartonisationSelected_ErrorOnAllocatePackageLabels

		public void TestAllocateAwaitingReplenishmentPicks_CartonisationSelected_ErrorOnAllocatePackageLabels()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var ruleSet = AllocationRulesHelper.MakeNewRuleSetAndDeactivateSystemAllocationRuleSet(Factory);
			AllocationRulesHelper.AddPickFaceRules(ruleSet, 10);
			AllocationRulesHelper.AddFifoRule(ruleSet, 20, preventPickingPickFacesFromBulk: true);

			data.Whs1.WW_IsPickByUOMEnabled = true;
			var whsGroup = Helper.CreateWhsCartonGroup("1", "WhsOrg");
			var whsSize1 = Helper.CreateWhsCartonSize("WH", 1, 2, 4, 8, 16, 32, new ZByte(80), Constants.Length.Metres, Constants.Weight.Kilograms);
			whsGroup.CartonSizes.Add(whsSize1);
			data.Whs1.WarehouseAddress.Header.MiscServ.OM_WCG_CartonGroup = whsGroup.PK;

			var pickFaceLocation = data.Whs1.FindLocation("A-1");
			var bulkLocation = data.Whs1.FindLocation("A-2");
			Helper.CreateProductPickFace(data.Part1, data.Org1, pickFaceLocation);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", ZDateTimeOffset.Today, data.Part1, 40m, bulkLocation, "");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", ZDateTimeOffset.Today, data.Part1, 15m, pickFaceLocation, "");
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 16m, WhsPickOption.Codes.Manual);
			var pick = Helper.CreatePickNew(order);
			pick.AutoAllocateItems();
			AssertEquals("Precondition", true, pick.WP_IsAwaitingReplenishment);
			AssertEquals("Precondition", false, pick.WP_IsCartonised);
			pick.WP_CartoniseSplitCases = true;
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "T1", Notify);
			Helper.CreateWhsTransferLine(transfer, data.Part1, 16m, bulkLocation, pickFaceLocation);
			transfer.FinaliseDocket();
			AssertEquals(true, transfer.IsFinalised);
			Factory.Save();

			AllocateAwaitingReplenishmentPicksForTest();
			AssertEquals("Pick is not waiting replenishment anymore.", PickStatus.Codes.Created, pick.WP_PickStatus);
			AssertEquals("Should not show any messages.", true, UnitTestUserNotification.Instance.LastMessage.WasNone);
			AssertEquals(
@"Information|Pick No: P00000001 has successfully allocated stock.
Error|Pick P00000001: No Order(s) on the Pick are valid for Allocating Package Labels. Check the Events on the Pick for more details.",
Logger.ToString().Trim());
		}

		#endregion

		#region TestAllocateAwaitingReplenishmentPicks_InvalidOrder_RequiredDate

		public void TestAllocateAwaitingReplenishmentPicks_InvalidOrder_RequiredDate()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var ruleSet = AllocationRulesHelper.MakeNewRuleSetAndDeactivateSystemAllocationRuleSet(Factory);
			AllocationRulesHelper.AddPickFaceRules(ruleSet, 10);
			AllocationRulesHelper.AddFifoRule(ruleSet, 20, preventPickingPickFacesFromBulk: true);

			data.Whs1.WW_IsPickByUOMEnabled = true;
			var refType = PackingHelper.CreateRefPackType("1", "1", 2m, 4m, 8m, Constants.Length.Metres, 3, Constants.Weight.Kilograms, UOMPackTypesList.Codes.Pallet);
			data.Part1.OP_StockKeepingUnit = refType.F3_Code;

			var pickFaceLocation = data.Whs1.FindLocation("A-1");
			var bulkLocation = data.Whs1.FindLocation("A-2");
			Helper.CreateProductPickFace(data.Part1, data.Org1, pickFaceLocation);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", ZDateTimeOffset.Today, data.Part1, 40m, bulkLocation, "");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", ZDateTimeOffset.Today, data.Part1, 15m, pickFaceLocation, "");
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 16m, WhsPickOption.Codes.Manual);
			var pick = Helper.CreatePickNew(order);
			pick.AutoAllocateItems();
			AssertEquals("Precondition", true, pick.WP_IsAwaitingReplenishment);
			AssertEquals("Precondition", false, pick.WP_IsCartonised);
			pick.WP_PickPalletsByLabel = true;
			order.WD_RequiredDate = ZDateTimeOffset.Empty; // make it invalid
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "T1", Notify);
			Helper.CreateWhsTransferLine(transfer, data.Part1, 16m, bulkLocation, pickFaceLocation);
			transfer.FinaliseDocket();
			AssertEquals(true, transfer.IsFinalised);
			Factory.Save();

			AllocateAwaitingReplenishmentPicksForTest();

			AssertEquals(false, pick.WP_IsCartonised);
			AssertMultilineASCIIEquals("There should be an error on the logs",
@"Information|Issue occurred during allocation of Pick No: P00000001: Error occurred while loading data: Order No. O1 cannot be allocated, Required Date is not valid.
Pick No: P00000001 did not find existing stock that could be allocated.",
			Logger.ToString());
		}

		#endregion

		#region TestAllocateAwaitingReplenishmentPicks_NoAllocationRules

		public void TestAllocateAwaitingReplenishmentPicks_NoAllocationRules()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var ruleSet = AllocationRulesHelper.MakeNewRuleSetAndDeactivateSystemAllocationRuleSet(Factory);
			AllocationRulesHelper.AddPickFaceRules(ruleSet, 10);
			AllocationRulesHelper.AddFifoRule(ruleSet, 20, preventPickingPickFacesFromBulk: true);

			data.Whs1.WW_IsPickByUOMEnabled = true;
			var refType = PackingHelper.CreateRefPackType("1", "1", 2m, 4m, 8m, Constants.Length.Metres, 3, Constants.Weight.Kilograms, UOMPackTypesList.Codes.Pallet);
			data.Part1.OP_StockKeepingUnit = refType.F3_Code;

			var pickFaceLocation = data.Whs1.FindLocation("A-1");
			var bulkLocation = data.Whs1.FindLocation("A-2");
			Helper.CreateProductPickFace(data.Part1, data.Org1, pickFaceLocation);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", ZDateTimeOffset.Today, data.Part1, 40m, bulkLocation, "");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", ZDateTimeOffset.Today, data.Part1, 15m, pickFaceLocation, "");
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 16m, WhsPickOption.Codes.Manual);
			var pick = Helper.CreatePickNew(order);
			pick.AutoAllocateItems();
			AssertEquals("Precondition", true, pick.WP_IsAwaitingReplenishment);
			AssertEquals("Precondition", false, pick.WP_IsCartonised);
			pick.WP_PickPalletsByLabel = true;
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "T1", Notify);
			Helper.CreateWhsTransferLine(transfer, data.Part1, 16m, bulkLocation, pickFaceLocation);
			transfer.FinaliseDocket();
			AssertEquals(true, transfer.IsFinalised);
			Factory.Save();

			var allocationRuleSetQuery = new ZQuery(ProductionRuleSetSchema.PRS_Context, "PWA");
			allocationRuleSetQuery.AddToFilter(ProductionRuleSetSchema.PRS_IsLive, true);

			var allocationRuleSet = Factory.LoadTop1<ProductionRuleSet>(allocationRuleSetQuery);
			allocationRuleSet.PRS_IsLive = false;
			Factory.Save();

			AllocateAwaitingReplenishmentPicksForTest();

			AssertEquals(false, pick.WP_IsCartonised);
			AssertMultilineASCIIEquals("There should be an error on the logs",
@"Information|Issue occurred during allocation of Pick No: P00000001: No rules found for context: 'PWA'
Pick No: P00000001 did not find existing stock that could be allocated.",
			Logger.ToString());
		}

		#endregion

		#region TestAllocateAwaitingReplenishmentPicks_DeallocateLowerPriorityPicks

		public void TestAllocateAwaitingReplenishmentPicks_DeallocateLowerPriorityPicks()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var ruleSet = AllocationRulesHelper.MakeNewRuleSetAndDeactivateSystemAllocationRuleSet(Factory);
			AllocationRulesHelper.AddPickFaceRules(ruleSet, 10);
			AllocationRulesHelper.AddFifoRule(ruleSet, 20, preventPickingPickFacesFromBulk: true);

			Helper.CreateProductUnit(data.Part1, Constants.PkgUnit.Pallet, 3m); // Part1 : 1PLT = 3 UNT

			var pickFaceLocation = data.Whs1.FindLocation("A-1");
			var bulkLocation = data.Whs1.FindLocation("A-2");
			Helper.CreateProductPickFace(data.Part1, data.Org1, pickFaceLocation);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", ZDateTimeOffset.Today, data.Part1, 40m, bulkLocation, "");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", ZDateTimeOffset.Today, data.Part1, 15m, pickFaceLocation, "");
			Factory.Save();

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 16m, WhsPickOption.Codes.Manual);
			order1.WD_PickPriority = 0;
			var pick1 = Helper.CreatePickNew(order1);
			pick1.AutoAllocateItems();
			AssertEquals(true, pick1.WP_IsAwaitingReplenishment);
			Factory.Save();

			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O2", data.Part1, 16m, WhsPickOption.Codes.Manual);
			order2.WD_PickPriority = 5;
			var pick2 = Helper.CreatePickNew(order2);
			pick2.AutoAllocateItems();
			AssertEquals(true, pick2.WP_IsAwaitingReplenishment);
			Factory.Save();

			var order3 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O3", data.Part1, 16m, WhsPickOption.Codes.Manual);
			var pick3 = Helper.CreatePickNew(order3);
			order3.WD_PickPriority = 3;
			pick3.AutoAllocateItems();
			AssertEquals(true, pick3.WP_IsAwaitingReplenishment);
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "T1", Notify);
			Helper.CreateWhsTransferLine(transfer, data.Part1, 16m, bulkLocation, pickFaceLocation);
			transfer.FinaliseDocket();
			AssertEquals(true, transfer.IsFinalised);
			Factory.Save();

			using (WarehouseDataRegistry.Instance.DeallocateLowerPriorityPicksWhenAllocatingWaitingReplenishmentPicks.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				AllocateAwaitingReplenishmentPicksForTest();
				AssertEquals("No errors should be shown.", @"Information|Allocated stock has been successfully deallocated from lower priority picks awaiting replenishment.
Pick No: P00000003 has successfully allocated stock.
Pick No: P00000002 has successfully allocated stock.
Pick No: P00000001 did not find existing stock that could be allocated.".Trim(), Logger.ToString().Trim());

				var newFactoryToLoadPicks = new BusinessObjectFactory();
				var pick1InNewFactory = newFactoryToLoadPicks.Load<WhsPick>(pick1.PK);
				var pick2InNewFactory = newFactoryToLoadPicks.Load<WhsPick>(pick2.PK);
				var pick3InNewFactory = newFactoryToLoadPicks.Load<WhsPick>(pick3.PK);
				AssertEquals(true, pick1InNewFactory.WP_IsAwaitingReplenishment);
				AssertEquals(true, pick2InNewFactory.WP_IsAwaitingReplenishment);
				AssertEquals(false, pick3InNewFactory.WP_IsAwaitingReplenishment);
				AssertEquals(PickStatus.Codes.Created, pick3InNewFactory.WP_PickStatus);

				var avaialbleInventoriesForPick1 = pick1InNewFactory.OrderedInventories.Cast<WhsPickOrderedInventory>().Single().AvailableInventories.Cast<WhsPickAvailableInventory>();
				var availableInventoryLineForPick1InPickface = avaialbleInventoriesForPick1.Single(a => a.LocationPK == pickFaceLocation.PK);
				AssertEquals(false, availableInventoryLineForPick1InPickface.Allocate);
				AssertEquals(0m, availableInventoryLineForPick1InPickface.PickLineQuantity);
				AssertEquals(0m, availableInventoryLineForPick1InPickface.QuantityAvailableToPick);
				AssertEquals(31m, availableInventoryLineForPick1InPickface.QuantityCommitted);

				var avaialbleInventoriesForPick2 = pick2InNewFactory.OrderedInventories.Cast<WhsPickOrderedInventory>().Single().AvailableInventories.Cast<WhsPickAvailableInventory>();
				var availableInventoryLineForPick2InPickface = avaialbleInventoriesForPick2.Single(a => a.LocationPK == pickFaceLocation.PK);
				AssertEquals(true, availableInventoryLineForPick2InPickface.Allocate);
				AssertEquals(15m, availableInventoryLineForPick2InPickface.PickLineQuantity);
				AssertEquals(15m, availableInventoryLineForPick2InPickface.QuantityAvailableToPick);
				AssertEquals(31m, availableInventoryLineForPick2InPickface.QuantityCommitted);

				var avaialbleInventoriesForPick3 = pick3InNewFactory.OrderedInventories.Cast<WhsPickOrderedInventory>().Single().AvailableInventories.Cast<WhsPickAvailableInventory>();
				var availableInventoryLineForPick3InPickface = avaialbleInventoriesForPick3.Single(a => a.LocationPK == pickFaceLocation.PK);
				AssertEquals(true, availableInventoryLineForPick3InPickface.Allocate);
				AssertEquals(16m, availableInventoryLineForPick3InPickface.PickLineQuantity);
				AssertEquals(16m, availableInventoryLineForPick3InPickface.QuantityAvailableToPick);
				AssertEquals(31m, availableInventoryLineForPick3InPickface.QuantityCommitted);
			}
		}

		public void TestAllocateAwaitingReplenishmentPicks_DeallocateLowerPriorityPicks_SinglePick()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var ruleSet = AllocationRulesHelper.MakeNewRuleSetAndDeactivateSystemAllocationRuleSet(Factory);
			AllocationRulesHelper.AddPickFaceRules(ruleSet, 10);
			AllocationRulesHelper.AddFifoRule(ruleSet, 20, preventPickingPickFacesFromBulk: true);

			Helper.CreateProductUnit(data.Part1, Constants.PkgUnit.Pallet, 3m); // Part1 : 1PLT = 3 UNT

			var pickFaceLocation = data.Whs1.FindLocation("A-1");
			var bulkLocation = data.Whs1.FindLocation("A-2");
			Helper.CreateProductPickFace(data.Part1, data.Org1, pickFaceLocation);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", ZDateTimeOffset.Today, data.Part1, 40m, bulkLocation, "");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", ZDateTimeOffset.Today, data.Part1, 15m, pickFaceLocation, "");
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 16m, WhsPickOption.Codes.Manual);
			order.WD_PickPriority = 0;
			var pick = Helper.CreatePickNew(order);
			pick.AutoAllocateItems();
			AssertEquals(true, pick.WP_IsAwaitingReplenishment);
			Factory.Save();

			using (WarehouseDataRegistry.Instance.DeallocateLowerPriorityPicksWhenAllocatingWaitingReplenishmentPicks.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				AllocateAwaitingReplenishmentPicksForTest();
				AssertEquals("No errors should be shown.", @"Information|Pick No: P00000001 did not find existing stock that could be allocated.".Trim(), Logger.ToString().Trim());

				var newFactoryToLoadPicks = new BusinessObjectFactory();
				var pickInNewFactory = newFactoryToLoadPicks.Load<WhsPick>(pick.PK);
				AssertEquals(true, pickInNewFactory.WP_IsAwaitingReplenishment);

				var avaialbleInventoriesForPick = pickInNewFactory.OrderedInventories.Cast<WhsPickOrderedInventory>().Single().AvailableInventories.Cast<WhsPickAvailableInventory>();
				var availableInventoryLineForPickInPickface = avaialbleInventoriesForPick.Single(a => a.LocationPK == pickFaceLocation.PK);
				AssertEquals(true, availableInventoryLineForPickInPickface.Allocate);
				AssertEquals("Allocated stock is still allocated to pick.", 15m, availableInventoryLineForPickInPickface.PickLineQuantity);
			}
		}

		public void TestAllocateAwaitingReplenishmentPicks_DeallocateLowerPriorityPicks_FailsOnDeallocateSave_ContinuesToAllocate()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var ruleSet = AllocationRulesHelper.MakeNewRuleSetAndDeactivateSystemAllocationRuleSet(Factory);
			AllocationRulesHelper.AddPickFaceRules(ruleSet, 10);
			AllocationRulesHelper.AddFifoRule(ruleSet, 20, preventPickingPickFacesFromBulk: true);

			Helper.CreateProductUnit(data.Part1, Constants.PkgUnit.Pallet, 3m); // Part1 : 1PLT = 3 UNT

			var pickFaceLocation = data.Whs1.FindLocation("A-1");
			var bulkLocation = data.Whs1.FindLocation("A-2");
			Helper.CreateProductPickFace(data.Part1, data.Org1, pickFaceLocation);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", ZDateTimeOffset.Today, data.Part1, 40m, bulkLocation, "");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", ZDateTimeOffset.Today, data.Part1, 15m, pickFaceLocation, "");
			Factory.Save();

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 16m, WhsPickOption.Codes.Manual);
			order1.WD_PickPriority = 0;
			var pick1 = Helper.CreatePickNew(order1);
			pick1.AutoAllocateItems();
			AssertEquals(true, pick1.WP_IsAwaitingReplenishment);
			Factory.Save();

			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O2", data.Part1, 16m, WhsPickOption.Codes.Manual);
			order2.WD_PickPriority = 5;
			var pick2 = Helper.CreatePickNew(order2);
			pick2.AutoAllocateItems();
			AssertEquals(true, pick2.WP_IsAwaitingReplenishment);
			Factory.Save();

			var order3 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O3", data.Part1, 16m, WhsPickOption.Codes.Manual);
			var pick3 = Helper.CreatePickNew(order3);
			order3.WD_PickPriority = 3;
			pick3.AutoAllocateItems();
			AssertEquals(true, pick3.WP_IsAwaitingReplenishment);
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "T1", Notify);
			Helper.CreateWhsTransferLine(transfer, data.Part1, 16m, bulkLocation, pickFaceLocation);
			transfer.FinaliseDocket();
			AssertEquals(true, transfer.IsFinalised);
			Factory.Save();

			using (WarehouseDataRegistry.Instance.DeallocateLowerPriorityPicksWhenAllocatingWaitingReplenishmentPicks.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var manager = new AllocatePicksAwaitingReplenishmentWithExistingInventoryProcessingManager(ObjectFactory.Get<IPartiallyReplenishedPickSplitter>());
				var throwException = true;
				manager.OnNewFactorySaving += (f) =>
				{
					BusinessObjectFactory.SavingEventHandler throwError = (factory) => throw new ZSaveConcurrencyException(new ZDataConcurrencyException(new Exception(), ((IBusinessObjectInternals)pick1).Row, Db.Connection), f); // throw ZSaveConcurencyException manually
					if (throwException)
					{
						f.Saving += throwError;
					}
					else
					{
						f.Saving -= throwError;
					}

					throwException = false;
				};
				manager.AllocateAwaitingReplenishmentPicks(Logger, new CancellationToken());
				AssertEquals("Deallocation error is logged but picks are still allocated.", @"Information|Pick No: P00000003 has successfully allocated stock.
Pick No: P00000002 did not find existing stock that could be allocated.
Pick No: P00000001 did not find existing stock that could be allocated.
Error|Stock could not be deallocated from lower priority picks before attempting to allocate picks. Someone else has changed the pick.".Trim(), Logger.ToString().Trim());

				var newFactoryToLoadPicks = new BusinessObjectFactory();
				var pick1InNewFactory = newFactoryToLoadPicks.Load<WhsPick>(pick1.PK);
				var pick2InNewFactory = newFactoryToLoadPicks.Load<WhsPick>(pick2.PK);
				var pick3InNewFactory = newFactoryToLoadPicks.Load<WhsPick>(pick3.PK);
				AssertEquals(true, pick1InNewFactory.WP_IsAwaitingReplenishment);
				AssertEquals(true, pick2InNewFactory.WP_IsAwaitingReplenishment);
				AssertEquals(false, pick3InNewFactory.WP_IsAwaitingReplenishment);
				AssertEquals(PickStatus.Codes.Created, pick3InNewFactory.WP_PickStatus);

				var avaialbleInventoriesForPick1 = pick1InNewFactory.OrderedInventories.Cast<WhsPickOrderedInventory>().Single().AvailableInventories.Cast<WhsPickAvailableInventory>();
				var availableInventoryLineForPick1InPickface = avaialbleInventoriesForPick1.Single(a => a.LocationPK == pickFaceLocation.PK);
				AssertEquals(true, availableInventoryLineForPick1InPickface.Allocate);
				AssertEquals(15m, availableInventoryLineForPick1InPickface.PickLineQuantity);
				AssertEquals(15m, availableInventoryLineForPick1InPickface.QuantityAvailableToPick);
				AssertEquals(31m, availableInventoryLineForPick1InPickface.QuantityCommitted);

				var avaialbleInventoriesForPick2 = pick2InNewFactory.OrderedInventories.Cast<WhsPickOrderedInventory>().Single().AvailableInventories.Cast<WhsPickAvailableInventory>();
				var availableInventoryLineForPick2InPickface = avaialbleInventoriesForPick2.Single(a => a.LocationPK == pickFaceLocation.PK);
				AssertEquals(false, availableInventoryLineForPick2InPickface.Allocate);
				AssertEquals(0m, availableInventoryLineForPick2InPickface.PickLineQuantity);
				AssertEquals(0m, availableInventoryLineForPick2InPickface.QuantityAvailableToPick);
				AssertEquals(31m, availableInventoryLineForPick2InPickface.QuantityCommitted);

				var avaialbleInventoriesForPick3 = pick3InNewFactory.OrderedInventories.Cast<WhsPickOrderedInventory>().Single().AvailableInventories.Cast<WhsPickAvailableInventory>();
				var availableInventoryLineForPick3InPickface = avaialbleInventoriesForPick3.Single(a => a.LocationPK == pickFaceLocation.PK);
				AssertEquals(true, availableInventoryLineForPick3InPickface.Allocate);
				AssertEquals(16m, availableInventoryLineForPick3InPickface.PickLineQuantity);
				AssertEquals(16m, availableInventoryLineForPick3InPickface.QuantityAvailableToPick);
				AssertEquals(31m, availableInventoryLineForPick3InPickface.QuantityCommitted);
			}
		}

		#endregion

		#region TestAllocateAwaitingReplenishmentPicks_SetsWarehouseBranchUserContext

		public void TestAllocateAwaitingReplenishmentPicks_SetsWarehouseBranchUserContext()
		{
			var warehouse1 = Helper.CreateWarehouse("WH1", "A", 2, 1);
			var warehouse2 = Helper.CreateWarehouse("WH2", "B", 2, 1);
			Factory.Save();

			var ruleSet = AllocationRulesHelper.MakeNewRuleSetAndDeactivateSystemAllocationRuleSet(Factory);
			AllocationRulesHelper.AddPickFaceRules(ruleSet, 10);
			AllocationRulesHelper.AddFifoRule(ruleSet, 20, preventPickingPickFacesFromBulk: true);

			var client = Helper.CreateClient("CL1");
			var product = Helper.CreateProduct("PROD1", client);
			Helper.CreateProductUnit(product, Constants.PkgUnit.Pallet, 3m); // Part1 : 1PLT = 3 UNT

			var pickFaceLocationWhs1 = warehouse1.FindLocation("A-1");
			var bulkLocationWhs1 = warehouse1.FindLocation("A-2");
			Helper.CreateProductPickFace(product, client, pickFaceLocationWhs1);
			Helper.CreateWhsReceiveWithInventory(client, warehouse1, "R1", ZDateTimeOffset.Today, product, 40m, bulkLocationWhs1, "");
			Helper.CreateWhsReceiveWithInventory(client, warehouse1, "R2", ZDateTimeOffset.Today, product, 15m, pickFaceLocationWhs1, "");

			var pickFaceLocationWhs2 = warehouse2.FindLocation("B-1");
			var bulkLocationWhs2 = warehouse2.FindLocation("B-2");
			Helper.CreateProductPickFace(product, client, pickFaceLocationWhs2);
			Helper.CreateWhsReceiveWithInventory(client, warehouse2, "R3", ZDateTimeOffset.Today, product, 40m, bulkLocationWhs2, "");
			Helper.CreateWhsReceiveWithInventory(client, warehouse2, "R4", ZDateTimeOffset.Today, product, 15m, pickFaceLocationWhs2, "");

			CreateAwaitingReplenishmentPick(warehouse1, "O1", 0);
			CreateAwaitingReplenishmentPick(warehouse2, "O2", 3);
			CreateAwaitingReplenishmentPick(warehouse1, "O3", 5);
			Factory.Save();

			CreateFinalisedTransfer(warehouse1, "T1", bulkLocationWhs1, pickFaceLocationWhs1);
			CreateFinalisedTransfer(warehouse2, "T2", bulkLocationWhs2, pickFaceLocationWhs2);
			CreateFinalisedTransfer(warehouse1, "T3", bulkLocationWhs1, pickFaceLocationWhs1);
			Factory.Save();

			var contextChangeCount = 0;
			var branchContexts = new HashSet<ZGuid>();
			var testBranchCode = Env.CurrentBranch.Code;

			try
			{
				Env.Instance.UserContextChanged += OnContextChanged;
				AllocateAwaitingReplenishmentPicksForTest();

				var logs = Logger.ToString();
				Assert("Allocation is successful.", logs.Contains("Pick No: P00000001 has successfully allocated stock."));
				Assert("Allocation is successful.", logs.Contains("Pick No: P00000002 has successfully allocated stock."));
				Assert("Allocation is successful.", logs.Contains("Pick No: P00000003 has successfully allocated stock."));
				Assert("There are no errors.", !logs.Contains("Error"));

				AssertEquals("Changed context twice.", 2, contextChangeCount);
				AssertContainsExactElementsInAnyOrder("Changed to 2 warehouse branch contexts.", new[] { warehouse1.WW_GB_RelatedCompanyBranch, warehouse2.WW_GB_RelatedCompanyBranch }, branchContexts);
			}
			finally
			{
				Env.Instance.UserContextChanged -= OnContextChanged;
			}

			void OnContextChanged(object sender, IUserContextChangingEventArgs e)
			{
				if (e.NewUserContext.Branch.Code != testBranchCode) // do not count if it's just reverting to the previous context on temp context dispose
				{
					contextChangeCount++;
					branchContexts.Add(e.NewUserContext.Branch.PK);
				}
			}

			void CreateAwaitingReplenishmentPick(WhsWarehouse warehouse, string orderReference, ZByte pickPriority)
			{
				var order = Helper.CreateWhsOrderWithOrderLine(client, warehouse, orderReference, product, 16m, WhsPickOption.Codes.Manual);
				order.WD_PickPriority = pickPriority;
				var pick = Helper.CreatePickNew(order);
				pick.AutoAllocateItems();
				AssertEquals(true, pick.WP_IsAwaitingReplenishment);
			}

			void CreateFinalisedTransfer(WhsWarehouse warehouse, string reference, WhsLocation bulkLocation, WhsLocation pickFaceLocation)
			{
				var transfer = Helper.CreateWhsTransfer(client, warehouse, "T1", Notify);
				Helper.CreateWhsTransferLine(transfer, product, 16m, bulkLocation, pickFaceLocation);
				transfer.FinaliseDocket();
				AssertEquals(true, transfer.IsFinalised);
			}
		}

		#endregion

		#region TestAllocateAwaitingReplenishmentPicks_PicksStillAwaitingReplenishmentAreSplit

		public void TestAllocateAwaitingReplenishmentPicks_PicksStillAwaitingReplenishmentAreSplit()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var pickParams = WhsClientPickingParams.GetClientPickingParams(data.Org1).WarehousePickPackParams.AddNew();
			pickParams.WPP_WW_Warehouse = data.Whs1.PK;
			pickParams.WPP_SplitOrdersFromPartiallyReplenishedPicks = true;

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
			pick.AddOrders(new[] { order1, order2 });

			pick.WP_IsAwaitingReplenishment = true;
			Factory.Save();
			AssertEquals("Precondition: pick is waiting replenishment.", true, pick.WP_IsAwaitingReplenishment);

			var newFactory = new BusinessObjectFactory();

			var splitter = new Mock<IPartiallyReplenishedPickSplitter>();
			splitter
				.Setup(m => m.SplitOrdersFromPartiallyReplenishedPick(It.IsAny<WhsPick>()))
				.Returns((WhsPick ps) =>
				{
					var initialPick = ps;
					var factory = initialPick.Factory;
					var pickFromSplit = factory.New<WhsPick>();
					pickFromSplit.WP_WW_Whs = initialPick.WP_WW_Whs;

					var orderSplit = factory.Load<WhsOrder>(order1.PK);
					orderSplit.WD_WP = pickFromSplit.PK;
					return pickFromSplit;
				});

			using (ObjectFactory.Substitute(splitter.Object))
			{
				AllocateAwaitingReplenishmentPicksForTest();
			}

			var picks = newFactory.Load<WhsPick>(new ZQuery());
			AssertEquals("Expect new pick created", 2, picks.Length);

			var newPick = picks.SingleOrDefault(p => p.PK != pick.PK);
			AssertEquals("New pick is NOT waiting replenishment.", false, newPick.WP_IsAwaitingReplenishment);
			AssertEquals("New Pick should have 1 order", 1, newPick.Orders.Count);
			AssertEquals("New Pick should have replenished order", order1.PK, newPick.Orders[0].PK);
			AssertEquals("New pick WP_PercentageComplete updated.", (ZByte)0, newPick.WP_PercentageComplete);

			var oldPick = picks.SingleOrDefault(p => p.PK == pick.PK);
			AssertEquals("Original pick is still waiting replenishment.", true, oldPick.WP_IsAwaitingReplenishment);
			AssertEquals("Original Pick should have 1 order", 1, oldPick.Orders.Count);
			AssertEquals("Original Pick should have not replenished order", order2.PK, oldPick.Orders[0].PK);
			AssertEquals("Original pick WP_PercentageComplete updated.", (ZByte)0, oldPick.WP_PercentageComplete);

			Assert("Log should be correct", Logger.ToString().Contains("Pick No: P00000001 has successfully been split with replenished orders being added to Pick No: P00000002."));
		}

		public void TestAllocateAwaitingReplenishmentPicks_PicksStillAwaitingReplenishmentAreSplit_EndToEnd()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var pickParams = WhsClientPickingParams.GetClientPickingParams(data.Org1).WarehousePickPackParams.AddNew();
			pickParams.WPP_WW_Warehouse = data.Whs1.PK;
			pickParams.WPP_SplitOrdersFromPartiallyReplenishedPicks = true;

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
			pick.AddOrders(new[] { order1, order2 });

			pick.WP_IsAwaitingReplenishment = true;
			Factory.Save();
			AssertEquals("Precondition: pick is waiting replenishment.", true, pick.WP_IsAwaitingReplenishment);

			AllocateAwaitingReplenishmentPicksForTest();

			var picks = new BusinessObjectFactory().Load<WhsPick>(new ZQuery());
			AssertEquals("Expect new pick created", 2, picks.Length);

			var newPick = picks.SingleOrDefault(p => p.PK != pick.PK);
			AssertEquals("New pick is NOT waiting replenishment.", false, newPick.WP_IsAwaitingReplenishment);
			AssertEquals("New Pick should have 1 order", 1, newPick.Orders.Count);
			AssertEquals("New Pick should have replenished order", order1.PK, newPick.Orders[0].PK);
			AssertEquals("New pick WP_PercentageComplete updated.", (ZByte)0, newPick.WP_PercentageComplete);

			var oldPick = picks.SingleOrDefault(p => p.PK == pick.PK);
			AssertEquals("Original pick is still waiting replenishment.", true, oldPick.WP_IsAwaitingReplenishment);
			AssertEquals("Original Pick should have 1 order", 1, oldPick.Orders.Count);
			AssertEquals("Original Pick should have not replenished order", order2.PK, oldPick.Orders[0].PK);
			AssertEquals("Original pick WP_PercentageComplete updated.", (ZByte)0, oldPick.WP_PercentageComplete);

			Assert("Log should be correct", Logger.ToString().Contains("Pick No: P00000001 has successfully been split with replenished orders being added to Pick No: P00000002."));
		}

		#endregion

		#region TestAllocateAwaitingReplenishmentPicks_PicksStillAwaitingReplenishmentAreSplit_EndToEnd_Cartonizes

		public void TestAllocateAwaitingReplenishmentPicks_PicksStillAwaitingReplenishmentAreSplit_EndToEnd_Cartonizes()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			data.Whs1.WW_IsPickByUOMEnabled = true;

			var pickParams1 = WhsClientPickingParams.GetClientPickingParams(data.Org1).WarehousePickPackParams.AddNew();
			pickParams1.WPP_WW_Warehouse = data.Whs1.PK;
			pickParams1.WPP_SplitOrdersFromPartiallyReplenishedPicks = true;

			var whsGroup = Helper.CreateWhsCartonGroup("1", "WhsOrg");
			var whsSize = Helper.CreateWhsCartonSize("WH", 10, 20, 40, 80, 160, 320, new ZByte(100), Constants.Length.Metres, Constants.Weight.Kilograms);
			whsGroup.CartonSizes.Add(whsSize);
			data.Whs1.WarehouseAddress.Header.MiscServ.OM_WCG_CartonGroup = whsGroup.PK;

			var packType = data.Part1.Lookups.PackTypes.Single(p => p.F3_Code == data.Part1.OP_StockKeepingUnit);
			packType.F3_UOMType = UOMPackTypesList.Codes.SplitCase;

			var pickFaceLocation1 = data.Whs1.FindLocation("A-1");
			var bulkLocation1 = data.Whs1.FindLocation("A-2");

			Helper.CreateProductPickFace(data.Part1, data.Org1, pickFaceLocation1);
			Helper.CreateProductPickFace(data.Part2, data.Org1, pickFaceLocation1);

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m, pickFaceLocation1, "");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part2, 10m, bulkLocation1, "");
			Factory.Save();

			var transfer1 = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "T1");
			var transferLine1 = Helper.CreateWhsTransferLine(transfer1, data.Part2, 10m, bulkLocation1, pickFaceLocation1);
			transferLine1.RunPreSaveValidation();
			Factory.Save();

			var order1 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			Helper.CreateWhsOrderLine(order1, data.Part1, 10m);

			var order2 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O2");
			Helper.CreateWhsOrderLine(order2, data.Part2, 10m);

			var pick = Factory.New<WhsPick>();
			pick.AddOrders(new[] { order1, order2 });

			pick.WP_CartoniseSplitCases = true;
			pick.WP_IsAwaitingReplenishment = true;
			Factory.Save();
			AssertEquals("Precondition: pick is waiting replenishment.", true, pick.WP_IsAwaitingReplenishment);

			AllocateAwaitingReplenishmentPicksForTest();

			var picks = new BusinessObjectFactory().Load<WhsPick>(new ZQuery());
			AssertEquals("Expect new pick created", 2, picks.Length);

			var newPick = picks.SingleOrDefault(p => p.PK != pick.PK);
			AssertEquals("New pick is NOT waiting replenishment.", false, newPick.WP_IsAwaitingReplenishment);
			AssertEquals("New pick is cartonized.", true, newPick.WP_IsCartonised);
			AssertEquals("New Pick should have 1 order", 1, newPick.Orders.Count);

			var oldPick = picks.SingleOrDefault(p => p.PK == pick.PK);
			AssertEquals("Original pick is still waiting replenishment.", true, oldPick.WP_IsAwaitingReplenishment);
			AssertEquals("Original pick is not cartonized.", false, oldPick.WP_IsCartonised);
			AssertEquals("Original Pick should have 1 order", 1, oldPick.Orders.Count);
		}

		#endregion

		#region TestAllocateAwaitingReplenishmentPicks_PicksStillAwaitingReplenishmentAreSplit_EndToEnd_DbHits

		[StressTest]
		public void TestAllocateAwaitingReplenishmentPicks_PicksStillAwaitingReplenishmentAreSplit_EndToEnd_DbHits() => TestAllocateAwaitingReplenishmentPicks_PicksStillAwaitingReplenishmentAreSplit_EndToEnd_DbHits_Core();

		[StressTest]
		public void TestAllocateAwaitingReplenishmentPicks_PicksStillAwaitingReplenishmentAreSplit_EndToEnd_DbHits_HeldGoodsForOrdersDisabled() => TestAllocateAwaitingReplenishmentPicks_PicksStillAwaitingReplenishmentAreSplit_EndToEnd_DbHits_Core(enableHeldGoodsForOrders: false);

		void TestAllocateAwaitingReplenishmentPicks_PicksStillAwaitingReplenishmentAreSplit_EndToEnd_DbHits_Core(bool enableHeldGoodsForOrders = true)
		{
			using (WarehouseDataRegistry.Instance.EnableHeldGoodsForOrders.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, enableHeldGoodsForOrders))
			{
				var data = new TestDataSimpleEnvironment(Factory, 100, 1);
				var pickParams1 = WhsClientPickingParams.GetClientPickingParams(data.Org1).WarehousePickPackParams.AddNew();
				pickParams1.WPP_WW_Warehouse = data.Whs1.PK;
				pickParams1.WPP_SplitOrdersFromPartiallyReplenishedPicks = true;

				var client2 = Helper.CreateClient("CL2");
				Helper.CreateProductClientRelationShip(client2, data.Part1);
				Helper.CreateProductClientRelationShip(client2, data.Part2);
				var pickParams2 = WhsClientPickingParams.GetClientPickingParams(client2).WarehousePickPackParams.AddNew();
				pickParams2.WPP_WW_Warehouse = data.Whs1.PK;
				pickParams2.WPP_SplitOrdersFromPartiallyReplenishedPicks = false;

				var loops = 5;
				var locations = new List<(WhsLocation, WhsLocation, WhsLocation, WhsLocation)>(loops);
				for (var i = 0; i < loops; i++)
				{
					var pickFaceLocation1 = data.Whs1.FindLocation($"A-{i * 4 + 1}");
					var pickFaceLocation2 = data.Whs1.FindLocation($"A-{i * 4 + 2}");
					var bulkLocation1 = data.Whs1.FindLocation($"A-{i * 4 + 3}");
					var bulkLocation2 = data.Whs1.FindLocation($"A-{i * 4 + 4}");
					locations.Add((pickFaceLocation1, pickFaceLocation2, bulkLocation1, bulkLocation2));

					Helper.CreateProductPickFace(data.Part1, data.Org1, pickFaceLocation1);
					Helper.CreateProductPickFace(data.Part2, data.Org1, pickFaceLocation1);
					Helper.CreateProductPickFace(data.Part1, client2, pickFaceLocation2);
					Helper.CreateProductPickFace(data.Part2, client2, pickFaceLocation2);

					Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, $"R1{i}", data.Part1, 10m, bulkLocation1, "");
					Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, $"R2{i}", data.Part2, 10m, bulkLocation1, "");
					Helper.CreateWhsReceiveWithInventory(client2, data.Whs1, $"R3{i}", data.Part1, 10m, bulkLocation2, "");
					Helper.CreateWhsReceiveWithInventory(client2, data.Whs1, $"R4{i}", data.Part2, 10m, bulkLocation2, "");
				}
				Factory.Save();

				var transfer1 = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "T1");
				var transfer2 = Helper.CreateWhsTransfer(client2, data.Whs1, "T2");
				for (var i = 0; i < loops; i++)
				{
					var (pickFaceLocation1, pickFaceLocation2, bulkLocation1, bulkLocation2) = locations[i];
					var transferLine1 = Helper.CreateWhsTransferLine(transfer1, data.Part1, 10m, bulkLocation1, pickFaceLocation1);
					transferLine1.RunPreSaveValidation();
					transferLine1.FinaliseDocketLine();

					var transferLine2 = Helper.CreateWhsTransferLine(transfer1, data.Part2, 10m, bulkLocation1, pickFaceLocation1);
					transferLine2.RunPreSaveValidation();

					var transferLine3 = Helper.CreateWhsTransferLine(transfer2, data.Part1, 10m, bulkLocation2, pickFaceLocation2);
					transferLine3.RunPreSaveValidation();

					var transferLine4 = Helper.CreateWhsTransferLine(transfer2, data.Part2, 10m, bulkLocation2, pickFaceLocation2);
					transferLine4.RunPreSaveValidation();
					transferLine4.FinaliseDocketLine();
				}
				Factory.Save();
				AssertEquals("Precondition", false, transfer1.IsFinalised);
				AssertEquals("Precondition", false, transfer2.IsFinalised);

				var ordersForPicks = new List<(WhsOrder O1, WhsOrder O2, WhsOrder O3, WhsOrder O4)>();
				for (var i = 0; i < loops; i++)
				{
					var order1 = Helper.CreateWhsOrder(data.Org1, data.Whs1, $"O1{i}");
					Helper.CreateWhsOrderLine(order1, data.Part1, 10m);

					var order2 = Helper.CreateWhsOrder(data.Org1, data.Whs1, $"O2{i}");
					Helper.CreateWhsOrderLine(order2, data.Part2, 10m);

					var order3 = Helper.CreateWhsOrder(client2, data.Whs1, $"O3{i}");
					Helper.CreateWhsOrderLine(order3, data.Part1, 10m);

					var order4 = Helper.CreateWhsOrder(client2, data.Whs1, $"O4{i}");
					Helper.CreateWhsOrderLine(order4, data.Part2, 10m);
					ordersForPicks.Add((order1, order2, order3, order4));
				}
				Factory.Save();

				for (var i = 0; i < loops; i++)
				{
					var (o1, o2, o3, o4) = ordersForPicks[i];

					var pick = Factory.New<WhsPick>();
					pick.AddOrders(new[] { o1, o2, o3, o4 });

					pick.WP_IsAwaitingReplenishment = true;
					AssertEquals("Precondition: pick is waiting replenishment.", true, pick.WP_IsAwaitingReplenishment);
				}
				Factory.Save();

				var expectedDbHits = new Dictionary<string, int>()
				{
					{ AccAllowedBranchDepartmentComboSchema.Constants.TableName, 1 },
					{ CusRefTradeGroupViewSchema.Constants.TableName, 1 },
					{ OrgSupplierPartSchema.Constants.TableName, 5 },
					{ OrgPartRelationSchema.Constants.TableName, 5 },
					{ OrgAddressSchema.Constants.TableName, 10 },
					{ PkgPackageSchema.Constants.TableName, 5 },
					{ PkgPackageJobSchema.Constants.TableName, 5 },
					{ OrgAddressCapabilitySchema.Constants.TableName, 5 },
					{ OrgCompanyDataSchema.Constants.TableName, 5 },
					{ OrgContactSchema.Constants.TableName, 5 },
					{ OrgCusCodeSchema.Constants.TableName, 5 },
					{ OrgCustomLabelsSchema.Constants.TableName, 6 },
					{ ProcessCompanyLinkRuleSchema.Constants.TableName, 5 },
					{ ProcessTasksSchema.Constants.TableName, 10 },
					{ ProcessTaskTemplateSchema.Constants.TableName, 10 },
					{ StmEventSchema.Constants.TableName, 10 },
					{ WhsDocketContainerSchema.Constants.TableName, 5 },
					// 1 for loading all picks' orders to sort with pick priority then 4 for each allocating factory - 1 for loading pick's orders, 1 for loading avail invs, 1 for loading committed qty to avail invs, 1 for loading pick transfers for splitting
					{ WhsDocketSchema.Constants.TableName, 21 },
					{ WhsDocketLineSchema.Constants.TableName, 10 }, // 1 for loading ordered invs and 1 for loading committed qty to avail invs
					{ WhsWarehouseSchema.Constants.TableName, 1 },
					{ OrgMiscServSchema.Constants.TableName, 5 },
					{ OrgHeaderSchema.Constants.TableName, 6 },
					{ WhsClientPickPackParamsByWhsSchema.Constants.TableName, 1 },
					{ WhsInventoryViewSchema.Constants.TableName, 5 },
					{ WhsLocationViewSchema.Constants.TableName, 10 }, // One for loading avail invs and one for validating WP_WL_DockDoor
					{ WhsPickSchema.Constants.TableName, 11 },
					{ WhsPickLineSchema.Constants.TableName, 25 }, // One for loading picklines for committed qty, one for loading picklines for available qty then on pick validation factory cache is reset which reloads both again during validation and one more hit for re-synced existing picklines prior to pick validation
					{ WhsRowSchema.Constants.TableName, 1 },
					{ JobDocAddressSchema.Constants.TableName, 5 },
					{ OrgPartUnitSchema.Constants.TableName, 5 },
					{ ProductionRuleSchema.Constants.TableName, 5 },
				};

				using (AssertDbHitsForAllFactories(
					message: null,
					expectedHitCounts: expectedDbHits,
					ignoreUnspecified: true,
					useOnlyNewFactories: true,
					ignoreHitsFromTablesCachedInUberFactory: true))
				{
					AllocateAwaitingReplenishmentPicksForTest();
				}
				AssertEquals("Expect correct number of picks returned", loops * 2, new BusinessObjectFactory().Load<WhsPick>(new ZQuery()).Length);
			}
		}

		#endregion

		#region AllocateAwaitingReplenishmentPicksForTest

		void AllocateAwaitingReplenishmentPicksForTest()
			=> ObjectFactory
					.Get<IAllocatePicksAwaitingReplenishmentWithExistingInventoryProcessingManager>()
					.AllocateAwaitingReplenishmentPicks(Logger, new CancellationToken());

		#endregion

		#region Logger

		TestServiceLogger Logger => logger ?? (logger = new TestServiceLogger());
		TestServiceLogger logger;

		#endregion

		#region PackingHelper

		PackingTestHelper PackingHelper => packingHelper ?? (packingHelper = new PackingTestHelper(Factory));
		PackingTestHelper packingHelper;

		#endregion

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();

			// Allocation rules typically pops up a GUI, so all tests in this class would need GuiTest for their setup.
			// This class is intended to be run in a service task anyway, so make the tests run non-user interactive to avoid this.
			isUserInteractiveDisposable = Globals.SetIsUserInteractiveForTest(false);
		}

		protected override void TearDown()
		{
			base.TearDown();

			isUserInteractiveDisposable?.Dispose();
		}

		IDisposable isUserInteractiveDisposable;

		#endregion
	}

	#region AllocatePicksAwaitingReplenishmentWithExistingInventoryProcessingManagerTriggersTest class

	public class AllocatePicksAwaitingReplenishmentWithExistingInventoryProcessingManagerTriggersTest : TestCase
	{
		#region TestAllocateAwaitingReplenishmentPicks_OverPickingTrigger

		[UseSnapshotProtection]
		public void TestAllocateAwaitingReplenishmentPicks_OverPickingTrigger()
		{
			using (Globals.SetIsUserInteractiveForTest(false))
			{
				// Setup data and pick algorithm
				// SkipPicksWaitingOnPickFaceReplenishment
				var data = new TestDataSimpleEnvironment(Factory);
				var ruleSet = AllocationRulesHelper.MakeNewRuleSetAndDeactivateSystemAllocationRuleSet(Factory);
				AllocationRulesHelper.AddFullPalletRule(ruleSet, 5);
				AllocationRulesHelper.AddPickFaceRules(ruleSet, 10);
				AllocationRulesHelper.AddFifoRule(ruleSet, 20, preventPickingPickFacesFromBulk: true);

				Helper.CreateProductUnit(data.Part1, Constants.PkgUnit.Pallet, 2m); // Part1 : 1PLT = 2 UNT

				var pickFaceLocation = data.Whs1.FindLocation("A");
				Helper.CreateProductPickFace(data.Part1, data.Org1, pickFaceLocation);

				// create two valid orders that are waiting on pf replenishment
				Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", ZDateTimeOffset.Today, data.Part1, 2m, pickFaceLocation, "");
				var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O3", data.Part1, 1m, WhsPickOption.Codes.Manual);
				var pick1 = Helper.CreatePickNew(order1);
				pick1.WP_IsAwaitingReplenishment = true;

				var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O4", data.Part1, 1m, WhsPickOption.Codes.Manual);
				var pick2 = Helper.CreatePickNew(order2);
				pick2.WP_IsAwaitingReplenishment = true;
				Factory.Save();

				var manager = new AllocatePicksAwaitingReplenishmentWithExistingInventoryProcessingManager(ObjectFactory.Get<IPartiallyReplenishedPickSplitter>());

				// over-pick stock then attempt to allocate, the over-pick trigger should then prevent save
				manager.OnNewFactorySaving += (f) =>
				{
					var orderInServiceTaskFactory = f.Load<WhsOrder>(order1.PK);
					if (orderInServiceTaskFactory.Lines[0].PickLines.Count > 0)
					{
						orderInServiceTaskFactory.Lines[0].PickLines[0].WZ_Units += 5m;
						orderInServiceTaskFactory.Lines[0].WE_TransactionQuantity += 5m;
					}
				};

				manager.AllocateAwaitingReplenishmentPicks(Logger, new CancellationToken());

				AssertEquals(
	@"Information|Pick No: P00000002 has successfully allocated stock.
Error|Pick No: P00000001 could not be allocated. Another job has taken some of the stock that this service task tried to allocate.", Logger.ToString().Trim());
			}
		}

		#endregion

		#region Implementation

		#region Factory

		BusinessObjectFactory Factory => factory ?? (factory = new BusinessObjectFactory(testCaseDbConnection));
		BusinessObjectFactory factory;
		DbConnection testCaseDbConnection;

		protected override void SetUp() => testCaseDbConnection = Db.NewExtraConnectionToMainDb();

		protected override void TearDown()
		{
			testCaseDbConnection.Dispose();
			testCaseDbConnection = null;
		}

		#endregion

		#region Helper

		WhsTestHelperFunctions Helper => helper ?? (helper = new WhsTestHelperFunctions(Factory));
		WhsTestHelperFunctions helper;

		#endregion

		#region Logger

		TestServiceLogger Logger => logger ?? (logger = new TestServiceLogger());
		TestServiceLogger logger;

		#endregion

		#endregion
	}

	#endregion
}
