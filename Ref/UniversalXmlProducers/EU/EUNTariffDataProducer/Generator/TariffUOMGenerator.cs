using System.Collections.Generic;
using System.Linq;
using CargoWise.RefDbRepo.Common.Argument;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.EUNTariffDataProducer
{
	public static class TariffUOMGenerator
	{
		public static RefCusTariffUOM[] GenerateUomRecords(IEnumerable<IRawRateRecord> uomRecords, bool skipTradeGroupAndDataGroupingMapping = false)
		{
			Argument.NotNull(uomRecords, nameof(uomRecords));
			var result = new List<RefCusTariffUOM>();

			foreach (var uomRecord in uomRecords.Where(x => ApplicationConfig.TariffUOMValidMeasureTypeIds.Contains(x.MeasureTypeId)))
			{
				var cu2Uom = new RefCusTariffUOM()
				{
					ZZ8_UOM = GetUomValue(uomRecord.Rate.Trim()) ?? uomRecord.Rate.Trim(),
					ZZ8_Type = Cu2Type,
				};

				if (!skipTradeGroupAndDataGroupingMapping)
				{
					cu2Uom.ZZ8_ZZA_NKTradeGroup = uomRecord.TradeGroup;
					cu2Uom.ZZ8_ZZA_ZZZ_NKDataGrouping = EunDataGrouping;
				}

				result.Add(cu2Uom);
			}
			return result.ToArray();
		}

		public static RefCusTariffUOM DefaultTariffUom => new RefCusTariffUOM() { ZZ8_Type = Cu1Type, ZZ8_UOM = KgmUom };

		static string GetUomValue(string supplementaryUnit)
		{
			foreach (var uomCode in UomCodeLookup.UomCodes)
			{
				if (uomCode.RawUomCode == supplementaryUnit)
				{
					return uomCode.ActualUomCode;
				}
			}
			return null;
		}

		const string Cu1Type = "CU1";
		const string Cu2Type = "CU2";
		const string KgmUom = "KGM";
		const string EunDataGrouping = "EUN";
	}
}
