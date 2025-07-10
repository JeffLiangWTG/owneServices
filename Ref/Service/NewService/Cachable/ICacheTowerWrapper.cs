using System.Threading.Tasks;

namespace CargoWise.RefDbRepo.NewService;

public interface ICacheTowerWrapper
{
	Task<T> Get<T>(string key, string regionName = null);
	Task Add(string key, object value, string regionName = null);
	Task Remove(string key, string regionName = null);
}
