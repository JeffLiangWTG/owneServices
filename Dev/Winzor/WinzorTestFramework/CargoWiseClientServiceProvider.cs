using CargoWise.Blazor.Client.Integration.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

namespace WinzorTestFramework;

public class CargoWiseClientServiceProvider : ICargoWiseClientServiceProvider
{
	public IServiceCollection AddCargoWiseClient(IServiceCollection services)
	{
		return services.AddCargoWiseClient();
	}
}
