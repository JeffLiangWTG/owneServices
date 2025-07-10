using System;
using System.Linq;
using System.Web.UI.WebControls;
using CargoWise.ComponentModel;
using CargoWise.Types;
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
	[TestedType(typeof(TrackingContainerColumnProvider))]
	[HttpContextEnabledTest]
	class TrackingContainerColumnProviderTest : GridColumnProviderTest
	{
		public void TestDoNotLoadBlobColumns()
		{
			var blobNames = JobContainerSchema.All.Where(x => x.IsLargeBinaryOrText).Select(x => x.Name);

			Assert("Blob columns will be lazy loaded so shouldn't have a column", !TestProvider.AllColumns.OfType<IBindTo>().Any(x => blobNames.Contains(x.BindTo)));
		}

		public override void TestFixOldLayout()
		{
			string cachedRegistryValue = WebDataRegistry.Instance.MilestoneVisibility.Value;
			CaptionAndHint cachedRegistryHint = (CaptionAndHint)FreightDataRegistry.Instance.ShipmentCustomText1.Value;
			bool cachedStatusRegistryValue = WebDataRegistry.Instance.ShowContainerStatusFromShipment.Value;
			base.TestFixOldLayout();
			WebDataRegistry.Instance.MilestoneVisibility.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, cachedRegistryValue);
			FreightDataRegistry.Instance.ShipmentCustomText1.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, cachedRegistryHint);
			WebDataRegistry.Instance.ShowContainerStatusFromShipment.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, cachedStatusRegistryValue);
		}

		protected override void BeforeLayoutsWithNoDynamicColumns()
		{
			base.BeforeLayoutsWithNoDynamicColumns();
			FreightDataRegistry.Instance.ShipmentCustomText1.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new CaptionAndHint("Test Caption", "Test Hint"));
			WebDataRegistry.Instance.ShowContainerStatusFromShipment.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
		}

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

		protected override DataGridColumn[] GetColumnsForLayoutFixNoDynamicColumns()
		{
			return new DataGridColumn[]
			{
				TestProvider[WebTracker.Grids.TrackingContainers.ShipmentStatuses],
				TestProvider[WebTracker.Grids.TrackingContainers.TimeSlot],
				TestProvider[WebTracker.Grids.TrackingContainers.DeliverySequence],
				TestProvider[WebTracker.Grids.TrackingContainers.LastFreeDay]
			};
		}

		protected override DataGridColumn[] GetColumnsForLayoutFixAllDynamicColumns()
		{
			return new DataGridColumn[]
			{
				TestProvider[WebTracker.Grids.TrackingContainers.DetentionStarts],
				TestProvider[WebTracker.Grids.Milestones.LastMilestoneDate],
				TestProvider[WebTracker.Grids.TrackingContainers.ConfirmedDelivery],
				TestProvider[WebTracker.Grids.Milestones.NextMilestoneDate],
				TestProvider[WebTracker.Grids.TrackingContainers.ContainerNumber],
				TestProvider[WebTracker.Grids.Milestones.LastMilestoneDescription],
				TestProvider[WebTracker.Grids.TrackingContainers.Vessel],
				TestProvider[WebTracker.Grids.Milestones.NextMilestoneDescription]
			};
		}

		protected override DataGridColumn[] GetColumnsForLayoutFixFewDynamicColumns()
		{
			return new DataGridColumn[]
			{
				TestProvider[WebTracker.Grids.TrackingContainers.TimeSlot],
				TestProvider[WebTracker.Grids.TrackingContainers.ShipmentNumber],
				TestProvider[WebTracker.Grids.Milestones.LastMilestoneDate],
				TestProvider[WebTracker.Grids.TrackingContainers.RequiredDelivery],
				TestProvider[WebTracker.Grids.Milestones.LastMilestoneDescription],
				TestProvider[WebTracker.Grids.TrackingContainers.Packs]
			};
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0002:Simplify Member Access", Justification = "Simplified access could change context here")]
		protected override void SetupColumnsCore()
		{
			base.SetupColumnsCore();
			AddRequiredColumn(new ZHyperLinkColumn("Container #", TrackingContainer.Schema.ContainerNumber)
			{
				DataNavigateUrlFormatString = TrackingConstants.RelativePath.ContainerDetailsPage + "?Ref={0}", // Part of URL string
				DataNavigateUrlFields = new string[1] { "PK" },
				ColumnKey = WebTracker.Grids.TrackingContainers.ContainerNumber
			});

			if (LoggedSiteUser != null && !LoggedSiteUser.IsShipmentQuickViewUser)
			{
				AddDefaultsColumn(new ZTextEditColumn("Shipment #", TrackingContainer.Schema.ShipmentNumbers) { ColumnKey = WebTracker.Grids.TrackingContainers.ShipmentNumber });
			}

			AddDefaultsColumn(new ZTextEditColumn("Seal #", JobContainerSchema.JC_SealNum.Name) { ColumnKey = WebTracker.Grids.TrackingContainers.SealNumber });
			AddDefaultsColumn(new ZTextEditColumn("Container Type", TrackingContainer.Schema.Type) { ColumnKey = WebTracker.Grids.TrackingContainers.Type });
			AddDefaultsColumn(new ZTextEditColumn("Container Mode", TrackingContainer.Schema.Mode) { ColumnKey = WebTracker.Grids.TrackingContainers.Mode });
			AddColumn(new ZCalcEditColumn("Packages", TrackingContainer.Schema.Packs) { ColumnKey = WebTracker.Grids.TrackingContainers.Packs });
			AddColumn(new ZDateTimeColumn("Departure", TrackingContainer.Schema.Departure, ZDateTimePickerFormat.Long) { ColumnKey = WebTracker.Grids.TrackingContainers.Departure });
			AddColumn(new ZDateTimeColumn("Arrival", TrackingContainer.Schema.Arrival, ZDateTimePickerFormat.Long) { ColumnKey = WebTracker.Grids.TrackingContainers.Arrival });
			AddColumn(new ZTextEditColumn("Quarantine", TrackingContainer.Schema.QuarantineCode) { ColumnKey = WebTracker.Grids.TrackingContainers.Quarantine });
			AddColumn(new ZDateTimeColumn("Available", TrackingContainer.Schema.Available, ZDateTimePickerFormat.Long) { ColumnKey = WebTracker.Grids.TrackingContainers.Available });
			AddColumn(new ZDateTimeColumn("Last Free Day", TrackingContainer.Schema.JC_LastFreeDay, ZDateTimePickerFormat.Long) { ColumnKey = WebTracker.Grids.TrackingContainers.LastFreeDay });
			AddColumn(new ZDateTimeColumn("Detention Starts", TrackingContainer.Schema.EmptyReturnRequired, ZDateTimePickerFormat.Long) { ColumnKey = WebTracker.Grids.TrackingContainers.DetentionStarts });
			AddColumn(new ZDateTimeColumn("Time Slot", TrackingContainer.Schema.SlotDate, ZDateTimePickerFormat.Long) { ColumnKey = WebTracker.Grids.TrackingContainers.TimeSlot });
			AddColumn(new ZTextEditColumn("Port Transport Ref", TrackingContainer.Schema.JC_DepartureCartageRef) { ColumnKey = WebTracker.Grids.TrackingContainers.LocalTransportReference });
			AddColumn(new ZDateTimeStatusColumn("Estimated Full Delivery", TrackingContainer.Schema.RequiredDelivery, TrackingContainer.Schema.RequiredDeliveryStatus, ZDateTimePickerFormat.Long) { ColumnKey = WebTracker.Grids.TrackingContainers.RequiredDelivery });
			AddColumn(new ZDateTimeStatusColumn("Transport Booked", TrackingContainer.Schema.ConfirmedDelivery, TrackingContainer.Schema.ConfirmedDeliveryStatus, ZDateTimePickerFormat.Long) { ColumnKey = WebTracker.Grids.TrackingContainers.ConfirmedDelivery });
			AddColumn(new ZDateTimeStatusColumn("Actual Delivery", TrackingContainer.Schema.ActualDelivery, TrackingContainer.Schema.ActualDeliveryStatus, ZDateTimePickerFormat.Long) { ColumnKey = WebTracker.Grids.TrackingContainers.ActualDelivery });
			AddColumn(new ZTextEditColumn("Deliver", TrackingContainer.Schema.Consignees) { ColumnKey = WebTracker.Grids.TrackingContainers.Deliver });
			AddColumn(new ZDateTimeColumn("Empty Ready for Return", TrackingContainer.Schema.EmptyReady, ZDateTimePickerFormat.Long) { ColumnKey = WebTracker.Grids.TrackingContainers.EmptyReady });
			AddColumn(new ZDateTimeColumn("Empty Return By", TrackingContainer.Schema.EmptyPickup, ZDateTimePickerFormat.Long) { ColumnKey = WebTracker.Grids.TrackingContainers.EmptyPickup });
			AddDefaultsColumn(new ZDateTimeStatusColumn("Empty Returned On", TrackingContainer.Schema.ActualDehire, TrackingContainer.Schema.ActualDehireStatus, ZDateTimePickerFormat.Long) { ColumnKey = WebTracker.Grids.TrackingContainers.ActualDehire });
			AddColumn(new ZTextEditColumn("Vessel Name", TrackingContainer.Schema.VesselName) { ColumnKey = WebTracker.Grids.TrackingContainers.Vessel });
			AddColumn(new ZTextEditColumn("Voyage No", TrackingContainer.Schema.Voyage) { ColumnKey = WebTracker.Grids.TrackingContainers.Voyage });
			AddColumn(new ZCalcEditColumn("Delivery Sequence", TrackingContainer.Schema.JC_DeliverySequence) { ColumnKey = WebTracker.Grids.TrackingContainers.DeliverySequence });

			if (WebDataRegistry.Instance.ShowContainerStatusFromShipment.Value)
			{
				ZString customText1Label = FreightDataRegistry.Instance.ShipmentCustomText1.Value.Caption;

				if (!customText1Label.IsEmpty)
				{
					AddColumn(new ZTextEditColumn(customText1Label, TrackingContainer.Schema.ShipmentStatuses) { ColumnKey = WebTracker.Grids.TrackingContainers.ShipmentStatuses });
				}
			}

			foreach (ZTemplateColumn column in ConfigurationHelper.GetMilestonesColumns("Milestones")) // Data column name
			{
				AddColumn(column);
			}

			AddColumn(new ZTextEditColumn("Status", TrackingContainer.Schema.StatusDescription) { ColumnKey = WebTracker.Grids.TrackingContainers.StatusDescription });
			AddColumn(new ZDateTimeColumn("Storage Begins", TrackingContainer.Schema.StorageBegins, ZDateTimePickerFormat.Long) { ColumnKey = WebTracker.Grids.TrackingContainers.StorageBegins });
			AddColumn(new ZTextEditColumn("Container Status", TrackingContainer.Schema.ContainerStatus) { ColumnKey = WebTracker.Grids.TrackingContainers.ContainerStatus });
			AddColumn(new ZTextEditColumn("Verified Weight", TrackingContainer.Schema.WeightWithUnits) { ColumnKey = WebTracker.Grids.TrackingContainers.GrossWeight });
			AddColumn(new ZDateTimeColumn("Verified Date", TrackingContainer.Schema.JC_GrossWeightVerificationDateTime, ZDateTimePickerFormat.Long) { ColumnKey = WebTracker.Grids.TrackingContainers.VerifiedDate });
			AddColumn(new ZTextEditColumn("Verified Method", TrackingContainer.Schema.VerifiedMethod) { ColumnKey = WebTracker.Grids.TrackingContainers.VerifiedMethod });
			AddColumn(new ZTextEditColumn("Verified Company", TrackingContainer.Schema.VerifiedByCompany) { ColumnKey = WebTracker.Grids.TrackingContainers.VerifiedCompany });
			AddColumn(new ZTextEditColumn("Verified Contact", TrackingContainer.Schema.VerifiedByPerson) { ColumnKey = WebTracker.Grids.TrackingContainers.VerifiedContact });
			AddColumn(new ZTextEditColumn("Verified Phone", TrackingContainer.Schema.VerifiedByPhone) { ColumnKey = WebTracker.Grids.TrackingContainers.VerifiedPhone });
			AddColumn(new ZTextEditColumn("Verified Email", TrackingContainer.Schema.VerifiedByEmail) { ColumnKey = WebTracker.Grids.TrackingContainers.VerifiedEmail });
		}

		protected override GridColumnProvider GetNewTestProvider()
		{
			return new TrackingContainerColumnProvider();
		}

		protected void LoginAsQuickViewUserForTest() => LoginAsQuickViewUser();

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

		protected TrackingSiteUser LoggedSiteUserForTest => LoggedSiteUser;

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
