using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.RefDbRepo.Common.SafeDataClient;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using CargoWise.RefDbRepo.SEReferenceData.Services;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.EUNCommonImportTariffPopulator
{
	public class ConditionCreator
	{
		public ConditionCreator(IConditionValueCreator conditionValueCreator)
		{
			this.conditionValueCreator = conditionValueCreator;
		}

		IConditionValueCreator conditionValueCreator;

		public IEnumerable<RefCusCondition> Get(measure measure, IEnumerable<string> preferences, IEnumerable<measureConditionCode> conditionCodes, string supplementaryUnit)
		{
			if (measure.measureCondition.Any())
			{
				var conditionPerTypes = measure.measureCondition.GroupBy(x => x.conditionCodeId);
				foreach (var conditionPerType in conditionPerTypes)
				{
					var comment = GetComment(conditionPerType.Key, conditionCodes);
					foreach (var pref in preferences)
					{
						var conditionValues = conditionValueCreator.Get(conditionPerType, supplementaryUnit);
						if (conditionValues.Any())
						{
							yield return Create(measure, pref, comment, conditionValues.ToArray());
						}
					}
				}
			}
		}

		static RefCusCondition Create(measure measure, string preference, string comment, RefCusConditionValue[] conditionValues)
		{
			return new RefCusCondition
			{
				ZX1_StartDate = measure.dateStartSpecified ? measure.dateStart : new DateTime(1900, 01, 01),
				ZX1_EndDate = measure.dateEndSpecified ? measure.dateEnd.MidnightToEndOfDay() : new DateTime(2079, 06, 06, 23, 59, 0),
				ZX1_ZX2_NKConditionType = measure.measureType,
				ZX1_ZZS_NKPreference = preference,
				ZX1_ZZS_ZZZ_NKDataGrouping = !string.IsNullOrEmpty(preference) ? "EUN" : string.Empty,
				ZX1_LogicalANDWithinGroup = 0,
				ZX1_Comment = comment,
				RefCusConditionValues = conditionValues
			};
		}

		static string GetComment(string conditionCodeId, IEnumerable<measureConditionCode> conditionCodes)
		{
			var conditionCode = conditionCodes.FirstOrDefault(x => x.conditionCode == conditionCodeId);
			var codeDescription = conditionCode != null ? conditionCode.measureConditionCodeDescription.FirstOrDefault(x => x.languageId == "EN").description : string.Empty;
			return $"Condition {conditionCodeId}:{codeDescription}";
		}
	}
}
