using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Environment.CodeLists;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	class WhsWorkOrderValidationTest : WhsComponentOrderValidationTest<WhsWorkOrder>
	{
		#region TestWD_DocketSubType_DisassemblyInwardProcessing

		public void TestWD_DocketSubType_DisassemblyInwardProcessing()
		{
			var warehouse = Factory.New<WhsWarehouse>();
			warehouse.WW_IsVirtualWarehouse = false;
			var iprArea = Helper.CreateArea(warehouse, "IPR", AreaTypes.Codes.InwardProcessing);

			var workOrder = Factory.New<WhsWorkOrder>();
			workOrder.WD_WW_Whs = warehouse.PK;
			workOrder.WD_IsInwardsProcessingJob = true;

			workOrder.WD_DocketSubType = WorkOrderType.Codes.Disassemble;
			AssertNoErrors(workOrder.WD_DocketSubTypeInfo);

			workOrder.WD_DocketSubType = WorkOrderType.Codes.Assemble;
			AssertNoErrors(workOrder.WD_DocketSubTypeInfo);
		}

		#endregion

		#region TestValidateWD_AutoFinaliseBOMIntoInventory

		public void TestValidateWD_AutoFinaliseBOMIntoInventory()
		{
			var data = new TestDataForBOM(Factory);
			data.CreateBOMProducts();

			var workOrder = GetNewBusinessObject();
			workOrder.WD_OH_Client = data.Org1.PK;
			workOrder.WD_WW_Whs = data.Whs1.PK;
			var line = Helper.CreateWhsWorkOrderLine(workOrder, data.BOM.Bike, 5m);

			workOrder.WD_AutoFinaliseBOMIntoInventory = true;
			AssertHasError(workOrder.WD_AutoFinaliseBOMIntoInventoryInfo, "Auto-Finalizing into Inventory can only be enabled if all Lines have a BOM Staging Location.");

			workOrder.WD_AutoFinaliseBOMIntoInventory = false;
			AssertNoErrors(workOrder.WD_AutoFinaliseBOMIntoInventoryInfo);

			var productParams = line.Product.ParamsByWhsAndClient.AddNew();
			productParams.W3_WL_StagingLocationBOM = data.Whs1.Rows.AddNew().Locations.AddNew().PK;
			productParams.W3_WW = data.Whs1.PK;

			workOrder.WD_AutoFinaliseBOMIntoInventory = true;
			AssertNoErrors(workOrder.WD_AutoFinaliseBOMIntoInventoryInfo);
		}

		public void TestValidateWD_AutoFinaliseBOMIntoInventory_VirtualWarehouse()
		{
			var data = new TestDataForBOM(Factory);
			data.CreateBOMProducts();
			data.Whs1.WW_IsVirtualWarehouse = false;

			var whs2 = Helper.CreateWarehouse("WH2");
			whs2.WW_IsVirtualWarehouse = true;

			var workOrder = GetNewBusinessObject();
			workOrder.WD_OH_Client = data.Org1.PK;
			workOrder.WD_WW_Whs = data.Whs1.PK;
			var line = Helper.CreateWhsWorkOrderLine(workOrder, data.BOM.Bike, 5m);

			workOrder.WD_AutoFinaliseBOMIntoInventory = true;
			AssertHasError(workOrder.WD_AutoFinaliseBOMIntoInventoryInfo, "Auto-Finalizing into Inventory can only be enabled if all Lines have a BOM Staging Location.");

			workOrder.WD_WW_Whs = whs2.PK;
			AssertNoErrors(workOrder.WD_AutoFinaliseBOMIntoInventoryInfo);

			workOrder.WD_WW_Whs = data.Whs1.PK;
			AssertHasError(workOrder.WD_AutoFinaliseBOMIntoInventoryInfo, "Auto-Finalizing into Inventory can only be enabled if all Lines have a BOM Staging Location.");

			var productParams = line.Product.ParamsByWhsAndClient.AddNew();
			productParams.W3_WL_StagingLocationBOM = data.Whs1.Rows.AddNew().Locations.AddNew().PK;
			productParams.W3_WW = data.Whs1.PK;

			workOrder.WD_AutoFinaliseBOMIntoInventory = true;
			AssertNoErrors(workOrder.WD_AutoFinaliseBOMIntoInventoryInfo);
		}

		#endregion

		#region TestValidateWD_TotalCubic and TestValidateWD_TotalWeight

		#region SetUpProductForTotal

		protected override void SetUpProductForTotal(TestDataSimpleEnvironment data, SchemaDecimalColumn partWeightOrVolumeColumn, SchemaStringColumn partWeightOrVolumeUQColumn, ZString docketUQ)
		{
			var org = Helper.CreateClient();
			var mainPart = data.Part1;
			var subPart1 = Helper.CreateProduct(org, "SubPart1");
			var subPart2 = Helper.CreateProduct(org, "SubPart2");
			Helper.SetProductWeightAndVolume(mainPart, 5, "KG", 5, "M3");
			Helper.SetProductWeightAndVolume(subPart1, 3, "KG", 4, "M3");
			Helper.SetProductWeightAndVolume(subPart2, 2, "KG", 3, "M3");
			Helper.CreateProductBOM(mainPart, subPart1, 2m, Constants.PkgUnit.Unit);
			Helper.CreateProductBOM(mainPart, subPart2, 1m, Constants.PkgUnit.Unit);

			data.Part1[partWeightOrVolumeColumn] = 5m;
			data.Part1[partWeightOrVolumeUQColumn] = docketUQ;
			subPart1[partWeightOrVolumeUQColumn] = docketUQ;
			subPart2[partWeightOrVolumeUQColumn] = docketUQ;
		}

		#endregion

		#region AssertTotalInfo

		protected override void AssertTotalInfo(WhsDocket whsDocket, ZString expectedWarningPrefix, SchemaDecimalColumn docketTotalColumn, ZPropertyInfo info)
		{
			if (docketTotalColumn == WhsDocketSchema.WD_TotalCubic)
			{
				AssertTotalInfo(whsDocket, expectedWarningPrefix, 110m, docketTotalColumn, info);
			}
			else if (docketTotalColumn == WhsDocketSchema.WD_TotalWeight)
			{
				AssertTotalInfo(whsDocket, expectedWarningPrefix, 80m, docketTotalColumn, info);
			}
		}

		#region AssertTotalCubicInfo

		void AssertTotalInfo(WhsDocket whsDocket, ZString expectedWarningPrefix, ZDecimal expectedNumber, SchemaDecimalColumn docketTotalColumn, ZPropertyInfo info)
		{
			AssertEquals("Precondition: Weight/Volume of a docket should be populated from lines.", expectedNumber, whsDocket[docketTotalColumn]);
			AssertHasWarningContaining(info, expectedWarningPrefix);

			whsDocket[docketTotalColumn] = 70m;
			AssertHasWarningContaining(info, expectedWarningPrefix);

			whsDocket[docketTotalColumn] = -10m;
			AssertHasErrors(info);
			AssertNoWarning(info, expectedWarningPrefix);

			whsDocket[docketTotalColumn] = 50m;
			AssertNoErrors(info);
			AssertNoWarning(info, expectedWarningPrefix);
		}

		#endregion

		#endregion

		#endregion

		#region TestValidateWD_TotalUnits

		public void TestTotalUnitsValidation_Assembly()
		{
			TestTotalUnitsValidation_AssemblyCore(isTotalUnitsValidationEnabled: false);
		}

		public void TestTotalUnitsValidation_Assembly_WithSecondaryProductsAndOverpickedComponents()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var mainProduct = Helper.CreateProduct("MAIN", data.Org1);
			var componentProduct1 = Helper.CreateProduct("SUB1", data.Org1);
			Helper.CreateProductBOM(mainProduct, componentProduct1, 1m, Constants.PkgUnit.Unit);
			var componentProduct2 = Helper.CreateProduct("SUB2", data.Org1);
			Helper.CreateProductBOM(mainProduct, componentProduct2, 2m, Constants.PkgUnit.Unit);
			Helper.EnableWarehouseForBond(data.Whs1, true);
			data.Whs1.WW_IsVirtualWarehouse = true;
			var secondaryPart1 = Helper.CreateProduct("SP3", data.Org1);
			var secondaryPart2 = Helper.CreateProduct("SP4", data.Org1);
			Helper.CreateSecondaryProduct(mainProduct, secondaryPart1, 3m);
			Helper.CreateSecondaryProduct(mainProduct, secondaryPart2, 4m);

			var area = Helper.CreateArea(data.Whs1, "IPR", AreaTypes.Codes.InwardProcessing);
			var row = Helper.CreateRowAndGenerateLocations(data.Whs1, "I");
			var location = row.Locations.Single();
			location.WLV_WA_PutawayArea = area.PK;
			location.WLV_WA_PickingArea = area.PK;
			Factory.Save();

			var componentReceive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveInventoryLine(componentReceive, componentProduct1, 10m, "BEK-1");
			Helper.CreateWhsReceiveInventoryLine(componentReceive, componentProduct2, 20m, "BEK-2");
			componentReceive.WD_DocketSubType = ReceiveType.Codes.Customs;
			componentReceive.WD_IsInwardsProcessingJob = true;
			componentReceive.Lines[0].WE_WL = location.PK;
			componentReceive.Lines[1].WE_WL = location.PK;
			componentReceive.FinaliseDocket();
			AssertIsFinalisedPrecondition(componentReceive);
			Factory.Save();

			var workOrder = Helper.CreateWhsWorkOrderWithLine(data.Org1, data.Whs1, "ExtRef", mainProduct, 10m);
			workOrder.WD_DocketSubType = WorkOrderType.Codes.Assemble;
			workOrder.WD_RequiredDate = ZDateTimeOffset.Now;
			workOrder.WD_IsInwardsProcessingJob = true;

			var productParam = workOrder.Lines[0].Product.ParamsByWhsAndClient.AddNew();
			productParam.W3_OH = data.Org1.PK;
			productParam.W3_WW = data.Whs1.PK;
			productParam.W3_WL_InwardsProcessingStagingLocationBOM = location.PK;

			var pick = Helper.CreatePickNew(workOrder);
			var pickLines = pick.GetAllPickLines();
			var pickLine = pickLines.Single(l => l.ProductCode == "SUB1");
			pickLine.WZ_Units = 8m;
			Factory.Save();

			using (WarehouseDataRegistry.Instance.TotalUnitsValidation.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true))
			{
				AssertEquals("Precondition", 0m, workOrder.WD_TotalUnits);
				workOrder.FinaliseDocketAlwaysFinalisingPick();
				AssertEquals("Work order is not finalised.", false, workOrder.IsFinalised);
				AssertHasError(workOrder.WD_TotalUnitsInfo, "Total Units 0 does not equal the total of all assembly line units (including secondary products and over-picked components) 68.");

				workOrder.WD_TotalUnits = 68m;
				workOrder.FinaliseDocketAlwaysFinalisingPick();
				AssertEquals("Work order is finalised.", true, workOrder.IsFinalised);

				Factory.Save();
				var receive = workOrder.Receive;
				AssertEquals("Receive's total unit is correct", 68m, receive.WD_TotalUnits);
				var lines = receive.Lines;
				AssertEquals("Should have 4 lines.", 4, lines.Count);
				var receiveLine1 = lines.Single(l => l.WE_OP == mainProduct.PK);
				var receiveLine2 = lines.Single(l => l.WE_OP == componentProduct2.PK);
				var receiveLine3 = lines.Single(l => l.WE_OP == secondaryPart1.PK);
				var receiveLine4 = lines.Single(l => l.WE_OP == secondaryPart2.PK);
				AssertEquals("Should have 8 main product1", 8m, receiveLine1.WE_TransactionQuantity);
				AssertEquals("Should have 4 over picked sub product", 4m, receiveLine2.WE_TransactionQuantity);
				AssertEquals("Should have 24 for secondary product", 24m, receiveLine3.WE_TransactionQuantity);
				AssertEquals("Should have 32 for secondary product", 32m, receiveLine4.WE_TransactionQuantity);
			}
		}

		public void TestTotalUnitsValidation_Assembly_WithOverpickedComponents()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var mainProduct1 = Helper.CreateProduct("BIKE", data.Org1);
			var componentProduct1_1 = Helper.CreateProduct("WHEEL", data.Org1);
			Helper.CreateProductBOM(mainProduct1, componentProduct1_1, 2m, Constants.PkgUnit.Unit);
			var componentProduct1_2 = Helper.CreateProduct("ENGINE", data.Org1);
			Helper.CreateProductBOM(mainProduct1, componentProduct1_2, 1m, Constants.PkgUnit.Unit);

			var mainProduct2 = Helper.CreateProduct("MAIN", data.Org1);
			var componentProduct2_1 = Helper.CreateProduct("SUB1", data.Org1);
			Helper.CreateProductBOM(mainProduct2, componentProduct2_1, 4m, Constants.PkgUnit.Unit);
			var componentProduct2_2 = Helper.CreateProduct("SUB2", data.Org1);
			Helper.CreateProductBOM(mainProduct2, componentProduct2_2, 2m, Constants.PkgUnit.Unit);
			var componentProduct2_3 = Helper.CreateProduct("SUB3", data.Org1);
			Helper.CreateProductBOM(mainProduct2, componentProduct2_3, 3m, Constants.PkgUnit.Unit);
			Factory.Save();

			var componentReceive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveInventoryLine(componentReceive, componentProduct1_1, 20m);
			Helper.CreateWhsReceiveInventoryLine(componentReceive, componentProduct1_2, 10m);
			Helper.CreateWhsReceiveInventoryLine(componentReceive, componentProduct2_1, 20m);
			Helper.CreateWhsReceiveInventoryLine(componentReceive, componentProduct2_2, 10m);
			Helper.CreateWhsReceiveInventoryLine(componentReceive, componentProduct2_3, 15m);
			componentReceive.AllocateLocationsWithMock();
			componentReceive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(componentReceive);
			Factory.Save();

			var workOrder = Helper.CreateWhsWorkOrderWithLine(data.Org1, data.Whs1, "ExtRef", mainProduct1, 10m);
			Helper.CreateWhsWorkOrderLine(workOrder, mainProduct2, 5m);
			workOrder.WD_DocketSubType = WorkOrderType.Codes.Assemble;
			var pick = Helper.CreatePickNew(workOrder);
			var pickLines = pick.GetAllPickLines();
			var pickLine1 = pickLines.Single(l => l.ProductCode == "WHEEL");
			var pickLine2 = pickLines.Single(l => l.ProductCode == "ENGINE");
			var pickLine3 = pickLines.Single(l => l.ProductCode == "SUB1");
			var pickLine4 = pickLines.Single(l => l.ProductCode == "SUB2");
			var pickLine5 = pickLines.Single(l => l.ProductCode == "SUB3");
			pickLine2.WZ_Units = 8m;
			pickLine5.WZ_Units = 13m;
			Factory.Save();

			using (WarehouseDataRegistry.Instance.TotalUnitsValidation.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true))
			{
				AssertEquals("Precondition", 0m, workOrder.WD_TotalUnits);
				workOrder.FinaliseDocketAlwaysFinalisingPick();

				AssertEquals("Work order is not finalised.", false, workOrder.IsFinalised);
				AssertHasError(workOrder.WD_TotalUnitsInfo, "Total Units 0 does not equal the total of all assembly line units (including over-picked components) 23.");

				workOrder.WD_TotalUnits = 23m;
				workOrder.FinaliseDocketAlwaysFinalisingPick();
				AssertEquals("Work order is finalised.", true, workOrder.IsFinalised);

				Factory.Save();
				var receive = workOrder.Receive;
				AssertEquals("Receive's total unit is correct", 23m, receive.WD_TotalUnits);
				var lines = receive.Lines;
				AssertEquals("Should have 6 lines.", 6, lines.Count);
				var receiveLine1 = lines.Single(l => l.WE_OP == mainProduct1.PK);
				var receiveLine2 = lines.Single(l => l.WE_OP == componentProduct1_1.PK);
				var receiveLine3 = lines.Single(l => l.WE_OP == mainProduct2.PK);
				var receiveLine4 = lines.Single(l => l.WE_OP == componentProduct2_1.PK);
				var receiveLine5 = lines.Single(l => l.WE_OP == componentProduct2_2.PK);
				var receiveLine6 = lines.Single(l => l.WE_OP == componentProduct2_3.PK);
				AssertEquals("Should have 8 main product1", 8m, receiveLine1.WE_TransactionQuantity);
				AssertEquals("Should have 4 over picked sub product", 4m, receiveLine2.WE_TransactionQuantity);
				AssertEquals("Should have 4 main product2", 4m, receiveLine3.WE_TransactionQuantity);
				AssertEquals("Should have 4 over picked sub product", 4m, receiveLine4.WE_TransactionQuantity);
				AssertEquals("Should have 2 over picked sub product", 2m, receiveLine5.WE_TransactionQuantity);
				AssertEquals("Should have 1 over picked sub product", 1m, receiveLine6.WE_TransactionQuantity);
			}
		}

		public void TestTotalUnitsValidation_Assembly_WithOverpickedComponents_ComponentPickedZero()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var mainProduct = Helper.CreateProduct("MAIN", data.Org1);
			var componentProduct1 = Helper.CreateProduct("SUB1", data.Org1);
			Helper.CreateProductBOM(mainProduct, componentProduct1, 4m, Constants.PkgUnit.Unit);
			var componentProduct2 = Helper.CreateProduct("SUB2", data.Org1);
			Helper.CreateProductBOM(mainProduct, componentProduct2, 2m, Constants.PkgUnit.Unit);
			var componentProduct3 = Helper.CreateProduct("SUB3", data.Org1);
			Helper.CreateProductBOM(mainProduct, componentProduct3, 3m, Constants.PkgUnit.Unit);
			Factory.Save();

			var componentReceive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveInventoryLine(componentReceive, componentProduct1, 20m);
			Helper.CreateWhsReceiveInventoryLine(componentReceive, componentProduct2, 10m);
			Helper.CreateWhsReceiveInventoryLine(componentReceive, componentProduct3, 15m);
			componentReceive.AllocateLocationsWithMock();
			componentReceive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(componentReceive);
			Factory.Save();

			var workOrder = Helper.CreateWhsWorkOrderWithLine(data.Org1, data.Whs1, "ExtRef", mainProduct, 5m);
			workOrder.WD_DocketSubType = WorkOrderType.Codes.Assemble;
			var pick = Helper.CreatePickNew(workOrder);
			var pickLines = pick.GetAllPickLines();
			var pickLine1 = pickLines.Single(l => l.ProductCode == "SUB1");
			var pickLine2 = pickLines.Single(l => l.ProductCode == "SUB2");
			var pickLine3 = pickLines.Single(l => l.ProductCode == "SUB3");
			pickLine3.WZ_Units = 0m;
			Factory.Save();

			using (WarehouseDataRegistry.Instance.TotalUnitsValidation.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true))
			{
				AssertEquals("Precondition", 0m, workOrder.WD_TotalUnits);
				workOrder.FinaliseDocketAlwaysFinalisingPick();

				AssertEquals("Work order is not finalised.", false, workOrder.IsFinalised);
				AssertHasError(workOrder.WD_TotalUnitsInfo, "Total Units 0 does not equal the total of all assembly line units (including over-picked components) 30.");

				workOrder.WD_TotalUnits = 30m;
				workOrder.FinaliseDocketAlwaysFinalisingPick();
				AssertEquals("Work order is finalised.", true, workOrder.IsFinalised);

				Factory.Save();
				var receive = workOrder.Receive;
				AssertEquals("Receive's total unit is correct", 30m, receive.WD_TotalUnits);
				var lines = receive.Lines;
				AssertEquals("Should have 2 lines.", 2, lines.Count);
				var receiveLine1 = lines.Single(l => l.WE_OP == componentProduct1.PK);
				var receiveLine2 = lines.Single(l => l.WE_OP == componentProduct2.PK);
				AssertEquals("Should have 20 over picked sub product", 20m, receiveLine1.WE_TransactionQuantity);
				AssertEquals("Should have 10 over picked sub product", 10m, receiveLine2.WE_TransactionQuantity);
			}
		}

		public void TestTotalUnitsValidation_Assembly_WhenProductDefinitionChangeForAllComponentProducts_BeforePick()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var mainProduct = Helper.CreateProduct("BIKE", data.Org1);
			var componentProduct1 = Helper.CreateProduct("WHEEL", data.Org1);
			Helper.CreateProductUnit(componentProduct1, Constants.PkgUnit.Carton, 2);
			var bomPart1 = Helper.CreateProductBOM(mainProduct, componentProduct1, 2m, Constants.PkgUnit.Unit);
			var componentProduct2 = Helper.CreateProduct("ENGINE", data.Org1);
			Helper.CreateProductUnit(componentProduct2, Constants.PkgUnit.Bag, 3);
			var bomPart2 = Helper.CreateProductBOM(mainProduct, componentProduct2, 4m, Constants.PkgUnit.Unit);
			Factory.Save();

			var componentReceive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveInventoryLine(componentReceive, componentProduct1, 20m);
			Helper.CreateWhsReceiveInventoryLine(componentReceive, componentProduct2, 60m);
			componentReceive.AllocateLocationsWithMock();
			componentReceive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(componentReceive);
			Factory.Save();

			var workOrder = Helper.CreateWhsWorkOrder(data.Org1, data.Whs1, "ExtRef");
			var workorderLine = Helper.CreateWhsWorkOrderLine(workOrder, mainProduct, 5m);
			workOrder.WD_DocketSubType = WorkOrderType.Codes.Assemble;
			bomPart1.OE_F3_NKPackType = Constants.PkgUnit.Carton;
			bomPart2.OE_F3_NKPackType = Constants.PkgUnit.Bag;
			Factory.Save();

			var pickability = workOrder.GetPickabilityWithoutPick();
			AssertEquals(false, pickability.IsDocketPickable);
			AssertEquals("One of the component products has been changed. Please cancel the work order ExtRef and recreate it.", pickability.Message);
			AssertEquals(NotificationTypes.Error, pickability.MessageType);

			workorderLine.Delete();

			var newWorkOrderLine = Helper.CreateWhsWorkOrderLine(workOrder, mainProduct, 5m);
			var pick = Helper.CreatePickNew(workOrder);
			var pickLines = pick.GetAllPickLines();
			var pickLine1 = pickLines.Single(l => l.ProductCode == "WHEEL");
			var pickLine2 = pickLines.Single(l => l.ProductCode == "ENGINE");
			pickLine1.WZ_Units = 15m;
			Factory.Save();

			using (WarehouseDataRegistry.Instance.TotalUnitsValidation.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true))
			{
				AssertEquals("Precondition", 0m, workOrder.WD_TotalUnits);
				workOrder.FinaliseDocketAlwaysFinalisingPick();

				AssertEquals("Work order is not finalised.", false, workOrder.IsFinalised);
				AssertHasError(workOrder.WD_TotalUnitsInfo, "Total Units 0 does not equal the total of all assembly line units (including over-picked components) 30.");

				workOrder.WD_TotalUnits = 30m;
				workOrder.FinaliseDocketAlwaysFinalisingPick();
				AssertEquals("Work order is finalised.", true, workOrder.IsFinalised);

				Factory.Save();
				var receive = workOrder.Receive;
				AssertEquals("Receive's total unit is correct", 30m, receive.WD_TotalUnits);
				var lines = receive.Lines;
				AssertEquals("Should have 3 lines.", 3, lines.Count);
				var receiveLine1 = lines.Single(l => l.WE_OP == mainProduct.PK);
				var receiveLine2 = lines.Single(l => l.WE_OP == componentProduct1.PK);
				var receiveLine3 = lines.Single(l => l.WE_OP == componentProduct2.PK);
				AssertEquals("Should have 3 main product", 3m, receiveLine1.WE_TransactionQuantity);
				AssertEquals("Should have 3 over picked sub product", 3m, receiveLine2.WE_TransactionQuantity);
				AssertEquals("Should have 24 over picked sub product", 24m, receiveLine3.WE_TransactionQuantity);
			}
		}

		public void TestTotalUnitsValidation_Assembly_WhenProductDefinitionChangeForAllComponentProducts_AfterPick()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var mainProduct = Helper.CreateProduct("BIKE", data.Org1);
			var componentProduct1 = Helper.CreateProduct("WHEEL", data.Org1);
			Helper.CreateProductUnit(componentProduct1, Constants.PkgUnit.Carton, 2);
			var bomPart1 = Helper.CreateProductBOM(mainProduct, componentProduct1, 2m, Constants.PkgUnit.Unit);
			var componentProduct2 = Helper.CreateProduct("ENGINE", data.Org1);
			Helper.CreateProductUnit(componentProduct2, Constants.PkgUnit.Bag, 3);
			var bomPart2 = Helper.CreateProductBOM(mainProduct, componentProduct2, 4m, Constants.PkgUnit.Unit);
			Factory.Save();

			var componentReceive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveInventoryLine(componentReceive, componentProduct1, 20m);
			Helper.CreateWhsReceiveInventoryLine(componentReceive, componentProduct2, 60m);
			componentReceive.AllocateLocationsWithMock();
			componentReceive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(componentReceive);
			Factory.Save();

			var workOrder = Helper.CreateWhsWorkOrder(data.Org1, data.Whs1, "ExtRef");
			var workorderLine = Helper.CreateWhsWorkOrderLine(workOrder, mainProduct, 10m);
			workOrder.WD_DocketSubType = WorkOrderType.Codes.Assemble;
			Factory.Save();

			var pick = Helper.CreatePickNew(workOrder);
			Factory.Save();

			bomPart2.Delete();
			Factory.Save();

			using (WarehouseDataRegistry.Instance.TotalUnitsValidation.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true))
			{
				AssertEquals("Precondition", 0m, workOrder.WD_TotalUnits);
				AssertNoExceptionThrown(() => workOrder.FinaliseDocketAlwaysFinalisingPick());

				AssertEquals("Work order is not finalised.", false, workOrder.IsFinalised);
				AssertHasError(workOrder.WD_TotalUnitsInfo, "Total Units 0 does not equal the total of all assembly line units 10.000.");

				workOrder.WD_TotalUnits = 10m;
				workOrder.FinaliseDocketAlwaysFinalisingPick();
				AssertEquals("Work order is finalised.", true, workOrder.IsFinalised);

				Factory.Save();
				var receive = workOrder.Receive;
				AssertEquals("Receive's total unit is correct", 10m, receive.WD_TotalUnits);
				var lines = receive.Lines;
				AssertEquals("Should have 1 line.", 1, lines.Count);
				var receiveLine1 = lines.Single(l => l.WE_OP == mainProduct.PK);
				AssertEquals("Should have 10 main product", 10m, receiveLine1.WE_TransactionQuantity);
			}
		}

		public void TestTotalUnitsValidation_Assembly_TotalUnitsValidationEnabled()
		{
			TestTotalUnitsValidation_AssemblyCore(isTotalUnitsValidationEnabled: true);
		}

		void TestTotalUnitsValidation_AssemblyCore(bool isTotalUnitsValidationEnabled)
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var mainProduct = Helper.CreateProduct("BIKE", data.Org1);
			var componentProduct1 = Helper.CreateProduct("WHEEL", data.Org1);
			Helper.CreateProductBOM(mainProduct, componentProduct1, 2, Constants.PkgUnit.Unit);
			var componentProduct2 = Helper.CreateProduct("ENGINE", data.Org1);
			Helper.CreateProductBOM(mainProduct, componentProduct2, 1, Constants.PkgUnit.Unit);
			Factory.Save();

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", componentProduct1, 10m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", componentProduct2, 10m);
			Factory.Save();

			var workOrder = Helper.CreateWhsWorkOrderWithLine(data.Org1, data.Whs1, "ExtRef", mainProduct, 5m);
			workOrder.WD_DocketSubType = WorkOrderType.Codes.Assemble;
			Helper.CreatePickNew(workOrder);
			Factory.Save();

			using (WarehouseDataRegistry.Instance.TotalUnitsValidation.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, isTotalUnitsValidationEnabled))
			{
				AssertEquals("Precondition", 0m, workOrder.WD_TotalUnits);
				workOrder.FinaliseDocketAlwaysFinalisingPick();

				if (isTotalUnitsValidationEnabled)
				{
					AssertEquals("Work order is not finalised.", false, workOrder.IsFinalised);
					AssertHasError(workOrder.WD_TotalUnitsInfo, "Total Units 0 does not equal the total of all assembly line units 5.");
				}
				else
				{
					Assert("Work order is finalised.", workOrder.IsFinalised);
				}
			}
		}

		public void TestTotalUnitsValidation_Assembly_DBHits()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var mainProducts = new List<OrgSupplierPart>();
			for (var i = 0; i <= 9; i++)
			{
				var mainProduct = Helper.CreateProduct($"BIKE{i}", data.Org1);
				var componentProduct1 = Helper.CreateProduct($"WHEEL{i}", data.Org1);
				Helper.CreateProductBOM(mainProduct, componentProduct1, 2, Constants.PkgUnit.Unit);
				var componentProduct2 = Helper.CreateProduct($"ENGINE{i}", data.Org1);
				Helper.CreateProductBOM(mainProduct, componentProduct2, 1, Constants.PkgUnit.Unit);
				Factory.Save();

				Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, $"R{i}{i}", componentProduct1, 10m);
				Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, $"R{i}{i + 1}", componentProduct2, 10m);
				Factory.Save();
				mainProducts.Add(mainProduct);
			}

			var workOrder = Helper.CreateWhsWorkOrder(data.Org1, data.Whs1, "ExtRef");
			workOrder.WD_DocketSubType = WorkOrderType.Codes.Assemble;
			mainProducts.ForEach(product => Helper.CreateWhsWorkOrderLine(workOrder, product, 5m));
			Helper.CreatePickNew(workOrder);
			Factory.Save();

			var expectedDbHits = new Dictionary<string, int>()
			{
				{ GenCustomAddOnRuleAckSchema.Constants.TableName, 1 },
				{ JobDocAddressSchema.Constants.TableName, 1 },
				{ JobServiceSchema.Constants.TableName, 1 },
				{ OrgAddressSchema.Constants.TableName, 2 },
				{ OrgAddressCapabilitySchema.Constants.TableName, 1 },
				{ OrgCompanyDataSchema.Constants.TableName, 1 },
				{ OrgCusCodeSchema.Constants.TableName, 1 },
				{ OrgHeaderSchema.Constants.TableName, 1 },
				{ OrgMiscServSchema.Constants.TableName, 1 },
				{ OrgPartBOMSchema.Constants.TableName, 1 },
				{ OrgPartRelationSchema.Constants.TableName, 1 },
				{ OrgCustomLabelsSchema.Constants.TableName, 1 },
				{ OrgSupplierPartSchema.Constants.TableName, 1 },
				{ ProcessTasksSchema.Constants.TableName, 1 },
				{ RefUNLOCOSchema.Constants.TableName, 1 },
				{ WhsAreaSchema.Constants.TableName, 1 },
				{ WhsDocketSchema.Constants.TableName, 1 },
				{ WhsDocketContainerSchema.Constants.TableName, 1 },
				{ WhsDocketLineSchema.Constants.TableName, 3 },
				{ WhsDocketPalletSchema.Constants.TableName, 1 },
				{ WhsDocketReferenceSchema.Constants.TableName, 1 },
				{ WhsPickSchema.Constants.TableName, 1 },
				{ WhsPickLineSchema.Constants.TableName, 1 },
				{ WhsWarehouseSchema.Constants.TableName, 1 },
			};

			var newFactory = new BusinessObjectFactory();
			var workOrderInNewFactory = newFactory.Load<WhsWorkOrder>(workOrder.PK);

			using (AssertDbHitsWithUsefulQueryInformation(expectedDbHits, newFactory))
			using (WarehouseDataRegistry.Instance.TotalUnitsValidation.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true))
			{
				AssertEquals("Precondition", 0m, workOrderInNewFactory.WD_TotalUnits);
				workOrderInNewFactory.FinaliseDocketAlwaysFinalisingPick();

				AssertEquals("Work order is not finalised.", false, workOrderInNewFactory.IsFinalised);
				AssertHasError(workOrderInNewFactory.WD_TotalUnitsInfo, "Total Units 0.000 does not equal the total of all assembly line units 50.000.");
			}
		}

		public void TestTotalUnitsValidation_Disassembly()
		{
			TestTotalUnitsValidation_DisassemblyCore(isTotalUnitsValidationEnabled: false);
		}

		public void TestTotalUnitsValidation_Disassembly_TotalUnitsValidationEnabled()
		{
			TestTotalUnitsValidation_DisassemblyCore(isTotalUnitsValidationEnabled: true);
		}

		void TestTotalUnitsValidation_DisassemblyCore(bool isTotalUnitsValidationEnabled)
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var mainProduct = Helper.CreateProduct("BIKE", data.Org1);
			var componentProduct1 = Helper.CreateProduct("WHEEL", data.Org1);
			Helper.CreateProductBOM(mainProduct, componentProduct1, 2, Constants.PkgUnit.Unit);
			var componentProduct2 = Helper.CreateProduct("ENGINE", data.Org1);
			Helper.CreateProductBOM(mainProduct, componentProduct2, 1, Constants.PkgUnit.Unit);
			Factory.Save();

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", componentProduct1, 10m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", componentProduct2, 10m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R3", mainProduct, 10m);
			Factory.Save();

			var workOrder = Helper.CreateWhsWorkOrderWithLine(data.Org1, data.Whs1, "ExtRef", mainProduct, 5m);
			workOrder.WD_DocketSubType = WorkOrderType.Codes.Disassemble;
			Helper.CreatePickNew(workOrder);
			Factory.Save();

			using (WarehouseDataRegistry.Instance.TotalUnitsValidation.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, isTotalUnitsValidationEnabled))
			{
				AssertEquals("Precondition", 0m, workOrder.WD_TotalUnits);
				workOrder.FinaliseDocketAlwaysFinalisingPick();

				if (isTotalUnitsValidationEnabled)
				{
					AssertEquals("Work order is not finalised.", false, workOrder.IsFinalised);
					AssertHasError(workOrder.WD_TotalUnitsInfo, "Total Units 0 does not equal the total of all disassembly line units 15.");
				}
				else
				{
					Assert("Work order is finalised.", workOrder.IsFinalised);
				}
			}
		}

		#region TestFinaliseDocket_TotalUnits_Disassemble_WithLinks

		public void TestFinaliseDocket_TotalUnits_Disassemble_WithLinks_Valid()
		{
			TestFinaliseDocket_TotalUnits_Disassemble_WithLinks(validTotal: true);
		}

		public void TestFinaliseDocket_TotalUnits_Disassemble_WithLinks_Invalid()
		{
			TestFinaliseDocket_TotalUnits_Disassemble_WithLinks(validTotal: false);
		}

		void TestFinaliseDocket_TotalUnits_Disassemble_WithLinks(bool validTotal)
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var bikeFrame = Helper.CreateProduct(data.Org1, "Frame");
			var bikeWheel = Helper.CreateProduct(data.Org1, "Wheel");
			var bomBike = Helper.CreateProduct(data.Org1, "Bike");
			var bomFrame = Helper.CreateProductBOM(bomBike, bikeFrame);
			var bomWheel = Helper.CreateProductBOM(bomBike, bikeWheel, 2m, bikeWheel.OP_StockKeepingUnit);
			bomWheel.OE_CanReuse = false;
			Factory.Save();

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", bikeWheel, 10m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", bikeFrame, 10m);
			Factory.Save();

			var workOrder = Helper.CreateWhsWorkOrderWithLine(data.Org1, data.Whs1, "Wo1", bomBike, 5m);
			workOrder.WD_DocketSubType = WorkOrderType.Codes.Assemble;

			Helper.CreatePickNew(workOrder);
			workOrder.FinaliseDocketAlwaysFinalisingPick();
			AssertIsFinalisedPrecondition(workOrder);

			var receive = workOrder.Receive;
			receive.FinaliseDocketWithoutUserConfirmation();
			WhsTestCaseWithFactory.AssertIsFinalisedPrecondition(receive);
			AssertEquals("Precondition: Should have created 1 products.", 1, receive.Lines.Count);
			AssertEquals("Precondition: Kit has two links.", 2, ((WhsReceiveLine)receive.Lines.Single()).BOMComponentLinks.Count());
			Factory.Save();

			var disassembleWorkOrder = Helper.CreateWhsWorkOrderWithLine(data.Org1, data.Whs1, "Wo2", bomBike, 5m);
			disassembleWorkOrder.WD_DocketSubType = WorkOrderType.Codes.Disassemble;

			WarehouseDataRegistry.Instance.TotalUnitsValidation.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);

			if (validTotal)
			{
				disassembleWorkOrder.WD_TotalUnits = 5m;
				using (ObjectFactory.Substitute<IAllocationEngineManager>(new AllocateFIFOLegacyMock()))
				{
					disassembleWorkOrder.FinaliseDocketAlwaysFinalisingPick();
				}
				AssertEquals("Work order is finalised.", true, workOrder.IsFinalised);
				AssertNoErrors(workOrder.WD_TotalUnitsInfo);

				var disassembleRreceive = disassembleWorkOrder.Receive;
				AssertEquals("Should have created one products.", 1, disassembleRreceive.Lines.Count);
				AssertEquals("Does not need to create link for disassemble.", 0, disassembleRreceive.Lines.Single().BOMComponentLinks.Count());
			}
			else
			{
				disassembleWorkOrder.WD_TotalUnits = 4m; // just not valid 5m
				using (ObjectFactory.Substitute<IAllocationEngineManager>(new AllocateFIFOLegacyMock()))
				{
					disassembleWorkOrder.FinaliseDocketAlwaysFinalisingPick();
				}

				AssertNull("Should not create receive.", disassembleWorkOrder.Receive);
			}
		}

		#endregion

		public void TestTotalUnitsValidation_Disassembly_DBHits()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var mainProducts = new List<OrgSupplierPart>();
			for (var i = 0; i <= 9; i++)
			{
				var mainProduct = Helper.CreateProduct($"BIKE{i}", data.Org1);
				var componentProduct1 = Helper.CreateProduct($"WHEEL{i}", data.Org1);
				Helper.CreateProductBOM(mainProduct, componentProduct1, 2, Constants.PkgUnit.Unit);
				var componentProduct2 = Helper.CreateProduct($"ENGINE{i}", data.Org1);
				Helper.CreateProductBOM(mainProduct, componentProduct2, 1, Constants.PkgUnit.Unit);
				Factory.Save();

				Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, $"R{i}{i}", componentProduct1, 10m);
				Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, $"R{i}{i + 1}", componentProduct2, 10m);
				Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, $"R{i}{i + 2}", mainProduct, 10m);
				Factory.Save();
				mainProducts.Add(mainProduct);
			}

			var workOrder = Helper.CreateWhsWorkOrder(data.Org1, data.Whs1, "ExtRef");
			workOrder.WD_DocketSubType = WorkOrderType.Codes.Disassemble;
			mainProducts.ForEach(product => Helper.CreateWhsWorkOrderLine(workOrder, product, 5m));
			Helper.CreatePickNew(workOrder);
			Factory.Save();

			var expectedDbHits = new Dictionary<string, int>()
			{
				{ GenCustomAddOnRuleAckSchema.Constants.TableName, 1 },
				{ JobDocAddressSchema.Constants.TableName, 1 },
				{ JobServiceSchema.Constants.TableName, 1 },
				{ OrgAddressSchema.Constants.TableName, 2 },
				{ OrgAddressCapabilitySchema.Constants.TableName, 1 },
				{ OrgCompanyDataSchema.Constants.TableName, 1 },
				{ OrgCusCodeSchema.Constants.TableName, 1 },
				{ OrgHeaderSchema.Constants.TableName, 1 },
				{ OrgMiscServSchema.Constants.TableName, 1 },
				{ OrgPartBOMSchema.Constants.TableName, 1 },
				{ OrgPartRelationSchema.Constants.TableName, 1 },
				{ OrgSupplierPartSchema.Constants.TableName, 1 },
				{ OrgCustomLabelsSchema.Constants.TableName, 1 },
				{ ProcessTasksSchema.Constants.TableName, 1 },
				{ RefUNLOCOSchema.Constants.TableName, 1 },
				{ WhsAreaSchema.Constants.TableName, 1 },
				{ WhsDocketSchema.Constants.TableName, 1 },
				{ WhsDocketContainerSchema.Constants.TableName, 1 },
				{ WhsDocketLineSchema.Constants.TableName, 3 },
				{ WhsDocketPalletSchema.Constants.TableName, 1 },
				{ WhsDocketReferenceSchema.Constants.TableName, 1 },
				{ WhsPickSchema.Constants.TableName, 1 },
				{ WhsPickLineSchema.Constants.TableName, 1 },
				{ WhsWarehouseSchema.Constants.TableName, 1 },
				{ WhsBOMInventoryPivotSchema.Constants.TableName, 1 },
			};

			var newFactory = new BusinessObjectFactory();
			var workOrderInNewFactory = newFactory.Load<WhsWorkOrder>(workOrder.PK);

			using (AssertDbHitsWithUsefulQueryInformation(expectedDbHits, newFactory))
			using (WarehouseDataRegistry.Instance.TotalUnitsValidation.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true))
			{
				AssertEquals("Precondition", 0m, workOrderInNewFactory.WD_TotalUnits);
				workOrderInNewFactory.FinaliseDocketAlwaysFinalisingPick();

				AssertEquals("Work order is not finalised.", false, workOrderInNewFactory.IsFinalised);
				AssertHasError(workOrderInNewFactory.WD_TotalUnitsInfo, "Total Units 0.000 does not equal the total of all disassembly line units 150.000000.");
			}
		}

		public void TestTotalUnitsValidation_Assembly_IPROrder_IncludesSecondaryProducts()
		{
			TestTotalUnitsValidation_Assembly_IPROrder_IncludesSecondaryProductsCore(isTotalUnitsValidationEnabled: false);
		}

		public void TestTotalUnitsValidation_Assembly_IPROrder_IncludesSecondaryProducts_TotalUnitsValidationEnabled()
		{
			TestTotalUnitsValidation_Assembly_IPROrder_IncludesSecondaryProductsCore(isTotalUnitsValidationEnabled: true);
		}

		void TestTotalUnitsValidation_Assembly_IPROrder_IncludesSecondaryProductsCore(bool isTotalUnitsValidationEnabled)
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.EnableWarehouseForBond(data.Whs1, true);
			data.Whs1.WW_IsVirtualWarehouse = true;
			var secondaryPart1 = Helper.CreateProduct("P3", data.Org1);
			var secondaryPart2 = Helper.CreateProduct("P4", data.Org1);
			Helper.CreateProductBOM(data.Part2, data.Part1, 2m, "UNT");
			Helper.CreateSecondaryProduct(data.Part2, secondaryPart1, 3m);
			Helper.CreateSecondaryProduct(data.Part2, secondaryPart2, 4m);

			var area = Helper.CreateArea(data.Whs1, "IPR", AreaTypes.Codes.InwardProcessing);
			var row = Helper.CreateRowAndGenerateLocations(data.Whs1, "I");
			var location = row.Locations.Single();
			location.WLV_WA_PutawayArea = area.PK;
			location.WLV_WA_PickingArea = area.PK;
			Factory.Save();

			var componentReceive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m, "BEK-1", allocateLocations: false, finalise: false);
			componentReceive.WD_DocketSubType = ReceiveType.Codes.Customs;
			componentReceive.WD_IsInwardsProcessingJob = true;
			componentReceive.Lines[0].WE_WL = location.PK;
			componentReceive.FinaliseDocket();
			AssertIsFinalisedPrecondition(componentReceive);
			Factory.Save();

			var workOrder = Helper.CreateWhsWorkOrderWithLine(data.Org1, data.Whs1, "ExtRef", data.Part2, 5m);
			workOrder.WD_DocketSubType = WorkOrderType.Codes.Assemble;
			workOrder.WD_RequiredDate = ZDateTimeOffset.Now;
			workOrder.WD_IsInwardsProcessingJob = true;

			var productParam = workOrder.Lines[0].Product.ParamsByWhsAndClient.AddNew();
			productParam.W3_OH = data.Org1.PK;
			productParam.W3_WW = data.Whs1.PK;
			productParam.W3_WL_InwardsProcessingStagingLocationBOM = location.PK;

			Helper.CreatePickNew(workOrder);
			Factory.Save();

			using (WarehouseDataRegistry.Instance.TotalUnitsValidation.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, isTotalUnitsValidationEnabled))
			{
				AssertEquals("Precondition", 0m, workOrder.WD_TotalUnits);
				workOrder.FinaliseDocketAlwaysFinalisingPick();

				if (isTotalUnitsValidationEnabled)
				{
					AssertEquals("Work order is not finalised.", false, workOrder.IsFinalised);
					AssertHasError(workOrder.WD_TotalUnitsInfo, "Total Units 0 does not equal the total of all assembly line units (including secondary products) 40.");
				}
				else
				{
					Assert("Work order is finalised.", workOrder.IsFinalised);
				}
			}
		}

		public void TestTotalUnitsValidation_Assembly_IPRSecondaryProducts_DBHits_Finalise()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.EnableWarehouseForBond(data.Whs1, true);
			data.Whs1.WW_IsVirtualWarehouse = true;

			var area = Helper.CreateArea(data.Whs1, "IPR", AreaTypes.Codes.InwardProcessing);
			var row = Helper.CreateRowAndGenerateLocations(data.Whs1, "I");
			var location = row.Locations.Single();
			location.WLV_WA_PutawayArea = area.PK;
			location.WLV_WA_PickingArea = area.PK;
			Factory.Save();

			var mainProducts = new List<OrgSupplierPart>();
			for (var i = 0; i <= 9; i++)
			{
				var mainProduct = Helper.CreateProduct($"BIKE{i}", data.Org1);
				var componentProduct = Helper.CreateProduct($"WHEEL{i}", data.Org1);
				var secondaryPart1 = Helper.CreateProduct($"P3{i}", data.Org1);
				var secondaryPart2 = Helper.CreateProduct($"P4{i}", data.Org1);
				Helper.CreateProductBOM(mainProduct, componentProduct, 2m, "UNT");
				Helper.CreateSecondaryProduct(mainProduct, secondaryPart1, 3m);
				Helper.CreateSecondaryProduct(mainProduct, secondaryPart2, 4m);
				Factory.Save();

				var componentReceive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, $"R{i}", componentProduct, 10m, $"BEK-1{i}", allocateLocations: false, finalise: false);
				componentReceive.WD_DocketSubType = ReceiveType.Codes.Customs;
				componentReceive.WD_IsInwardsProcessingJob = true;
				componentReceive.Lines[0].WE_WL = location.PK;
				componentReceive.FinaliseDocket();
				AssertIsFinalisedPrecondition(componentReceive);
				Factory.Save();
				mainProducts.Add(mainProduct);
			}

			var workOrder = Helper.CreateWhsWorkOrder(data.Org1, data.Whs1, "ExtRef");
			workOrder.WD_DocketSubType = WorkOrderType.Codes.Assemble;
			workOrder.WD_RequiredDate = ZDateTimeOffset.Now;
			workOrder.WD_IsInwardsProcessingJob = true;
			mainProducts.ForEach(product => Helper.CreateWhsWorkOrderLine(workOrder, product, 5m));
			Helper.CreatePickNew(workOrder);

			var productParam = workOrder.Lines[0].Product.ParamsByWhsAndClient.AddNew();
			productParam.W3_OH = data.Org1.PK;
			productParam.W3_WW = data.Whs1.PK;
			productParam.W3_WL_InwardsProcessingStagingLocationBOM = location.PK;

			Factory.Save();

			var expectedDbHits = new Dictionary<string, int>()
			{
				{ GenCustomAddOnRuleAckSchema.Constants.TableName, 1 },
				{ JobDocAddressSchema.Constants.TableName, 1 },
				{ JobServiceSchema.Constants.TableName, 1 },
				{ OrgAddressSchema.Constants.TableName, 3 },
				{ OrgAddressCapabilitySchema.Constants.TableName, 1 },
				{ OrgCompanyDataSchema.Constants.TableName, 1 },
				{ OrgCusCodeSchema.Constants.TableName, 1 },
				{ OrgHeaderSchema.Constants.TableName, 1 },
				{ OrgMiscServSchema.Constants.TableName, 1 },
				{ OrgPartBOMSchema.Constants.TableName, 1 },
				{ OrgPartRelationSchema.Constants.TableName, 1 },
				{ OrgSupplierPartSchema.Constants.TableName, 1 },
				{ OrgCustomLabelsSchema.Constants.TableName, 1 },
				{ ProcessTasksSchema.Constants.TableName, 1 },
				{ RefUNLOCOSchema.Constants.TableName, 1 },
				{ StmALogSchema.Constants.TableName, 1 },
				{ WhsAreaSchema.Constants.TableName, 1 },
				{ WhsBondedWarehouseAttributeSchema.Constants.TableName, 1 },
				{ WhsDocketSchema.Constants.TableName, 1 },
				{ WhsDocketContainerSchema.Constants.TableName, 1 },
				{ WhsDocketLineSchema.Constants.TableName, 3 },
				{ WhsDocketPalletSchema.Constants.TableName, 1 },
				{ WhsDocketReferenceSchema.Constants.TableName, 1 },
				{ WhsPickSchema.Constants.TableName, 1 },
				{ WhsPickLineSchema.Constants.TableName, 1 },
				{ WhsWarehouseSchema.Constants.TableName, 1 },
				{ OrgSecondaryPartBOMSchema.Constants.TableName, 1 },
			};

			var newFactory = new BusinessObjectFactory();
			var workOrderInNewFactory = newFactory.Load<WhsWorkOrder>(workOrder.PK);

			using (AssertDbHitsWithUsefulQueryInformation(expectedDbHits, newFactory))
			using (WarehouseDataRegistry.Instance.TotalUnitsValidation.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true))
			{
				AssertEquals("Precondition", 0m, workOrderInNewFactory.WD_TotalUnits);
				workOrderInNewFactory.FinaliseDocketAlwaysFinalisingPick();

				AssertEquals("Work order is not finalised.", false, workOrderInNewFactory.IsFinalised);
				AssertHasError(workOrderInNewFactory.WD_TotalUnitsInfo, "Total Units 0.000 does not equal the total of all assembly line units (including secondary products) 400.000000.");
			}
		}

		public void TestTotalUnitsValidation_Assembly_IPRSecondaryProducts_DBHits_RunPreSaveValidation()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.EnableWarehouseForBond(data.Whs1, true);
			data.Whs1.WW_IsVirtualWarehouse = true;

			var area = Helper.CreateArea(data.Whs1, "IPR", AreaTypes.Codes.InwardProcessing);
			var row = Helper.CreateRowAndGenerateLocations(data.Whs1, "I");
			var location = row.Locations.Single();
			location.WLV_WA_PutawayArea = area.PK;
			location.WLV_WA_PickingArea = area.PK;
			Factory.Save();

			var mainProducts = new List<OrgSupplierPart>();
			for (var i = 0; i <= 9; i++)
			{
				var mainProduct = Helper.CreateProduct($"BIKE{i}", data.Org1);
				var componentProduct = Helper.CreateProduct($"WHEEL{i}", data.Org1);
				var secondaryPart1 = Helper.CreateProduct($"P3{i}", data.Org1);
				var secondaryPart2 = Helper.CreateProduct($"P4{i}", data.Org1);
				Helper.CreateProductBOM(mainProduct, componentProduct, 2m, "UNT");
				Helper.CreateSecondaryProduct(mainProduct, secondaryPart1, 3m);
				Helper.CreateSecondaryProduct(mainProduct, secondaryPart2, 4m);
				Factory.Save();

				var componentReceive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, $"R{i}", componentProduct, 10m, $"BEK-1{i}", allocateLocations: false, finalise: false);
				componentReceive.WD_DocketSubType = ReceiveType.Codes.Customs;
				componentReceive.WD_IsInwardsProcessingJob = true;
				componentReceive.Lines[0].WE_WL = location.PK;
				componentReceive.FinaliseDocket();
				AssertIsFinalisedPrecondition(componentReceive);
				Factory.Save();
				mainProducts.Add(mainProduct);
			}

			var workOrder = Helper.CreateWhsWorkOrder(data.Org1, data.Whs1, "ExtRef");
			workOrder.WD_DocketSubType = WorkOrderType.Codes.Assemble;
			workOrder.WD_RequiredDate = ZDateTimeOffset.Now;
			workOrder.WD_IsInwardsProcessingJob = true;
			mainProducts.ForEach(product => Helper.CreateWhsWorkOrderLine(workOrder, product, 5m));
			Helper.CreatePickNew(workOrder);

			var productParam = workOrder.Lines[0].Product.ParamsByWhsAndClient.AddNew();
			productParam.W3_OH = data.Org1.PK;
			productParam.W3_WW = data.Whs1.PK;
			productParam.W3_WL_InwardsProcessingStagingLocationBOM = location.PK;

			Factory.Save();

			var expectedDbHits = new Dictionary<string, int>()
			{
				{ OrgAddressSchema.Constants.TableName, 1 },
				{ OrgCompanyDataSchema.Constants.TableName, 1 },
				{ OrgHeaderSchema.Constants.TableName, 1 },
				{ OrgMiscServSchema.Constants.TableName, 1 },
				{ OrgPartBOMSchema.Constants.TableName, 1 },
				{ OrgPartRelationSchema.Constants.TableName, 1 },
				{ OrgSupplierPartSchema.Constants.TableName, 1 },
				{ OrgCustomLabelsSchema.Constants.TableName, 1 },
				{ RefUNLOCOSchema.Constants.TableName, 1 },
				{ StmALogSchema.Constants.TableName, 1 },
				{ WhsAreaSchema.Constants.TableName, 1 },
				{ WhsBondedWarehouseAttributeSchema.Constants.TableName, 1 },
				{ WhsDocketSchema.Constants.TableName, 1 },
				{ WhsDocketLineSchema.Constants.TableName, 2 },
				{ WhsPickSchema.Constants.TableName, 1 },
				{ WhsPickLineSchema.Constants.TableName, 1 },
				{ WhsWarehouseSchema.Constants.TableName, 1 },
			};

			var newFactory = new BusinessObjectFactory();
			var workOrderInNewFactory = newFactory.Load<WhsWorkOrder>(workOrder.PK);

			using (AssertDbHitsWithUsefulQueryInformation(expectedDbHits, newFactory))
			using (WarehouseDataRegistry.Instance.TotalUnitsValidation.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true))
			{
				workOrderInNewFactory.RunPreSaveValidationWithFetchHints();
			}
		}

		#endregion

		#region Implementation

		protected override IEnumerable<string> ValidSubTypeForInwardProcessing => new[] { WorkOrderType.Codes.Assemble };

		#endregion
	}
}
