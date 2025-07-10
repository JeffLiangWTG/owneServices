using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

namespace CargoWise.RefDbRepo.NewService.Cachable;

public class CacheTowerManifestInitializationService : IHostedService
{
	readonly ICacheTowerProvider cacheProvider;

	public CacheTowerManifestInitializationService(ICacheTowerProvider cacheProvider) => this.cacheProvider = cacheProvider;

	public async Task StartAsync(CancellationToken cancellationToken)
	{
		await cacheProvider.InitializeManifestAsync();
	}

	public Task StopAsync(CancellationToken cancellationToken)
	{
		return Task.CompletedTask;
	}
}
