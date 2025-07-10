using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(TROrgCusCodePremiseAddressValidator))]
	sealed class TROrgCusCodePremiseAddressValidatorTest : CountrySpecificOrgCusCodePremiseAddressValidatorTest<TROrgCusCodePremiseAddressValidator>
	{
		protected override string[] OrgCusCodeTypesForPremisesAddressRequired => new[] { OrgCusCode.CodeTypes.WarehouseControlledPremisesID, OrgCusCode.CodeTypes.TerminalControlledPremisesID };

		protected override string[] OrgCusCodeTypesForPremisesAddressAllowed => new[] { TurkeyOrgCusCodeInfo.OrgCusCodes.PEC };

		protected override string[] CountryCodes => new[] { Core.Constants.CountryCodes.Turkey };
	}
}
