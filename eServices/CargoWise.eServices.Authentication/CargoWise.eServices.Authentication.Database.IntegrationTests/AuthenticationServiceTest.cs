namespace CargoWise.eServices.Authentication.IntegrationTests
{
	using CargoWise.eServices.Authentication.ServiceClient;
	using NUnit.Framework;

	[TestFixture]
	public class AuthenticationServiceTest : AuthenticationTest
	{
		AuthWebServiceApi api;

		[TestCase("C", true)]
		[TestCase("XXXXX", false)]
		public void TestCheckSystemExistence_SystemId(string systemId, bool expectedResult)
		{
			api = new AuthWebServiceApi();

			var result = api.CheckSystemExistence(systemId);

			AssertExpectedResult(expectedResult, result);
		}

		[TestCase("DAU", "TST", true)]
		[TestCase("DAU", "TTT", false)]
		public void TestCheckSystemExistence_EnterpriseAndServer(string enterpriseCode, string serverCode, bool expectedResult)
		{
			api = new AuthWebServiceApi();

			var result = api.CheckSystemExistence(enterpriseCode, serverCode);

			AssertExpectedResult(expectedResult, result);
		}

		[TestCase("C", "password", true)]
		[TestCase("C", "wrong", false)]
		public void TestValidateSystem_SystemId(string systemId, string password, bool expectedResult)
		{
			api = new AuthWebServiceApi();

			var result = api.ValidateSystem(systemId, password);

			AssertExpectedResult(expectedResult, result);
		}

		[TestCase("DAU", "TST", "password", true)]
		[TestCase("DAU", "TST", "wrong", false)]
		[TestCase("DAU", "TTT", "password", false)]
		[TestCase("XXX", "TST", "password", false)]
		public void TestValidateSystem_EnterpriseAndServer(string enterpriseCode, string serverCode, string password, bool expectedResult)
		{
			api = new AuthWebServiceApi();

			var result = api.ValidateSystem(enterpriseCode, serverCode, password);

			AssertExpectedResult(expectedResult, result);
		}

		[Test]
		public void TestRevalidateAfterPasswordChange()
		{
			api = new AuthWebServiceApi();

			AssertExpectedResult(true, api.ValidateSystem("DAU", "TST", "password"));
			AssertExpectedResult(false, api.ValidateSystem("DAU", "TST", "wrong"));
			AssertExpectedResult(true, api.ValidateSystem("DAU", "TST", "password"));

			AssertExpectedResult(true, api.ValidateSystem("C", "password"));
			AssertExpectedResult(false, api.ValidateSystem("C", "wrong"));
			AssertExpectedResult(true, api.ValidateSystem("C", "password"));
		}

		private void AssertExpectedResult(bool expected, bool actual)
		{
			Assert.AreEqual(expected, actual, $"API client returned {actual}, but expected {expected}.  API Result: {api.LastResult}");
		}
	}
}
