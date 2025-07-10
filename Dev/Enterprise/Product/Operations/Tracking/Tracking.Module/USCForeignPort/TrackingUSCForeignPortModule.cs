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
	/// Summary description for TrackingUSCForeignPortModule.
	/// </summary>
	public class TrackingUSCForeignPortModule : ZFilterStripGridModule
	{
		public TrackingUSCForeignPortModule(BusinessObjectFactory factory, ZPage page)
			: base(factory, page)
		{
		}

		public override ModuleIdentifier ID
		{
			get { return WebModuleIDs.TrackingUSCForeignPort; }
		}

		public override Type GridCollectionType
		{
			get { return typeof(USCForeignPortCollection); }
		}

		protected override FilterLayoutCodePairRegistryItem DefaultLayoutRegistryItem
		{
			get { return WebDataRegistry.Instance.DefaultFilterLayoutUSCForeignPort; }
		}

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new USCForeignPortFilterStripBusinessObject();
		}

		protected override ZQuery GetCurrentLoggedInUserFilter(FilterBusinessObject filterBizO) => new ZQuery();

		#region Grid Columns

		protected override GridColumnProvider GetColumnProvider()
		{
			return new TrackingUSCForeignPortColumnProvider();
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
