using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class PedimentoNumberValidatorTest : TestCase
	{
		public void TestValidate()
		{
			AssertEquals(PedimentoNumberValidator.PedimentoNumberRightFormat, PedimentoNumberValidator.Validate(""));
			AssertEquals(PedimentoNumberValidator.PedimentoNumberRightFormat, PedimentoNumberValidator.Validate("061234"));
			AssertEquals(PedimentoNumberValidator.PedimentoNumberRightFormat, PedimentoNumberValidator.Validate("061234A21234567"));
			AssertEquals("", PedimentoNumberValidator.Validate("123456789012345"));
		}

		//YYPPBBBBDDDDDDD
		public void TestIsValidPedimentoNumber()
		{
			AssertEquals(false, PedimentoNumberValidator.IsValidPedimentoNumber(""));
			AssertEquals(false, PedimentoNumberValidator.IsValidPedimentoNumber("061234"));
			AssertEquals(false, PedimentoNumberValidator.IsValidPedimentoNumber("061234A21234567"));
			AssertEquals(true, PedimentoNumberValidator.IsValidPedimentoNumber("123456789012345"));
		}
	}
}
