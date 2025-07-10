using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Routing;
using CargoWise.EntityFramework.Testing;
using Enterprise.Dash.Business.Services;
using Enterprise.Dash.Integration;
using Moq;

namespace Enterprise.Services.ServiceHost.Tests
{
	public class DashDocumentControllerTests : TestCaseWithFactory
	{
		public void TestComplete_Returns_BadRequest_When_DashDocumentId_Is_Empty()
		{
			// act
			var result = dashDocumentController.Complete(Guid.Empty);
			var responseMessage = result.ExecuteAsync(CancellationToken.None).GetAwaiter().GetResult();

			// assert
			AssertEquals(responseMessage.StatusCode, HttpStatusCode.BadRequest);

			var error = responseMessage.Content.ReadAsAsync<HttpError>().GetAwaiter().GetResult();

			AssertEquals(error.Message, "Parameter dashDocumentId can't be empty");
		}

		public void TestComplete_Returns_BadRequest_When_DashException_Is_Thrown()
		{
			// arrange
			const string dashExceptionMessage = "Document is obsolete";
			completionServiceMock.Setup(x => x.Complete(It.IsAny<Guid>(), null)).Throws(() => new DashException(dashExceptionMessage));

			// act
			var result = dashDocumentController.Complete(Guid.NewGuid());
			var responseMessage = result.ExecuteAsync(CancellationToken.None).GetAwaiter().GetResult();

			// assert
			AssertEquals(responseMessage.StatusCode, HttpStatusCode.BadRequest);

			var error = responseMessage.Content.ReadAsAsync<HttpError>().GetAwaiter().GetResult();

			AssertEquals(error.Message, dashExceptionMessage);
		}

		public void TestComplete_Returns_Ok()
		{
			// act
			var result = dashDocumentController.Complete(Guid.NewGuid());
			var responseMessage = result.ExecuteAsync(CancellationToken.None).GetAwaiter().GetResult();

			// assert
			AssertEquals(responseMessage.StatusCode, HttpStatusCode.OK);

			completionServiceMock.Verify(x => x.Complete(It.IsAny<Guid>(), null));
		}

		Mock<IDashCompletionService> completionServiceMock;

		DashDocumentController dashDocumentController;

		protected override void SetUp()
		{
			base.SetUp();

			completionServiceMock = new Mock<IDashCompletionService>();
			dashDocumentController = new DashDocumentController(completionServiceMock.Object);
			dashDocumentController.ControllerContext = new HttpControllerContext(new HttpConfiguration(), new HttpRouteData(new HttpRoute()), new HttpRequestMessage())
			{
				Controller = dashDocumentController
			};
			dashDocumentController.Request.SetConfiguration(new HttpConfiguration());
		}
	}
}
