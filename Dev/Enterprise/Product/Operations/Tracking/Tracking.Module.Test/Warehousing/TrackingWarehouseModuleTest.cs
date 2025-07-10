using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Web.UI.WebControls;
using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using Enterprise.Tracking.Business.Testing;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Web.Business.Testing;
using Enterprise.ZArchitecture.Web.GUI.WebControls;
using Enterprise.ZArchitecture.Web.Modules;
using Enterprise.ZArchitecture.Web.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Tracking.Module.Testing
{
	[TestedType(typeof(TrackingWarehouseModule))]
	sealed class TrackingWarehouseModuleTest : ZFilterStripGridModuleTestCase
	{
		#region Overriden Methods

		protected override void SetupForActiveStatusFilterTest()
		{
			base.SetupForActiveStatusFilterTest();
			var helper = new TestHelper(Factory);
			helper.TestSiteUser.Login(helper.TestOrg.OH_Code, helper.TestContact.OC_Email, helper.TestContact.PasswordForTesting);
		}

		protected override BusinessObject GetNewElement(Type elementType, bool isCancelled)
		{
			var element = base.GetNewElement(elementType, isCancelled);
			var warehouse = element as WhsWarehouse;
			if (warehouse != null)
			{
				warehouse.WW_WarehouseName = Guid.NewGuid().ToString();
				var order = Factory.NewWithValidTestData<WhsOrder>();
				order.WD_OH_Client = SiteUser.LoggedInOrganisation.PK;
				order.WD_WW_Whs = warehouse.PK;
			}

			return element;
		}

		#region TestCollectionLoadDBHitsWithDBOnlyQuery

		protected override void SetupForCollectionLoadDBHitsWithDBOnlyQueryTests()
		{
			base.SetupForCollectionLoadDBHitsWithDBOnlyQueryTests();
			TestHelper helper = new TestHelper(Factory);
			helper.TestSiteUser.Login(helper.TestOrg.OH_Code, helper.TestContact.OC_Email, helper.TestContact.PasswordForTesting);
		}

		protected override List<BusinessObject> GetNewBusinessObjectsExpectedFromFilter()
		{
			List<BusinessObject> result = new List<BusinessObject>();
			for (int i = 1; i < 10; i++)
			{
				WhsWarehouse testObject = Factory.NewWithValidTestData<WhsWarehouse>();
				testObject.WW_WarehouseName = "Include" + i.ToString();
				WhsOrder testDocket = Factory.NewWithValidTestData<WhsOrder>();
				testDocket.WD_OH_Client = SiteUser.LoggedInOrganisation.PK;
				testDocket.WD_WW_Whs = testObject.PK;
				result.Add(testObject);
			}
			return result;
		}

		protected override List<BusinessObject> GetNewBusinessObjectsUnexpectedFromFilter()
		{
			List<BusinessObject> result = new List<BusinessObject>();
			for (int i = 1; i < 10; i++)
			{
				WhsWarehouse testObject = Factory.NewWithValidTestData<WhsWarehouse>();
				testObject.WW_WarehouseName = "Other" + i.ToString();
				WhsOrder testDocket = Factory.NewWithValidTestData<WhsOrder>();
				testDocket.WD_OH_Client = SiteUser.LoggedInOrganisation.PK;
				testDocket.WD_WW_Whs = testObject.PK;
				result.Add(testObject);
			}
			return result;
		}

		protected override void SetupDBOnlyQuery(ZDBOnlyQuery filter)
		{
			filter.AddToFilter(WhsWarehouseSchema.WW_WarehouseName, SQLComparisonOperator.StartsWith, "Include");
		}

		#endregion

		protected override ZWebModule GetNewZWebModule()
		{
			return new TrackingWarehouseModule(Factory, TestPage);
		}

		protected override WebModuleID TestID => WebModuleIDs.TrackingWarehouse;

		protected override ColumnAndSortOrder[] ExpectedSortInfos => new[] { new ColumnAndSortOrder(WhsWarehouseSchema.WW_WarehouseName.Name, ListSortDirection.Ascending) };

		protected override DataGridColumn[] ExpectedRequiredGridColumns
		{
			get
			{
				var module = FilterGridModule as TrackingWarehouseModule;

				return new[]
				{
					module.AllColumns["Warehouse Name"]
				};
			}
		}

		protected override DataGridColumn[] ExpectedDefaultGridColumns
		{
			get
			{
				var module = FilterGridModule as TrackingWarehouseModule;

				return new[]
				{
					module.AllColumns["Code"]
				};
			}
		}

		protected override ColumnDetailsForTest[] ExpectedColumnDetails
		{
			get
			{
				var result = new List<ColumnDetailsForTest>();

				result.Add(new ColumnDetailsForTest("Warehouse Name", 0, typeof(ZButtonColumn)));
				result.Add(new ColumnDetailsForTest("Code", 1, typeof(ZTextEditColumn)));

				return result.ToArray();
			}
		}

		protected override ZWebTestHelper GetNewHelper() => new TestHelper(Factory);

		protected override FilterLayoutCodePairRegistryItem ExpectedDefaultLayoutRegistryItem => WebDataRegistry.Instance.DefaultFilterLayoutWarehouses;

		protected override string ExpectedDefaultLayoutName => DefaultLayoutNameValue;

		#endregion

		#region TestGetCurrentLoggedInUserFilter

		public void TestGetCurrentLoggedInUserFilter()
		{
			var warehouse = Factory.NewWithValidTestData<WhsWarehouse>();
			Factory.Save();

			AssertWarehousesQuantity(0);

			var order = Factory.NewWithValidTestData<WhsOrder>();
			order.WD_WW_Whs = warehouse.PK;
			order.WD_OH_Client = SiteUser.LoggedInOrganisation.PK;
			Factory.Save();

			AssertWarehousesQuantity(1);
		}

		void AssertWarehousesQuantity(int expectedCount)
		{
			using (var module = new TrackingWarehouseModuleForTest(Factory, null))
			{
				var warehouses = Factory.Load<WhsWarehouse>(module.GetCurrentLoggedInUserFilterForTest());

				AssertEquals("Expected warehouses quantity " + expectedCount, expectedCount, warehouses.Length);
			}
		}

		#endregion

		#region TestGetCurrentLoggedInUserFilter_OnlyReturnsClientAccessibleWarehouses

		public void TestGetCurrentLoggedInUserFilter_OnlyReturnsClientAccessibleWarehouses()
		{
			var testHelper = new TestHelper(Factory);
			testHelper.TestSiteUser.Login(testHelper.TestOrg.OH_Code, testHelper.TestContact.OC_Email, testHelper.TestContact.PasswordForTesting);
			var helper = new WhsTestHelperFunctions(Factory);
			var whs1 = helper.CreateWarehouse("WH1");
			var whs2 = helper.CreateWarehouse("WH2");

			helper.CreateWhsOrder(testHelper.TestOrg, whs1, "O1");
			helper.CreateWhsOrder(testHelper.TestOrg, whs2, "O2");
			Factory.Save();
			AssertAccessibleWarehouses("Precondition - All warehouses must be accessible.", new[] { whs1, whs2 });

			helper.ProhibitWarehouseAccessForOrgContact(whs1, testHelper.TestContact);
			Factory.Save();
			AssertAccessibleWarehouses("Only accessible warehouses for logged in contact must be returned.", new[] { whs2 });

			helper.ProhibitWarehouseAccessForOrgContact(whs2, testHelper.TestContact);
			Factory.Save();
			AssertAccessibleWarehouses("No warehouses must be returned since all of them are disabled.", Array.Empty<WhsWarehouse>());
		}

		void AssertAccessibleWarehouses(string message, WhsWarehouse[] expectedWarehouses)
		{
			using (var module = new TrackingWarehouseModuleForTest(Factory, null))
			{
				var warehouses = Factory.Load<WhsWarehouse>(module.GetCurrentLoggedInUserFilterForTest());
				AssertContainsExactElementsInAnyOrder(message, expectedWarehouses, warehouses);
			}
		}

		#endregion

		#region Implementation

		class TrackingWarehouseModuleForTest : TrackingWarehouseModule
		{
			public TrackingWarehouseModuleForTest(BusinessObjectFactory factory, ZPage page)
				: base(factory, page)
			{
			}

			public ZQuery GetCurrentLoggedInUserFilterForTest() => base.GetCurrentLoggedInUserFilter(null);
		}

		protected override Dictionary<string, string> GetExpectedAuditFilters() => new Dictionary<string, string>()
		{
			{ "Created Time", "Created Time" },
			{ "Last Edit Time", "Last Edit Time" },
			{ "Created On Web/Internal", "Created On Web/Internal" },
		};

		#endregion
	}
}
