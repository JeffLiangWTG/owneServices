using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using CargoWise.Common.Interop;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Services.ServiceHost.Tests
{
	class AddChallengeOnUnauthorizedResultTest : TestCaseWithFactory
	{
		public void TestExecuteAsyncThrowException()
		{
			var challenge = new AuthenticationHeaderValue("Basic");
			var result = new AddChallengeOnUnauthorizedResult(challenge, new HttpActionResultForTest());
			HttpResponseMessage message;
			var exception = AssertExceptionThrown<AggregateException>(() => message = result.ExecuteAsync(new CancellationToken()).Result);
			AssertEquals(typeof(HttpException), exception.InnerException?.GetType());
			AssertContains("InnerResult Type: Enterprise.Services.ServiceHost.Tests.HttpActionResultForTest. Time Elapsed: ", exception.InnerException.Message);
			AssertContains("HttpActionResultForTest Exception", exception.InnerException.InnerException?.Message);
		}
	}

	class HttpActionResultForTest : IHttpActionResult
	{
		public Task<HttpResponseMessage> ExecuteAsync(CancellationToken cancellationToken)
		{
			throw new HttpException("HttpActionResultForTest Exception", HResult.E_FAIL);
		}
	}
}
