namespace Enterprise.MasterFiles.Business
{
	class ZAOrgCusCodePremiseAddressValidator : CountrySpecificOrgCusCodePremiseAddressValidator
	{
		public override bool IsPremiseAddressRequired(string codeType)
		{
			return codeType == OrgCusCode.CodeTypes.VGMRegistrationNumber;
		}
	}
}
