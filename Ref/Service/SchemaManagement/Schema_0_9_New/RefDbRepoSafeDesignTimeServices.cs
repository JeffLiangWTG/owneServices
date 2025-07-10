using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.DependencyInjection;

namespace CargoWise.RefDbRepo.Service.Schema_0_9_New
{
	class RefDbRepoSafeDesignTimeServices : IDesignTimeServices
	{
		public void ConfigureDesignTimeServices(IServiceCollection serviceCollection)
		{
			serviceCollection.AddSingleton<IPluralizer, RefDbRepoSafePluralizer>();
		}
	}
}
