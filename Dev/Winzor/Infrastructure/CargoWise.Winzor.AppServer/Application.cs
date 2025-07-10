using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.Blazor.Common;
using Enterprise.ZArchitecture.Core;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CargoWise.Winzor.AppServer;

public class Application
{
	readonly ILogger<Application> logger;
	readonly IHost host;
	readonly IHostApplicationLifetime _appLifetime;

	public Application(
			ILogger<Application> logger,
			IHost host,
			IHostApplicationLifetime appLifetime,
			IOptions<CargoWiseOptions> cwOptions)
	{
		this.logger = logger;
		this.host = host;
		_appLifetime = appLifetime;
		_appLifetime.ApplicationStopping.Register(OnShutdown);
	}

	void OnShutdown()
	{
		System.Windows.Forms.Application.Exit();
	}

	public async Task ShutdownAsync()
	{
		logger.LogInformation((NoResString)"Stopping host");
		await host.StopAsync(CancellationToken.None);
	}
}
