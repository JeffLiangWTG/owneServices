using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(JPOrgCusCodePremiseAddressValidator))]
	sealed class JPOrgCusCodePremiseAddressValidatorTest : CountrySpecificOrgCusCodePremiseAddressValidatorTest<JPOrgCusCodePremiseAddressValidator>
	{
		protected override string[] CountryCodes => new[] { Core.Constants.CountryCodes.Japan };

		protected override string[] OrgCusCodeTypesForPremisesAddressRequired => new[] {
			OrgCusCode.JapanCodeTypes.JAS,
			OrgCusCode.JapanCodeTypes.LPC,
			OrgCusCode.JapanCodeTypes.CIE,
			OrgCusCode.JapanCodeTypes.FSB,
			OrgCusCode.JapanCodeTypes.NUC,
			OrgCusCode.CodeTypes.ControlledPremisesID };
	}
}
