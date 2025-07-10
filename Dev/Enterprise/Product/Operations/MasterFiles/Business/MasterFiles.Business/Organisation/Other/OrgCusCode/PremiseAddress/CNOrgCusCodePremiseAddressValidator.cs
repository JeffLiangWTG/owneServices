namespace Enterprise.MasterFiles.Business
{
	class CNOrgCusCodePremiseAddressValidator : CountrySpecificOrgCusCodePremiseAddressValidator
	{
		protected override bool IsPremiseAddressAllowedWithoutBeingRequired(string codeType)
		{
			return new Customs.CN.EnterpriseQualificationList().ContainsCode(codeType);
		}

		public override bool IsPremiseAddressRequired(string codeType) => false;
	}
}
