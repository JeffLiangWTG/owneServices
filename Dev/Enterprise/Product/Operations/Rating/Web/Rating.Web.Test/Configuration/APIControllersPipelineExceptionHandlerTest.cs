using System.Net;
using System.Threading;
using System.Web.Http.ExceptionHandling;
using Enterprise.Rating.Web.Configuration;
using NUnit.Framework;

namespace Enterprise.Rating.Web.Test.Configuration
{
	public class APIControllersPipelineExceptionHandlerTest : TestCase
	{
		public void TestInvalidBranchException()
		{
			var handler = new APIControllersPipelineExceptionHandler();

			var context = new ExceptionContext(new InvalidBranchException(), new ExceptionContextCatchBlock(string.Empty, true, false));
			var handlerContext = new ExceptionHandlerContext(context);

			handler.Handle(handlerContext);

			var response = handlerContext.Result.ExecuteAsync(CancellationToken.None).GetAwaiter().GetResult();
			AssertEquals(response.StatusCode, HttpStatusCode.BadRequest);
		}

		public void TestInvalidDepartmentException()
		{
			var handler = new APIControllersPipelineExceptionHandler();

			var context = new ExceptionContext(new InvalidDepartmentException(), new ExceptionContextCatchBlock(string.Empty, true, false));
			var handlerContext = new ExceptionHandlerContext(context);

			handler.Handle(handlerContext);

			var response = handlerContext.Result.ExecuteAsync(CancellationToken.None).GetAwaiter().GetResult();
			AssertEquals(response.StatusCode, HttpStatusCode.BadRequest);
		}
	}
}
