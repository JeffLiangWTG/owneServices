namespace Enterprise.MasterFiles.Business
{
	class UKOrgCusCodePremiseAddressValidator : CountrySpecificOrgCusCodePremiseAddressValidator
	{
		protected override bool IsPremiseAddressAllowedWithoutBeingRequired(string codeType)
		{
			switch (codeType)
			{
				case OrgCusCode.UnitedKingdomCodeTypes.EoriBranchSuffix:
				case OrgCusCode.UnitedKingdomCodeTypes.CTOShed:
				case OrgCusCode.EuropeanUnionSharedCodeTypes.Eori:
					return true;
				default:
					return false;
			}
		}

		public override bool IsPremiseAddressRequired(string codeType)
		{
			return codeType == OrgCusCode.CodeTypes.VGMRegistrationNumber;
		}
	}
}
