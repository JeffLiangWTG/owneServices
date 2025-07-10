using Enterprise.Tracking.Web.ServerServices;
using Enterprise.ZArchitecture.Web.Business.Testing;
using Enterprise.ZArchitecture.Web.ServerServices;
using Enterprise.ZArchitecture.Web.ServerServices.Testing;

namespace Enterprise.Tracking.Web.Testing
{
	[HttpContextEnabledTest]
	sealed class TrackingWebServicesTest : WebServicesTest
	{
		#region Test Cases

		public override void TestInstance()
		{
			AssertNotNull(WebServices.Instance);
			AssertEquals(typeof(WebServices), WebServices.Instance.GetType());
		}

		public override void TestSharedWebServiceReference()
		{
			AssertNotNull(TrackingWebServices.Instance.WebServiceReference);
			AssertEquals("/WebService/TrackingWebService.asmx", TrackingWebServices.Instance.WebServiceReference.Path);
		}

		#endregion
	}
}
