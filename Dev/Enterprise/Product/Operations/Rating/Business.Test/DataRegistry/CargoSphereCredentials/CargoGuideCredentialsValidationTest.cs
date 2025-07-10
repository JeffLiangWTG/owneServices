using System;
using CargoWise.ComponentModel;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Rating.Business.Testing
{
	[TestedType(typeof(CargoGuideCredentials))]
	public class CargoGuideCredentialsValidationTest : RegistryBusinessObjectTemplateTestCase<CargoGuideCredentials>
	{
		protected override bool RequiresFactory
		{
			get { return true; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return true; }
		}
		protected override CargoGuideCredentials GetBusinessObjectToClone()
		{
			return new CargoGuideCredentials { Login = "testlogin1", Password = "testpassword1" };
		}

		protected override CargoGuideCredentials GetBusinessObjectToSerialise()
		{
			return new CargoGuideCredentials { Login = "testlogin2", Password = "testpassword2" };
		}

		public void TestConstructor()
		{
			var testCargoGuideCredentials = new CargoGuideCredentials { Login = "testlogin", Password = "testpassword" };

			AssertExceptionThrown<ArgumentNullException>("Should be exception when CargoGuideCredentials parameter is null", () => new CargoGuideCredentialsValidation(null));
			AssertNoExceptionThrown("Should not thrown any exception.", () => new CargoGuideCredentialsValidation(testCargoGuideCredentials));
		}

		const string PasswordMustNotContainsNonAsciiChar = "The password contains non-ASCII characters. Please check that you entered the intended text correctly. If you pasted it into the text box, please try typing it instead.";
		const string LoginMustNotContainsNonAsciiChar = "The login contains non-ASCII characters. Please check that you entered the intended text correctly. If you pasted it into the text box, please try typing it instead.";

		public void TestValidateCargoGuideCredentialsPassword()
		{
			var testCargoGuideeCredentials = new CargoGuideCredentials();

			testCargoGuideeCredentials.Password = "pa¶ssword";
			AssertEquals(true, testCargoGuideeCredentials.PasswordInfo.HasError(PasswordMustNotContainsNonAsciiChar));

			testCargoGuideeCredentials.Password = "password";
			AssertEquals(false, testCargoGuideeCredentials.PasswordInfo.HasErrors());
		}

		public void TestValidateCargoGuideCredentialsLogin()
		{
			var testCargoGuideeCredentials = new CargoGuideCredentials();

			testCargoGuideeCredentials.Login = "lo¶gin";
			AssertEquals(true, testCargoGuideeCredentials.LoginInfo.HasError(LoginMustNotContainsNonAsciiChar));

			testCargoGuideeCredentials.Login = "login";
			AssertEquals(false, testCargoGuideeCredentials.LoginInfo.HasErrors());
		}
	}
}
