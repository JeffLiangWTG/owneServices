using System;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using Enterprise.Freight.Business;
using Enterprise.Freight.Module;
using Enterprise.Registry.Business;
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
	public class TrackingRoadSchedulesModule : ZFilterStripGridModule
	{
		public TrackingRoadSchedulesModule(BusinessObjectFactory factory, ZPage page) : base(factory, page) { }

		public override ModuleIdentifier ID => WebModuleIDs.TrackingRoadSchedules;

		protected override FilterLayoutCodePairRegistryItem DefaultLayoutRegistryItem => WebDataRegistry.Instance.DefaultFilterLayoutForwardingRoadSchedules;

		public override ColumnAndSortOrder[] GetSortInfos(FilterBusinessObject filter) => new[] { new ColumnAndSortOrder(JobSailingSchema.JX_DepotAvailabilityDate.Name, DefaultSortOrder) };

		public override Type GridCollectionType => typeof(JobSailingCollection);

		protected override ZQuery GetCurrentLoggedInUserFilter(FilterBusinessObject filterBizO) => null;

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject() => new JobRoadSailingFilterBusinessObject { UserIsLoggedIn = Page.SiteUser.IsLoggedIn };

		protected override GridColumnProvider GetColumnProvider() => new ScheduleColumnProvider(ID.Name, IsUsedAsLookup);

		protected override SchemaPKColumn RelevantPersistantPKColumn => JobSailingSchema.PK;
	}
}
