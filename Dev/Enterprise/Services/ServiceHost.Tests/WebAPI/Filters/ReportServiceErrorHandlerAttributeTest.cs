using System.Net;
using System.Net.Http;
using System.Threading;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using System.Web.Http.Routing;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework.Testing;
using Enterprise.DocumentEngine.Exceptions;
using Enterprise.MasterFiles.Business;
using Newtonsoft.Json;

namespace Enterprise.Services.ServiceHost.Tests
{
	sealed class ReportServiceErrorHandlerAttributeTest : TestCaseWithFactory
	{
		public void TestGetService_ShouldNotThrowAttemptToUseConnectionWithoutDisposableAction()
		{
			var lastError = string.Empty;
			OrgContact contact;
			using (Db.DisposableActionForDbConnection())
			{
				contact = Factory.NewWithValidTestData<OrgContact>();
				contact.ParentOrg.OH_Code = "AAAAAA";
				Factory.Save();
			}

			var thread = new Thread(() =>
			{
				ErrorReporter.Clear();
				var controller = new ReportControllerForTest();
				GlowTicketTestHelper.SetUpContactPrincipal(controller, contact);

				using (var request = new HttpRequestMessage())
				{
					var httpActionContext = new HttpActionContext()
					{
						ControllerContext = new HttpControllerContext()
						{
							Request = request,
							Controller = controller,
							RequestContext = new HttpRequestContext() { }
						}
					};

					var errorHandlerAttribute = new ReportServiceErrorHandlerAttribute();
					errorHandlerAttribute.OnActionExecuting(httpActionContext);

					var httpActionExecutedContext = new HttpActionExecutedContext(httpActionContext, null);
					errorHandlerAttribute.OnActionExecuted(httpActionExecutedContext);
				}

				lastError = ErrorReporter.LastMessageReported;
				ErrorReporter.Clear();
			});
			thread.Start();
			thread.Join();

			// Should not be "Attempt to use Db.Connection without using Db.DisposableActionForDbConnection()"
			AssertNullOrEmpty(lastError);
		}

		public void TestCrearRunningErrorOnActionExecuting()
		{
			var controller = new ReportControllerForTest();
			using (var request = new HttpRequestMessage())
			{
				var httpActionContext = new HttpActionContext()
				{
					ControllerContext = new HttpControllerContext()
					{
						Request = request,
						Controller = controller,
						RequestContext = new HttpRequestContext() { }
					}
				};

				controller.Service.RunningError.Errors.Add("Jerry Test Error");
				AssertEquals("Should have 1 error", 1, controller.Service.RunningError.Errors.Count);

				var errorHandlerAttribute = new ReportServiceErrorHandlerAttribute();
				errorHandlerAttribute.OnActionExecuting(httpActionContext);
				AssertEquals("Error should be cleared", 0, controller.Service.RunningError.Errors.Count);
			}
		}

		public void TestResponseWithErrorMessageOnActionExecuted()
		{
			var controller = new ReportControllerForTest();
			using (var request = new HttpRequestMessage())
			{
				request.Properties.Add("MS_HttpConfiguration", new HttpConfiguration());
				var httpActionContext = new HttpActionContext()
				{
					ControllerContext = new HttpControllerContext(new HttpConfiguration(), new HttpRouteData(new HttpRoute()), request)
					{
						Controller = controller,
						RequestContext = new HttpRequestContext() { },
					}
				};
				var httpActionExecutedContext = new HttpActionExecutedContext(httpActionContext, null);
				var runningError = controller.Service.RunningError;
				var errorHandlerAttribute = new ReportServiceErrorHandlerAttribute();

				CombineAssertions(() =>
				{
					AssertReportRunningError(HttpStatusCode.BadRequest, ReportServiceErrorType.ValidationError, "ValidationError Test");
					AssertReportRunningError(HttpStatusCode.InternalServerError, ReportServiceErrorType.RunningError, "RunningError Test");
					AssertReportRunningError(HttpStatusCode.InternalServerError, ReportServiceErrorType.DeliveryError, "DeliveryError Test");
					AssertReportRunningError(HttpStatusCode.InternalServerError, ReportServiceErrorType.ConfigurationError, "ConfigurationError Test");
					AssertReportRunningError(HttpStatusCode.Forbidden, ReportServiceErrorType.Unauthorized, "Unauthorized Test");
					AssertReportRunningError(HttpStatusCode.InternalServerError, ReportServiceErrorType.LookupError, "LookupError Test");
				});

				void AssertReportRunningError(HttpStatusCode expectedHttpStatusCode, ReportServiceErrorType expectedErrorType, string expectedErrorMessage)
				{
					runningError.Errors.Clear();
					runningError.ErrorType = expectedErrorType;
					runningError.Errors.Add(expectedErrorMessage);

					var expectedJson = JsonConvert.SerializeObject(runningError);

					errorHandlerAttribute.OnActionExecuted(httpActionExecutedContext);
					var actualResultJson = httpActionExecutedContext.Response.Content.ReadAsStringAsync().GetAwaiter().GetResult();

					AssertEquals(expectedHttpStatusCode, httpActionExecutedContext.Response.StatusCode);
					AssertEquals(expectedJson, actualResultJson);
				}
			}
		}

		class ReportControllerForTest : ReportDataBaseController
		{
		}
	}
}
