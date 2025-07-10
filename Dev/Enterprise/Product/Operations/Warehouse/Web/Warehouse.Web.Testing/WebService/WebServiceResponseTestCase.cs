using NUnit.Framework;

namespace Enterprise.Warehouse.Web.WebService.Testing
{
	public class WebServiceResponseTestCase : TestCase
	{
		#region TestProperties

		public void TestProperties()
		{
			var response = GetNewResponse();
			AssertEquals("", response.SecurityKey);

			response.SecurityKey = "123";
			AssertEquals("123", response.SecurityKey);

			response.ErrorMessage = "Error";
			AssertEquals("Error", response.ErrorMessage);

			TestPropertiesCore();
		}

		protected virtual void TestPropertiesCore()
		{
		}

		#endregion

		#region TestLogError_SetValuesCorrectly

		public void TestLogError_SetValuesCorrectly()
		{
			AssertEquals("Precondition: Error is None", ErrorTypes.None, response.Error);
			AssertNull("Precondition: Error Message is empty", response.ErrorMessage);

			response.LogError(ErrorTypes.BusinessValidationError, "Testing error message");
			AssertEquals("Error is correct", ErrorTypes.BusinessValidationError, response.Error);
			AssertEquals("Error Message is correct", "Testing error message", response.ErrorMessage);

			response.LogError(ErrorTypes.UpgradeRequired, "Adding new error message to check there is not caching.");
			AssertEquals("Error is correct", ErrorTypes.UpgradeRequired, response.Error);
			AssertEquals("Error Message is correct", "Adding new error message to check there is not caching.", response.ErrorMessage);
		}

		#endregion

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			response = GetNewResponse();
		}

		protected virtual WebServiceResponse GetNewResponse()
		{
			return new WebServiceResponse();
		}

		protected WebServiceResponse Response
		{
			get { return response; }
		}

		WebServiceResponse response;

		#endregion
	}
}
