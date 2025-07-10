using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.GUI;
using Enterprise.MasterFiles.Integration;
using Enterprise.Warehouse.Integration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	[TestedType(typeof(OrgSupplierPartFilterStripBusinessObject))]
	public class OrgSupplierPartFilterStripBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		public void TestCommodityCode()
		{
			RefCommodityCode commodityCode1 = Factory.New<RefCommodityCode>();
			commodityCode1.RH_Code = "COM1";

			RefCommodityCode commodityCode2 = Factory.New<RefCommodityCode>();
			commodityCode1.RH_Code = "COM2";

			OrgSupplierPart part1 = Factory.New<OrgSupplierPart>();
			part1.OP_RH_NKCommodityCode = commodityCode1.RH_Code;
			part1.OP_PartNum = "Part1";

			OrgSupplierPart part2 = Factory.New<OrgSupplierPart>();
			part2.OP_RH_NKCommodityCode = commodityCode1.RH_Code;
			part2.OP_PartNum = "Part2";

			OrgSupplierPart part3 = Factory.New<OrgSupplierPart>();
			part3.OP_RH_NKCommodityCode = commodityCode2.RH_Code;
			part3.OP_PartNum = "Part3";

			Factory.Save();

			OrgSupplierPartFilterStripBusinessObject filterStrip = new OrgSupplierPartFilterStripBusinessObject();
			filterStrip["Commodity Code"].IsActive = true;
			((ModuleNkFilter)filterStrip["Commodity Code"]).Property = commodityCode1.RH_Code;

			OrgSupplierPartCollection collection = new OrgSupplierPartCollection(Factory);
			collection.Load(filterStrip.Filter);

			AssertEquals(2, collection.Count);
			AssertCollectionContains(part1, collection);
			AssertCollectionContains(part2, collection);
			AssertCollectionNotContains(part3, collection);
		}

		public void TestForResale()
		{
			OrgSupplierPart partResellable = Factory.New<OrgSupplierPart>();
			partResellable.OP_CanResell = true;
			OrgSupplierPart partNonresellable = Factory.New<OrgSupplierPart>();
			partNonresellable.OP_CanResell = false;

			OrgSupplierPartFilterStripBusinessObject filterStrip = new OrgSupplierPartFilterStripBusinessObject();
			((ModuleFlagsFilter)filterStrip["For Resale"]).IsActive = true;
			OrgSupplierPartCollection collection = new OrgSupplierPartCollection(Factory);

			((ModuleFlagsFilter)filterStrip["For Resale"]).Property0 = true;
			collection.Load(filterStrip.Filter);
			AssertCollectionContains(partResellable, collection);
			AssertCollectionNotContains(partNonresellable, collection);

			((ModuleFlagsFilter)filterStrip["For Resale"]).Property0 = false;
			collection.Load(filterStrip.Filter);
			AssertCollectionContains(partResellable, collection);
			AssertCollectionContains(partNonresellable, collection);
		}

		public void TestLocalPartNumberQuery()
		{
			var product1 = Factory.NewWithValidTestData<OrgSupplierPart>();
			var header1 = Factory.NewWithValidTestData<OrgHeader>();
			var relation1 = product1.RelatedOrganisations.AddNew();
			relation1.OU_LocalPartNumber = "LOCAL";
			relation1.OU_OH = header1.PK;

			var product2 = Factory.NewWithValidTestData<OrgSupplierPart>();
			var header2 = Factory.NewWithValidTestData<OrgHeader>();
			var relation2 = product2.RelatedOrganisations.AddNew();
			relation2.OU_LocalPartNumber = "TESTLOCAL";
			relation2.OU_OH = header2.PK;
			Factory.Save();

			var collection = GetFilteredCollection(OrgSupplierPartFilterStripBusinessObject.Descriptions.LocalPartNo, "LOCAL");
			AssertEquals(1, collection.Count);
			Assert(collection.Contains(product1));
		}

		public void TestImporterSupplierIsNotVisibleOnWeb()
		{
			var filterStrip = new OrgSupplierPartFilterStripBusinessObject();
			Assert("Importer/Supplier filter is not visible on web", !filterStrip["Importer/Supplier"].IsPublishedOnWeb);
		}

		public void TestImporterSupplierFilter()
		{
			AssertEquals("Precondition: WhsAllowedClients", true, Env.Security.WhsAllowedClients.IsAllowed);
			OrgSupplierPartFilterStripBusinessObject orgSupplierPartFilter = new OrgSupplierPartFilterStripBusinessObject();
			ImporterSupplierModuleGuidsWithListFilter filter = (ImporterSupplierModuleGuidsWithListFilter)orgSupplierPartFilter["Importer/Supplier"];
			AssertEquals("Visibility", FilterVisibility.Visible, filter.Visibility);
			AssertEquals("Default Filter Condition should be 'Both'", ImporterSuplierFilterConditions.Codes.Both, filter.FilterCondition);

			Env.Security.WhsAllowedClients.IsAllowed = false;
			try
			{
				OrgSupplierPartFilterStripBusinessObject orgSupplierPartFilter1 = new OrgSupplierPartFilterStripBusinessObject();
				ImporterSupplierModuleGuidsWithListFilter filter1 = (ImporterSupplierModuleGuidsWithListFilter)orgSupplierPartFilter1["Importer/Supplier"];
				AssertEquals("Should be visible not always visible", FilterVisibility.Visible, filter1.Visibility);

				OrgSupplierPartFilterStripBusinessObject whsorgSupplierPartFilter = new OrgSupplierPartFilterStripBusinessObject();
				((IFilterStripBusinessObjectInternals)whsorgSupplierPartFilter).LayoutContext = ModuleIDs.WhsConfigProduct.Name;
				ImporterSupplierModuleGuidsWithListFilter filter2 = (ImporterSupplierModuleGuidsWithListFilter)whsorgSupplierPartFilter["Importer/Supplier"];
				AssertEquals("Should now be always visible", FilterVisibility.AlwaysVisible, filter2.Visibility);

				Globals.IsWeb = true;
				try
				{
					OrgSupplierPartFilterStripBusinessObject orgSupplierPartFilter3 = new OrgSupplierPartFilterStripBusinessObject();
					ImporterSupplierModuleGuidsWithListFilter filter3 = (ImporterSupplierModuleGuidsWithListFilter)orgSupplierPartFilter3["Importer/Supplier"];
					AssertEquals("Visibility", FilterVisibility.Visible, filter3.Visibility);
				}
				finally
				{
					Globals.IsWeb = false;
				}
			}
			finally
			{
				Env.Security.WhsAllowedClients.IsAllowed = true;
			}
		}

		#region TestABCCategoryWarehouseFilter

		public void TestABCCategoryWarehouseFilter()
		{
			var orgSupplierPartFilter = new OrgSupplierPartFilterStripBusinessObject();
			var filter = (ABCCategoryWarehouseFilter)orgSupplierPartFilter["ABC Category / Warehouse"];
			AssertEquals("Visibility", FilterVisibility.Visible, filter.Visibility);
			AssertEquals("Filter Category", FilterCategories.Other, filter.Category);
		}

		#endregion

		public void TestConsigneesAndWarehouses()
		{
			OrgHeader consignee = Factory.New<OrgHeader>();
			consignee.OH_Code = "TSTCONSG";
			consignee.OH_IsConsignee = true;

			OrgHeader warehouse = Factory.New<OrgHeader>();
			warehouse.OH_Code = "TSTWARE";
			warehouse.OH_IsWarehouseClient = true;
			Factory.Save();
			OrgSupplierPartFilterStripBusinessObject filterStrip = new OrgSupplierPartFilterStripBusinessObject();
			OrgHeaderCollection collection = filterStrip.ConsigneesAndWarehouses;
			collection.Load();
			Assert("Consignee is in collection", collection.Contains(consignee));
			Assert("Warehouse is in collection", collection.Contains(warehouse));
		}

		public void TestAddBOMFilters()
		{
			var filterStrip = new OrgSupplierPartFilterStripBusinessObject();
			var parentPartFilter = (ModuleTextFilter)filterStrip["Parent Part"];
			AssertEquals(1, parentPartFilter.ComparisonOperator_List.Count);
			AssertEquals(ModuleTextFilter.ComparisonConstants.Exact, parentPartFilter.ComparisonOperator);

			filterStrip = new OrgSupplierPartFilterStripBusinessObject();
			var componentPartFilter = (ModuleTextFilter)filterStrip["Component Part"];
			AssertEquals(3, componentPartFilter.ComparisonOperator_List.Count);
			AssertEquals(ModuleTextFilter.ComparisonConstants.Exact, componentPartFilter.ComparisonOperator_List[0].ToString());
			AssertEquals(ModuleTextFilter.ComparisonConstants.IsBlank, componentPartFilter.ComparisonOperator_List[1].ToString());
			AssertEquals(ModuleTextFilter.ComparisonConstants.IsNotBlank, componentPartFilter.ComparisonOperator_List[2].ToString());
		}

		public void TestBomFilters_ParentPart()
		{
			//							A
			//						/	
			//					B		
			//				/		\
			//			E				F
			//		/		\				\
			//	G				H				E
			//								/		\
			//							G				H

			ParentPart.OP_PartNum = "A";
			var b = CreateNewWithPart(ParentPart, "B");
			var e = CreateNewWithPart(b.Component, "E");
			var f = CreateNewWithPart(b.Component, "F");
			var g = CreateNewWithPart(e.Component, "G");
			var h = CreateNewWithPart(e.Component, "H");
			var repeatPart = f.Component.BillOfMaterials.AddNew();
			repeatPart.OE_OP_Component = e.Component.PK;
			Factory.Save();

			var collection = GetFilteredCollection("Parent Part", "H");

			AssertNotNull("H in collection", collection.FindByPK(h.Component.PK));
			AssertEquals(1, collection.Count);

			collection = GetFilteredCollection("Parent Part", "A");
			AssertNotNull("A in collection", collection.FindByPK(ParentPart.PK));
			AssertNotNull("B in collection", collection.FindByPK(b.Component.PK));
			AssertNotNull("E in collection", collection.FindByPK(e.Component.PK));
			AssertNotNull("F in collection", collection.FindByPK(f.Component.PK));
			AssertNotNull("G in collection", collection.FindByPK(g.Component.PK));
			AssertNotNull("H in collection", collection.FindByPK(h.Component.PK));
			AssertEquals(6, collection.Count);

			collection = GetFilteredCollection("Parent Part", "");
			AssertEquals(6, collection.Count);

			collection = GetFilteredCollection("Parent Part", "G");
			AssertNotNull("G in collection", collection.FindByPK(g.Component.PK));
			AssertEquals(1, collection.Count);

			collection = GetFilteredCollection("Parent Part", "F");
			AssertNotNull("E in collection", collection.FindByPK(e.Component.PK));
			AssertNotNull("G in collection", collection.FindByPK(g.Component.PK));
			AssertNotNull("H in collection", collection.FindByPK(h.Component.PK));
			AssertNotNull("F in collection", collection.FindByPK(f.Component.PK));
			AssertEquals(4, collection.Count);

			collection = GetFilteredCollection("Parent Part", "Q");
			AssertEquals(0, collection.Count);
		}

		public void TestBomFilters_ComponentPart_Exact()
		{
			//							A
			//						/	
			//					B		
			//				/		\
			//			E				F
			//		/		\				\
			//	G				H				E
			//								/		\
			//							G				H

			ParentPart.OP_PartNum = "A";
			var b = CreateNewWithPart(ParentPart, "B");
			var e = CreateNewWithPart(b.Component, "E");
			var f = CreateNewWithPart(b.Component, "F");
			var g = CreateNewWithPart(e.Component, "G");
			var h = CreateNewWithPart(e.Component, "H");
			var repeatPart = f.Component.BillOfMaterials.AddNew();
			repeatPart.OE_OP_Component = e.Component.PK;
			Factory.Save();

			var collection = GetFilteredCollection("Component Part", "H");

			AssertNotNull("E in collection", collection.FindByPK(e.Component.PK));
			AssertNotNull("F in collection", collection.FindByPK(f.Component.PK));
			AssertNotNull("B in collection", collection.FindByPK(b.Component.PK));
			AssertNotNull("A in collection", collection.FindByPK(ParentPart.PK));
			AssertNotNull("H in collection", collection.FindByPK(h.Component.PK));
			AssertEquals(5, collection.Count);

			collection = GetFilteredCollection("Component Part", "A");

			AssertNotNull("A in collection", collection.FindByPK(ParentPart.PK));
			AssertEquals(1, collection.Count);

			collection = GetFilteredCollection("Component Part", "G");
			AssertNotNull("E in collection", collection.FindByPK(e.Component.PK));
			AssertNotNull("F in collection", collection.FindByPK(f.Component.PK));
			AssertNotNull("B in collection", collection.FindByPK(b.Component.PK));
			AssertNotNull("A in collection", collection.FindByPK(ParentPart.PK));
			AssertNotNull("G in collection", collection.FindByPK(g.Component.PK));
			AssertEquals(5, collection.Count);

			collection = GetFilteredCollection("Component Part", "F");
			AssertNotNull("B in collection", collection.FindByPK(b.Component.PK));
			AssertNotNull("A in collection", collection.FindByPK(ParentPart.PK));
			AssertNotNull("F in collection", collection.FindByPK(f.Component.PK));
			AssertEquals(3, collection.Count);

			collection = GetFilteredCollection("Component Part", "Q");
			AssertEquals(0, collection.Count);

			collection = GetFilteredCollection("Component Part", "");
			AssertEquals(6, collection.Count);//all
		}

		public void TestBomFilters_ComponentPart_IsBlankComparator()
		{
			//							A       F
			//						/	
			//					B		
			//				/		\
			//			C				D

			var a = Factory.New<OrgSupplierPart>();
			a.OP_PartNum = "A";
			a.OP_Desc = "A";

			var b = CreateNewWithPart(a, "B");
			var c = CreateNewWithPart(b.Component, "C");
			var d = CreateNewWithPart(b.Component, "D");

			var f = Factory.New<OrgSupplierPart>();
			f.OP_PartNum = "F";
			f.OP_Desc = "F";

			Factory.Save();

			var collection = GetFilteredCollectionWithComparisonOperator("Component Part", ModuleTextFilter.ComparisonConstants.IsBlank);

			AssertNotNull("C in collection", collection.FindByPK(c.Component.PK));
			AssertNotNull("D in collection", collection.FindByPK(d.Component.PK));
			AssertNotNull("F in collection", collection.FindByPK(f.PK));
			AssertEquals(3, collection.Count);
		}

		public void TestBomFilters_ComponentPart_IsNotBlankComparator()
		{
			//							A       F
			//						/	
			//					B		
			//				/		\
			//			C				D
			//		/			
			//	E					

			var a = Factory.New<OrgSupplierPart>();
			a.OP_PartNum = "A";
			a.OP_Desc = "A";

			var b = CreateNewWithPart(a, "B");
			var c = CreateNewWithPart(b.Component, "C");
			var d = CreateNewWithPart(b.Component, "D");
			var e = CreateNewWithPart(c.Component, "E");

			var f = Factory.New<OrgSupplierPart>();
			f.OP_PartNum = "F";
			f.OP_Desc = "F";

			Factory.Save();

			var collection = GetFilteredCollectionWithComparisonOperator("Component Part", ModuleTextFilter.ComparisonConstants.IsNotBlank);

			AssertNotNull("A in collection", collection.FindByPK(a.PK));
			AssertNotNull("B in collection", collection.FindByPK(b.Component.PK));
			AssertNotNull("C in collection", collection.FindByPK(c.Component.PK));
			AssertEquals(3, collection.Count);
		}

		[ExpectNoExceptions]
		public void TestCustomsJobCreatedFilter()
		{
			OrgSupplierPartFilterStripBusinessObject filterStrip = new OrgSupplierPartFilterStripBusinessObject();

			ModuleDateFilter dateFilter = (ModuleDateFilter)filterStrip["Customs Job Created"];
			dateFilter.IsActive = true;

			((ModuleDateFilter)filterStrip["Customs Job Created"]).Property1 = ZDateTime.UtcNow.AddDays(-2);
			((ModuleDateFilter)filterStrip["Customs Job Created"]).Property2 = ZDateTime.UtcNow;
			OrgSupplierPartCollection collection = new OrgSupplierPartCollection(Factory);
			collection.Load(filterStrip.Filter);

			dateFilter.PropertySearch = ModuleDateFilter.HasNoDateEntered;
			collection.Load(filterStrip.Filter);

			dateFilter.PropertySearch = ModuleDateFilter.HasDateEntered;
			collection.Load(filterStrip.Filter);
		}

		public void TestIsExpensiveQuery()
		{
			OrgSupplierPartFilterStripBusinessObject filterStrip = new OrgSupplierPartFilterStripBusinessObject();
			AssertEquals(false, filterStrip.IsExpensiveQuery);
		}

		#region Warehouse Job Created

		public void TestWarehouseJobCreated()
		{
			var notify = new NotificationBuffer();
			var helper = WhsTransactionTestHelperCreator.GetNewHelper(Factory);
			ZGuid clientPK = helper.CreateClient("Client");
			ZGuid whsPK = helper.CreateWarehouse("WHS", "A", 2, 1).PK;
			var part1 = helper.CreateProduct(clientPK, "P1");
			var part2 = helper.CreateProduct(clientPK, "P2");
			var part3 = helper.CreateProduct(clientPK, "P3");
			var part4 = helper.CreateProduct(clientPK, "P4");
			var part5 = helper.CreateProduct(clientPK, "P5");
			Factory.Save();

			ZGuid receivePK = helper.CreateWhsReceive(clientPK, whsPK, "R1", notify);
			helper.CreateWhsReceiveInventoryLine(receivePK, part1.PK, 10m, "A-1");

			ZGuid orderPK = helper.CreateWhsOrder(clientPK, whsPK, "OR1", notify);
			helper.CreateWhsOrderLine(orderPK, part2.PK, 10m);

			var adjustment = helper.CreateWhsAdjustment(clientPK, whsPK, "ADJ1", notify);
			helper.CreateWhsAdjustmentLine(adjustment.PK, part3.PK, 10m, "A-1");

			var emptyDate = ZDateTime.Empty;
			var someDate = ZDateTime.Today.AddDays(-7);
			SetCreatedDateForWhsJobs(receivePK, someDate.AddDays(1));
			SetCreatedDateForWhsJobs(orderPK, someDate.AddDays(2));
			SetCreatedDateForWhsJobs(adjustment.PK, someDate.AddDays(3));

			// create inventory for transfer to commit
			var receive2PK = helper.CreateWhsReceive(clientPK, whsPK, "R2", notify);
			helper.CreateWhsReceiveInventoryLine(receive2PK, part4.PK, 10m, "A-1");
			var receive2 = Factory.Load<IWhsDocket>(receive2PK);
			receive2.FinaliseDocketWithoutUserConfirmation();
			Factory.Save();

			var transferPK = helper.CreateWhsTransfer(clientPK, whsPK, "TR1", notify);
			helper.CreateWhsTransferLine(transferPK, part4.PK, 10m, "A-1", "A-2");
			SetCreatedDateForWhsJobs(transferPK, someDate.AddDays(4));
			Factory.Save();

			// If HasNoDateEntered then load only products from warehouse jobs with no CreatedDate entered (i.e. None)
			OrgSupplierPartCollection collection1 = GetFilteredCollection("Warehouse Job Created", ModuleDateFilter.HasNoDateEntered, emptyDate, emptyDate);
			AssertEquals(false, collection1.Contains(part1.PK));
			AssertEquals(false, collection1.Contains(part2.PK));
			AssertEquals(false, collection1.Contains(part3.PK));
			AssertEquals(false, collection1.Contains(part4.PK));
			AssertEquals(false, collection1.Contains(part5.PK));

			// If HasNoDateEntered then load only products from warehouse jobs with CreatedDate set
			OrgSupplierPartCollection collection2 = GetFilteredCollection("Warehouse Job Created", ModuleDateFilter.HasDateEntered, emptyDate, emptyDate);
			AssertEquals(true, collection2.Contains(part1.PK));
			AssertEquals(true, collection2.Contains(part2.PK));
			AssertEquals(true, collection2.Contains(part3.PK));
			AssertEquals(true, collection2.Contains(part4.PK));
			AssertEquals(false, collection2.Contains(part5.PK));

			// If only FromDate set
			OrgSupplierPartCollection collection3 = GetFilteredCollection("Warehouse Job Created", ModuleDateFilter.SpecifiedDateRange, someDate.AddDays(2), emptyDate);
			AssertEquals(false, collection3.Contains(part1.PK));
			AssertEquals(true, collection3.Contains(part2.PK));
			AssertEquals(true, collection3.Contains(part3.PK));
			AssertEquals(true, collection3.Contains(part4.PK));
			AssertEquals(false, collection3.Contains(part5.PK));

			// If only ToDate set
			OrgSupplierPartCollection collection4 = GetFilteredCollection("Warehouse Job Created", ModuleDateFilter.SpecifiedDateRange, emptyDate, someDate.AddDays(3));
			AssertEquals(true, collection4.Contains(part1.PK));
			AssertEquals(true, collection4.Contains(part2.PK));
			AssertEquals(true, collection4.Contains(part3.PK));
			AssertEquals(false, collection4.Contains(part4.PK));
			AssertEquals(false, collection4.Contains(part5.PK));

			// If DateRange set
			OrgSupplierPartCollection collection5 = GetFilteredCollection("Warehouse Job Created", ModuleDateFilter.SpecifiedDateRange, someDate.AddDays(2), someDate.AddDays(3));
			AssertEquals(false, collection5.Contains(part1.PK));
			AssertEquals(true, collection5.Contains(part2.PK));
			AssertEquals(true, collection5.Contains(part3.PK));
			AssertEquals(false, collection5.Contains(part4.PK));
			AssertEquals(false, collection5.Contains(part5.PK));
		}

		void SetCreatedDateForWhsJobs(ZGuid jobPK, ZDateTime createdDate)
		{
			var bizO = (BusinessObject)Factory.Load<IWhsDocket>(jobPK);
			bizO[WhsDocketSchema.WD_BookingDate.Name] = createdDate;
			bizO.RunPreSaveValidation(); // to commit inventory by transfers and adjustments
		}

		#endregion

		#region TestAddWorkflowCustomFieldsFilters

		public void TestAddWorkflowCustomFieldsFilters()
		{
			var collection = new OrgSupplierPartFilterStripBusinessObject().ModuleFilters;

			AssertNull(collection["Custom1 String"]);
			AssertNull(collection["Custom1 Integer"]);
			AssertNull(collection["Workflow Flags"]);
			AssertNull(collection["Custom2 Boolean"]);
			AssertNull(collection["Custom2 Datetime"]);
			AssertNull(collection["Unrelated Custom String"]);

			PrepareTemplatesWithCustomFields();

			collection = new OrgSupplierPartFilterStripBusinessObject().ModuleFilters;

			AssertEquals(typeof(ModuleTextFilter), collection["Custom1 String"].GetType());
			AssertEquals(typeof(ModuleNumberRangeFilter), collection["Custom1 Integer"].GetType());
			AssertEquals(typeof(ModuleFlagsFilter), collection["Workflow Flags"].GetType());
			AssertEquals(typeof(ModuleTextFilter), collection["Custom2 Boolean"].GetType());
			AssertEquals(typeof(ModuleDateFilter), collection["Custom2 Datetime"].GetType());
			AssertNull(collection["Unrelated Custom String"]);
		}

		void PrepareTemplatesWithCustomFields()
		{
			var template1 = CreateWorkflowTemplate(WorkflowDescriptors.OrgSupplierPartWorkflowDescriptorCode);
			AddCustomField(template1, "Custom1 String", AddOnColumnDataType.Codes.String);
			AddCustomField(template1, "Custom1 Integer", AddOnColumnDataType.Codes.Integer);

			var template2 = CreateWorkflowTemplate(WorkflowDescriptors.OrgSupplierPartWorkflowDescriptorCode);
			AddCustomField(template2, "Custom2 Boolean", AddOnColumnDataType.Codes.Boolean);
			AddCustomField(template2, "Custom2 Datetime", AddOnColumnDataType.Codes.Datetime);

			var unrelatedTemplate = CreateWorkflowTemplate("ZZZ");
			AddCustomField(unrelatedTemplate, "Unrelated Custom String", AddOnColumnDataType.Codes.String);

			Factory.Save();

			WorkflowCustomFieldsFilter.ClearCache();
		}

		#endregion

		#region TestDynamicProductAreaFilter

		public void TestDynamicProductAreaFilter()
		{
			var helper = WhsTransactionTestHelperCreator.GetNewHelper(Factory);
			var warehouse = (IWhsWarehouse)helper.CreateWarehouse("WHS", "A", 3, 1);
			var clientPK = helper.CreateClient("CLIENT");

			var dynamicArea1 = (IWhsArea)helper.CreateWhsArea(warehouse.PK, "DYNAMIC1");
			var dynamicArea2 = (IWhsArea)helper.CreateWhsArea(warehouse.PK, "DYNAMIC2");

			dynamicArea1[WhsAreaSchema.Constants.WA_AreaType] = "DPF";
			dynamicArea1[WhsAreaSchema.Constants.WA_IsPickingArea] = true;
			dynamicArea1[WhsAreaSchema.Constants.WA_IsPutawayArea] = false;

			dynamicArea2[WhsAreaSchema.Constants.WA_AreaType] = "DPF";
			dynamicArea2[WhsAreaSchema.Constants.WA_IsPickingArea] = true;
			dynamicArea2[WhsAreaSchema.Constants.WA_IsPutawayArea] = false;

			var locations = Factory.Load<IWhsLocation>(new ZQuery(WhsLocationViewSchema.WLV_WW_Whs, warehouse.PK));
			locations[0][WhsLocationViewSchema.Constants.WLV_WA_PickingArea] = dynamicArea1.PK;
			locations[1][WhsLocationViewSchema.Constants.WLV_WA_PickingArea] = dynamicArea2.PK;

			var product1 = (OrgSupplierPart)helper.CreateProduct(clientPK, "PR1");
			var product2 = (OrgSupplierPart)helper.CreateProduct(clientPK, "PR2");
			var product3 = (OrgSupplierPart)helper.CreateProduct(clientPK, "PR3");
			var product4 = (OrgSupplierPart)helper.CreateProduct(clientPK, "PR4");

			var productParams1 = helper.CreateProductParamsByWhsAndClient(product1.PK, clientPK, warehouse.PK, 1);
			productParams1.W3_WA_DynamicPickFaceArea = dynamicArea1.PK;
			var productParams2 = helper.CreateProductParamsByWhsAndClient(product2.PK, clientPK, warehouse.PK, 1);
			productParams2.W3_WA_DynamicPickFaceArea = dynamicArea2.PK;
			var productParams3 = helper.CreateProductParamsByWhsAndClient(product3.PK, clientPK, warehouse.PK, 1);
			productParams3.W3_WA_DynamicPickFaceArea = dynamicArea1.PK;

			Factory.Save();

			var productCollection = new OrgSupplierPartCollection(Factory);
			var filterStrip = new OrgSupplierPartFilterStripBusinessObject();
			filterStrip[OrgSupplierPartFilterStripBusinessObject.Descriptions.DynamicPickFaceArea].IsActive = true;

			((ModuleGuidFilter)filterStrip[OrgSupplierPartFilterStripBusinessObject.Descriptions.DynamicPickFaceArea]).Property = dynamicArea1.PK;
			productCollection.Load(filterStrip.Filter);
			CombineAssertions(() =>
			{
				AssertEquals(2, productCollection.Count);
				AssertEquals(true, productCollection.Contains(product1.PK));
				AssertEquals(true, productCollection.Contains(product3.PK));
			});

			((ModuleGuidFilter)filterStrip[OrgSupplierPartFilterStripBusinessObject.Descriptions.DynamicPickFaceArea]).Property = dynamicArea2.PK;
			productCollection.Load(filterStrip.Filter);
			CombineAssertions(() =>
			{
				AssertEquals(1, productCollection.Count);
				AssertEquals(true, productCollection.Contains(product2.PK));
			});
		}

		public void TestDynamicProductAreaFilter_NoResults()
		{
			var helper = WhsTransactionTestHelperCreator.GetNewHelper(Factory);
			var warehouse = (IWhsWarehouse)helper.CreateWarehouse("WHS", "A", 3, 1);
			var clientPK = helper.CreateClient("CLIENT");

			var dynamicArea1 = (IWhsArea)helper.CreateWhsArea(warehouse.PK, "DYNAMIC1");
			var dynamicArea2 = (IWhsArea)helper.CreateWhsArea(warehouse.PK, "DYNAMIC2");

			dynamicArea1[WhsAreaSchema.Constants.WA_AreaType] = "DPF";
			dynamicArea1[WhsAreaSchema.Constants.WA_IsPickingArea] = true;
			dynamicArea1[WhsAreaSchema.Constants.WA_IsPutawayArea] = false;

			dynamicArea2[WhsAreaSchema.Constants.WA_AreaType] = "DPF";
			dynamicArea2[WhsAreaSchema.Constants.WA_IsPickingArea] = true;
			dynamicArea2[WhsAreaSchema.Constants.WA_IsPutawayArea] = false;

			var locations = Factory.Load<IWhsLocation>(new ZQuery(WhsLocationViewSchema.WLV_WW_Whs, warehouse.PK));
			locations[0][WhsLocationViewSchema.Constants.WLV_WA_PickingArea] = dynamicArea1.PK;
			locations[1][WhsLocationViewSchema.Constants.WLV_WA_PickingArea] = dynamicArea2.PK;

			var product1 = (OrgSupplierPart)helper.CreateProduct(clientPK, "PR1");
			var product2 = (OrgSupplierPart)helper.CreateProduct(clientPK, "PR2");

			var productParams1 = helper.CreateProductParamsByWhsAndClient(product1.PK, clientPK, warehouse.PK, 1);
			productParams1.W3_WA_DynamicPickFaceArea = dynamicArea1.PK;

			Factory.Save();

			var productCollection = new OrgSupplierPartCollection(Factory);
			var filterStrip = new OrgSupplierPartFilterStripBusinessObject();
			filterStrip[OrgSupplierPartFilterStripBusinessObject.Descriptions.DynamicPickFaceArea].IsActive = true;

			((ModuleGuidFilter)filterStrip[OrgSupplierPartFilterStripBusinessObject.Descriptions.DynamicPickFaceArea]).Property = dynamicArea2.PK;
			productCollection.Load(filterStrip.Filter);
			AssertEquals(0, productCollection.Count);
		}

		public void TestDynamicProductAreaFilter_ProductWithMultipleAreas()
		{
			var helper = WhsTransactionTestHelperCreator.GetNewHelper(Factory);
			var warehouse1 = (IWhsWarehouse)helper.CreateWarehouse("WHS1", "A", 2, 1);
			var warehouse2 = (IWhsWarehouse)helper.CreateWarehouse("WHS2", "A", 2, 1);

			var clientPK = helper.CreateClient("CLIENT");

			var dynamicArea1 = (IWhsArea)helper.CreateWhsArea(warehouse1.PK, "DYNAMIC1");
			var dynamicArea2 = (IWhsArea)helper.CreateWhsArea(warehouse2.PK, "DYNAMIC2");
			var dynamicArea3 = (IWhsArea)helper.CreateWhsArea(warehouse1.PK, "DYNAMIC3");

			dynamicArea1[WhsAreaSchema.Constants.WA_AreaType] = "DPF";
			dynamicArea1[WhsAreaSchema.Constants.WA_IsPickingArea] = true;
			dynamicArea1[WhsAreaSchema.Constants.WA_IsPutawayArea] = false;

			dynamicArea2[WhsAreaSchema.Constants.WA_AreaType] = "DPF";
			dynamicArea2[WhsAreaSchema.Constants.WA_IsPickingArea] = true;
			dynamicArea2[WhsAreaSchema.Constants.WA_IsPutawayArea] = false;

			dynamicArea3[WhsAreaSchema.Constants.WA_AreaType] = "DPF";
			dynamicArea3[WhsAreaSchema.Constants.WA_IsPickingArea] = true;
			dynamicArea3[WhsAreaSchema.Constants.WA_IsPutawayArea] = false;

			var locations1 = Factory.Load<IWhsLocation>(new ZQuery(WhsLocationViewSchema.WLV_WW_Whs, warehouse1.PK));
			locations1[0][WhsLocationViewSchema.Constants.WLV_WA_PickingArea] = dynamicArea1.PK;
			locations1[1][WhsLocationViewSchema.Constants.WLV_WA_PickingArea] = dynamicArea3.PK;

			var locations2 = Factory.Load<IWhsLocation>(new ZQuery(WhsLocationViewSchema.WLV_WW_Whs, warehouse2.PK));
			locations2[0][WhsLocationViewSchema.Constants.WLV_WA_PickingArea] = dynamicArea2.PK;

			var product1 = (OrgSupplierPart)helper.CreateProduct(clientPK, "PR1");
			var product2 = (OrgSupplierPart)helper.CreateProduct(clientPK, "PR2");

			var productParams1 = helper.CreateProductParamsByWhsAndClient(product1.PK, clientPK, warehouse1.PK, 1);
			productParams1.W3_WA_DynamicPickFaceArea = dynamicArea1.PK;
			var productParams2 = helper.CreateProductParamsByWhsAndClient(product1.PK, clientPK, warehouse2.PK, 1);
			productParams2.W3_WA_DynamicPickFaceArea = dynamicArea2.PK;

			Factory.Save();

			var productCollection = new OrgSupplierPartCollection(Factory);
			var filterStrip = new OrgSupplierPartFilterStripBusinessObject();
			filterStrip[OrgSupplierPartFilterStripBusinessObject.Descriptions.DynamicPickFaceArea].IsActive = true;

			((ModuleGuidFilter)filterStrip[OrgSupplierPartFilterStripBusinessObject.Descriptions.DynamicPickFaceArea]).Property = dynamicArea1.PK;
			productCollection.Load(filterStrip.Filter);
			CombineAssertions(() =>
			{
				AssertEquals(1, productCollection.Count);
				AssertEquals(true, productCollection.Contains(product1.PK));
			});

			((ModuleGuidFilter)filterStrip[OrgSupplierPartFilterStripBusinessObject.Descriptions.DynamicPickFaceArea]).Property = dynamicArea2.PK;
			productCollection.Load(filterStrip.Filter);
			CombineAssertions(() =>
			{
				AssertEquals(1, productCollection.Count);
				AssertEquals(true, productCollection.Contains(product1.PK));
			});

			((ModuleGuidFilter)filterStrip[OrgSupplierPartFilterStripBusinessObject.Descriptions.DynamicPickFaceArea]).Property = dynamicArea3.PK;
			productCollection.Load(filterStrip.Filter);

			AssertEquals(0, productCollection.Count);
		}

		public void TestDynamicProductAreaFilter_MultipleFilters_GroupedWithAND()
		{
			var helper = WhsTransactionTestHelperCreator.GetNewHelper(Factory);
			var warehouse1 = (IWhsWarehouse)helper.CreateWarehouse("WHS1", "A", 2, 1);
			var warehouse2 = (IWhsWarehouse)helper.CreateWarehouse("WHS2", "A", 2, 1);

			var clientPK = helper.CreateClient("CLIENT");

			var dynamicArea1 = (IWhsArea)helper.CreateWhsArea(warehouse1.PK, "DYNAMIC1");
			var dynamicArea2 = (IWhsArea)helper.CreateWhsArea(warehouse2.PK, "DYNAMIC2");

			dynamicArea1[WhsAreaSchema.Constants.WA_AreaType] = "DPF";
			dynamicArea1[WhsAreaSchema.Constants.WA_IsPickingArea] = true;
			dynamicArea1[WhsAreaSchema.Constants.WA_IsPutawayArea] = false;

			dynamicArea2[WhsAreaSchema.Constants.WA_AreaType] = "DPF";
			dynamicArea2[WhsAreaSchema.Constants.WA_IsPickingArea] = true;
			dynamicArea2[WhsAreaSchema.Constants.WA_IsPutawayArea] = false;

			var locations1 = Factory.Load<IWhsLocation>(new ZQuery(WhsLocationViewSchema.WLV_WW_Whs, warehouse1.PK));
			locations1[0][WhsLocationViewSchema.Constants.WLV_WA_PickingArea] = dynamicArea1.PK;

			var locations2 = Factory.Load<IWhsLocation>(new ZQuery(WhsLocationViewSchema.WLV_WW_Whs, warehouse2.PK));
			locations2[0][WhsLocationViewSchema.Constants.WLV_WA_PickingArea] = dynamicArea2.PK;

			var product1 = (OrgSupplierPart)helper.CreateProduct(clientPK, "PR1");
			var product2 = (OrgSupplierPart)helper.CreateProduct(clientPK, "PR2");
			var product3 = (OrgSupplierPart)helper.CreateProduct(clientPK, "PR3");
			var product4 = (OrgSupplierPart)helper.CreateProduct(clientPK, "PR4");

			var productParams1 = helper.CreateProductParamsByWhsAndClient(product1.PK, clientPK, warehouse1.PK, 1);
			productParams1.W3_WA_DynamicPickFaceArea = dynamicArea1.PK;
			var productParams2 = helper.CreateProductParamsByWhsAndClient(product1.PK, clientPK, warehouse2.PK, 1);
			productParams2.W3_WA_DynamicPickFaceArea = dynamicArea2.PK;
			var productParams3 = helper.CreateProductParamsByWhsAndClient(product2.PK, clientPK, warehouse1.PK, 1);
			productParams3.W3_WA_DynamicPickFaceArea = dynamicArea1.PK;
			var productParams4 = helper.CreateProductParamsByWhsAndClient(product3.PK, clientPK, warehouse2.PK, 1);
			productParams4.W3_WA_DynamicPickFaceArea = dynamicArea2.PK;

			Factory.Save();

			var productCollection = new OrgSupplierPartCollection(Factory);
			var filterStrip = new OrgSupplierPartFilterStripBusinessObject();

			var filter1 = filterStrip.AddFilterStrip<ModuleGuidFilter>(OrgSupplierPartFilterStripBusinessObject.Descriptions.DynamicPickFaceArea);
			filter1.IsActive = true;
			filter1.Property = dynamicArea1.PK;

			var filter2 = filterStrip.AddFilterStrip<ModuleGuidFilter>(OrgSupplierPartFilterStripBusinessObject.Descriptions.DynamicPickFaceArea);
			filter2.IsActive = true;
			filter2.Property = dynamicArea2.PK;

			productCollection.Load(filterStrip.Filter);
			CombineAssertions(() =>
			{
				AssertEquals(1, productCollection.Count);
				AssertEquals(true, productCollection.Contains(product1.PK));
			});
		}

		public void TestDynamicProductAreaFilter_MultipleFilters_GroupedWithOR()
		{
			var helper = WhsTransactionTestHelperCreator.GetNewHelper(Factory);
			var warehouse1 = (IWhsWarehouse)helper.CreateWarehouse("WHS1", "A", 2, 1);
			var warehouse2 = (IWhsWarehouse)helper.CreateWarehouse("WHS2", "A", 2, 1);

			var clientPK = helper.CreateClient("CLIENT");

			var dynamicArea1 = (IWhsArea)helper.CreateWhsArea(warehouse1.PK, "DYNAMIC1");
			var dynamicArea2 = (IWhsArea)helper.CreateWhsArea(warehouse2.PK, "DYNAMIC2");

			dynamicArea1[WhsAreaSchema.Constants.WA_AreaType] = "DPF";
			dynamicArea1[WhsAreaSchema.Constants.WA_IsPickingArea] = true;
			dynamicArea1[WhsAreaSchema.Constants.WA_IsPutawayArea] = false;

			dynamicArea2[WhsAreaSchema.Constants.WA_AreaType] = "DPF";
			dynamicArea2[WhsAreaSchema.Constants.WA_IsPickingArea] = true;
			dynamicArea2[WhsAreaSchema.Constants.WA_IsPutawayArea] = false;

			var locations1 = Factory.Load<IWhsLocation>(new ZQuery(WhsLocationViewSchema.WLV_WW_Whs, warehouse1.PK));
			locations1[0][WhsLocationViewSchema.Constants.WLV_WA_PickingArea] = dynamicArea1.PK;

			var locations2 = Factory.Load<IWhsLocation>(new ZQuery(WhsLocationViewSchema.WLV_WW_Whs, warehouse2.PK));
			locations2[0][WhsLocationViewSchema.Constants.WLV_WA_PickingArea] = dynamicArea2.PK;

			var product1 = (OrgSupplierPart)helper.CreateProduct(clientPK, "PR1");
			var product2 = (OrgSupplierPart)helper.CreateProduct(clientPK, "PR2");
			var product3 = (OrgSupplierPart)helper.CreateProduct(clientPK, "PR3");
			var product4 = (OrgSupplierPart)helper.CreateProduct(clientPK, "PR4");

			var productParams1 = helper.CreateProductParamsByWhsAndClient(product1.PK, clientPK, warehouse1.PK, 1);
			productParams1.W3_WA_DynamicPickFaceArea = dynamicArea1.PK;
			var productParams2 = helper.CreateProductParamsByWhsAndClient(product1.PK, clientPK, warehouse2.PK, 1);
			productParams2.W3_WA_DynamicPickFaceArea = dynamicArea2.PK;
			var productParams3 = helper.CreateProductParamsByWhsAndClient(product2.PK, clientPK, warehouse1.PK, 1);
			productParams3.W3_WA_DynamicPickFaceArea = dynamicArea1.PK;
			var productParams4 = helper.CreateProductParamsByWhsAndClient(product3.PK, clientPK, warehouse2.PK, 1);
			productParams4.W3_WA_DynamicPickFaceArea = dynamicArea2.PK;

			Factory.Save();

			var productCollection = new OrgSupplierPartCollection(Factory);
			var filterStrip = new OrgSupplierPartFilterStripBusinessObject();

			var filter1 = filterStrip.AddFilterStrip<ModuleGuidFilter>(OrgSupplierPartFilterStripBusinessObject.Descriptions.DynamicPickFaceArea);
			filter1.IsActive = true;
			filter1.Property = dynamicArea1.PK;
			filter1.OrCategory = FilterOrCategory.Red;

			var filter2 = filterStrip.AddFilterStrip<ModuleGuidFilter>(OrgSupplierPartFilterStripBusinessObject.Descriptions.DynamicPickFaceArea);
			filter2.IsActive = true;
			filter2.Property = dynamicArea2.PK;
			filter2.OrCategory = FilterOrCategory.Red;

			productCollection.Load(filterStrip.Filter);
			CombineAssertions(() =>
			{
				AssertEquals(3, productCollection.Count);
				AssertEquals(true, productCollection.Contains(product1.PK));
				AssertEquals(true, productCollection.Contains(product2.PK));
				AssertEquals(true, productCollection.Contains(product3.PK));
			});
		}

		public void TestDynamicProductAreaFilter_NonDynamicArea()
		{
			var helper = WhsTransactionTestHelperCreator.GetNewHelper(Factory);
			var warehouse = (IWhsWarehouse)helper.CreateWarehouse("WHS", "A", 2, 1);
			var clientPK = helper.CreateClient("CLIENT");

			var dynamicArea = (IWhsArea)helper.CreateWhsArea(warehouse.PK, "DYNAMIC");
			var freestoreArea = (IWhsArea)helper.CreateWhsArea(warehouse.PK, "FREE");

			dynamicArea[WhsAreaSchema.Constants.WA_AreaType] = "DPF";
			dynamicArea[WhsAreaSchema.Constants.WA_IsPickingArea] = true;
			dynamicArea[WhsAreaSchema.Constants.WA_IsPutawayArea] = false;

			freestoreArea[WhsAreaSchema.Constants.WA_AreaType] = "FRE";
			freestoreArea[WhsAreaSchema.Constants.WA_IsPickingArea] = true;
			freestoreArea[WhsAreaSchema.Constants.WA_IsPutawayArea] = false;

			var locations = Factory.Load<IWhsLocation>(new ZQuery(WhsLocationViewSchema.WLV_WW_Whs, warehouse.PK));
			locations[0][WhsLocationViewSchema.Constants.WLV_WA_PickingArea] = dynamicArea.PK;

			var product1 = (OrgSupplierPart)helper.CreateProduct(clientPK, "PR1");

			var productParams1 = helper.CreateProductParamsByWhsAndClient(product1.PK, clientPK, warehouse.PK, 1);
			productParams1.W3_WA_DynamicPickFaceArea = dynamicArea.PK;

			Factory.Save();

			var productCollection = new OrgSupplierPartCollection(Factory);
			var filterStrip = new OrgSupplierPartFilterStripBusinessObject();
			filterStrip[OrgSupplierPartFilterStripBusinessObject.Descriptions.DynamicPickFaceArea].IsActive = true;

			var dynamicAreaFilter = ((ModuleGuidFilter)filterStrip[OrgSupplierPartFilterStripBusinessObject.Descriptions.DynamicPickFaceArea]);
			dynamicAreaFilter.Property = freestoreArea.PK;
			AssertHasError(dynamicAreaFilter.PropertyInfo, "Enter a valid selection.");
			productCollection.Load(filterStrip.Filter);
			AssertEquals(0, productCollection.Count);

			dynamicAreaFilter.Property = dynamicArea.PK;
			AssertNoError(dynamicAreaFilter.PropertyInfo, "Enter a valid selection.");
			productCollection.Load(filterStrip.Filter);
			CombineAssertions(() =>
			{
				AssertEquals(1, productCollection.Count);
				AssertEquals(true, productCollection.Contains(product1.PK));
			});
		}

		#endregion

		#region  Workflow Custom Fields Filter

		public void TestCustomFieldFilter()
		{
			var client = Factory.NewWithValidTestData<OrgHeader>();
			var template = CreateWorkflowTemplate(WorkflowDescriptors.OrgSupplierPartWorkflowDescriptorCode);
			template.P0_OH_Client = client.PK;
			AddCustomField(template, "Custom StringField", AddOnColumnDataType.Codes.String);

			var part1 = Factory.New<OrgSupplierPart>();
			part1.OP_PartNum = "PartNum1";
			var relation1 = part1.RelatedOrganisations.AddOrganisationIfNotExist(client.PK, OrgPartRelation.RelationshipTypes.Owner);
			relation1.OU_FormLayoutController = true;
			part1.SetUserDefinedValue("Custom StringField", (ZString)"111");

			var part2 = Factory.New<OrgSupplierPart>();
			part2.OP_PartNum = "PartNum2";
			var relation2 = part2.RelatedOrganisations.AddOrganisationIfNotExist(client.PK, OrgPartRelation.RelationshipTypes.Owner);
			relation2.OU_FormLayoutController = true;
			part2.SetUserDefinedValue("Custom StringField", (ZString)"222");

			Factory.Save();
			WorkflowCustomFieldsFilter.ClearCache();
			var filter = new OrgSupplierPartFilterStripBusinessObject();
			((ModuleTextFilter)filter["Custom StringField"]).Property = "111";
			((ModuleTextFilter)filter["Custom StringField"]).IsActive = true;

			var collection = new OrgSupplierPartCollection(Factory);
			collection.Load(filter.Filter);

			AssertEquals("Should have 1 OrgSupplierPart", 1, collection.Count);
			AssertCollectionNotContains("Should not have part2", part2, collection);
			AssertCollectionContains("Should have part1", part1, collection);
		}

		ProcessTaskTemplate CreateWorkflowTemplate(ZString workflowDescriptorCode)
		{
			var result = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			result.P0_ProcessType = workflowDescriptorCode;
			return result;
		}

		GenCustomColumnDefinition AddCustomField(ProcessTaskTemplate template, ZString name, ZString addOnColumnDataTypeCode)
		{
			var result = template.GenCustomColumnDefinitions.AddNew();
			result.XC_Name = name;
			result.XC_Type = addOnColumnDataTypeCode;
			return result;
		}

		#endregion

		#region TestProductCategoryFilter

		public void TestProductCategoryFilter()
		{
			var helper = WhsTransactionTestHelperCreator.GetNewHelper(Factory);

			var clientPK = helper.CreateClient("Client");
			var client = Factory.Load<OrgHeader>(clientPK);

			// product categories and products
			var categoryBeverage = helper.CreateProductCategory("BEV", "Beverages", ZGuid.Empty);
			var categorySoftdrink = helper.CreateProductCategory("SOFTDRK", "Soft Drinks", categoryBeverage.PK);
			var categoryBeer = helper.CreateProductCategory("BEER", "All Beers", categoryBeverage.PK);
			var categoryDarkBeer = helper.CreateProductCategory("DARKBEER", "Dark Beers", categoryBeer.PK);

			var productCoke = (OrgSupplierPart)helper.CreateProduct(client.PK, "Coke");
			var relationCoke = productCoke.RelatedOrganisations.FindByOrganisationAndRelationship(client, OrgPartRelation.RelationshipTypes.Owner);
			relationCoke.OU_OPC_Category = categorySoftdrink.PK;

			var productTea = (OrgSupplierPart)helper.CreateProduct(client.PK, "Tea");
			var relationTea = productTea.RelatedOrganisations.FindByOrganisationAndRelationship(client, OrgPartRelation.RelationshipTypes.Owner);
			relationTea.OU_OPC_Category = categorySoftdrink.PK;

			var productVB = (OrgSupplierPart)helper.CreateProduct(client.PK, "VB");
			var relationVB = productVB.RelatedOrganisations.FindByOrganisationAndRelationship(client, OrgPartRelation.RelationshipTypes.Owner);
			relationVB.OU_OPC_Category = categoryBeer.PK;

			var productGuinness = (OrgSupplierPart)helper.CreateProduct(client.PK, "Guinness");
			var relationGuinness = productGuinness.RelatedOrganisations.FindByOrganisationAndRelationship(client, OrgPartRelation.RelationshipTypes.Owner);
			relationGuinness.OU_OPC_Category = categoryDarkBeer.PK;

			Factory.Save();

			var productCollection = new OrgSupplierPartCollection(Factory);
			var filterStrip = new OrgSupplierPartFilterStripBusinessObject();
			filterStrip["Product Category"].IsActive = true;

			((ModuleGuidFilter)filterStrip["Product Category"]).Property = categorySoftdrink.PK;
			productCollection.Load(filterStrip.Filter);
			AssertEquals(2, productCollection.Count);
			AssertEquals("Find all soft-drinks.", true, productCollection.Contains(productCoke.PK));
			AssertEquals("Find all soft-drinks.", true, productCollection.Contains(productTea.PK));
			AssertEquals("Find all soft-drinks.", false, productCollection.Contains(productVB.PK));
			AssertEquals("Find all soft-drinks.", false, productCollection.Contains(productGuinness.PK));

			((ModuleGuidFilter)filterStrip["Product Category"]).Property = categoryBeer.PK;
			productCollection.Load(filterStrip.Filter);
			AssertEquals(2, productCollection.Count);
			AssertEquals("Find all Beers.", false, productCollection.Contains(productCoke.PK));
			AssertEquals("Find all Beers.", false, productCollection.Contains(productTea.PK));
			AssertEquals("Find all Beers.", true, productCollection.Contains(productVB.PK));
			AssertEquals("Find all Beers.", true, productCollection.Contains(productGuinness.PK));

			((ModuleGuidFilter)filterStrip["Product Category"]).Property = categoryDarkBeer.PK;
			productCollection.Load(filterStrip.Filter);
			AssertEquals(1, productCollection.Count);
			AssertEquals("Find all Dark Beers.", false, productCollection.Contains(productCoke.PK));
			AssertEquals("Find all Dark Beers.", false, productCollection.Contains(productTea.PK));
			AssertEquals("Find all Dark Beers.", false, productCollection.Contains(productVB.PK));
			AssertEquals("Find all Dark Beers.", true, productCollection.Contains(productGuinness.PK));

			((ModuleGuidFilter)filterStrip["Product Category"]).Property = categoryBeverage.PK;
			productCollection.Load(filterStrip.Filter);
			AssertEquals(4, productCollection.Count);
			AssertEquals("Find all Beverages.", true, productCollection.Contains(productCoke.PK));
			AssertEquals("Find all Beverages.", true, productCollection.Contains(productTea.PK));
			AssertEquals("Find all Beverages.", true, productCollection.Contains(productVB.PK));
			AssertEquals("Find all Beverages.", true, productCollection.Contains(productGuinness.PK));

			((ModuleGuidFilter)filterStrip["Product Category"]).Property = ZGuid.Empty;
			productCollection.Load(filterStrip.Filter);
			AssertEquals(4, productCollection.Count);
			AssertEquals("Find all Beverages.", true, productCollection.Contains(productCoke.PK));
			AssertEquals("Find all Beverages.", true, productCollection.Contains(productTea.PK));
			AssertEquals("Find all Beverages.", true, productCollection.Contains(productVB.PK));
			AssertEquals("Find all Beverages.", true, productCollection.Contains(productGuinness.PK));
		}

		#endregion

		#region TestClassificationOrganizationsFilter

		public void TestClassificationOrganizationsFilter()
		{
			var org = OrgHeader.New(Factory);
			org.OH_Code = "ORG";
			var orgParent = OrgParent(org, "ORGPARENT");

			var otherOrg = OrgHeader.New(Factory);
			otherOrg.OH_Code = "OTHERORG";
			var otherOrgParent = OrgParent(otherOrg, "ORGPARENT2");

			var part1 = CreatePartWithRelatedOrg("PART1", org, OrgPartRelation.RelationshipTypes.ClassificationOrganization);
			var part2 = CreatePartWithRelatedOrg("PART2", orgParent, OrgPartRelation.RelationshipTypes.ClassificationOrganization);
			var part3 = CreatePartWithRelatedOrg("PART3", otherOrg, OrgPartRelation.RelationshipTypes.ClassificationOrganization);
			var part4 = CreatePartWithRelatedOrg("PART4", org, OrgPartRelation.RelationshipTypes.Both);
			var part5 = CreatePartWithRelatedOrg("PART5", otherOrgParent, OrgPartRelation.RelationshipTypes.Both);

			Factory.Save();

			var productCollection = new OrgSupplierPartCollection(Factory);
			var filterStrip = new OrgSupplierPartFilterStripBusinessObject();
			var filter = (ModuleGuidFilter)filterStrip[OrgSupplierPartFilterStripBusinessObject.Descriptions.ClassificationOrganizations];
			filter.IsActive = true;
			filter.Property = org.PK;

			var parts = new OrgSupplierPartCollection(Factory, filterStrip.Filter);
			parts.Load();
			AssertContainsExactElementsInAnyOrder("ORG and its parent ORGPARENT matched", new OrgSupplierPart[] { part1, part2 }, parts);

			filter.Property = orgParent.PK;
			parts = new OrgSupplierPartCollection(Factory, filter.Query);
			parts.Load();
			AssertEquals("ORGPARENT matched", part2, parts.Single());

			filter.Property = otherOrg.PK;
			parts = new OrgSupplierPartCollection(Factory, filter.Query);
			parts.Load();
			AssertEquals("OTHERORG matched", part3, parts.Single());
		}

		OrgHeader OrgParent(OrgHeader org, string parentOrgCode)
		{
			var orgParent = OrgHeader.New(Factory);
			orgParent.OH_Code = parentOrgCode;
			var relatedParty = orgParent.AllRelatedParties.AddNew();
			relatedParty.PR_OH_Parent = orgParent.PK;
			relatedParty.PR_OH_RelatedParty = org.PK;
			relatedParty.PR_PartyType = RelatedPartyTypeList.Codes.ProductRelationship;
			return orgParent;
		}

		#endregion

		#region Implementation

		OrgSupplierPartCollection GetFilteredCollection(string propertyName, Action<ModuleFilter> extraSetup = null)
		{
			var filterStrip = new OrgSupplierPartFilterStripBusinessObject();

			var moduleFilter = filterStrip[propertyName];
			moduleFilter.IsActive = true;
			if (extraSetup != null)
			{
				extraSetup(moduleFilter);
			}
			var result = new OrgSupplierPartCollection(Factory);
			result.Load(filterStrip.Filter);
			return result;
		}

		OrgSupplierPartCollection GetFilteredCollection(string propertyName, string propertyValue)
		{
			return GetFilteredCollection(propertyName, (x) =>
			{
				((ModuleTextFilter)x).Property = propertyValue;
			});
		}

		OrgSupplierPartCollection GetFilteredCollection(string propertyName, string moduleDataFilter, ZDateTime dateFrom, ZDateTime dateTo)
		{
			return GetFilteredCollection(propertyName, (x) =>
			{
				var datefilter = (ModuleDateFilter)x;
				datefilter.PropertySearch = moduleDataFilter;
				datefilter.Property1 = dateFrom;
				datefilter.Property2 = dateTo;
			});
		}

		OrgSupplierPartCollection GetFilteredCollectionWithComparisonOperator(string propertyName, string comparisonOperator)
		{
			return GetFilteredCollection(propertyName, (x) =>
			{
				((ModuleTextFilter)x).ComparisonOperator = comparisonOperator;
			});
		}

		OrgPartBOM CreateNewWithPart(OrgSupplierPart part, string componentPartNum/*to help with debugging*/)
		{
			OrgPartBOM result = part.BillOfMaterials.AddNew();
			result.OE_OP_Component = Factory.New<OrgSupplierPart>().PK;
			result.Component.OP_PartNum = componentPartNum;
			result.Component.OP_Desc = componentPartNum;
			return result;
		}

		OrgSupplierPart ParentPart
		{
			get
			{
				if (parentPart == null)
				{
					parentPart = Factory.New<OrgSupplierPart>();
					parentPart.OP_PartNum = "A";
				}
				return parentPart;
			}
		}
		OrgSupplierPart parentPart;

		OrgSupplierPart CreatePartWithRelatedOrg(string partCode, OrgHeader org, string relationshipType)
		{
			var part = Factory.New<OrgSupplierPart>();
			part.OP_PartNum = partCode;
			part.OP_Desc = partCode;
			if (org != null)
			{
				var relation1 = part.RelatedOrganisations.AddNew();
				relation1.OU_Relationship = relationshipType;
				relation1.OU_OH = org.PK;
			}
			return part;
		}

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new OrgSupplierPartFilterStripBusinessObject();
		}

		#endregion
	}
}
