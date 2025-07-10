using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class GlbStaffCredentialLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestInsuranceAgents()
		{
			var helper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);
			helper.CreateCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.InsuranceAgent, "Insurance Agent");
			helper.CreateCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.InsuranceAgent, "ABC", ZDateTime.Today.AddYears(-1), ZDateTime.Today.AddYears(1));
			helper.CreateCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.InsuranceAgent, "DEF", ZDateTime.Today.AddYears(-1), ZDateTime.Today.AddYears(1));
			Factory.Save();

			var password = Factory.New<GlbStaffCredential>();
			var lookups = password.Lookups;

			AssertEquals(lookups.InsuranceAgents.CodesAsString, "ABC, DEF");
		}
	}
}
