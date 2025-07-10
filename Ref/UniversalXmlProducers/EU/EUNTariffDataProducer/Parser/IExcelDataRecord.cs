using System;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.EUNTariffDataProducer
{
	public interface IExcelDataRecord
	{
		string TariffHeader { get; }
		DateTime StartDate { get; }
	}
}
