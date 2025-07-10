using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(ESOrgCusCodePremiseAddressValidator))]
	sealed class ESOrgCusCodePremiseAddressValidatorTest : CountrySpecificOrgCusCodePremiseAddressValidatorTest<ESOrgCusCodePremiseAddressValidator>
	{
		protected override string[] OrgCusCodeTypesForPremisesAddressRequired => new[] { OrgCusCode.CodeTypes.ControlledPremisesID };

		protected override string[] CountryCodes => new[] { Core.Constants.CountryCodes.Spain };
	}
}
