using System;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Module;
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
	class DangerousGoodsModule : ZFilterStripGridModule
	{
		public DangerousGoodsModule(BusinessObjectFactory factory, ZPage page)
			: base(factory, page)
		{
		}

		public override ModuleIdentifier ID
		{
			get { return WebModuleIDs.DangerousGoods; }
		}

		public override Type GridCollectionType
		{
			get { return typeof(UNDGSubstanceCollection); }
		}

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new UNDGSubstanceFilterBusinessObject();
		}

		protected override ZQuery GetCurrentLoggedInUserFilter(FilterBusinessObject filterBizO) => new ZQuery();

		#region Grid Columns

		protected override GridColumnProvider GetColumnProvider()
		{
			return new DangerousGoodsColumnProvider();
		}

		#endregion

		#region Sorting

		public override ColumnAndSortOrder[] GetSortInfos(FilterBusinessObject filter)
		{
			return new[] { new ColumnAndSortOrder(UNDGSubstanceSchema.DG_Code.Name, DefaultSortOrder) };
		}

		#endregion

		protected override FilterLayoutCodePairRegistryItem DefaultLayoutRegistryItem
		{
			get { return WebDataRegistry.Instance.DefaultFilterLayoutLinerAndAgencyBookings; }
		}

		protected override SchemaPKColumn RelevantPersistantPKColumn
		{
			get
			{
				return UNDGSubstanceSchema.PK;
			}
		}
	}
}
