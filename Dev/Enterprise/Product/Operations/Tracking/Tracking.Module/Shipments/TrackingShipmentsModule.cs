using System;
using System.ComponentModel;
using System.Linq;
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
	/// <summary>
	/// Summary description for TrackingShipmentsModule.
	/// </summary>
	public class TrackingShipmentsModule : ZFilterStripGridModule
	{
		public TrackingShipmentsModule(BusinessObjectFactory factory, ZPage page)
			: base(factory, page)
		{
		}

		#region Overriden

		public override ModuleIdentifier ID => WebModuleIDs.TrackingShipments;

		public override Type GridCollectionType => typeof(ShipmentDeclarationCollection);

		protected override FilterLayoutCodePairRegistryItem DefaultLayoutRegistryItem => WebDataRegistry.Instance.DefaultFilterLayoutForwardingShipments;

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			var result = new TrackingShipmentFilterBusinessObject();
			if (Page.SiteUser != null)
			{
				result.LoggedInWebUsersOrg = ((OrgContactWebUser)Page.SiteUser).LoggedInOrganisation;
			}

			return result;
		}

		public override SchemaColumn GetBusinessObjectPKColumn(FilterBusinessObject filter) => ShipmentDeclarationSchema.PersistentBizOPK;

		protected override ZQuery GetCurrentLoggedInUserFilter(FilterBusinessObject filterBizO) => OrgRestrictionFilterFactory.Instance.GetFilter<TrackingShipment>();

		public override bool CacheCollectionPKs => false;

		protected override IBusinessObjectCollection LoadCollectionCore(FilterBusinessObject filterBizO, ZQuery query, IBusinessObjectCollection collection, bool ignoreCache)
		{
			if (collection is ShipmentDeclarationCollection shipDecCollection)
			{
				var sortInfo = GetSortInfos(filterBizO).Single();
				var sortOrderDirection = sortInfo.SortDirection == ListSortDirection.Ascending ? OrderByClause.Ascending : OrderByClause.Descending;

				query.AddToFilter(GetCurrentLoggedInUserFilter(filterBizO));
				query.OrderBy = GetOrderByColumn(sortInfo.OrderByColumnName, true).Name + sortOrderDirection + ", " + JobShipmentSchema.PK.Name; // Need it for better performance

				var declarationQuery = ((TrackingShipmentFilterBusinessObject)filterBizO).GetDeclarationQuery();
				declarationQuery.OrderBy = GetOrderByColumn(sortInfo.OrderByColumnName, false).Name + sortOrderDirection;

				shipDecCollection.Load(query, declarationQuery, sortInfo.OrderByColumnName, sortInfo.SortDirection);
			}

			return collection;
		}

		#endregion

		#region Grid Columns

		protected override GridColumnProvider GetColumnProvider() => new TrackingShipmentColumnProvider();

		#endregion

		#region Sorting

		public override ListSortDirection DefaultSortOrder => ListSortDirection.Descending;

		public override ColumnAndSortOrder[] GetSortInfos(FilterBusinessObject filter) => new[] { new ColumnAndSortOrder(ShipmentDeclarationSchema.ETA.Name, DefaultSortOrder) };

		#endregion

		#region Implementation

		protected override Type GetCancellableCollectionElementType() => typeof(TrackingShipment);

		protected SchemaColumn GetOrderByColumn(string shipmentDeclarationSchemaColumnName, bool forShipment)
		{
			if (shipmentDeclarationSchemaColumnName == ShipmentDeclarationSchema.ETA.Name)
			{
				return forShipment ? JobShipmentSchema.JS_E_ARV : JobDeclarationSchema.JE_DateAtFinalDestination;
			}

			throw new NotImplementedException("ETA column is expected here");
		}

		#endregion

		protected override SchemaPKColumn RelevantPersistantPKColumn => ShipmentDeclarationSchema.PersistentBizOPK;
	}
}
