using System;
using System.Linq;
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
	/// Summary description for TrackingWhsOrderModule.
	/// </summary>
	public class TrackingWhsOrderModule : ZFilterStripGridModule
	{
		public TrackingWhsOrderModule(BusinessObjectFactory factory, ZPage page)
			: base(factory, page)
		{
		}

		public override ModuleIdentifier ID
		{
			get { return WebModuleIDs.TrackingWarehouseOrders; }
		}

		public override Type GridCollectionType
		{
			get { return typeof(TrackingWhsOrderCollection); }
		}

		public override ZGuid[] GetBusinessObjectPK(BusinessObject bizO)
		{
			var trackingWhsOrder = bizO as TrackingWhsOrder;
			if (trackingWhsOrder != null)
			{
				return new ZGuid[] { trackingWhsOrder.WhsOrder.PK };
			}
			else
			{
				return new ZGuid[] { bizO.PK };
			}
		}

		protected override FilterLayoutCodePairRegistryItem DefaultLayoutRegistryItem
		{
			get { return WebDataRegistry.Instance.DefaultFilterLayoutWarehouseOrders; }
		}

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			var result = new TrackingWhsOrderFilterBusinessObject();
			if (SiteUser != null)
			{
				result.LoggedInWebUsersOrg = SiteUser.LoggedInOrganisation;
				result.LoggedInUser = SiteUser.LoggedInUser;
			}
			return result;
		}

		protected override ZQuery GetCurrentLoggedInUserFilter(FilterBusinessObject filterBizO)
		{
			ZQuery currentLoggedInUserFilter = ZQuery.NoResultQuery;

			OrgContactWebUser siteUser = Page.SiteUser as OrgContactWebUser;

			if (siteUser != null && siteUser.LoggedInOrganisation != null)
			{
				currentLoggedInUserFilter = OrgRestrictionFilterFactory.Instance.GetFilter<TrackingWhsOrder>();
			}

			return currentLoggedInUserFilter;
		}

		#region Grid Columns

		protected override GridColumnProvider GetColumnProvider()
		{
			return new TrackingWhsOrderColumnProvider();
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
			return new[] { new ColumnAndSortOrder(WhsDocketSchema.WD_RequiredDate.Name, DefaultSortOrder) };
		}

		#endregion

		public override ZGuid GetEDocsBulkDownloadRelevantPK(ZDataGrid grid, int itemIndex)
		{
			return grid.GetPKByRowIndex(itemIndex);
		}

		public override ZString GetPersistantBizoHumanReadableName(ZDataGrid grid, ZGuid persistantBizoPK)
		{
			var collection = grid.DataSource as TrackingWhsOrderCollection;
			var whsOrder = collection.Cast<TrackingWhsOrder>().First(trackingWhsOrder => trackingWhsOrder.WhsOrder.PK == persistantBizoPK);
			return whsOrder.WhsOrder.HumanReadableName;
		}

		protected override SchemaPKColumn RelevantPersistantPKColumn
		{
			get { return WhsDocketSchema.PK; }
		}
	}
}
