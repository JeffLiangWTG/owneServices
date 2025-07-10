namespace Enterprise.MasterFiles.Business
{
	class CAOrgCusCodePremiseAddressValidator : CountrySpecificOrgCusCodePremiseAddressValidator
	{
		public override bool IsPremiseAddressRequired(string codeType)
		{
			switch (codeType)
			{
				case OrgCusCode.CACodeTypes.CustomsOfficeCode:
				case OrgCusCode.CACodeTypes.CSAReferenceID:
				case OrgCusCode.CodeTypes.ControlledPremisesID:
					return true;
				default:
					return false;
			}
		}
	}
}
