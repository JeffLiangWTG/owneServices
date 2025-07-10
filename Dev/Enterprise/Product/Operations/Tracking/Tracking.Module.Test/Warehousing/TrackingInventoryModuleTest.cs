using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Web.UI.WebControls;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.Tracking.Business;
using Enterprise.Tracking.Business.Testing;
using Enterprise.Tracking.Web;
using Enterprise.Tracking.Web.Testing;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Web.GUI;
using Enterprise.ZArchitecture.Web.GUI.WebControls;
using Enterprise.ZArchitecture.Web.Modules;
using Enterprise.ZArchitecture.Web.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Tracking.Module.Testing
{
	[TestedType(typeof(TrackingInventoryModule))]
	class TrackingInventoryModuleTest : ZFilterStripGridModuleTestCase
	{
		public void TestLoadCollection_SortedByArrivalDate()
		{
			SetupData();

			var filterBizO = FilterGridModule.CreateNewFilterBusinessObject();

			FilterGridModule.LoadCollection(filterBizO);
			var collection = (TrackingInventorySummaryCollection)FilterGridModule.GridCollection;
			CombineAssertions(() =>
			{
				AssertEquals("P3", collection[0].ProductCode);
				AssertEquals("P1", collection[1].ProductCode);
				AssertEquals("P2", collection[2].ProductCode);
				AssertEquals(3, collection.Count);
			});
		}

		void SetupData()
		{
			var warehouse = Helper.CreateWarehouse("WH1", "A", 1, 1);
			var contact = TestPage.SiteUser.LoggedInUser as OrgContact;
			var client = contact.ParentOrg;
			var product1 = Helper.CreateProduct(client, "P1");
			var product2 = Helper.CreateProduct(client, "P2");
			var product3 = Helper.CreateProduct(client, "P3");

			var now = ZDateTimeOffset.Now;
			Helper.CreateWhsReceiveWithInventory(client, warehouse, "R1", now.AddDays(-1), product1, 10, warehouse.DefaultLocation, "");
			Helper.CreateWhsReceiveWithInventory(client, warehouse, "R2", now, product2, 10, warehouse.DefaultLocation, "");
			Helper.CreateWhsReceiveWithInventory(client, warehouse, "R3", now.AddDays(-2), product3, 10, warehouse.DefaultLocation, "");
			Factory.Save();
		}

		int ExcelElementsCount;

		protected override void SetupForExcelExport()
		{
			base.SetupForExcelExport();

			ExcelElementsCount = 0;
		}

		protected override BusinessObject CreateNewElementForExcelExport()
		{
			var receive = Helper.CreateWhsReceive(SiteUser.LoggedInOrganisation, Data.Whs1, $"Receive{ExcelElementsCount}");
			var part = Helper.CreateProduct($"Part{ExcelElementsCount++}", SiteUser.LoggedInOrganisation);

			return Helper.CreateWhsReceiveInventoryLine(receive, part, 1m);
		}

		protected override bool AllowActiveStatusFilterTest() => false;

		public override void TestLoadCollectionReturnsRowCount()
		{
			var filterBizO = FilterGridModule.CreateNewFilterBusinessObject();

			FilterGridModule.MaxRows = 250;
			FilterGridModule.LoadCollection(filterBizO);
			if (FilterGridModule.GridCollection.TypeOfElements.IsClass && !FilterGridModule.GridCollection.TypeOfElements.IsSubclassOf(typeof(NonPersistentBusinessObject)))
			{
				var expectedCount = Factory.GetDatabaseCount(FilterGridModule.GridCollection.TypeOfElements, filterBizO.Filter);
				expectedCount = (expectedCount > FilterGridModule.MaxRows) ? FilterGridModule.MaxRows : expectedCount;
				AssertEquals("LoadCollection should not have returned more than 250 rows", expectedCount, FilterGridModule.GridCollection.Count);
			}
			else
			{
				Assert("LoadCollection should not have returned more than 250 rows", FilterGridModule.GridCollection.Count <= 250);
			}
		}

		#region Overrides

		protected override bool ExpectCachingOfCollectionKeys => false;

		#region TestCollectionLoadDBHitsWithDBOnlyQuery

		protected override bool ExpectDBHits => false;

		protected override Type GetCollectionElementType() => typeof(WhsInventoryView);

		protected override string GetTableName() => WhsTrackingInventorySummaryItemViewSchema.Constants.TableName;

		protected override List<BusinessObject> GetNewBusinessObjectsExpectedFromFilter()
		{
			var receive = Helper.CreateWhsReceive(SiteUser.LoggedInOrganisation, Data.Whs1, "R1");

			var result = new List<BusinessObject>();
			for (var i = 1; i < 10; i++)
			{
				var part = Factory.New<OrgSupplierPart>();
				Helper.CreateProductClientRelationShip(SiteUser.LoggedInOrganisation, part);
				part.OP_PartNum = string.Format("A{0}", i);
				result.Add(Helper.CreateWhsReceiveInventoryLine(receive, part.PK, 1m, ZGuid.Empty, string.Format("Include {0}", i)));
			}

			return result;
		}

		protected override List<BusinessObject> GetNewBusinessObjectsUnexpectedFromFilter()
		{
			var receive = Helper.CreateWhsReceive(SiteUser.LoggedInOrganisation, Data.Whs1, "R2");

			var result = new List<BusinessObject>();
			for (var i = 1; i < 10; i++)
			{
				var part = Factory.New<OrgSupplierPart>();
				part.OP_PartNum = string.Format("B{0}", i);
				result.Add(Helper.CreateWhsReceiveInventoryLine(receive, part.PK, 1m, ZGuid.Empty, string.Format("Other {0}", i)));
			}

			return result;
		}

		TestDataSimpleEnvironment Data => data ?? (data = new TestDataSimpleEnvironment(Factory));

		WhsTestHelperFunctions Helper => helper ?? (helper = new WhsTestHelperFunctions(Factory));

		TestDataSimpleEnvironment data;
		WhsTestHelperFunctions helper;

		protected override void SetupDBOnlyQuery(ZDBOnlyQuery filter)
		{
			filter.AddToFilter(WhsTrackingInventorySummaryItemViewSchema.WI_PalletID, SQLComparisonOperator.StartsWith, "Include");
		}

		#endregion

		protected override WebModuleID TestID => WebModuleIDs.TrackingInventory;

		protected override FilterLayoutCodePairRegistryItem ExpectedDefaultLayoutRegistryItem => WebDataRegistry.Instance.DefaultFilterLayoutWarehouseInventory;

		protected override string ExpectedDefaultLayoutName => DefaultLayoutNameValue;

		protected override ZWebModule GetNewZWebModule() => new TrackingInventoryModuleForTest(TestPage);

		#endregion

		#region TestFilterBusinessObjectSetup

		protected virtual FilterStripBusinessObject GetFilterStripBusinessObject() => ((TrackingInventoryModuleForTest)TestZWebModule).GetNewFilterStripBizOForTest();

		#endregion

		#region TestDecimalPlaces

		public void TestDecimalPlaces()
		{
			using (var module = FilterGridModule as TrackingInventoryModule)
			{
				AssertBindToDecimals(module.AllColumns["Available Pick Qty"] as ZCalcEditColumn);
				AssertBindToDecimals(module.AllColumns["Reserved Qty"] as ZCalcEditColumn);
				AssertBindToDecimals(module.AllColumns["Committed Qty"] as ZCalcEditColumn);
				AssertBindToDecimals(((ZGroupColumn)module.AllColumns["Total Qty"]).GroupMembers[0] as ZCalcEditColumn);
				AssertBindToDecimals(((ZGroupColumn)module.AllColumns["Client Qty"]).GroupMembers[0] as ZCalcEditColumn);

				AssertEquals(3, (module.AllColumns["Product Wt."] as ZCalcEditColumn).Decimals);
				AssertEquals(3, (module.AllColumns["Total Wt."] as ZCalcEditColumn).Decimals);
				AssertEquals(3, (module.AllColumns["Product Vol."] as ZCalcEditColumn).Decimals);
				AssertEquals(3, (module.AllColumns["Total Vol."] as ZCalcEditColumn).Decimals);
			}
		}

		void AssertBindToDecimals(ZCalcEditColumn column)
		{
			AssertEquals("SupplierPart+OP_CountDecimalPlaces", column.BindToDecimals);
		}

		#endregion

		#region TrackingInventoryModuleForTest

		class TrackingInventoryModuleForTest : TrackingInventoryModule
		{
			public TrackingInventoryModuleForTest(ZPage page) : base(new BusinessObjectFactory(), page) { }

			internal FilterStripBusinessObject GetNewFilterStripBizOForTest() => GetNewFilterStripBusinessObject();
		}

		#endregion

		#region TrackingInventoryModuleFilterBusinessObjectTest

		[TestedType(typeof(TrackingInventoryFilterBusinessObject))]
		public class TrackingInventoryModuleFilterBusinessObjectTest : WarehouseFilterBusinessObjectTest
		{
			// WARNING!!!
			// DO NOT REMOVE.
			// 
			// If this is failing it means the filter business object is not applying the tracker specific
			// filter for product. Please use the constructor overload of WhsInventoryLineFilterBusinessObject.cs
			// which allows the query to be passed in.

			protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
			{
				using (var module = new TrackingInventoryModuleForTest(new ZPage()))
				{
					return module.GetNewFilterStripBizOForTest();
				}
			}
		}

		#endregion

		#region TestInventoriesFromProhibitedWarehousesAreNotShown

		public void TestInventoriesFromProhibitedWarehousesAreNotShown()
		{
			AssertNotNull("Precondition:", TestPage.SiteUser.LoggedInUser);
			var whs1 = Helper.CreateWarehouse("WH1", "A", 1, 1);
			var whs2 = Helper.CreateWarehouse("WH2", "A", 1, 1);
			var contact = TestPage.SiteUser.LoggedInUser as OrgContact;
			AssertNotNull("PreCondition:", contact);
			var client = contact.ParentOrg;
			var product = Helper.CreateProduct(client, "P1");
			Helper.CreateWhsReceiveWithInventory(client, whs1, "R1", product, 10, whs1.DefaultLocation, "");
			Helper.CreateWhsReceiveWithInventory(client, whs2, "R2", product, 10, whs2.DefaultLocation, "");
			Factory.Save();

			FilterGridModule.LoadCollection(FilterGridModule.CreateNewFilterBusinessObject());
			AssertEquals(2, FilterGridModule.GridCollection.Count);

			helper.ProhibitWarehouseAccessForOrgContact(whs1, contact);
			Factory.Save();

			TestPage.Session[SearchControl.SearchControlIsSearchingIndexer] = true;
			FilterGridModule.LoadCollection(FilterGridModule.CreateNewFilterBusinessObject());
			AssertEquals(1, FilterGridModule.GridCollection.Count);

			AssertEquals("Only inventory for whs2 should be shown.", whs2.PK, (FilterGridModule.GridCollection[0] as TrackingInventorySummary).WI_WW_Whs);
		}

		#endregion

		#region Columns and Sorting

		protected override ColumnDetailsForTest[] ExpectedColumnDetails
		{
			get
			{
				var i = 0;
				return new[]
				{
					new ColumnDetailsForTest("Warehouse", i++, typeof(ZTextEditColumn)),
					new ColumnDetailsForTest("Product", i++, typeof(ZHyperLinkColumn)),
					new ColumnDetailsForTest("Description", i++, typeof(ZTextEditColumn)),
					new ColumnDetailsForTest("Image", i++, typeof(ZHyperLinkColumn)),
					new ColumnDetailsForTest("Available Pick Qty", i++, typeof(ZCalcEditColumn)),
					new ColumnDetailsForTest("Reserved Qty", i++, typeof(ZCalcEditColumn)),
					new ColumnDetailsForTest("Committed Qty", i++, typeof(ZCalcEditColumn)),
					new ColumnDetailsForTest("Client Qty", i++, typeof(ZGroupColumn)),
					new ColumnDetailsForTest("Total Qty", i++, typeof(ZGroupColumn)),
					new ColumnDetailsForTest("Product Wt.", i++, typeof(ZCalcEditColumn)),
					new ColumnDetailsForTest("Total Wt.", i++, typeof(ZCalcEditColumn)),
					new ColumnDetailsForTest("Wt. UQ", i++, typeof(ZTextEditColumn)),
					new ColumnDetailsForTest("Product Vol.", i++, typeof(ZCalcEditColumn)),
					new ColumnDetailsForTest("Total Vol.", i++, typeof(ZCalcEditColumn)),
					new ColumnDetailsForTest("Vol. UQ", i++, typeof(ZTextEditColumn)),
					new ColumnDetailsForTest("Total Value", i++, typeof(ZCalcEditColumn)),
					new ColumnDetailsForTest("Currency", i++, typeof(ZTextEditColumn)),
					new ColumnDetailsForTest("Last Cost", i++, typeof(ZCalcEditColumn))
				};
			}
		}

		protected override DataGridColumn[] ExpectedDefaultGridColumns
		{
			get
			{
				var result = new List<DataGridColumn>();

				using (TrackingInventoryModule module = FilterGridModule as TrackingInventoryModule)
				{
					result.Add(module.AllColumns["Description"]);
					result.Add(module.AllColumns["Image"]);
					result.Add(module.AllColumns["Available Pick Qty"]);
					result.Add(module.AllColumns["Reserved Qty"]);
					result.Add(module.AllColumns["Committed Qty"]);
					result.Add(module.AllColumns["Client Qty"]);
					result.Add(module.AllColumns["Total Qty"]);
				}

				return result.ToArray();
			}
		}

		protected override DataGridColumn[] ExpectedRequiredGridColumns => new[]
		{
			(FilterGridModule as TrackingInventoryModule).AllColumns["Warehouse"],
			(FilterGridModule as TrackingInventoryModule).AllColumns["Product"]
		};

		protected override ColumnAndSortOrder[] ExpectedSortInfos => new[] { new ColumnAndSortOrder(WhsTrackingInventorySummaryItemViewSchema.WI_ArrivalDate.Name, ListSortDirection.Ascending) };

		#endregion

		#region Implementation

		protected override List<string> ExcludeFromExcelExportColumnsBoundTo
		{
			get
			{
				var result = base.ExcludeFromExcelExportColumnsBoundTo;
				result.Add("Inventories");

				return result;
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			var user = TestPage.SiteUser as TrackingSiteUser;
			AssertNotNull(user);

			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "XXXYYYZZZ";
			org.OH_IsWarehouseClient = true;
			var contact = org.Contacts.AddNew();
			contact.OC_Email = "user@user.com";
			contact.SetHashedPassword("password");
			contact.OC_WebAccessEnabled = true;

			var contactSecurity = new List<OrgSecurityContacts>();
			var orgRight = org.SecurityRights.AddNew();
			orgRight.OX_Granted = true;
			orgRight.OX_SecurityItemName = WebSecurityRightsList.WebOrdersAddEdit.Code;
			var userRight = contact.SecurityRightsForBindingOnly.AddNew();
			userRight.OZ_OX = orgRight.PK;
			userRight.OZ_Granted = true;
			contactSecurity.Add(userRight);

			Factory.Save();

			user.Login(org.OH_Code, "user@user.com", "password");
		}

		protected override ZPage GetNewTestPage() => new DummyPage();

		protected override void CallTestPageLoadEvents(ZPage page)
		{
			((DummyPage)page).CallOnLoad();
			((DummyPage)page).CallOnLoadComplete();
		}

		class DummyPage : BasePage
		{
			protected override ZGlobal GetNewTestGlobal() => new TestGlobal();

			public void CallOnLoad() => base.OnLoad(EventArgs.Empty);

			public void CallOnLoadComplete() => base.OnLoadComplete(EventArgs.Empty);
		}

		protected override BusinessObject GetNewBizObjOfType(Type type)
		{
			if (type == typeof(TrackingInventorySummary))
			{
				return new TrackingInventorySummary(Factory.NewWithValidTestData<DynamicBusinessObject>(), null);
			}
			else
			{
				return base.GetNewBizObjOfType(type);
			}
		}

		#endregion
	}

	[TestedType(typeof(TrackingInventoryModule))]
	class TrackingInventoryModuleWithCachingTest : ZFilterGridModuleWithCachingTest
	{
		protected override void SetupData()
		{
			var helper = new WhsTestHelperFunctions(Factory);
			var warehouse = helper.CreateWarehouse("WH1", "A", 1, 1);
			var contact = TestPage.SiteUser.LoggedInUser as OrgContact;
			var client = contact.ParentOrg;
			var product1 = helper.CreateProduct(client, "P1");
			var product2 = helper.CreateProduct(client, "P2");
			var product3 = helper.CreateProduct(client, "P3");

			var now = ZDateTimeOffset.Now;
			helper.CreateWhsReceiveWithInventory(client, warehouse, "R1", now.AddDays(-1), product1, 10, warehouse.DefaultLocation, "");
			helper.CreateWhsReceiveWithInventory(client, warehouse, "R2", now, product2, 10, warehouse.DefaultLocation, "");
			helper.CreateWhsReceiveWithInventory(client, warehouse, "R3", now.AddDays(-2), product3, 10, warehouse.DefaultLocation, "");
			Factory.Save();
		}

		protected override ZFilterGridModule GetNewFilterGridModule() => new TrackingInventoryModule(Factory, TestPage);
	}
}
