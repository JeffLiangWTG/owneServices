using System.Net.Http;

namespace CargoWise.RefDbRepo.CarrierMessagingBuss.Shared
{
	public interface IHttpClientFactory
	{
		HttpClient CreateClient();
	}
}
