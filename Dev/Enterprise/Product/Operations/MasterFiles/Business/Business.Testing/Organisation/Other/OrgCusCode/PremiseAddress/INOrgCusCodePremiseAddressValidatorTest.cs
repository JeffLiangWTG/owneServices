using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing;

[TestedType(typeof(INOrgCusCodePremiseAddressValidator))]
sealed class INOrgCusCodePremiseAddressValidatorTest : CountrySpecificOrgCusCodePremiseAddressValidatorTest<INOrgCusCodePremiseAddressValidator>
{
	protected override string[] OrgCusCodeTypesForPremisesAddressAllowed => new[] { IndiaOrgCusCodeInfo.OrgCusCodes.BSN, IndiaOrgCusCodeInfo.OrgCusCodes.ADC };

	protected override string[] CountryCodes => new[] { Core.Constants.CountryCodes.India };
}

