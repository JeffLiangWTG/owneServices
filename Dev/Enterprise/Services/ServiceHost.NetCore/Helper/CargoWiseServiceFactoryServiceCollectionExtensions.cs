using Microsoft.Extensions.DependencyInjection;

namespace Enterprise.Services.ServiceHost.NetCore
{
	public static class CargoWiseServiceFactoryServiceCollectionExtensions
	{
		/// <summary>
		/// Add Service Factories for CargoWise Enterprise services to allows them to be injected by ASP.Net Core's inbuilt DI.
		/// 
		/// To use wrap the affected class with ICargoWiseServiceFactory<> (e.g. "ICargoWiseServiceFactory<IOIDCConfig>") and a factory will be created
		/// via generics and added to the inbuilt DI Service Collection. The factory can then be used to obtain the CargoWise service (including in the
		/// Controller and other service constructors).
		///
		/// Note there is no need to use this for simple classes (such as POCOs) and is mainly to ensure that the CargoWise Enterprise services do not
		/// get instigated before underlying services are (such as the DB).
		/// </summary>
		/// <param name="services"></param>
		/// <returns></returns>
		public static IServiceCollection AddCargoWiseServiceFactory(this IServiceCollection services)
		{
			services.AddSingleton(typeof(ICargoWiseServiceFactory<>), typeof(CargoWiseServiceFactory<>));

			return services;
		}
	}
}
