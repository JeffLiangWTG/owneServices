using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Routing;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Dash.Integration;
using Enterprise.Dash.Integration.Services;
using Moq;

namespace Enterprise.Services.ServiceHost.Tests
{
	public class DashEDocsControllerTests : TestCaseWithFactory
	{
		const string ParameterMustHaveAValueMessage = "Parameter '{0}' must have a value";
		const string ParameterMustBeAValidGuidMessage = "Parameter '{0}' must be a valid GUID: {1}";
		const string DocumentNotFound = "Document not found";

		public void TestGetDashEdocsDetails_Returns_OkResult_With_DashEdocsDetails_When_Edocs_Document_Is_Found()
		{
			const string relatedEntityType = "SHP";
			var relatedEntityId = ZGuid.NewZGuid();
			var docId = ZGuid.NewZGuid();
			var docMainId = ZGuid.NewZGuid();
			var docToken = new ZString(ZGuid.NewZGuid().ToString());
			var branchId = Guid.NewGuid();
			var departmentId = Guid.NewGuid();

			var dashEDocsDetails = new DashEDocsDetails
			{
				DocId = docId.ToGuid(),
				DocMainId = docMainId.ToGuid(),
				RelatedEntityId = relatedEntityId.ToGuid(),
				RelatedEntityTypeCode = relatedEntityType,
				RelatedBranchId = branchId,
				RelatedDepartmentId = departmentId,
			};

			serviceMock.Setup(x => x.GetDashEDocsDetails(docMainId, docId, docToken)).Returns(dashEDocsDetails);

			var result = controller.GetDashEDocsDetails(docMainId.ToString(), docId.ToString(), docToken);
			var responseMessage = result.ExecuteAsync(CancellationToken.None).GetAwaiter().GetResult();

			AssertEquals(responseMessage.StatusCode, HttpStatusCode.OK);

			var resultDashEDocsDetails = responseMessage.Content.ReadAsAsync<DashEDocsDetails>().GetAwaiter().GetResult();

			AssertEquals(resultDashEDocsDetails.RelatedEntityId, relatedEntityId.ToGuid());
			AssertEquals(resultDashEDocsDetails.RelatedEntityTypeCode, relatedEntityType);
			AssertEquals(resultDashEDocsDetails.RelatedBranchId, branchId);
			AssertEquals(resultDashEDocsDetails.RelatedDepartmentId, departmentId);

			serviceMock.Verify(x => x.GetDashEDocsDetails(It.Is<ZGuid>(x => x == docMainId), It.Is<ZGuid>(x => x == docId), It.Is<ZString>(x => x == docToken)), Times.Once);
		}

		public void TestGetDashEdocsDetails_Returns_NotFoundResult_When_Edocs_Document_Is_Not_Found()
		{
			var docId = new ZGuid(Guid.NewGuid());
			var docMainId = new ZGuid(Guid.NewGuid());
			var docToken = new ZString(ZGuid.NewZGuid().ToString());

			serviceMock.Setup(x => x.GetDashEDocsDetails(new ZGuid(docMainId), new ZGuid(docId), docToken)).Throws(new DashException(DocumentNotFound));

			var result = controller.GetDashEDocsDetails(docMainId.ToString(), docId.ToString(), docToken);
			var responseMessage = result.ExecuteAsync(CancellationToken.None).GetAwaiter().GetResult();

			AssertEquals(responseMessage.StatusCode, HttpStatusCode.NotFound);

			var error = responseMessage.Content.ReadAsAsync<HttpError>().GetAwaiter().GetResult();

			AssertEquals(error.Message, DocumentNotFound);

			serviceMock.Verify(x => x.GetDashEDocsDetails(It.Is<ZGuid>(x => x == docMainId), It.Is<ZGuid>(x => x == docId), It.Is<ZString>(x => x == docToken)), Times.Once);
		}

		public void TestGetDashEdocsDetails_Returns_BadRequestResult_When_DocPk_Is_Null()
		{
			string docId = null;
			var docMainId = Guid.NewGuid().ToString();
			var docToken = new ZString(ZGuid.NewZGuid().ToString());

			var result = controller.GetDashEDocsDetails(docMainId, docId, docToken);
			var responseMessage = result.ExecuteAsync(CancellationToken.None).GetAwaiter().GetResult();

			AssertEquals(responseMessage.StatusCode, HttpStatusCode.BadRequest);

			var httpError = responseMessage.Content.ReadAsAsync<HttpError>().GetAwaiter().GetResult();
			var expectedErrorMessage = string.Format(ParameterMustHaveAValueMessage, nameof(docId));

			AssertEquals(httpError.Message, expectedErrorMessage);

			serviceMock.Verify(x => x.GetDashEDocsDetails(It.IsAny<ZGuid>(), It.IsAny<ZGuid>(), It.Is<ZString>(x => x == docToken)), Times.Never);
		}

