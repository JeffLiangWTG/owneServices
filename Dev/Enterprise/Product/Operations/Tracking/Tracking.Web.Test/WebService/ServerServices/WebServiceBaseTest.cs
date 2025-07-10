using System.Collections.Generic;
using Enterprise.Tracking.Web.WebService;
using Enterprise.ZArchitecture.Web.Business.Testing;
using Enterprise.ZArchitecture.Web.ServerServices;

namespace Enterprise.Tracking.Web.Testing
{
	[HttpContextEnabledTest]
	public abstract class WebServiceBaseTest<T> : WebServiceWithFactoryTest<T>
			where T : WebServiceBase, new()
	{
		#region Test Cases

		public void TestConstructors()
		{
			AssertNotNull(WebService);
		}

		public void TestMethodWithInvalidParameters()
		{
			AssertNotNull(WebService);
			string actualResponse = WebService.Execute(GetMethodName(), "TEST TEST TEST");
			WebServiceResponse response = new WebServiceResponse();
			if (GetMethodIsAllowedToRespondWithErrors())
			{
				response.Add(new ShowErrorResponseToken("Please provide valid parameters for this method"));
			}
			string expectedResponse = response.ToString();
			AssertEquals(expectedResponse, actualResponse);
		}

		public virtual void TestExecute()
		{
			AssertNotNull(WebService);
			TestExecuteSetup();
			foreach (string testMethodParameters in MethodParametersAndExpectedResponseTokens.Keys)
			{
				string expectedResponse = MethodParametersAndExpectedResponseTokens[testMethodParameters];
				string actualResponse = WebService.Execute(GetMethodName(), testMethodParameters);
				AssertEquals(testMethodParameters, expectedResponse, actualResponse);
			}
		}

		#endregion

		#region Implementation

		protected Dictionary<string, WebServiceResponse> MethodParametersAndExpectedResponseTokens
		{
			get
			{
				Dictionary<string, WebServiceResponse> result = new Dictionary<string, WebServiceResponse>();
				SetMethodParametersAndExpectedResponseTokens(result);
				return result;
			}
		}

		protected abstract void SetMethodParametersAndExpectedResponseTokens(Dictionary<string, WebServiceResponse> setting);

		protected virtual void TestExecuteSetup()
		{
		}

		protected virtual bool GetMethodIsAllowedToRespondWithErrors()
		{
			return true;
		}

		protected abstract string GetMethodName();

		protected new WebServiceBase WebService => base.WebService;

		#endregion
	}
}
