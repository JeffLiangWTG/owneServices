namespace Enterprise.MasterFiles.Business
{
	class TROrgCusCodePremiseAddressValidator : CountrySpecificOrgCusCodePremiseAddressValidator
	{
		protected override bool IsPremiseAddressAllowedWithoutBeingRequired(string codeType) => codeType == TurkeyOrgCusCodeInfo.OrgCusCodes.PEC;
		public override bool IsPremiseAddressRequired(string codeType)
		{
			switch (codeType)
			{
				case OrgCusCode.CodeTypes.WarehouseControlledPremisesID:
				case OrgCusCode.CodeTypes.TerminalControlledPremisesID:
					return true;
				default:
					return false;
			}
		}
	}
}
