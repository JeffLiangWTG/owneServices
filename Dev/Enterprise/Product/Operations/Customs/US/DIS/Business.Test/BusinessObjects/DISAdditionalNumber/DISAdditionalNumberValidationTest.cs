using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.US.DIS.Business.Testing
{
	sealed class DISAdditionalNumberValidationTest : TestCaseWithFactory
	{
		public void TestCheckNumber()
		{
			var number = new DISAdditionalNumber(Factory);
			number.Number = ZString.Empty;
			AssertHasMessageErrorContaining(number.NumberInfo, MandatoryValidation.YouHaveNotEntered);
			number.Number = "78423";
			AssertNoMessageErrorContaining(number.NumberInfo, MandatoryValidation.YouHaveNotEntered);
		}
	}
}
