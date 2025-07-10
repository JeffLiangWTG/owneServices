using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Moq;
using NUnit.Framework;
using WinzorFramework;

namespace CargoWise.Winzor.AppServer.Test;

class WinzorDispatcherTest
{
	[Test, ExpectNoExceptions]
	public void WinzorDispatcherDisposeTwiceWillNotThrowException()
	{
		var hostBuilder = Host.CreateDefaultBuilder()
			.ConfigureServices((context, services) =>
			{
				services.AddSingleton(s => new WinzorDispatcher(Mock.Of<IFormOpener>(), Mock.Of<IFormInstanceRegister>()));
			});
		using (var host = hostBuilder.Build())
		{
			using (var winzorDispatcher = host.Services.GetService<WinzorDispatcher>())
			{
				_ = Task.Delay(1000).ContinueWith(async t =>
				{
					await host.StopAsync();
				}, CancellationToken.None, TaskContinuationOptions.None, TaskScheduler.Default);
				host.Start();
				host.WaitForShutdown();
			}
		}
	}
}
