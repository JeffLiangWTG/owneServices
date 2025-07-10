using System;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Freight.LocalCartage.Business;
using Enterprise.Freight.LocalCartage.Module;
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
	public class TrackingCartageModule : ZFilterStripGridModule
	{
		public TrackingCartageModule(BusinessObjectFactory factory, ZPage page)
			: base(factory, page)
		{ }

		public override ModuleIdentifier ID
		{
			get { return WebModuleIDs.TrackingCartage; }
		}

		public override Type GridCollectionType
		{
			get { return typeof(TrackingCartageCollection); }
		}

		protected override FilterLayoutCodePairRegistryItem DefaultLayoutRegistryItem
		{
			get { return WebDataRegistry.Instance.DefaultFilterLayoutTrackingCartage; }
		}

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new CartageFilterBusinessObject();
		}

		protected override ZQuery GetCurrentLoggedInUserFilter(FilterBusinessObject filterBizO)
		{
			ZQuery currentLoggedInUserFilter = ZQuery.NoResultQuery;
			OrgContactWebUser siteUser = (OrgContactWebUser)Page.SiteUser;
			ZGuid currentOrg = siteUser.CurrentOrg;
			if (siteUser != null && siteUser.LoggedInOrganisation != null)
			{
				currentLoggedInUserFilter = OrgRestrictionFilterFactory.Instance.GetFilter<CommonCartage>();
			}

			return currentLoggedInUserFilter;
		}

		protected override GridColumnProvider GetColumnProvider()
		{
			return new TrackingCartageModuleColumnProvider();
		}

		public override ColumnAndSortOrder[] GetSortInfos(FilterBusinessObject filter)
		{
			return new[] { new ColumnAndSortOrder(JobCartageSchema.JJ_ConsignmentID.Name, DefaultSortOrder) };
		}

		protected override SchemaPKColumn RelevantPersistantPKColumn
		{
			get
			{
				return JobCartageSchema.PK;
			}
		}
	}
}
