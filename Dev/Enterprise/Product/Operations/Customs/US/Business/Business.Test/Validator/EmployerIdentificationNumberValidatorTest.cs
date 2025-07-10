using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class EmployerIdentificationNumberValidatorTest : TestCase
	{
		public void TestValidate()
		{
			AssertEquals(EmployerIdentificationNumberValidator.EINNumberRightFormat, EmployerIdentificationNumberValidator.Validate(""));
			AssertEquals(EmployerIdentificationNumberValidator.EINNumberRightFormat, EmployerIdentificationNumberValidator.Validate("123456"));
			AssertEquals("", EmployerIdentificationNumberValidator.Validate("12-3456789XY"));
		}

		//NN-NNNNNNNXX or NN-NNNNNNN
		public void TestIsValidEIN()
		{
			AssertEquals(false, EmployerIdentificationNumberValidator.IsValidEIN("12-1234567", true));
			AssertEquals(true, EmployerIdentificationNumberValidator.IsValidEIN("12-123456789", true));
			AssertEquals(false, EmployerIdentificationNumberValidator.IsValidEIN("", false));
			AssertEquals(false, EmployerIdentificationNumberValidator.IsValidEIN("061234", false));
			AssertEquals(false, EmployerIdentificationNumberValidator.IsValidEIN("12-34567890", false));
			AssertEquals(true, EmployerIdentificationNumberValidator.IsValidEIN("12-3456789XY", false));
			AssertEquals(true, EmployerIdentificationNumberValidator.IsValidEIN("12-3456789", false));
		}

		public void TestIsValidEINForMessage()
		{
			AssertEquals(false, EmployerIdentificationNumberValidator.IsValidEINForStandAloneInBondMessage(""));
			AssertEquals(false, EmployerIdentificationNumberValidator.IsValidEINForStandAloneInBondMessage("061234"));
			AssertEquals(true, EmployerIdentificationNumberValidator.IsValidEINForStandAloneInBondMessage("12-3456789**"));
			AssertEquals(true, EmployerIdentificationNumberValidator.IsValidEINForStandAloneInBondMessage("12-3456789"));
		}

		public void TestGetValidEINForInBondMessage()
		{
			AssertEquals("", EmployerIdentificationNumberValidator.GetValidEINForInBondMessage(""));
			AssertEquals("061234", EmployerIdentificationNumberValidator.GetValidEINForInBondMessage("061234"));
			AssertEquals("12-3456789**", EmployerIdentificationNumberValidator.GetValidEINForInBondMessage("12-3456789**"));
			AssertEquals("12-345678900", EmployerIdentificationNumberValidator.GetValidEINForInBondMessage("12-3456789  "));
			AssertEquals("91-013199000", EmployerIdentificationNumberValidator.GetValidEINForInBondMessage("91-0131990"));
		}
	}
}
