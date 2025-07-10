using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(PLOrgCusCodePremiseAddressValidator))]
	sealed class PLOrgCusCodePremiseAddressValidatorTest : CountrySpecificOrgCusCodePremiseAddressValidatorTest<PLOrgCusCodePremiseAddressValidator>
	{
		protected override string[] OrgCusCodeTypesForPremisesAddressRequired => new[] { OrgCusCode.CodeTypes.WarehouseControlledPremisesID };

		protected override string[] CountryCodes => new[] { Core.Constants.CountryCodes.Poland };
	}
}
