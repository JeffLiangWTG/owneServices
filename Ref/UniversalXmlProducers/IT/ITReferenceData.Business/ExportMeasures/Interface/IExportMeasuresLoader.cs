using System.Collections.Generic;
using System.Threading.Tasks;

namespace CargoWise.RefDbRepo.ITReferenceData.Business.ExportMeasures
{
	public interface IExportMeasuresLoader
	{
		Task DoHandshakeAsync();
		Task<IReadOnlyCollection<MeasureInformation>> GetMeasureInformationAsync(string tariffCode);
	}
}
