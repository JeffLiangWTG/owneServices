using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Moq;
using NUnit.Framework;

namespace CargoWise.Blazor.SessionBroker.Test
{
	public class RequestTrackerMiddlewareTests
	{
		[Test]
		public async Task TestRequestTrackerMiddlewareInvokeAsync()
		{
			var nextMock = new Mock<RequestDelegate>();

			var requestTracker = new RequestTracker();
			var middleware = new RequestTrackerMiddleware(
				nextMock.Object,
				requestTracker
			);

			var context = new DefaultHttpContext();

			await middleware.InvokeAsync(context);
			await middleware.InvokeAsync(context);
			await middleware.InvokeAsync(context);

			nextMock.Verify(x => x(context), Times.Exactly(3));
		}
	}
}
