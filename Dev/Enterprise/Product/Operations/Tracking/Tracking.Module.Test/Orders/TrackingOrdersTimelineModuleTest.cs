using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Web.UI.WebControls;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Orders.Module;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.Tracking.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Web.Business;
using Enterprise.ZArchitecture.Web.Business.Testing;
using Enterprise.ZArchitecture.Web.GUI.Testing;
using Enterprise.ZArchitecture.Web.GUI.WebControls;
using Enterprise.ZArchitecture.Web.Modules;
using NUnit.Framework;

namespace Enterprise.Tracking.Module.Testing
{
	[TestedType(typeof(TrackingOrdersTimelineModule))]
	sealed class TrackingOrdersTimelineModuleTest : TrackingOrdersModuleTest
	{
		#region Overrides

		protected override BusinessObject GetNewBizObjOfType(Type type)
		{
			if (type == typeof(TrackingMilestone))
			{
				return new TrackingMilestone(string.Empty, ZDateTimeOffset.Empty, ZDateTimeOffset.Empty, ZInt.Zero);
			}
			return base.GetNewBizObjOfType(type);
		}

		protected override WebModuleID TestID
		{
			get
			{
				return WebModuleIDs.TrackingOrdersTimeline;
			}
		}

		protected override FilterLayoutCodePairRegistryItem ExpectedDefaultLayoutRegistryItem
		{
			get
			{
				return WebDataRegistry.Instance.DefaultFilterLayoutForwardingOrders;
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
			return new TrackingOrdersTimelineModuleForTest(Factory, TestPage);
		}

		protected override string ExpectedFilterStripLayoutContext
		{
			get
			{
				return WebModuleIDs.TrackingOrders.Name;
			}
		}

		protected override void TestLoadCollectionInternal(ZFilterPage page)
		{
			ZWebTestHelper helper = GetNewHelper();
			page.SiteUser.Login(helper.TestOrg.OH_Code, helper.TestContact.OC_Email, helper.TestContact.PasswordForTesting);
			base.TestLoadCollectionInternal(page);
		}

		public override void TestFilterBusinessObjectSetup()
		{
			AssertNotNull("LoggedInOrg should be set on OrdersFilterBusinessObject creation", ((OrdersFilterBusinessObject)((TrackingOrdersTimelineModuleForTest)TestZWebModule).GetNewFilterStripBizOForTest()).LoggedInWebUsersOrg);
		}

		#endregion

		#region Columns and Sorting

		protected override DataGridColumn[] ExpectedDefaultGridColumns
		{
			get
			{
				List<DataGridColumn> result = new List<DataGridColumn>();

				using (TrackingOrdersModule module = FilterGridModule as TrackingOrdersModule)
				{
					result.Add(module.AllColumns["Split Number"]);
					result.Add(module.AllColumns["Transport Mode"]);
					result.Add(module.AllColumns["Supplier"]);
					result.Add(module.AllColumns["Buyer"]);
					result.Add(module.AllColumns["Controlling Customer"]);
					result.Add(module.AllColumns["Status"]);
					result.Add(module.AllColumns["Order Date"]);
					result.Add(module.AllColumns["Origin"]);
					result.Add(module.AllColumns["Destination"]);
					result.Add(module.AllColumns["Packs"]);
					result.Add(module.AllColumns["Volume"]);
					result.Add(module.AllColumns["Weight"]);
				}

				return result.ToArray();
			}
		}

		protected override DataGridColumn[] ExpectedRequiredGridColumns
		{
			get
			{
				var module = FilterGridModule as TrackingOrdersModule;
				return new[] {
					module.AllColumns["Order #"]
				};
			}
		}

		protected override ColumnAndSortOrder[] ExpectedSortInfos => new[] { new ColumnAndSortOrder(JobOrderHeaderSchema.JD_OrderDate.Name, ListSortDirection.Descending) };
		protected override ListSortDirection ExpectedDefaultSortOrder
		{
			get
			{
				return ListSortDirection.Descending;
			}
		}

		#endregion

		#region TestGetNewGridColumnFields

		public void TestGetNewGridColumnFields()
		{
			OrgHeader loggedInOrganisation = ((OrgContactWebUser)TestPage.AppInstance.SiteUser).LoggedInOrganisation;
			OrgCustomLabels customLabels1 = loggedInOrganisation.CustomLabels.AddNew();
			customLabels1.OT_FieldName = "OrderHeader.UserTrackDate1";
			customLabels1.OT_Caption = "Test date 1";

			OrgCustomLabels customLabels2 = loggedInOrganisation.CustomLabels.AddNew();
			customLabels2.OT_FieldName = "OrderHeader.UserTrackDate2";
			customLabels2.OT_Caption = "Test date 2";

			OrgCustomLabels customLabels3 = loggedInOrganisation.CustomLabels.AddNew();
			customLabels3.OT_FieldName = "OrderHeader.UserTrackDate3";
			customLabels3.OT_Caption = "Test date 3";

			OrgCustomLabels customLabels4 = loggedInOrganisation.CustomLabels.AddNew();
			customLabels4.OT_FieldName = "OrderHeader.UserTrackDate4";
			customLabels4.OT_Caption = "Test date 4";

			using (TrackingOrdersTimelineModuleForTest timelineModule = new TrackingOrdersTimelineModuleForTest(new BusinessObjectFactory(), TestPage))
			{
				DataGridColumn[] columns = timelineModule.GetNewGridColumnFieldsForTest();

				AssertEquals("Est. " + customLabels1.OT_Caption, RemoveBrTags(columns[51].HeaderText));
				AssertEquals("Act. " + customLabels1.OT_Caption, RemoveBrTags(columns[52].HeaderText));
				AssertEquals("Est. " + customLabels2.OT_Caption, RemoveBrTags(columns[53].HeaderText));
				AssertEquals("Act. " + customLabels2.OT_Caption, RemoveBrTags(columns[54].HeaderText));
				AssertEquals("Est. " + customLabels3.OT_Caption, RemoveBrTags(columns[55].HeaderText));
				AssertEquals("Act. " + customLabels3.OT_Caption, RemoveBrTags(columns[56].HeaderText));
				AssertEquals("Est. " + customLabels4.OT_Caption, RemoveBrTags(columns[57].HeaderText));
				AssertEquals("Act. " + customLabels4.OT_Caption, RemoveBrTags(columns[58].HeaderText));
			}
		}

		#endregion

		#region TrackingOrdersTimelineModuleForTest

		class TrackingOrdersTimelineModuleForTest : TrackingOrdersTimelineModule
		{
			public TrackingOrdersTimelineModuleForTest(BusinessObjectFactory factory, ZPage page) : base(factory, page) { }

			public DataGridColumn[] GetNewGridColumnFieldsForTest()
			{
				return base.GetNewGridColumnFields();
			}

			internal FilterStripBusinessObject GetNewFilterStripBizOForTest()
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
		}

		string RemoveBrTags(string headerText)
		{
			return headerText.Replace("<br/>", " ");
		}

		#endregion
	}
}
