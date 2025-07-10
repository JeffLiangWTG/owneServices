using System.Threading.Tasks;

namespace CargoWise.RefDbRepo.UniversalXmlParser.Interfaces
{
	public interface IFileTrace
	{
		Task<int> GetLastSuccessLineNumberAsync();
		Task SaveLastSuccessLineNumberAsync(int lineNumber);
		void Remove();
	}
}
