using System.Threading.Tasks;

namespace CargoWise.RefDbRepo.Common.Utils;

public interface IFileReader
{
	Task<string> ReadAllTextAsync();
}
