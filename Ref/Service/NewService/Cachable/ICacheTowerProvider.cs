using System.Threading.Tasks;

namespace CargoWise.RefDbRepo.NewService;

public interface ICacheTowerProvider
{
	Task<T> Get<T>(string key);
	Task Add(string key, object value);
	Task Remove(string key);
	Task InitializeManifestAsync();
}
