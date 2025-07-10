using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Recruiter.Business
{
	public class HRRecruitmentJobCampaignDocManagerInfo : DocManagerInfo
	{
		public HRRecruitmentJobCampaignDocManagerInfo(HRRecruitmentJobCampaign parent, ZString docManagerCode)
			: base(parent, docManagerCode)
		{
		}

		protected override BusinessObject[] GetRelatedObjects()
		{
			List<BusinessObject> result = new List<BusinessObject>();

			HRRecruitmentJobCampaign jobCampaign = (HRRecruitmentJobCampaign)BusinessEntity;
			result.AddRange(jobCampaign.Applications.ToArray());

			return result.ToArray();
		}
	}
}
