using System.Net;
using System.Security.Principal;
using CargoWise.EntityFramework.Testing;
using Enterprise.Services.ServiceHost.NetCore;

namespace Enterprise.Services.ServiceHost.Tests
{
	class DummyControllerTest : TestCaseWithFactory
	{
		public void TestDummyControllerGet()
		{
			var expected = "DummyController - Get";
			var result = GetController();
			AssertHelper.AssertResult(expected, result.Get());
		}

		public void TestDummyControllerGetFullTestName()
		{
			var expected = "FirstName : Test A LastName : Test B";
			var result = GetController().GetWithRouteParams("Test A", "Test B");
			AssertHelper.AssertResult(expected, result);
		}

		#region testing helper methods for returning http response codes with content-type application/json

		public void TestDummyControllerBadRequest()
		{
			var controller = GetController();

			controller.BadRequestSample()
				.AssertResultContains(HttpStatusCode.BadRequest, "Bad Request");
		}

		public void TestDummyControllerBadRequestStringResponse()
		{
			var controller = GetController();

			var expected = """
				{"Message":"Bad Request"}
				""";

			controller.BadRequestWithStringResponse()
				.AssertResultEquals(HttpStatusCode.BadRequest, expected);
		}

		public void TestDummyControllerForbidden()
		{
			var controller = GetController();
			controller.ForbiddenSample()
				.AssertResultContains(HttpStatusCode.Forbidden, "Forbidden");
		}

		public void TestDummyControllerConflict()
		{
			var controller = GetController();
			controller.ConflictSample()
				.AssertResultContains(HttpStatusCode.Conflict, "Conflict");
		}

		public void TestDummyControllerUnprocessableEntity()
		{
			var controller = GetController();
			controller.UnprocessableEntitySample()
				.AssertResultContains((HttpStatusCode)422, "Unprocessable Entity");
		}

		public void TestDummyControllerInternalServerError()
		{
			var controller = GetController();
			controller.InternalServerErrorSample()
				.AssertResultContains(HttpStatusCode.InternalServerError, "Internal Server Error");
		}

		public void TestDummyControllerInternalServerErrorWithException()
		{
			var controller = GetController();

			var expected = """
				{"Message":"An error has occurred."}
				""";

			controller.InternalServerErrorWithExceptionSample()
				.AssertResultEquals(HttpStatusCode.InternalServerError, expected);
		}

		#endregion
		#region testing helper methods for returning http response codes with custom content-type e.g text/plain
		public void TestDummyControllerPlainBadRequest()
		{
			var controller = GetController();
			var response = controller.PlainTextBadRequestSample();

			response.AssertResultIsString();
			response.AssertResultContains(HttpStatusCode.BadRequest, "Bad Request");
		}

		public void TestDummyControllerPlainForbiddenObject()
		{
			var controller = GetController();
			var response = controller.PlainTextForbiddenObjectSample();

			response.AssertResultIsString();
			response.AssertResultContains(HttpStatusCode.Forbidden, "Forbidden");
		}

		public void TestDummyControllerPlainForbidden()
		{
			var controller = GetController();
			var response = controller.PlainTextForbiddenSample();

			response.AssertResultIsString();
			response.AssertResultEquals(HttpStatusCode.Forbidden, "Forbidden String Sample.");
		}

		public void TestDummyControllerPlainConflict()
		{
			var controller = GetController();
			var response = controller.PlainTextConflictSample();

			response.AssertResultIsString();
			response.AssertResultContains(HttpStatusCode.Conflict, "Conflict");
		}

		public void TestDummyControllerPlainUnprocessableEntity()
		{
			var controller = GetController();
			var response = controller.PlainTextUnprocessableEntitySample();

			response.AssertResultIsString();
			response.AssertResultContains((HttpStatusCode)422, "Unprocessable Entity");
		}

		public void TestDummyControllerPlainInternalServerError()
		{
			var controller = GetController();
			var response = controller.PlainTextInternalServerErrorSample();

			response.AssertResultIsString();
			response.AssertResultContains(HttpStatusCode.InternalServerError, "Internal Server Error");
		}
		#endregion

		#region Testing mocked Identity

		public void TestDummyControllerHasUser()
		{
			var builder = new ControllerBuilder<DummyController>();
			var controller = builder
				.AddIdentity(new GenericIdentity("TestUser"))
				.BuildController();

			AssertNotNull(controller.User);
			Assert(controller.User.Identity.IsAuthenticated);
			AssertEquals("TestUser", controller.User.Identity.Name);
		}

		public void TestDummyControllerHasNoUser()
		{
			var controller = GetController();
			AssertNotNull(controller.User);
			Assert(!controller.User.Identity.IsAuthenticated);
			AssertNullOrEmptyOrWhitespace(controller.User.Identity.Name);
		}
		#endregion

		DummyController GetController()
		{
			var builder = new ControllerBuilder<DummyController>();
			return builder
				.BuildController();
		}
	}
}
