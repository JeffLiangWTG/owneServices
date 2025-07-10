using Enterprise.MasterFiles.Business.CountryCompliance;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(KROrgCusCodePremiseAddressValidator))]
	sealed class KROrgCusCodePremiseAddressValidatorTest : CountrySpecificOrgCusCodePremiseAddressValidatorTest<KROrgCusCodePremiseAddressValidator>
	{
		protected override string[] OrgCusCodeTypesForPremisesAddressRequired => new[] { KoreaSouthComplianceInfo.CodeTypes.IndustrialParkCode, KoreaSouthComplianceInfo.CodeTypes.RoadNameCode, KoreaSouthComplianceInfo.CodeTypes.BuildingNumber, OrgCusCode.CodeTypes.ControlledPremisesID, KoreaSouthComplianceInfo.CodeTypes.OfficeID };

		protected override string[] CountryCodes => new[] { Core.Constants.CountryCodes.KoreaSouth };
	}
}
