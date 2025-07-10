using System.Net.Http;
using System.Threading.Tasks;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.ITCustomsTariffRateProducer.WebHandler
{
	public interface IHttpHandler
	{
		HttpResponseMessage Get(string url);
		Task<HttpResponseMessage> GetAsync(string url);
		HttpResponseMessage Post(string url, HttpContent content);
		Task<HttpResponseMessage> PostAsync(string url, HttpContent content);
		void SetSecureConnection(bool requireSecureConnection);
	}
}