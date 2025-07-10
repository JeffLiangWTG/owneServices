using System;
using System.Linq;
using Enterprise.Warehouse.Environment.CodeLists;
using Enterprise.Warehouse.Transactions.CodeLists;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	class WorkOrderStagingLocationHelperTest : WhsTestCaseWithFactory
	{
		#region TestConstructor_DoesNotAcceptNullWarehouse

		public void TestConstructor_DoesNotAcceptNullWarehouse()
		{
			AssertExceptionThrown<ArgumentNullException>(() => new WorkOrderStagingLocationHelper(null));
		}

		#endregion

		#region TestGetStagingLocationForWorkOrderLine_DoesNotAcceptNullWorkOrderLine

		public void TestGetStagingLocationForWorkOrderLine_DoesNotAcceptNullWorkOrderLine()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			AssertExceptionThrown<ArgumentNullException>(() =>
				WorkOrderStagingLocationHelper.GetStagingLocationForWorkOrderLine(data.Whs1, null));
			AssertExceptionThrown<ArgumentNullException>(() =>
				new WorkOrderStagingLocationHelper(data.Whs1).GetStagingLocationForWorkOrderLine(null));

			Helper.CreateProductBOM(data.Part2, data.Part1, 2m, "UNT");
			var workOrder = Helper.CreateWhsWorkOrder(data.Org1, data.Whs1);
			var workOrderLine = Helper.CreateWhsWorkOrderLine(workOrder, data.Part2, 1m);
			AssertNoExceptionThrown(() =>
				WorkOrderStagingLocationHelper.GetStagingLocationForWorkOrderLine(data.Whs1, workOrderLine));
			AssertNoExceptionThrown(() =>
				new WorkOrderStagingLocationHelper(data.Whs1).GetStagingLocationForWorkOrderLine(workOrderLine));
		}

		#endregion

		#region TestGetStagingLocationForWorkOrderLine_WorkOrderLineMustHaveSameWarehouse

		public void TestGetStagingLocationForWorkOrderLine_WorkOrderLineMustHaveSameWarehouse()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var warehouse2 = Helper.CreateWarehouse("WHS");

			Helper.CreateProductBOM(data.Part2, data.Part1, 2m, "UNT");
			var workOrder = Helper.CreateWhsWorkOrder(data.Org1, data.Whs1);
			var workOrderLine = Helper.CreateWhsWorkOrderLine(workOrder, data.Part2, 1m);

			AssertExceptionThrown(typeof(InvalidOperationException),
				"Work Order Line must have the same Warehouse as the one passed through the Constructor.",
				() => WorkOrderStagingLocationHelper.GetStagingLocationForWorkOrderLine(warehouse2, workOrderLine));
			AssertExceptionThrown(typeof(InvalidOperationException),
				"Work Order Line must have the same Warehouse as the one passed through the Constructor.",
				() => new WorkOrderStagingLocationHelper(warehouse2).GetStagingLocationForWorkOrderLine(workOrderLine));
		}

		#endregion

		#region TestGetStagingLocationForWorkOrderLine_GetsStagingLocationFromKit

		public void TestGetStagingLocationForWorkOrderLine_GetsStagingLocationFromKit()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var warehouse2 = Helper.CreateWarehouse("COL");
			Helper.CreateProductBOM(data.Part2, data.Part1, 2m, "UNT");

			var stagingArea1 = Helper.CreateArea(data.Whs1, "STAGING1");
			var stagingArea2 = Helper.CreateArea(data.Whs1, "STAGING2");
			var stagingAreaInOtherWarehouse = Helper.CreateArea(warehouse2, "STAGING");
			var stagingRow1 = Helper.CreateRowAndGenerateLocations(data.Whs1, "STAGING1");
			var stagingRow2 = Helper.CreateRowAndGenerateLocations(data.Whs1, "STAGING2");
			var stagingRowInOtherWarehouse = Helper.CreateRowAndGenerateLocations(warehouse2, "STAGING");
			var stagingLocation1 = stagingRow1.Locations.Single();
			var stagingLocation2 = stagingRow2.Locations.Single();
			var stagingLocationInOtherWarehouse = stagingRowInOtherWarehouse.Locations.Single();
			stagingLocation1.WLV_WA_PickingArea = stagingArea1.PK;
			stagingLocation2.WLV_WA_PickingArea = stagingArea2.PK;
			stagingLocationInOtherWarehouse.WLV_WA_PickingArea = stagingAreaInOtherWarehouse.PK;

			var paramsWithDifferentClient = WhsProduct.GetWhsProduct(data.Part2).ParamsByWhsAndClient.AddNew();
			paramsWithDifferentClient.W3_OH = Helper.CreateClient("BAR").PK;
			paramsWithDifferentClient.W3_WW = data.Whs1.PK;
			paramsWithDifferentClient.W3_WL_StagingLocationBOM = stagingLocation1.PK;

			var paramsWithDifferentWarehouse = WhsProduct.GetWhsProduct(data.Part2).ParamsByWhsAndClient.AddNew();
			paramsWithDifferentWarehouse.W3_OH = data.Org1.PK;
			paramsWithDifferentWarehouse.W3_WW = warehouse2.PK;
			paramsWithDifferentWarehouse.W3_WL_StagingLocationBOM = stagingLocationInOtherWarehouse.PK;

			var correctParams = WhsProduct.GetWhsProduct(data.Part2).ParamsByWhsAndClient.AddNew();
			correctParams.W3_OH = data.Org1.PK;
			correctParams.W3_WW = data.Whs1.PK;
			correctParams.W3_WL_StagingLocationBOM = stagingLocation2.PK;

			Factory.Save();

			var iprArea = Helper.CreateArea(data.Whs1, "IPR", AreaTypes.Codes.InwardProcessing);
			var stagingRowIPR = Helper.CreateRowAndGenerateLocations(data.Whs1, "STAGINGIPR");
			var stagingLocationIPR = stagingRowIPR.Locations.Single();
			stagingLocationIPR.WLV_WA_PutawayArea = iprArea.PK;
			stagingLocationIPR.WLV_WA_PickingArea = iprArea.PK;

			paramsWithDifferentClient.W3_WL_InwardsProcessingStagingLocationBOM = stagingLocationIPR.PK;
			paramsWithDifferentWarehouse.W3_WL_InwardsProcessingStagingLocationBOM = stagingLocationIPR.PK;
			correctParams.W3_WL_InwardsProcessingStagingLocationBOM = stagingLocationIPR.PK;

			var workOrder = Helper.CreateWhsWorkOrder(data.Org1, data.Whs1);
			var workOrderLine = Helper.CreateWhsWorkOrderLine(workOrder, data.Part2, 1m);
			AssertEquals(
				"Product had a Staging Location set, it should use the one with the same Warehouse and Client.",
				stagingLocation2.PK,
				new WorkOrderStagingLocationHelper(data.Whs1).GetStagingLocationForWorkOrderLine(workOrderLine));
		}

		public void TestGetStagingLocationForWorkOrderLine_GetsStagingLocationFromKit_InwardProcessing()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateProductBOM(data.Part2, data.Part1, 2m, "UNT");

			var stagingArea1 = Helper.CreateArea(data.Whs1, "STAGING1");
			var stagingRow1 = Helper.CreateRowAndGenerateLocations(data.Whs1, "STAGING1");
			var stagingLocation1 = stagingRow1.Locations.Single();
			stagingLocation1.WLV_WA_PickingArea = stagingArea1.PK;
			stagingLocation1.WLV_WA_PutawayArea = stagingArea1.PK;

			var correctParams = WhsProduct.GetWhsProduct(data.Part2).ParamsByWhsAndClient.AddNew();
			correctParams.W3_OH = data.Org1.PK;
			correctParams.W3_WW = data.Whs1.PK;
			correctParams.W3_WL_StagingLocationBOM = stagingLocation1.PK;

			Factory.Save();

			var stagingArea2 = Helper.CreateArea(data.Whs1, "STAGING2", AreaTypes.Codes.InwardProcessing);
			var stagingRow2 = Helper.CreateRowAndGenerateLocations(data.Whs1, "STAGING2");
			var stagingLocation2 = stagingRow2.Locations.Single();
			stagingLocation2.WLV_WA_PickingArea = stagingArea2.PK;
			stagingLocation2.WLV_WA_PutawayArea = stagingArea2.PK;
			correctParams.W3_WL_InwardsProcessingStagingLocationBOM = stagingLocation2.PK;

			var workOrder = Helper.CreateWhsWorkOrder(data.Org1, data.Whs1);
			workOrder.WD_IsInwardsProcessingJob = true;
			var workOrderLine = Helper.CreateWhsWorkOrderLine(workOrder, data.Part2, 1m);
			AssertEquals(
				"Product had a Staging Location set, it should use Inward Processing Staging Location.",
				stagingLocation2.PK,
				new WorkOrderStagingLocationHelper(data.Whs1).GetStagingLocationForWorkOrderLine(workOrderLine));
		}

		public void TestGetStagingLocationForWorkOrderLine_GetsStagingLocationFromKit_InwardProcessing_LocationAreaIsIncorrect()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateProductBOM(data.Part2, data.Part1, 2m, "UNT");

			var stagingArea = Helper.CreateArea(data.Whs1, "STAGING");
			var stagingRow = Helper.CreateRowAndGenerateLocations(data.Whs1, "STAGING");
			var stagingLocation = stagingRow.Locations.Single();
			stagingLocation.WLV_WA_PickingArea = stagingArea.PK;
			stagingLocation.WLV_WA_PutawayArea = stagingArea.PK;

			var correctParams = WhsProduct.GetWhsProduct(data.Part2).ParamsByWhsAndClient.AddNew();
			correctParams.W3_OH = data.Org1.PK;
			correctParams.W3_WW = data.Whs1.PK;
			correctParams.W3_WL_InwardsProcessingStagingLocationBOM = stagingLocation.PK;

			Factory.Save();

			var workOrder = Helper.CreateWhsWorkOrder(data.Org1, data.Whs1);
			workOrder.WD_IsInwardsProcessingJob = true;
			var workOrderLine = Helper.CreateWhsWorkOrderLine(workOrder, data.Part2, 1m);
			AssertEquals(
				"Staging Location was incorrect, return empty location.",
				Guid.Empty, new WorkOrderStagingLocationHelper(data.Whs1).GetStagingLocationForWorkOrderLine(workOrderLine));
		}

		public void TestGetStagingLocationForWorkOrderLine_GetsStagingLocationFromKit_LocationAreaIsBonded()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateProductBOM(data.Part2, data.Part1, 2m, "UNT");

			var stagingArea = Helper.CreateArea(data.Whs1, "STAGING", AreaTypes.Codes.Bonded);
			var stagingRow = Helper.CreateRowAndGenerateLocations(data.Whs1, "STAGING");
			var stagingLocation = stagingRow.Locations.Single();
			stagingLocation.WLV_WA_PickingArea = stagingArea.PK;
			stagingLocation.WLV_WA_PutawayArea = stagingArea.PK;

			var correctParams = WhsProduct.GetWhsProduct(data.Part2).ParamsByWhsAndClient.AddNew();
			correctParams.W3_OH = data.Org1.PK;
			correctParams.W3_WW = data.Whs1.PK;
			correctParams.W3_WL_StagingLocationBOM = stagingLocation.PK;

			Factory.Save();

			var workOrder = Helper.CreateWhsWorkOrder(data.Org1, data.Whs1);
			var workOrderLine = Helper.CreateWhsWorkOrderLine(workOrder, data.Part2, 1m);
			AssertEquals(
				"Staging Location was incorrect, return default non bonded location.",
				data.Whs1.DefaultLocationInNonBondedArea.PK,
				new WorkOrderStagingLocationHelper(data.Whs1).GetStagingLocationForWorkOrderLine(workOrderLine));
		}

		public void TestGetStagingLocationForWorkOrderLine_GetsStagingLocationFromKit_LocationAreaIsIPR()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateProductBOM(data.Part2, data.Part1, 2m, "UNT");

			var correctParams = WhsProduct.GetWhsProduct(data.Part2).ParamsByWhsAndClient.AddNew();
			correctParams.W3_OH = data.Org1.PK;
			correctParams.W3_WW = data.Whs1.PK;

			Factory.Save();

			var stagingArea = Helper.CreateArea(data.Whs1, "STAGING", AreaTypes.Codes.InwardProcessing);
			var stagingRow = Helper.CreateRowAndGenerateLocations(data.Whs1, "STAGING");
			var stagingLocation = stagingRow.Locations.Single();
			stagingLocation.WLV_WA_PickingArea = stagingArea.PK;
			stagingLocation.WLV_WA_PutawayArea = stagingArea.PK;
			correctParams.W3_WL_StagingLocationBOM = stagingLocation.PK;

			var workOrder = Helper.CreateWhsWorkOrder(data.Org1, data.Whs1);
			var workOrderLine = Helper.CreateWhsWorkOrderLine(workOrder, data.Part2, 1m);
			AssertEquals(
				"Staging Location was incorrect, return default non bonded location.",
				data.Whs1.DefaultLocationInNonBondedArea.PK,
				new WorkOrderStagingLocationHelper(data.Whs1).GetStagingLocationForWorkOrderLine(workOrderLine));
		}

		#endregion

		#region TestGetStagingLocationForWorkOrderLine_StagingAreaWithNoLocations

		public void TestGetStagingLocationForWorkOrderLine_StagingAreaWithNoLocations()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateProductBOM(data.Part2, data.Part1, 2m, "UNT");

			Helper.CreateArea(data.Whs1, "FOO");
			var paramsWithNoLocations = WhsProduct.GetWhsProduct(data.Part2).ParamsByWhsAndClient.AddNew();
			paramsWithNoLocations.W3_OH = data.Org1.PK;
			paramsWithNoLocations.W3_WW = data.Whs1.PK;

			Factory.Save();

			var workOrder = Helper.CreateWhsWorkOrder(data.Org1, data.Whs1);
			var workOrderLine = Helper.CreateWhsWorkOrderLine(workOrder, data.Part2, 1m);
			AssertEquals("Product had no staging location, Destination Location should be Default Warehouse Location.",
				data.Whs1.DefaultLocation.PK,
				new WorkOrderStagingLocationHelper(data.Whs1).GetStagingLocationForWorkOrderLine(workOrderLine));
		}

		#endregion

		#region TestGetStagingLocationForWorkOrderLine_StagingAreaWithNoLocations

		public void TestGetStagingLocationForWorkOrderLine_StagingAreaWithDockDoorLocation()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateProductBOM(data.Part2, data.Part1, 2m, "UNT");

			var areaWithNoLocations = Helper.CreateArea(data.Whs1, "FOO");
			var paramsWithNoLocations = WhsProduct.GetWhsProduct(data.Part2).ParamsByWhsAndClient.AddNew();
			paramsWithNoLocations.W3_OH = data.Org1.PK;
			paramsWithNoLocations.W3_WW = data.Whs1.PK;
			paramsWithNoLocations.W3_WL_StagingLocationBOM = data.Whs1.DefaultOutboundDockDoorLocation.PK;

			Factory.Save();

			var workOrder = Helper.CreateWhsWorkOrder(data.Org1, data.Whs1);
			var workOrderLine = Helper.CreateWhsWorkOrderLine(workOrder, data.Part2, 1m);
			AssertEquals(
				"Since Product had dock door as a staging location, Destination Location must be Default Warehouse Location.",
				data.Whs1.DefaultLocation.PK,
				new WorkOrderStagingLocationHelper(data.Whs1).GetStagingLocationForWorkOrderLine(workOrderLine));
		}

		#endregion

		#region TestGetStagingLocationForWorkOrderLine_KitProductWithNoStagingArea

		public void TestGetStagingLocationForWorkOrderLine_KitProductWithNoStagingArea()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateProductBOM(data.Part2, data.Part1, 2m, "UNT");

			var paramsWithNoArea = WhsProduct.GetWhsProduct(data.Part2).ParamsByWhsAndClient.AddNew();
			paramsWithNoArea.W3_OH = data.Org1.PK;
			paramsWithNoArea.W3_WW = data.Whs1.PK;

			Factory.Save();

			var workOrder = Helper.CreateWhsWorkOrder(data.Org1, data.Whs1);
			var workOrderLine = Helper.CreateWhsWorkOrderLine(workOrder, data.Part2, 1m);
			AssertEquals("Product had no Staging Area, Destination Location should be Default Warehouse Location.",
				data.Whs1.DefaultLocation.PK,
				new WorkOrderStagingLocationHelper(data.Whs1).GetStagingLocationForWorkOrderLine(workOrderLine));
		}

		public void TestGetStagingLocationForWorkOrderLine_KitProductWithNoStagingArea_InwardProcessing()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			Helper.CreateProductBOM(data.Part2, data.Part1, 2m, "UNT");

			var paramsWithNoArea = WhsProduct.GetWhsProduct(data.Part2).ParamsByWhsAndClient.AddNew();
			paramsWithNoArea.W3_OH = data.Org1.PK;
			paramsWithNoArea.W3_WW = data.Whs1.PK;

			Factory.Save();
			var area = Helper.CreateArea(data.Whs1, "INWARDS", AreaTypes.Codes.InwardProcessing);
			var location = data.Whs1.FindLocation("A-2");
			location.WLV_WA_PutawayArea = area.PK;
			location.WLV_WA_PickingArea = area.PK;
			AssertNotEquals("Precondition: Default Location & Inward Processing Location should be different.",
				data.Whs1.DefaultLocation.PK, data.Whs1.DefaultLocationInInwardProcessingArea.PK);

			var workOrder = Helper.CreateWhsWorkOrder(data.Org1, data.Whs1);
			workOrder.WD_IsInwardsProcessingJob = true;
			var workOrderLine = Helper.CreateWhsWorkOrderLine(workOrder, data.Part2, 1m);
			AssertEquals("Product had no Staging Area, Destination Location should be Default Warehouse Inward Processing Location.",
				data.Whs1.DefaultLocationInInwardProcessingArea.PK,
				new WorkOrderStagingLocationHelper(data.Whs1).GetStagingLocationForWorkOrderLine(workOrderLine));
		}

		#endregion

		#region TestGetStagingLocationForWorkOrderLine_KitProductWithNoProductParams

		public void TestGetStagingLocationForWorkOrderLine_KitProductWithNoProductParams()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			Helper.CreateProductBOM(data.Part2, data.Part1, 2m, "UNT");

			Factory.Save();
			var area = Helper.CreateArea(data.Whs1, "INWARDS", AreaTypes.Codes.InwardProcessing);
			var location = data.Whs1.FindLocation("A-2");
			location.WLV_WA_PutawayArea = area.PK;
			location.WLV_WA_PickingArea = area.PK;
			AssertNotEquals("Precondition: Default Location & Inward Processing Location should be different.",
				data.Whs1.DefaultLocation.PK, data.Whs1.DefaultLocationInInwardProcessingArea.PK);

			var workOrder = Helper.CreateWhsWorkOrder(data.Org1, data.Whs1);
			var workOrderLine = Helper.CreateWhsWorkOrderLine(workOrder, data.Part2, 1m);
			AssertEquals("Product had no Product Params, Destination Location should be Default Warehouse Location.",
				data.Whs1.DefaultLocation.PK,
				new WorkOrderStagingLocationHelper(data.Whs1).GetStagingLocationForWorkOrderLine(workOrderLine));
		}

		public void TestGetStagingLocationForWorkOrderLine_KitProductWithNoProductParams_InwardProcessing()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			Helper.CreateProductBOM(data.Part2, data.Part1, 2m, "UNT");

			Factory.Save();
			var area = Helper.CreateArea(data.Whs1, "INWARDS", AreaTypes.Codes.InwardProcessing);
			var location = data.Whs1.FindLocation("A-2");
			location.WLV_WA_PutawayArea = area.PK;
			location.WLV_WA_PickingArea = area.PK;
			AssertNotEquals("Precondition: Default Location & Inward Processing Location should be different.",
				data.Whs1.DefaultLocation.PK, data.Whs1.DefaultLocationInInwardProcessingArea.PK);

			var workOrder = Helper.CreateWhsWorkOrder(data.Org1, data.Whs1);
			workOrder.WD_IsInwardsProcessingJob = true;
			var workOrderLine = Helper.CreateWhsWorkOrderLine(workOrder, data.Part2, 1m);
			AssertEquals("Product had no Product Params, Destination Location should be Default Warehouse Inward Processing Location.",
				data.Whs1.DefaultLocationInInwardProcessingArea.PK,
				new WorkOrderStagingLocationHelper(data.Whs1).GetStagingLocationForWorkOrderLine(workOrderLine));
		}

		#endregion

		#region TestGetStagingLocationForWorkOrderLine_Disassembly

		public void TestGetStagingLocationForWorkOrderLine_Disassembly()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateProductBOM(data.Part2, data.Part1, 2m, "UNT");

			var stagingArea1 = Helper.CreateArea(data.Whs1, "STAGING1");
			var stagingArea2 = Helper.CreateArea(data.Whs1, "STAGING2");
			var stagingRow1 = Helper.CreateRowAndGenerateLocations(data.Whs1, "STAGING1");
			var stagingRow2 = Helper.CreateRowAndGenerateLocations(data.Whs1, "STAGING2");
			var stagingLocation1 = stagingRow1.Locations.Single();
			var stagingLocation2 = stagingRow2.Locations.Single();
			stagingLocation1.WLV_WA_PickingArea = stagingArea1.PK;
			stagingLocation2.WLV_WA_PickingArea = stagingArea2.PK;

			var paramsOnPart1 = WhsProduct.GetWhsProduct(data.Part1).ParamsByWhsAndClient.AddNew();
			paramsOnPart1.W3_OH = data.Org1.PK;
			paramsOnPart1.W3_WW = data.Whs1.PK;
			paramsOnPart1.W3_WL_StagingLocationBOM = stagingLocation1.PK;

			var paramsOnPart2 = WhsProduct.GetWhsProduct(data.Part2).ParamsByWhsAndClient.AddNew();
			paramsOnPart2.W3_OH = data.Org1.PK;
			paramsOnPart2.W3_WW = data.Whs1.PK;
			paramsOnPart2.W3_WL_StagingLocationBOM = stagingLocation2.PK;

			Factory.Save();

			var workOrder = Helper.CreateWhsWorkOrder(data.Org1, data.Whs1);
			workOrder.WD_DocketSubType = WorkOrderType.Codes.Disassemble;
			var parentLine = Helper.CreateWhsWorkOrderLine(workOrder, data.Part2, 2m);
			var workOrderLine = (WhsWorkOrderLine)parentLine.ChildComponentLines.Single();
			AssertEquals("It should use the Kit Staging Location.", stagingLocation2.PK,
				new WorkOrderStagingLocationHelper(data.Whs1).GetStagingLocationForWorkOrderLine(workOrderLine));
		}

		#endregion
	}
}
