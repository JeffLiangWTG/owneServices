using System;
using System.Threading.Tasks;
using System.Timers;
using CargoWise.Blazor.Common;
using CargoWise.Blazor.HostedAppRuntime;
using CargoWise.Common;
using Enterprise.ZArchitecture.GUI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NUnit.Framework;
using WinzorFramework;

namespace CargoWise.Winzor.AppServer.Test;

class WinzorDispatcherAppServerTest
{
	[Test, CancelAfter(2000)]
	public async Task ApplicationDispatcherShouldBeResetWhenWinzorDispatcherIsDisposed()
	{
		var tcs = new TaskCompletionSource();
		var formInstanceRegister = new RegisteredFormInstances();
		var formOpener = new FormOpener();
		using (var winzorDispatcher = new WinzorDispatcher(formOpener, formInstanceRegister))
		{
			var winzorDispatcherIsDisposed = false;
			winzorDispatcher.RegisterDisposeAction(() => winzorDispatcherIsDisposed = true);

			var testEnv = bool.TryParse(Environment.GetEnvironmentVariable("DAT_IS_TESTING"), out var datIsTesting) && datIsTesting ? "DAT" : Environments.Development;
			using var hosting = Program.CreateHostBuilder([], winzorDispatcher).UseEnvironment(testEnv)
				.ConfigureServices(services => services.PostConfigure<CargoWiseOptions>(o => o.AppServerProcessCorrelationId = Guid.Empty)).Build();
			hosting.InitialiseCargoWiseRuntime();
			await hosting.StartAsync();

#pragma warning disable CA2000
			var timer = new Timer { Interval = 1000 };
#pragma warning restore CA2000
			timer.Elapsed += OnElapsedEventHandler;
			timer.Start();

			await hosting.StopAsync();

			void OnElapsedEventHandler(object o, ElapsedEventArgs elapsedEventArgs)
			{
				if (!winzorDispatcherIsDisposed)
				{
					return;
				}

				timer.Stop();
				var helper = new HeartbeatRemoteLogoff();
				var utc = new DateTime(2024, 1, 17, 12, 34, 56);
				helper.OnRemoteUpgradeLogoff(utc, () => false);
				tcs.SetResult();
				timer.Dispose();
			}
		}

		Assert.That(ApplicationDispatcher.Current, Is.Null);
		await tcs.Task;
	}
}
