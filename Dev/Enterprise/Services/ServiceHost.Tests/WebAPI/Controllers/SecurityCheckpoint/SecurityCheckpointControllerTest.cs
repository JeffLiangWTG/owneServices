using System.Net;
using System.Net.Http;
using System.Threading;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Routing;
using Enterprise.Environment;
using NUnit.Framework;

namespace Enterprise.Services.ServiceHost.Tests
{
	class SecurityCheckpointControllerTest : TestCase
	{
		public SecurityCheckpointControllerTest()
		{
			Controller = new SecurityCheckpointController();
			var controllerContext = new HttpControllerContext(new HttpConfiguration(), new HttpRouteData(new HttpRoute()), new HttpRequestMessage());
			Controller.ControllerContext = controllerContext;
		}

		readonly SecurityCheckpointController Controller;

		public void TestGetCheckpointIsAllowed_BadCheckpointName()
		{
			var result = Controller.GetSecurityCheckpointIsAllowed("heather");
			var response = result.ExecuteAsync(CancellationToken.None).GetAwaiter().GetResult();
			AssertEquals(HttpStatusCode.NotFound, response.StatusCode);
		}

		public void TestGetCheckpointIsAllowed_GoodCheckpointName()
		{
			Env.Security.WorkItemNew.IsAllowed = true;
			var result = Controller.GetSecurityCheckpointIsAllowed("WorkItemNew");
			var response = result.ExecuteAsync(CancellationToken.None).GetAwaiter().GetResult();
			AssertEquals(HttpStatusCode.OK, response.StatusCode);

			Env.Security.WorkItemNew.IsAllowed = false;
			result = Controller.GetSecurityCheckpointIsAllowed("WorkItemNew");
			response = result.ExecuteAsync(CancellationToken.None).GetAwaiter().GetResult();
			AssertEquals(HttpStatusCode.Forbidden, response.StatusCode);
		}
	}
}
