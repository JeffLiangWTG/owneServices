using System.Collections.Generic;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.EUNTariffDataProducer
{
	public interface ITariffMerger
	{
		IEnumerable<RefCusTariff> ConvertToActualTariffs(IEnumerable<RefCusTariff> rawTariffs, IEnumerable<IWebTariffHeader> applicableTariffs = null, IEnumerable<IRawRateRecord> rawUomRecords = null);
	}
}
