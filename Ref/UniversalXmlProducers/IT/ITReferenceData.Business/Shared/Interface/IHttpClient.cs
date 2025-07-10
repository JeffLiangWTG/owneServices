using System.Net.Http;

namespace CargoWise.RefDbRepo.ITReferenceData.Business
{
	public interface IHttpClient
	{
		string Post(string requestUri, HttpContent content);

		string Get(string requestUri);
	}
}
