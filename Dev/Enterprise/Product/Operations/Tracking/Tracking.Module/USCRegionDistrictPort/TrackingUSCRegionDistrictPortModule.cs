using System;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using Enterprise.Customs.US.Business;
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
	/// <summary>
	/// Summary description for TrackingUSCRegionDistrictPortModule.
	/// </summary>
	public class TrackingUSCRegionDistrictPortModule : ZFilterStripGridModule
	{
		public TrackingUSCRegionDistrictPortModule(BusinessObjectFactory factory, ZPage page)
			: base(factory, page)
		{
		}

		public override ModuleIdentifier ID
		{
			get { return WebModuleIDs.TrackingUSCRegionDistrictPort; }
		}

		public override Type GridCollectionType
		{
			get { return typeof(USRegionDistrictPortCollection); }
		}

		protected override FilterLayoutCodePairRegistryItem DefaultLayoutRegistryItem
		{
			get { return WebDataRegistry.Instance.DefaultFilterLayoutUSCRegionDistrictPort; }
		}

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new USCRegionDistrictPortFilterStripBusinessObject();
		}

		protected override ZQuery GetCurrentLoggedInUserFilter(FilterBusinessObject filterBizO) => new ZQuery();

		#region Grid Columns

		protected override GridColumnProvider GetColumnProvider()
		{
			return new TrackingUSCRegionDistrictPortColumnProvider();
		}

		#endregion

		#region Sorting

		public override ColumnAndSortOrder[] GetSortInfos(FilterBusinessObject filter)
		{
			return new[] { new ColumnAndSortOrder(ZZRefCusCodeListCombinedSchema.ZZD_Code.Name, DefaultSortOrder) };
		}

		#endregion

		protected override SchemaPKColumn RelevantPersistantPKColumn
		{
			get
			{
				return ZZRefCusCodeListCombinedSchema.PK;
			}
		}
	}
}
