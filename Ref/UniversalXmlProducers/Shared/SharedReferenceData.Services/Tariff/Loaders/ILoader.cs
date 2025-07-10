using System.Collections.Generic;
using System.Text;
using CargoWise.RefDbRepo.SharedReferenceData.Services.Tariff.Models;

namespace CargoWise.RefDbRepo.SharedReferenceData.Services.Tariff.Loaders
{
	public interface ILoader
	{
		List<ITariffModel> LoadData(IReadOnlyCollection<IFileDetails> files, StringBuilder errorCollector);
	}
}
