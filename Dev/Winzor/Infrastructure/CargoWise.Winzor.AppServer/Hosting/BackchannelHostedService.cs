using System.Threading;
using System.Threading.Tasks;
using CargoWise.Blazor.HostedAppRuntime.Backchannel;
using Enterprise.Winzor.Architecture;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace CargoWise.Winzor.AppServer.Hosting
{
	public class BackchannelHostedService : BackgroundService
	{
		readonly ILogger logger;
		readonly IBackchannelProvider backchannelProvider;
		readonly ICargoWiseAuthStateProvider cargoWiseAuthStateProvider;

		public BackchannelHostedService(ILogger logger, IBackchannelProvider backchannelProvider, ICargoWiseAuthStateProvider cargoWiseAuthStateProvider)
		{
			this.logger = logger.ForContext<BackchannelHostedService>();
			this.backchannelProvider = backchannelProvider;
			this.cargoWiseAuthStateProvider = cargoWiseAuthStateProvider;
		}

		protected override async Task ExecuteAsync(CancellationToken stoppingToken)
		{
			logger.Verbose($"Start backchannel service");

			var authSuccess = await cargoWiseAuthStateProvider.AuthenticationComplete.Task;
			if (!authSuccess)
			{
				logger.Information($"Authentication failed. Backchannel will not be initialised.");
				return;
			}

			if (cargoWiseAuthStateProvider.BackChannelUrl is not null)
			{
				await backchannelProvider.OpenBackchannelAsync(cargoWiseAuthStateProvider.BackChannelUrl.ToString());
				logger.Debug($"Opened backchannel URL: {cargoWiseAuthStateProvider.BackChannelUrl}.");
			}
			else
			{
				logger.Debug($"Backchannel URL was not provided. Backchannel will not be initialised.");
			}
		}

		public override Task StopAsync(CancellationToken cancellationToken)
		{
			logger.Verbose($"Stop backchannel service");
			return Task.CompletedTask;
		}
	}
}
