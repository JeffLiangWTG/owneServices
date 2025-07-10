namespace Enterprise.MasterFiles.Business
{
	class JPOrgCusCodePremiseAddressValidator : CountrySpecificOrgCusCodePremiseAddressValidator
	{
		public override bool IsPremiseAddressRequired(string codeType)
		{
			switch (codeType)
			{
				case OrgCusCode.JapanCodeTypes.JAS:
				case OrgCusCode.JapanCodeTypes.LPC:
				case OrgCusCode.JapanCodeTypes.CIE:
				case OrgCusCode.JapanCodeTypes.FSB:
				case OrgCusCode.JapanCodeTypes.NUC:
				case OrgCusCode.JapanCodeTypes.AAL:
				case OrgCusCode.CodeTypes.ControlledPremisesID:
					return true;

				default:
					return false;
			}
		}
	}
}
