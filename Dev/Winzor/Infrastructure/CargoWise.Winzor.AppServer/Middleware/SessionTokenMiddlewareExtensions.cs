using System;
using Microsoft.AspNetCore.Builder;

namespace CargoWise.Winzor.AppServer.Middleware;

/// <summary>
/// Extensions for <see cref="SessionTokenMiddleware"/>
/// </summary>
public static class SessionTokenMiddlewareExtensions
{
	/// <summary>
	/// Registers middleware to validate the CW-Session-Key header <see cref="SessionTokenMiddleware"/>
	/// </summary>
	/// <param name="app"></param>
	/// <returns></returns>
	/// <exception cref="ArgumentNullException"></exception>
	public static IApplicationBuilder UseSessionTokenMiddleware(this IApplicationBuilder app)
	{
		if (app is null)
		{
			throw new ArgumentNullException(nameof(app));
		}

		return app.UseMiddleware<SessionTokenMiddleware>();
	}
}