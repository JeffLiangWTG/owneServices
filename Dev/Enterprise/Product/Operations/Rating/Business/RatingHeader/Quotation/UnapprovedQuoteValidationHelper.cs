using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.Security;

namespace Enterprise.Rating.Business
{
	public class UnapprovedQuoteValidationHelper : LevelAuthorizationSecurityHelper<PaymentThreeLevelAuthorisationSettings, PaymentThreeLevelAuthorisationSettingsCollection>
	{
		public UnapprovedQuoteValidationHelper()
		{
			this.security = Env.Security;
		}

		public UnapprovedQuoteValidationHelper(SecurityCore security)
		{
			this.security = security;
		}

		readonly SecurityCore security;

		protected override PaymentThreeLevelAuthorisationSettingsCollection RegistryValue
		{
			get { return DataRegistryRating.Instance.SpotQuoteApprovalSettings.Value; }
		}

		protected override SecurityCheckpoint GetCheckPointFromRegistrySetting(PaymentThreeLevelAuthorisationSettings setting)
		{
			switch (setting?.AuthorisationRequirement)
			{
				case PaymentThreeLevelAuthorisationSettings.AuthorisationRequirementCodes.NoApprovalRequired:
					return security.None;

				case PaymentThreeLevelAuthorisationSettings.AuthorisationRequirementCodes.FirstApprovalRequiredOnly:
					return security.OneOffQuoteFirstLevelApproval;

				case PaymentThreeLevelAuthorisationSettings.AuthorisationRequirementCodes.SecondApprovalRequiredOnly:
					return security.OneOffQuoteSecondLevelApproval;

				case PaymentThreeLevelAuthorisationSettings.AuthorisationRequirementCodes.ThirdApprovalRequiredOnly:
					return security.OneOffQuoteThirdLevelApproval;

				default:
					return security.OneOffQuoteApproveOneOffQuotes;
			}
		}
	}
}

