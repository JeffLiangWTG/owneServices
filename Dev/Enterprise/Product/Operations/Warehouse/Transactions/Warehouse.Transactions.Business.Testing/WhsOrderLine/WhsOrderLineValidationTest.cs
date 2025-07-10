using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.Warehouse.Environment.CodeLists;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	public class WhsOrderLineValidationTest : WhsPickableDocketLineValidationTest<WhsOrderLine, WhsOrder>
	{
		#region TestCheckPickGroupForBinding

		public void TestCheckPickGroupForBinding()
		{
			var orderLine = GetNewDocketLine();
			var collection = new PickGroupCollection();
			var pickGroup = collection.AddNew();
			pickGroup.Description = (NoResString)"Desc";

			using (WarehouseDataRegistry.Instance.PickGroups.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, collection))
			{
				AssertNoErrors("Precondition:", orderLine.PickGroupForBindingInfo);

				orderLine.PickGroupForBinding = "1";
				AssertNoErrors(orderLine.PickGroupForBindingInfo);

				orderLine.PickGroupForBinding = "2";
				AssertHasError(orderLine.PickGroupForBindingInfo, "Enter a valid Pick Group.");

				orderLine.PickGroupForBinding = "";
				AssertNoErrors(orderLine.PickGroupForBindingInfo);

				orderLine.WE_PickGroup = 3;
				AssertHasError(orderLine.PickGroupForBindingInfo, "Enter a valid Pick Group.");
			}
		}

		#endregion

		#region TestCheckProductHasPalletDefinition

		public void TestCheckProductHasPalletDefinition()
		{
			OrgSupplierPart product = Helper.CreateProduct(Helper.CreateClient(), "P1");
			DocketLine.WE_OP = product.PK;

			if (!Globals.IsWeb)
			{
				AssertHasWarning(DocketLine.WE_OPInfo, WhsOrderLineValidation.ProductHasNoPalletDefinitionError);

				using (new SemaphoreManager(Docket.FinaliseDocketSemaphore))
				{
					AssertEquals("Precondition", true, Docket.IsFinalising);
					DocketLine.Validation.ValidateWE_OP();
					AssertNoWarning("Validating HasPalletDefinition is expensive, dont add the warning during pre-finalise validation.",
						DocketLine.WE_OPInfo, WhsOrderLineValidation.ProductHasNoPalletDefinitionError);
				}
			}
			else
			{
				AssertNoWarning(DocketLine.WE_OPInfo, WhsOrderLineValidation.ProductHasNoPalletDefinitionError);
			}

			DocketLine.ReadOnly = true;
			DocketLine.Validation.ValidateWE_OP();
			AssertNoWarning(DocketLine.WE_OPInfo, WhsOrderLineValidation.ProductHasNoPalletDefinitionError);

			DocketLine.ReadOnly = false;
			Helper.CreateProductUnit(product, "PLT", 1m);
			DocketLine.Validation.ValidateWE_OP();
			AssertNoWarning(DocketLine.WE_OPInfo, WhsOrderLineValidation.ProductHasNoPalletDefinitionError);
		}

		#endregion

		#region TestCheckCanResell

		public void TestCheckCanResell()
		{
			OrgSupplierPart product = Helper.CreateProduct(Helper.CreateClient(), "P1");
			product.OP_CanResell = false;
			DocketLine.WE_OP = product.PK;
			AssertHasError(DocketLine.WE_OPInfo, OrgSupplierPartCollection.ProductIsNotForResaleErrorMessage);

			DocketLine.ReadOnly = true;
			DocketLine.Validation.ValidateWE_OP();
			AssertNoError(DocketLine.WE_OPInfo, OrgSupplierPartCollection.ProductIsNotForResaleErrorMessage);

			DocketLine.ReadOnly = false;
			product.OP_CanResell = true;
			DocketLine.Validation.ValidateWE_OP();
			AssertNoError(DocketLine.WE_OPInfo, OrgSupplierPartCollection.ProductIsNotForResaleErrorMessage);
		}

		#endregion

		#region TestCheckWE_OPShouldNotBeChangedIfThisOrderLineHasReservedInventory

		public void TestCheckWE_OPShouldNotBeChangedIfThisOrderLineHasReservedInventory()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m, finalise: false);
			var inventory = receive.Inventory[0];
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			Factory.Save();

			AssertNotNull("Precondition", orderLine.ReserveStockIfAbleTo(inventory));
			AssertNoErrors("Precondition", orderLine.WE_OPInfo);

			orderLine.WE_OP = data.Part2.PK;
			AssertHasError(orderLine.WE_OPInfo, "There is Inventory Cross Docked to this Order Line, you cannot change the Product.");

			orderLine.WE_OP = data.Part1.PK;
			AssertNoErrors(orderLine.WE_OPInfo);
		}

		#endregion

		#region TestCheckWE_RX_NKUnitPriceCurrency

		public void TestCheckWE_RX_NKUnitPriceCurrency()
		{
			DocketLine.WE_RX_NKUnitPriceCurrency = ZString.Empty;
			AssertNoErrors(DocketLine.WE_RX_NKUnitPriceCurrencyInfo);

			DocketLine.WE_RX_NKUnitPriceCurrency = "XXX";
			AssertHasError(DocketLine.WE_RX_NKUnitPriceCurrencyInfo, "Please enter a valid currency.");

			DocketLine.WE_RX_NKUnitPriceCurrency = "AUD";
			AssertNoErrors(DocketLine.WE_RX_NKUnitPriceCurrencyInfo);
		}

		#endregion

		#region TestCheckWE_TransactionQuantityDoesNotGoBelowReservedAmount

		public void TestCheckWE_TransactionQuantityDoesNotGoBelowReservedAmount()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m, finalise: false);
			var inventory = receive.Inventory[0];
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			Factory.Save();

			AssertNotNull("Precondition", orderLine.ReserveStockIfAbleTo(inventory));
			AssertNoErrors("Precondition", orderLine.WE_TransactionQuantityInfo);

			orderLine.WE_TransactionQuantity = 9m;
			AssertHasError(orderLine.WE_TransactionQuantityInfo, "This Order Line has 10 units of Cross Docked Inventory, you cannot set the Quantity less than this.");

			orderLine.WE_TransactionQuantity = 10m;
			AssertNoErrors(orderLine.WE_TransactionQuantityInfo);
		}

		#endregion

		#region TestCheckWE_TransactionQuantityDoesNotGoBelowPackedQty

		public void TestCheckWE_TransactionQuantityDoesNotGoBelowPackedQty()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 20m, finalise: true);
			var inventory = receive.Inventory[0];
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 20m);
			var pick = Helper.CreatePickNew(order);
			var orderedInventory = pick.OrderedInventories[0];
			AssertEquals("Precondition", 20m, orderedInventory.PickLineQuantity);

			var releaseLine = orderLine.ReleaseLines[0];
			var package = order.PackageJob.Packages.AddNew();
			package.Pack(releaseLine, 12m);
			Factory.Save();

			AssertNoErrors("Precondition", orderLine.WE_TransactionQuantityInfo);

			orderLine.WE_TransactionQuantity = 9m;
			AssertHasError(orderLine.WE_TransactionQuantityInfo, "This Order Line has 12 units packed, you cannot set the Quantity less than this.");

			orderLine.WE_TransactionQuantity = 12m;
			AssertNoErrors(orderLine.WE_TransactionQuantityInfo);
		}

		#endregion

		#region TestCheckWE_TransactionQuantityDoesNotAllowTooLargeTotalLineWeight

		public void TestCheckWE_TransactionQuantityDoesNotAllowTooLargeTotalLineWeight()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			data.Part1.OP_Weight = 1000m;
			data.Part1.OP_WeightUQ = "KG";

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderline = Helper.CreateWhsOrderLine(order, data.Part1, 1);
			Factory.Save();
			AssertNoErrors("Precondition:", orderline.WE_TransactionQuantityInfo);

			orderline.WE_TransactionQuantity = 2000m;
			AssertHasError(orderline.WE_TransactionQuantityInfo, "The number 2,000.000 will make the Warehouse Order TEST's Total Line Weight too large, the current value is 2,000,000.000, and the maximum value is 999,999.999.");
		}

		public void TestCheckWE_TransactionQuantityDoesNotAllowTooLargeTotalLineWeight_EmptySupplierPart()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			data.Part1.OP_Weight = 1000m;
			data.Part1.OP_WeightUQ = "KG";

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderline = Helper.CreateWhsOrderLine(order, data.Part1, 1);
			Factory.Save();
			AssertNoErrors("Precondition:", orderline.WE_TransactionQuantityInfo);

			orderline.WE_OP = ZGuid.Empty;
			orderline.Validation.ValidateWE_OP();
			Assert("Precondition: Expected WE_OP to have errors.", orderline.WE_OPInfo.HasErrors());

			orderline.WE_TransactionQuantity = 1234567890m;
			AssertEquals("Expected WE_TransactionQuantity not to have any errors since Product is empty.", false, orderline.WE_TransactionQuantityInfo.HasErrors());
		}

		public void TestCheckWE_TransactionQuantityDoesNotAllowTooLargeTotalLineWeight_InvalidQuantity()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			data.Part1.OP_Weight = 1000m;
			data.Part1.OP_WeightUQ = "KG";

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderline = Helper.CreateWhsOrderLine(order, data.Part1, 1);
			Factory.Save();
			AssertNoErrors("Precondition:", orderline.WE_TransactionQuantityInfo);

			orderline.WE_OP = ZGuid.NewZGuid();
			orderline.WE_TransactionQuantity = 2000m;
			AssertNull("Expected SupplierPart to be null.", orderline.SupplierPart);
			AssertEquals("Expected WE_TransactionQuantity not to have any errors since Product is invalid.", false, orderline.WE_TransactionQuantityInfo.HasErrors());

			orderline.WE_OP = data.Part1.PK;
			orderline.WE_TransactionQuantity = 2000m;
			AssertNotNull("Expected SupplierPart not to be null.", orderline.SupplierPart);
			AssertHasError("Expected weight error.", orderline.WE_TransactionQuantityInfo, "The number 2,000.000 will make the Warehouse Order TEST's Total Line Weight too large, the current value is 2,000,000.000, and the maximum value is 999,999.999.");

			orderline.WE_TransactionQuantity = 1m;
			AssertEquals("WE_TransactionQuantity should not have any errors.", false, orderline.WE_TransactionQuantityInfo.HasErrors());
		}

		#endregion

		#region TestCheckWE_TransactionQuantityDoesNotAllowTooLargeVolume

		public void TestCheckWE_TransactionQuantityDoesNotAllowTooLargeVolume()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			data.Part1.OP_Cubic = 1000m;
			data.Part1.OP_CubicUQ = "M3";

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderline = Helper.CreateWhsOrderLine(order, data.Part1, 1);
			Factory.Save();
			AssertNoErrors("Precondition:", orderline.WE_TransactionQuantityInfo);

			orderline.WE_TransactionQuantity = 2000m;
			AssertHasError(orderline.WE_TransactionQuantityInfo, "The number 2,000.000 will make the Warehouse Order TEST's Total Line Volume too large, the current value is 2,000,000.000, and the maximum value is 999,999.999.");
		}

		public void TestCheckWE_TransactionQuantityDoesNotAllowTooLargeVolume_InvalidQuantity()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			data.Part1.OP_Cubic = 1000m;
			data.Part1.OP_CubicUQ = "M3";

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderline = Helper.CreateWhsOrderLine(order, data.Part1, 1);
			Factory.Save();
			AssertNoErrors("Precondition:", orderline.WE_TransactionQuantityInfo);

			orderline.WE_OP = ZGuid.Empty;
			orderline.WE_TransactionQuantity = 2000m;
			AssertNull("Expected SupplierPart to be null.", orderline.SupplierPart);
			AssertEquals("Expected WE_TransactionQuantity not to have any errors since Product is empty.", false, orderline.WE_TransactionQuantityInfo.HasErrors());

			orderline.WE_OP = data.Part1.PK;
			orderline.WE_TransactionQuantity = 2000m;
			AssertNotNull("Expected SupplierPart not to be null.", orderline.SupplierPart);
			AssertHasError("Expected volume error.", orderline.WE_TransactionQuantityInfo, "The number 2,000.000 will make the Warehouse Order TEST's Total Line Volume too large, the current value is 2,000,000.000, and the maximum value is 999,999.999.");

			orderline.WE_TransactionQuantity = 1m;
			AssertEquals("WE_TransactionQuantity should not have any errors.", false, orderline.WE_TransactionQuantityInfo.HasErrors());
		}

		#endregion

		#region TestCheckWE_TransactionQuantity_NullPickableDocket

		public void TestCheckWE_TransactionQuantity_NullPickableDocket()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var mainProduct = Helper.CreateProduct(data.Org1, "MP1");
			mainProduct.OP_IsComponentPickedOnSalesOrder = true;
			var bomComponentProduct1 = Helper.CreateProduct(data.Org1, "BOM1");
			Helper.CreateProductBOM(mainProduct, bomComponentProduct1, 1m, Constants.PkgUnit.Unit);

			var bomComponentProduct2 = Helper.CreateProduct(data.Org1, "BOM2");
			Helper.CreateProductBOM(mainProduct, bomComponentProduct2, 3m, Constants.PkgUnit.Unit);

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderline = Helper.CreateWhsOrderLine(order, mainProduct, 1m);
			Factory.Save();

			orderline.WE_WD = ZGuid.Empty;
			AssertNoExceptionThrown(() => orderline.WE_TransactionQuantity = 20m);
		}

		#endregion

		#region TestCheckWE_OrderLineIsNotForBOMProduct

		public void TestCheckWE_OrderLineIsNotForBOMProduct()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var mainProduct = Helper.CreateProduct(data.Org1, "MP1");
			mainProduct.OP_IsComponentPickedOnSalesOrder = true;
			var bomComponentProduct1 = Helper.CreateProduct(data.Org1, "BOM1");
			Helper.CreateProductBOM(mainProduct, bomComponentProduct1, 1m, Constants.PkgUnit.Unit);

			var bomComponentProduct2 = Helper.CreateProduct(data.Org1, "BOM2");
			Helper.CreateProductBOM(mainProduct, bomComponentProduct2, 3m, Constants.PkgUnit.Unit);

			var inventoryLocation = data.Whs1.FindLocation("A-1");
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveInventoryLine(receive, mainProduct, 5m, inventoryLocation);
			Helper.CreateWhsReceiveInventoryLine(receive, bomComponentProduct1, 20m, inventoryLocation);
			Helper.CreateWhsReceiveInventoryLine(receive, bomComponentProduct2, 20m, inventoryLocation);
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertEquals("Receive should be finalised", true, receive.IsFinalised);
			Factory.Save();

			var order1 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine11 = Helper.CreateWhsOrderLine(order1, mainProduct, 7m);
			Factory.Save();

			Helper.CreatePickNew(order1);
			AssertNoErrors("Precondition", orderLine11.WE_TransactionQuantityInfo);

			orderLine11.WE_TransactionQuantity = 9m;
			AssertHasError(orderLine11.WE_TransactionQuantityInfo, "This is a BOM product that is setup so that the components are picked on the sales order. The quantity ordered cannot be changed once the pick is created.");

			orderLine11.WE_TransactionQuantity = 7m;
			AssertNoErrors(orderLine11.WE_TransactionQuantityInfo);
		}

		#endregion

		#region TestValidateWE_ShortfallQuantityCached

		protected override void TestValidateWE_ShortfallQuantityCachedCore()
		{
			var data = new TestDataForInventory(Factory);
			data.CreateSimpleInventory();
			Factory.Save();

			WhsOrder order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			order.WD_WhsOrderFulfillmentRule = WhsOrderFulfillmentRuleList.Codes.All;

			// create a line with no product
			WhsOrderLine line = order.Lines.AddNew();
			line.Validation.ValidateWE_ShortfallQuantityCached();
			AssertNoWarnings("No warning expected as Docket has no supplier part.", line.WE_ShortfallQuantityCachedInfo);
			AssertNoWarnings("No warning expected as Docket has no supplier part.", line.PickableDocket.WD_WhsOrderFulfillmentRuleInfo);

			// order 7 then pick
			line.WE_OP = data.Part1.PK;
			line.WE_TransactionQuantity = 7m;
			Helper.CreatePickNew(order);
			AssertEquals("Precondition - pick failed.", true, order.IsAttachedToPickButNotFinalised);
			AssertNoWarnings(line.WE_ShortfallQuantityCachedInfo);
			AssertNoWarnings(line.PickableDocket.WD_WhsOrderFulfillmentRuleInfo);

			// fudge the data so that we've only picked 5/7 units
			line.ReleaseLines[0].Quantity = 5;
			AssertHasWarning(line.WE_ShortfallQuantityCachedInfo, "Shortfall: Only 5 unit(s) have been selected for release");
			AssertHasWarning(order.WD_WhsOrderFulfillmentRuleInfo, ExpectedFulfillmentWarning);
		}

		readonly string ExpectedFulfillmentWarning = "The Fulfillment Rule has not been met.\r\n" +
			"Pick documentation cannot be printed until the Fulfillment Rule has been satisfied or manually overridden.";

		#endregion

		#region TestCheckWE_ShortfallQuantityCached_WithNoParentDocket

		public void TestCheckWE_ShortfallQuantityCached_WithNoParentDocket()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var line = Helper.CreateWhsOrderLine(order, data.Part2, 1m);

			order.Lines.RemoveFromRelationship(line);

			AssertNoExceptionThrown(delegate
			{ line.Validation.ValidateAll(); });
		}

		#endregion

		#region TestValidateWE_ShortfallQuantityCached_WithoutUpdatingCache

		public void TestValidateWE_ShortfallQuantityCached_WithoutUpdatingCache()
		{
			var data = new TestDataForInventory(Factory);
			data.CreateSimpleInventory();

			DocketLine.PickableDocket.WD_WhsOrderFulfillmentRule = WhsOrderFulfillmentRuleList.Codes.All;
			DocketLine.PickableDocket.WD_RequiredDate = ZDateTimeOffset.Today;

			DocketLine.WE_TransactionQuantity = 150; //shortfall of 50
			DocketLine.Validation.ValidateWE_ShortfallQuantityCached_WithoutUpdatingCache();
			AssertNoWarnings("No warning expected as Docket has no supplier part.", DocketLine.WE_ShortfallQuantityCachedInfo);
			AssertNoWarnings("No warning expected as Docket has no supplier part.", DocketLine.PickableDocket.WD_WhsOrderFulfillmentRuleInfo);

			DocketLine.WE_OP = data.Part1.PK;
			DocketLine.Validation.ValidateWE_ShortfallQuantityCached_WithoutUpdatingCache();
			AssertHasWarnings(DocketLine.WE_ShortfallQuantityCachedInfo);
			AssertHasWarning(DocketLine.PickableDocket.WD_WhsOrderFulfillmentRuleInfo, ExpectedFulfillmentWarning);

			if (!Globals.IsWeb)
			{
				DocketLine.WE_TransactionQuantity = 10;
				DocketLine.Validation.ValidateWE_ShortfallQuantityCached_WithoutUpdatingCache();
				AssertHasWarnings("Cache should not be updated, therefore the warning should still exist even tho 0 units are in shortfall.", DocketLine.WE_ShortfallQuantityCachedInfo);
				AssertHasWarning("Cache should not be updated, therefore the warning should still exist even tho 0 units are in shortfall.", DocketLine.PickableDocket.WD_WhsOrderFulfillmentRuleInfo, ExpectedFulfillmentWarning);
			}
		}

		#endregion

		#region TestCheckWE_WHC_NKOrderedHeldCode

		protected override void TestCheckWE_WHC_NKOrderedHeldCodeCore()
		{
			DocketLine.Validation.ValidateAll();
			AssertNoErrors(DocketLine.WE_WHC_NKOrderedHeldCodeInfo);

			DocketLine.WE_WHC_NKOrderedHeldCode = InventoryHoldCodes.Codes.Held;
			Docket.RunPreSaveValidation();
			AssertNoErrors(DocketLine.WE_WHC_NKOrderedHeldCodeInfo);
		}

		public void TestCheckWE_WHC_NKOrderedHeldCode_WithInvalidHoldCode()
		{
			DocketLine.WE_WHC_NKOrderedHeldCode = "INVALID";
			Docket.RunPreSaveValidation();
			AssertHasError(DocketLine.WE_WHC_NKOrderedHeldCodeInfo, "Enter a valid selection.");

			DocketLine.WE_WHC_NKOrderedHeldCode = InventoryHoldCodes.Codes.ShortPicked;
			Docket.RunPreSaveValidation();
			AssertHasError(DocketLine.WE_WHC_NKOrderedHeldCodeInfo, "SHORT hold code may not be selected.");

			DocketLine.WE_WHC_NKOrderedHeldCode = InventoryHoldCodes.Codes.LostInCycleCount;
			Docket.RunPreSaveValidation();
			AssertHasError(DocketLine.WE_WHC_NKOrderedHeldCodeInfo, "LCC hold code may not be selected.");
		}

		public void TestCheckWE_WHC_NKOrderedHeldCode_WithClientConfiguredHoldCode()
		{
			var clientHeldCode = Helper.CreateInventoryHeldCode("DEL", "Delayed");

			DocketLine.WE_WHC_NKOrderedHeldCode = clientHeldCode.WHC_Code;
			Docket.RunPreSaveValidation();
			AssertNoErrors(DocketLine.WE_WHC_NKOrderedHeldCodeInfo);
		}

		public void TestCheckWE_WHC_NKOrderedHeldCode_WithDirectedPackingConsolidation()
		{
			Docket.WD_UseDirectedPackingConsolidation = true;
			DocketLine.WE_WHC_NKOrderedHeldCode = InventoryHoldCodes.Codes.Held;
			Docket.RunPreSaveValidation();

			AssertHasError(DocketLine.WE_WHC_NKOrderedHeldCodeInfo, "Please do not enter a value.");
		}

		public void TestCheckWE_WHC_NKOrderedHeldCode_WithMixedInventoryLines()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			var order1 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "order1");
			var orderLine1 = Helper.CreateWhsOrderLine(order1, data.Part1, 10m);
			orderLine1.WE_WHC_NKOrderedHeldCode = InventoryHoldCodes.Codes.Held;
			var orderLine2 = Helper.CreateWhsOrderLine(order1, data.Part1, 10m);
			orderLine2.WE_WHC_NKOrderedHeldCode = InventoryHoldCodes.Codes.Held;

			order1.RunPreSaveValidation();
			AssertNoErrors(orderLine1.WE_WHC_NKOrderedHeldCodeInfo);
			AssertNoErrors(orderLine2.WE_WHC_NKOrderedHeldCodeInfo);
			
			var order2 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "order2");
			var orderLine3 = Helper.CreateWhsOrderLine(order2, data.Part1, 10m);
			var orderLine4 = Helper.CreateWhsOrderLine(order2, data.Part1, 10m);

			order2.RunPreSaveValidation();
			AssertNoErrors(orderLine3.WE_WHC_NKOrderedHeldCodeInfo);
			AssertNoErrors(orderLine4.WE_WHC_NKOrderedHeldCodeInfo);

			var order3 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "order3");
			var orderLine5 = Helper.CreateWhsOrderLine(order3, data.Part1, 10m);
			orderLine5.WE_WHC_NKOrderedHeldCode = InventoryHoldCodes.Codes.Held;
			var orderLine6 = Helper.CreateWhsOrderLine(order3, data.Part1, 10m);

			order3.RunPreSaveValidation();
			AssertHasError(orderLine5.WE_WHC_NKOrderedHeldCodeInfo, "All lines must be for either held or available inventory.");
			AssertHasError(orderLine6.WE_WHC_NKOrderedHeldCodeInfo, "All lines must be for either held or available inventory.");
		}

		public void TestCheckWE_WHC_NKOrderedHeldCode_PickWithMixedInventoryOrders()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			var order1 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "order1");
			var orderLine1 = Helper.CreateWhsOrderLine(order1, data.Part1, 10m);
			orderLine1.WE_WHC_NKOrderedHeldCode = InventoryHoldCodes.Codes.Held;

			var order2 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "order2");
			var orderLine2 = Helper.CreateWhsOrderLine(order2, data.Part1, 10m);
			orderLine2.WE_WHC_NKOrderedHeldCode = InventoryHoldCodes.Codes.Held;

			Factory.Save();

			var pick1 = Helper.CreatePickNew(order1);
			pick1.Orders.Add(order2);

			Factory.Save();

			orderLine2.WE_WHC_NKOrderedHeldCode = string.Empty;
			order2.RunPreSaveValidation();

			AssertHasError(orderLine2.WE_WHC_NKOrderedHeldCodeInfo, PickErrorTypes.PickContainsMixedInventoryOrders.Message);
		}

		#endregion

		#region TestValidateAll

		protected override ZString ExpectedShortfallWarning1 => "Shortfall: Only 100 unit(s) currently available";
		protected override ZString ExpectedShortfallWarning2 => "Shortfall: Only 0 unit(s) currently available";

		#region TestValidateAll_CheckIsNewLineOnLoadingOrLoadedOrDepartedOrder

		public void TestValidateAll_CheckIsNewLineOnLoadingOrLoadedOrDepartedOrder()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var carrierServicelevel = data.Org1.MiscServ.CarrierServiceLevels.AddNew();
			carrierServicelevel.PL_Code = "RD";
			carrierServicelevel.PL_CarrierServiceLevelDescription = "Road";
			var truck = Helper.CreateEquipment("T001", 1m, Core.Constants.Weight.Kilograms, 1m, Core.Constants.Volume.CubicMetres);
			var load = Helper.CreateWhsLoad(data.Org1, data.Whs1.DefaultOutboundDockDoorLocation, transportUnit: truck, startTime: DateTimeOffset.Now);

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			var orderLine = order.Lines.Single();

			order.Validation.ValidateAll();
			AssertEquals(false, order.Lines.Any(l => l.HasRowErrors));
			Factory.Save();

			Helper.CreatePickNew(order);
			var pickLine = order.Lines.Single().PickLines.Single();
			pickLine.WZ_PickedDateTime = ZDateTimeOffset.Now;
			Factory.Save();

			var package = order.PackageJob.Packages.AddNew("CTN");
			package.Pack(order.Lines[0].ReleaseLines[0], 10m);
			var pivot = Helper.CreateLoadPkgPackagePivot(package.PK, load);
			pivot.WLP_LoadedTime = ZDateTimeOffset.Now;
			pivot.WLP_GS_NKLoadingUser = "E";

			order.Validation.ValidateAll();
			AssertEquals(false, order.Lines.Any(l => l.HasRowErrors));
			Factory.Save();

			var orderInNewFactory = new BusinessObjectFactory().Load<WhsOrder>(order.PK);
			var newLine = orderInNewFactory.Lines.AddNew();
			newLine.WE_OP = data.Part1.PK;
			newLine.WE_TransactionQuantity = 1m;

			newLine.Validation.ValidateAll();
			AssertEquals(false, orderLine.HasRowErrors);
			AssertEquals(true, newLine.HasRowErrors);
			AssertEquals("Cannot add Line to an Order which is Loading, Loaded or Departed.", newLine.RowErrors.Single().Message);

			newLine.Delete();
			AssertEquals(false, order.Lines.Any(l => l.HasRowErrors));
		}

		public void TestValidateAll_CheckIsNewLineOnLoadingOrLoadedOrDepartedOrder_OrderCanHaveNewLinesAfterFinalization()
		{
			var client = Helper.CreateClient("Client");
			var product = Helper.CreateProduct("P1", client);
			var whs = Helper.CreateWarehouse("Whs", "A", 3, 1);
			whs.WW_IsVirtualWarehouse = true;
			Factory.Save();

			var locationA1 = whs.FindLocation("A-1");
			var bondedArea = Helper.CreateArea(whs, "C", AreaTypes.Codes.Bonded);
			locationA1.WLV_WA_PickingArea = bondedArea.PK;

			var receive = Helper.CreateWhsReceive(client, whs);
			Helper.CreateWhsReceiveInventoryLine(receive, product, 10m);

			receive.AllocateLocationsWithMock();
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var order = Helper.CreateWhsOrder(client, whs);
			order.WD_DocketSubType = OrderType.Codes.Customs;
			var orderLine = Helper.CreateWhsOrderLine(order, product, 10m);
			orderLine.WE_LineNo = 1;
			orderLine.CustomsData.WB_EntryKey = "123";

			var pick = Helper.CreatePickNew(order);
			pick.FinaliseAllOrders();

			AssertEquals("Precondition", 1, order.Lines.Count);

			pick.FinalisePick();
			AssertIsFinalisedPrecondition(pick);
			AssertEquals(WhsOrderStatus.Codes.Departed, order.WarehouseOrderStatus);
			Factory.Save();

			var orderInNewFactory = new BusinessObjectFactory().Load<WhsOrder>(order.PK);
			var newLine = orderInNewFactory.Lines.AddNew();
			newLine.WE_OP = product.PK;
			newLine.WE_TransactionQuantity = 1m;

			newLine.Validation.ValidateAll();
			AssertEquals(false, orderLine.HasRowErrors);
			AssertEquals(false, newLine.HasRowErrors);
		}

		public void TestValidateAll_CheckIsNewLineOnLoadingOrLoadedOrDepartedOrder_CustomsOrder()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var locationA1 = data.Whs1.FindLocation("A-1");
			var bondedArea = Helper.CreateArea(data.Whs1, "C", AreaTypes.Codes.Bonded);
			locationA1.WLV_WA_PickingArea = bondedArea.PK;
			Factory.Save();

			var carrierServicelevel = data.Org1.MiscServ.CarrierServiceLevels.AddNew();
			carrierServicelevel.PL_Code = "RD";
			carrierServicelevel.PL_CarrierServiceLevelDescription = "Road";
			var truck = Helper.CreateEquipment("T001", 1m, Core.Constants.Weight.Kilograms, 1m, Core.Constants.Volume.CubicMetres);
			var load = Helper.CreateWhsLoad(data.Org1, data.Whs1.DefaultOutboundDockDoorLocation, transportUnit: truck, startTime: DateTimeOffset.Now);

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m, locationA1, "");
			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			order.WD_DocketSubType = OrderType.Codes.Customs;
			var orderLine = order.Lines.Single();
			orderLine.WE_LineNo = 1;
			orderLine.CustomsData.WB_EntryKey = "123";

			order.Validation.ValidateAll();
			AssertEquals(false, order.Lines.Any(l => l.HasRowErrors));
			Factory.Save();

			Helper.CreatePickNew(order);
			var pickLine = order.Lines.Single().PickLines.Single();
			pickLine.WZ_PickedDateTime = ZDateTimeOffset.Now;
			Factory.Save();

			var package = order.PackageJob.Packages.AddNew("CTN");
			package.Pack(order.Lines[0].ReleaseLines[0], 10m);
			var pivot = Helper.CreateLoadPkgPackagePivot(package.PK, load);
			pivot.WLP_LoadedTime = ZDateTimeOffset.Now;
			pivot.WLP_GS_NKLoadingUser = "E";

			order.Validation.ValidateAll();
			AssertEquals(false, order.Lines.Any(l => l.HasRowErrors));
			Factory.Save();

			var orderInNewFactory = new BusinessObjectFactory().Load<WhsOrder>(order.PK);
			var newLine = orderInNewFactory.Lines.AddNew();
			newLine.WE_OP = data.Part1.PK;
			newLine.WE_TransactionQuantity = 1m;

			newLine.Validation.ValidateAll();
			AssertEquals(false, orderLine.HasRowErrors);
			AssertEquals(true, newLine.HasRowErrors);
			AssertEquals("Cannot add Line to an Order which is Loading, Loaded or Departed.", newLine.RowErrors.Single().Message);
		}

		public void TestValidateAll_CheckIsNewLineOnLoadingOrLoadedOrDepartedOrder_VirtualWarehouse()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			data.Whs1.WW_IsVirtualWarehouse = true;
			Factory.Save();

			var carrierServicelevel = data.Org1.MiscServ.CarrierServiceLevels.AddNew();
			carrierServicelevel.PL_Code = "RD";
			carrierServicelevel.PL_CarrierServiceLevelDescription = "Road";
			var truck = Helper.CreateEquipment("T001", 1m, Core.Constants.Weight.Kilograms, 1m, Core.Constants.Volume.CubicMetres);
			var load = Helper.CreateWhsLoad(data.Org1, data.Whs1.DefaultOutboundDockDoorLocation, transportUnit: truck, startTime: DateTimeOffset.Now);

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			var orderLine = order.Lines.Single();

			order.Validation.ValidateAll();
			AssertEquals(false, order.Lines.Any(l => l.HasRowErrors));
			Factory.Save();

			Helper.CreatePickNew(order);
			var pickLine = order.Lines.Single().PickLines.Single();
			pickLine.WZ_PickedDateTime = ZDateTimeOffset.Now;
			Factory.Save();

			var package = order.PackageJob.Packages.AddNew("CTN");
			package.Pack(order.Lines[0].ReleaseLines[0], 10m);
			var pivot = Helper.CreateLoadPkgPackagePivot(package.PK, load);
			pivot.WLP_LoadedTime = ZDateTimeOffset.Now;
			pivot.WLP_GS_NKLoadingUser = "E";

			order.Validation.ValidateAll();
			AssertEquals(false, order.Lines.Any(l => l.HasRowErrors));
			Factory.Save();

			var orderInNewFactory = new BusinessObjectFactory().Load<WhsOrder>(order.PK);
			var newLine = orderInNewFactory.Lines.AddNew();
			newLine.WE_OP = data.Part1.PK;
			newLine.WE_TransactionQuantity = 1m;

			newLine.Validation.ValidateAll();
			AssertEquals(false, orderLine.HasRowErrors);
			AssertEquals(true, newLine.HasRowErrors);
			AssertEquals("Cannot add Line to an Order which is Loading, Loaded or Departed.", newLine.RowErrors.Single().Message);
		}

		public void TestValidateAll_CheckIsNewLineOnLoadingOrLoadedOrDepartedOrder_FinalisedPick()
		{
			var client = Helper.CreateClient("Client");
			var product = Helper.CreateProduct("P1", client);
			var whs = Helper.CreateWarehouse("Whs", "A", 3, 1);
			Factory.Save();

			var receive = Helper.CreateWhsReceive(client, whs);
			Helper.CreateWhsReceiveInventoryLine(receive, product, 10m);

			receive.AllocateLocationsWithMock();
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var order = Helper.CreateWhsOrder(client, whs);
			var orderLine = Helper.CreateWhsOrderLine(order, product, 10m);
			orderLine.WE_LineNo = 1;

			var pick = Helper.CreatePickNew(order);
			pick.FinaliseAllOrders();

			AssertEquals("Precondition", 1, order.Lines.Count);

			pick.FinalisePick();
			AssertIsFinalisedPrecondition(pick);
			AssertEquals(WhsOrderStatus.Codes.Departed, order.WarehouseOrderStatus);
			Factory.Save();

			var orderInNewFactory = new BusinessObjectFactory().Load<WhsOrder>(order.PK);
			var newLine = orderInNewFactory.Lines.AddNew();
			newLine.WE_OP = product.PK;
			newLine.WE_TransactionQuantity = 1m;

			newLine.Validation.ValidateAll();
			AssertEquals(false, orderLine.HasRowErrors);
			AssertEquals(true, newLine.HasRowErrors);
			AssertEquals("Cannot add Line to an Order which is Loading, Loaded or Departed.", newLine.RowErrors.Single().Message);
		}

		#endregion

		#endregion

		#region TestValidateAll_WithPickGroup

		public void TestValidateAll_WithPickGroup()
		{
			var orderLine = GetNewDocketLine();
			using (orderLine.GetValidationSuspender())
			{
				orderLine.PickGroupForBinding = "3";
			}
			AssertNoErrors("Precondition:", orderLine.PickGroupForBindingInfo);

			orderLine.Validation.ValidateAll();
			AssertHasError(orderLine.PickGroupForBindingInfo, "Enter a valid Pick Group.");
		}

		#endregion

		#region Attributes

		#region IsJulianBatchNumberFormatValidationRequired

		protected override bool IsJulianBatchNumberFormatValidationRequired(ZPropertyInfo partAttributeInfo)
		{
			return !partAttributeInfo.Value.IsEmpty;
		}

		#endregion

		protected override bool IsReleaseCapturedValidationRequired(WhsOrderLine line)
		{
			return true;
		}

		#endregion

		#region TestCheckWE_PalletID

		public void TestCheckWE_PalletID_InvalidProduct()
		{
			var data = new TestDataSimpleEnvironment(Factory, 3, 1);
			var location1 = data.Whs1.FindLocation("A-1");

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 1m);
			orderLine.WE_OP = ZGuid.Invalid;
			AssertNoExceptionThrown(() => orderLine.WE_PalletID = "ABC");
			AssertNoErrors(orderLine.WE_PalletIDInfo);
		}

		public void TestCheckWE_PalletID_ProductHasFixedPickFace()
		{
			var data = new TestDataSimpleEnvironment(Factory, 3, 1);
			var location1 = data.Whs1.FindLocation("A-1");
			Helper.CreateProductPickFace(data.Part1, data.Org1, location1);

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 1m);
			orderLine.WE_PalletID = "ABC";
			AssertHasError(orderLine.WE_PalletIDInfo, "Product P1 has Fixed Pick Face configured. No Pallet ID entry allowed.");

			orderLine.WE_PalletID = "";
			AssertNoError(orderLine.WE_PalletIDInfo, "Product P1 has Fixed Pick Face configured. No Pallet ID entry allowed.");
		}

		public void TestCheckWE_PalletID_ProductHasFixedPickFace_ProductChanged()
		{
			var data = new TestDataSimpleEnvironment(Factory, 3, 1);
			var part2 = Helper.CreateProduct(data.Org1, "P2");
			var location1 = data.Whs1.FindLocation("A-1");
			Helper.CreateProductPickFace(data.Part1, data.Org1, location1);

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 1m);
			orderLine.WE_PalletID = "ABC";
			AssertHasError(orderLine.WE_PalletIDInfo, "Product P1 has Fixed Pick Face configured. No Pallet ID entry allowed.");

			orderLine.WE_OP = part2.PK;
			AssertNoError(orderLine.WE_PalletIDInfo, "Product P1 has Fixed Pick Face configured. No Pallet ID entry allowed.");

			orderLine.WE_OP = data.Part1.PK;
			AssertHasError(orderLine.WE_PalletIDInfo, "Product P1 has Fixed Pick Face configured. No Pallet ID entry allowed.");
		}

		public void TestCheckWE_PalletID_ProductHasDynamicPickFaceArea()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var bulkLocation = data.Whs1.FindLocation("A-1");
			var dynamicPickFaceLocation = data.Whs1.FindLocation("A-2");
			var dynamicArea = Helper.CreateArea(data.Whs1, "DYNAMIC");
			dynamicPickFaceLocation.WLV_WA_PickingArea = dynamicArea.PK;
			var dynamicLocationType = Helper.CreateLocationType("DLC", LocationClasses.Codes.DPF);
			dynamicPickFaceLocation.WLV_WLT_LocationType = dynamicLocationType.PK;

			var whsProduct = WhsProduct.GetWhsProduct(data.Part1);
			var productParams = whsProduct.ParamsByWhsAndClient.AddNew();
			productParams.W3_OH = data.Org1.PK;
			productParams.W3_WW = data.Whs1.PK;
			productParams.W3_WA_DynamicPickFaceArea = dynamicArea.PK;

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 1m);

			orderLine.WE_PalletID = "ABC";
			AssertHasError(orderLine.WE_PalletIDInfo, "Product P1 has Dynamic Pick Face Area configured. No Pallet ID entry allowed.");

			orderLine.WE_PalletID = "";
			AssertNoError(orderLine.WE_PalletIDInfo, "Product P1 has Dynamic Pick Face Area configured. No Pallet ID entry allowed.");
		}

		#endregion

		#region ValidStatuses

		protected override IEnumerable<ZString> ValidStatuses => new ZString[] { "", "FIN", "DEP", "CAN" };

		#endregion

		#region Implementation

		protected override FinalisableDocketHelper<WhsOrder> GetNewDocketHelper()
		{
			return new FinalisableOrderHelper(Factory);
		}

		protected void AttachLineToCustomsOrder()
		{
			DocketLine.Order.InvoiceLink
				= new WhsDocketTestCase<WhsPickableDocket>.InvoiceLinkTest();
		}

		#endregion
	}
}
