using System.Collections.Generic;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.EUNTariffDataProducer
{
	public interface IDeclarableCodeParser
	{
		IEnumerable<IRawDeclarableCodeRecord> Parse(string filePath);
	}
}
