using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(ZAOrgCusCodePremiseAddressValidator))]
	sealed class ZAOrgCusCodePremiseAddressValidatorTest : CountrySpecificOrgCusCodePremiseAddressValidatorTest<ZAOrgCusCodePremiseAddressValidator>
	{
		protected override string[] OrgCusCodeTypesForPremisesAddressRequired => new[] { OrgCusCode.CodeTypes.VGMRegistrationNumber };

		protected override string[] CountryCodes => new[] { Core.Constants.CountryCodes.SouthAfrica };
	}
}
