using Enterprise.MasterFiles.Business.CountryCompliance;

namespace Enterprise.MasterFiles.Business
{
	class KROrgCusCodePremiseAddressValidator : CountrySpecificOrgCusCodePremiseAddressValidator
	{
		public override bool IsPremiseAddressRequired(string codeType)
		{
			switch (codeType)
			{
				case KoreaSouthComplianceInfo.CodeTypes.IndustrialParkCode:
				case KoreaSouthComplianceInfo.CodeTypes.RoadNameCode:
				case KoreaSouthComplianceInfo.CodeTypes.BuildingNumber:
				case OrgCusCode.CodeTypes.ControlledPremisesID:
				case KoreaSouthComplianceInfo.CodeTypes.OfficeID:
					return true;
				default:
					return false;
			}
		}
	}
}
