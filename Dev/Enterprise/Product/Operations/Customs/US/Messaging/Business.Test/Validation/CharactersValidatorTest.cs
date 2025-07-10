using CargoWise.Common;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.US.Messaging.Business.Testing
{
	sealed class CharactersValidatorTest : TestCaseWithFactory
	{
		public void TestValidateAMSCharacters()
		{
			var message = @"Code : US Customs only accepts standard English alphabetic characters; invalid characters, including some punctuation and foreign characters, will be replaced with an asterisk '*'";
			var obj = Factory.New<DummyBusinessObject>();
			obj.Z0_Code = "Î  ¢";
			AMSCharactersValidator.ValidateCharacters(obj.Z0_CodeInfo, true);
			AssertHasWarnings(message, obj.Z0_CodeInfo);
			AMSCharactersValidator.ValidateCharacters(obj.Z0_CodeInfo, false);
			AssertHasMessageErrors(message, obj.Z0_CodeInfo);
			obj.Z0_Code = "A  $";
			AMSCharactersValidator.ValidateCharacters(obj.Z0_CodeInfo, false);
			AssertNoWarnings(obj.Z0_CodeInfo);
			AMSCharactersValidator.ValidateCharacters(obj.Z0_CodeInfo, true);
			AssertNoMessageErrors(obj.Z0_CodeInfo);

			if (ErrorReporter.LastKeyReported == "Validation:Z0_Code")
			{
				ErrorReporter.Clear();
			}
		}

		public void TestValidateABICharacters()
		{
			var message = @"Code : US Customs only accepts standard English alphabetic characters; invalid characters, including some punctuation and foreign characters, will be replaced with a question mark '?'";
			var obj = Factory.New<DummyBusinessObject>();
			obj.Z0_Code = "Î  Ï";
			ABICharactersValidator.ValidateCharacters(obj.Z0_CodeInfo, true);
			AssertHasWarnings(message, obj.Z0_CodeInfo);
			ABICharactersValidator.ValidateCharacters(obj.Z0_CodeInfo, false);
			AssertHasMessageErrors(message, obj.Z0_CodeInfo);
			obj.Z0_Code = "A  *";
			ABICharactersValidator.ValidateCharacters(obj.Z0_CodeInfo, false);
			AssertNoWarnings(obj.Z0_CodeInfo);
			ABICharactersValidator.ValidateCharacters(obj.Z0_CodeInfo, true);
			AssertNoMessageErrors(obj.Z0_CodeInfo);

			if (ErrorReporter.LastKeyReported == "Validation:Z0_Code")
			{
				ErrorReporter.Clear();
			}
		}
	}
}
