using System.Threading.Tasks;
using CargoWise.RefDbRepo.Staging.Schema_New;

namespace CargoWise.RefDbRepo.SourceDataBinaryScanner
{
	public interface IScanner
	{
		Task Scan();

		SourceData FindDataInQUEStatus();
	}
}
