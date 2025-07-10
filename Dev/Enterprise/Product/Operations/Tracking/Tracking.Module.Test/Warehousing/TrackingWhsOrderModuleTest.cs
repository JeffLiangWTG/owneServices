using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web.UI.WebControls;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Web;
using Enterprise.Tracking.Business;
using Enterprise.Tracking.Business.Testing;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Web.Business;
using Enterprise.ZArchitecture.Web.Business.Testing;
using Enterprise.ZArchitecture.Web.GUI.Testing;
using Enterprise.ZArchitecture.Web.GUI.WebControls;
using Enterprise.ZArchitecture.Web.Modules;
using Enterprise.ZArchitecture.Web.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Tracking.Module.Testing
{
	[TestedType(typeof(TrackingWhsOrderModule))]
	class TrackingWhsOrderModuleTest : ZFilterStripGridModuleTestCase
	{
		public void TestDefaultColumnsWhenMilestonesDisabled()
		{
			string oldValue = WebDataRegistry.Instance.MilestoneVisibility.Value;
			WebDataRegistry.Instance.MilestoneVisibility.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, MilestoneVisibilityList.Codes.All);
			var testModule = new TrackingWhsOrderModuleForTest(Factory, null);
			Assert(testModule.AllColumns.ContainsKey("Last Milestone Desc."));
			var hasMilestoneColumn = false;
			DataGridColumn[] defaultCols = testModule.ForTest_GetDefaultGridColumnFields();
			foreach (var col in defaultCols)
			{
				if (col.HeaderText == "Last Milestone Desc.")
				{
					hasMilestoneColumn = true;
					break;
				}
			}
			Assert(hasMilestoneColumn);

			WebDataRegistry.Instance.MilestoneVisibility.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, MilestoneVisibilityList.Codes.None);
			testModule.Dispose();
			testModule = new TrackingWhsOrderModuleForTest(Factory, null);
			Assert(!testModule.AllColumns.ContainsKey("Last Milestone Desc."));
			hasMilestoneColumn = false;
			defaultCols = testModule.ForTest_GetDefaultGridColumnFields();
			foreach (var col in defaultCols)
			{
				if (col.HeaderText == "Last Milestone Desc.")
				{
					hasMilestoneColumn = true;
					break;
				}
			}
			Assert(!hasMilestoneColumn);

			WebDataRegistry.Instance.MilestoneVisibility.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, oldValue);
			testModule.Dispose();
		}

		public void TestGetEDocsBulkDownloadRelevantPK()
		{
			var testHelper = new TestHelper(Factory);
			testHelper.TestSiteUser.Login(testHelper.TestOrg.OH_Code, testHelper.TestContact.OC_Email, testHelper.TestContact.PasswordForTesting);

			var filter = new TrackingWhsOrderFilterBusinessObject();

			var order1 = Factory.NewWithValidTestData<WhsOrder>();
			order1.WD_OH_Client = testHelper.TestOrg.PK;
			order1.WD_DocketID = "1234";

			var order2 = Factory.NewWithValidTestData<WhsOrder>();
			order2.WD_OH_Client = testHelper.TestOrg.PK;
			order2.WD_DocketID = "4321";

			Factory.Save();

			FilterGridModule.LoadCollection(filter);
			AssertEquals(2, FilterGridModule.GridCollection.Count);

			using (var module = new TrackingWhsOrderModule(Factory, null))
			{
				var grid = new TestHelper.ZTestDataGrid();
				grid.AllowPaging = true;
				grid.PageSize = 2;
				grid.BindTo = "GridCollection";
				grid.Bind(FilterGridModule);

				AssertEquals(1, grid.PageCount);

				var dataKeys = grid.ViewState_Exposed["DataKeys"] as System.Collections.ArrayList;
				dataKeys.Add(order1.PK);
				dataKeys.Add(order2.PK);

				var trackingOrder1PK = FilterGridModule.GridCollection.Cast<TrackingWhsOrder>().First(trackingWhsOrder => trackingWhsOrder.WhsOrder.PK == order1.PK);
				var trackingOrder2PK = FilterGridModule.GridCollection.Cast<TrackingWhsOrder>().First(trackingWhsOrder => trackingWhsOrder.WhsOrder.PK == order2.PK);

				var iSupportEDocsBulkDownload = module as ISupportEDocsBulkDownload;
				AssertEquals(order1.PK, iSupportEDocsBulkDownload.GetEDocsBulkDownloadRelevantPK(grid, 0));
				AssertEquals(order2.PK, iSupportEDocsBulkDownload.GetEDocsBulkDownloadRelevantPK(grid, 1));

				AssertEquals("Warehouse Order 1234", iSupportEDocsBulkDownload.GetPersistantBizoHumanReadableName(grid, trackingOrder1PK.PK));
				AssertEquals("Warehouse Order 4321", iSupportEDocsBulkDownload.GetPersistantBizoHumanReadableName(grid, trackingOrder2PK.PK));
			}
		}

		#region TestAdditionalInformationColuns

		public void TestAdditionalInformationColumns()
		{
			var testLoggedInOrg = ((OrgContactWebUser)TestPage.SiteUser).LoggedInOrganisation;

			try
			{
				Globals.IsWeb = true;
				GlbCompany.CurrentCompany.OrgProxy.CustomLabels.RemoveAndDeleteAll();
				testLoggedInOrg.CustomLabels.RemoveAndDeleteAll();

				var attribute1 = GlbCompany.CurrentCompany.OrgProxy.CustomLabels.AddNew();
				attribute1.OT_FieldName = Constants.CustomLabels.WhsDocket.CustomAttribute3;
				attribute1.OT_Caption = "OrgProxy'sCA3";

				var attribute2 = testLoggedInOrg.CustomLabels.AddNew();
				attribute2.OT_FieldName = Constants.CustomLabels.WhsDocket.CustomAttribute3;
				attribute2.OT_Caption = "LoggedInOrg'sCA3";

				var attribute3 = GlbCompany.CurrentCompany.OrgProxy.CustomLabels.AddNew();
				attribute3.OT_FieldName = Constants.CustomLabels.WhsDocket.CustomFlag4;
				attribute3.OT_Caption = "OrgProxy'sCF4";

				AssertEquals("Should be 1 more columns", ExpectedColumnDetails.Length + 1, ((TrackingWhsOrderModule)TestZWebModule).GridColumnFields.Length);
				AssertHeaderExistence("OrgProxy'sCA3", ((TrackingWhsOrderModule)TestZWebModule).GridColumnFields, false);
				AssertHeaderExistence("LoggedInOrg'sCA3", ((TrackingWhsOrderModule)TestZWebModule).GridColumnFields, true);
				AssertHeaderExistence("OrgProxy'sCF4 If company has its own settings, OrgProxy ones should not be used at all", ((TrackingWhsOrderModule)TestZWebModule).GridColumnFields, false);
			}
			finally
			{
				GlbCompany.CurrentCompany.OrgProxy.CustomLabels.RemoveAndDeleteAll();
				testLoggedInOrg.CustomLabels.RemoveAndDeleteAll();
				Globals.IsWeb = false;
			}
		}

		void AssertHeaderExistence(ZString header, DataGridColumn[] columns, ZBool shouldBeContained)
		{
			ZBool result = !shouldBeContained;
			foreach (var column in columns)
			{
				if (shouldBeContained ? column.HeaderText == header : column.HeaderText != header)
				{
					result = shouldBeContained;
					break;
				}
			}
			AssertEquals("Checking existence of " + header, shouldBeContained, result);
		}

		#endregion

		#region Overrides

		#region TestCollectionLoadDBHitsWithDBOnlyQuery

		protected override List<BusinessObject> GetNewBusinessObjectsExpectedFromFilter()
		{
			return createTestData("Include");
		}

		protected override List<BusinessObject> GetNewBusinessObjectsUnexpectedFromFilter()
		{
			return createTestData("Other");
		}

		List<BusinessObject> createTestData(string docketIdPrefix)
		{
			var result = new List<BusinessObject>();
			for (int i = 1; i < 10; i++)
			{
				var testObject = TrackingHelper.Get(Factory.NewWithValidTestData<WhsOrder>());
				testObject.WhsOrder.WD_DocketID = docketIdPrefix + i.ToString();
				testObject.WhsOrder.WD_OH_Client = SiteUser.LoggedInOrganisation.PK;
				result.Add(testObject);
			}
			return result;
		}

		protected override void SetupDBOnlyQuery(ZDBOnlyQuery filter)
		{
			filter.AddToFilter(WhsDocketSchema.WD_DocketID, SQLComparisonOperator.StartsWith, "Include");
		}

		#endregion

		protected override BusinessObject GetNewElement(Type elementType, bool isCancelled)
		{
			var element = TrackingHelper.Get(Factory.NewWithValidTestData<WhsOrder>());

			var order = element.WhsOrder;
			if (order != null)
			{
				order.WD_OH_Client = SiteUser.LoggedInOrganisation.PK;
			}

			return element;
		}

		protected override bool CanHaveInactiveElements(Type elementType)
		{
			return false;
		}

		protected override Dictionary<string, string> GetExpectedAuditFilters()
		{
			Dictionary<string, string> result = base.GetExpectedAuditFilters();
			result.Add("Created On Web/Internal", "Created On Web/Internal");
			result.Add("Created Time", "Created Time");
			result.Add("Last Edit Time", "Last Edit Time");

			return result;
		}

		protected override BusinessObject GetNewBizObjOfType(Type type)
		{
			if (type == typeof(TrackingWhsOrder))
			{
				return TrackingHelper.Get(Factory.New<WhsOrder>());
			}
			if (type == typeof(TrackingWhsOrderLine))
			{
				return TrackingHelper.Get(Factory.New<WhsOrderLine>());
			}
			if (type == typeof(TrackingMilestone))
			{
				return new TrackingMilestone(string.Empty, ZDateTimeOffset.Empty, ZDateTimeOffset.Empty, ZInt.Zero);
			}
			return base.GetNewBizObjOfType(type);
		}

		protected override WebModuleID TestID
		{
			get { return WebModuleIDs.TrackingWarehouseOrders; }
		}

		protected override FilterLayoutCodePairRegistryItem ExpectedDefaultLayoutRegistryItem
		{
			get { return WebDataRegistry.Instance.DefaultFilterLayoutWarehouseOrders; }
		}

		protected override string ExpectedDefaultLayoutName
		{
			get { return DefaultLayoutNameValue; }
		}

		#endregion

		#region Columns and Sorting

		protected override ZWebModule GetNewZWebModule()
		{
			return new TrackingWhsOrderModuleForTest(Factory, TestPage);
		}

		protected override ColumnAndSortOrder[] ExpectedSortInfos => new[] { new ColumnAndSortOrder(WhsDocketSchema.WD_RequiredDate.Name, ListSortDirection.Ascending) };

		protected override DataGridColumn[] ExpectedRequiredGridColumns
		{
			get
			{
				var module = FilterGridModule as TrackingWhsOrderModuleForTest;
				return new[] {
					module.AllColumns["Order#"]
				};
			}
		}

		protected override DataGridColumn[] ExpectedDefaultGridColumns
		{
			get
			{
				var result = new List<DataGridColumn>();

				using (var module = FilterGridModule as TrackingWhsOrderModuleForTest)
				{
					result.Add(module.AllColumns["Warehouse"]);
					result.Add(module.AllColumns["Consignee"]);
					result.Add(module.AllColumns["Transport Co."]);
					result.Add(module.AllColumns["Transport Ref."]);
					result.Add(module.AllColumns["Docket#"]);
					result.Add(module.AllColumns["Req. Date"]);
					result.Add(module.AllColumns["Status"]);
					result.Add(module.AllColumns["Cubic"]);
					result.Add(module.AllColumns["Finalized Date"]);
					if (WebDataRegistry.Instance.MilestoneVisibility.Value != MilestoneVisibilityList.Codes.None)
					{
						result.Add(module.AllColumns["Last Milestone Desc."]);
					}
				}

				return result.ToArray();
			}
		}

		protected override ColumnDetailsForTest[] ExpectedColumnDetails
		{
			get
			{
				var result = new List<ColumnDetailsForTest>();

				result.Add(new ColumnDetailsForTest("Order#", 0, typeof(ZHyperLinkColumn)));
				result.Add(new ColumnDetailsForTest("Warehouse", 1, typeof(ZTextEditColumn)));
				result.Add(new ColumnDetailsForTest("Consignee", 2, typeof(ZTextEditColumn)));
				result.Add(new ColumnDetailsForTest("Transport Co.", 3, typeof(ZTextEditColumn)));
				result.Add(new ColumnDetailsForTest("Transport Ref.", 4, typeof(ZHyperLinkColumn)));
				result.Add(new ColumnDetailsForTest("Docket#", 5, typeof(ZTextEditColumn)));
				result.Add(new ColumnDetailsForTest("Req. Date", 6, typeof(ZDateTimeColumn)));
				result.Add(new ColumnDetailsForTest("Status", 7, typeof(ZTextEditColumn)));
				result.Add(new ColumnDetailsForTest("Units", 8, typeof(ZTextEditColumn)));
				result.Add(new ColumnDetailsForTest("Weight", 9, typeof(ZTextEditColumn)));
				result.Add(new ColumnDetailsForTest("Cubic", 10, typeof(ZTextEditColumn)));
				result.Add(new ColumnDetailsForTest("Finalized Date", 11, typeof(ZDateTimeColumn)));
				result.Add(new ColumnDetailsForTest("Last Milestone Desc.", 12, typeof(ZTextEditColumn)));
				result.Add(new ColumnDetailsForTest("Last Milestone Date", 13, typeof(ZDateTimeColumn)));
				result.Add(new ColumnDetailsForTest("Next Milestone Desc.", 14, typeof(ZTextEditColumn)));
				result.Add(new ColumnDetailsForTest("Next Milestone Date", 15, typeof(ZDateTimeColumn)));
				result.Add(new ColumnDetailsForTest("Customer Ref.", 16, typeof(ZTextEditColumn)));
				return result.ToArray();
			}
		}

		#endregion

		#region TestOrdersFromProhibitedWarehousesAreNotShown

		public void TestOrdersFromProhibitedWarehousesAreNotShown()
		{
			var helper = new WhsTestHelperFunctions(Helper.TestContact.Factory);
			var whs1 = helper.CreateWarehouse("WH1");
			var whs2 = helper.CreateWarehouse("WH2");
			var order1 = helper.CreateWhsOrder(Helper.TestOrg, whs1, "O1");
			var order2 = helper.CreateWhsOrder(Helper.TestOrg, whs2, "O2");
			Helper.TestContact.Factory.Save();

			using (var module = new TrackingWhsOrderModuleForTest(Helper.TestContact.Factory, TestPage))
			{
				// no filters.
				module.LoadCollection(module.CreateNewFilterBusinessObject());
				AssertEquals("Both orders should be returned as there are no restricted warehouses yet.", 2, module.GridCollection.Count);

				helper.ProhibitWarehouseAccessForOrgContact(whs1, Helper.TestContact);
				Helper.TestContact.Factory.Save();
				module.LoadCollection(module.CreateNewFilterBusinessObject());
				AssertEquals("Only 1 order should be returned as user lost access to whs1 and orders for it.", 1, module.GridCollection.Count);
				AssertEquals("Only order2 should be returned in the list.", order2.PK, ((TrackingWhsOrder)module.GridCollection[0]).WhsOrder.PK);
			}
		}

		#endregion

		#region TrackingWhsOrderModule For Test

		protected class TrackingWhsOrderModuleForTest : TrackingWhsOrderModule
		{
			public TrackingWhsOrderModuleForTest(BusinessObjectFactory factory, ZPage page) : base(factory, page) { }

			protected override ZQuery GetCurrentLoggedInUserFilter(FilterBusinessObject filterBizO)
			{
				ZQuery currentLoggedInUserFilter = ZQuery.NoResultQuery;

				var siteUser = (OrgContactWebUser)Page.SiteUser;
				if (siteUser != null && siteUser.LoggedInOrganisation != null)
				{
					currentLoggedInUserFilter = OrgRestrictionFilterFactory.Instance.GetFilter<TrackingWhsOrder>();
				}

				return currentLoggedInUserFilter;
			}

			public DataGridColumn[] ForTest_GetDefaultGridColumnFields()
			{
				return this.GetDefaultGridColumnFields();
			}

			public FilterStripBusinessObject GetNewFilterStripBizOForTest()
			{
				return GetNewFilterStripBusinessObject();
			}
		}

		#endregion

		#region Implementation

		ZWebTestHelper Helper;
		protected override void SetUp()
		{
			TestPage = new ZTestPage();
			Helper = new ZWebTestHelper(Factory);
			Factory.Save();
			base.SetUp();
			TestPage.SiteUser.Login(Helper.TestOrg.OH_Code, Helper.TestContact.OC_Email, Helper.TestContact.PasswordForTesting);

			AssertNotNull("SiteUser.LoggedInOrganisation", ((OrgContactWebUser)TestPage.SiteUser).LoggedInOrganisation);
			AssertNotNull("SiteUser.LoggedInUser", ((OrgContactWebUser)TestPage.SiteUser).LoggedInUser);
		}

		#endregion
	}
}
