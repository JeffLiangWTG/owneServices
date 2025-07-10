using System.Collections.Generic;
using System.Text;
using CargoWise.RefDbRepo.SharedReferenceData.Services.Tariff.Models;

namespace CargoWise.RefDbRepo.SharedReferenceData.Services.Tariff.Loaders
{
	public abstract class LoaderBase<T> : XmlElementReader<T>, ILoader
		where T : ITariffModel
	{
		public List<ITariffModel> LoadData(IReadOnlyCollection<IFileDetails> files, StringBuilder errorCollector)
		{
			var models = ProcessXml(string.Empty, files, errorCollector, (file) => { });

			return models;
		}
	}
}
