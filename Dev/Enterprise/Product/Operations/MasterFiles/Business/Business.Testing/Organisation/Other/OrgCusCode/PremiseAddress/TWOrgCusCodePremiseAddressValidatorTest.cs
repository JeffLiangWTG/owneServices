using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(TWOrgCusCodePremiseAddressValidator))]
	sealed class TWOrgCusCodePremiseAddressValidatorTest : CountrySpecificOrgCusCodePremiseAddressValidatorTest<TWOrgCusCodePremiseAddressValidator>
	{
		protected override string[] OrgCusCodeTypesForPremisesAddressRequired => new[]
		{
			OrgCusCode.TaiwanCodeTypes.EPZ,
			OrgCusCode.TaiwanCodeTypes.FTZ,
			OrgCusCode.TaiwanCodeTypes.CBF,
			OrgCusCode.TaiwanCodeTypes.FactoryRegistrationNumber,
			OrgCusCode.CodeTypes.WarehouseControlledPremisesID,
			OrgCusCode.CodeTypes.ControlledPremisesID,
			OrgCusCode.CodeTypes.FDAEstablishmentIdentifier,
			OrgCusCode.TaiwanCodeTypes.AgriculturalTechnologyPark,
			OrgCusCode.TaiwanCodeTypes.SciencePark,
			OrgCusCode.CodeTypes.CarrierCode
		};

		protected override string[] CountryCodes => new[] { Core.Constants.CountryCodes.Taiwan };
	}
}
