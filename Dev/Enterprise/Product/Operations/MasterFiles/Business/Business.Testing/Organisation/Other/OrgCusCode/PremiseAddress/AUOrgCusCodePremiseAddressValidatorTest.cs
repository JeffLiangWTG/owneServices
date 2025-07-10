using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(AUOrgCusCodePremiseAddressValidator))]
	sealed class AUOrgCusCodePremiseAddressValidatorTest : CountrySpecificOrgCusCodePremiseAddressValidatorTest<AUOrgCusCodePremiseAddressValidator>
	{
		protected override string[] OrgCusCodeTypesForPremisesAddressRequired => new[] { OrgCusCode.AUQuarantineCodeTypes.EXDOCEstablishmentNumber };

		protected override string[] OrgCusCodeTypesForPremisesAddressAllowed => new[] { OrgCusCode.CodeTypes.CustomsClientID };

		protected override string[] CountryCodes => new[] { Core.Constants.CountryCodes.Australia };
	}
}
