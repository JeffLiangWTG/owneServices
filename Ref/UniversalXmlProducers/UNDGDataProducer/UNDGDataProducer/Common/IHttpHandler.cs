using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.UNDGDataProducer
{
	public interface IHttpHandler
	{
		Task<HttpResponseMessage> GetAsync(Uri url);
	}
}
