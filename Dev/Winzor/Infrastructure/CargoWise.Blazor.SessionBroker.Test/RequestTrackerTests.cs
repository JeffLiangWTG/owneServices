using System;
using System.Threading.Tasks;
using CargoWise.Blazor.Testing.Common;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;

namespace CargoWise.Blazor.SessionBroker.Test
{
	public class RequestTrackerTests
	{
		[Test]
		public async Task RequestTrackerStartsNewStopwatchWhenNewClientAccessesAsync()
		{
			using var factory = new CustomWebApplicationFactory<Startup>();
			var requestTracker = factory.Services.GetRequiredService<RequestTracker>();

			using var client = factory.CreateClient();
			await client.GetAsync("/");
			await Task.Delay(5000);
			var time = requestTracker.TimeElapsedSinceLastActionCompleted().TotalMilliseconds;

			await client.GetAsync("/");
			var time1 = requestTracker.TimeElapsedSinceLastActionCompleted().TotalMilliseconds;

			// Allow some leeway for inexact timing on DAT test servers
			Assert.That(time, Is.InRange(4500, 5500));
			Assert.That(time1, Is.LessThan(500));
		}
	}
}
