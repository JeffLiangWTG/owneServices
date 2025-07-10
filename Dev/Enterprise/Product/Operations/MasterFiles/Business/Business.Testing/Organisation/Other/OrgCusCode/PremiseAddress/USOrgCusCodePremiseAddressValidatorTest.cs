using System.Linq;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(USOrgCusCodePremiseAddressValidator))]
	sealed class USOrgCusCodePremiseAddressValidatorTest : CountrySpecificOrgCusCodePremiseAddressValidatorTest<USOrgCusCodePremiseAddressValidator>
	{
		protected override string[] OrgCusCodeTypesForPremisesAddressRequired => new string[]
		{
			OrgCusCode.USACodeTypes.ManufacturerID,
			OrgCusCode.USACodeTypes.FoodFacilityRegistrationNumber,
			OrgCusCode.USACodeTypes.ForeignProducerIdentifier,
			OrgCusCode.USACodeTypes.ForeignProducerIdentifierBeer,
			OrgCusCode.USACodeTypes.ForeignProducerIdentifierSpirits,
			OrgCusCode.USACodeTypes.ForeignProducerIdentifierWine,
			OrgCusCode.USACodeTypes.FIRMSCode,
			OrgCusCode.USACodeTypes.ABIRoutingCode,
			OrgCusCode.USACodeTypes.TTIRegistrationNumber,
			OrgCusCode.CodeTypes.FDAEstablishmentIdentifier,
			OrgCusCode.USACodeTypes.DataUniversalNumberingSystemPlus4,
			OrgCusCode.PuertoRicoCodeTypes.ImpuestoSobreVentasyUso,
			OrgCusCode.USACodeTypes.TireManufacturerCode,
			OrgCusCode.USACodeTypes.GlazingManufacturerCode,
			OrgCusCode.USACodeTypes.CPSCAccreditedLabId,
			OrgCusCode.USACodeTypes.CertifiedCargoScreening,
			OrgCusCode.CodeTypes.CommercialAndGovernmentEntity,
			OrgCusCode.CodeTypes.PortSystemNumber,
			OrgCusCode.CodeTypes.PortServiceReference,
			OrgCusCode.USACodeTypes.LegalEntityIdentifier,
			OrgCusCode.USACodeTypes.GlobalLocationNumber,
			OrgCusCode.USACodeTypes.AirAMSOriginatorCode
		};

		protected override string[] OrgCusCodeTypesForPremisesAddressAllowed => new[] { OrgCusCode.USACodeTypes.DepartmentOfDefenseActivityAddressCode, OrgCusCode.USACodeTypes.CarrierPrefixCode };

		protected override string[] CountryCodes => Core.Constants.CountryCodes.UsaAndTerritoriesList.ToArray();
	}
}
