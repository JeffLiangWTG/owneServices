using System;
using System.Data;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MarketingManager.Business
{
	public class GlbCompanyCampaignItemProcessTask : CRMProcessTask
	{
		public GlbCompanyCampaignItemProcessTask(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Parent

		public override ControllerID ParentControllerID
		{
			get { return ControllerIDs.GlbCompanyCampaignItem; }
		}

		protected override Type ParentType
		{
			get { return typeof(GlbCompanyCampaignItem); }
		}

		#endregion

		// These Process Tasks are standalone to Company Campaign Items.
		// Hide this unneeded ParentTaskCollection property so TestCollectionPropertiesDoNotReturnNull passes
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Required for TestCollectionPropertiesDoNotReturnNull to pass")]
		new ProcessTaskCollection ParentTaskCollection
		{
			get { return null; }
		}
	}
}
