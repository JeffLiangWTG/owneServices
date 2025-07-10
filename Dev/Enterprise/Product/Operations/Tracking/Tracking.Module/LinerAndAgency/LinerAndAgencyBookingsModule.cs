using System;
using System.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Freight.Agency.Module;
using Enterprise.Registry.Business;
using Enterprise.Tracking.Business;
using Enterprise.Tracking.Web;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Web.GUI;
using Enterprise.ZArchitecture.Web.GUI.WebControls;
using Enterprise.ZArchitecture.Web.Modules;

namespace Enterprise.Tracking.Module
{
	public class LinerAndAgencyBookingsModule : ZFilterStripGridModule
	{
		public LinerAndAgencyBookingsModule(BusinessObjectFactory factory, ZPage page)
			: base(factory, page)
		{
		}

		public override ModuleIdentifier ID
		{
			get { return WebModuleIDs.LinerAndAgencyBookings; }
		}

		public override Type GridCollectionType
		{
			get { return typeof(TrackingLinerAndAgencyBookingCollection); }
		}

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			var result = new AgencyBookingFilterStrip();
			result.AddModuleFiltersCreatedHook((x) => SetupFilterStripBusinessObject(x));

			return result;
		}

		void SetupFilterStripBusinessObject(FilterStripBusinessObject filterStripBizO)
		{
			ModuleGuidFilter clientFilter = filterStripBizO[AgencyShipmentFilterStrip.Descriptions.BookingParty] as ModuleGuidFilter;
			if (clientFilter != null && Page.SiteUser.IsLoggedIn)
			{
				clientFilter.Property = ZGuid.Empty; // ((OrgContactWebUser)Page.SiteUser).LoggedInOrganisation.PK;
				clientFilter.IsActive = true;
				clientFilter.Visibility = FilterVisibility.AlwaysAppliedAndHidden;
			}
		}

		protected override ZQuery GetCurrentLoggedInUserFilter(FilterBusinessObject filterBizO)
		{
			if (Page.SiteUser != null && Page.SiteUser.IsLoggedIn)
			{
				var currentOrgFilter = new ZDBOnlyQuery(typeof(TrackingLinerAndAgencyBooking));
				currentOrgFilter.AddToFilter(OrgRestrictionFilterFactory.Instance.GetFilter<TrackingLinerAndAgencyBooking>());

				return currentOrgFilter;
			}
			else
			{
				return ZQuery.NoResultQuery;
			}
		}

		#region Grid Columns

		protected override GridColumnProvider GetColumnProvider()
		{
			return new LinerAndAgencyBookingsColumnProvider();
		}

		#endregion

		#region Sorting

		public override ColumnAndSortOrder[] GetSortInfos(FilterBusinessObject filter)
		{
			return new[] { new ColumnAndSortOrder(JobShipmentSchema.JS_A_BKD.Name, DefaultSortOrder) };
		}

		public override ListSortDirection DefaultSortOrder
		{
			get { return ListSortDirection.Descending; }
		}

		#endregion

		protected override FilterLayoutCodePairRegistryItem DefaultLayoutRegistryItem
		{
			get { return WebDataRegistry.Instance.DefaultFilterLayoutLinerAndAgencyBookings; }
		}

		protected override SchemaPKColumn RelevantPersistantPKColumn
		{
			get
			{
				return JobShipmentSchema.PK;
			}
		}
	}
}
