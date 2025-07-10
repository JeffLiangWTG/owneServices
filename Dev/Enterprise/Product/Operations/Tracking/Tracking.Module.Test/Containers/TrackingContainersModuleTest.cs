using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Web.UI.WebControls;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.Tracking.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Web.Business;
using Enterprise.ZArchitecture.Web.Business.Testing;
using Enterprise.ZArchitecture.Web.GUI.WebControls;
using Enterprise.ZArchitecture.Web.Modules;
using Enterprise.ZArchitecture.Web.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Tracking.Module.Testing
{
	[TestedType(typeof(TrackingContainersModule))]
	sealed class TrackingContainersModuleTest : ZFilterStripGridModuleTestCase
	{
		public void TestDoNotLoadBlobs()
		{
			var query = new ZQuery();
			FilterStripGridModule.LoadCollection(FilterStripGridModule.CreateNewFilterBusinessObject(), query);

			AssertEquals(0, query.LoadSmallBlobs);
		}

		protected override bool ExpectCachingOfCollectionKeys => false;

		protected override BusinessObject GetNewElement(Type elementType, bool isCancelled)
		{
			var result = base.GetNewElement(elementType, isCancelled);
			var container = result as TrackingContainer;
			if (container != null)
			{
				container.JC_ContainerNum = DateTime.Now.Ticks.ToString();
				container.JC_OH_ShippingLine = SiteUser.LoggedInOrganisation.PK;
			}
			return result;
		}

		#region TestCollectionLoadDBHitsWithDBOnlyQuery

		protected override List<BusinessObject> GetNewBusinessObjectsExpectedFromFilter()
		{
			List<BusinessObject> result = new List<BusinessObject>();
			for (int i = 1; i < 10; i++)
			{
				TrackingContainer testObject = Factory.NewWithValidTestData<TrackingContainer>();
				testObject.JC_ContainerNum = "Include" + i.ToString();
				testObject.JC_OH_ShippingLine = SiteUser.LoggedInOrganisation.PK;
				result.Add(testObject);
			}
			return result;
		}

		protected override List<BusinessObject> GetNewBusinessObjectsUnexpectedFromFilter()
		{
			List<BusinessObject> result = new List<BusinessObject>();
			for (int i = 1; i < 10; i++)
			{
				TrackingContainer testObject = Factory.NewWithValidTestData<TrackingContainer>();
				testObject.JC_ContainerNum = "Other" + i.ToString();
				testObject.JC_OH_ShippingLine = SiteUser.LoggedInOrganisation.PK;
				result.Add(testObject);
			}
			return result;
		}

		protected override void SetupDBOnlyQuery(ZDBOnlyQuery filter)
		{
			filter.AddToFilter(JobContainerSchema.JC_ContainerNum, SQLComparisonOperator.StartsWith, "Include");
		}

		#endregion

		protected override BusinessObject GetNewBizObjOfType(Type type)
		{
			if (type == typeof(TrackingMilestone))
			{
				return new TrackingMilestone(string.Empty, ZDateTimeOffset.Empty, ZDateTimeOffset.Empty, ZInt.Zero);
			}
			return base.GetNewBizObjOfType(type);
		}

		protected override Dictionary<string, string> GetExpectedAuditFilters()
		{
			Dictionary<string, string> result = base.GetExpectedAuditFilters();
			result.Add("Created On Web/Internal", "Created On Web/Internal");
			result.Add("Created Time", "Created Time");
			result.Add("Last Edit Time", "Last Edit Time");
			return result;
		}

		protected override WebModuleID TestID
		{
			get { return WebModuleIDs.TrackingContainers; }
		}

		protected override ColumnAndSortOrder[] ExpectedSortInfos => new[] { new ColumnAndSortOrder(JobContainerSchema.JC_ArrivalEstimatedDelivery.Name, ListSortDirection.Descending) };

		protected override ListSortDirection ExpectedDefaultSortOrder
		{
			get { return ListSortDirection.Descending; }
		}
		protected override ColumnDetailsForTest[] ExpectedColumnDetails
		{
			get
			{
				int i = 0;
				return new[]
					   {
							new ColumnDetailsForTest("Container #", i++, typeof(ZHyperLinkColumn)),
							new ColumnDetailsForTest("Shipment #", i++, typeof(ZTextEditColumn)),
							new ColumnDetailsForTest("Seal #", i++, typeof(ZTextEditColumn)),
							new ColumnDetailsForTest("Container Type", i++, typeof(ZTextEditColumn)),
							new ColumnDetailsForTest("Container Mode", i++, typeof(ZTextEditColumn)),
							new ColumnDetailsForTest("Packages", i++, typeof(ZCalcEditColumn)),
							new ColumnDetailsForTest("Departure", i++, typeof(ZDateTimeColumn)),
							new ColumnDetailsForTest("Arrival", i++, typeof(ZDateTimeColumn)),
							new ColumnDetailsForTest("Quarantine", i++, typeof(ZTextEditColumn)),
							new ColumnDetailsForTest("Available", i++, typeof(ZDateTimeColumn)),
							new ColumnDetailsForTest("Last Free Day", i++, typeof(ZDateTimeColumn)),
							new ColumnDetailsForTest("Detention Starts", i++, typeof(ZDateTimeColumn)),
							new ColumnDetailsForTest("Time Slot", i++, typeof(ZDateTimeColumn)),
							new ColumnDetailsForTest("Port Transport Ref", i++, typeof(ZTextEditColumn)),
							new ColumnDetailsForTest("Estimated Full Delivery", i++, typeof(ZDateTimeStatusColumn)),
							new ColumnDetailsForTest("Transport Booked", i++, typeof(ZDateTimeStatusColumn)),
							new ColumnDetailsForTest("Actual Delivery", i++, typeof(ZDateTimeStatusColumn)),
							new ColumnDetailsForTest("Deliver", i++, typeof(ZTextEditColumn)),
							new ColumnDetailsForTest("Empty Ready for Return", i++, typeof(ZDateTimeColumn)),
							new ColumnDetailsForTest("Empty Return By", i++, typeof(ZDateTimeColumn)),
							new ColumnDetailsForTest("Empty Returned On", i++, typeof(ZDateTimeStatusColumn)),
							new ColumnDetailsForTest("Vessel Name", i++, typeof(ZTextEditColumn)),
							new ColumnDetailsForTest("Voyage No", i++, typeof(ZTextEditColumn)),
							new ColumnDetailsForTest("Delivery Sequence", i++, typeof(ZCalcEditColumn)),
							new ColumnDetailsForTest("Last Milestone Desc.", i++, typeof(ZTextEditColumn)),
							new ColumnDetailsForTest("Last Milestone Date", i++, typeof(ZDateTimeColumn)),
							new ColumnDetailsForTest("Next Milestone Desc.", i++, typeof(ZTextEditColumn)),
							new ColumnDetailsForTest("Next Milestone Date", i++, typeof(ZDateTimeColumn)),
							new ColumnDetailsForTest("Status", i++, typeof(ZTextEditColumn)),
							new ColumnDetailsForTest("Storage Begins", i++, typeof(ZDateTimeColumn)),
							new ColumnDetailsForTest("Container Status", i++, typeof(ZTextEditColumn)),
							new ColumnDetailsForTest("Verified Weight",i++,typeof(ZTextEditColumn)),
							new ColumnDetailsForTest("Verified Date",i++, typeof(ZDateTimeColumn)),
							new ColumnDetailsForTest("Verified Method", i++, typeof(ZTextEditColumn)),
							new ColumnDetailsForTest("Verified Company", i++, typeof(ZTextEditColumn)),
							new ColumnDetailsForTest("Verified Contact", i++, typeof(ZTextEditColumn)),
							new ColumnDetailsForTest("Verified Phone", i++, typeof(ZTextEditColumn)),
							new ColumnDetailsForTest("Verified Email", i++, typeof(ZTextEditColumn))
				};
			}
		}

		protected override DataGridColumn[] ExpectedRequiredGridColumns
		{
			get
			{
				return new DataGridColumn[]
				{
						(FilterGridModule as TrackingContainersModule).AllColumns["Container #"]
				};
			}
		}

		protected override DataGridColumn[] ExpectedDefaultGridColumns
		{
			get
			{
				return new DataGridColumn[]
				{
						(FilterGridModule as TrackingContainersModule).AllColumns["Shipment #"],
						(FilterGridModule as TrackingContainersModule).AllColumns["Seal #"],
						(FilterGridModule as TrackingContainersModule).AllColumns["Container Type"],
						(FilterGridModule as TrackingContainersModule).AllColumns["Container Mode"],
						(FilterGridModule as TrackingContainersModule).AllColumns["Empty Returned On"]
				};
			}
		}

		protected override FilterLayoutCodePairRegistryItem ExpectedDefaultLayoutRegistryItem
		{
			get { return WebDataRegistry.Instance.DefaultFilterLayoutForwardingContainers; }
		}

		protected override void SetUp()
		{
			base.SetUp();
			LoginAsQuickViewUser();
		}

		void LoginAsQuickViewUser()
		{
			OrgContact contact = Factory.Load<OrgContact>(new ZGuid("f960e868-fef4-4cab-a1f5-3ace433c04e9"));
			AssertNotNull("Fixed Contact", contact);
			AssertEquals("Contact Name", "TONY MORAN - SALES", contact.OC_ContactName);
			contact.OC_WebAccessEnabled = true;
			contact.SetHashedPassword("test");

			Factory.Save();

			TrackingSiteUser user = new TrackingSiteUser();
			user.Login(contact.ParentOrg.OH_Code, contact.OC_Email, contact.PasswordForTesting);
			((DummyHttpApplication)WebEnv.AppInstance).SetSiteUser(user);
			AssertNotNull("No tracking site user", LoggedSiteUser);
			AssertEquals("IsLogged", true, LoggedSiteUser.IsLoggedIn);
			AssertEquals("Not IsShipmentQuickViewUser", false, LoggedSiteUser.IsShipmentQuickViewUser);
		}

		TrackingSiteUser LoggedSiteUser
		{
			get
			{
				return WebEnv.AppInstance.SiteUser as TrackingSiteUser;
			}
		}
	}

	class TrackingContainersModuleWithCachingTest : ZFilterGridModuleWithCachingTest
	{
		protected override void SetupData()
		{
			var user = (OrgContactWebUser)TestPage.SiteUser;
			var container1 = Factory.NewWithValidTestData<TrackingContainer>();
			container1.JC_OH_ShippingLine = user.LoggedInOrganisation.PK;

			var container2 = Factory.NewWithValidTestData<TrackingContainer>();
			container2.JC_OH_ShippingLine = user.LoggedInOrganisation.PK;

			var container3 = Factory.NewWithValidTestData<TrackingContainer>();
			container3.JC_OH_ShippingLine = user.LoggedInOrganisation.PK;

			Factory.Save();
		}

		protected override ZFilterGridModule GetNewFilterGridModule() => new TrackingContainersModule(Factory, TestPage);
	}
}
