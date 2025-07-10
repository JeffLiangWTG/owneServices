using System;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using Enterprise.Freight.QuotedBookings.Module;
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
	/// <summary>
	/// Summary description for TrackingBookingsModule.
	/// </summary>
	public class TrackingBookingsModule : ZFilterStripGridModule
	{
		public TrackingBookingsModule(BusinessObjectFactory factory, ZPage page)
			: base(factory, page)
		{
		}

		public override ModuleIdentifier ID => WebModuleIDs.TrackingBookings;

		public override Type GridCollectionType => typeof(ViewTrackingBookingCollection);

		protected override FilterLayoutCodePairRegistryItem DefaultLayoutRegistryItem => WebDataRegistry.Instance.DefaultFilterLayoutForwardingBookings;

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject() => new QuotedBookingFilterStripBusinessObject();

		protected override ZQuery GetCurrentLoggedInUserFilter(FilterBusinessObject filterBizO)
		{
			var result = ZQuery.NoResultQuery;
			if (Page.SiteUser.IsLoggedIn)
			{
				result = OrgRestrictionFilterFactory.Instance.GetFilter<TrackingBooking>();
			}

			return result;
		}

		#region Grid Columns

		protected override GridColumnProvider GetColumnProvider() => new TrackingBookingColumnProvider();

		#endregion

		#region Sorting

		public override ColumnAndSortOrder[] GetSortInfos(FilterBusinessObject filter) => new[] { new ColumnAndSortOrder(ViewQuotedBookingSchema.VB_JS.Name, DefaultSortOrder) };

		#endregion

		protected override SchemaPKColumn RelevantPersistantPKColumn => ViewQuotedBookingSchema.PK;
	}
}
