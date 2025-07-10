using System;
using System.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.Freight.Forwarding.Orders.Module;
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
	/// Summary description for TrackingOrderLinesModule.
	/// </summary>
	public class TrackingOrderLinesModule : ZFilterStripGridModule
	{
		public TrackingOrderLinesModule(BusinessObjectFactory factory, ZPage page)
			: base(factory, page)
		{
		}

		public override ModuleIdentifier ID
		{
			get { return WebModuleIDs.TrackingOrderLines; }
		}

		public override Type GridCollectionType
		{
			get { return typeof(TrackingOrderLineCollection); }
		}

		protected override FilterLayoutCodePairRegistryItem DefaultLayoutRegistryItem
		{
			get
			{
				return WebDataRegistry.Instance.DefaultFilterLayoutForwardingOrderLines;
			}
		}

		protected override Type GetObjectsToFilterType()
		{
			return typeof(OrderLine);
		}

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			OrderLineFilterBusinessObject result = new OrderLineFilterBusinessObject();
			if (SiteUser != null)
			{
				result.LoggedInWebUsersOrg = SiteUser.LoggedInOrganisation;
			}
			return result;
		}

		protected override ZQuery GetCurrentLoggedInUserFilter(FilterBusinessObject filterBizO) => new ZQuery();

		OrgContactWebUser SiteUser
		{
			get
			{
				return (Page != null ? Page.SiteUser : WebEnv.AppInstance.SiteUser) as OrgContactWebUser;
			}
		}

		protected override sealed ZString FilterStripLayoutContext
		{
			get { return WebModuleIDs.TrackingOrderLines.Name; } // We want this to be the same for Orders & Timeline
		}

		#region Grid Columns

		protected override GridColumnProvider GetColumnProvider()
		{
			return new TrackingOrderLinesColumnProvider();
		}

		#endregion

		#region Sorting

		public override ColumnAndSortOrder[] GetSortInfos(FilterBusinessObject filter)
		{
			return new[] { new ColumnAndSortOrder(JobOrderLineSchema.JO_LineNo.Name, DefaultSortOrder) };
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
				return JobOrderLineSchema.PK;
			}
		}
	}
}
