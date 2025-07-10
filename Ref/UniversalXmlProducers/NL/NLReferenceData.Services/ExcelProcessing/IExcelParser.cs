using System.Collections.Generic;

namespace CargoWise.RefDbRepo.NLReferenceData.Services
{
	public interface IExcelParser<T> where T : class
	{
		IList<T> ReadXlsFile(IList<string> files);
	}
}
