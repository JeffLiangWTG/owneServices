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
	/// Summary description for TrackingOrdersModule.
	/// </summary>
	public class TrackingOrdersModule : ZFilterStripGridModule
	{
		public TrackingOrdersModule(BusinessObjectFactory factory, ZPage page)
			: base(factory, page)
		{
		}

		public override ModuleIdentifier ID
		{
			get { return WebModuleIDs.TrackingOrders; }
		}

		public override Type GridCollectionType
		{
			get { return typeof(TrackingOrderCollection); }
		}

		protected override FilterLayoutCodePairRegistryItem DefaultLayoutRegistryItem
		{
			get
			{
				return WebDataRegistry.Instance.DefaultFilterLayoutForwardingOrders;
			}
		}

		protected override Type GetObjectsToFilterType()
		{
			return typeof(Order);
		}

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			OrdersFilterBusinessObject result = new OrdersFilterBusinessObject();
			if (SiteUser != null)
			{
				result.LoggedInWebUsersOrg = SiteUser.LoggedInOrganisation;
			}
			return result;
		}

		protected override ZQuery GetCurrentLoggedInUserFilter(FilterBusinessObject filterBizO)
		{
			if (SiteUser != null && SiteUser.LoggedInOrganisation != null)
			{
				ZQuery currentOrgFilter = new ZQuery();
				ZGuid currentOrgPK = SiteUser.LoggedInOrganisation.PK;
				ZDBOnlyQuery onlyMyOrgFilter = new ZDBOnlyQuery(typeof(Order));
				onlyMyOrgFilter.AddToFilter(OrgRestrictionFilterFactory.Instance.GetFilter<TrackingOrder>());
				ZDBOnlySubQuery shipmentOrgs = new ZDBOnlySubQuery(typeof(TrackingShipment), JobOrderHeaderSchema.JD_JS);
				shipmentOrgs.AddToFilter(OrgRestrictionFilterFactory.Instance.GetFilter<TrackingShipment>());
				onlyMyOrgFilter.AddSubQuery(shipmentOrgs, JoinCondition.Or);
				currentOrgFilter.AddToFilter(onlyMyOrgFilter);

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
			get { return WebModuleIDs.TrackingOrders.Name; } // We want this to be the same for Orders & Timeline
		}

		#region Grid Columns

		protected override GridColumnProvider GetColumnProvider()
		{
			return new TrackingOrderColumnProvider(IsUsedAsLookup);
		}

		#endregion

		#region Sorting

		public override ColumnAndSortOrder[] GetSortInfos(FilterBusinessObject filter)
		{
			return new[] { new ColumnAndSortOrder(JobOrderHeaderSchema.JD_OrderDate.Name, DefaultSortOrder) };
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
				return JobOrderHeaderSchema.PK;
			}
		}
	}
}
