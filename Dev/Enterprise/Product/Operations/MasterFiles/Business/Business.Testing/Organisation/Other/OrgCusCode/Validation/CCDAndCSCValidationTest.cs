using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class CCDAndCSCValidationTest : TestCase
	{
		public void TestValidCCD()
		{
			CCDAndCSCValidation cCDValidator = new CCDAndCSCValidation("0059781A", true);
			ZString result = cCDValidator.CheckValid();
			AssertEquals("No error as it is a valid CCD", "", result);
		}

		public void TestValidCCDWithLowerCaseCheckDigit()
		{
			CCDAndCSCValidation cCDValidator = new CCDAndCSCValidation("0059781a", true);
			ZString result = cCDValidator.CheckValid();
			AssertEquals("No error as it is a valid CCD", "", result);
		}

		public void TestInvalidCCDDueToCheckDigit()
		{
			CCDAndCSCValidation cCDValidator = new CCDAndCSCValidation("0059781B", true);
			ZString result = cCDValidator.CheckValid();
			AssertEquals("Error as Check Digit is invalid", CCDAndCSCValidation.InvalidCheckDigitCCD("B", "A") + " " + CCDAndCSCValidation.ModifyProcedureCCD, result);
		}

		public void TestInvalidCCDDueToCharactersAtStart()
		{
			CCDAndCSCValidation cCDValidator = new CCDAndCSCValidation("F059781A", true);
			ZString result = cCDValidator.CheckValid();
			AssertEquals("Error as character at the start", "The Customs Client Code needs to have a format of 7 or less numerics and a check digit. " + CCDAndCSCValidation.ModifyProcedureCCD, result);
		}

		public void TestInvalidCCDDueToNumericAtEnd()
		{
			CCDAndCSCValidation cCDValidator = new CCDAndCSCValidation("10597813", true);
			ZString result = cCDValidator.CheckValid();
			AssertEquals("Error as character at the start", CCDAndCSCValidation.InvalidCheckDigitCCD("3", "H") + " " + CCDAndCSCValidation.ModifyProcedureCCD, result);
		}

		public void TestInvalidCCDDueToIncorrectLength()
		{
			CCDAndCSCValidation cCDValidator = new CCDAndCSCValidation("1059782131231231", true);
			ZString result = cCDValidator.CheckValid();
			AssertEquals("Error as it is too long enough", CCDAndCSCValidation.InvalidCCDLength, result);
		}

		public void TestValidCSC()
		{
			CCDAndCSCValidation cSCValidator = new CCDAndCSCValidation("0074873R");
			ZString result = cSCValidator.CheckValid();
			AssertEquals("No error as it is a valid CSC", "", result);
		}

		public void TestValidCSCWithLowerCaseCheckDigit()
		{
			CCDAndCSCValidation cSCValidator = new CCDAndCSCValidation("0074873r");
			ZString result = cSCValidator.CheckValid();
			AssertEquals("No error as it is a valid CSC", "", result);
		}

		public void TestInvalidCSCDueToCheckDigit()
		{
			CCDAndCSCValidation cSCValidator = new CCDAndCSCValidation("0074873Q");
			ZString result = cSCValidator.CheckValid();
			AssertEquals("Error as Check Digit is invalid", CCDAndCSCValidation.InvalidCheckDigitCSC("Q", "R") + " " + CCDAndCSCValidation.ModifyProcedureCSC, result);
		}

		public void TestInvalidCSCDueToCharactersAtStart()
		{
			CCDAndCSCValidation cSCValidator = new CCDAndCSCValidation("F074873R");
			ZString result = cSCValidator.CheckValid();
			AssertEquals("Error as character at the start", "The Customs Supplier Code needs to have a format of 7 or less numerics and a check digit. " + CCDAndCSCValidation.ModifyProcedureCSC, result);
		}

		public void TestInvalidCSCDueToNumericAtEnd()
		{
			CCDAndCSCValidation cSCValidator = new CCDAndCSCValidation("00748733");
			ZString result = cSCValidator.CheckValid();
			AssertEquals("Error as character at the start", CCDAndCSCValidation.InvalidCheckDigitCSC("3", "R") + " " + CCDAndCSCValidation.ModifyProcedureCSC, result);
		}

		public void TestInvalidCSCDueToIncorrectLength()
		{
			CCDAndCSCValidation cSCValidator = new CCDAndCSCValidation("1059782131231231");
			ZString result = cSCValidator.CheckValid();
			AssertEquals("Error as it is too long enough", CCDAndCSCValidation.InvalidCSCLength, result);
		}
	}
}
