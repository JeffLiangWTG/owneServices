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
	[TestedType(typeof(TrackingInventoryDetailsModule))]
	sealed class TrackingInventoryDetailsModuleTest : ZFilterStripGridModuleTestCase
	{
		public override void TestLoadCollectionReturnsRowCount()
		{
			var filterBizO = FilterGridModule.CreateNewFilterBusinessObject();

			FilterGridModule.MaxRows = 250;
			FilterGridModule.LoadCollection(filterBizO);
			if (FilterGridModule.GridCollection.TypeOfElements.IsClass && !FilterGridModule.GridCollection.TypeOfElements.IsSubclassOf(typeof(NonPersistentBusinessObject)))
			{
				int expectedCount = Factory.GetDatabaseCount(FilterGridModule.GridCollection.TypeOfElements, filterBizO.Filter);
				expectedCount = (expectedCount > FilterGridModule.MaxRows) ? FilterGridModule.MaxRows : expectedCount;
				AssertEquals("LoadCollection should not have returned more than 250 rows", expectedCount, FilterGridModule.GridCollection.Count);
			}
			else
			{
				Assert("LoadCollection should not have returned more than 250 rows", FilterGridModule.GridCollection.Count <= 250);
			}
		}

		[TestDate(2019, 11, 11)]
		public void TestIModuleManualSort_Load()
		{
			var today = ZDateTimeOffset.Now;
			var testHelper = new TestHelper(Factory);
			var whs = Helper.CreateWarehouse("AAA", "A", 3, 1);
			var client = testHelper.TestOrg;
			var part1 = Helper.CreateProduct("Part1", client);
			var part2 = Helper.CreateProduct("Part2", client);
			var part3 = Helper.CreateProduct("Part3", client);

			Helper.SetClientAllAttributeType(client, false);
			Helper.SetProductAllAttributeUse(client, part3, true);
			var location = whs.FindLocation("A-1");
			Factory.Save();

			var receive1 = Helper.CreateWhsReceive(client.PK, whs.PK, "R1", today);
			var receiveLine11 = Helper.CreateWhsReceiveLine(receive1, part1, 10m, location, "PLT1");
			var receiveLine12 = Helper.CreateWhsReceiveLine(receive1, part2, 10m, location, "PLT1");
			var receiveLine13 = Helper.CreateWhsReceiveLine(receive1, part1, 10m, location, "PLT2");

			var receive2 = Helper.CreateWhsReceive(client.PK, whs.PK, "R2", today);
			var receiveLine21 = Helper.CreateWhsReceiveLine(receive2, part1, 10m, location, "PLT1");

			var receive3 = Helper.CreateWhsReceive(client.PK, whs.PK, "R3", today.AddDays(-1));
			var receiveLine31 = Helper.CreateWhsReceiveLine(receive3, part2, 10m, location, "PLT5");

			var receive4 = Helper.CreateWhsReceive(client.PK, whs.PK, "R4", today);
			var receiveLine41 = Helper.CreateWhsReceiveLine(receive4, part3, 10m, location, "PLT1", ZDate.Today, ZDate.Today, "A1", "A1", "A1", "");
			var receiveLine42 = Helper.CreateWhsReceiveLine(receive4, part3, 10m, location, "PLT1", ZDate.Today, ZDate.Today, "A1", "A2", "A1", "");
			var receiveLine43 = Helper.CreateWhsReceiveLine(receive4, part3, 10m, location, "PLT1", ZDate.Today, ZDate.Today, "A1", "A2", "A3", "");
			Factory.Save();

			var page = new ZPage();
			page.SiteUser.Login(testHelper.TestOrg.OH_Code, testHelper.TestContact.OC_Email, testHelper.TestContact.PasswordForTesting);
			using (var module = new TrackingInventoryDetailsModuleForTest(page))
			{
				var filterBizO = module.CreateNewFilterBusinessObject();
				var sortInfos = module.GetSortInfos(filterBizO);
				var sorts = module.GetListSortDescriptionCollectionForTest(sortInfos);

				module.LoadCollection(filterBizO);

				var collection = module.GridCollection as TrackingWhsInventoryCollection;
				AssertEquals("Should load all inventories", 8, collection.Count);
				AssertEquals("Order by Arrival Date", receiveLine31.PK, collection[0].PK);
				AssertEquals("Order by Receipt Reference", receiveLine11.PK, collection[1].PK);
				AssertEquals("Order by Product Code", receiveLine13.PK, collection[2].PK);
				AssertEquals("Order by Pallet ID", receiveLine12.PK, collection[3].PK);
				AssertEquals("Order by Receipt Reference", receiveLine21.PK, collection[4].PK);
				AssertEquals("Order by Part Attribute 1", receiveLine41.PK, collection[5].PK);
				AssertEquals("Order by Part Attribute 2", receiveLine42.PK, collection[6].PK);
				AssertEquals("Order by Part Attribute 3", receiveLine43.PK, collection[7].PK);
			}
		}

		[TestDate(2021, 08, 04)]
		public void TestIModuleManualSort_Load_WithSerialNumber()
		{
			var today = ZDateTimeOffset.Now;
			var testHelper = new TestHelper(Factory);
			var whs = Helper.CreateWarehouse("AAA", "A", 3, 1);
			var client = testHelper.TestOrg;
			var part = Helper.CreateProduct("Part", client);

			Helper.SetClientAllAttributeType(client, false);
			Helper.SetProductAllAttributeUse(client, part, true);
			var location = whs.FindLocation("A-1");
			Factory.Save();

			var receive = Helper.CreateWhsReceive(client.PK, whs.PK, "R1", today);
			var receiveLine1 = Helper.CreateWhsReceiveLine(receive, part, 1m, location, "PLT1", ZDate.Today, ZDate.Today, "A1", "A2", "A3", "SN1");
			receiveLine1.Inventory[0].WI_SerialNumber = "SN1";
			var receiveLine2 = Helper.CreateWhsReceiveLine(receive, part, 1m, location, "PLT1", ZDate.Today, ZDate.Today, "A1", "A2", "A3", "SN3");
			receiveLine2.Inventory[0].WI_SerialNumber = "SN3";
			var receiveLine3 = Helper.CreateWhsReceiveLine(receive, part, 1m, location, "PLT1", ZDate.Today, ZDate.Today, "A1", "A2", "A3", "SN2");
			receiveLine3.Inventory[0].WI_SerialNumber = "SN2";
			Factory.Save();

			var page = new ZPage();
			page.SiteUser.Login(testHelper.TestOrg.OH_Code, testHelper.TestContact.OC_Email, testHelper.TestContact.PasswordForTesting);
			using (var module = new TrackingInventoryDetailsModuleForTest(page))
			{
				var filterBizO = module.CreateNewFilterBusinessObject();
				var sortInfos = module.GetSortInfos(filterBizO);
				var sorts = module.GetListSortDescriptionCollectionForTest(sortInfos);

				module.LoadCollection(filterBizO);

				var collection = module.GridCollection as TrackingWhsInventoryCollection;
				AssertEquals("Should load all inventories", 3, collection.Count);
				AssertEquals("Order by Serial Number", receiveLine1.PK, collection[0].PK);
				AssertEquals("Order by Serial Number", receiveLine3.PK, collection[1].PK);
				AssertEquals("Order by Serial Number", receiveLine2.PK, collection[2].PK);
			}
		}

		#region Overrides

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

		#region TestCollectionLoadDBHitsWithDBOnlyQuery

		protected override Type GetCollectionElementType() => typeof(WhsInventoryView);

		protected override string GetTableName() => AutoWhsInventoryView.Schema.TableName;

		protected override List<BusinessObject> GetNewBusinessObjectsExpectedFromFilter()
		{
			var receive = Helper.CreateWhsReceive(SiteUser.LoggedInOrganisation, Data.Whs1, "R1");

			var result = new List<BusinessObject>();
			for (int i = 1; i < 10; i++)
			{
				result.Add(Helper.CreateWhsReceiveInventoryLine(receive, Data.Part1.PK, 1m, ZGuid.Empty, string.Format("Include {0}", i)));
			}

			return result;
		}

		protected override List<BusinessObject> GetNewBusinessObjectsUnexpectedFromFilter()
		{
			var receive = Helper.CreateWhsReceive(SiteUser.LoggedInOrganisation, Data.Whs1, "R2");

			var result = new List<BusinessObject>();
			for (int i = 1; i < 10; i++)
			{
				result.Add(Helper.CreateWhsReceiveInventoryLine(receive, Data.Part1.PK, 1m, ZGuid.Empty, string.Format("Other {0}", i)));
			}

			return result;
		}

		TestDataSimpleEnvironment Data => data ?? (data = new TestDataSimpleEnvironment(Factory));
		TestDataSimpleEnvironment data;

		WhsTestHelperFunctions Helper => helper ?? (helper = new WhsTestHelperFunctions(Factory));
		WhsTestHelperFunctions helper;

		protected override void SetupDBOnlyQuery(ZDBOnlyQuery filter)
		{
			filter.AddToFilter(WhsInventoryViewSchema.WI_PalletID, SQLComparisonOperator.StartsWith, "Include");
		}

		#endregion

		protected override bool AllowActiveStatusFilterTest() => false;

		protected override WebModuleID TestID => WebModuleIDs.TrackingInventoryDetails;

		protected override FilterLayoutCodePairRegistryItem ExpectedDefaultLayoutRegistryItem => WebDataRegistry.Instance.DefaultFilterLayoutWarehouseInventory;

		protected override string ExpectedFilterStripLayoutContext => WebModuleIDs.TrackingInventory.Name;

		protected override ZWebModule GetNewZWebModule() => new TrackingInventoryDetailsModuleForTest(TestPage);

		#endregion

		#region TrackingInventoryDetailsModuleForTest

		class TrackingInventoryDetailsModuleForTest : TrackingInventoryDetailsModule
		{
			public TrackingInventoryDetailsModuleForTest(ZPage page) : base(new BusinessObjectFactory(), page) { }

			internal FilterStripBusinessObject GetNewFilterStripBizOForTest() => GetNewFilterStripBusinessObject();

			public ListSortDescriptionCollection GetListSortDescriptionCollectionForTest(ColumnAndSortOrder[] sortInfos) => GetListSortDescriptionCollection(sortInfos);
		}

		#endregion

		#region TrackingInventoryDetailsModuleFilterBusinessObjectTest

		[TestedType(typeof(TrackingInventoryFilterBusinessObject))]
		public class TrackingInventoryDetailsModuleFilterBusinessObjectTest : WarehouseFilterBusinessObjectTest
		{
			// WARNING!!!
			// DO NOT REMOVE.
			// 
			// If this is failing it means the filter business object is not applying the tracker specific
			// filter for product. Please use the constructor overload of WhsInventoryLineFilterBusinessObject.cs
			// which allows the query to be passed in.

			protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
			{
				using (var module = new TrackingInventoryDetailsModuleForTest(new ZPage()))
				{
					return module.GetNewFilterStripBizOForTest();
				}
			}
		}

		#endregion

		#region Columns and Sorting

		protected override ColumnDetailsForTest[] ExpectedColumnDetails
		{
			get
			{
				var result = new List<ColumnDetailsForTest>();

				result.Add(new ColumnDetailsForTest("Receipt Ref", 0, typeof(ZHyperLinkColumn)));
				result.Add(new ColumnDetailsForTest("Product", 1, typeof(ZTextEditColumn)));
				result.Add(new ColumnDetailsForTest("ETA/Arrival", 2, typeof(ZDateTimeColumn)));
				result.Add(new ColumnDetailsForTest("Status", 3, typeof(ZTextEditColumn)));
				result.Add(new ColumnDetailsForTest("Available Pick Qty", 4, typeof(ZCalcEditColumn)));
				result.Add(new ColumnDetailsForTest("Committed Qty", 5, typeof(ZCalcEditColumn)));
				result.Add(new ColumnDetailsForTest("Reserved Qty", 6, typeof(ZCalcEditColumn)));
				result.Add(new ColumnDetailsForTest("Total Qty", 7, typeof(ZCalcEditColumn)));
				result.Add(new ColumnDetailsForTest("Total Value", 8, typeof(ZCalcEditColumn)));
				result.Add(new ColumnDetailsForTest("Currency", 9, typeof(ZTextEditColumn)));
				result.Add(new ColumnDetailsForTest("Last Cost", 10, typeof(ZCalcEditColumn)));

				return result.ToArray();
			}
		}

		protected override DataGridColumn[] ExpectedDefaultGridColumns
		{
			get
			{
				var result = new List<DataGridColumn>();

				using (var module = FilterGridModule as TrackingInventoryDetailsModule)
				{
					result.Add(module.AllColumns["Product"]);
					result.Add(module.AllColumns["ETA/Arrival"]);
				}

				return result.ToArray();
			}
		}

		protected override DataGridColumn[] ExpectedRequiredGridColumns
		{
			get
			{
				var result = new List<DataGridColumn>();

				using (var module = FilterGridModule as TrackingInventoryDetailsModule)
				{
					result.Add(module.AllColumns["Receipt Ref"]);
				}

				return result.ToArray();
			}
		}

		protected override ColumnAndSortOrder[] ExpectedSortInfos
		{
			get
			{
				var sortInfos = new List<ColumnAndSortOrder>();
				sortInfos.Add(new ColumnAndSortOrder(TrackingWhsInventory.Schema.TrackingSerialNumber, ListSortDirection.Ascending));
				sortInfos.Add(new ColumnAndSortOrder(TrackingWhsInventory.Schema.TrackingPartAttrib3, ListSortDirection.Ascending));
				sortInfos.Add(new ColumnAndSortOrder(TrackingWhsInventory.Schema.TrackingPartAttrib2, ListSortDirection.Ascending));
				sortInfos.Add(new ColumnAndSortOrder(TrackingWhsInventory.Schema.TrackingPartAttrib1, ListSortDirection.Ascending));
				sortInfos.Add(new ColumnAndSortOrder(TrackingWhsInventory.Schema.TrackingPalletID, ListSortDirection.Ascending));
				sortInfos.Add(new ColumnAndSortOrder(TrackingWhsInventory.Schema.TrackingProductCode, ListSortDirection.Ascending));
				sortInfos.Add(new ColumnAndSortOrder(TrackingWhsInventory.Schema.TrackingReceiptReference, ListSortDirection.Ascending));
				sortInfos.Add(new ColumnAndSortOrder(TrackingWhsInventory.Schema.TrackingArrivalDate, ListSortDirection.Ascending));

				return sortInfos.ToArray();
			}
		}

		#endregion

		#region Implementation

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

		#endregion
	}
}
