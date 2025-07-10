using System.Threading.Tasks;
using CargoWise.RefDbRepo.UniversalXmlParser.Interfaces;

namespace CargoWise.RefDbRepo.SourceDataBinaryScanner
{
	public class DummyFileTrace : IFileTrace
	{
		public async Task<int> GetLastSuccessLineNumberAsync()
		{
			return await Task.FromResult(0);
		}

		public void Remove()
		{
		}

		public async Task SaveLastSuccessLineNumberAsync(int lineNumber)
		{
			await Task.CompletedTask;
		}
	}
}
