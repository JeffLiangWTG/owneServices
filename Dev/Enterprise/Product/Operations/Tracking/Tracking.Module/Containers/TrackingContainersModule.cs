using System;
using System.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Schema;
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
	public class TrackingContainersModule : ZFilterStripGridModule
	{
		public TrackingContainersModule(BusinessObjectFactory factory, ZPage page)
			: base(factory, page)
		{
		}

		protected override bool CacheCollection => true;

		public override ModuleIdentifier ID
		{
			get { return WebModuleIDs.TrackingContainers; }
		}

		protected override FilterLayoutCodePairRegistryItem DefaultLayoutRegistryItem
		{
			get { return WebDataRegistry.Instance.DefaultFilterLayoutForwardingContainers; }
		}

		public override Type GridCollectionType
		{
			get { return typeof(TrackingContainerStandaloneCollection); }
		}

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new TrackingContainerFilterStripBusinessObject();
		}

		protected override ZQuery GetCurrentLoggedInUserFilter(FilterBusinessObject filterBizO) => OrgRestrictionFilterFactory.Instance.GetFilter<TrackingContainer>();

		protected override GridColumnProvider GetColumnProvider()
		{
			return new TrackingContainerColumnProvider();
		}

		public override SchemaColumn GetBusinessObjectPKColumn(FilterBusinessObject filter)
		{
			return JobContainerSchema.PK;
		}

		protected override SchemaPKColumn RelevantPersistantPKColumn
		{
			get
			{
				return JobContainerSchema.PK;
			}
		}

		protected override IBusinessObjectCollection LoadCollectionCore(FilterBusinessObject filterBizO, ZQuery query, IBusinessObjectCollection collection, bool ignoreCache)
		{
			query.LoadSmallBlobs = 0;

			return base.LoadCollectionCore(filterBizO, query, collection, ignoreCache);
		}

		#region Sorting

		public override ColumnAndSortOrder[] GetSortInfos(FilterBusinessObject filter)
		{
			return new[] { new ColumnAndSortOrder(JobContainerSchema.JC_ArrivalEstimatedDelivery.Name, DefaultSortOrder) };
		}

		public override ListSortDirection DefaultSortOrder
		{
			get { return ListSortDirection.Descending; }
		}

		#endregion
	}
}
