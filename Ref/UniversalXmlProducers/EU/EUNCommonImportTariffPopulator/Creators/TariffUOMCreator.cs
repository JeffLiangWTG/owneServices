using System.Collections.Generic;
using CargoWise.RefDbRepo.Common.Argument;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using CargoWise.RefDbRepo.SEReferenceData.Services;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.EUNCommonImportTariffPopulator
{
	public class TariffUOMCreator : ITariffUOMCreator
	{
		const string CU1Type = "CU1";
		const string CU2Type = "CU2";
		const string KGMUom = "KGM";

		public static RefCusTariffUOM GetDefaultTariffUOM => new RefCusTariffUOM { ZZ8_Type = CU1Type, ZZ8_UOM = KGMUom };

		public IEnumerable<RefCusTariffUOM> Get(measure measure)
		{
			Argument.NotNull(measure, nameof(measure));
			switch (measure.measureType)
			{
				case "109":
				case "110":
					var uomList = new List<string>();
					foreach (var component in measure.measureComponent)
					{
						var uom = component.measurementUnitCode + (component.measurementUnitQualifierCode ?? string.Empty);
						if (!uomList.Contains(uom))
						{
							uomList.Add(uom);
							yield return new RefCusTariffUOM { ZZ8_UOM = uom, ZZ8_Type = CU2Type, ZZ8_ZZA_NKTradeGroup = measure.geographicalAreaId };
						}
					}
					break;
				default:
					break;
			}
		}
	}
}
