namespace Enterprise.MasterFiles.Business
{
	class USOrgCusCodePremiseAddressValidator : CountrySpecificOrgCusCodePremiseAddressValidator
	{
		protected override bool IsPremiseAddressAllowedWithoutBeingRequired(string codeType)
		{
			switch (codeType)
			{
				case OrgCusCode.USACodeTypes.DepartmentOfDefenseActivityAddressCode:
				case OrgCusCode.USACodeTypes.CarrierPrefixCode:
					return true;
				default:
					return false;
			}
		}

		public override bool IsPremiseAddressRequired(string codeType)
		{
			switch (codeType)
			{
				case OrgCusCode.USACodeTypes.ManufacturerID:
				case OrgCusCode.USACodeTypes.FoodFacilityRegistrationNumber:
				case OrgCusCode.USACodeTypes.ForeignProducerIdentifier:
				case OrgCusCode.USACodeTypes.ForeignProducerIdentifierBeer:
				case OrgCusCode.USACodeTypes.ForeignProducerIdentifierSpirits:
				case OrgCusCode.USACodeTypes.ForeignProducerIdentifierWine:
				case OrgCusCode.USACodeTypes.FIRMSCode:
				case OrgCusCode.USACodeTypes.ABIRoutingCode:
				case OrgCusCode.USACodeTypes.TTIRegistrationNumber:
				case OrgCusCode.CodeTypes.FDAEstablishmentIdentifier:
				case OrgCusCode.USACodeTypes.DataUniversalNumberingSystemPlus4:
				case OrgCusCode.PuertoRicoCodeTypes.ImpuestoSobreVentasyUso:
				case OrgCusCode.USACodeTypes.TireManufacturerCode:
				case OrgCusCode.USACodeTypes.GlazingManufacturerCode:
				case OrgCusCode.USACodeTypes.CPSCAccreditedLabId:
				case OrgCusCode.USACodeTypes.CertifiedCargoScreening:
				case OrgCusCode.CodeTypes.CommercialAndGovernmentEntity:
				case OrgCusCode.CodeTypes.PortSystemNumber:
				case OrgCusCode.CodeTypes.PortServiceReference:
				case OrgCusCode.USACodeTypes.LegalEntityIdentifier:
				case OrgCusCode.USACodeTypes.GlobalLocationNumber:
				case OrgCusCode.USACodeTypes.AirAMSOriginatorCode:
					return true;
				default:
					return false;
			}
		}
	}
}

