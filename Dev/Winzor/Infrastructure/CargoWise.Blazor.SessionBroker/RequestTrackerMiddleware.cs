using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace CargoWise.Blazor.SessionBroker
{
	public class RequestTrackerMiddleware
	{
		readonly RequestDelegate _next;
		readonly RequestTracker requestTracker;

		public RequestTrackerMiddleware(RequestDelegate next, RequestTracker requestTracker)
		{
			_next = next;
			this.requestTracker = requestTracker;
		}

		public async Task InvokeAsync(HttpContext context)
		{
			try
			{
				requestTracker.Increment();
				await _next(context);
			}
			finally
			{
				requestTracker.Decrement();
			}
		}
	}
}