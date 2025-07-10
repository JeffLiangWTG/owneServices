using System.Threading.Tasks;

namespace CargoWise.RefDbRepo.CarrierMessagingBuss.Shared
{
	public interface IHttpWebHelper<T> where T : class
	{
		Task<T> GetAsync(string url, string token);
	}
}
