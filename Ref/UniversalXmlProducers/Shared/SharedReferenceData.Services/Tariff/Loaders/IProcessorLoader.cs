using System.Collections.Generic;
using System.Text;
using CargoWise.RefDbRepo.SharedReferenceData.Services.Tariff.Models;

namespace CargoWise.RefDbRepo.SharedReferenceData.Services.Tariff.Loaders
{
	public interface IProcessorLoader
	{
		List<ITariffModel> ProcessXml(string chapterFilter, IReadOnlyCollection<IFileDetails> files, StringBuilder errorCollector, PostFileActionDelegate processingAction);
	}
}
