using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.Business.Testing
{
	class USOrganisationRequirementLookupsTest : TestCaseWithFactory
	{
		public void TestGetUSPPIDocAddressRegNumTypes()
		{
			var usRegion = new[] { Core.Constants.CountryCodes.UnitedStates, Core.Constants.CountryCodes.PuertoRico, Core.Constants.CountryCodes.VirginIslands };

			foreach (var region in usRegion)
			{
				var docAddress = Factory.New<JobDocAddress>();
				docAddress.E2_RN_NKCountryCode = region;
				var list = new USOrganisationRequirementLookups(Factory).GetUSPPIDocAddressRegNumTypes(docAddress.Lookups);
				AssertEquals(2, list.Count);
				AssertEquals(OrgCusCode.USACodeTypes.EmployerIdentificationNumber, list[0].Code);
				AssertEquals(OrgCusCode.CodeTypes.PassportID, list[1].Code);
			}

			var otherRegion = new[] { Core.Constants.CountryCodes.Australia, Core.Constants.CountryCodes.Canada, Core.Constants.CountryCodes.SouthAfrica };
			foreach (var region in otherRegion)
			{
				var docAddress = Factory.New<JobDocAddress>();
				docAddress.E2_RN_NKCountryCode = region;
				var list = new USOrganisationRequirementLookups(Factory).GetUSPPIDocAddressRegNumTypes(docAddress.Lookups);
				AssertEquals(4, list.Count);
				AssertEquals(true, list.ContainsCode(OrgCusCode.USACodeTypes.EmployerIdentificationNumber));
				AssertEquals(true, list.ContainsCode(OrgCusCode.CodeTypes.PassportID));
				AssertEquals(true, list.ContainsCode(OrgCusCode.USACodeTypes.ForeignRegistrationNumber));
				AssertEquals(true, list.ContainsCode(OrgCusCode.CodeTypes.DataUniversalNumberingSystem));
			}
		}
	}
}
