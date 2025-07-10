using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Web.UI.WebControls;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Web;
using Enterprise.Tracking.Business;
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
	[TestedType(typeof(TrackingWhsReceiveModule))]
	class TrackingWhsReceiveModuleTest : ZFilterStripGridModuleTestCase
	{
		public void TestDefaultColumnsWhenMilestonesDisabled()
		{
			string oldValue = WebDataRegistry.Instance.MilestoneVisibility.Value;
			WebDataRegistry.Instance.MilestoneVisibility.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, MilestoneVisibilityList.Codes.All);
			TrackingWhsReceiveModuleForTest testModule = new TrackingWhsReceiveModuleForTest(Factory, null);
			Assert(testModule.AllColumns.ContainsKey("Last Milestone Desc."));
			bool hasMilestoneColumn = false;
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
			testModule = new TrackingWhsReceiveModuleForTest(Factory, null);
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

		#region TestAdditionalInformationColuns

		[SetGlobalsIsWeb]
		public void TestAdditionalInformationColumns()
		{
			OrgHeader testLoggedInOrg = ((OrgContactWebUser)TestPage.SiteUser).LoggedInOrganisation;

			GlbCompany.CurrentCompany.OrgProxy.CustomLabels.RemoveAndDeleteAll();
			testLoggedInOrg.CustomLabels.RemoveAndDeleteAll();

			OrgCustomLabels attribute1 = GlbCompany.CurrentCompany.OrgProxy.CustomLabels.AddNew();
			attribute1.OT_FieldName = Constants.CustomLabels.WhsDocket.CustomAttribute3;
			attribute1.OT_Caption = "OrgProxy'sCA3";

			OrgCustomLabels attribute2 = testLoggedInOrg.CustomLabels.AddNew();
			attribute2.OT_FieldName = Constants.CustomLabels.WhsDocket.CustomAttribute3;
			attribute2.OT_Caption = "LoggedInOrg'sCA3";

			OrgCustomLabels attribute3 = GlbCompany.CurrentCompany.OrgProxy.CustomLabels.AddNew();
			attribute3.OT_FieldName = Constants.CustomLabels.WhsDocket.CustomFlag4;
			attribute3.OT_Caption = "OrgProxy'sCF4";

			AssertEquals("Should be 1 more columns", ExpectedColumnDetails.Length + 1, ((TrackingWhsReceiveModule)TestZWebModule).GridColumnFields.Length);
			AssertHeaderExistence("OrgProxy'sCA3", ((TrackingWhsReceiveModule)TestZWebModule).GridColumnFields, false);
			AssertHeaderExistence("LoggedInOrg'sCA3", ((TrackingWhsReceiveModule)TestZWebModule).GridColumnFields, true);
			AssertHeaderExistence("OrgProxy'sCF4 If company has its own settings, OrgProxy ones should not be used at all", ((TrackingWhsReceiveModule)TestZWebModule).GridColumnFields, false);
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
			List<BusinessObject> result = new List<BusinessObject>();
			for (int i = 1; i < 10; i++)
			{
				TrackingWhsReceive testObject = TrackingHelper.Get(Factory.NewWithValidTestData<WhsReceive>());
				testObject.WhsReceive.WD_DocketID = "Include" + i.ToString();
				testObject.WhsReceive.WD_OH_Client = SiteUser.LoggedInOrganisation.PK;
				result.Add(testObject);
			}
			return result;
		}

		protected override List<BusinessObject> GetNewBusinessObjectsUnexpectedFromFilter()
		{
			return createTestData("Other");
		}

		List<BusinessObject> createTestData(string docketIdPrefix)
		{
			List<BusinessObject> result = new List<BusinessObject>();
			for (int i = 1; i < 10; i++)
			{
				var testObject = TrackingHelper.Get(Factory.NewWithValidTestData<WhsReceive>());
				testObject.WhsReceive.WD_DocketID = "Other" + i.ToString();
				testObject.WhsReceive.WD_OH_Client = SiteUser.LoggedInOrganisation.PK;
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
			var element = TrackingHelper.Get(Factory.NewWithValidTestData<WhsReceive>());
			var receive = element.WhsReceive;
			if (receive != null)
			{
				receive.WD_OH_Client = SiteUser.LoggedInOrganisation.PK;
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
			if (type == typeof(TrackingWhsReceive))
			{
				return TrackingHelper.Get(Factory.New<WhsReceive>());
			}
			if (type == typeof(TrackingWhsReceiveLine))
			{
				return TrackingHelper.Get(Factory.New<WhsReceiveLine>());
			}
			if (type == typeof(TrackingMilestone))
			{
				return new TrackingMilestone(string.Empty, ZDateTimeOffset.Empty, ZDateTimeOffset.Empty, ZInt.Zero);
			}
			return base.GetNewBizObjOfType(type);
		}

		protected override WebModuleID TestID
		{
			get { return WebModuleIDs.TrackingWarehouseReceive; }
		}

		protected override FilterLayoutCodePairRegistryItem ExpectedDefaultLayoutRegistryItem
		{
			get
			{
				return WebDataRegistry.Instance.DefaultFilterLayoutWarehouseReceipts;
			}
		}

		protected override string ExpectedDefaultLayoutName
		{
			get
			{
				return DefaultLayoutNameValue;
			}
		}

		protected override ZWebModule GetNewZWebModule()
		{
			return new TrackingWhsReceiveModuleForTest(Factory, TestPage);
		}

		#endregion

		#region Columns and Sorting

		protected override ColumnDetailsForTest[] ExpectedColumnDetails
		{
			get
			{
				List<ColumnDetailsForTest> result = new List<ColumnDetailsForTest>();

				result.Add(new ColumnDetailsForTest("Receive Ref. #", 0, typeof(ZHyperLinkColumn)));
				result.Add(new ColumnDetailsForTest("Warehouse", 1, typeof(ZFindBoxColumn)));
				result.Add(new ColumnDetailsForTest("Docket #", 2, typeof(ZTextEditColumn)));
				result.Add(new ColumnDetailsForTest("Booking Date", 3, typeof(ZDateTimeColumn)));
				result.Add(new ColumnDetailsForTest("ETA", 4, typeof(ZDateTimeColumn)));
				result.Add(new ColumnDetailsForTest("Arrival Date", 5, typeof(ZDateTimeColumn)));
				result.Add(new ColumnDetailsForTest("Status", 6, typeof(ZTextEditColumn)));
				result.Add(new ColumnDetailsForTest("Total Units", 7, typeof(ZTextEditColumn)));
				result.Add(new ColumnDetailsForTest("Total Pallets", 8, typeof(ZTextEditColumn)));
				result.Add(new ColumnDetailsForTest("Finalized Date", 9, typeof(ZDateTimeColumn)));
				result.Add(new ColumnDetailsForTest("Last Milestone Desc.", 10, typeof(ZTextEditColumn)));
				result.Add(new ColumnDetailsForTest("Last Milestone Date", 11, typeof(ZDateTimeColumn)));
				result.Add(new ColumnDetailsForTest("Next Milestone Desc.", 12, typeof(ZTextEditColumn)));
				result.Add(new ColumnDetailsForTest("Next Milestone Date", 13, typeof(ZDateTimeColumn)));
				result.Add(new ColumnDetailsForTest("Customer Ref.", 14, typeof(ZTextEditColumn)));

				return result.ToArray();
			}
		}

		protected override DataGridColumn[] ExpectedDefaultGridColumns
		{
			get
			{
				List<DataGridColumn> result = new List<DataGridColumn>();

				using (TrackingWhsReceiveModuleForTest module = FilterGridModule as TrackingWhsReceiveModuleForTest)
				{
					result.Add(module.AllColumns["Warehouse"]);
					result.Add(module.AllColumns["Docket #"]);
					result.Add(module.AllColumns["Booking Date"]);
					result.Add(module.AllColumns["ETA"]);
					result.Add(module.AllColumns["Arrival Date"]);
					result.Add(module.AllColumns["Status"]);
					result.Add(module.AllColumns["Total Units"]);
					result.Add(module.AllColumns["Total Pallets"]);
					result.Add(module.AllColumns["Finalized Date"]);
					result.Add(module.AllColumns["Last Milestone Desc."]);
				}

				return result.ToArray();
			}
		}

		protected override DataGridColumn[] ExpectedRequiredGridColumns
		{
			get
			{
				var module = FilterGridModule as TrackingWhsReceiveModuleForTest;
				return new[] {
					module.AllColumns["Receive Ref. #"]
				};
			}
		}

		protected override ColumnAndSortOrder[] ExpectedSortInfos => new[] { new ColumnAndSortOrder(WhsDocketSchema.WD_BookingDate.Name, ListSortDirection.Descending) };

		protected override ListSortDirection ExpectedDefaultSortOrder
		{
			get { return ListSortDirection.Descending; }
		}

		#endregion

		#region TrackingWhsReceiveModule For Test

		protected class TrackingWhsReceiveModuleForTest : TrackingWhsReceiveModule
		{
			public TrackingWhsReceiveModuleForTest(BusinessObjectFactory factory, ZPage page)
				: base(factory, page)
			{
			}

			protected override ZQuery GetCurrentLoggedInUserFilter(FilterBusinessObject filterBizO)
			{
				ZQuery currentLoggedInUserFilter = ZQuery.NoResultQuery;

				OrgContactWebUser siteUser = (OrgContactWebUser)Page.SiteUser;
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

		#region TestReceivesFromProhibitedWarehousesAreNotShown

		public void TestReceivesFromProhibitedWarehousesAreNotShown()
		{
			var helper = new WhsTestHelperFunctions(Helper.TestContact.Factory);
			var whs1 = helper.CreateWarehouse("WH1");
			var whs2 = helper.CreateWarehouse("WH2");
			var receive1 = helper.CreateWhsReceive(Helper.TestOrg, whs1, "R1");
			var receive2 = helper.CreateWhsReceive(Helper.TestOrg, whs2, "R2");
			Helper.TestContact.Factory.Save();

			using (var module = new TrackingWhsReceiveModuleForTest(Helper.TestContact.Factory, TestPage))
			{
				// no filters.
				module.LoadCollection(module.CreateNewFilterBusinessObject());
				AssertEquals("Both receives should be returned as there are no restricted warehouses yet.", 2, module.GridCollection.Count);

				helper.ProhibitWarehouseAccessForOrgContact(whs1, Helper.TestContact);
				Helper.TestContact.Factory.Save();
				module.LoadCollection(module.CreateNewFilterBusinessObject());
				AssertEquals("Only 1 receive should be returned as user lost access to whs1 and receives for it.", 1, module.GridCollection.Count);
				AssertEquals("Only receive2 should be returned in the list.", receive2.PK, ((TrackingWhsReceive)module.GridCollection[0]).WhsReceive.PK);
			}
		}

		#endregion

		#region Implementation

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
		ZWebTestHelper Helper;

		protected override IList GetNewFilterGridCollection()
		{
			return new List<TrackingWhsReceive>(); // Required for TestActiveStatusFilter(), collection used like ad hoc.
		}

		#endregion
	}
}
