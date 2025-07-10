using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(NZOrgCusCodePremiseAddressValidator))]
	sealed class NZOrgCusCodePremiseAddressValidatorTest : CountrySpecificOrgCusCodePremiseAddressValidatorTest<NZOrgCusCodePremiseAddressValidator>
	{
		protected override string[] OrgCusCodeTypesForPremisesAddressAllowed => new[] { OrgCusCode.NZCodeTypes.ApprovedTransitionalFacility };

		protected override string[] CountryCodes => new[] { Core.Constants.CountryCodes.NewZealand };
	}
}
