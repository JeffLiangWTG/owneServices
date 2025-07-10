using System.Linq;
using CargoWise.Integration;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(CNOrgCusCodePremiseAddressValidator))]
	sealed class CNOrgCusCodePremiseAddressValidatorTest : CountrySpecificOrgCusCodePremiseAddressValidatorTest<CNOrgCusCodePremiseAddressValidator>
	{
		protected override string[] OrgCusCodeTypesForPremisesAddressAllowed => new Customs.CN.EnterpriseQualificationList().Cast<ICodeDescription>().Select(x => x.Code).ToArray();

		protected override string[] CountryCodes => new[] { Core.Constants.CountryCodes.China };
	}
}
