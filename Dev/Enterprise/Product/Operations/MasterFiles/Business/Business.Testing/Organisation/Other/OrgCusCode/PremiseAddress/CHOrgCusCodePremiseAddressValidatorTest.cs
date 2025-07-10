using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(CHOrgCusCodePremiseAddressValidator))]
	sealed class CHOrgCusCodePremiseAddressValidatorTest : CountrySpecificOrgCusCodePremiseAddressValidatorTest<CHOrgCusCodePremiseAddressValidator>
	{
		protected override string[] OrgCusCodeTypesForPremisesAddressAllowed => new[] { OrgCusCode.SwissCodeTypes.ASN };

		protected override string[] CountryCodes => new[] { Core.Constants.CountryCodes.Switzerland };
	}
}
