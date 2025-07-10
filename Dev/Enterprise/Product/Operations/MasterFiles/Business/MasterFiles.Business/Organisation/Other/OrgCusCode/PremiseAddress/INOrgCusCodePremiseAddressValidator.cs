namespace Enterprise.MasterFiles.Business;

class INOrgCusCodePremiseAddressValidator : CountrySpecificOrgCusCodePremiseAddressValidator
{
	protected override bool IsPremiseAddressAllowedWithoutBeingRequired(string codeType)
	{
		switch (codeType)
		{
			case IndiaOrgCusCodeInfo.OrgCusCodes.BSN:
			case IndiaOrgCusCodeInfo.OrgCusCodes.ADC:
				return true;
			default:
				return false;
		}
	}

	public override bool IsPremiseAddressRequired(string codeType) => false;
}
