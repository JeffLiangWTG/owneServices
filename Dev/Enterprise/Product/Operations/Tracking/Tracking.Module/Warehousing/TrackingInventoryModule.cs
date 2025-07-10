using System;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
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
	/// Summary description for TrackingInventoryModule.
	/// </summary>
	public class TrackingInventoryModule : ZFilterStripGridModule
	{
		public TrackingInventoryModule(BusinessObjectFactory factory, ZPage page)
			: base(factory, page)
		{
		}

		protected override bool CacheCollection => true;

		public override ModuleIdentifier ID => WebModuleIDs.TrackingInventory;

		public override Type GridCollectionType => typeof(TrackingInventorySummaryCollection);

		protected override FilterLayoutCodePairRegistryItem DefaultLayoutRegistryItem => WebDataRegistry.Instance.DefaultFilterLayoutWarehouseInventory;

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			OrgHeader loggedInOrganisation = null;
			OrgContact loggedInUser = null;
			if (SiteUser != null)
			{
				loggedInOrganisation = SiteUser.LoggedInOrganisation;
				loggedInUser = SiteUser.LoggedInUser;
			}
			var result = new TrackingInventoryFilterBusinessObject(loggedInOrganisation);
			result.LoggedInUser = loggedInUser;

			return result;
		}

		OrgContactWebUser SiteUser => (Page != null ? Page.SiteUser : WebEnv.AppInstance.SiteUser) as OrgContactWebUser;

		protected override IBusinessObjectCollection LoadExcelCollection(FilterBusinessObject filterBizO, ZQuery query, IBusinessObjectCollection collection) => base.LoadCollectionCore(filterBizO, query, collection);

		protected override ZQuery GetCurrentLoggedInUserFilter(FilterBusinessObject filterBizO)
		{
			var orgRestrictionFilter = ZQuery.NoResultQuery;

			var siteUser = (OrgContactWebUser)Page.SiteUser;
			if (siteUser != null && siteUser.LoggedInOrganisation != null)
			{
				orgRestrictionFilter = OrgRestrictionFilterFactory.Instance.GetFilter<TrackingWhsInventory>();
			}

			return orgRestrictionFilter;
		}

		public override ZGuid[] GetBusinessObjectPK(BusinessObject bizO) => Array.Empty<ZGuid>();

		#region Grid Columns

		protected override GridColumnProvider GetColumnProvider() => new TrackingInventoryColumnProvider();

		protected override int SelectionColumnIndex => 1;

		#endregion

		#region Sorting

		public override ColumnAndSortOrder[] GetSortInfos(FilterBusinessObject filter) => new[] { new ColumnAndSortOrder(WhsTrackingInventorySummaryItemViewSchema.WI_ArrivalDate.Name, DefaultSortOrder) };

		#endregion

		public override string GetBusinessObjectTableName(FilterBusinessObject filter) => WhsTrackingInventorySummaryItemViewSchema.Constants.TableName;

		protected override SchemaPKColumn RelevantPersistantPKColumn => null;
	}
}
