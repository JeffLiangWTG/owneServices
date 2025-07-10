using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(DKOrgCusCodePremiseAddressValidator))]
	sealed class DKOrgCusCodePremiseAddressValidatorTest : CountrySpecificOrgCusCodePremiseAddressValidatorTest<DKOrgCusCodePremiseAddressValidator>
	{
		protected override string[] OrgCusCodeTypesForPremisesAddressRequired => new[] { OrgCusCode.DenmarkCodeTypes.ProductionNumber };

		protected override string[] CountryCodes => new[] { Core.Constants.CountryCodes.Denmark };
	}
}