		public void TestGetDashEdocsDetails_Returns_BadRequestResult_When_DocPk_Is_Empty()
		{
			var docId = string.Empty;
			var docMainId = Guid.NewGuid().ToString();
			var docToken = new ZString(ZGuid.NewZGuid().ToString());

			var result = controller.GetDashEDocsDetails(docMainId, docId, docToken);
			var responseMessage = result.ExecuteAsync(CancellationToken.None).GetAwaiter().GetResult();

			AssertEquals(responseMessage.StatusCode, HttpStatusCode.BadRequest);

			var httpError = responseMessage.Content.ReadAsAsync<HttpError>().GetAwaiter().GetResult();
			var expectedErrorMessage = string.Format(ParameterMustHaveAValueMessage, nameof(docId));

			AssertEquals(httpError.Message, expectedErrorMessage);

			serviceMock.Verify(x => x.GetDashEDocsDetails(It.IsAny<ZGuid>(), It.IsAny<ZGuid>(), It.Is<ZString>(x => x == docToken)), Times.Never);
		}

		public void TestGetDashEdocsDetails_Returns_BadRequestResult_When_DocToken_Is_Null()
		{
			var docId = Guid.NewGuid().ToString();
			var docMainId = Guid.NewGuid().ToString();
			var docToken = (string)null;

			var result = controller.GetDashEDocsDetails(docMainId, docId, docToken);
			var responseMessage = result.ExecuteAsync(CancellationToken.None).GetAwaiter().GetResult();

			AssertEquals(responseMessage.StatusCode, HttpStatusCode.BadRequest);

			var httpError = responseMessage.Content.ReadAsAsync<HttpError>().GetAwaiter().GetResult();
			var expectedErrorMessage = string.Format(ParameterMustHaveAValueMessage, nameof(docToken));

			AssertEquals(httpError.Message, expectedErrorMessage);

			serviceMock.Verify(x => x.GetDashEDocsDetails(It.IsAny<ZGuid>(), It.IsAny<ZGuid>(), It.Is<ZString>(x => x == docToken)), Times.Never);
		}

		public void TestGetDashEdocsDetails_Returns_BadRequestResult_When_DocPk_Is_Whitespace()
		{
			var docId = " ";
			var docMainId = Guid.NewGuid().ToString();
			var docToken = new ZString(ZGuid.NewZGuid().ToString());

			var result = controller.GetDashEDocsDetails(docMainId, docId, docToken);
			var responseMessage = result.ExecuteAsync(CancellationToken.None).GetAwaiter().GetResult();

			AssertEquals(responseMessage.StatusCode, HttpStatusCode.BadRequest);

			var httpError = responseMessage.Content.ReadAsAsync<HttpError>().GetAwaiter().GetResult();
			var expectedErrorMessage = string.Format(ParameterMustHaveAValueMessage, nameof(docId));

			AssertEquals(httpError.Message, expectedErrorMessage);

			serviceMock.Verify(x => x.GetDashEDocsDetails(It.IsAny<ZGuid>(), It.IsAny<ZGuid>(), It.Is<ZString>(x => x == docToken)), Times.Never);
		}

		public void TestGetDashEdocsDetails_Returns_BadRequestResult_When_DocPk_Is_Not_A_Valid_Guid()
		{
			var docId = "bla";
			var docMainId = Guid.NewGuid().ToString();
			var docToken = new ZString(ZGuid.NewZGuid().ToString());

			var result = controller.GetDashEDocsDetails(docMainId, docId, docToken);
			var responseMessage = result.ExecuteAsync(CancellationToken.None).GetAwaiter().GetResult();

			AssertEquals(responseMessage.StatusCode, HttpStatusCode.BadRequest);

			var httpError = responseMessage.Content.ReadAsAsync<HttpError>().GetAwaiter().GetResult();
			var expectedErrorMessage = string.Format(ParameterMustBeAValidGuidMessage, nameof(docId), docId);

			AssertEquals(httpError.Message, expectedErrorMessage);

			serviceMock.Verify(x => x.GetDashEDocsDetails(It.IsAny<ZGuid>(), It.IsAny<ZGuid>(), It.Is<ZString>(x => x == docToken)), Times.Never);
		}

		public void TestGetDashEdocsDetails_Returns_BadRequestResult_When_DocMainPk_Is_Not_A_Valid_Guid()
		{
			var docId = Guid.NewGuid().ToString();
			var docMainId = "bla";
			var docToken = new ZString(ZGuid.NewZGuid().ToString());

			var result = controller.GetDashEDocsDetails(docMainId, docId, docToken);
			var responseMessage = result.ExecuteAsync(CancellationToken.None).GetAwaiter().GetResult();

			AssertEquals(responseMessage.StatusCode, HttpStatusCode.BadRequest);

			var httpError = responseMessage.Content.ReadAsAsync<HttpError>().GetAwaiter().GetResult();
			var expectedErrorMessage = string.Format(ParameterMustBeAValidGuidMessage, nameof(docMainId), docMainId);

			AssertEquals(httpError.Message, expectedErrorMessage);

			serviceMock.Verify(x => x.GetDashEDocsDetails(It.IsAny<ZGuid>(), It.IsAny<ZGuid>(), It.Is<ZString>(x => x == docToken)), Times.Never);
		}

		Mock<IDashEDocsService> serviceMock;

		DashEDocsController controller;

		protected override void SetUp()
		{
			base.SetUp();

			serviceMock = new Mock<IDashEDocsService>();
			controller = new DashEDocsController(serviceMock.Object);
			controller.ControllerContext = new HttpControllerContext(new HttpConfiguration(), new HttpRouteData(new HttpRoute()), new HttpRequestMessage())
			{
				Controller = controller
			};
			controller.Request.SetConfiguration(new HttpConfiguration());
		}
	}
}
