using System.Collections.Generic;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.CAReferenceData.Business.CASurtax
{
	public interface ITariffDataProducer
	{
		IEnumerable<RefCusTariff> GetTariff(IEnumerable<string> tariffCodes, int codeLength);
	}
}
