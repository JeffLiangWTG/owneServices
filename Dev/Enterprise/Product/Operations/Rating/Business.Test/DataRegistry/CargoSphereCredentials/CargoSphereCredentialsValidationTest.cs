using System;
using CargoWise.ComponentModel;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Rating.Business.Testing
{
	[TestedType(typeof(CargoSphereCredentials))]
	public class CargoSphereCredentialsValidationTest : RegistryBusinessObjectTemplateTestCase<CargoSphereCredentials>
	{
		protected override bool RequiresFactory
		{
			get { return true; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return true; }
		}
		protected override CargoSphereCredentials GetBusinessObjectToClone()
		{
			return new CargoSphereCredentials { Login = "testlogin1", Password = "testpassword1", SystemCode = "testsystemcode1" };
		}

		protected override CargoSphereCredentials GetBusinessObjectToSerialise()
		{
			return new CargoSphereCredentials { Login = "testlogin2", Password = "testpassword2", SystemCode = "testsystemcode2" };
		}

		public void TestConstructor()
		{
			var testCargoSphereCredentials = new CargoSphereCredentials { Login = "testlogin", Password = "testpassword", SystemCode = "testsystemcode" };

			AssertExceptionThrown<ArgumentNullException>("Should be exception when CargoSphereCredentials parameter is null", () => new CargoSphereCredentialsValidation(null));
			AssertNoExceptionThrown("Should not thrown any exception.", () => new CargoSphereCredentialsValidation(testCargoSphereCredentials));
		}

		const string PasswordMustNotContainsNonAsciiChar = "The password contains non-ASCII characters. Please check that you entered the intended text correctly. If you pasted it into the text box, please try typing it instead.";
		const string LoginMustNotContainsNonAsciiChar = "The login contains non-ASCII characters. Please check that you entered the intended text correctly. If you pasted it into the text box, please try typing it instead.";
		const string SystemCodeMustNotContainsNonAsciiChar = "The system code contains non-ASCII characters. Please check that you entered the intended text correctly. If you pasted it into the text box, please try typing it instead.";

		public void TestValidateCargoSphereCredentialsPassword()
		{
			var testCargoSphereCredentials = new CargoSphereCredentials();

			testCargoSphereCredentials.Password = "pa¶ssword";
			AssertEquals(true, testCargoSphereCredentials.PasswordInfo.HasError(PasswordMustNotContainsNonAsciiChar));

			testCargoSphereCredentials.Password = "password";
			AssertEquals(false, testCargoSphereCredentials.PasswordInfo.HasErrors());
		}

		public void TestValidateCargoSphereCredentialsLogin()
		{
			var testCargoSphereCredentials = new CargoSphereCredentials();

			testCargoSphereCredentials.Login = "lo¶gin";
			AssertEquals(true, testCargoSphereCredentials.LoginInfo.HasError(LoginMustNotContainsNonAsciiChar));

			testCargoSphereCredentials.Login = "login";
			AssertEquals(false, testCargoSphereCredentials.LoginInfo.HasErrors());
		}
		public void TestValidateCargoSphereCredentialsSystemCode()
		{
			var testCargoSphereCredentials = new CargoSphereCredentials();

			testCargoSphereCredentials.SystemCode = "SY¶S";
			AssertEquals(true, testCargoSphereCredentials.SystemCodeInfo.HasError(SystemCodeMustNotContainsNonAsciiChar));

			testCargoSphereCredentials.SystemCode = "SYS";
			AssertEquals(false, testCargoSphereCredentials.SystemCodeInfo.HasErrors());
		}
	}
}
