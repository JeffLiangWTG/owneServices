using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class RFGValidatorTest : TestCaseWithFactory
	{
		string CodeType => MexicoOrgCusCodeInfo.OrgCusCodes.RFG;
		string CountryCode => Core.Constants.CountryCodes.Mexico;

		public void TestRFGCodeLengthValidation()
		{
			OrgCusCodeValidatorTestHelper.AssertValidation(CountryCode, CodeType, codesWithInvalidLength, validCodes, InvalidLengthMessage, isOnlyWarning: true);
		}

		public void TestRFGCodePatternValidation()
		{
			OrgCusCodeValidatorTestHelper.AssertValidation(CountryCode, CodeType, codesWithInvalidPattern, validCodes, InvalidPatternMessage, isOnlyWarning: true);
		}

		readonly ZString[] codesWithInvalidLength = new ZString[] { "12345678901", "12345678901234", "123456789" };
		readonly ZString[] codesWithInvalidPattern = new ZString[] { "XAXX010101001", "R1NA85&821l78", "OLM14GG144Z7A", "AME880912I&GF", "PES1HY212&G73", "1AN70081730X0", "1AN700G173R5X", "aab123456GHK1", "AA&123456yyyD" };
		readonly ZString[] validCodes = new ZString[] { "XAXX010101000", "XEXX010101000" };

		string InvalidLengthMessage
		{
			get { return "The RFG registration code needs to be 13 characters in length."; }
		}

		string InvalidPatternMessage
		{
			get
			{
				return @"The RFG registration code pattern is invalid.

Valid patterns are:
	XAXX010101000 (for General Public Organizations)
	XEXX010101000 (for Foreign Country Organizations)

Please verify that you are entering a correct registration code.";
			}
		}
	}
}
