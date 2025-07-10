using System.Collections.Generic;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using CargoWise.RefDbRepo.SEReferenceData.Services;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.EUNCommonImportTariffPopulator
{
	public class RateUOMCreator : IRateUOMCreator
	{
		public IEnumerable<RefCusRateUOM> Get(IEnumerable<measureComponent> components)
		{
			var uomList = new List<string>();
			foreach (var component in components)
			{
				if (!string.IsNullOrEmpty(component.measurementUnitCode))
				{
					var uom = component.measurementUnitCode + (component.measurementUnitQualifierCode ?? string.Empty);
					if (!uomList.Contains(uom))
					{
						uomList.Add(uom);
						yield return new RefCusRateUOM { ZXG_UOM = uom };
					}
				}
			}
		}

		public IEnumerable<RefCusRateUOM> Get(IEnumerable<measureCondition> conditions)
		{
			var uomList = new List<string>();
			foreach (var condition in conditions)
			{
				if (!string.IsNullOrEmpty(condition.measurementUnitCode))
				{
					var uom = condition.measurementUnitCode + (condition.measurementUnitQualifierCode ?? string.Empty);
					if (!uomList.Contains(uom))
					{
						uomList.Add(uom);
						yield return new RefCusRateUOM { ZXG_UOM = uom };
					}
				}
			}
		}
	}
}
