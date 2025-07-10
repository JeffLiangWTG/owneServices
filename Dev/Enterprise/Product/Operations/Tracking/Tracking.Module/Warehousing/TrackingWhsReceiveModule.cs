using System;
using System.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.Tracking.Business;
using Enterprise.Tracking.Web;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Web.Business;
using Enterprise.ZArchitecture.Web.GUI;
using Enterprise.ZArchitecture.Web.GUI.WebControls;
using Enterprise.ZArchitecture.Web.Modules;

namespace Enterprise.Tracking.Module
{
	/// <summary>
	/// Summary description for TrackingWhsReceiveModule.
	/// </summary>
	public class TrackingWhsReceiveModule : ZFilterStripGridModule
	{
		public TrackingWhsReceiveModule(BusinessObjectFactory factory, ZPage page)
			: base(factory, page)
		{
		}

		public override ModuleIdentifier ID
		{
			get { return WebModuleIDs.TrackingWarehouseReceive; }
		}

		public override Type GridCollectionType
		{
			get { return typeof(TrackingWhsReceiveCollection); }
		}

		public override ZGuid[] GetBusinessObjectPK(BusinessObject bizO)
		{
			var trackingWhsReceive = bizO as TrackingWhsReceive;
			if (trackingWhsReceive != null)
			{
				return new ZGuid[] { trackingWhsReceive.WhsReceive.PK };
			}
			else
			{
				return new ZGuid[] { bizO.PK };
			}
		}

		protected override FilterLayoutCodePairRegistryItem DefaultLayoutRegistryItem
		{
			get { return WebDataRegistry.Instance.DefaultFilterLayoutWarehouseReceipts; }
		}

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			var result = new TrackingWhsReceiveFilterBusinessObject();
			if (SiteUser != null)
			{
				result.LoggedInWebUsersOrg = SiteUser.LoggedInOrganisation;
				result.LoggedInUser = SiteUser.LoggedInUser;
			}
			return result;
		}

		protected override ZQuery GetCurrentLoggedInUserFilter(FilterBusinessObject filterBizO)
		{
			ZQuery result = ZQuery.NoResultQuery;

			if (Page.SiteUser is OrgContactWebUser siteUser && siteUser.IsLoggedIn)
			{
				result = OrgRestrictionFilterFactory.Instance.GetFilter<TrackingWhsReceive>();
			}

			return result;
		}

		#region Grid Columns

		protected override GridColumnProvider GetColumnProvider()
		{
			return new TrackingWhsReceiveColumnProvider();
		}

		OrgContactWebUser SiteUser
		{
			get
			{
				return (Page != null ? Page.SiteUser : WebEnv.AppInstance.SiteUser) as OrgContactWebUser;
			}
		}

		#endregion

		#region Sorting

		public override ColumnAndSortOrder[] GetSortInfos(FilterBusinessObject filter)
		{
			return new[] { new ColumnAndSortOrder(WhsDocketSchema.WD_BookingDate.Name, DefaultSortOrder) };
		}

		public override ListSortDirection DefaultSortOrder
		{
			get { return ListSortDirection.Descending; }
		}

		#endregion

		protected override SchemaPKColumn RelevantPersistantPKColumn
		{
			get
			{
				return WhsDocketSchema.PK;
			}
		}
	}
}
