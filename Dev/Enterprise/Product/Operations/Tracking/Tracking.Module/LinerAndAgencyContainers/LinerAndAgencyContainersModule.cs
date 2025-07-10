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
using Enterprise.ZArchitecture.Web.Business;
using Enterprise.ZArchitecture.Web.GUI;
using Enterprise.ZArchitecture.Web.GUI.WebControls;
using Enterprise.ZArchitecture.Web.Modules;

namespace Enterprise.Tracking.Module
{
	public class LinerAndAgencyContainersModule : ZFilterStripGridModule
	{
		public LinerAndAgencyContainersModule(BusinessObjectFactory factory, ZPage page)
			: base(factory, page)
		{
		}

		public override ModuleIdentifier ID => WebModuleIDs.LinerAndAgencyContainers;

		protected override FilterLayoutCodePairRegistryItem DefaultLayoutRegistryItem => WebDataRegistry.Instance.DefaultFilterLayoutLinerAndAgencyContainers;

		public override Type GridCollectionType => typeof(LinerAndAgencyContainerStandaloneCollection);

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			var result = new LinerAndAgencyContainerFilterStripBusinessObject();
			result.LoggedInUser = SiteUser.LoggedInUser;

			return result;
		}

		OrgContactWebUser SiteUser => (Page != null ? Page.SiteUser : WebEnv.AppInstance.SiteUser) as OrgContactWebUser;

		protected override ZQuery GetCurrentLoggedInUserFilter(FilterBusinessObject filterBizO) => OrgRestrictionFilterFactory.Instance.GetFilter<LinerAndAgencyContainer>();

		protected override GridColumnProvider GetColumnProvider() => new LinerAndAgencyContainerColumnProvider();

		public override SchemaColumn GetBusinessObjectPKColumn(FilterBusinessObject filter) => JobContainerSchema.PK;

		protected override SchemaPKColumn RelevantPersistantPKColumn => JobContainerSchema.PK;

		#region Sorting

		public override ColumnAndSortOrder[] GetSortInfos(FilterBusinessObject filter) => new[] { new ColumnAndSortOrder(JobContainerSchema.JC_ArrivalEstimatedDelivery.Name, DefaultSortOrder) };

		public override ListSortDirection DefaultSortOrder => ListSortDirection.Descending;

		#endregion

	}
}
