using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using CargoWise.RefDbRepo.SEReferenceData.Services;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.EUNCommonImportTariffPopulator
{
	public static class XMLObjectCreationHelper
	{
		public static IEnumerable<(measure measure, IEnumerable<RefCusRate> rates, IEnumerable<RefCusTariffUOM> tariffUoms)> CreateRatesAndTariffUOMs(IEnumerable<measureType1> measureTypes, IEnumerable<measure> measures)
		{
			var rateCodeCreator = new RateCodeCreator(measureTypes);
			var rateTypeCreator = new RateTypeCreator();
			var excludeTradeGroupCreator = new ExcludeTradeGroupCreator();
			var preferenceCreator = new PreferenceCreator();
			var rateCreator = new RateCreator(new DutyFormulaCreator(), new ConditionDutyFormulaCreator(), new RateUOMCreator());
			var tariffUomCreator = new TariffUOMCreator();

			foreach (var measure in measures)
			{
				var rateCode = rateCodeCreator.Get(measure);
				var rateType = rateTypeCreator.Get(rateCode);
				var preferences = preferenceCreator.Get(measure);
				var rates = rateCreator.Get(measure, rateCode, rateType, preferences).ToArray();
				var applicability = ApplicabilityCreator.Get(measure);
				applicability.RefCusExcludedTradeGroups = excludeTradeGroupCreator.Get(measure).ToArray();
				Array.ForEach(rates, x => x.RefCusApplicabilities = new[] { applicability });
				var tariffUoms = tariffUomCreator.Get(measure);
				yield return (measure, rates, tariffUoms);
			}
		}

		public static IEnumerable<(measure, Func<string, IEnumerable<RefCusCondition>>)> CreateConditions(IEnumerable<measureConditionCode> conditionCodes, IEnumerable<measure> measures)
		{
			var preferenceCreator = new PreferenceCreator();
			var excludeTradeGroupCreator = new ExcludeTradeGroupCreator();
			var conditionValueTypeCreator = new ConditionValueTypeCreator();
			var conditionCreator = new ConditionCreator(new ConditionValueCreator(conditionValueTypeCreator));

			foreach (var measure in measures)
			{
				Func<string, IEnumerable<RefCusCondition>> conditionCreateFunc = supplementaryUnit =>
				{
					var preferences = preferenceCreator.Get(measure);
					var conditions = conditionCreator.Get(measure, preferences, conditionCodes, supplementaryUnit).ToArray();
					var applicability = ApplicabilityCreator.Get(measure);
					applicability.RefCusExcludedTradeGroups = excludeTradeGroupCreator.Get(measure).ToArray();
					Array.ForEach(conditions, x => x.RefCusApplicabilities = new[] { applicability });
					return conditions;
				};
				yield return (measure, conditionCreateFunc);
			}
		}

		public static string GetSupplementaryUnit(IEnumerable<measure> supplementaryUnitMeasures, string goodsNomenclatureCode)
		{
			var matchedCode = string.Empty;
			var supplementaryUnit = string.Empty;
			foreach (var measure in supplementaryUnitMeasures)
			{
				var matchingCode = measure.goodsNomenclatureCode.TrimEnd("00");
				if (matchingCode.Length > matchedCode.Length && goodsNomenclatureCode.StartsWith(matchingCode, StringComparison.InvariantCultureIgnoreCase))
				{
					matchedCode = matchingCode;
					supplementaryUnit = measure.measureComponent.FirstOrDefault(x => x.dutyExpressionId == "99").measurementUnitCode;
				}
			}
			return supplementaryUnit;
		}
	}
}
