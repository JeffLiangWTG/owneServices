using System;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Module;
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
	/// Summary description for OrgSupplierPartModule.
	/// </summary>
	public class OrgSupplierPartModule : ZFilterStripGridModule
	{
		public OrgSupplierPartModule(BusinessObjectFactory factory, ZPage page)
			: base(factory, page)
		{
		}

		public override ModuleIdentifier ID
		{
			get { return WebModuleIDs.OrgSupplierPartTracking; }
		}

		public override Type GridCollectionType
		{
			get { return typeof(OrgSupplierPartCollection); }
		}

		protected override FilterLayoutCodePairRegistryItem DefaultLayoutRegistryItem
		{
			get
			{
				return WebDataRegistry.Instance.DefaultFilterLayoutWarehouseProducts;
			}
		}

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new OrgSupplierPartFilterStripBusinessObject();
		}

		protected override ZQuery GetCurrentLoggedInUserFilter(FilterBusinessObject filterBizO) => OrgRestrictionFilterFactory.Instance.GetSubQuery(typeof(OrgSupplierPart), typeof(OrgPartRelation), OrgPartRelationSchema.OU_OP);

		#region Grid Columns

		protected override GridColumnProvider GetColumnProvider()
		{
			return new OrgSupplierPartColumnProvider(IsUsedAsLookup);
		}

		#endregion

		#region Sorting

		public override ColumnAndSortOrder[] GetSortInfos(FilterBusinessObject filter)
		{
			return new[] { new ColumnAndSortOrder(OrgSupplierPartSchema.OP_PartNum.Name, DefaultSortOrder) };
		}

		#endregion

		protected override SchemaPKColumn RelevantPersistantPKColumn
		{
			get
			{
				return OrgSupplierPartSchema.PK;
			}
		}
	}
}
