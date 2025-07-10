using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.DataTransfer.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.DataTransfer.Testing.Universal
{
	sealed class RefCusProcedureLoaderTest : TestCaseWithFactory
	{
		public void TestGetProcedure()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var currentCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			helper.CreateNewOrGetExistingDataGrouping(currentCountry);
			var procedure1 = helper.CreateRefCusProcedure(currentCountry, ZString.Empty, "AA", "BB", ZString.Empty, "description", "EXP");

			var procedureLoader = new RefCusProcedureLoader(Factory);

			var procedureLoadedNull1 = procedureLoader.GetProcedure(ZString.Empty, "55BB", currentCountry);
			AssertNull(procedureLoadedNull1);

			var procedureLoaded1 = procedureLoader.GetProcedure("AA", "55BB", currentCountry);
			AssertEquals(procedure1.PK, procedureLoaded1.PK);
		}
	}
}
