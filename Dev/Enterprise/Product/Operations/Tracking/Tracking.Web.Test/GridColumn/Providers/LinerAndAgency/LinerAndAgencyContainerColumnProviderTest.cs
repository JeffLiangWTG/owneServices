using System;
using System.Web.UI.WebControls;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Tracking;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Web;
using Enterprise.Tracking.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Web.Business;
using Enterprise.ZArchitecture.Web.Business.Testing;
using Enterprise.ZArchitecture.Web.GUI;
using Enterprise.ZArchitecture.Web.GUI.Testing;
using Enterprise.ZArchitecture.Web.GUI.WebControls;
using NUnit.Framework;

namespace Enterprise.Tracking.Web.Testing
{
	[TestedType(typeof(LinerAndAgencyContainerColumnProvider))]
	[HttpContextEnabledTest]
	sealed class LinerAndAgencyContainerColumnProviderTest : GridColumnProviderTest
	{
		protected override void BeforeLayoutsWithFewDynamicColumns()
		{
			base.BeforeLayoutsWithFewDynamicColumns();
			WebDataRegistry.Instance.MilestoneVisibility.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, MilestoneVisibilityList.Codes.LastCompletedMilestoneOnly);
		}

		protected override void BeforeLayoutsWithAllDynamicColumns()
		{
			base.BeforeLayoutsWithAllDynamicColumns();
			WebDataRegistry.Instance.MilestoneVisibility.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, MilestoneVisibilityList.Codes.All);
		}

		protected override DataGridColumn[] GetColumnsForLayoutFixAllDynamicColumns()
		{
			return new DataGridColumn[]
			{
				TestProvider[WebTracker.Grids.Milestones.LastMilestoneDate],
				TestProvider[WebTracker.Grids.Milestones.NextMilestoneDate],
				TestProvider[WebTracker.Grids.LinerAndAgencyContainers.ContainerNumber],
				TestProvider[WebTracker.Grids.Milestones.LastMilestoneDescription],
				TestProvider[WebTracker.Grids.LinerAndAgencyContainers.Vessel],
				TestProvider[WebTracker.Grids.Milestones.NextMilestoneDescription]
			};
		}

		protected override DataGridColumn[] GetColumnsForLayoutFixFewDynamicColumns()
		{
			return new DataGridColumn[]
			{
				TestProvider[WebTracker.Grids.LinerAndAgencyContainers.ShipmentNumber],
				TestProvider[WebTracker.Grids.Milestones.LastMilestoneDate],
				TestProvider[WebTracker.Grids.LinerAndAgencyContainers.RequiredDelivery],
				TestProvider[WebTracker.Grids.Milestones.LastMilestoneDescription],
				TestProvider[WebTracker.Grids.LinerAndAgencyContainers.Packs]
			};
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0002:Simplify Member Access", Justification = "Simplified access could change context here")]
		protected override void SetupColumnsCore()
		{
			base.SetupColumnsCore();
			AddRequiredColumn(new ZHyperLinkColumn("Container #", LinerAndAgencyContainer.Schema.ContainerNumber)
			{
				DataNavigateUrlFormatString = TrackingConstants.RelativePath.ContainerDetailsPage + "?Ref={0}", // Part of URL string
				DataNavigateUrlFields = new string[1] { "PK" },
				ColumnKey = WebTracker.Grids.LinerAndAgencyContainers.ContainerNumber
			});

			if (LoggedSiteUser != null && !LoggedSiteUser.IsShipmentQuickViewUser)
			{
				AddDefaultsColumn(new ZTextEditColumn("Shipment #", LinerAndAgencyContainer.Schema.ShipmentNumbers) { ColumnKey = WebTracker.Grids.LinerAndAgencyContainers.ShipmentNumber });
			}

			AddDefaultsColumn(new ZTextEditColumn("Seal #", JobContainerSchema.JC_SealNum.Name) { ColumnKey = WebTracker.Grids.LinerAndAgencyContainers.SealNumber });
			AddDefaultsColumn(new ZTextEditColumn("Container Type", LinerAndAgencyContainer.Schema.TypeDescription) { ColumnKey = WebTracker.Grids.LinerAndAgencyContainers.TypeDescription });
			AddDefaultsColumn(new ZTextEditColumn("Container Mode", LinerAndAgencyContainer.Schema.Mode) { ColumnKey = WebTracker.Grids.LinerAndAgencyContainers.Mode });
			AddColumn(new ZCalcEditColumn("Packages", LinerAndAgencyContainer.Schema.Packs) { ColumnKey = WebTracker.Grids.LinerAndAgencyContainers.Packs });
			AddColumn(new ZDateTimeStatusColumn("Estimated Full Delivery", LinerAndAgencyContainer.Schema.RequiredDelivery, LinerAndAgencyContainer.Schema.RequiredDeliveryStatus, ZDateTimePickerFormat.Long) { ColumnKey = WebTracker.Grids.LinerAndAgencyContainers.RequiredDelivery });
			AddColumn(new ZDateTimeStatusColumn("Actual Delivery", LinerAndAgencyContainer.Schema.ActualDelivery, LinerAndAgencyContainer.Schema.ActualDeliveryStatus, ZDateTimePickerFormat.Long) { ColumnKey = WebTracker.Grids.LinerAndAgencyContainers.ActualDelivery });
			AddColumn(new ZTextEditColumn("Deliver", LinerAndAgencyContainer.Schema.ConsigneesExtended) { ColumnKey = WebTracker.Grids.LinerAndAgencyContainers.Deliver });
			AddColumn(new ZDateTimeColumn("Empty Ready for Return", LinerAndAgencyContainer.Schema.EmptyReady, ZDateTimePickerFormat.Long) { ColumnKey = WebTracker.Grids.LinerAndAgencyContainers.EmptyReady });
			AddColumn(new ZDateTimeColumn("Empty Return By", LinerAndAgencyContainer.Schema.EmptyReturnRequired, ZDateTimePickerFormat.Long) { ColumnKey = WebTracker.Grids.LinerAndAgencyContainers.EmptyReturnRequired });
			AddColumn(new ZDateTimeStatusColumn("Empty Returned On", LinerAndAgencyContainer.Schema.ActualDehire, LinerAndAgencyContainer.Schema.ActualDehireStatus, ZDateTimePickerFormat.Long) { ColumnKey = WebTracker.Grids.LinerAndAgencyContainers.ActualDehire });
			AddColumn(new ZTextEditColumn("Vessel Name", LinerAndAgencyContainer.Schema.VesselName) { ColumnKey = WebTracker.Grids.LinerAndAgencyContainers.Vessel });
			AddColumn(new ZTextEditColumn("Voyage No", LinerAndAgencyContainer.Schema.Voyage) { ColumnKey = WebTracker.Grids.LinerAndAgencyContainers.Voyage });

			foreach (ZTemplateColumn column in ConfigurationHelper.GetMilestonesColumns("Milestones")) // Data column name
			{
				AddColumn(column);
			}

			AddColumn(new ZTextEditColumn("Container Status", LinerAndAgencyContainer.Schema.ContainerStatus) { ColumnKey = WebTracker.Grids.LinerAndAgencyContainers.ContainerStatus });
			AddColumn(new ZTextEditColumn("Verified Weight", LinerAndAgencyContainer.Schema.WeightWithUnits) { ColumnKey = WebTracker.Grids.LinerAndAgencyContainers.GrossWeight });
			AddColumn(new ZDateTimeColumn("Verified Date", LinerAndAgencyContainer.Schema.JC_GrossWeightVerificationDateTime, ZDateTimePickerFormat.Long) { ColumnKey = WebTracker.Grids.LinerAndAgencyContainers.VerifiedDate });
			AddColumn(new ZTextEditColumn("Verified Method", LinerAndAgencyContainer.Schema.VerifiedMethod) { ColumnKey = WebTracker.Grids.LinerAndAgencyContainers.VerifiedMethod });
			AddColumn(new ZTextEditColumn("Verified Company", LinerAndAgencyContainer.Schema.VerifiedByCompany) { ColumnKey = WebTracker.Grids.LinerAndAgencyContainers.VerifiedCompany });
			AddColumn(new ZTextEditColumn("Verified Contact", LinerAndAgencyContainer.Schema.VerifiedByPerson) { ColumnKey = WebTracker.Grids.LinerAndAgencyContainers.VerifiedContact });
			AddColumn(new ZTextEditColumn("Verified Phone", LinerAndAgencyContainer.Schema.VerifiedByPhone) { ColumnKey = WebTracker.Grids.LinerAndAgencyContainers.VerifiedPhone });
			AddColumn(new ZTextEditColumn("Verified Email", LinerAndAgencyContainer.Schema.VerifiedByEmail) { ColumnKey = WebTracker.Grids.LinerAndAgencyContainers.VerifiedEmail });
		}

		protected override GridColumnProvider GetNewTestProvider()
		{
			return new LinerAndAgencyContainerColumnProvider();
		}

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

		TrackingSiteUser LoggedSiteUser
		{
			get
			{
				return WebEnv.AppInstance.SiteUser as TrackingSiteUser;
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			LoginAsQuickViewUser();
		}
	}
}
