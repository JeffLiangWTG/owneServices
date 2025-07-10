using Enterprise.ZArchitecture.Web.ServerServices;
using Enterprise.ZArchitecture.Web.ServerServices.Testing;

namespace Enterprise.Tracking.Web.Testing
{
	abstract class TrackingWebServiceMethodTest<T> : WebServiceMethodTest<T>
			where T : IWebServiceMethod
	{
		protected void AssertResponse(string testMethodParameters, WebServiceResponse expectedResponse, WebServiceResponse actualResponse)
		{
			AssertEquals(expectedResponse.Count, actualResponse.Count);
			for (var i = 0; i < expectedResponse.Count; i++)
			{
				AssertWebServiceResponseToken(testMethodParameters, expectedResponse[i], actualResponse[i]);
			}
		}

		#region Overrides

		protected override string GetExpectedServiceReferencePath()
		{
			return "/WebService/TrackingWebService.asmx";
		}

		protected override string GetExpectedRunTimeAssemblyFormatForScriptReference()
		{
			return "/Runtime/Enterprise_Tracking_Web/{0}/TrackingWebService/{1}";
		}

		#endregion
	}
}
