using CargoWise.EntityFramework;

namespace Enterprise.Recruiter.Business
{
	public class HRJobAdPlacementDependentCollection : DependentBusinessObjectCollection<HRJobAdPlacement, HRRecruitmentJobCampaign>
	{
		public HRJobAdPlacementDependentCollection(HRRecruitmentJobCampaign parent) : base(parent)
		{
			this.Parent = parent;
		}

		readonly HRRecruitmentJobCampaign Parent;

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);
			if (Parent.HV_CampaignStartDate.IsValid)
			{
				((HRJobAdPlacement)child).HQ_EffectiveStartDate = Parent.HV_CampaignStartDate;
			}
			if (Parent.HV_CampaignEndDate.IsValid)
			{
				((HRJobAdPlacement)child).HQ_EffectiveEndDate = Parent.HV_CampaignEndDate;
			}
		}
	}
}
