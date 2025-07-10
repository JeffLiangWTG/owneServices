namespace Enterprise.MasterFiles.Business
{
	class CHOrgCusCodePremiseAddressValidator : CountrySpecificOrgCusCodePremiseAddressValidator
	{
		protected override bool IsPremiseAddressAllowedWithoutBeingRequired(string codeType)
		{
			switch (codeType)
			{
				case OrgCusCode.SwissCodeTypes.ASN:
					return true;
				default:
					return false;
			}
		}

		public override bool IsPremiseAddressRequired(string codeType) => false;
	}
}
