using System.Collections.Generic;
using System.Net.Http;
using System.Reflection;
using System.Security.Principal;
using System.Threading;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Routing;
using CargoWise.EntityFramework.Testing;
using Enterprise.DocumentEngine;
using Enterprise.ZArchitecture.Schema;
using Moq;
using Newtonsoft.Json;

namespace Enterprise.Services.ServiceHost.Tests
{
	abstract class BaseReportControllerTest<T> : TestCaseWithFactory where T : ReportDataBaseController
	{
		protected void AssertJsonResult(object expected, IHttpActionResult actionResult, string message = "", JsonSerializerSettings jsonSerializerSettings = default)
		{
			var response = actionResult.ExecuteAsync(CancellationToken.None).GetAwaiter().GetResult();
			AssertJsonResult(expected, response, message, jsonSerializerSettings);
		}

		protected void AssertJsonResult(object expected, HttpResponseMessage response, string message = "", JsonSerializerSettings jsonSerializerSettings = default)
		{
			var expectedJson = JsonConvert.SerializeObject(expected, jsonSerializerSettings);
			var actualJson = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();

			AssertEquals(message, expectedJson, actualJson);
		}

		protected Mock<IReportDataService> mockService;
		protected Mock<T> mockController;
		protected Mock<IGlowAuthenticationTicketIdentity> mockIdentity;
		protected T controller;

		protected override void SetUp()
		{
			base.SetUp();
			mockService = new Mock<IReportDataService>();
			mockController = new Mock<T>() { CallBase = true };
			mockController.SetupGet(c => c.Service).Returns(mockService.Object);
			controller = mockController.Object;

			mockIdentity = new Mock<IGlowAuthenticationTicketIdentity>();
			var mockUser = new Mock<IPrincipal>();
			controller.User = mockUser.Object;
			mockUser.Setup(x => x.Identity).Returns(mockIdentity.Object);
			var mockPrincipal = new Mock<IPrincipal>();
			mockPrincipal.SetupGet(x => x.Identity).Returns(mockIdentity.Object);
			mockIdentity.SetupGet(x => x.IsAuthenticated).Returns(true);
			mockIdentity.SetupGet(x => x.ProviderType).Returns(GlbStaffSchema.Constants.Prefix);

			var request = new HttpRequestMessage();
			request.Properties.Add("MS_HttpConfiguration", new HttpConfiguration());
			controller.ControllerContext = new HttpControllerContext(new HttpConfiguration(), new HttpRouteData(new HttpRoute()), request)
			{
				Controller = controller,
				RequestContext = new HttpRequestContext() { Principal = mockPrincipal.Object }
			};
		}

		public void TestStaffOnlyAuthorizationFilterMethods()
		{
			var methods = new List<string>();
			foreach (var method in controller.GetType().GetMethods())
			{
				if (method.GetCustomAttribute(typeof(StaffOnlyAuthorizationFilterAttribute)) != null)
				{
					methods.Add(method.Name);
				}
			}
			AssertContainsExactElementsInAnyOrder("The method which has StaffOnlyAuthorizationFilterAttribute should be put into StaffOnlyAuthorizationFilterMethods", StaffOnlyAuthorizationFilterMethods, methods);
		}

		protected virtual string[] StaffOnlyAuthorizationFilterMethods { get; } = System.Array.Empty<string>();

		public void TestControllerShouldHaveReportServiceErrorHandlerAttribute()
		{
			var attribute = controller.GetType().GetCustomAttribute<ReportServiceErrorHandlerAttribute>();
			AssertNotNull("Report controller should have ReportServiceErrorHandlerAttribute", attribute);
		}

		protected void AssertJsonResult(string message, object expected, IHttpActionResult actionResult)
		{
			var expectedJson = JsonConvert.SerializeObject(expected);

			var response = actionResult.ExecuteAsync(CancellationToken.None).GetAwaiter().GetResult();
			var actualJson = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();

			AssertEquals(message, expectedJson, actualJson);
		}
	}
}
