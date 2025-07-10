using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.Business.Testing
{
	sealed class UserEnterableTokenPinValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckPinMandatoryValidation()
		{
			var tokenPin = new UserEnterableTokenPin();
			tokenPin.Pin = "123";
			AssertNoErrorContaining(tokenPin.PinInfo, MandatoryValidation.MustBeEntered);

			tokenPin.Pin = "";
			AssertHasErrorContaining(tokenPin.PinInfo, MandatoryValidation.MustBeEntered);
		}
	}
}
