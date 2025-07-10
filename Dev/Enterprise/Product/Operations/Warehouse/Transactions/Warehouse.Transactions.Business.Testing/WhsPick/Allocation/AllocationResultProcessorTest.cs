using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using WTG.ProductionRules.Business.ProductWarehouseAllocation;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	class AllocationResultProcessorTest : WhsTestCaseWithFactory
	{
		public void TestProcessResults_NullPick_Throws()
		{
			var processor = new AllocationResultProcessor();
			AssertExceptionThrown<ArgumentNullException>(() =>
				processor.ProcessResults(null, Enumerable.Empty<AllocationResultFact>()));
		}

		public void TestProcessResults_NullResults_Throws()
		{
			var processor = new AllocationResultProcessor();
			AssertExceptionThrown<ArgumentNullException>(() => processor.ProcessResults(Factory.New<WhsPick>(), null));
		}

		public void TestProcessResults_InvalidOrderedInventoryPK_Throws()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 5m);

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			order.WD_PickOption = WhsPickOption.Codes.Manual;
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			Factory.Save();

			var pick = Helper.CreatePickNew(order);
			var orderedInventory = pick.OrderedInventories[0];
			var availableInventory = orderedInventory.AvailableInventories[0];

			var result = new AllocationResultFact(
				Guid.NewGuid(),
				"TEST",
				null,
				orderLine.PK.ToGuid(),
				Guid.NewGuid(),
				availableInventory.PK.ToGuid(),
				1m);

			var processor = new AllocationResultProcessor();
			var ex = AssertExceptionThrown<ArgumentException>(() => processor.ProcessResults(pick, new[] { result }));
			AssertEquals("Could not find ordered inventory!", ex.Message);
		}

		public void TestProcessResults_InvalidAvailableInventoryPK_Throws()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 5m);

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			order.WD_PickOption = WhsPickOption.Codes.Manual;
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			Factory.Save();

			var pick = Helper.CreatePickNew(order);
			var orderedInventory = pick.OrderedInventories[0];
			var availableInventory = orderedInventory.AvailableInventories[0];

			var result = new AllocationResultFact(
				Guid.NewGuid(),
				"TEST",
				null,
				orderLine.PK.ToGuid(),
				orderedInventory.PK.ToGuid(),
				Guid.NewGuid(),
				1m);

			var processor = new AllocationResultProcessor();
			var ex = AssertExceptionThrown<ArgumentException>(() => processor.ProcessResults(pick, new[] { result }));
			AssertEquals("Could not find available inventory!", ex.Message);
		}

		public void TestProcessResults_InvalidOrderLinePK_Throws()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 5m);

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			order.WD_PickOption = WhsPickOption.Codes.Manual;
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			Factory.Save();

			var pick = Helper.CreatePickNew(order);
			var orderedInventory = pick.OrderedInventories[0];
			var availableInventory = orderedInventory.AvailableInventories[0];

			var result = new AllocationResultFact(
				Guid.NewGuid(),
				"TEST",
				null,
				Guid.NewGuid(),
				orderedInventory.PK.ToGuid(),
				availableInventory.PK.ToGuid(),
				1m);

			var processor = new AllocationResultProcessor();
			var ex = AssertExceptionThrown<ArgumentException>(() => processor.ProcessResults(pick, new[] { result }));
			AssertEquals("Attempted to UpdatePickLineQuantity for an order line from another ordered inventory!",
				ex.Message);
		}

		public void TestProcessResults_FailsToAllocate_Throws()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 5m);

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			Factory.Save();

			var pick = Helper.CreatePickNew(order);
			var orderedInventory = pick.OrderedInventories[0];
			var availableInventory = orderedInventory.AvailableInventories[0];
			AssertEquals("Precondition: Fully allocated.", 0m, availableInventory.QuantityUnPicked);

			var result = new AllocationResultFact(
				Guid.NewGuid(),
				"TEST",
				null,
				orderLine.PK.ToGuid(),
				orderedInventory.PK.ToGuid(),
				availableInventory.PK.ToGuid(),
				1m);

			var processor = new AllocationResultProcessor();
			var ex = AssertExceptionThrown<ArgumentException>(() => processor.ProcessResults(pick, new[] { result }));
			AssertEquals("Attempted to allocate 1, but failed to allocate 1.", ex.Message);
		}

		public void TestProcessResults()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 5m);

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			order.WD_PickOption = WhsPickOption.Codes.Manual;
			var orderLine1 = Helper.CreateWhsOrderLine(order, data.Part1, 5m);
			var orderLine2 = Helper.CreateWhsOrderLine(order, data.Part1, 5m);
			Factory.Save();

			var pick = Helper.CreatePickNew(order);
			var orderedInventory = pick.OrderedInventories[0];
			var availableInventory = orderedInventory.AvailableInventories[0];

			var result = new AllocationResultFact(
				Guid.NewGuid(),
				"TEST",
				null,
				orderLine2.PK.ToGuid(),
				orderedInventory.PK.ToGuid(),
				availableInventory.PK.ToGuid(),
				2m);

			var processor = new AllocationResultProcessor();
			var processedResult = processor.ProcessResults(pick, new[] { result });
			AssertNull(processedResult.Notification);
			Assert(processedResult.InventoryAllocated);
			AssertEquals("Should have allocated stock.", 2m, availableInventory.PickLineQuantity);
			AssertEquals("Should have allocated stock to order line 2.", 2m, orderLine2.PickLineQuantity);
			AssertEquals("Should *not* have allocated stock to order line 1.", 0m, orderLine1.PickLineQuantity);
			AssertEquals("Should have logged the allocation.",
				"2 Units allocated from Rule: TEST" + System.Environment.NewLine, availableInventory.AllocationLog);
		}

		public void TestProcessResults_ExpiryDateFilter()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 5m);

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			order.WD_PickOption = WhsPickOption.Codes.Manual;
			var orderLine1 = Helper.CreateWhsOrderLine(order, data.Part1, 5m);
			var orderLine2 = Helper.CreateWhsOrderLine(order, data.Part1, 5m);
			Factory.Save();

			var pick = Helper.CreatePickNew(order);
			var orderedInventory = pick.OrderedInventories[0];
			var availableInventory = orderedInventory.AvailableInventories[0];

			var result = new AllocationResultFact(
				Guid.NewGuid(),
				"TEST",
				new DateTime(2023, 12, 25),
				orderLine2.PK.ToGuid(),
				orderedInventory.PK.ToGuid(),
				availableInventory.PK.ToGuid(),
				2m);

			var processor = new AllocationResultProcessor();
			var processedResult = processor.ProcessResults(pick, new[] { result });
			AssertNull(processedResult.Notification);
			Assert(processedResult.InventoryAllocated);
			AssertEquals("Should have allocated stock.", 2m, availableInventory.PickLineQuantity);
			AssertEquals("Should have allocated stock to order line 2.", 2m, orderLine2.PickLineQuantity);
			AssertEquals("Should *not* have allocated stock to order line 1.", 0m, orderLine1.PickLineQuantity);
			AssertEquals("Should have logged the allocation.",
				$"2 Units allocated from Rule: TEST up to Expiry Date: {result.ExpiryDateFilter.Value.ToString("d")}" + System.Environment.NewLine, availableInventory.AllocationLog);
		}

		public void TestProcessResults_Decimal()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			data.Part1.OP_CountDecimalPlaces = 2;

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 5m);

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			order.WD_PickOption = WhsPickOption.Codes.Manual;
			var orderLine1 = Helper.CreateWhsOrderLine(order, data.Part1, 5m);
			var orderLine2 = Helper.CreateWhsOrderLine(order, data.Part1, 5m);
			Factory.Save();

			var pick = Helper.CreatePickNew(order);
			var orderedInventory = pick.OrderedInventories[0];
			var availableInventory = orderedInventory.AvailableInventories[0];

			var result = new AllocationResultFact(
				Guid.NewGuid(),
				"TEST",
				null,
				orderLine2.PK.ToGuid(),
				orderedInventory.PK.ToGuid(),
				availableInventory.PK.ToGuid(),
				3.14m);

			var processor = new AllocationResultProcessor();
			var processedResult = processor.ProcessResults(pick, new[] { result });
			AssertNull(processedResult.Notification);
			Assert(processedResult.InventoryAllocated);
			AssertEquals("Should have allocated stock.", 3.14m, availableInventory.PickLineQuantity);
			AssertEquals("Should have allocated stock to order line 2.", 3.14m, orderLine2.PickLineQuantity);
			AssertEquals("Should *not* have allocated stock to order line 1.", 0m, orderLine1.PickLineQuantity);
			AssertEquals("Should have logged the allocation.",
				"3.14 Units allocated from Rule: TEST" + System.Environment.NewLine, availableInventory.AllocationLog);
		}

		public void TestProcessResults_UpdatesSplitDetails() =>
			TestProcessResults_UpdatesSplitDetails(pickByUOM: false);

		public void TestProcessResults_UpdatesSplitDetails_PickByUOM() =>
			TestProcessResults_UpdatesSplitDetails(pickByUOM: true);

		void TestProcessResults_UpdatesSplitDetails(bool pickByUOM)
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			data.Whs1.WW_IsPickByUOMEnabled = pickByUOM;

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 5m);

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			order.WD_PickOption = WhsPickOption.Codes.Manual;
			var orderLine1 = Helper.CreateWhsOrderLine(order, data.Part1, 5m);
			var orderLine2 = Helper.CreateWhsOrderLine(order, data.Part1, 5m);
			Factory.Save();

			var pick = Helper.CreatePickNew(order);
			var orderedInventory = pick.OrderedInventories[0];
			var availableInventory = orderedInventory.AvailableInventories[0];
			AssertEquals("Precondition", 0, availableInventory.AvailableInventoriesSplitByPickedDetails.Count);
			AssertEquals("Precondition", 0, availableInventory.AvailableInventoriesSplitByUOM.Count);

			var result = new AllocationResultFact(
				Guid.NewGuid(),
				"TEST",
				null,
				orderLine2.PK.ToGuid(),
				orderedInventory.PK.ToGuid(),
				availableInventory.PK.ToGuid(),
				2m);

			var refreshBindingCount = 0;
			((IBusiness)orderedInventory).ListChanged += (s, e) => refreshBindingCount++;

			var processor = new AllocationResultProcessor();
			var processedResult = processor.ProcessResults(pick, new[] { result });
			AssertNull(processedResult.Notification);
			Assert(processedResult.InventoryAllocated);
			AssertEquals("Should have allocated stock.", 2m, availableInventory.PickLineQuantity);
			AssertEquals("Should have allocated stock to order line 2.", 2m, orderLine2.PickLineQuantity);
			AssertEquals("Should *not* have allocated stock to order line 1.", 0m, orderLine1.PickLineQuantity);
			AssertEquals("Should have logged the allocation.",
				"2 Units allocated from Rule: TEST" + System.Environment.NewLine, availableInventory.AllocationLog);
			AssertHasWarning("Should have validated ordered inventory.", orderedInventory.QuantityShortInfo, "This item is short by 8 Units. Allocate more stock in the Inventory grid to bring this value to zero.");
			AssertEquals("Should have refreshed binding twice, once for validation and once at the end.", 2, refreshBindingCount);

			var pickLine = orderLine2.PickLines.Single();
			AssertContainsExactElementsInAnyOrder(new[] { pickLine }, availableInventory.PickLines);

			if (pickByUOM)
			{
				AssertEquals("Should have 1 Inventory Split.", 1,
					availableInventory.AvailableInventoriesSplitByUOM.Count);
				AssertContainsExactElementsInAnyOrder(new[] { pickLine },
					availableInventory.AvailableInventoriesSplitByUOM[0].PickLinesForPickingDetails);
				AssertContainsExactElementsInAnyOrder(new[] { pickLine },
					availableInventory.AvailableInventoriesSplitByUOM[0].PickLinesOnOrder);
			}
			else
			{
				AssertEquals("Should have 1 Inventory Split.", 1,
					availableInventory.AvailableInventoriesSplitByPickedDetails.Count);
				AssertContainsExactElementsInAnyOrder(new[] { pickLine },
					availableInventory.AvailableInventoriesSplitByPickedDetails[0].PickLinesForPickingDetails);
				AssertContainsExactElementsInAnyOrder(new[] { pickLine },
					availableInventory.AvailableInventoriesSplitByPickedDetails[0].PickLinesOnOrder);
			}
		}

		public void TestProcessResults_MultipleOrderedInventories()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var receive1 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 5m);
			var receive2 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part2, 5m);

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			order.WD_PickOption = WhsPickOption.Codes.Manual;
			var orderLine1 = Helper.CreateWhsOrderLine(order, data.Part1, 5m);
			var orderLine2 = Helper.CreateWhsOrderLine(order, data.Part2, 5m);
			Factory.Save();

			var pick = Helper.CreatePickNew(order);
			var orderedInvs = pick.OrderedInventories.Cast<WhsPickOrderedInventory>();
			var orderedInventory1 = orderedInvs.Single(inv => inv.SupplierPartPK == data.Part1.PK);
			var availableInventory1 = orderedInventory1.AvailableInventories[0];

			var orderedInventory2 = orderedInvs.Single(inv => inv.SupplierPartPK == data.Part2.PK);
			var availableInventory2 = orderedInventory2.AvailableInventories[0];

			var result1 = new AllocationResultFact(
				Guid.NewGuid(),
				"TEST",
				null,
				orderLine1.PK.ToGuid(),
				orderedInventory1.PK.ToGuid(),
				availableInventory1.PK.ToGuid(),
				2m);

			var result2 = new AllocationResultFact(
				Guid.NewGuid(),
				"TEST",
				null,
				orderLine2.PK.ToGuid(),
				orderedInventory2.PK.ToGuid(),
				availableInventory2.PK.ToGuid(),
				3m);

			var processor = new AllocationResultProcessor();
			var processedResult = processor.ProcessResults(pick, new[] { result1, result2 });
			AssertNull(processedResult.Notification);
			Assert(processedResult.InventoryAllocated);
			AssertEquals("Should have allocated stock.", 2m, availableInventory1.PickLineQuantity);
			AssertEquals("Should have allocated stock.", 3m, availableInventory2.PickLineQuantity);
			AssertEquals("Should have allocated stock to order line 1.", 2m, orderLine1.PickLineQuantity);
			AssertEquals("Should have allocated stock to order line 2.", 3m, orderLine2.PickLineQuantity);
			AssertEquals("Should have logged the allocation.",
				"2 Units allocated from Rule: TEST" + System.Environment.NewLine, availableInventory1.AllocationLog);
			AssertEquals("Should have logged the allocation.",
				"3 Units allocated from Rule: TEST" + System.Environment.NewLine, availableInventory2.AllocationLog);
		}

		public void TestProcessResults_MultipleOrderedInventories_AttributeNeutral()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);
			data.Part1.RelatedOrganisations.FindFirstByOrganisationPK(data.Org1.PK).OU_PickMode = WhsPickMode.Codes.AttributeNeutral;

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receiveLine1 = Helper.CreateWhsReceiveLine(receive, data.Part1, 1m, data.Whs1.DefaultLocation, "PLT-1");
			receiveLine1.WE_SerialNumber = "SN1";

			var receiveLine2 = Helper.CreateWhsReceiveLine(receive, data.Part1, 1m, data.Whs1.DefaultLocation, "PLT-1");
			receiveLine2.WE_SerialNumber = "SN2";

			var receiveLine3 = Helper.CreateWhsReceiveLine(receive, data.Part1, 1m, data.Whs1.DefaultLocation, "PLT-1");
			receiveLine3.WE_SerialNumber = "SN3";

			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1", WhsPickOption.Codes.Manual);
			var line1 = Helper.CreateWhsOrderLine(order, data.Part1, 1m);

			var line2 = Helper.CreateWhsOrderLine(order, data.Part1, 1m);
			line2.WE_SerialNumber = "SN1";

			var line3 = Helper.CreateWhsOrderLine(order, data.Part1, 1m);
			line3.WE_SerialNumber = "SN2";
			Factory.Save();

			var pick = Helper.CreatePickNew(order);
			Factory.Save();

			var orderedInventories = pick.OrderedInventories.Cast<WhsPickOrderedInventory>().ToArray();
			var orderedInventorySn1 = orderedInventories.Single(oi => oi.SerialNumber == "SN1");
			var availableInventorySn1 = (WhsPickAvailableInventory)orderedInventorySn1.AvailableInventories.Single();

			var orderedInventorySn2 = orderedInventories.Single(oi => oi.SerialNumber == "SN2");
			var availableInventorySn2 = (WhsPickAvailableInventory)orderedInventorySn2.AvailableInventories.Single();

			var orderedInventoryNonSpecified = orderedInventories.Single(oi => oi.SerialNumber.IsEmpty);
			var availableInventoryNonSpecified = (WhsPickAvailableInventory)orderedInventoryNonSpecified.AvailableInventories.Single();

			var result1 = new AllocationResultFact(
				Guid.NewGuid(),
				"TEST",
				null,
				line1.PK.ToGuid(),
				orderedInventoryNonSpecified.PK.ToGuid(),
				availableInventoryNonSpecified.PK.ToGuid(),
				1m);

			var result2 = new AllocationResultFact(
				Guid.NewGuid(),
				"TEST",
				null,
				line2.PK.ToGuid(),
				orderedInventorySn1.PK.ToGuid(),
				availableInventorySn1.PK.ToGuid(),
				1m);

			var result3 = new AllocationResultFact(
				Guid.NewGuid(),
				"TEST",
				null,
				line3.PK.ToGuid(),
				orderedInventorySn2.PK.ToGuid(),
				availableInventorySn2.PK.ToGuid(),
				1m);

			var processor = new AllocationResultProcessor();
			var processedResult = processor.ProcessResults(pick, new[] { result1, result2, result3 });
			AssertNull(processedResult.Notification);
			Assert(processedResult.InventoryAllocated);
			AssertEquals("Should have allocated stock.", 1m, availableInventorySn1.PickLineQuantity);
			AssertEquals("Should have allocated stock.", 1m, availableInventorySn2.PickLineQuantity);
			AssertEquals("Should have allocated stock.", 1m, availableInventoryNonSpecified.PickLineQuantity);
		}

		public void TestProcessResults_MultipleOrderedInventories_SuspendsValidation()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var receive1 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 5m);
			var receive2 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part2, 5m);

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			order.WD_PickOption = WhsPickOption.Codes.Manual;
			var orderLine1 = Helper.CreateWhsOrderLine(order, data.Part1, 5m);
			var orderLine2 = Helper.CreateWhsOrderLine(order, data.Part2, 5m);
			Factory.Save();

			var validationHitCount = 0;
			orderLine1.WE_ShortfallQuantityCachedInfo.AdditionalValidation += () => validationHitCount++;
			orderLine2.WE_ShortfallQuantityCachedInfo.AdditionalValidation += () => validationHitCount++;

			var pick = Helper.CreatePickNew(order);
			var orderedInvs = pick.OrderedInventories.Cast<WhsPickOrderedInventory>();
			var orderedInventory1 = orderedInvs.Single(inv => inv.SupplierPartPK == data.Part1.PK);
			var availableInventory1 = orderedInventory1.AvailableInventories[0];

			var orderedInventory2 = orderedInvs.Single(inv => inv.SupplierPartPK == data.Part2.PK);
			var availableInventory2 = orderedInventory2.AvailableInventories[0];

			bool? isValidationSuspendedDuringProcess1 = null;
			bool? isValidationSuspendedDuringProcess2 = null;

			orderedInventory1.AvailableInventories[0].PickLineQuantityInfo.ValueChanged += delegate
			{
				isValidationSuspendedDuringProcess1 = ((IBusiness)orderedInventory1.Owners).IsValidationSuspended;
			};

			orderedInventory2.AvailableInventories[0].PickLineQuantityInfo.ValueChanged += delegate
			{
				isValidationSuspendedDuringProcess2 = ((IBusiness)orderedInventory2.Owners).IsValidationSuspended;
			};

			var result1 = new AllocationResultFact(
				Guid.NewGuid(),
				"TEST",
				null,
				orderLine1.PK.ToGuid(),
				orderedInventory1.PK.ToGuid(),
				availableInventory1.PK.ToGuid(),
				2m);

			var result2 = new AllocationResultFact(
				Guid.NewGuid(),
				"TEST",
				null,
				orderLine2.PK.ToGuid(),
				orderedInventory2.PK.ToGuid(),
				availableInventory2.PK.ToGuid(),
				3m);

			var processor = new AllocationResultProcessor();
			var processedResult = processor.ProcessResults(pick, new[] { result1, result2 });
			AssertNull(processedResult.Notification);
			Assert(processedResult.InventoryAllocated);
			AssertEquals("Should have allocated stock.", 2m, availableInventory1.PickLineQuantity);
			AssertEquals("Should have allocated stock.", 3m, availableInventory2.PickLineQuantity);
			AssertEquals("Should have allocated stock to order line 1.", 2m, orderLine1.PickLineQuantity);
			AssertEquals("Should have allocated stock to order line 2.", 3m, orderLine2.PickLineQuantity);
			AssertEquals("Should have suspended validation on Owners", true, isValidationSuspendedDuringProcess1);
			AssertEquals("Should have suspended validation on Owners", true, isValidationSuspendedDuringProcess2);
		}

		public void TestProcessResults_MultipleResults() => TestProcessResults_MultipleResults(withExpiryFilter: false);

		public void TestProcessResults_MultipleResults_WithExpiryFilter() => TestProcessResults_MultipleResults(withExpiryFilter: true);

		void TestProcessResults_MultipleResults(bool withExpiryFilter)
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 5m);

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			order.WD_PickOption = WhsPickOption.Codes.Manual;
			var orderLine1 = Helper.CreateWhsOrderLine(order, data.Part1, 5m);
			var orderLine2 = Helper.CreateWhsOrderLine(order, data.Part1, 5m);
			Factory.Save();

			var pick = Helper.CreatePickNew(order);
			var orderedInventory = pick.OrderedInventories[0];
			var availableInventory = orderedInventory.AvailableInventories[0];

			var result1 = new AllocationResultFact(
				Guid.NewGuid(),
				"TEST",
				withExpiryFilter ? new DateTime(2023, 12, 25) : null,
				orderLine2.PK.ToGuid(),
				orderedInventory.PK.ToGuid(),
				availableInventory.PK.ToGuid(),
				2m);

			var result2 = new AllocationResultFact(
				result1.ActivationID,
				"TEST",
				result1.ExpiryDateFilter,
				orderLine2.PK.ToGuid(),
				orderedInventory.PK.ToGuid(),
				availableInventory.PK.ToGuid(),
				3m);

			var processor = new AllocationResultProcessor();
			var processedResult = processor.ProcessResults(pick, new[] { result1, result2 });
			AssertNull(processedResult.Notification);
			Assert(processedResult.InventoryAllocated);
			AssertEquals("Should have allocated stock.", 5m, availableInventory.PickLineQuantity);
			AssertEquals("Should have allocated stock to order line 2.", 5m, orderLine2.PickLineQuantity);
			AssertEquals("Should *not* have allocated stock to order line 1.", 0m, orderLine1.PickLineQuantity);

			if (withExpiryFilter)
			{
				AssertEquals("Should have logged the allocation.",
					$"5 Units allocated from Rule: TEST up to Expiry Date: {result1.ExpiryDateFilter.Value.ToString("d")}" + System.Environment.NewLine, availableInventory.AllocationLog);
			}
			else
			{
				AssertEquals("Should have logged the allocation.",
					"5 Units allocated from Rule: TEST" + System.Environment.NewLine, availableInventory.AllocationLog);
			}
		}

		public void TestProcessResults_MultipleResults_MultipleOrderLinesAllocated() =>
			TestProcessResults_MultipleResults_MultipleOrderLinesAllocated(isPickByUOM: false);

		public void TestProcessResults_MultipleResults_MultipleOrderLinesAllocated_PickByUOM() =>
			TestProcessResults_MultipleResults_MultipleOrderLinesAllocated(isPickByUOM: true);

		void TestProcessResults_MultipleResults_MultipleOrderLinesAllocated(bool isPickByUOM)
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			data.Whs1.WW_IsPickByUOMEnabled = isPickByUOM;

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 5m);

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			order.WD_PickOption = WhsPickOption.Codes.Manual;
			Helper.CreateWhsOrderLine(order, data.Part1, 1m);
			Helper.CreateWhsOrderLine(order, data.Part1, 1m);
			Helper.CreateWhsOrderLine(order, data.Part1, 1m);
			Helper.CreateWhsOrderLine(order, data.Part1, 1m);
			Helper.CreateWhsOrderLine(order, data.Part1, 1m);
			Factory.Save();

			var pick = Helper.CreatePickNew(order);
			var orderedInventory = pick.OrderedInventories[0];
			var availableInventory = orderedInventory.AvailableInventories[0];

			var listChangedCount = 0;
			((IBusinessObjectCollection)orderedInventory.PickLines).ListChanged += (s, e) => listChangedCount++;

			var pickLineQuantityValidationCount = 0;
			availableInventory.PickLineQuantityInfo.AdditionalValidation += () => pickLineQuantityValidationCount++;

			var splitByPickedDetailsRefreshCount = 0;
			if (isPickByUOM)
			{
				((IBusinessObjectCollection)availableInventory.AvailableInventoriesSplitByUOM).ListChanged +=
					(s, e) => splitByPickedDetailsRefreshCount++;
			}
			else
			{
				((IBusinessObjectCollection)availableInventory.AvailableInventoriesSplitByPickedDetails).ListChanged +=
					(s, e) => splitByPickedDetailsRefreshCount++;
			}

			var activationId = Guid.NewGuid();
			var results = new List<AllocationResultFact>();
			for (int i = 0; i < 5; i++)
			{
				results.Add(
					new AllocationResultFact(
						activationId,
						"TEST",
						null,
						order.Lines[i].PK.ToGuid(),
						orderedInventory.PK.ToGuid(),
						availableInventory.PK.ToGuid(),
						1m));
			}

			var processor = new AllocationResultProcessor();
			var processedResult = processor.ProcessResults(pick, results);
			AssertNull(processedResult.Notification);
			Assert(processedResult.InventoryAllocated);
			AssertEquals("Should have allocated stock.", 5m, availableInventory.PickLineQuantity);
			AssertEquals("Should have refreshed pick line collection once during allocation.",
				1 + (isPickByUOM ? order.Lines.Count : 0), listChangedCount);
			AssertEquals(
				"Should have refreshed split by picked details twice: on ListChanged suspension ending and RefreshBinding.",
				2, splitByPickedDetailsRefreshCount);
			AssertEquals("Should have validated pick line quantity once.", 1, pickLineQuantityValidationCount);
			AssertEquals("Should have logged the allocation.",
				"5 Units allocated from Rule: TEST" + System.Environment.NewLine, availableInventory.AllocationLog);
		}

		public void TestProcessResults_MultipleResults_MultipleActivations()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 5m);

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			order.WD_PickOption = WhsPickOption.Codes.Manual;
			var orderLine1 = Helper.CreateWhsOrderLine(order, data.Part1, 5m);
			Factory.Save();

			var pick = Helper.CreatePickNew(order);
			var orderedInventory = pick.OrderedInventories[0];
			var availableInventory = orderedInventory.AvailableInventories[0];

			var result1 = new AllocationResultFact(
				Guid.NewGuid(),
				"TEST",
				null,
				orderLine1.PK.ToGuid(),
				orderedInventory.PK.ToGuid(),
				availableInventory.PK.ToGuid(),
				2m);

			var result2 = new AllocationResultFact(
				Guid.NewGuid(),
				"TEST",
				null,
				orderLine1.PK.ToGuid(),
				orderedInventory.PK.ToGuid(),
				availableInventory.PK.ToGuid(),
				3m);

			var processor = new AllocationResultProcessor();
			var processedResult = processor.ProcessResults(pick, new[] { result1, result2 });
			AssertNull(processedResult.Notification);
			Assert(processedResult.InventoryAllocated);
			AssertEquals("Should have allocated stock.", 5m, availableInventory.PickLineQuantity);
			AssertEquals("Should have allocated stock to order line 1.", 5m, orderLine1.PickLineQuantity);
			AssertEquals("Should have logged the allocation.",
				@"2 Units allocated from Rule: TEST
3 Units allocated from Rule: TEST
", availableInventory.AllocationLog);
		}

		public void TestProcessResults_MultipleResults_MultipleRuleNames()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 5m);

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			order.WD_PickOption = WhsPickOption.Codes.Manual;
			var orderLine1 = Helper.CreateWhsOrderLine(order, data.Part1, 5m);
			Factory.Save();

			var pick = Helper.CreatePickNew(order);
			var orderedInventory = pick.OrderedInventories[0];
			var availableInventory = orderedInventory.AvailableInventories[0];

			var result1 = new AllocationResultFact(
				Guid.NewGuid(),
				"TEST1",
				null,
				orderLine1.PK.ToGuid(),
				orderedInventory.PK.ToGuid(),
				availableInventory.PK.ToGuid(),
				2m);

			var result2 = new AllocationResultFact(
				result1.ActivationID,
				"TEST2",
				null,
				orderLine1.PK.ToGuid(),
				orderedInventory.PK.ToGuid(),
				availableInventory.PK.ToGuid(),
				3m);

			var processor = new AllocationResultProcessor();
			var processedResult = processor.ProcessResults(pick, new[] { result1, result2 });
			AssertNull(processedResult.Notification);
			Assert(processedResult.InventoryAllocated);
			AssertEquals("Should have allocated stock.", 5m, availableInventory.PickLineQuantity);
			AssertEquals("Should have allocated stock to order line 1.", 5m, orderLine1.PickLineQuantity);
			AssertEquals("Should have logged the allocation.",
				@"2 Units allocated from Rule: TEST1
3 Units allocated from Rule: TEST2
", availableInventory.AllocationLog);
		}

		public void TestProcessResults_MultipleResults_MultipleExpiryDates()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 5m);

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			order.WD_PickOption = WhsPickOption.Codes.Manual;
			var orderLine1 = Helper.CreateWhsOrderLine(order, data.Part1, 5m);
			Factory.Save();

			var pick = Helper.CreatePickNew(order);
			var orderedInventory = pick.OrderedInventories[0];
			var availableInventory = orderedInventory.AvailableInventories[0];

			var result1 = new AllocationResultFact(
				Guid.NewGuid(),
				"TEST1",
				new DateTime(2023, 12, 25),
				orderLine1.PK.ToGuid(),
				orderedInventory.PK.ToGuid(),
				availableInventory.PK.ToGuid(),
				2m);

			var result2 = new AllocationResultFact(
				result1.ActivationID,
				"TEST1",
				new DateTime(2025, 3, 14),
				orderLine1.PK.ToGuid(),
				orderedInventory.PK.ToGuid(),
				availableInventory.PK.ToGuid(),
				3m);

			var processor = new AllocationResultProcessor();
			var processedResult = processor.ProcessResults(pick, new[] { result1, result2 });
			AssertNull(processedResult.Notification);
			Assert(processedResult.InventoryAllocated);
			AssertEquals("Should have allocated stock.", 5m, availableInventory.PickLineQuantity);
			AssertEquals("Should have allocated stock to order line 1.", 5m, orderLine1.PickLineQuantity);
			AssertEquals("Should have logged the allocation.",
				$@"2 Units allocated from Rule: TEST1 up to Expiry Date: {result1.ExpiryDateFilter.Value.ToString("d")}
3 Units allocated from Rule: TEST1 up to Expiry Date: {result2.ExpiryDateFilter.Value.ToString("d")}
", availableInventory.AllocationLog);
		}

		public void TestProcessResults_MultipleOrderLines()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 5m);

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			order.WD_PickOption = WhsPickOption.Codes.Manual;
			var orderLine1 = Helper.CreateWhsOrderLine(order, data.Part1, 5m);
			var orderLine2 = Helper.CreateWhsOrderLine(order, data.Part1, 5m);
			Factory.Save();

			var pick = Helper.CreatePickNew(order);
			var orderedInventory = pick.OrderedInventories[0];
			var availableInventory = orderedInventory.AvailableInventories[0];

			var result1 = new AllocationResultFact(
				Guid.NewGuid(),
				"TEST",
				null,
				orderLine1.PK.ToGuid(),
				orderedInventory.PK.ToGuid(),
				availableInventory.PK.ToGuid(),
				2m);

			var result2 = new AllocationResultFact(
				result1.ActivationID,
				"TEST",
				null,
				orderLine2.PK.ToGuid(),
				orderedInventory.PK.ToGuid(),
				availableInventory.PK.ToGuid(),
				3m);

			var processor = new AllocationResultProcessor();
			var processedResult = processor.ProcessResults(pick, new[] { result1, result2 });
			AssertNull(processedResult.Notification);
			Assert(processedResult.InventoryAllocated);
			AssertEquals("Should have allocated stock.", 5m, availableInventory.PickLineQuantity);
			AssertEquals("Should have allocated stock to order line 1.", 2m, orderLine1.PickLineQuantity);
			AssertEquals("Should have allocated stock to order line 2.", 3m, orderLine2.PickLineQuantity);
			AssertEquals("Should have logged the allocation.",
				"5 Units allocated from Rule: TEST" + System.Environment.NewLine, availableInventory.AllocationLog);
		}

		public void TestProcessResults_MultipleInventoryLines()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			data.Whs1.WW_IsPickByUOMEnabled = true;

			var receive1 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 5m, data.Whs1.DefaultLocation, "123");
			var receive2 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 5m, data.Whs1.DefaultLocation, "456");
			AssertIsFinalisedPrecondition(receive1);
			AssertIsFinalisedPrecondition(receive2);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			order.WD_PickOption = WhsPickOption.Codes.Manual;
			var orderLine1 = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			Factory.Save();

			var pick = Helper.CreatePickNew(order);
			var orderedInventory = pick.OrderedInventories[0];
			var availableInventory1 = orderedInventory.AvailableInventories[0];
			var availableInventory2 = orderedInventory.AvailableInventories[1];
			AssertEquals("Precondition", 0, availableInventory1.AvailableInventoriesSplitByPickedDetails.Count);
			AssertEquals("Precondition", 0, availableInventory1.AvailableInventoriesSplitByUOM.Count);
			AssertEquals("Precondition", 0, availableInventory2.AvailableInventoriesSplitByPickedDetails.Count);
			AssertEquals("Precondition", 0, availableInventory2.AvailableInventoriesSplitByUOM.Count);

			var quantityValidationCount = 0;
			var shortValidationCount = 0;
			orderedInventory.PickLineQuantityInfo.AdditionalValidation += () => quantityValidationCount++;
			orderedInventory.QuantityShortInfo.AdditionalValidation += () => shortValidationCount++;

			var result1 = new AllocationResultFact(
				Guid.NewGuid(),
				"TEST",
				null,
				orderLine1.PK.ToGuid(),
				orderedInventory.PK.ToGuid(),
				availableInventory1.PK.ToGuid(),
				5m);

			var result2 = new AllocationResultFact(
				Guid.NewGuid(),
				"TEST",
				null,
				orderLine1.PK.ToGuid(),
				orderedInventory.PK.ToGuid(),
				availableInventory2.PK.ToGuid(),
				5m);

			var refreshBindingCount = 0;
			((IBusiness)orderedInventory).ListChanged += (s, e) => refreshBindingCount++;

			var processor = new AllocationResultProcessor();
			var processedResult = processor.ProcessResults(pick, new[] { result1, result2 });
			AssertNull(processedResult.Notification);
			Assert(processedResult.InventoryAllocated);
			AssertEquals("Should have allocated stock.", 5m, availableInventory1.PickLineQuantity);
			AssertEquals("Should have allocated stock.", 5m, availableInventory2.PickLineQuantity);
			AssertEquals("Should have allocated stock to order line 1.", 10m, orderLine1.PickLineQuantity);
			AssertEquals("Should have logged the allocation.",
				"5 Units allocated from Rule: TEST" + System.Environment.NewLine, availableInventory1.AllocationLog);
			AssertEquals("Should have logged the allocation.",
				"5 Units allocated from Rule: TEST" + System.Environment.NewLine, availableInventory2.AllocationLog);
			AssertNoWarnings(orderedInventory.QuantityShortInfo);
			AssertEquals("Should have validated ordered inventory once at the end.", 1, quantityValidationCount);
			AssertEquals("Should have validated ordered inventory once at the end.", 1, shortValidationCount);
			AssertEquals("Should have refreshed binding twice, once to clear notifications and once at the end.", 2, refreshBindingCount);
		}
	}
}
