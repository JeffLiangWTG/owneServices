using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(UKOrgCusCodePremiseAddressValidator))]
	sealed class UKOrgCusCodePremiseAddressValidatorTest : CountrySpecificOrgCusCodePremiseAddressValidatorTest<UKOrgCusCodePremiseAddressValidator>
	{
		protected override string[] OrgCusCodeTypesForPremisesAddressRequired => new[] { OrgCusCode.CodeTypes.VGMRegistrationNumber };

		protected override string[] OrgCusCodeTypesForPremisesAddressAllowed => new[] { OrgCusCode.UnitedKingdomCodeTypes.EoriBranchSuffix, OrgCusCode.UnitedKingdomCodeTypes.CTOShed, OrgCusCode.EuropeanUnionSharedCodeTypes.Eori };

		protected override string[] CountryCodes => new[] { Core.Constants.CountryCodes.UnitedKingdom };
	}
}
