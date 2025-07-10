using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public class OrgCusCodeValidityLookups : AutoOrgCusCodeValidityLookups
	{
		public OrgCusCodeValidityLookups(AutoOrgCusCodeValidity parent) : base(parent)
		{
		}

		#region VerificationStatus

		public CodeDescriptionPairList VerificationStatusList
		{
			get
			{
				return Factory.GetCachedValue("CusCodeValidityVerificationList", () =>
				{
					var result = new CodeDescriptionPairList();
					result.AddPair(OrgConstants.CusCodeValidityVerification.Verified, OrgConstants.CusCodeValidityVerification.Verified);
					result.AddPair(OrgConstants.CusCodeValidityVerification.NotVerified, OrgConstants.CusCodeValidityVerification.NotVerified);
					return result;
				});
			}
		}

		#endregion
	}
}
