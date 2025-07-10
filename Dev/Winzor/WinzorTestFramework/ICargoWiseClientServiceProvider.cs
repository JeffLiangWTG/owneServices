using Microsoft.Extensions.DependencyInjection;

namespace WinzorTestFramework;

public interface ICargoWiseClientServiceProvider
{
	IServiceCollection AddCargoWiseClient(IServiceCollection services);
}
