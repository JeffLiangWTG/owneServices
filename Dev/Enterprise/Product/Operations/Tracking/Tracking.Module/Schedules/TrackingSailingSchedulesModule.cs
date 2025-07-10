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
	public class TrackingSailingSchedulesModule : ZFilterStripGridModule
	{
		public TrackingSailingSchedulesModule(BusinessObjectFactory factory, ZPage page) : base(factory, page) { }

		public override ModuleIdentifier ID
		{
			get { return WebModuleIDs.TrackingSailingSchedules; }
		}

		protected override FilterLayoutCodePairRegistryItem DefaultLayoutRegistryItem
		{
			get { return WebDataRegistry.Instance.DefaultFilterLayoutForwardingSailingSchedules; }
		}

		protected override ZQuery GetCurrentLoggedInUserFilter(FilterBusinessObject filterBizO) => null;

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new JobSeaSailingFilterBusinessObject { UserIsLoggedIn = Page.SiteUser.IsLoggedIn };
		}

		public override Type GridCollectionType
		{
			get { return typeof(JobSailingCollection); }
		}

		public override ColumnAndSortOrder[] GetSortInfos(FilterBusinessObject filter)
		{
			return new[] { new ColumnAndSortOrder(JobSailingSchema.JX_DepotAvailabilityDate.Name, DefaultSortOrder) };
		}

		protected override GridColumnProvider GetColumnProvider()
		{
			return new ScheduleColumnProvider(ID.Name, IsUsedAsLookup);
		}

		protected override SchemaPKColumn RelevantPersistantPKColumn
		{
			get
			{
				return JobSailingSchema.PK;
			}
		}
	}
}
