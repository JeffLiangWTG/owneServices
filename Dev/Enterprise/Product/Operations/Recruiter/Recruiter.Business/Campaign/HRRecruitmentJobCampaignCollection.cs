using CargoWise.EntityFramework;

namespace Enterprise.Recruiter.Business
{
	public class HRRecruitmentJobCampaignCollection : BusinessObjectCollection<HRRecruitmentJobCampaign>
	{
		public HRRecruitmentJobCampaignCollection(BusinessObjectFactory factory)
			: base(factory)
		{ }

		public HRRecruitmentJobCampaignCollection(BusinessObjectFactory factory, ZQuery filter)
			: base(factory, filter)
		{ }

		protected override IFindBoxListProvider FindBoxListProvider
		{
			get { return new HRRecruitmentJobCampaignFindBoxListProvider(this); }
		}
	}
}
