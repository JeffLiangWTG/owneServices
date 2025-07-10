using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(CAOrgCusCodePremiseAddressValidator))]
	sealed class CAOrgCusCodePremiseAddressValidatorTest : CountrySpecificOrgCusCodePremiseAddressValidatorTest<CAOrgCusCodePremiseAddressValidator>
	{
		protected override string[] OrgCusCodeTypesForPremisesAddressRequired => new[] { OrgCusCode.CACodeTypes.CustomsOfficeCode, OrgCusCode.CACodeTypes.CSAReferenceID, OrgCusCode.CodeTypes.ControlledPremisesID };

		protected override string[] CountryCodes => new[] { Core.Constants.CountryCodes.Canada };
	}
}
