using System;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using Enterprise.Registry.Business;
using Enterprise.Tracking.Business;
using Enterprise.Tracking.Web;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Environment.Module;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Web.GUI;
using Enterprise.ZArchitecture.Web.GUI.WebControls;
using Enterprise.ZArchitecture.Web.Modules;

namespace Enterprise.Tracking.Module
{
	public class TrackingWarehouseModule : ZFilterStripGridModule
	{
		public TrackingWarehouseModule(BusinessObjectFactory factory, ZPage page)
			: base(factory, page)
		{
		}

		public override ModuleIdentifier ID => WebModuleIDs.TrackingWarehouse;

		protected override FilterLayoutCodePairRegistryItem DefaultLayoutRegistryItem => WebDataRegistry.Instance.DefaultFilterLayoutWarehouses;

		public override Type GridCollectionType => typeof(WhsWarehouseCollection);

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject() => new WarehouseFilterBusinessObject();

		protected override ZQuery GetCurrentLoggedInUserFilter(FilterBusinessObject filterBizO) => OrgRestrictionFilterFactory.GetOrderedProhibitedWarehouseQuery();

		#region Grid Columns

		protected override GridColumnProvider GetColumnProvider() => new TrackingWarehouseColumnProvider();

		public override ColumnAndSortOrder[] GetSortInfos(FilterBusinessObject filter) => new[] { new ColumnAndSortOrder(WhsWarehouseSchema.WW_WarehouseName.Name, DefaultSortOrder) };

		#endregion

		protected override SchemaPKColumn RelevantPersistantPKColumn => WhsWarehouseSchema.PK;
	}
}
