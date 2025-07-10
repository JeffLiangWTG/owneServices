using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Scaffolding;
using Microsoft.Extensions.DependencyInjection;

namespace CargoWise.RefDbRepo.Staging.Schema_New
{
	class RefDbRepoStagingDesignTimeServices : IDesignTimeServices
	{
		public void ConfigureDesignTimeServices(IServiceCollection serviceCollection)
		{
			serviceCollection.AddSingleton<IPluralizer, RefDbRepoStagingPluralizer>();
			serviceCollection.AddSingleton<IScaffoldingModelFactory, RefDbRepoStagingScaffoldingModelFactory>();
		}
	}
}
