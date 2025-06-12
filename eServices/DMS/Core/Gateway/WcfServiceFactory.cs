using Unity;
using Unity.Wcf;

namespace DmsGateway
{
	public class WcfServiceFactory : UnityServiceHostFactory
	{
		protected override void ConfigureContainer(IUnityContainer container)
		{
			container
				// https://github.com/dotnet/runtime/blob/2af0051c894aac4d1a9d6479ad6dd7547a688363/src/libraries/Microsoft.Extensions.Http/src/DependencyInjection/HttpClientFactoryServiceCollectionExtensions.cs#L26
				.RegisterType<IeHubStreamedService, eHubStreamedService>();
		}
	}
}
