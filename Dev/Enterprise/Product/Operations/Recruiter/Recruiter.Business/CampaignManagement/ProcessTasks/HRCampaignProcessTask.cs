using System;
using System.Data;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Recruiter.Business
{
	public class HRCampaignProcessTasks : ProcessTask, Enterprise.Integration.Recruiter.IHRCampaignProcessTasks
	{
		public HRCampaignProcessTasks(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Parent

		public override ControllerID ParentControllerID
		{
			get { return ControllerIDs.HRGlbCompanyCampaign; }
		}

		protected override Type ParentType
		{
			get { return typeof(HRGlbCompanyCampaign); }
		}

		#endregion
	}
}
