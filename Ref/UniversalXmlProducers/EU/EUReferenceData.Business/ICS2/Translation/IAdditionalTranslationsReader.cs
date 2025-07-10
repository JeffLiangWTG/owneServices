using System.Collections.Generic;

namespace CargoWise.RefDbRepo.EUReferenceData.Business
{
	public interface IAdditionalTranslationsReader
	{
		IReadOnlyCollection<Language> GetAllTranslations();
	}

	public record Language(string Key, DataParser[] DataParsers);
}
