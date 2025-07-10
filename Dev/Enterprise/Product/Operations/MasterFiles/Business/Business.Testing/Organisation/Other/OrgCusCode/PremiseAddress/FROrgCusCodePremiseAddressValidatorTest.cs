using System.Linq;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(FROrgCusCodePremiseAddressValidator))]
	sealed class FROrgCusCodePremiseAddressValidatorTest : CountrySpecificOrgCusCodePremiseAddressValidatorTest<FROrgCusCodePremiseAddressValidator>
	{
		protected override string[] OrgCusCodeTypesForPremisesAddressAllowed => new string[] { OrgCusCode.FranceCodeTypes.CI5, OrgCusCode.FranceCodeTypes.SON, OrgCusCode.FranceCodeTypes.SOA, OrgCusCode.FranceCodeTypes.SOW, OrgCusCode.FranceCodeTypes.CIN };

		protected override string[] CountryCodes => Core.Constants.CountryCodes.FranceAndOverseasDepartmentsAndTerritories.Where(x => x != Core.Constants.CountryCodes.Reunion).ToArray();
	}
}
