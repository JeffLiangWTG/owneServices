namespace Enterprise.MasterFiles.Business
{
	class FROrgCusCodePremiseAddressValidator : CountrySpecificOrgCusCodePremiseAddressValidator
	{
		protected override bool IsPremiseAddressAllowedWithoutBeingRequired(string codeType)
		{
			switch (codeType)
			{
				case OrgCusCode.FranceCodeTypes.EoriBranchSuffix:
				case OrgCusCode.FranceCodeTypes.CI5:
				case OrgCusCode.FranceCodeTypes.SON:
				case OrgCusCode.FranceCodeTypes.SOA:
				case OrgCusCode.FranceCodeTypes.SOW:
				case OrgCusCode.FranceCodeTypes.CIN:
					return true;
				default:
					return false;
			}
		}

		public override bool IsPremiseAddressRequired(string codeType) => false;
	}
}

