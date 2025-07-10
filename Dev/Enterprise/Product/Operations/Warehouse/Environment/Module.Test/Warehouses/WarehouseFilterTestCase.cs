using System;
using System.Linq;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Environment.Business.Testing;
using Enterprise.Warehouse.Integration;
using Enterprise.Warehouse.Integration.CodeLists;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Warehouse.Environment.Module.Testing
{
	[TestedType(typeof(WarehouseFilterBusinessObject))]
	class WarehouseFilterTestCase : FilterStripBusinessObjectTestCase
	{
		#region TextFiltersTests

		public void TestCodeFilter()
		{
			var warehouse1 = Factory.NewWithValidTestData<WhsWarehouse>();
			var warehouse2 = Factory.NewWithValidTestData<WhsWarehouse>();
			warehouse1.WW_WarehouseCode = "SEA";
			warehouse2.WW_WarehouseCode = "AIR";
			Factory.Save();

			var filterBizO1 = new WarehouseFilterBusinessObject();
			Asserter.AddToScope(warehouse1);
			Asserter.AddToScope(warehouse2);

			var moduleFilter1 = (ModuleTextFilter)filterBizO1["Code"];
			moduleFilter1.IsActive = true;
			moduleFilter1.Property = "SEA";
			Asserter.AssertMatches("Warehouse1 should show as its Whs code is being searched", moduleFilter1, warehouse1);

			Env.Security.WhsAllowedWarehouses.IsAllowed = false;
			var staff = GlbStaff.CurrentUser;
			staff.SecurityAllowedOrgsAndWarehousesView.SecurityRight = GlbSecurity.AllowedWarehousesSecurityRightName;
			((IOrgsAndWarehousesAccessProvider)staff).AddSecurityToAccessOrgOrWarehouse("SEA");
			Factory.Save();

			AssertEquals("Precondition:", true, WhsWarehouseCollectionWithSecurityCheck.AllowedAccessTo(warehouse1));
			AssertEquals("Precondition:", false, WhsWarehouseCollectionWithSecurityCheck.AllowedAccessTo(warehouse2));

			var filterBizO2 = new WarehouseFilterBusinessObject();
			var moduleFilter2 = (ModuleTextFilter)filterBizO2["Code"];
			moduleFilter2.IsActive = true;
			moduleFilter2.Property = "AIR";
			AssertHasError(moduleFilter2.PropertyInfo, "Enter a valid selection.");

			moduleFilter2.Property = "XXX";
			AssertHasError(moduleFilter2.PropertyInfo, "Enter a valid selection.");

			moduleFilter2.Property = "SEA";
			AssertNoErrors(moduleFilter2.PropertyInfo);

			moduleFilter2.Property = "";
			AssertHasError(moduleFilter2.PropertyInfo, "Please select a warehouse to filter by");
		}

		public void TestCodeFilterVisibility()
		{
			AssertEquals("Precondition: WhsAllowedWarehouses", true, Env.Security.WhsAllowedWarehouses.IsAllowed);
			var filterBusinessObject1 = GetNewFilterStripBusinessObject();
			var filter = (ModuleTextFilter)filterBusinessObject1["Code"];
			AssertEquals("Visibility", FilterVisibility.Visible, filter.Visibility);

			Env.Security.WhsAllowedWarehouses.IsAllowed = false;
			try
			{
				var filterBusinessObject2 = GetNewFilterStripBusinessObject();
				filter = (ModuleTextFilter)filterBusinessObject2["Code"];
				AssertEquals("Visibility", FilterVisibility.AlwaysVisible, filter.Visibility);

				Globals.IsWeb = true;
				try
				{
					var filterBusinessObject3 = GetNewFilterStripBusinessObject();
					filter = (ModuleTextFilter)filterBusinessObject3["Code"];
					AssertEquals("Visibility", FilterVisibility.Visible, filter.Visibility);
				}
				finally
				{
					Globals.IsWeb = false;
				}
			}
			finally
			{
				Env.Security.WhsAllowedWarehouses.IsAllowed = true;
			}
		}

		public void TestNameFilter()
		{
			WhsWarehouse name1 = Factory.NewWithValidTestData<WhsWarehouse>();
			WhsWarehouse name2 = Factory.NewWithValidTestData<WhsWarehouse>();

			name1.WW_WarehouseName = "SEA";
			name2.WW_WarehouseName = "AIR";

			Factory.Save();

			WarehouseFilterBusinessObject filter = new WarehouseFilterBusinessObject();
			((ModuleTextFilter)filter["Name"]).Property = "SEA";
			((ModuleTextFilter)filter["Name"]).IsActive = true;

			WhsWarehouseCollection warehouses = new WhsWarehouseCollection(Factory, filter.Filter);
			warehouses.Load();

			AssertCollectionContains(name1, warehouses);
			AssertCollectionNotContains(name2, warehouses);

			using (Res.TemporarilySwitchLanguage(Enterprise.Core.SharedConstants.Languages.ChineseSimplified))
			using (var mockRes = Res.UseMockData())
			{
				filter = new WarehouseFilterBusinessObject();
				AssertType(typeof(ModuleTextFilter), filter["Name"]);
				AssertEquals("Name (English)", filter["Name"].MultilingualDescription);
				AssertEquals(WhsWarehouseSchema.WW_WarehouseName, filter["Name"].FilterColumn);

				AssertType(typeof(ModuleTranslatableTextFilter), filter["Name_Local"]);
				AssertEquals("Name (Chinese - Simplified)", filter["Name_Local"].MultilingualDescription);
				AssertEquals(WhsWarehouseSchema.WW_WarehouseName, filter["Name_Local"].FilterColumn);
			}
		}

		#endregion

		#region TestWarehouseTypeFilterList

		public void TestProductWarehouseTypeFilter()
		{
			AssertWarehouseTypeFilterList("Product warehouse types should be visible.", WarehouseCollectionType.ProductWarehouse, "All, PRW, FTZ");
		}

		public void TestTransitWarehouseTypeFilter()
		{
			AssertWarehouseTypeFilterList("Transit warehouse types should be visible.", WarehouseCollectionType.TransitWarehouse, "All, TRW");
		}

		public void TestAllWarehouseTypeFilter()
		{
			AssertWarehouseTypeFilterList("All warehouse types should be visible.", WarehouseCollectionType.All, "All, CYD, FTZ, PRW, TRW");
		}

		void AssertWarehouseTypeFilterList(string message, WarehouseCollectionType warehouseType, string expectedList)
		{
			var filterObject = new WarehouseFilterBusinessObject(warehouseType);
			var warehouseTypeFilter = (ModuleTextFilter)filterObject["WarehouseType"];
			var codeList = (CodeDescriptionPairList)warehouseTypeFilter.List;
			AssertEquals(message, expectedList, codeList.CodesAsString);
		}

		#endregion

		#region TestWarehouseTypeDefaultProperty

		public void TestProductWarehouseTypeDefaultProperty()
		{
			AssertWarehouseTypeDefaultProperty("Product warehouse types should be visible.", WarehouseCollectionType.ProductWarehouse, WarehouseTypes.Codes.Product);
		}

		public void TestTransitWarehouseTypeDefaultProperty()
		{
			AssertWarehouseTypeDefaultProperty("Transit warehouse types should be visible.", WarehouseCollectionType.TransitWarehouse, WarehouseTypes.Codes.Transit);
		}

		public void TestCYDWarehouseTypeDefaultProperty()
		{
			AssertWarehouseTypeDefaultProperty("Transit warehouse types should be visible.", WarehouseCollectionType.CYDWarehouse, WarehouseTypes.Codes.ContainerYard);
		}

		void AssertWarehouseTypeDefaultProperty(string message, WarehouseCollectionType warehouseType, string expectedProperty)
		{
			var filterObject = new WarehouseFilterBusinessObject(warehouseType);
			var warehouseTypeFilter = (ModuleTextFilter)filterObject["WarehouseType"];
			AssertEquals(message, expectedProperty, warehouseTypeFilter.DefaultProperty);
		}

		#endregion

		#region CheckBoxFiltersTests

		public void TestStatusBondedFilter()
		{
			var freestoreWarehouse = Factory.NewWithValidTestData<WhsWarehouse>();
			var bondedWarehouse = Factory.NewWithValidTestData<WhsWarehouse>();
			var exciseWarehouse = Factory.NewWithValidTestData<WhsWarehouse>();
			var virtualWarehouse = Factory.NewWithValidTestData<WhsWarehouse>();
			Helper.EnableWarehouseForFreeStore(freestoreWarehouse, true);
			Helper.EnableWarehouseForBond(bondedWarehouse, true);
			Helper.EnableWarehouseForExcise(exciseWarehouse, true);
			virtualWarehouse.WW_IsVirtualWarehouse = true;

			// Warehouses are Freestore enabled by default, so remove this state for filter checking
			Helper.EnableWarehouseForFreeStore(bondedWarehouse, false);
			Helper.EnableWarehouseForFreeStore(exciseWarehouse, false);
			Helper.EnableWarehouseForFreeStore(virtualWarehouse, false);

			Factory.Save();

			var filter = new WarehouseFilterBusinessObject();
			((ModuleTextFilter)filter["TransactionType"]).IsActive = true;
			AssertEquals("Default value", "All", ((ModuleTextFilter)filter["TransactionType"]).Property);

			var warehouses = new WhsWarehouseCollection(Factory);
			warehouses.Load(filter.Filter);
			AssertCollectionContains(freestoreWarehouse, warehouses);
			AssertCollectionContains(bondedWarehouse, warehouses);
			AssertCollectionContains(exciseWarehouse, warehouses);
			AssertCollectionContains(virtualWarehouse, warehouses);

			((ModuleTextFilter)filter["TransactionType"]).Property = "Free Store";
			((ModuleTextFilter)filter["TransactionType"]).IsActive = true;

			warehouses.Load(filter.Filter);
			AssertCollectionContains(freestoreWarehouse, warehouses);
			AssertCollectionNotContains(bondedWarehouse, warehouses);
			AssertCollectionNotContains(exciseWarehouse, warehouses);
			AssertCollectionNotContains(virtualWarehouse, warehouses);

			((ModuleTextFilter)filter["TransactionType"]).Property = "Bonded";
			((ModuleTextFilter)filter["TransactionType"]).IsActive = true;

			warehouses.Load(filter.Filter);
			AssertCollectionNotContains(freestoreWarehouse, warehouses);
			AssertCollectionContains(bondedWarehouse, warehouses);
			AssertCollectionNotContains(exciseWarehouse, warehouses);
			AssertCollectionNotContains(virtualWarehouse, warehouses);

			((ModuleTextFilter)filter["TransactionType"]).Property = "Excise";
			((ModuleTextFilter)filter["TransactionType"]).IsActive = true;

			warehouses.Load(filter.Filter);
			AssertCollectionNotContains(freestoreWarehouse, warehouses);
			AssertCollectionNotContains(bondedWarehouse, warehouses);
			AssertCollectionContains(exciseWarehouse, warehouses);
			AssertCollectionNotContains(virtualWarehouse, warehouses);

			((ModuleTextFilter)filter["TransactionType"]).Property = "Virtual";
			((ModuleTextFilter)filter["TransactionType"]).IsActive = true;

			warehouses.Load(filter.Filter);
			AssertCollectionNotContains(freestoreWarehouse, warehouses);
			AssertCollectionNotContains(bondedWarehouse, warehouses);
			AssertCollectionNotContains(exciseWarehouse, warehouses);
			AssertCollectionContains(virtualWarehouse, warehouses);

			freestoreWarehouse.WW_IsVirtualWarehouse = ZBool.True;
			bondedWarehouse.WW_IsVirtualWarehouse = ZBool.True;
			exciseWarehouse.WW_IsVirtualWarehouse = ZBool.True;
			Factory.Save();

			warehouses.Load(filter.Filter);
			AssertCollectionContains(freestoreWarehouse, warehouses);
			AssertCollectionContains(bondedWarehouse, warehouses);
			AssertCollectionContains(exciseWarehouse, warehouses);
			AssertCollectionContains(virtualWarehouse, warehouses);
		}

		public void TestStatus_InwardProcessing()
		{
			var freestoreWarehouse = Factory.NewWithValidTestData<WhsWarehouse>();
			var bondedWarehouse = Factory.NewWithValidTestData<WhsWarehouse>();
			var exciseWarehouse = Factory.NewWithValidTestData<WhsWarehouse>();
			var iprWarehouse = Factory.NewWithValidTestData<WhsWarehouse>();
			Helper.EnableWarehouseForFreeStore(freestoreWarehouse, true);
			Helper.EnableWarehouseForBond(bondedWarehouse, true);
			Helper.EnableWarehouseForExcise(exciseWarehouse, true);
			iprWarehouse.WW_IsVirtualWarehouse = true;
			Helper.EnableWarehouseForInwardProcessing(iprWarehouse, true);

			// Warehouses are Freestore enabled by default, so remove this state for filter checking
			Helper.EnableWarehouseForFreeStore(bondedWarehouse, false);
			Helper.EnableWarehouseForFreeStore(exciseWarehouse, false);
			Helper.EnableWarehouseForFreeStore(iprWarehouse, false);

			Factory.Save();

			var filter = new WarehouseFilterBusinessObject();
			((ModuleTextFilter)filter["TransactionType"]).IsActive = true;
			AssertEquals("Default value", "All", ((ModuleTextFilter)filter["TransactionType"]).Property);

			var warehouses = new WhsWarehouseCollection(Factory);
			warehouses.Load(filter.Filter);
			AssertCollectionContains(freestoreWarehouse, warehouses);
			AssertCollectionContains(bondedWarehouse, warehouses);
			AssertCollectionContains(exciseWarehouse, warehouses);
			AssertCollectionContains(iprWarehouse, warehouses);

			var codeList = (CodeDescriptionPairList)((ModuleTextFilter)filter["TransactionType"]).List;
			Assert("Inward Processing filter should be present.", codeList.GetAllCodes().Any(c => c.Equals("Inward Processing")));

			((ModuleTextFilter)filter["TransactionType"]).Property = "Inward Processing";
			((ModuleTextFilter)filter["TransactionType"]).IsActive = true;

			warehouses.Load(filter.Filter);
			AssertCollectionNotContains(freestoreWarehouse, warehouses);
			AssertCollectionNotContains(bondedWarehouse, warehouses);
			AssertCollectionNotContains(exciseWarehouse, warehouses);
			AssertCollectionContains(iprWarehouse, warehouses);
		}

		#endregion

		#region RelatedItemFiltersTests

		public void TestBranchFilter()
		{
			var glbBranch1 = Factory.NewWithValidTestData<GlbBranch>();
			var glbBranch2 = Factory.NewWithValidTestData<GlbBranch>();

			var warehouse0 = Factory.NewWithValidTestData<WhsWarehouse>();
			warehouse0.WW_GB_RelatedCompanyBranch = glbBranch1.PK;

			var warehouse1 = Factory.NewWithValidTestData<WhsWarehouse>();
			warehouse1.WW_GB_RelatedCompanyBranch = glbBranch2.PK;

			// make this virtual to allow multiple warehouses for the same branch
			var warehouse2 = Factory.NewWithValidTestData<WhsWarehouse>();
			warehouse2.WW_GB_RelatedCompanyBranch = glbBranch2.PK;
			warehouse2.WW_IsVirtualWarehouse = true;

			Factory.Save();

			var glbBranch1Filter = new WarehouseFilterBusinessObject();
			((ModuleGuidFilter)glbBranch1Filter["Branch"]).Property = glbBranch1.PK;
			((ModuleGuidFilter)glbBranch1Filter["Branch"]).IsActive = true;

			var mawbs = new WhsWarehouseCollection(Factory, glbBranch1Filter.Filter);
			mawbs.Load();

			AssertCollectionContains(warehouse0, mawbs);
			AssertCollectionNotContains(warehouse1, mawbs);
			AssertCollectionNotContains(warehouse2, mawbs);

			var glbBranch2Filter = new WarehouseFilterBusinessObject();
			((ModuleGuidFilter)glbBranch2Filter["Branch"]).Property = glbBranch2.PK;
			((ModuleGuidFilter)glbBranch2Filter["Branch"]).IsActive = true;

			mawbs = new WhsWarehouseCollection(Factory, glbBranch2Filter.Filter);
			mawbs.Load();

			AssertCollectionNotContains(warehouse0, mawbs);
			AssertCollectionContains(warehouse1, mawbs);
			AssertCollectionContains(warehouse2, mawbs);

			var emptyGlbBranchFilter = new WarehouseFilterBusinessObject();
			((ModuleGuidFilter)emptyGlbBranchFilter["Branch"]).Property = ZGuid.Empty;
			((ModuleGuidFilter)emptyGlbBranchFilter["Branch"]).IsActive = false;

			mawbs = new WhsWarehouseCollection(Factory, emptyGlbBranchFilter.Filter);
			mawbs.Load();

			AssertCollectionContains(warehouse0, mawbs);
			AssertCollectionContains(warehouse1, mawbs);
			AssertCollectionContains(warehouse2, mawbs);
		}

		public void TestReleaseGroup_TaskManagementDisabled()
		{
			using (WarehouseDataRegistry.Instance.ExposeWarehouseTaskManagement.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var filterBizO = new WarehouseFilterBusinessObject();
				AssertNull(filterBizO[WarehouseFilterBusinessObject.Schema.ReleaseGroup]);
			}
		}

		public void TestReleaseGroup_TaskManagementEnabled()
		{
			using (WarehouseDataRegistry.Instance.ExposeWarehouseTaskManagement.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var releaseGroup1 = Helper.CreateReleaseGroup("RG1", "RG1");
				var releaseGroup2 = Helper.CreateReleaseGroup("RG2", "RG2");
				var warehouse1 = Factory.NewWithValidTestData<WhsWarehouse>();
				var warehouse2 = Factory.NewWithValidTestData<WhsWarehouse>();
				warehouse1.WW_WarehouseCode = "SEA";
				warehouse1.WW_GG_ReleaseGroup = releaseGroup1.PK;
				warehouse2.WW_WarehouseCode = "AIR";
				warehouse2.WW_GG_ReleaseGroup = releaseGroup2.PK;
				Factory.Save();

				var filterBizO1 = new WarehouseFilterBusinessObject();
				Asserter.AddToScope(warehouse1);
				Asserter.AddToScope(warehouse2);

				var moduleFilter = (ModuleGuidFilter)filterBizO1[WarehouseFilterBusinessObject.Schema.ReleaseGroup];
				AssertNotNull(moduleFilter);
				moduleFilter.IsActive = true;
				Asserter.AssertMatches("Both warehouses should show.", moduleFilter, warehouse1, warehouse2);

				moduleFilter.Property = releaseGroup1.PK;
				Asserter.AssertMatches("Warehouse1 should show.", moduleFilter, warehouse1);

				moduleFilter.Property = releaseGroup2.PK;
				Asserter.AssertMatches("Warehouse2 should show.", moduleFilter, warehouse2);
			}
		}

		#endregion

		#region Implementation

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new WarehouseFilterBusinessObject();
		}

		WhsTestHelperFunctionsEnv helper;
		WhsTestHelperFunctionsEnv Helper
		{
			get { return helper ?? (helper = new WhsTestHelperFunctionsEnv(Factory)); }
		}

		FilterStripAsserter<WhsWarehouse> Asserter
		{
			get { return asserter ?? (asserter = new FilterStripAsserter<WhsWarehouse>(Factory, w => w.WW_WarehouseCode)); }
		}

		FilterStripAsserter<WhsWarehouse> asserter;

		#endregion
	}
}
