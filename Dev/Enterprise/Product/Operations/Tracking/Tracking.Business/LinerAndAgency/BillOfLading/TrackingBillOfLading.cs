using System.Collections.Generic;
using System.Data;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Agency.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Web.Business;
using Enterprise.ZArchitecture.Web.Shared;

namespace Enterprise.Tracking.Business
{
	public partial class TrackingBillOfLading : BillOfLading,
	IMilestonesProvider,
	IUpdatableMilestoneEventsProvider,
	ITrackingEventsProvider,
	IWebUserVisibleNotesSupport,
	IEventReferenceProvider
	{
		#region Schema

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0001:Simplify Names", Justification = "Simplification hides desired base class")]
		public abstract new class Schema : AgencyBooking.Schema
		{
			public const string VolumeWithUnits = "VolumeWithUnits";
			public const string WeightWithUnits = "WeightWithUnits";
		}

		#endregion

		public TrackingBillOfLading(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public static TrackingBillOfLading FromPKBySiteUser(BusinessObjectFactory factory, ZGuid billOfLadingPK, TrackingSiteUser siteUser)
		{
			TrackingBillOfLading result = null;
			if (siteUser != null && siteUser.LoggedInOrganisation != null && !billOfLadingPK.IsEmpty)
			{
				ZDBOnlyQuery filter = new ZDBOnlyQuery(typeof(TrackingBillOfLading));
				filter.AddToFilter(JobShipmentSchema.PK, billOfLadingPK);
				filter.AddToFilter(OrgRestrictionFilterFactory.Instance.GetFilter<TrackingBillOfLading>());
				filter.IgnoreActiveFilter = true;
				result = factory.LoadTop1<TrackingBillOfLading>(filter);
			}
			return result;
		}

		#region VolumeWithUnits

		public ZString VolumeWithUnits
		{
			get
			{
				var roundedDecimal = this.GetRoundedValue(JobShipmentSchema.JS_ActualVolume, JS_ActualVolumeInfo, JS_ActualVolume);
				var formattedDecimal = FormatNumber(roundedDecimal, DefaultNumberOfDecimalsSupporterHelperForShipping.GetDefaultNumberOfDecimalsMetaDataProperty(this, JS_ActualVolumeInfo.PropertyDescriptor));
				return string.Format("{0} {1}", formattedDecimal, JS_UnitOfVolume);
			}
		}

		public ZPropertyInfo VolumeWithUnitsInfo
		{
			get { return GetZPropertyInfo(Schema.VolumeWithUnits); }
		}

		#endregion

		#region WeightWithUnits

		public ZString WeightWithUnits
		{
			get
			{
				var roundedDecimal = this.GetRoundedValue(JobShipmentSchema.JS_ActualWeight, JS_ActualWeightInfo, JS_ActualWeight);
				var formattedDecimal = FormatNumber(roundedDecimal, DefaultNumberOfDecimalsSupporterHelperForShipping.GetDefaultNumberOfDecimalsMetaDataProperty(this, JS_ActualWeightInfo.PropertyDescriptor));
				return string.Format("{0} {1}", formattedDecimal, JS_UnitOfWeight);
			}
		}

		public ZPropertyInfo WeightWithUnitsInfo
		{
			get { return GetZPropertyInfo(Schema.WeightWithUnits); }
		}

		protected string FormatNumber(ZDecimal number, int decimalsToShow)
		{
			return Utilities.FormatNumber(number, decimalsToShow, WebEnvShared.ClientCulture);
		}

		#endregion

		#region TrackingEvents

		public StmALogCollection TrackingEvents
		{
			get { return this.GetTrackingEvents(SiteUser); }
		}

		public bool CanViewTrackingEvents
		{
			get { return SiteUser?.CanViewEvents ?? false; }
		}

		TrackingSiteUser SiteUser
		{
			get { return WebEnv.AppInstance != null ? WebEnv.AppInstance.SiteUser as TrackingSiteUser : null; }
		}

		#endregion

		#region Milestones

		public void ReloadMilestones()
		{
			milestones = null;
		}

		public TrackingMilestoneCollection Milestones
		{
			get { return milestones ?? (milestones = new TrackingMilestoneCollection(this)); }
		}
		TrackingMilestoneCollection milestones;

		public TrackingMilestoneCollection EditableMilestones
		{
			get { return editableMilestones ?? (editableMilestones = new TrackingMilestoneCollection(this, true)); }
		}
		TrackingMilestoneCollection editableMilestones;

		#endregion

		#region IUpdatableMilestoneEventsProvider

		public List<string> UpdatableMilestoneEventCodes
		{
			get
			{
				if (WebEnv.AppInstance != null && WebEnv.AppInstance.SiteUser != null)
				{
					return (new UpdateableMilestoneEventsHelper(SiteUser)).GetUpdateableMilestoneEvents(WebDataRegistry.Instance.BillOfLadingMilestoneEventUpdates.Value, WebParties);
				}
				return new List<string>();
			}
		}

		#endregion

		#region IEventReferenceProvider

		public string EventReference
		{
			get
			{
				if (WebEnv.AppInstance != null && WebEnv.AppInstance.SiteUser != null)
				{
					return (new EventReferenceHelper(WebEnv.AppInstance.SiteUser as TrackingSiteUser)).GetEventReferences(WebParties);
				}
				return string.Empty;
			}
		}

		#endregion

		#region WebParties

		WebPartyTypeOrgPairCollection WebParties
		{
			get
			{
				if (webParties == null)
				{
					webParties = new WebPartyTypeOrgPairCollection();
					webParties.Add(WebPartyType.BookingParty, BookingParty);
					webParties.Add(WebPartyType.Consignor, Consignor);
					webParties.Add(WebPartyType.Consignee, Consignee);
				}

				return webParties;
			}
		}
		WebPartyTypeOrgPairCollection webParties;

		#endregion

		#region Overrides

		protected override bool EnableLightValidationIfAvailable
		{
			get { return false; }
		}

		#endregion

		#region IWebUserVisibleNotesSupport members

		public OrgContact LoggedInContact
		{
			get
			{
				return (WebEnv.AppInstance != null && WebEnv.AppInstance.SiteUser != null) ?
					WebEnv.AppInstance.SiteUser.LoggedInUser as OrgContact : null;
			}
		}

		public IStmNoteParent NotesParentBO
		{
			get { return this; }
		}

		public WebUserVisibleNotes NotesHelper
		{
			get { return notesHelper ?? (notesHelper = new WebUserVisibleNotes(this)); }
		}
		WebUserVisibleNotes notesHelper;

		public bool ShowAgentNotes
		{
			get { return false; }
		}

		#endregion
	}
}
