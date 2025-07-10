namespace Enterprise.MasterFiles.Business
{
	class DKOrgCusCodePremiseAddressValidator : CountrySpecificOrgCusCodePremiseAddressValidator
	{
		public override bool IsPremiseAddressRequired(string codeType)
		{
			switch (codeType)
			{
				case OrgCusCode.DenmarkCodeTypes.ProductionNumber:
					return true;
				default:
					return false;
			}
		}
	}
}
