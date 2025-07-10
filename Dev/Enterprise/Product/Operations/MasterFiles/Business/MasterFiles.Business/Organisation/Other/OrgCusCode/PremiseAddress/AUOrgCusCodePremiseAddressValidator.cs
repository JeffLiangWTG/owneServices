namespace Enterprise.MasterFiles.Business
{
	class AUOrgCusCodePremiseAddressValidator : CountrySpecificOrgCusCodePremiseAddressValidator
	{
		public override bool IsPremiseAddressRequired(string codeType)
		{
			switch (codeType)
			{
				case OrgCusCode.AUQuarantineCodeTypes.EXDOCEstablishmentNumber:
				case OrgCusCode.AustraliaCodeTypes.ApprovedArrangementNumber:
					return true;
				default:
					return false;
			}
		}

		protected override bool IsPremiseAddressAllowedWithoutBeingRequired(string codeType)
		{
			switch (codeType)
			{
				case OrgCusCode.CodeTypes.CustomsClientID:
					return true;
				default:
					return false;
			}
		}
	}
}
