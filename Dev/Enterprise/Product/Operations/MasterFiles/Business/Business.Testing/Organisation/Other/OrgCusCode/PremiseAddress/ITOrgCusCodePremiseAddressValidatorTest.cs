using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(ITOrgCusCodePremiseAddressValidator))]
	sealed class ITOrgCusCodePremiseAddressValidatorTest : CountrySpecificOrgCusCodePremiseAddressValidatorTest<ITOrgCusCodePremiseAddressValidator>
	{
		protected override string[] OrgCusCodeTypesForPremisesAddressAllowed => new[] { ItalyOrgCusCodeInfo.OrgCusCodes.CertifiedEmail };

		protected override string[] CountryCodes => new[] { Core.Constants.CountryCodes.Italy };
	}
}
