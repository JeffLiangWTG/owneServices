using System.Collections.Generic;
using System.Text;
using CargoWise.RefDbRepo.SharedReferenceData.Services.Tariff.Models;

namespace CargoWise.RefDbRepo.SharedReferenceData.Business.Tariff.Processors
{
	public interface IProcessor
	{
		void LoadData(string chapterFilter, IReadOnlyCollection<IFileDetails> files, StringBuilder errorCollector);
		void UpdateModels(string chapterFilter, List<ITariffModel> referenceData, StringBuilder errorCollector);
		void ProcessChapter(string chapterFilter, string outputPath);
		List<ITariffModel> Models { get; }
		bool IsChapterSpecific { get; }
	}
}
