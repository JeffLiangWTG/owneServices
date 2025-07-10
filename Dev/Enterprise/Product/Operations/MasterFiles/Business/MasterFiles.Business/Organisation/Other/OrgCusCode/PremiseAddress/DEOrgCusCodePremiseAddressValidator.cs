namespace Enterprise.MasterFiles.Business
{
	class DEOrgCusCodePremiseAddressValidator : CountrySpecificOrgCusCodePremiseAddressValidator
	{
		protected override bool IsPremiseAddressAllowedWithoutBeingRequired(string codeType)
		{
			switch (codeType)
			{
				case GermanyOrgCusCodeInfo.OrgCusCodes.CWC:
				case GermanyOrgCusCodeInfo.OrgCusCodes.TaxOffice:
				case GermanyOrgCusCodeInfo.OrgCusCodes.DakosyBerthCode:
				case GermanyOrgCusCodeInfo.OrgCusCodes.DakosyParticipantCode:
					return true;
				default:
					return false;
			}
		}

		public override bool IsPremiseAddressRequired(string codeType)
		{
			switch (codeType)
			{
				case GermanyOrgCusCodeInfo.OrgCusCodes.EoriBranchSuffix:
				case GermanyOrgCusCodeInfo.OrgCusCodes.EMCSWarehouseParticipantIdentificationNumber:
				case GermanyOrgCusCodeInfo.OrgCusCodes.ATLASParticipantIdentificationNumber:
					return true;
				default:
					return false;
			}
		}
	}
}
