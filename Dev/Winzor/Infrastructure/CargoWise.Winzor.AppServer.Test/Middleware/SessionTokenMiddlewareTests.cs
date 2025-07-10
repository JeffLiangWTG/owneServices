using System;
using System.Net;
using System.Threading.Tasks;
using CargoWise.Blazor.Common;
using CargoWise.Winzor.AppServer.Middleware;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using NUnit.Framework;

namespace CargoWise.Winzor.AppServer.Test.Middleware;

public class SessionTokenMiddlewareTests
{
	[Test]
	public async Task SessionTokenMiddlewareRejectsRequestsWithoutSessionToken()
	{
		var options = Options.Create(new CargoWiseAuthOptions
		{
			SessionToken = Guid.NewGuid().ToString()
		});
		var nextCalled = false;

		var sessionTokenMiddleWare = new SessionTokenMiddleware(_ =>
			{
				nextCalled = true;
				return Task.CompletedTask;
			},
			options,
			NullLogger<SessionTokenMiddleware>.Instance);

		var context = new DefaultHttpContext();
		await sessionTokenMiddleWare.InvokeAsync(context);

		Assert.That(context.Response.StatusCode, Is.EqualTo((int)HttpStatusCode.Unauthorized));
		Assert.That(nextCalled, Is.False);
	}

	[Test]
	public async Task SessionTokenMiddlewareAcceptsRequestsWithSessionToken()
	{
		var options = Options.Create(new CargoWiseAuthOptions
		{
			SessionToken = Guid.NewGuid().ToString()
		});
		var nextCalled = false;

		var sessionTokenMiddleWare = new SessionTokenMiddleware(_ =>
			{
				nextCalled = true;
				return Task.CompletedTask;
			},
			options,
			NullLogger<SessionTokenMiddleware>.Instance);

		var context = new DefaultHttpContext();
		context.Request.Headers.Append(CustomHeaders.CWSessionToken, options.Value.SessionToken);
		await sessionTokenMiddleWare.InvokeAsync(context);

		Assert.That(nextCalled, Is.True);
		Assert.That(context.Response.StatusCode, Is.EqualTo((int)HttpStatusCode.OK));
	}
}
