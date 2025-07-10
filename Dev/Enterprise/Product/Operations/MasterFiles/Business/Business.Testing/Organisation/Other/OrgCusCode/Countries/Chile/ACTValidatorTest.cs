using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class ACTValidatorTest : TestCaseWithFactory
	{
		string CodeType => ChileOrgCusCodeInfo.OrgCusCodes.ACT;
		string CountryCode => Core.Constants.CountryCodes.Chile;

		public void TestACTCodePatternValidation()
		{
			OrgCusCodeValidatorTestHelper.AssertValidation(CountryCode, ChileOrgCusCodeInfo.OrgCusCodes.ACT, codesWithInvalidPattern, validCodes, InvalidPatternMessage, CodeType);
		}

		string InvalidPatternMessage => @"The ACT registration code pattern is invalid.

Valid patterns are:

nnnnnn
nnnnnn,nnnnnn
nnnnnn,nnnnnn,nnnnnn
nnnnnn,nnnnnn,nnnnnn,nnnnnn

Where 'n' is a digit from 0 to 9.";

		readonly ZString[] validCodes = new ZString[]
		{
			"123456",
			"123456,789012",
			"123456,789012,345467",
			"123456,789012,345467,890123"
		};

		readonly ZString[] codesWithInvalidPattern = new ZString[]
		{
			"12345X",
			"XXXXXX",
			"1234567890121",
			"123456-789012",
			"123456.789012.345467",
			"123456 789012 345467 890123",
			"123456,",
			"123456,123456,",
			"123456,123456,123456,",
			"123456,123456,123456,123456,",
			"1234567,123456,123456,123456",
			"123456,123456,1234567,123456",
			"123456,123456,123456,1234567",
			"123",
			"123456,1236",
			"123456,123456,1236",
			"123456,123456,123456,12347",
		};
	}
}
