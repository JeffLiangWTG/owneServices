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
	/// <summary>
	/// Summary description for TrackingCFSShipmentsModule.
	/// </summary>
	public class TrackingCFSShipmentsModule : ZFilterStripGridModule
	{
		public TrackingCFSShipmentsModule(BusinessObjectFactory factory, ZPage page)
			: base(factory, page)
		{
		}

		public override ModuleIdentifier ID
		{
			get { return WebModuleIDs.TrackingCFSShipments; }
		}

		public override Type GridCollectionType
		{
			get { return typeof(TrackingCFSShipmentsCollection); }
		}

		protected override FilterLayoutCodePairRegistryItem DefaultLayoutRegistryItem
		{
			get
			{
				return WebDataRegistry.Instance.DefaultFilterLayoutCFSShipments;
			}
		}

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new TrackingCFSShipmentFilterStripBusinessObject();
		}

		protected override ZQuery GetCurrentLoggedInUserFilter(FilterBusinessObject filterBizO) => OrgRestrictionFilterFactory.Instance.GetFilter<TrackingCFSShipment>();

		#region Grid Columns

		protected override GridColumnProvider GetColumnProvider()
		{
			return new TrackingCFSShipmentColumnProvider();
		}

		#endregion

		#region Sorting

		public override ListSortDirection DefaultSortOrder
		{
			get { return ListSortDirection.Descending; }
		}

		protected override SchemaPKColumn RelevantPersistantPKColumn
		{
			get
			{
				return JobShipmentSchema.PK;
			}
		}

		public override ColumnAndSortOrder[] GetSortInfos(FilterBusinessObject filter)
		{
			return new[] { new ColumnAndSortOrder(JobShipmentSchema.JS_E_ARV.Name, DefaultSortOrder) };
		}

		#endregion

	}
}
