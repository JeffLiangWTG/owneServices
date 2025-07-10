using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Environment.CodeLists;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	class WhsDynamicWorkOrderValidationTest : WhsComponentOrderValidationTest<WhsDynamicWorkOrder>
	{
		#region TestCheckWD_WW_Whs

		public void TestCheckWD_WW_Whs_WarehouseMustBeVirtual()
		{
			var whs1 = Helper.CreateWarehouse("NON");
			Helper.CreateArea(whs1, "IPR", AreaTypes.Codes.InwardProcessing);
			whs1.WW_IsVirtualWarehouse = false;

			var whs2 = Helper.CreateWarehouse("VIR");
			Helper.CreateArea(whs2, "IPR", AreaTypes.Codes.InwardProcessing);
			whs2.WW_IsVirtualWarehouse = true;

			var workOrder = GetNewBusinessObject();
			workOrder.WD_WW_Whs = whs2.PK;
			AssertNoErrors(workOrder.WD_WW_WhsInfo);

			workOrder.WD_WW_Whs = whs1.PK;
			AssertHasError(workOrder.WD_WW_WhsInfo, "Warehouse used for Dynamic Work Orders must be virtual.");

			workOrder.WD_WW_Whs = whs2.PK;
			AssertNoErrors(workOrder.WD_WW_WhsInfo);
		}

		#endregion

		#region TestCheckWD_IsInwardsProcessingJob

		protected override void TestCheckWD_IsInwardsProcessingJobCore()
		{
			var warehouse = Factory.New<WhsWarehouse>();
			warehouse.WW_IsVirtualWarehouse = false;
			Helper.CreateArea(warehouse, "IPR", AreaTypes.Codes.InwardProcessing);

			var docket = GetNewBusinessObject();
			docket.WD_WW_Whs = warehouse.PK;

			docket.WD_IsInwardsProcessingJob = true;
			AssertHasError(docket.WD_IsInwardsProcessingJobInfo, "Inward Processing Jobs can only be created in Virtual Warehouses.");

			warehouse.WW_IsVirtualWarehouse = true;
			docket.WD_IsInwardsProcessingJob = true;
			AssertNoErrors(docket.WD_IsInwardsProcessingJobInfo);

			docket.WD_IsInwardsProcessingJob = false;
			AssertHasError(docket.WD_IsInwardsProcessingJobInfo, "Dynamic Work Orders must be Inward Processing Jobs.");

			docket.WD_WW_Whs = ZGuid.Empty;
			docket.WD_IsInwardsProcessingJob = true;
			AssertHasError(docket.WD_IsInwardsProcessingJobInfo, "Inward Processing Jobs can only be created in Virtual Warehouses.");
		}

		#endregion

		#region CheckWD_RequiredDate

		public void TestCheckWD_RequiredDate()
		{
			var workOrder = GetNewBusinessObject();
			workOrder.WD_RequiredDate = ZDateTimeOffset.Now;
			AssertNoErrors(workOrder.WD_RequiredDateInfo);

			workOrder.WD_RequiredDate = ZDateTimeOffset.Empty;
			AssertHasError(workOrder.WD_RequiredDateInfo, "Please enter a Required Date.");

			workOrder.WD_RequiredDate = ZDateTimeOffset.Now;
			AssertNoErrors(workOrder.WD_RequiredDateInfo);
		}

		#endregion

		#region TestValidateWD_DocketSubType_BondedWarehouse

		protected override void TestValidateWD_DocketSubType_BondedWarehouse()
		{
			var docket = GetNewBusinessObject();
			var whs = Helper.CreateWarehouse("1");
			docket.WD_WW_Whs = whs.PK;

			AssertHasError(docket.WD_DocketSubTypeInfo, WhsDocketValidation.InwardProcessingNotEnabledErrorMsg);

			Helper.CreateArea(whs, "IPR", AreaTypes.Codes.InwardProcessing);
			docket.WD_DocketSubType = WorkOrderType.Codes.Disassemble;
			docket.WD_DocketSubType = WorkOrderType.Codes.Assemble;
			AssertNoErrors(docket.WD_DocketSubTypeInfo);

			Helper.EnableWarehouseForBond(whs, true);
			docket.WD_DocketSubType = WorkOrderType.Codes.Disassemble;
			docket.WD_DocketSubType = WorkOrderType.Codes.Assemble;
			AssertNoErrors(docket.WD_DocketSubTypeInfo);
		}

		#endregion

		#region TestValidateWD_TotalUnits

		#region TestTotalUnitsValidation_Assembly

		public void TestTotalUnitsValidation_Assembly()
		{
			TestTotalUnitsValidation_AssemblyCore(isTotalUnitsValidationEnabled: false, 0m);
		}

		public void TestTotalUnitsValidation_Assembly_TotalUnitsValidationEnabled_NoValue()
		{
			TestTotalUnitsValidation_AssemblyCore(isTotalUnitsValidationEnabled: true, 0m);
		}

		public void TestTotalUnitsValidation_Assembly_TotalUnitsValidationEnabled_GreaterThanValue()
		{
			TestTotalUnitsValidation_AssemblyCore(isTotalUnitsValidationEnabled: true, 8m);
		}

		public void TestTotalUnitsValidation_Assembly_TotalUnitsValidationEnabled_EqualValue()
		{
			TestTotalUnitsValidation_AssemblyCore(isTotalUnitsValidationEnabled: true, 5m);
		}

		void TestTotalUnitsValidation_AssemblyCore(bool isTotalUnitsValidationEnabled, decimal totalUnits)
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.EnableWarehouseForBond(data.Whs1, true);
			data.Whs1.WW_IsVirtualWarehouse = true;

			var area = Helper.CreateArea(data.Whs1, "IPR", AreaTypes.Codes.InwardProcessing);
			var row = Helper.CreateRowAndGenerateLocations(data.Whs1, "I");
			var location = row.Locations.Single();
			location.WLV_WA_PutawayArea = area.PK;
			location.WLV_WA_PickingArea = area.PK;

			var mainProduct = Helper.CreateProduct("BIKE", data.Org1);
			var componentProduct1 = Helper.CreateProduct("WHEEL", data.Org1);
			var componentProduct2 = Helper.CreateProduct("ENGINE", data.Org1);
			Factory.Save();

			var component1Receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, $"R1", componentProduct1, 10m, $"BEK-1", allocateLocations: false, finalise: false);
			component1Receive.WD_DocketSubType = ReceiveType.Codes.Customs;
			component1Receive.WD_IsInwardsProcessingJob = true;
			component1Receive.Lines[0].WE_WL = location.PK;
			component1Receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(component1Receive);

			var component2Receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, $"R2", componentProduct2, 10m, $"BEK-2", allocateLocations: false, finalise: false);
			component2Receive.WD_DocketSubType = ReceiveType.Codes.Customs;
			component2Receive.WD_IsInwardsProcessingJob = true;
			component2Receive.Lines[0].WE_WL = location.PK;
			component2Receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(component2Receive);
			Factory.Save();

			var workOrder = Helper.CreateWhsDynamicWorkOrder(data.Org1, data.Whs1, "ExtRef");
			workOrder.WD_RequiredDate = ZDateTimeOffset.Now;
			workOrder.WD_IsInwardsProcessingJob = true;

			var workOrderLineMain = Helper.CreateWhsDynamicWorkOrderLine(workOrder, mainProduct, 5m);
			workOrderLineMain.CustomsData.WB_IsMainInwardsProcessedItem = true;

			var workOrderLineMainComp1 = Helper.CreateWhsDynamicWorkOrderLine(workOrder, componentProduct1, 5m);
			workOrderLineMainComp1.WE_WE_ParentDocketLine = workOrderLineMain.PK;

			var workOrderLineMainComp2 = Helper.CreateWhsDynamicWorkOrderLine(workOrder, componentProduct2, 10m);
			workOrderLineMainComp2.WE_WE_ParentDocketLine = workOrderLineMain.PK;

			var productParam = workOrder.Lines[0].Product.ParamsByWhsAndClient.AddNew();
			productParam.W3_OH = data.Org1.PK;
			productParam.W3_WW = data.Whs1.PK;
			productParam.W3_WL_InwardsProcessingStagingLocationBOM = location.PK;

			Helper.CreatePickNew(workOrder);
			Factory.Save();

			using (WarehouseDataRegistry.Instance.TotalUnitsValidation.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, isTotalUnitsValidationEnabled))
			{
				workOrder.WD_TotalUnits = totalUnits;
				AssertEquals("Precondition", totalUnits, workOrder.WD_TotalUnits);
				workOrder.FinaliseDocketAlwaysFinalisingPick();

				if (isTotalUnitsValidationEnabled && totalUnits != 5m)
				{
					AssertEquals("Dynamic Work order is not finalised.", false, workOrder.IsFinalised);
					AssertHasError(workOrder.WD_TotalUnitsInfo, $"Total Units {totalUnits} does not equal the total of all assembly line units (including secondary products) 5.");
				}
				else
				{
					Assert("Dynamic Work order is finalised.", workOrder.IsFinalised);
				}
			}
		}

		public void TestTotalUnitsValidation_Assembly_DBHits()
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

			var mainProduct = Helper.CreateProduct($"MAIN", data.Org1);
			var componentReceive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "MAIN", mainProduct, 10m, "BEK-MAIN", allocateLocations: false, finalise: false);
			componentReceive.WD_DocketSubType = ReceiveType.Codes.Customs;
			componentReceive.WD_IsInwardsProcessingJob = true;
			componentReceive.Lines[0].WE_WL = location.PK;
			componentReceive.FinaliseDocket();
			AssertIsFinalisedPrecondition(componentReceive);

			var products = new List<(OrgSupplierPart secondary, OrgSupplierPart comp1, OrgSupplierPart comp2)>();
			for (var i = 0; i <= 9; i++)
			{
				var secondaryProduct = Helper.CreateProduct($"BIKE{i}", data.Org1);
				var componentProduct1 = Helper.CreateProduct($"WHEEL{i}", data.Org1);
				Helper.CreateProductBOM(mainProduct, componentProduct1, 2, Constants.PkgUnit.Unit);
				var componentProduct2 = Helper.CreateProduct($"ENGINE{i}", data.Org1);
				Helper.CreateProductBOM(mainProduct, componentProduct2, 1, Constants.PkgUnit.Unit);
				Factory.Save();

				var component1Receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, $"R{i}{i}", componentProduct1, 10m, $"BEK-1{i}", allocateLocations: false, finalise: false);
				component1Receive.WD_DocketSubType = ReceiveType.Codes.Customs;
				component1Receive.WD_IsInwardsProcessingJob = true;
				component1Receive.Lines[0].WE_WL = location.PK;
				component1Receive.FinaliseDocket();
				AssertIsFinalisedPrecondition(component1Receive);

				var component2Receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, $"R{i}{i + 1}", componentProduct2, 10m, $"BEK-1{i + 1}", allocateLocations: false, finalise: false);
				component2Receive.WD_DocketSubType = ReceiveType.Codes.Customs;
				component2Receive.WD_IsInwardsProcessingJob = true;
				component2Receive.Lines[0].WE_WL = location.PK;
				component2Receive.FinaliseDocket();
				AssertIsFinalisedPrecondition(component2Receive);
				Factory.Save();
				products.Add((mainProduct, componentProduct1, componentProduct2));
			}

			var workOrder = Helper.CreateWhsDynamicWorkOrder(data.Org1, data.Whs1, "ExtRef");
			workOrder.WD_RequiredDate = ZDateTimeOffset.Now;
			workOrder.WD_IsInwardsProcessingJob = true;

			var workOrderLineMain = Helper.CreateWhsDynamicWorkOrderLine(workOrder, mainProduct, 5m);
			workOrderLineMain.CustomsData.WB_IsMainInwardsProcessedItem = true;

			foreach (var (secondProd, componentProd1, componentProd2) in products)
			{
				var workOrderLineMainComp1 = Helper.CreateWhsDynamicWorkOrderLine(workOrder, componentProd1, 5m);
				workOrderLineMainComp1.WE_WE_ParentDocketLine = workOrderLineMain.PK;

				var workOrderLineMainComp2 = Helper.CreateWhsDynamicWorkOrderLine(workOrder, componentProd2, 10m);
				workOrderLineMainComp2.WE_WE_ParentDocketLine = workOrderLineMain.PK;

				var workOrderLineSecondary = Helper.CreateWhsDynamicWorkOrderLine(workOrder, secondProd, 5m);
				workOrderLineSecondary.CustomsData.WB_IsSecondaryInwardsProcessedItem = true;

				var workOrderLineSecondaryComp = Helper.CreateWhsDynamicWorkOrderLine(workOrder, componentProd1, 2m);
				workOrderLineSecondaryComp.WE_WE_ParentDocketLine = workOrderLineSecondary.PK;
			}

			var productParam = workOrder.Lines[0].Product.ParamsByWhsAndClient.AddNew();
			productParam.W3_OH = data.Org1.PK;
			productParam.W3_WW = data.Whs1.PK;
			productParam.W3_WL_InwardsProcessingStagingLocationBOM = location.PK;

			Helper.CreatePickNew(workOrder);
			Factory.Save();

			var expectedDbHits = new Dictionary<string, int>()
			{
				{ RefUNLOCOSchema.Constants.TableName, 1 },
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
				{ StmALogSchema.Constants.TableName, 1 },
				{ ProcessTasksSchema.Constants.TableName, 1 },
				{ WhsAreaSchema.Constants.TableName, 1 },
				{ WhsDocketSchema.Constants.TableName, 1 },
				{ WhsDocketContainerSchema.Constants.TableName, 1 },
				{ WhsDocketLineSchema.Constants.TableName, 4 },
				{ WhsDocketPalletSchema.Constants.TableName, 1 },
				{ WhsDocketReferenceSchema.Constants.TableName, 1 },
				{ WhsPickSchema.Constants.TableName, 1 },
				{ WhsPickLineSchema.Constants.TableName, 1 },
				{ WhsWarehouseSchema.Constants.TableName, 1 },
				{ WhsBondedWarehouseAttributeSchema.Constants.TableName, 1 },
				{ OrgContactSchema.Constants.TableName, 1 },
			};

			var newFactory = new BusinessObjectFactory();
			var workOrderInNewFactory = newFactory.Load<WhsDynamicWorkOrder>(workOrder.PK);

			using (AssertDbHitsWithUsefulQueryInformation(expectedDbHits, newFactory))
			using (WarehouseDataRegistry.Instance.TotalUnitsValidation.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true))
			{
				AssertEquals("Precondition", 0m, workOrderInNewFactory.WD_TotalUnits);
				workOrderInNewFactory.FinaliseDocketAlwaysFinalisingPick();

				AssertEquals("Dynamic Work order is not finalised.", false, workOrderInNewFactory.IsFinalised);
				AssertHasError(workOrderInNewFactory.WD_TotalUnitsInfo, "Total Units 0.000 does not equal the total of all assembly line units (including secondary products) 55.000.");
			}
		}

		public void TestTotalUnitsValidation_Assembly_IncludesSecondaryProducts()
		{
			TestTotalUnitsValidation_Assembly_IncludesSecondaryProductsCore(isTotalUnitsValidationEnabled: false, 0m);
		}

		public void TestTotalUnitsValidation_Assembly_IncludesSecondaryProducts_TotalUnitsValidationEnabled_NoValue()
		{
			TestTotalUnitsValidation_Assembly_IncludesSecondaryProductsCore(isTotalUnitsValidationEnabled: true, 0m);
		}

		public void TestTotalUnitsValidation_Assembly_IncludesSecondaryProducts_TotalUnitsValidationEnabled_GreaterThanValue()
		{
			TestTotalUnitsValidation_Assembly_IncludesSecondaryProductsCore(isTotalUnitsValidationEnabled: true, 20m);
		}

		public void TestTotalUnitsValidation_Assembly_IncludesSecondaryProducts_TotalUnitsValidationEnabled_EqualValue()
		{
			TestTotalUnitsValidation_Assembly_IncludesSecondaryProductsCore(isTotalUnitsValidationEnabled: true, 15m);
		}

		void TestTotalUnitsValidation_Assembly_IncludesSecondaryProductsCore(bool isTotalUnitsValidationEnabled, decimal totalValue)
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.EnableWarehouseForBond(data.Whs1, true);
			data.Whs1.WW_IsVirtualWarehouse = true;
			var secondaryPart1 = Helper.CreateProduct("P3", data.Org1);
			var secondaryPart2 = Helper.CreateProduct("P4", data.Org1);

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

			var workOrder = Helper.CreateWhsDynamicWorkOrder(data.Org1, data.Whs1, "ExtRef");
			workOrder.WD_DocketSubType = WorkOrderType.Codes.Assemble;
			workOrder.WD_RequiredDate = ZDateTimeOffset.Now;
			workOrder.WD_IsInwardsProcessingJob = true;

			var workOrderLineMain = Helper.CreateWhsDynamicWorkOrderLine(workOrder, data.Part2, 10m);
			workOrderLineMain.CustomsData.WB_IsMainInwardsProcessedItem = true;
			var workOrderLineMainComp = Helper.CreateWhsDynamicWorkOrderLine(workOrder, data.Part1, 10m);
			workOrderLineMainComp.WE_WE_ParentDocketLine = workOrderLineMain.PK;

			var workOrderLineSecondary1 = Helper.CreateWhsDynamicWorkOrderLine(workOrder, secondaryPart1, 2m);
			workOrderLineSecondary1.CustomsData.WB_IsSecondaryInwardsProcessedItem = true;
			var workOrderLineSecondaryComp1 = Helper.CreateWhsDynamicWorkOrderLine(workOrder, data.Part1, 2m);
			workOrderLineSecondaryComp1.WE_WE_ParentDocketLine = workOrderLineSecondary1.PK;

			var workOrderLineSecondary2 = Helper.CreateWhsDynamicWorkOrderLine(workOrder, secondaryPart2, 3m);
			workOrderLineSecondary2.CustomsData.WB_IsSecondaryInwardsProcessedItem = true;
			var workOrderLineSecondaryComp2 = Helper.CreateWhsDynamicWorkOrderLine(workOrder, data.Part1, 3m);
			workOrderLineSecondaryComp2.WE_WE_ParentDocketLine = workOrderLineSecondary2.PK;

			var productParam = workOrder.Lines[0].Product.ParamsByWhsAndClient.AddNew();
			productParam.W3_OH = data.Org1.PK;
			productParam.W3_WW = data.Whs1.PK;
			productParam.W3_WL_InwardsProcessingStagingLocationBOM = location.PK;

			Helper.CreatePickNew(workOrder);
			Factory.Save();

			using (WarehouseDataRegistry.Instance.TotalUnitsValidation.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, isTotalUnitsValidationEnabled))
			{
				workOrder.WD_TotalUnits = totalValue;
				AssertEquals("Precondition", totalValue, workOrder.WD_TotalUnits);
				workOrder.FinaliseDocketAlwaysFinalisingPick();

				if (isTotalUnitsValidationEnabled && totalValue != 15m)
				{
					AssertEquals("Dynamic Work order is not finalised.", false, workOrder.IsFinalised);
					AssertHasError(workOrder.WD_TotalUnitsInfo, $"Total Units {totalValue} does not equal the total of all assembly line units (including secondary products) 15.");
				}
				else
				{
					AssertIsFinalisedPrecondition(workOrder);
				}
			}
		}

		#endregion

		#region TestTotalUnitsValidation_Disassembly

		[GuiTest]
		public void TestTotalUnitsValidation_Disassembly()
		{
			TestTotalUnitsValidation_DisassemblyCore(isTotalUnitsValidationEnabled: false, 0m);
		}

		[GuiTest]
		public void TestTotalUnitsValidation_Disassembly_TotalUnitsValidationEnabled_NoValue()
		{
			TestTotalUnitsValidation_DisassemblyCore(isTotalUnitsValidationEnabled: true, 0m);
		}

		[GuiTest]
		public void TestTotalUnitsValidation_Disassembly_TotalUnitsValidationEnabled_GreaterThanValue()
		{
			TestTotalUnitsValidation_DisassemblyCore(isTotalUnitsValidationEnabled: true, 8m);
		}

		[GuiTest]
		public void TestTotalUnitsValidation_Disassembly_TotalUnitsValidationEnabled_EqualValue()
		{
			TestTotalUnitsValidation_DisassemblyCore(isTotalUnitsValidationEnabled: true, 15m);
		}

		void TestTotalUnitsValidation_DisassemblyCore(bool isTotalUnitsValidationEnabled, decimal totalUnits)
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.EnableWarehouseForBond(data.Whs1, true);
			data.Whs1.WW_IsVirtualWarehouse = true;

			var area = Helper.CreateArea(data.Whs1, "IPR", AreaTypes.Codes.InwardProcessing);
			var row = Helper.CreateRowAndGenerateLocations(data.Whs1, "I");
			var location = row.Locations.Single();
			location.WLV_WA_PutawayArea = area.PK;
			location.WLV_WA_PickingArea = area.PK;

			var mainProduct = Helper.CreateProduct("BIKE", data.Org1);
			var componentProduct1 = Helper.CreateProduct("WHEEL", data.Org1);
			var componentProduct2 = Helper.CreateProduct("ENGINE", data.Org1);
			Factory.Save();

			var component1Receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, $"R1", componentProduct1, 10m, $"BEK-1", allocateLocations: false, finalise: false);
			component1Receive.WD_DocketSubType = ReceiveType.Codes.Customs;
			component1Receive.WD_IsInwardsProcessingJob = true;
			component1Receive.Lines[0].WE_WL = location.PK;
			component1Receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(component1Receive);

			var component2Receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, $"R2", componentProduct2, 10m, $"BEK-2", allocateLocations: false, finalise: false);
			component2Receive.WD_DocketSubType = ReceiveType.Codes.Customs;
			component2Receive.WD_IsInwardsProcessingJob = true;
			component2Receive.Lines[0].WE_WL = location.PK;
			component2Receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(component2Receive);
			Factory.Save();

			var dynamicWorkOrder1 = Helper.CreateWhsDynamicWorkOrder(data.Org1, data.Whs1, "D1");
			dynamicWorkOrder1.WD_RequiredDate = ZDateTimeOffset.Now;
			dynamicWorkOrder1.WD_IsInwardsProcessingJob = true;

			var workOrderLineMain = Helper.CreateWhsDynamicWorkOrderLine(dynamicWorkOrder1, mainProduct, 5m);
			workOrderLineMain.CustomsData.WB_IsMainInwardsProcessedItem = true;

			var workOrderLineMainComp1 = Helper.CreateWhsDynamicWorkOrderLine(dynamicWorkOrder1, componentProduct1, 5m);
			workOrderLineMainComp1.WE_WE_ParentDocketLine = workOrderLineMain.PK;

			var workOrderLineMainComp2 = Helper.CreateWhsDynamicWorkOrderLine(dynamicWorkOrder1, componentProduct2, 10m);
			workOrderLineMainComp2.WE_WE_ParentDocketLine = workOrderLineMain.PK;

			Helper.CreatePickNew(dynamicWorkOrder1);
			dynamicWorkOrder1.FinaliseDocketAlwaysFinalisingPick();
			Factory.Save();

			var dynamicWorkOrder2 = Helper.CreateWhsDynamicWorkOrder(data.Org1, data.Whs1, "D2");
			dynamicWorkOrder2.WD_DocketSubType = DynamicWorkOrderType.Codes.Disassemble;
			dynamicWorkOrder2.WD_RequiredDate = ZDateTimeOffset.Now;
			dynamicWorkOrder2.WD_IsInwardsProcessingJob = true;

			var disassemblyLine = Helper.CreateWhsDynamicWorkOrderLine(dynamicWorkOrder2, mainProduct, 5m);
			disassemblyLine.CustomsData.WB_IsMainInwardsProcessedItem = true;

			Helper.CreatePickNew(dynamicWorkOrder2);

			using (WarehouseDataRegistry.Instance.TotalUnitsValidation.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, isTotalUnitsValidationEnabled))
			{
				dynamicWorkOrder2.WD_TotalUnits = totalUnits;
				AssertEquals("Precondition", totalUnits, dynamicWorkOrder2.WD_TotalUnits);
				dynamicWorkOrder2.FinaliseDocketAlwaysFinalisingPick();

				if (isTotalUnitsValidationEnabled && totalUnits != 15m)
				{
					AssertEquals("Dynamic Work order is not finalised.", false, dynamicWorkOrder2.IsFinalised);
					AssertHasError(dynamicWorkOrder2.WD_TotalUnitsInfo, $"Total Units {totalUnits} does not equal the total of all components: 15.");
				}
				else
				{
					Assert("Dynamic Work order is finalised.", dynamicWorkOrder2.IsFinalised);
				}
			}
		}

		[GuiTest]
		public void TestTotalUnitsValidation_Disassembly_PartialDisassembly_CorrectTotal()
		{
			TestTotalUnitsValidation_Disassembly_PartialDisassemblyCore(invalidTotal: false);
		}

		[GuiTest]
		public void TestTotalUnitsValidation_Disassembly_PartialDisassembly_IncorrectTotal()
		{
			TestTotalUnitsValidation_Disassembly_PartialDisassemblyCore(invalidTotal: true);
		}

		void TestTotalUnitsValidation_Disassembly_PartialDisassemblyCore(bool invalidTotal)
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.EnableWarehouseForBond(data.Whs1, true);
			data.Whs1.WW_IsVirtualWarehouse = true;

			var area = Helper.CreateArea(data.Whs1, "IPR", AreaTypes.Codes.InwardProcessing);
			var row = Helper.CreateRowAndGenerateLocations(data.Whs1, "I");
			var location = row.Locations.Single();
			location.WLV_WA_PutawayArea = area.PK;
			location.WLV_WA_PickingArea = area.PK;

			var mainProduct = Helper.CreateProduct("BIKE", data.Org1);
			var componentProduct1 = Helper.CreateProduct("WHEEL", data.Org1);
			var componentProduct2 = Helper.CreateProduct("ENGINE", data.Org1);
			Factory.Save();

			var component1Receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, $"R1", componentProduct1, 20m, $"BEK-1", allocateLocations: false, finalise: false);
			component1Receive.WD_DocketSubType = ReceiveType.Codes.Customs;
			component1Receive.WD_IsInwardsProcessingJob = true;
			component1Receive.Lines[0].WE_WL = location.PK;
			component1Receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(component1Receive);

			var component2Receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, $"R2", componentProduct2, 20m, $"BEK-2", allocateLocations: false, finalise: false);
			component2Receive.WD_DocketSubType = ReceiveType.Codes.Customs;
			component2Receive.WD_IsInwardsProcessingJob = true;
			component2Receive.Lines[0].WE_WL = location.PK;
			component2Receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(component2Receive);
			Factory.Save();

			var dynamicWorkOrder1 = Helper.CreateWhsDynamicWorkOrder(data.Org1, data.Whs1, "D1");
			dynamicWorkOrder1.WD_RequiredDate = ZDateTimeOffset.Now;
			dynamicWorkOrder1.WD_IsInwardsProcessingJob = true;

			var workOrderLineMain = Helper.CreateWhsDynamicWorkOrderLine(dynamicWorkOrder1, mainProduct, 10m);
			workOrderLineMain.CustomsData.WB_IsMainInwardsProcessedItem = true;

			var workOrderLineMainComp1 = Helper.CreateWhsDynamicWorkOrderLine(dynamicWorkOrder1, componentProduct1, 10m);
			workOrderLineMainComp1.WE_WE_ParentDocketLine = workOrderLineMain.PK;

			var workOrderLineMainComp2 = Helper.CreateWhsDynamicWorkOrderLine(dynamicWorkOrder1, componentProduct2, 20m);
			workOrderLineMainComp2.WE_WE_ParentDocketLine = workOrderLineMain.PK;

			Helper.CreatePickNew(dynamicWorkOrder1);
			dynamicWorkOrder1.FinaliseDocketAlwaysFinalisingPick();
			Factory.Save();

			var dynamicWorkOrder2 = Helper.CreateWhsDynamicWorkOrder(data.Org1, data.Whs1, "D2");
			dynamicWorkOrder2.WD_DocketSubType = DynamicWorkOrderType.Codes.Disassemble;
			dynamicWorkOrder2.WD_RequiredDate = ZDateTimeOffset.Now;
			dynamicWorkOrder2.WD_IsInwardsProcessingJob = true;

			var disassemblyLine = Helper.CreateWhsDynamicWorkOrderLine(dynamicWorkOrder2, mainProduct, 5m);
			disassemblyLine.CustomsData.WB_IsMainInwardsProcessedItem = true;

			Helper.CreatePickNew(dynamicWorkOrder2);

			using (WarehouseDataRegistry.Instance.TotalUnitsValidation.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true))
			{
				dynamicWorkOrder2.WD_TotalUnits = invalidTotal ? 2m : 15m;
				dynamicWorkOrder2.FinaliseDocketAlwaysFinalisingPick();

				if (invalidTotal)
				{
					AssertEquals("Dynamic Work order is not finalised.", false, dynamicWorkOrder2.IsFinalised);
					AssertHasError(dynamicWorkOrder2.WD_TotalUnitsInfo, $"Total Units 2 does not equal the total of all components: 15.");
				}
				else
				{
					Assert("Dynamic Work order is finalised.", dynamicWorkOrder2.IsFinalised);
				}
			}
		}

		#endregion

		#endregion

		#region TestConsigneeNameOrPKValidation

		protected override void TestConsigneeNameOrPKValidationCore()
		{
			var docket = GetNewBusinessObject();
			var consignee = Factory.New<OrgHeader>();
			consignee.OH_IsConsignee = true;

			docket.ConsigneeDocAddress.E2_AddressOverride = false;
			docket.Validation.ValidateConsigneeNameOrPK();
			AssertNoErrors(docket.ConsigneeNameOrPKInfo);

			docket.ConsigneeDocAddress.OrganisationPK = consignee.PK;
			docket.Validation.ValidateConsigneeNameOrPK();
			AssertNoErrors(docket.ConsigneeNameOrPKInfo);

			docket.ConsigneeDocAddress.E2_AddressOverride = true;
			docket.ConsigneeDocAddress.E2_CompanyName = "Blah";
			docket.Validation.ValidateConsigneeNameOrPK();
			AssertNoErrors(docket.ConsigneeNameOrPKInfo);

			docket.ConsigneeDocAddress.E2_CompanyName = "";
			docket.Validation.ValidateConsigneeNameOrPK();
			AssertNoErrors(docket.ConsigneeNameOrPKInfo);
		}

		#endregion

		#region TestTransportCoNameOrPKValidation

		protected override void TestTransportCoNameOrPKValidationCore()
		{
			var docket = GetNewBusinessObject();
			var job = (IJobWithTransportCompany)docket;
			var transportCo = Factory.New<OrgHeader>();
			transportCo.OH_IsTransportClient = true;

			job.TransportCoDocAddress.E2_AddressOverride = false;
			ValidateTransportCoNameOrPK(docket);
			AssertNoErrors(job.TransportCoNameOrPKInfo);

			job.TransportCoDocAddress.OrganisationPK = transportCo.PK;
			ValidateTransportCoNameOrPK(docket);
			AssertNoErrors(job.TransportCoNameOrPKInfo);

			job.TransportCoDocAddress.E2_AddressOverride = true;
			job.TransportCoDocAddress.E2_CompanyName = "Blah";
			ValidateTransportCoNameOrPK(docket);
			AssertNoErrors(job.TransportCoNameOrPKInfo);

			job.TransportCoDocAddress.E2_CompanyName = "";
			ValidateTransportCoNameOrPK(docket);
			AssertNoErrors(job.TransportCoNameOrPKInfo);
		}

		#endregion

		#region AssertValidationForDocketTotalVsLinesTotal

		public void TestValidationForDocketTotalVsLinesTotal_WithSecondaryLines()
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

			var setup = DynamicWorkOrderTestSetup.CreateDynamicWorkOrderForTest(Helper, data.Org1, data.Whs1);
			var workOrder = setup.WorkOrder;

			var expectedWarningPrefix = "Original {0} of product calculated from the product master file was";
			AssertEquals(false, workOrder.WD_TotalWeightInfo.Notifications.Any(n => n.Message.Contains(string.Format(expectedWarningPrefix, "Weight"))));
			AssertEquals(false, workOrder.WD_TotalCubicInfo.Notifications.Any(n => n.Message.Contains(string.Format(expectedWarningPrefix, "Volume"))));

			workOrder.WD_TotalWeight = 5m;
			AssertEquals(true, workOrder.WD_TotalWeightInfo.Notifications.Any(n => n.Message.Contains(string.Format(expectedWarningPrefix, "Weight"))));
			AssertEquals(false, workOrder.WD_TotalCubicInfo.Notifications.Any(n => n.Message.Contains(string.Format(expectedWarningPrefix, "Volume"))));

			workOrder.WD_TotalCubic = 5m;
			AssertEquals(true, workOrder.WD_TotalWeightInfo.Notifications.Any(n => n.Message.Contains(string.Format(expectedWarningPrefix, "Weight"))));
			AssertEquals(true, workOrder.WD_TotalCubicInfo.Notifications.Any(n => n.Message.Contains(string.Format(expectedWarningPrefix, "Volume"))));
		}

		public void TestValidationForDocketTotalVsLinesTotal_WithSplitLines()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetProductWeightAndVolume(data.Part1, 5m, "KG", 2m, "M3");
			var part3 = Helper.CreateProduct("P3", data.Org1);
			Helper.EnableWarehouseForBond(data.Whs1, true);
			data.Whs1.WW_IsVirtualWarehouse = true;

			var area = Helper.CreateArea(data.Whs1, "IPR", AreaTypes.Codes.InwardProcessing);
			var row = Helper.CreateRowAndGenerateLocations(data.Whs1, "I");
			var location = row.Locations.Single();
			location.WLV_WA_PutawayArea = area.PK;
			location.WLV_WA_PickingArea = area.PK;

			var componentReceive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			componentReceive1.WD_DocketSubType = ReceiveType.Codes.Customs;
			componentReceive1.WD_IsInwardsProcessingJob = true;
			Helper.CreateWhsReceiveInventoryLine(componentReceive1, data.Part1.PK, 8m, data.Whs1.DefaultLocationInInwardProcessingArea.PK, palletID: "", entryKey: "ENT-1");
			componentReceive1.FinaliseDocket();
			AssertIsFinalisedPrecondition(componentReceive1);

			var componentReceive2 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R2", Notify);
			componentReceive2.WD_DocketSubType = ReceiveType.Codes.Customs;
			componentReceive2.WD_IsInwardsProcessingJob = true;
			Helper.CreateWhsReceiveInventoryLine(componentReceive2, data.Part1.PK, 4m, data.Whs1.DefaultLocationInInwardProcessingArea.PK, palletID: "", entryKey: "ENT-2");
			componentReceive2.FinaliseDocket();
			AssertIsFinalisedPrecondition(componentReceive2);
			Factory.Save();

			var workOrder = Helper.CreateWhsDynamicWorkOrderWithLine(data.Org1, data.Whs1, "ExtRef", data.Part2, 6m);
			workOrder.WD_DocketSubType = WorkOrderType.Codes.Assemble;
			workOrder.WD_RequiredDate = ZDateTimeOffset.Now;

			var mainLine = workOrder.Lines[0];
			mainLine.IsMainInwardProcessedItem = true;

			var componentLine = Helper.CreateWhsDynamicWorkOrderLine(workOrder, data.Part1, 0m);
			mainLine.ChildComponentLinesCollection.Add(componentLine);
			componentLine.WE_TransactionQuantity = 12m;

			var secondaryLine = Helper.CreateWhsDynamicWorkOrderLine(workOrder, part3, 0m);
			secondaryLine.IsSecondaryInwardProcessedItem = true;
			secondaryLine.WE_TransactionQuantity = 3m;

			var secondaryComponentLine = Helper.CreateWhsDynamicWorkOrderLine(workOrder, data.Part1, 0m);
			secondaryLine.ChildComponentLinesCollection.Add(secondaryComponentLine);
			secondaryComponentLine.WE_TransactionQuantity = 6m;

			workOrder.RunPreSaveValidation();
			Helper.CreatePickNew(workOrder);

			workOrder.FinaliseDocketAlwaysFinalisingPick();

			var expectedWarningPrefix = "Original {0} of product calculated from the product master file was";
			AssertEquals(false, workOrder.WD_TotalWeightInfo.Notifications.Any(n => n.Message.Contains(string.Format(expectedWarningPrefix, "Weight"))));
			AssertEquals(false, workOrder.WD_TotalCubicInfo.Notifications.Any(n => n.Message.Contains(string.Format(expectedWarningPrefix, "Volume"))));

			workOrder.WD_TotalWeight = 5m;
			AssertEquals(true, workOrder.WD_TotalWeightInfo.Notifications.Any(n => n.Message.Contains(string.Format(expectedWarningPrefix, "Weight"))));
			AssertEquals(false, workOrder.WD_TotalCubicInfo.Notifications.Any(n => n.Message.Contains(string.Format(expectedWarningPrefix, "Volume"))));

			workOrder.WD_TotalCubic = 5m;
			AssertEquals(true, workOrder.WD_TotalWeightInfo.Notifications.Any(n => n.Message.Contains(string.Format(expectedWarningPrefix, "Weight"))));
			AssertEquals(true, workOrder.WD_TotalCubicInfo.Notifications.Any(n => n.Message.Contains(string.Format(expectedWarningPrefix, "Volume"))));
		}

		protected override void AddLineForCheckWD_TotalWeightOrCubicValidation(WhsDocket docket, OrgSupplierPart part, ZDecimal units)
		{
			var line1 = (WhsDynamicWorkOrderLine)docket.Lines.AddNew();
			line1.IsMainInwardProcessedItem = true;

			var childLine = (WhsDynamicWorkOrderLine)docket.Lines.AddNew();
			childLine.WE_WE_ParentDocketLine = line1.PK;
			childLine.WE_OP = part.PK;
			childLine.WE_TransactionQuantity = units;
		}

		protected override IEnumerable<WhsDocketLine> GetLinesForWeightAndVolumeCalculation(WhsDynamicWorkOrder docket)
		{
			IEnumerable<WhsDocketLine> lines;

			if (docket.IsAssembly)
			{
				lines = docket
				.AllLines
				.Cast<WhsDynamicWorkOrderLine>()
				.Where(l => !l.WE_WE_ParentDocketLine.IsEmpty && (l.ParentLine?.IsMainInwardProcessedItem ?? false));
			}
			else
			{
				lines = docket.Lines;
			}

			return lines;
		}

		public void TestValidationForDocketTotalVsLinesTotal_ForDisassembly()
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

			var product = Helper.CreateProduct("P1", data.Org1);

			Helper.SetProductWeightAndVolume(product, 3m, "KG", 7m, "M3");

			var dynamicWorkOrder = Helper.CreateWhsDynamicWorkOrder(data.Org1, data.Whs1);
			dynamicWorkOrder.WD_RequiredDate = ZDateTimeOffset.Now;
			dynamicWorkOrder.WD_DocketSubType = DynamicWorkOrderType.Codes.Disassemble;

			Helper.CreateWhsDynamicWorkOrderLine(dynamicWorkOrder, product, 5m);

			dynamicWorkOrder.RunPreSaveValidation();

			var expectedWarningPrefix = "Original {0} of product calculated from the product master file was";
			AssertEquals(false, dynamicWorkOrder.WD_TotalWeightInfo.Notifications.Any(n => n.Message.Contains(string.Format(expectedWarningPrefix, "Weight"))));
			AssertEquals(false, dynamicWorkOrder.WD_TotalCubicInfo.Notifications.Any(n => n.Message.Contains(string.Format(expectedWarningPrefix, "Volume"))));

			dynamicWorkOrder.WD_TotalWeight = 29m;
			AssertEquals(true, dynamicWorkOrder.WD_TotalWeightInfo.Notifications.Any(n => n.Message.Contains(string.Format(expectedWarningPrefix, "Weight"))));
			AssertEquals(false, dynamicWorkOrder.WD_TotalCubicInfo.Notifications.Any(n => n.Message.Contains(string.Format(expectedWarningPrefix, "Volume"))));

			dynamicWorkOrder.WD_TotalCubic = 41m;
			AssertEquals(true, dynamicWorkOrder.WD_TotalWeightInfo.Notifications.Any(n => n.Message.Contains(string.Format(expectedWarningPrefix, "Weight"))));
			AssertEquals(true, dynamicWorkOrder.WD_TotalCubicInfo.Notifications.Any(n => n.Message.Contains(string.Format(expectedWarningPrefix, "Volume"))));
		}

		#endregion

		#region Implementation

		protected override IEnumerable<string> ValidSubTypeForInwardProcessing => new[] { WorkOrderType.Codes.Assemble };

		#endregion
	}
}
