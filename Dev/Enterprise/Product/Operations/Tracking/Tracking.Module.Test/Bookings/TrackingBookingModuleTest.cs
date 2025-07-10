using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Web.UI.WebControls;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Web;
using Enterprise.Tracking.Business;
using Enterprise.Tracking.Business.Testing;
using Enterprise.Tracking.Web.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Web.Business.Testing;
using Enterprise.ZArchitecture.Web.GUI;
using Enterprise.ZArchitecture.Web.GUI.WebControls;
using Enterprise.ZArchitecture.Web.Modules;
using Enterprise.ZArchitecture.Web.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Tracking.Module.Testing
{
	[TestedType(typeof(TrackingBookingsModule))]
	sealed class TrackingBookingModuleTest : ZFilterStripGridModuleTestCase
	{
		protected override void SetupForActiveStatusFilterTest()
		{
			base.SetupForActiveStatusFilterTest();
			var testHelper = new TestHelper(Factory);
			testHelper.TestSiteUser.Login(testHelper.TestOrg.OH_Code, testHelper.TestContact.OC_Email, testHelper.TestContact.PasswordForTesting);
		}

		protected override System.Collections.IList GetNewFilterGridCollection() => new List<TrackingBooking>();

		protected override BusinessObject GetNewElement(Type elementType, bool isCancelled)
		{
			var result = new TrackingBooking(Factory, SiteUser as TrackingSiteUser);
			result.IsCancelled = isCancelled;

			return result;
		}

		protected override ZWebTestHelper GetNewHelper() => new TestHelper(Factory);

		#region TestCollectionLoadDBHitsWithDBOnlyQuery

		protected override bool GetShoudTestLoadDBHitsWithDBOnlyQuery(IBusinessObjectCollection collection) => false;

		#endregion

		public void TestDefaultColumnsWhenMilestonesDisabled()
		{
			var oldValue = WebDataRegistry.Instance.MilestoneVisibility.Value;
			WebDataRegistry.Instance.MilestoneVisibility.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, MilestoneVisibilityList.Codes.All);
			var testModule = new TrackingBookingsModuleForTest(null);
			Assert(testModule.AllColumns.ContainsKey("Last Milestone Desc."));
			var defaultCols = testModule.ForTest_GetDefaultGridColumnFields();
			var hasMilestoneColumn = false;
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
			testModule = new TrackingBookingsModuleForTest(null);
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

		public void TestCreateAllocateColumn()
		{
			var page = new DummyPage();
			using (TrackingBookingsModuleForTest module = new TrackingBookingsModuleForTest(page))
			{
				var user = page.SiteUser as TrackingSiteUser;
				AssertNotNull(user);

				var org = Factory.NewWithValidTestData<OrgHeader>();
				org.OH_Code = "XXXYYYZZZ";
				var contact = org.Contacts.AddNew();
				contact.OC_Email = "user@user.com";
				contact.SetHashedPassword("password");
				contact.OC_WebAccessEnabled = true;

				var contactSecurity = new List<OrgSecurityContacts>();
				var orgRight = org.SecurityRights.AddNew();
				orgRight.OX_Granted = true;
				orgRight.OX_SecurityItemName = WebSecurityRightsList.WebBookingsAddEdit.Code;
				var userRight = contact.SecurityRightsForBindingOnly.AddNew();
				userRight.OZ_OX = orgRight.PK;
				userRight.OZ_Granted = true;
				contactSecurity.Add(userRight);

				Factory.Save();

				user.Login(org.OH_Code, "user@user.com", "password");

				AssertEquals(35, module.GetNewGridColumnFieldsForTest().Length);

				user.LoggedInUser.SecurityRightsForBindingOnly.RemoveAll();

				AssertEquals(35, module.GetNewGridColumnFieldsForTest().Length);
			}
		}

		#region Overrides

		protected override Dictionary<string, string> GetExpectedAuditFilters()
		{
			var result = base.GetExpectedAuditFilters();
			result.Add("Created On Web/Internal", "Created On Web/Internal");
			result.Add("Created Time", "Created Time");
			result.Add("Last Edit Time", "Last Edit Time");

			return result;
		}

		protected override void FillCollectionWithAtLeastOneElement()
		{
			new TrackingBooking(Factory, null);
			Factory.Save();
			((ViewTrackingBookingCollection)FilterGridModule.GridCollection).Load();
		}

		protected override WebModuleID TestID => WebModuleIDs.TrackingBookings;

		protected override FilterLayoutCodePairRegistryItem ExpectedDefaultLayoutRegistryItem => WebDataRegistry.Instance.DefaultFilterLayoutForwardingBookings;

		protected override string ExpectedDefaultLayoutName => DefaultLayoutNameValue;

		protected override ZWebModule GetNewZWebModule() => new TrackingBookingsModuleForTest(TestPage);

		protected override BusinessObject GetNewBizObjOfType(Type type)
		{
			if (type == typeof(TrackingBooking))
			{
				return new TrackingBooking(Factory, null);
			}
			if (type == typeof(TrackingMilestone))
			{
				return new TrackingMilestone(string.Empty, ZDateTimeOffset.Empty, ZDateTimeOffset.Empty, ZInt.Zero);
			}

			return base.GetNewBizObjOfType(type);
		}

		#endregion

		#region Columns and Sorting

		protected override DataGridColumn[] ExpectedRequiredGridColumns
		{
			get
			{
				var module = FilterGridModule as TrackingBookingsModuleForTest;

				return new[] { module.AllColumns["Booking#"] };
			}
		}

		protected override DataGridColumn[] ExpectedDefaultGridColumns
		{
			get
			{
				var result = new List<DataGridColumn>();

				using (var module = FilterGridModule as TrackingBookingsModuleForTest)
				{
					result.Add(module.AllColumns["Description"]);
					result.Add(module.AllColumns["Shipper's Ref#"]);
					result.Add(module.AllColumns["Origin"]);
					result.Add(module.AllColumns["Destination"]);
					result.Add(module.AllColumns["Packs"]);
					result.Add(module.AllColumns["Weight"]);
					result.Add(module.AllColumns["Volume"]);
					result.Add(module.AllColumns["Goods Value"]);
					result.Add(module.AllColumns["Last Milestone Desc."]);
				}

				return result.ToArray();
			}
		}

		protected override ColumnDetailsForTest[] ExpectedColumnDetails
		{
			get
			{
				var result = new List<ColumnDetailsForTest>();
				var i = 0;

				result.Add(new ColumnDetailsForTest("Booking#", i++, typeof(ZHyperLinkColumn)));
				result.Add(new ColumnDetailsForTest("Description", i++, typeof(ZTextEditColumn)));
				result.Add(new ColumnDetailsForTest("Shipper's Ref#", i++, typeof(ZTextEditColumn)));
				result.Add(new ColumnDetailsForTest("Origin", i++, typeof(ZTextEditColumn)));
				result.Add(new ColumnDetailsForTest("Destination", i++, typeof(ZTextEditColumn)));
				result.Add(new ColumnDetailsForTest("Packs", i++, typeof(ZCalcEditColumn)));
				result.Add(new ColumnDetailsForTest("Weight", i++, typeof(ZTextEditColumn)));
				result.Add(new ColumnDetailsForTest("Volume", i++, typeof(ZTextEditColumn)));
				result.Add(new ColumnDetailsForTest("Goods Value", i++, typeof(ZGroupColumn)));
				result.Add(new ColumnDetailsForTest("Canceled?", i++, typeof(ZTextEditColumn)));
				result.Add(new ColumnDetailsForTest("Estimated Pickup", i++, typeof(ZDateTimeColumn)));
				result.Add(new ColumnDetailsForTest("Pickup Required By", i++, typeof(ZDateTimeColumn)));
				result.Add(new ColumnDetailsForTest("Estimated Delivery", i++, typeof(ZDateTimeColumn)));
				result.Add(new ColumnDetailsForTest("Delivery Required By", i++, typeof(ZDateTimeColumn)));
				result.Add(new ColumnDetailsForTest("Delivery Date", i++, typeof(ZDateTimeColumn)));
				result.Add(new ColumnDetailsForTest("Service Level", i++, typeof(ZCodeFindBoxColumn)));
				result.Add(new ColumnDetailsForTest("Order Ref#", i++, typeof(ZTextEditColumn)));
				result.Add(new ColumnDetailsForTest("Last Milestone Desc.", i++, typeof(ZTextEditColumn)));
				result.Add(new ColumnDetailsForTest("Last Milestone Date", i++, typeof(ZDateTimeColumn)));
				result.Add(new ColumnDetailsForTest("Next Milestone Desc.", i++, typeof(ZTextEditColumn)));
				result.Add(new ColumnDetailsForTest("Next Milestone Date", i++, typeof(ZDateTimeColumn)));
				result.Add(new ColumnDetailsForTest("Vessel", i++, typeof(ZTextEditColumn)));
				result.Add(new ColumnDetailsForTest("Voyage/Flight", i++, typeof(ZTextEditColumn)));
				result.Add(new ColumnDetailsForTest("MAWB", i++, typeof(ZTextEditColumn)));
				result.Add(new ColumnDetailsForTest("Consignee", i++, typeof(ZTextEditColumn)));
				result.Add(new ColumnDetailsForTest(FreightDataRegistry.Instance.ConsignorShipperTerminology.Value, i++, typeof(ZTextEditColumn)));
				result.Add(new ColumnDetailsForTest("CFS Cut Off", i++, typeof(ZDateTimeColumn)));
				result.Add(new ColumnDetailsForTest("CFS Ref#", i++, typeof(ZTextEditColumn)));
				result.Add(new ColumnDetailsForTest("Additional Terms", i++, typeof(ZTextEditColumn)));
				result.Add(new ColumnDetailsForTest("Payment Term", i++, typeof(ZDropEditColumn)));
				result.Add(new ColumnDetailsForTest("Charges Apply", i++, typeof(ZDropEditColumn)));
				result.Add(new ColumnDetailsForTest("Release Type", i++, typeof(ZDropEditColumn)));
				result.Add(new ColumnDetailsForTest("On Board", i++, typeof(ZDropEditColumn)));
				result.Add(new ColumnDetailsForTest("Pickup Agent", i++, typeof(ZTextEditColumn)));
				result.Add(new ColumnDetailsForTest("Delivery Agent", i++, typeof(ZTextEditColumn)));

				return result.ToArray();
			}
		}

		protected override ColumnAndSortOrder[] ExpectedSortInfos => new[] { new ColumnAndSortOrder(ViewQuotedBookingSchema.VB_JS.Name, ListSortDirection.Ascending) };

		#endregion

		#region TrackingBookingsModuleForTest

		class TrackingBookingsModuleForTest : TrackingBookingsModule
		{
			public TrackingBookingsModuleForTest(ZPage page)
				: base(new BusinessObjectFactory(), page)
			{
			}

			public DataGridColumn[] GetNewGridColumnFieldsForTest() => GetNewGridColumnFields();

			public DataGridColumn[] ForTest_GetDefaultGridColumnFields() => GetDefaultGridColumnFields();
		}

		#endregion

		#region DummyPage

		class DummyPage : ZPage
		{
			protected override ZGlobal GetNewTestGlobal() => new TestGlobal();
		}

		#endregion
	}
}
