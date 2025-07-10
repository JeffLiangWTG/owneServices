using System.Net.Http;
using System.Threading.Tasks;

namespace CargoWise.RefDbRepo.CNReferenceData.Services
{
	public interface IHttpHandler
	{
		Task<HttpResponseMessage> GetAsync(string url);
	}
}
