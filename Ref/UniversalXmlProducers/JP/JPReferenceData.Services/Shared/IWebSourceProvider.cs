using System.IO;
using System.Threading.Tasks;

namespace CargoWise.RefDbRepo.JPReferenceData.Services
{
	public interface IWebSourceProvider
	{
		Task<string> GetPageAsync(string url);

		Task<Stream> GetAsync(string url);

		bool IsParallelMode { get; }
	}
}
