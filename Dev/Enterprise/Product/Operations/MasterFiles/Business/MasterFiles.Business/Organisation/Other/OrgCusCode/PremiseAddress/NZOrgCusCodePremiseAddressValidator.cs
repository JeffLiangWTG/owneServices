namespace Enterprise.MasterFiles.Business
{
	class NZOrgCusCodePremiseAddressValidator : CountrySpecificOrgCusCodePremiseAddressValidator
	{
		protected override bool IsPremiseAddressAllowedWithoutBeingRequired(string codeType)
		{
			switch (codeType)
			{
				case OrgCusCode.NZCodeTypes.ApprovedTransitionalFacility:
					return true;
				default:
					return false;
			}
		}

		public override bool IsPremiseAddressRequired(string codeType) => false;
	}
}
