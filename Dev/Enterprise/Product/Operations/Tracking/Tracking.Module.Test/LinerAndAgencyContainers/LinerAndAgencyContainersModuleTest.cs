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
	[TestedType(typeof(LinerAndAgencyContainersModule))]
	sealed class LinerAndAgencyContainersModuleTest : ZFilterStripGridModuleTestCase
	{
		protected override BusinessObject GetNewElement(Type elementType, bool isCancelled)
		{
			var result = base.GetNewElement(elementType, isCancelled);
			var container = result as LinerAndAgencyContainer;
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
			var result = new List<BusinessObject>();
			for (var i = 1; i < 10; i++)
			{
				var testObject = Factory.NewWithValidTestData<LinerAndAgencyContainer>();
				testObject.JC_ContainerNum = "Include" + i.ToString();
				testObject.JC_OH_ShippingLine = SiteUser.LoggedInOrganisation.PK;
				result.Add(testObject);
			}

			return result;
		}

		protected override List<BusinessObject> GetNewBusinessObjectsUnexpectedFromFilter()
		{
			var result = new List<BusinessObject>();
			for (var i = 1; i < 10; i++)
			{
				var testObject = Factory.NewWithValidTestData<LinerAndAgencyContainer>();
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

		protected override BusinessObject CreateNewElementForExcelExport()
		{
			var container = (LinerAndAgencyContainer)base.CreateNewElementForExcelExport();
			var shipment = container.Shipments.AddNew();
			shipment.JS_IsShipping = true;
			container.JC_OH_ShippingLine = LoggedSiteUser.LoggedInOrganisation.PK;
			container.JC_JS_FCLBookingOnlyLink = shipment.PK;

			return container;
		}

		protected override Dictionary<string, string> GetExpectedAuditFilters()
		{
			var result = base.GetExpectedAuditFilters();
			result.Add("Created On Web/Internal", "Created On Web/Internal");
			result.Add("Created Time", "Created Time");
			result.Add("Last Edit Time", "Last Edit Time");

			return result;
		}

		protected override WebModuleID TestID => WebModuleIDs.LinerAndAgencyContainers;

		protected override ColumnAndSortOrder[] ExpectedSortInfos => new[] { new ColumnAndSortOrder(JobContainerSchema.JC_ArrivalEstimatedDelivery.Name, ListSortDirection.Descending) };

		protected override ListSortDirection ExpectedDefaultSortOrder => ListSortDirection.Descending;

		protected override ColumnDetailsForTest[] ExpectedColumnDetails
		{
			get
			{
				var i = 0;

				return new[]
					   {
							new ColumnDetailsForTest("Container #", i++, typeof(ZHyperLinkColumn)),
							new ColumnDetailsForTest("Shipment #", i++, typeof(ZTextEditColumn)),
							new ColumnDetailsForTest("Seal #", i++, typeof(ZTextEditColumn)),
							new ColumnDetailsForTest("Container Type", i++, typeof(ZTextEditColumn)),
							new ColumnDetailsForTest("Container Mode", i++, typeof(ZTextEditColumn)),
							new ColumnDetailsForTest("Packages", i++, typeof(ZCalcEditColumn)),
							new ColumnDetailsForTest("Estimated Full Delivery", i++, typeof(ZDateTimeColumn)),
							new ColumnDetailsForTest("Actual Delivery", i++, typeof(ZDateTimeColumn)),
							new ColumnDetailsForTest("Deliver", i++, typeof(ZTextEditColumn)),
							new ColumnDetailsForTest("Empty Ready for Return", i++, typeof(ZDateTimeColumn)),
							new ColumnDetailsForTest("Empty Return By", i++, typeof(ZDateTimeColumn)),
							new ColumnDetailsForTest("Empty Returned On", i++, typeof(ZDateTimeStatusColumn)),
							new ColumnDetailsForTest("Vessel Name", i++, typeof(ZTextEditColumn)),
							new ColumnDetailsForTest("Voyage No", i++, typeof(ZTextEditColumn)),
							new ColumnDetailsForTest("Last Milestone Desc.", i++, typeof(ZTextEditColumn)),
							new ColumnDetailsForTest("Last Milestone Date", i++, typeof(ZDateTimeColumn)),
							new ColumnDetailsForTest("Next Milestone Desc.", i++, typeof(ZTextEditColumn)),
							new ColumnDetailsForTest("Next Milestone Date", i++, typeof(ZDateTimeColumn)),
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

		protected override DataGridColumn[] ExpectedRequiredGridColumns => new[] { (FilterGridModule as LinerAndAgencyContainersModule).AllColumns["Container #"] };

		protected override DataGridColumn[] ExpectedDefaultGridColumns => new[]
		{
			(FilterGridModule as LinerAndAgencyContainersModule).AllColumns["Shipment #"],
			(FilterGridModule as LinerAndAgencyContainersModule).AllColumns["Seal #"],
			(FilterGridModule as LinerAndAgencyContainersModule).AllColumns["Container Type"],
			(FilterGridModule as LinerAndAgencyContainersModule).AllColumns["Container Mode"],
		};

		protected override FilterLayoutCodePairRegistryItem ExpectedDefaultLayoutRegistryItem => WebDataRegistry.Instance.DefaultFilterLayoutLinerAndAgencyContainers;

		protected override void SetUp()
		{
			base.SetUp();
			LoginAsQuickViewUser();
		}

		protected override bool AllowActiveStatusFilterTest() => false;

		void LoginAsQuickViewUser()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "AAA";
			org.OH_IsActive = true;

			Factory.Save();

			var contact = Factory.NewWithValidTestData<OrgContact>();
			contact.OC_ContactName = "TONY MORAN - SALES";
			contact.OC_OH = org.PK;

			var user = new TrackingSiteUser();
			contact.OC_Email = "test@cargowise.com";
			contact.SetHashedPassword("test");
			contact.OC_WebAccessEnabled = true;
			contact.Factory.Save();

			AssertNotNull("Fixed Contact", contact);
			AssertEquals("Contact Name", "TONY MORAN - SALES", contact.OC_ContactName);

			user.Login(org.OH_Code, contact.OC_Email, contact.PasswordForTesting);
			((DummyHttpApplication)WebEnv.AppInstance).SetSiteUser(user);
			AssertNotNull("No tracking site user", LoggedSiteUser);
			AssertEquals("IsLogged", true, LoggedSiteUser.IsLoggedIn);
			AssertEquals("Not IsShipmentQuickViewUser", false, LoggedSiteUser.IsShipmentQuickViewUser);
		}

		TrackingSiteUser LoggedSiteUser => WebEnv.AppInstance.SiteUser as TrackingSiteUser;
	}
}
