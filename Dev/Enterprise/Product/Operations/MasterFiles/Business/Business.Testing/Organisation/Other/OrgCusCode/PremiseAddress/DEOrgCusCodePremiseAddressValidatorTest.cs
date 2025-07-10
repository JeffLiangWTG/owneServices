using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(DEOrgCusCodePremiseAddressValidator))]
	sealed class DEOrgCusCodePremiseAddressValidatorTest : CountrySpecificOrgCusCodePremiseAddressValidatorTest<DEOrgCusCodePremiseAddressValidator>
	{
		protected override string[] OrgCusCodeTypesForPremisesAddressRequired => new[]
		{
			GermanyOrgCusCodeInfo.OrgCusCodes.EoriBranchSuffix,
			GermanyOrgCusCodeInfo.OrgCusCodes.EMCSWarehouseParticipantIdentificationNumber,
			GermanyOrgCusCodeInfo.OrgCusCodes.ATLASParticipantIdentificationNumber
		};

		protected override string[] OrgCusCodeTypesForPremisesAddressAllowed => new[]
		{
			GermanyOrgCusCodeInfo.OrgCusCodes.CWC,
			GermanyOrgCusCodeInfo.OrgCusCodes.TaxOffice,
			GermanyOrgCusCodeInfo.OrgCusCodes.DakosyBerthCode,
			GermanyOrgCusCodeInfo.OrgCusCodes.DakosyParticipantCode,
		};

		protected override string[] OrgCusCodeTypesForNotAllowed => new[] { GermanyOrgCusCodeInfo.OrgCusCodes.AccreditedExporter };

		protected override string[] CountryCodes => new[] { Core.Constants.CountryCodes.Germany };
	}
}
