namespace Enterprise.MasterFiles.Business
{
	class ESOrgCusCodePremiseAddressValidator : CountrySpecificOrgCusCodePremiseAddressValidator
	{
		public override bool IsPremiseAddressRequired(string codeType)
		{
			switch (codeType)
			{
				case OrgCusCode.CodeTypes.ControlledPremisesID:
					return true;
				default:
					return false;
			}
		}
	}
}
