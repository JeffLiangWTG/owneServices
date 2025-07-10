using CargoWise.EntityFramework.Testing;

namespace Enterprise.Freight.DataTransfer.Universal.Testing
{
	sealed class CO2eUniversalCodeMapperTest : TestCaseWithFactory
	{
		public void TestCO2eUniversalCodeMapper()
		{
			var cO2eResponse = CO2eTestHelper.GetSampleCO2eResponseDataObject();

			var codeMapper = CO2eUniversalCodeMapper.Create(cO2eResponse, Factory);
			AssertNotNull("code mapper has been created", codeMapper);
			AssertEquals("code mapper organization", null, codeMapper.SourceOrganisation);
		}
	}
}
