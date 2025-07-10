using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.Packing.Business.Testing;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	class WhsPickFetchStrategyTest : WhsTestCaseWithFactory
	{
		#region TestDBHitsForPickValidation

		[StressTest]
		public void TestDBHitsForPickValidation() => TestDBHitsForPickValidation_Core();

		[StressTest]
		public void TestDBHitsForPickValidation_HeldGoodsForOrdersDisabled() => TestDBHitsForPickValidation_Core(enableHeldGoodsForOrders: false);

		void TestDBHitsForPickValidation_Core(bool enableHeldGoodsForOrders = true)
		{
			using (WarehouseDataRegistry.Instance.EnableHeldGoodsForOrders.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, enableHeldGoodsForOrders))
			{
				var data = new TestDataSimpleEnvironment(Factory, 2, 1);
				var orderList = new List<WhsOrder>();
				for (int orderCount = 0; orderCount < 10; orderCount++)
				{
					var postfix = orderCount.ToString();
					var client = Helper.CreateClient("C" + postfix);
					var order = Helper.CreateWhsOrder(client, data.Whs1, "O" + postfix, WhsPickOption.Codes.Manual);
					for (int orderLineCount = 0; orderLineCount < 10; orderLineCount++)
					{
						var product = Helper.CreateProduct(client, "P" + postfix + orderLineCount.ToString());
						Helper.CreateWhsReceiveWithInventory(client, data.Whs1, "R" + postfix + orderLineCount.ToString(), ZDateTimeOffset.Today.AddDays(-1), product, 2, true, true);

						Helper.CreateWhsOrderLine(order, product, 1m);
					}
					orderList.Add(order);
				}

				var pick = Helper.CreatePickNew(orderList.ToArray());
				Helper.CreateWhsDockDoorAssignment(data.Whs1.DefaultInboundDockDoorLocation, pick);
				Factory.Save();

				var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
				var pickInNewFactory = newFactory.Load<WhsPick>(pick.PK);

				using (RowFactory.SetCachedTables())
				{
					pickInNewFactory.RunPreSaveValidationWithFetchHints();
				}

				var dbHits = new Dictionary<string, int>
				{
					{ JobDocAddressSchema.Constants.TableName, 2 },
					{ OrgAddressSchema.Constants.TableName, 2 },
					{ OrgAddressCapabilitySchema.Constants.TableName, 1 },
					{ OrgCompanyDataSchema.Constants.TableName, 1 },
					{ OrgContactSchema.Constants.TableName, 1 },
					{ OrgCusCodeSchema.Constants.TableName, 1 },
					{ OrgCustomLabelsSchema.Constants.TableName, 1 },
					{ OrgHeaderSchema.Constants.TableName, 1 },
					{ OrgMiscServSchema.Constants.TableName, 1 },
					{ OrgPartRelationSchema.Constants.TableName, 2 },
					{ OrgPartUnitSchema.Constants.TableName, 2 },
					{ OrgSupplierPartSchema.Constants.TableName, 2 },
					{ WhsAreaSchema.Constants.TableName, 1 },
					{ WhsDocketSchema.Constants.TableName, 4 },
					{ WhsDocketContainerSchema.Constants.TableName, 1 },
					{ WhsDocketLineSchema.Constants.TableName, 1 },
					{ WhsInventoryViewSchema.Constants.TableName, 1 },
					{ WhsPickSchema.Constants.TableName, 1 },
					{ WhsPickLineSchema.Constants.TableName, 2 }, // 1 Load With WZ_WE_TransactionLine on OrderedInventory
					{ WhsWarehouseSchema.Constants.TableName, 1 },
					// increased by A.V from 0 to 1 due to introduction of Dock Door Location
					// increased by MMC from 1 to 2 due to checking Area Type for non Customs Orders
					{ WhsLocationViewSchema.Constants.TableName, 2 },
					// increased by A.V from 0 to 1 due to introduction of Dock Door Location
					{ WhsLocationTypeSchema.Constants.TableName, 1 },
					{ PkgPackageSchema.Constants.TableName, 1 }, // Because we hook onto pkgpackagejob and packages.
					{ PkgPackageJobSchema.Constants.TableName, 1 },
					{ RefCountrySchema.Constants.TableName, 1 },
					{ RefPacksSchema.Constants.TableName, 1 },
					{ RefPackTypeSchema.Constants.TableName, 1 },
					{ GlbBranchSchema.Constants.TableName, 1 },
					{ RefTimeZoneSchema.Constants.TableName, 1 },
					{ RefTimeZoneSetSchema.Constants.TableName, 1 },
					{ RefUNLOCOSchema.Constants.TableName, 1 },
					{ WhsDockDoorAssignmentSchema.Constants.TableName, 1 }
				};

				AssertDbHits(dbHits, newFactory);
			}
		}

		#endregion

		#region TestDBHitsForPickValidation_PickByBOM

		[StressTest]
		public void TestDBHitsForPickValidation_PickByBOM()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WW_IsPickByUOMEnabled = true;

			var packingHelper = new PackingTestHelper(Factory);
			var splitCaseRefType = packingHelper.CreateRefPackType("1", "1", 3m, 6m, 12m, Constants.Length.Feet, 7, Constants.Weight.Pounds, UOMPackTypesList.Codes.SplitCase);

			var bike = Helper.CreateProduct(data.Org1, "BIKE");
			bike.OP_IsComponentPickedOnSalesOrder = true;
			var wheel = Helper.CreateProduct(data.Org1, "WHEEL");
			var frame = Helper.CreateProduct(data.Org1, "FRAME");
			Helper.CreateProductBOM(bike, wheel, 2m, splitCaseRefType.F3_Code);
			Helper.CreateProductBOM(bike, frame, 1m, splitCaseRefType.F3_Code);

			bike.OP_StockKeepingUnit = splitCaseRefType.F3_Code;
			wheel.OP_StockKeepingUnit = splitCaseRefType.F3_Code;
			frame.OP_StockKeepingUnit = splitCaseRefType.F3_Code;
			data.Part2.OP_StockKeepingUnit = splitCaseRefType.F3_Code;
			Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			Helper.CreateWhsReceiveInventoryLine(receive, wheel, 100m, data.Whs1.FindLocation("A"));
			Helper.CreateWhsReceiveInventoryLine(receive, frame, 50m, data.Whs1.FindLocation("A"));
			receive.FinaliseDocket();
			AssertEquals("Receive should be finalised.", true, receive.IsFinalised);
			Factory.Save();

			var orderList = new List<WhsOrder>();
			for (int orderCount = 0; orderCount < 10; orderCount++)
			{
				var postfix = orderCount.ToString();
				var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O" + postfix, bike, 1m);
				for (int orderLineCount = 0; orderLineCount < 10; orderLineCount++)
				{
					Helper.CreateWhsOrderLine(order, bike, 1m);
				}
				orderList.Add(order);
			}

			var pick = Helper.CreatePickNew(orderList.ToArray());
			Factory.Save();

			var hasPickByBomProducts = pick.Orders.Cast<WhsOrder>().SelectMany(o => o.Lines).Cast<WhsPickableDocketLine>().Any(l => l.IsBOMProduct && l.ChildComponentLines.Count > 0);
			AssertEquals("Precondition - Picked By BOM", true, hasPickByBomProducts);

			var dbHits = new Dictionary<string, int>
				{
					{ GlbBranchSchema.Constants.TableName, 1 },
					{ JobDocAddressSchema.Constants.TableName, 2 },
					{ OrgAddressSchema.Constants.TableName, 2 },
					{ OrgAddressCapabilitySchema.Constants.TableName, 1 },
					{ OrgCompanyDataSchema.Constants.TableName, 1 },
					{ OrgContactSchema.Constants.TableName, 1 },
					{ OrgCusCodeSchema.Constants.TableName, 1 },
					{ OrgCustomLabelsSchema.Constants.TableName, 1 },
					{ OrgHeaderSchema.Constants.TableName, 1 },
					{ OrgMiscServSchema.Constants.TableName, 1 },
					{ OrgPartRelationSchema.Constants.TableName, 1 },
					{ OrgPartUnitSchema.Constants.TableName, 1 },
					{ OrgSupplierPartSchema.Constants.TableName, 1 },
					{ OrgPartBOMSchema.Constants.TableName, 1 },
					{ RefPackTypeSchema.Constants.TableName, 1 },
					{ WhsAreaSchema.Constants.TableName, 1 },
					{ WhsDocketSchema.Constants.TableName, 3 },
					{ WhsDocketContainerSchema.Constants.TableName, 1 },
					// increased by AUS from 6 to 8 - due to refactor of pick by BOM
					{ WhsDocketLineSchema.Constants.TableName, 8 },
					{ WhsInventoryViewSchema.Constants.TableName, 1 },
					{ WhsPickSchema.Constants.TableName, 1 },
					// increased by A.V from 4 to 5 - due to rework of pick line collections
					// increased by AUS from 5 to 7 - due to refactor of pick by BOM
					{ WhsPickLineSchema.Constants.TableName, 7 },
					// increased by A.V from 1 to 2 - due to introduction of Dock Door Location
					{ WhsLocationViewSchema.Constants.TableName, 2 },
					// increased by A.V from 0 to 1 - due to introduction of Dock Door Location
					{ WhsLocationTypeSchema.Constants.TableName, 1 },
					{ WhsWarehouseSchema.Constants.TableName, 1 },
					{ PkgPackageSchema.Constants.TableName, 1 },
					{ PkgPackageJobSchema.Constants.TableName, 1 },
					{ RefCountrySchema.Constants.TableName, 1 },
					{ RefPacksSchema.Constants.TableName, 1 },
					{ RefTimeZoneSchema.Constants.TableName, 1 },
					{ RefTimeZoneSetSchema.Constants.TableName, 1 },
					{ RefUNLOCOSchema.Constants.TableName, 1 },
				};

			var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			using (AssertDbHitsWithUsefulQueryInformation(dbHits, newFactory))
			{
				var pickInNewFactory = newFactory.Load<WhsPick>(pick.PK);

				using (RowFactory.SetCachedTables())
				{
					pickInNewFactory.RunPreSaveValidationWithFetchHints();
				}
			}
		}

		#endregion

		#region TestFetchForValidateForWorkOrderPicks

		public void TestFetchForValidateForWorkOrderPicks()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R", data.Part2, 2m);
			Factory.Save();

			var bomProduct = Helper.CreateProductBOM(data.Part1, data.Part2);
			var workOrder = Helper.CreateWhsWorkOrderWithLine(data.Org1, data.Whs1, data.Part1, 1m);
			var pick = Helper.CreatePickNew(workOrder);
			AssertNoExceptionThrown(() => pick.FetchStrategy.FetchForValidate());
		}

		#endregion
	}
}
