using System.Collections.Generic;
using System.Linq;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Recruiter.Business.Applicant
{
	public static class HRJobApplicantContactPhoneDialBuilder
	{
		public static PhoneDialInfo GetDefaultDialInfo(HRJobApplicant applicant)
		{
			var result = GetAlternativePhoneDialInfos(applicant);
			return result?.FirstOrDefault();
		}

		public static IEnumerable<PhoneDialInfo> GetAlternativePhoneDialInfos(HRJobApplicant applicant)
		{
			if (applicant != null)
			{
				if (!applicant.HA_MobilePhone.IsEmpty)
				{
					yield return new PhoneDialInfo(applicant.HA_MobilePhone, PhoneContactItemDescriptionList.Descriptions.Mobile);
				}
				if (!applicant.HA_HomePhone.IsEmpty)
				{
					yield return new PhoneDialInfo(applicant.HA_HomePhone, PhoneContactItemDescriptionList.Descriptions.Home);
				}
				if (!applicant.HA_WorkPhone.IsEmpty)
				{
					yield return new PhoneDialInfo(applicant.HA_WorkPhone, PhoneContactItemDescriptionList.Descriptions.Work);
				}
			}
		}
	}
}
