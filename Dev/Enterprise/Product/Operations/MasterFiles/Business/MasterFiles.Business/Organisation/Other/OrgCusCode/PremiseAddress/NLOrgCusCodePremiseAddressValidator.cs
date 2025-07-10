namespace Enterprise.MasterFiles.Business
{
	class NLOrgCusCodePremiseAddressValidator : CountrySpecificOrgCusCodePremiseAddressValidator
	{
		protected override bool IsPremiseAddressAllowedWithoutBeingRequired(string codeType)
		{
			return codeType == OrgCusCode.NetherlandsCodeTypes.CargonautRegistationCode;
		}

		public override bool IsPremiseAddressRequired(string codeType) => false;
	}
}
