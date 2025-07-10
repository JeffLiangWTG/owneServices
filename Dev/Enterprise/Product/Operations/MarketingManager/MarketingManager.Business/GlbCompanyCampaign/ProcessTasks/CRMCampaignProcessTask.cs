using System;
using System.Data;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MarketingManager.Business
{
	public class CRMCampaignProcessTasks : ProcessTask, Enterprise.Integration.MarketingManager.ICRMCampaignProcessTasks
	{
		public CRMCampaignProcessTasks(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Parent

		public override ControllerID ParentControllerID
		{
			get { return ControllerIDs.GlbCompanyCampaign; }
		}

		protected override Type ParentType
		{
			get { return typeof(GlbCompanyCampaign); }
		}

		#endregion
	}
}
