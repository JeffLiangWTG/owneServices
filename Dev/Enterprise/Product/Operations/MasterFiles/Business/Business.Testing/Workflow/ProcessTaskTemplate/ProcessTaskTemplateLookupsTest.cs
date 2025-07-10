using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Warehouse.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class ProcessTaskTemplateLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestFallbackTypes()
		{
			var template = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			AssertNotNull("Items in the fallback types", template.Lookups.FallbackTypes);
		}

		public void TestCustomFieldFallbackTypes()
		{
			var template = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			AssertNotNull("Items in the fallback types", template.Lookups.CustomFieldFallbackTypes);
			AssertEquals("No fallback if empty", false, template.Lookups.CustomFieldFallbackTypes.ContainsCode(FallbackTypeList.Codes.EmptyFallback));
		}

		public void TestReleaseGroupFallbackMethods()
		{
			var template = Factory.NewWithValidTestData<ProcessTaskTemplate>();

			AssertEquals("No 'always fall back' because there's only one field to fill, so multiple values cannot be used", false, template.Lookups.ReleaseGroupFallbackMethods.ContainsCode(FallbackTypeList.Codes.AlwaysFallback));

			AssertEquals(true, template.Lookups.ReleaseGroupFallbackMethods.ContainsCode(FallbackTypeList.Codes.EmptyFallback));
			AssertEquals(true, template.Lookups.ReleaseGroupFallbackMethods.ContainsCode(FallbackTypeList.Codes.NeverFallback));

			AssertEquals("Falls back if no Release Group is determined", template.Lookups.ReleaseGroupFallbackMethods.GetDescriptionFromCode(FallbackTypeList.Codes.EmptyFallback));
			AssertEquals("Does not fall back, even if no Release Group is determined", template.Lookups.ReleaseGroupFallbackMethods.GetDescriptionFromCode(FallbackTypeList.Codes.NeverFallback));
		}

		public void TestWorkflowTypes()
		{
			ProcessTaskTemplate template = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			AssertNotNull("Items in the process types list", template.Lookups.WorkflowTypeList);

			foreach (CodeDescriptionPair providerCodePair in template.Lookups.WorkflowTypeList)
			{
				WorkflowDescriptor provider;
				AssertEquals(true, WorkflowDescriptors.Instance.TryGetValue(providerCodePair.Code, out provider));
				AssertNotNull(provider);
				AssertEquals(true, provider.SupportsWorkflowTemplates || provider.SupportsUniversalTemplates);
			}
		}

		public void TestWorkflowTypes_ProductivityWiseEnabled()
		{
			DataRegistry.Instance.ProductivityWiseModeEnabled = true;

			ProcessTaskTemplate template = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			AssertNotNull(template.Lookups.WorkflowTypeList);

			AssertContainsExactElementsInAnyOrder(WorkflowDescriptorsTest.ProductivityWiseWorkflowTypes, template.Lookups.WorkflowTypeList.GetAllCodes());
		}

		public void TestLocations()
		{
			ProcessTaskTemplate template = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			AssertNotNull("Locations", template.Lookups.Locations);
			AssertEquals("Regions not allowed", false, template.Lookups.Locations.AllowZones);
		}

		public void TestClients()
		{
			ProcessTaskTemplate template = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			AssertNotNull("Clients", template.Lookups.Clients);
		}

		public void TestWarehouses()
		{
			ProcessTaskTemplate template = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			AssertNotNull("Warehouses", template.Lookups.Warehouses);
		}

		public void TestWarehousewithDifferentType()
		{
			var warehouseTWD = (IWhsWarehouse)Helper.CreateWarehouse("1", "A");
			warehouseTWD.WW_WarehouseType = "TRW";
			var warehouse3PL = (IWhsWarehouse)Helper.CreateWarehouse("1", "A");
			warehouse3PL.WW_WarehouseType = "3PL";

			var template = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template.P0_ProcessType = DummyWorkflowDescriptor.Instance.Code;
			AssertContainsExactElementsInAnyOrder(new[] { warehouseTWD, warehouse3PL }, template.Lookups.Warehouses);

			DummyWorkflowDescriptor.Instance.WarehouseTypeNeeded = WarehouseCollectionType.TransitWarehouse;
			AssertContainsExactElementsInAnyOrder(new[] { warehouseTWD }, template.Lookups.Warehouses);
		}

		public void TestBMSystems()
		{
			ProcessTaskTemplate template = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			AssertNotNull("BMSystems", template.Lookups.BMSystems);
		}

		public void TestSubTypeLists()
		{
			AssertNotNull(DummyWorkflowDescriptor.Instance);
			ProcessTaskTemplate template = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template.P0_ProcessType = "DUM";

			AssertEquals("1 item in the list1", 1, template.Lookups.List1.Count);
			AssertEquals("1st item in the list1 - COL", "COL", ((CodeDescriptionPairList)template.Lookups.List1)[0].Code);

			AssertEquals("2 items in the list2", 2, template.Lookups.List2.Count);
			AssertEquals("1st item in the list2 - AUS", "AUS", ((CodeDescriptionPairList)template.Lookups.List2)[0].Code);
			AssertEquals("2nd item in the list2 - CHN", "CHN", ((CodeDescriptionPairList)template.Lookups.List2)[1].Code);

			AssertEquals("1 item in the list3", 1, template.Lookups.List3.Count);
			AssertEquals("1st item in the list3 - USA", "USA", ((CodeDescriptionPairList)template.Lookups.List3)[0].Code);

			AssertEquals("2 items in the list4", 2, template.Lookups.List4.Count);
			AssertEquals("1st item in the list4 - RUS", "RUS", ((CodeDescriptionPairList)template.Lookups.List4)[0].Code);
			AssertEquals("2nd item in the list4 - GBR", "GBR", ((CodeDescriptionPairList)template.Lookups.List4)[1].Code);

			AssertEquals("1 item in the Collection", 1, template.Lookups.List5.Count);
			AssertNotNull("item exists in the collection - MON", ((IActiveBusinessObjectCollection)template.Lookups.List5).GetBusinessObjectFromCode("MON"));
		}

		#region Validation Rules

		public void TestProcessTemplateValidationActionSourceList()
		{
			var template = Factory.New<ProcessTaskTemplate>();
			template.P0_ProcessType = "RPN";
			var actionSourceList = template.Lookups.ProcessTemplateValidationActionSourceList;
			AssertEquals(true, actionSourceList.Count > 1);
			AssertEquals(true, actionSourceList.ContainsCode("SAV"));
			AssertSame("load action source list from cache", actionSourceList, template.Lookups.ProcessTemplateValidationActionSourceList);
		}

		#endregion

		IWhsTransactionTestHelper Helper
		{
			get { return helper ?? (helper = WhsTransactionTestHelperCreator.GetNewHelper(Factory)); }
		}
		IWhsTransactionTestHelper helper;
	}
}
