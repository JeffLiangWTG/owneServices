using System;
using CargoWise.Types;
using Enterprise.Warehouse.Environment.CodeLists;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	public class WhsDynamicWorkOrderValidationCacheTest : WhsTestCaseWithFactory
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentNullException>(() => new WhsDynamicWorkOrderValidationCache(null));
		}

		#region TestDoesDynamicWorkOrderCache_ProcessedItems_NoLines

		public void TestDoesDynamicWorkOrderHaveNoMainProcessedItems_NoLines()
		{
			TestCacheCore_NoLines(expectedResult: true, (cache) => cache.DoesDynamicWorkOrderHaveNoMainProcessedItems());
		}

		public void TestDoesDynamicWorkOrderHaveMoreThanOneMainProcessedItem_NoLines()
		{
			TestCacheCore_NoLines(expectedResult: false, (cache) => cache.DoesDynamicWorkOrderHaveMoreThanOneMainProcessedItem());
		}

		void TestCacheCore_NoLines(bool expectedResult, Func<WhsDynamicWorkOrderValidationCache, bool> runCode)
		{
			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WW_IsVirtualWarehouse = true;
			Helper.CreateArea(data.Whs1, "IPR", AreaTypes.Codes.InwardProcessing);
			Factory.Save();

			var workOrder = Helper.CreateWhsDynamicWorkOrder(data.Org1, data.Whs1);
			var cache = new WhsDynamicWorkOrderValidationCache(workOrder.AllLines);
			AssertEquals("Should return correct value.", expectedResult, runCode(cache));
		}

		#endregion

		#region TestDoesDynamicWorkOrderCache_ProcessedItems_NoMains

		public void TestDoesDynamicWorkOrderHaveNoMainProcessedItems_NoMains()
		{
			TestCacheCore_NoMains(expectedResult: true, (cache) => cache.DoesDynamicWorkOrderHaveNoMainProcessedItems());
		}

		public void TestDoesDynamicWorkOrderHaveMoreThanOneMainProcessedItem_NoMains()
		{
			TestCacheCore_NoMains(expectedResult: false, (cache) => cache.DoesDynamicWorkOrderHaveMoreThanOneMainProcessedItem());
		}

		void TestCacheCore_NoMains(bool expectedResult, Func<WhsDynamicWorkOrderValidationCache, bool> runCode)
		{
			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WW_IsVirtualWarehouse = true;
			Helper.CreateArea(data.Whs1, "IPR", AreaTypes.Codes.InwardProcessing);
			Factory.Save();

			var workOrder = Helper.CreateWhsDynamicWorkOrder(data.Org1, data.Whs1);
			var line = Helper.CreateWhsDynamicWorkOrderLine(workOrder, data.Part2, 10m);
			line.IsSecondaryInwardProcessedItem = true;
			var cache = new WhsDynamicWorkOrderValidationCache(workOrder.AllLines);
			AssertEquals("Should return correct value.", expectedResult, runCode(cache));
		}

		#endregion

		#region TestDoesDynamicWorkOrderCache_ProcessedItems_TwoMains

		public void TestDoesDynamicWorkOrderHaveNoMainProcessedItems_TwoMains()
		{
			TestCacheCore_TwoMains(expectedResult: false, (cache) => cache.DoesDynamicWorkOrderHaveNoMainProcessedItems());
		}

		public void TestDoesDynamicWorkOrderHaveMoreThanOneMainProcessedItem_TwoMains()
		{
			TestCacheCore_TwoMains(expectedResult: true, (cache) => cache.DoesDynamicWorkOrderHaveMoreThanOneMainProcessedItem());
		}

		void TestCacheCore_TwoMains(bool expectedResult, Func<WhsDynamicWorkOrderValidationCache, bool> runCode)
		{
			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WW_IsVirtualWarehouse = true;
			Helper.CreateArea(data.Whs1, "IPR", AreaTypes.Codes.InwardProcessing);
			Factory.Save();

			var workOrder = Helper.CreateWhsDynamicWorkOrder(data.Org1, data.Whs1);
			var line1 = Helper.CreateWhsDynamicWorkOrderLine(workOrder, data.Part1, 1m);
			line1.IsMainInwardProcessedItem = true;
			var line2 = Helper.CreateWhsDynamicWorkOrderLine(workOrder, data.Part2, 2m);
			line2.IsMainInwardProcessedItem = true;
			var cache = new WhsDynamicWorkOrderValidationCache(workOrder.AllLines);
			AssertEquals("Should return correct value.", expectedResult, runCode(cache));
		}

		#endregion

		#region TestDoesDynamicWorkOrderCache_ProcessedItems_OneMain

		public void TestDoesDynamicWorkOrderHaveNoMainProcessedItems_OneMain()
		{
			TestCacheCore_OneMain(expectedResult: false, (cache) => cache.DoesDynamicWorkOrderHaveNoMainProcessedItems());
		}

		public void TestDoesDynamicWorkOrderHaveMoreThanOneMainProcessedItem_OneMain()
		{
			TestCacheCore_OneMain(expectedResult: false, (cache) => cache.DoesDynamicWorkOrderHaveMoreThanOneMainProcessedItem());
		}

		void TestCacheCore_OneMain(bool expectedResult, Func<WhsDynamicWorkOrderValidationCache, bool> runCode)
		{
			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WW_IsVirtualWarehouse = true;
			Helper.CreateArea(data.Whs1, "IPR", AreaTypes.Codes.InwardProcessing);
			Factory.Save();

			var workOrder = Helper.CreateWhsDynamicWorkOrder(data.Org1, data.Whs1);
			var line1 = Helper.CreateWhsDynamicWorkOrderLine(workOrder, data.Part1, 1m);
			line1.IsMainInwardProcessedItem = true;
			var line2 = Helper.CreateWhsDynamicWorkOrderLine(workOrder, data.Part2, 2m);
			line2.IsSecondaryInwardProcessedItem = true;
			var cache = new WhsDynamicWorkOrderValidationCache(workOrder.AllLines);
			AssertEquals("Should return correct value.", expectedResult, runCode(cache));
		}

		#endregion

		#region TestIsSecondaryComponentSumMoreThanMainComponent

		public void TestIsSecondaryComponentSumMoreThanMainComponent()
		{
			var data = new TestDataSimpleEnvironment(Factory, 20, 1);
			var mainPart = Helper.CreateProduct("MAIN", data.Org1);
			Factory.Save();

			var dynamicWorkOrder = Helper.CreateWhsDynamicWorkOrder(data.Org1, data.Whs1);

			// Main
			var kitLine = Helper.CreateWhsDynamicWorkOrderLine(dynamicWorkOrder, mainPart, 5m);
			kitLine.IsMainInwardProcessedItem = true;

			var componentLine = Helper.CreateWhsDynamicWorkOrderLine(dynamicWorkOrder, data.Part2, 10m);
			componentLine.WE_WE_ParentDocketLine = kitLine.PK;

			// Secondary
			var secondaryLine = Helper.CreateWhsDynamicWorkOrderLine(dynamicWorkOrder, data.Part1, 2m);
			secondaryLine.IsSecondaryInwardProcessedItem = true;

			var componentLine3 = Helper.CreateWhsDynamicWorkOrderLine(dynamicWorkOrder, data.Part2, 5m);
			componentLine3.WE_WE_ParentDocketLine = secondaryLine.PK;

			var cache = new WhsDynamicWorkOrderValidationCache(dynamicWorkOrder.AllLines);
			AssertEquals("10 on Main, 5 on Secondary = false.", false, cache.IsSecondaryComponentSumMoreThanMainComponent(data.Part2.PK));
		}

		public void TestIsSecondaryComponentSumMoreThanMainComponent_SameOnSecondary()
		{
			var data = new TestDataSimpleEnvironment(Factory, 20, 1);
			var mainPart = Helper.CreateProduct("MAIN", data.Org1);
			Factory.Save();

			var dynamicWorkOrder = Helper.CreateWhsDynamicWorkOrder(data.Org1, data.Whs1);

			// Main
			var kitLine = Helper.CreateWhsDynamicWorkOrderLine(dynamicWorkOrder, mainPart, 5m);
			kitLine.IsMainInwardProcessedItem = true;

			var componentLine = Helper.CreateWhsDynamicWorkOrderLine(dynamicWorkOrder, data.Part2, 10m);
			componentLine.WE_WE_ParentDocketLine = kitLine.PK;

			// Secondary
			var secondaryLine = Helper.CreateWhsDynamicWorkOrderLine(dynamicWorkOrder, data.Part1, 2m);
			secondaryLine.IsSecondaryInwardProcessedItem = true;

			var componentLine3 = Helper.CreateWhsDynamicWorkOrderLine(dynamicWorkOrder, data.Part2, 10m);
			componentLine3.WE_WE_ParentDocketLine = secondaryLine.PK;

			var cache = new WhsDynamicWorkOrderValidationCache(dynamicWorkOrder.AllLines);
			AssertEquals("10 on Main, 10 on Secondary = false.", false, cache.IsSecondaryComponentSumMoreThanMainComponent(data.Part2.PK));
		}

		public void TestIsSecondaryComponentSumMoreThanMainComponent_NoSecondary()
		{
			var data = new TestDataSimpleEnvironment(Factory, 20, 1);
			Factory.Save();

			var dynamicWorkOrder = Helper.CreateWhsDynamicWorkOrder(data.Org1, data.Whs1);
			var kitLine = Helper.CreateWhsDynamicWorkOrderLine(dynamicWorkOrder, data.Part1, 5m);
			kitLine.IsMainInwardProcessedItem = true;

			var componentLine = Helper.CreateWhsDynamicWorkOrderLine(dynamicWorkOrder, data.Part2, 10m);
			componentLine.WE_WE_ParentDocketLine = kitLine.PK;

			var cache = new WhsDynamicWorkOrderValidationCache(dynamicWorkOrder.AllLines);
			AssertEquals("10 on Main, 0 on Secondary = false.", false, cache.IsSecondaryComponentSumMoreThanMainComponent(data.Part2.PK));
		}

		public void TestIsSecondaryComponentSumMoreThanMainComponent_TwoComponents()
		{
			var data = new TestDataSimpleEnvironment(Factory, 20, 1);
			var mainPart = Helper.CreateProduct("MAIN", data.Org1);
			var otherPart = Helper.CreateProduct("Other", data.Org1);
			Factory.Save();

			var dynamicWorkOrder = Helper.CreateWhsDynamicWorkOrder(data.Org1, data.Whs1);

			// Main
			var kitLine = Helper.CreateWhsDynamicWorkOrderLine(dynamicWorkOrder, mainPart, 5m);
			kitLine.IsMainInwardProcessedItem = true;

			var componentLine1 = Helper.CreateWhsDynamicWorkOrderLine(dynamicWorkOrder, data.Part2, 10m);
			componentLine1.WE_WE_ParentDocketLine = kitLine.PK;

			var componentLine2 = Helper.CreateWhsDynamicWorkOrderLine(dynamicWorkOrder, otherPart, 7m);
			componentLine2.WE_WE_ParentDocketLine = kitLine.PK;

			// Secondary
			var secondaryLine = Helper.CreateWhsDynamicWorkOrderLine(dynamicWorkOrder, data.Part1, 2m);
			secondaryLine.IsSecondaryInwardProcessedItem = true;

			var componentLine3 = Helper.CreateWhsDynamicWorkOrderLine(dynamicWorkOrder, data.Part2, 5m);
			componentLine3.WE_WE_ParentDocketLine = secondaryLine.PK;

			var componentLine4 = Helper.CreateWhsDynamicWorkOrderLine(dynamicWorkOrder, otherPart, 5m);
			componentLine4.WE_WE_ParentDocketLine = secondaryLine.PK;

			var cache = new WhsDynamicWorkOrderValidationCache(dynamicWorkOrder.AllLines);
			AssertEquals(
				"10 Part2 on Main, 5 Part2 on Secondary = false.",
				false,
				cache.IsSecondaryComponentSumMoreThanMainComponent(data.Part2.PK));
			AssertEquals(
				" 7 OtherPart on Main, 5 OtherPart on Secondary = false.",
				false,
				cache.IsSecondaryComponentSumMoreThanMainComponent(otherPart.PK));
		}

		public void TestIsSecondaryComponentSumMoreThanMainComponent_ComponentPerSecondary()
		{
			var data = new TestDataSimpleEnvironment(Factory, 20, 1);
			var mainPart = Helper.CreateProduct("MAIN", data.Org1);
			var otherPart = Helper.CreateProduct("Other", data.Org1);
			var dancingPart = Helper.CreateProduct("dancing", data.Org1);
			Factory.Save();

			var dynamicWorkOrder = Helper.CreateWhsDynamicWorkOrder(data.Org1, data.Whs1);

			// Main
			var kitLine = Helper.CreateWhsDynamicWorkOrderLine(dynamicWorkOrder, mainPart, 5m);
			kitLine.IsMainInwardProcessedItem = true;

			var componentLine1 = Helper.CreateWhsDynamicWorkOrderLine(dynamicWorkOrder, data.Part2, 10m);
			componentLine1.WE_WE_ParentDocketLine = kitLine.PK;

			var componentLine2 = Helper.CreateWhsDynamicWorkOrderLine(dynamicWorkOrder, otherPart, 7m);
			componentLine2.WE_WE_ParentDocketLine = kitLine.PK;

			// Secondary
			var secondaryLine1 = Helper.CreateWhsDynamicWorkOrderLine(dynamicWorkOrder, data.Part1, 2m);
			secondaryLine1.IsSecondaryInwardProcessedItem = true;

			var componentLine3 = Helper.CreateWhsDynamicWorkOrderLine(dynamicWorkOrder, data.Part2, 5m);
			componentLine3.WE_WE_ParentDocketLine = secondaryLine1.PK;

			var secondaryLine2 = Helper.CreateWhsDynamicWorkOrderLine(dynamicWorkOrder, dancingPart, 2m);
			secondaryLine2.IsSecondaryInwardProcessedItem = true;

			var componentLine4 = Helper.CreateWhsDynamicWorkOrderLine(dynamicWorkOrder, otherPart, 5m);
			componentLine4.WE_WE_ParentDocketLine = secondaryLine2.PK;

			var cache = new WhsDynamicWorkOrderValidationCache(dynamicWorkOrder.AllLines);
			AssertEquals(
				"10 Part2 on Main, 5 Part2 on Secondary1 = false.",
				false,
				cache.IsSecondaryComponentSumMoreThanMainComponent(data.Part2.PK));
			AssertEquals(
				" 7 OtherPart on Main, 5 OtherPart on Secondary2 = false.",
				false,
				cache.IsSecondaryComponentSumMoreThanMainComponent(otherPart.PK));
		}

		public void TestIsSecondaryComponentSumMoreThanMainComponent_MoreOnSecondary_OnlyOnSecondary()
		{
			var data = new TestDataSimpleEnvironment(Factory, 20, 1);
			var mainPart = Helper.CreateProduct("MAIN", data.Org1);
			Factory.Save();

			var dynamicWorkOrder = Helper.CreateWhsDynamicWorkOrder(data.Org1, data.Whs1);
			var kitLine = Helper.CreateWhsDynamicWorkOrderLine(dynamicWorkOrder, mainPart, 5m);
			kitLine.IsMainInwardProcessedItem = true;

			var secondaryLine = Helper.CreateWhsDynamicWorkOrderLine(dynamicWorkOrder, data.Part1, 2m);
			secondaryLine.IsSecondaryInwardProcessedItem = true;

			var componentLine = Helper.CreateWhsDynamicWorkOrderLine(dynamicWorkOrder, data.Part2, 10m);
			componentLine.WE_WE_ParentDocketLine = secondaryLine.PK;

			var cache = new WhsDynamicWorkOrderValidationCache(dynamicWorkOrder.AllLines);
			AssertEquals("0 on Main, 10 on Secondary = true.", true, cache.IsSecondaryComponentSumMoreThanMainComponent(data.Part2.PK));
		}

		public void TestIsSecondaryComponentSumMoreThanMainComponent_MoreOnSecondary_ExtraComponentSecondary()
		{
			var data = new TestDataSimpleEnvironment(Factory, 20, 1);
			var mainPart = Helper.CreateProduct("MAIN", data.Org1);
			var otherPart = Helper.CreateProduct("Other", data.Org1);
			Factory.Save();

			var dynamicWorkOrder = Helper.CreateWhsDynamicWorkOrder(data.Org1, data.Whs1);
			var kitLine = Helper.CreateWhsDynamicWorkOrderLine(dynamicWorkOrder, mainPart, 5m);
			kitLine.IsMainInwardProcessedItem = true;

			var componentLine1 = Helper.CreateWhsDynamicWorkOrderLine(dynamicWorkOrder, data.Part2, 10m);
			componentLine1.WE_WE_ParentDocketLine = kitLine.PK;

			var secondaryLine = Helper.CreateWhsDynamicWorkOrderLine(dynamicWorkOrder, data.Part1, 2m);
			secondaryLine.IsSecondaryInwardProcessedItem = true;

			var componentLine2 = Helper.CreateWhsDynamicWorkOrderLine(dynamicWorkOrder, data.Part2, 5m);
			componentLine2.WE_WE_ParentDocketLine = secondaryLine.PK;

			var componentLine3 = Helper.CreateWhsDynamicWorkOrderLine(dynamicWorkOrder, otherPart, 5m);
			componentLine3.WE_WE_ParentDocketLine = secondaryLine.PK;

			var cache = new WhsDynamicWorkOrderValidationCache(dynamicWorkOrder.AllLines);
			AssertEquals(
				"10 Part2 on Main, 5 Part2 on Secondary = false.",
				false,
				cache.IsSecondaryComponentSumMoreThanMainComponent(data.Part2.PK));
			AssertEquals(
				"0 OtherPart on Main, 5 OtherPart on Secondary = true.",
				true,
				cache.IsSecondaryComponentSumMoreThanMainComponent(otherPart.PK));
		}

		public void TestIsSecondaryComponentSumMoreThanMainComponent_ProductNotOnOrder()
		{
			var data = new TestDataSimpleEnvironment(Factory, 20, 1);
			var mainPart = Helper.CreateProduct("MAIN", data.Org1);
			Factory.Save();

			var dynamicWorkOrder = Helper.CreateWhsDynamicWorkOrder(data.Org1, data.Whs1);
			var kitLine = Helper.CreateWhsDynamicWorkOrderLine(dynamicWorkOrder, data.Part1, 5m);
			kitLine.IsMainInwardProcessedItem = true;

			var componentLine1 = Helper.CreateWhsDynamicWorkOrderLine(dynamicWorkOrder, data.Part2, 10m);
			componentLine1.WE_WE_ParentDocketLine = kitLine.PK;

			var cache = new WhsDynamicWorkOrderValidationCache(dynamicWorkOrder.AllLines);
			AssertEquals(
				"0 mainPart on Main, 0 mainPart on Secondary = false.",
				false,
				cache.IsSecondaryComponentSumMoreThanMainComponent(mainPart.PK));
		}

		public void TestIsSecondaryComponentSumMoreThanMainComponent_NoLines()
		{
			var data = new TestDataSimpleEnvironment(Factory, 20, 1);
			Factory.Save();

			var dynamicWorkOrder = Helper.CreateWhsDynamicWorkOrder(data.Org1, data.Whs1);
			var cache = new WhsDynamicWorkOrderValidationCache(dynamicWorkOrder.AllLines);
			AssertEquals(
				"No Lines = false.",
				false,
				cache.IsSecondaryComponentSumMoreThanMainComponent(data.Part1.PK));
			AssertEquals(
				"No Lines = false.",
				false,
				cache.IsSecondaryComponentSumMoreThanMainComponent(data.Part2.PK));
		}

		public void TestIsSecondaryComponentSumMoreThanMainComponent_ComponentPK_Invalid()
		{
			TestIsSecondaryComponentSumMoreThanMainComponent_ComponentPKCore(ZGuid.Invalid);
		}

		public void TestIsSecondaryComponentSumMoreThanMainComponent_ComponentPK_Empty()
		{
			TestIsSecondaryComponentSumMoreThanMainComponent_ComponentPKCore(ZGuid.Empty);
		}

		void TestIsSecondaryComponentSumMoreThanMainComponent_ComponentPKCore(ZGuid pk)
		{
			var data = new TestDataSimpleEnvironment(Factory, 20, 1);
			Factory.Save();

			var dynamicWorkOrder = Helper.CreateWhsDynamicWorkOrder(data.Org1, data.Whs1);
			var kitLine = Helper.CreateWhsDynamicWorkOrderLine(dynamicWorkOrder, data.Part1, 5m);
			kitLine.IsMainInwardProcessedItem = true;

			var componentLine1 = Helper.CreateWhsDynamicWorkOrderLine(dynamicWorkOrder, data.Part2, 10m);
			componentLine1.WE_WE_ParentDocketLine = kitLine.PK;

			var cache = new WhsDynamicWorkOrderValidationCache(dynamicWorkOrder.AllLines);
			AssertEquals(
				"Invalid/Empty componentPK = false",
				false,
				cache.IsSecondaryComponentSumMoreThanMainComponent(pk));
		}

		#endregion
	}
}
