using System;
using System.Data;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Recruiter.Business
{
	public class HRRecruitmentJobCampaignProcessTask
		: ProcessTask
		, Enterprise.Integration.Recruiter.IHRRecruitmentJobCampaignProcessTask
	{
		public HRRecruitmentJobCampaignProcessTask(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{ }

		public override ControllerID ParentControllerID
			=> ControllerIDs.HRJobOpenings;

		protected override Type ParentType
			=> typeof(HRRecruitmentJobCampaign);
	}
}
