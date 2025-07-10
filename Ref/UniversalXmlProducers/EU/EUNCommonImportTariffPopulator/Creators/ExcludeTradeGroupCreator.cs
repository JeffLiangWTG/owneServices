using System.Collections.Generic;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using CargoWise.RefDbRepo.SEReferenceData.Services;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.EUNCommonImportTariffPopulator
{
	public class ExcludeTradeGroupCreator : IExcludeTradeGroupCreator
	{
		public IEnumerable<RefCusExcludedTradeGroup> Get(measure measure)
		{
			foreach (var ex in measure.measureExcludedGeographicalArea)
			{
				yield return new RefCusExcludedTradeGroup
				{
					ZZC_ZZA_NKTradeGroup = ex.geographicalAreaId
				};
			}
		}
	}
}
