using System;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Routing;
using CargoWise.Common;
using CargoWise.EntityFramework.Testing;
using Enterprise.DocumentScanning.Integration;
using Moq;
using Newtonsoft.Json;

namespace Enterprise.Services.ServiceHost.Tests
{
	public class EDocsShipamaxControllerTest : TestCaseWithFactory
	{
		public void TestGetErrorMessage()
		{
			var validErrorException = new ShipamaxServiceException(ShipamaxServiceErrorType.ValidationError, "Invalid error");
			var result = controller.GetErrorMessage(validErrorException);
			result.AssertResultContains(HttpStatusCode.Forbidden, "Invalid error");

			var notFoundException = new ShipamaxServiceException(ShipamaxServiceErrorType.NotFoundError, "Not found error");
			result = controller.GetErrorMessage(notFoundException);
			result.AssertResultContains(HttpStatusCode.NotFound, "Not found error");

			var statusException = new ShipamaxServiceException(ShipamaxServiceErrorType.ConfigurationError, "Status error");
			result = controller.GetErrorMessage(statusException);
			result.AssertResultContains(HttpStatusCode.BadRequest, "Status error");

			var configurationException = new ShipamaxServiceException(ShipamaxServiceErrorType.ConfigurationError, "Configuration error");
			result = controller.GetErrorMessage(configurationException);
			result.AssertResultContains(HttpStatusCode.BadRequest, "Configuration error");

			var internalServiceException = new ShipamaxServiceException(ShipamaxServiceErrorType.RunningError, "Internal service error");
			result = controller.GetErrorMessage(internalServiceException);
			result.AssertResultContains(HttpStatusCode.InternalServerError, "Internal service error");
			AssertEquals("EDocsShipamaxController_ExceptionHandler", ErrorReporter.LastKeyReported);
			ErrorReporter.Clear();
		}

		public void TestUpdateParseResult_TokenNotIncluded()
		{
			var actionResult = controller.UpdateParseResult(Guid.Empty, new ShipamaxParseResult());
			actionResult.AssertResultContains(HttpStatusCode.Forbidden, "The eDoc authorization token is missing");
		}

		public void TestUpdateParseResult_TokenIncludedButInvalid()
		{
			controller.Request.Headers.Add("Doc-Token", "XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX");
			serviceMock.Setup(s => s.SaveParseResult(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<ShipamaxParseResult>()))
					   .Throws(() => new ShipamaxServiceException(ShipamaxServiceErrorType.ValidationError, "The eDoc authorization token is invalid."));

			var actionResult = controller.UpdateParseResult(Guid.Empty, new ShipamaxParseResult());
			actionResult.AssertResultContains(HttpStatusCode.Forbidden, "The eDoc authorization token is invalid.");
		}

		public void TestUpdateParseResult()
		{
			controller.Request.Headers.Add("Doc-Token", "XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX");
			var actionResult = controller.UpdateParseResult(Guid.Empty, new ShipamaxParseResult());
			actionResult.AssertResultContains(HttpStatusCode.OK, "Parse result has been saved successfully.");
		}

		public void TestValidateDocument_TokenNotIncluded()
		{
			var actionResult = controller.ValidateDocument(Guid.Empty);
			actionResult.AssertResultContains(HttpStatusCode.Forbidden, "The eDoc authorization token is missing");
		}

		public void TestValidateDocument_TokenIncludedButInvalid()
		{
			controller.Request.Headers.Add("Doc-Token", "XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX");
			serviceMock.Setup(s => s.CheckEDocsChanges(It.IsAny<Guid>(), It.IsAny<string>()))
					   .Throws(() => new ShipamaxServiceException(ShipamaxServiceErrorType.ValidationError, "The eDoc authorization token is invalid."));

			var actionResult = controller.ValidateDocument(Guid.Empty);
			actionResult.AssertResultContains(HttpStatusCode.Forbidden, "The eDoc authorization token is invalid.");
		}

		public void TestValidateDocument_OK()
		{
			controller.Request.Headers.Add("Doc-Token", "XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX");
			var actionResult = controller.ValidateDocument(Guid.Empty);
			actionResult.AssertResultContains(HttpStatusCode.OK);
		}

		public void TestValidateDocument_DocumentChanged()
		{
			var docChanges = new ShipamaxEDocsChange[]
			{
				new() { Field = ShipamaxEDocsChangeType.DocType, OldValue = "CIV", NewValue = "PIN" },
				new() { Field = ShipamaxEDocsChangeType.DocFormat, OldValue = "PDF", NewValue = "JPG" },
				new() { Field = ShipamaxEDocsChangeType.DocEditTime, OldValue = new DateTime(2023, 1, 1), NewValue = new DateTime(2024, 1, 1) },
				new() { Field = ShipamaxEDocsChangeType.Deleted, OldValue = false, NewValue = true },
				new() { Field = ShipamaxEDocsChangeType.DocumentPK, OldValue = "12345", NewValue = "23456" }
			};

			controller.Request.Headers.Add("Doc-Token", "XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX");
			serviceMock.Setup(s => s.CheckEDocsChanges(It.IsAny<Guid>(), It.IsAny<string>())).Returns(docChanges);

			var actionResult = controller.ValidateDocument(Guid.Empty);
			actionResult.AssertResultContains((HttpStatusCode)422, JsonConvert.SerializeObject(docChanges));
		}

		#region Implementation

		Mock<IShipamaxService> serviceMock;

		EDocsShipamaxController controller;

		protected override void SetUp()
		{
			base.SetUp();

			serviceMock = new Mock<IShipamaxService>();
			controller = new EDocsShipamaxController(serviceMock.Object);
			controller.ControllerContext = new HttpControllerContext(new HttpConfiguration(), new HttpRouteData(new HttpRoute()), new HttpRequestMessage())
			{
				Controller = controller
			};
			controller.Request.SetConfiguration(new HttpConfiguration());
		}

		#endregion
	}
}
