using System;
using System.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business.AWB;
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
	/// Summary description for TrackingMAWBHeaderModule.
	/// </summary>
	public class TrackingMAWBModule : ZFilterStripGridModule
	{
		public TrackingMAWBModule(BusinessObjectFactory factory, ZPage page)
			: base(factory, page)
		{
		}

		public override ModuleIdentifier ID
		{
			get { return WebModuleIDs.TrackingMAWB; }
		}

		public override Type GridCollectionType
		{
			get { return typeof(TrackingMAWBHeaderCollection); }
		}

		protected override FilterLayoutCodePairRegistryItem DefaultLayoutRegistryItem
		{
			get
			{
				return WebDataRegistry.Instance.DefaultFilterLayoutMAWB;
			}
		}

		protected override Type GetObjectsToFilterType()
		{
			return typeof(ExportAWBHeader);
		}

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			var result = new TrackingMAWBFilterBusinessObject();

			if (SiteUser != null)
			{
				result.LoggedInWebUser = SiteUser;
			}
			return result;
		}

		protected override ZQuery GetCurrentLoggedInUserFilter(FilterBusinessObject filterBizO)
		{
			if (SiteUser != null && SiteUser.LoggedInOrganisation != null)
			{
				ZQuery currentOrgFilter = new ZQuery();

				return currentOrgFilter;
			}
			else
			{
				return ZQuery.NoResultQuery;
			}
		}

		OrgContactWebUser SiteUser
		{
			get
			{
				return (Page != null ? Page.SiteUser : WebEnv.AppInstance.SiteUser) as OrgContactWebUser;
			}
		}

		protected override sealed ZString FilterStripLayoutContext
		{
			get { return WebModuleIDs.TrackingMAWB.Name; }
		}

		#region Grid Columns

		protected override GridColumnProvider GetColumnProvider()
		{
			return new TrackingMAWBHeaderColumnProvider();
		}

		#endregion

		#region Sorting

		public override ColumnAndSortOrder[] GetSortInfos(FilterBusinessObject filter)
		{
			return new[] { new ColumnAndSortOrder(ExportAWBHeaderSchema.EH_AWBIssueDate.Name, DefaultSortOrder) };
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
				return ExportAWBHeaderSchema.PK;
			}
		}
	}
}
