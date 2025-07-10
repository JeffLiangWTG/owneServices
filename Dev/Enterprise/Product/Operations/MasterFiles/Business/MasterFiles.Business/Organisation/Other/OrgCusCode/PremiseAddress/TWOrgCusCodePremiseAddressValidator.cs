namespace Enterprise.MasterFiles.Business
{
	class TWOrgCusCodePremiseAddressValidator : CountrySpecificOrgCusCodePremiseAddressValidator
	{
		public override bool IsPremiseAddressRequired(string codeType)
		{
			switch (codeType)
			{
				case OrgCusCode.TaiwanCodeTypes.EPZ:
				case OrgCusCode.TaiwanCodeTypes.FTZ:
				case OrgCusCode.TaiwanCodeTypes.CBF:
				case OrgCusCode.TaiwanCodeTypes.FactoryRegistrationNumber:
				case OrgCusCode.CodeTypes.WarehouseControlledPremisesID:
				case OrgCusCode.CodeTypes.ControlledPremisesID:
				case OrgCusCode.CodeTypes.FDAEstablishmentIdentifier:
				case OrgCusCode.TaiwanCodeTypes.AgriculturalTechnologyPark:
				case OrgCusCode.TaiwanCodeTypes.SciencePark:
				case OrgCusCode.CodeTypes.CarrierCode:
					return true;
				default:
					return false;
			}
		}
	}
}
