using System;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.EUNCommonImportTariffPopulator
{
	public interface IXmlFilter<T>
	{
		bool IsValid(T value, DateTime publicationDate);
		T GetValidValue(T value);
	}
}
