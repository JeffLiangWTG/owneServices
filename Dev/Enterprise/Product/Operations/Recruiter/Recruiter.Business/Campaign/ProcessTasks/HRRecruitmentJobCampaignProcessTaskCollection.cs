using Enterprise.MasterFiles.Business;

namespace Enterprise.Recruiter.Business
{
	public class HRRecruitmentJobCampaignProcessTaskCollection : ProcessTaskCollection
	{
		public HRRecruitmentJobCampaignProcessTaskCollection(HRRecruitmentJobCampaign hrRecruitmentJobCampaign)
			: base(hrRecruitmentJobCampaign)
		{ }

		public new HRRecruitmentJobCampaignProcessTask this[int index]
			=> (HRRecruitmentJobCampaignProcessTask)Elements[index];

		public new HRRecruitmentJobCampaignProcessTask AddNew()
			=> (HRRecruitmentJobCampaignProcessTask)base.AddNew();
	}
}
