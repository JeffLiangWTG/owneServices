namespace Enterprise.MasterFiles.Business
{
	class ITOrgCusCodePremiseAddressValidator : CountrySpecificOrgCusCodePremiseAddressValidator
	{
		protected override bool IsPremiseAddressAllowedWithoutBeingRequired(string codeType)
		{
			switch (codeType)
			{
				case ItalyOrgCusCodeInfo.OrgCusCodes.CertifiedEmail:
					return true;
				default:
					return false;
			}
		}

		public override bool IsPremiseAddressRequired(string codeType) => false;
	}
}
