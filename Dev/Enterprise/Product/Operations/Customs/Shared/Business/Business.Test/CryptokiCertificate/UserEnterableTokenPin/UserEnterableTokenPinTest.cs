using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(UserEnterableTokenPin))]
	sealed class UserEnterableTokenPinTest : NonPersistentBusinessObjectTestCase
	{
		public void TestPinMaxLength()
		{
			var token = new UserEnterableTokenPin();
			AssertEquals("Pin MaxLength", 30, token.PinInfo.MaxLength);
		}

		public void TestValidationType()
		{
			var token = new UserEnterableTokenPin();
			AssertType<UserEnterableTokenPinValidation>("Validation Type", token.Validation);
		}
	}
}
