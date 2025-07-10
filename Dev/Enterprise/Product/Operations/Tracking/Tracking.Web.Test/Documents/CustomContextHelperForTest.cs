using System.Collections.Generic;
using System.IO;
using System.Web;
using System.Web.SessionState;
using Enterprise.ZArchitecture.Web.Business.Testing;

namespace Enterprise.Tracking.Web.Testing
{
	class CustomContextHelperForTest
	{
		public static HttpContext GetCustomContext(string queryParameter, string queryParameterValue)
		{
			Dictionary<string, string> queryParameters = new Dictionary<string, string>();
			queryParameters.Add(queryParameter, queryParameterValue);

			return GetCustomContext(queryParameters);
		}

		public static HttpContext GetCustomContext(Dictionary<string, string> queryParameters)
		{
			string query = "";
			foreach (KeyValuePair<string, string> queryParameter in queryParameters)
			{
				query += string.Format("{0}{1}={2}",
						query.Length == 0 ? "" : "&",
						queryParameter.Key,
						queryParameter.Value);
			}

			var dummyRequest = new DummyWorkerRequest(string.Empty, query, new StringWriter());
			var context = new HttpContext(dummyRequest);
			context.ApplicationInstance = new DummyHttpApplication(dummyRequest);
			var container = new HttpSessionStateContainer("DummySession", new SessionStateItemCollection(), new HttpStaticObjectsCollection(), 60, true, HttpCookieMode.AutoDetect, SessionStateMode.InProc, false);
			SessionStateUtility.AddHttpSessionStateToContext(context, container);
			return context;
		}
	}
}
