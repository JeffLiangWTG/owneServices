using System.IO;
using System.Threading.Tasks;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.Common
{
	public interface IHttpClientHelper
	{
		Task<string> GetWebPageAsync(string url);
		Task<string> GetWebPageAsync(string url, string charset);
		Task<TRead> PostAndReadAsAsync<TRead, TPost>(string url, TPost value);
		Task<string> PostAndReadAsAsyncString(string url, string value, string mediaTypeName);
		Task<Stream> GetAsync(string url);
		Task<string[]> GetMatchedEntityCodesAsync(string url, string[] entityNames);
		void CloseConnection();
	}
}
