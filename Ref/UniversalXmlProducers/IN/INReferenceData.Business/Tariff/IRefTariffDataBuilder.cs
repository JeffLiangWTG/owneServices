using System.Collections.Generic;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.INReferenceData.Business
{
	public interface IRefTariffDataBuilder
	{
		Dictionary<string, List<RefCusTariff>> GetRefData(string jsonFolder);
	}
}
