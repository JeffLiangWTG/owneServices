using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(NLOrgCusCodePremiseAddressValidator))]
	sealed class NLOrgCusCodePremiseAddressValidatorTest : CountrySpecificOrgCusCodePremiseAddressValidatorTest<NLOrgCusCodePremiseAddressValidator>
	{
		protected override string[] OrgCusCodeTypesForPremisesAddressAllowed => new string[] { OrgCusCode.NetherlandsCodeTypes.CargonautRegistationCode };

		protected override string[] CountryCodes => new[] { Core.Constants.CountryCodes.Netherlands };
	}
}
