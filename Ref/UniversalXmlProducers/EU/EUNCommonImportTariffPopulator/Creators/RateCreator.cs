using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using CargoWise.RefDbRepo.SEReferenceData.Services;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.EUNCommonImportTariffPopulator
{
	public class RateCreator : IRateCreator
	{
		public RateCreator(IDutyFormulaCreator dutyFormulaCreator, IConditionDutyFormulaCreator conditionDutyFormulaCreator, IRateUOMCreator rateUOMCreator)
		{
			this.dutyFormulaCreator = dutyFormulaCreator;
			this.conditionDutyFormulaCreator = conditionDutyFormulaCreator;
			this.rateUOMCreator = rateUOMCreator;
		}

		readonly IDutyFormulaCreator dutyFormulaCreator;
		readonly IConditionDutyFormulaCreator conditionDutyFormulaCreator;
		readonly IRateUOMCreator rateUOMCreator;

		public IEnumerable<RefCusRate> Get(measure measure, string rateCode, string rateType, IEnumerable<string> preferences)
		{
			if (!string.IsNullOrEmpty(rateCode))
			{
				if (measure.measureComponent.Any())
				{
					var rateFormula = dutyFormulaCreator.Get(measure.measureComponent, measure.reductionIndicator);
					if (!string.IsNullOrEmpty(rateFormula))
					{
						var uoms = rateUOMCreator.Get(measure.measureComponent);

						foreach (var pref in preferences)
						{
							var rate = Create(measure, rateCode, rateType, rateFormula, pref);
							rate.RefCusRateUOMs = uoms.ToArray();
							yield return rate;
						}
					}
				}
				if (measure.measureCondition.Any())
				{
					var conditionPerTypes = measure.measureCondition.GroupBy(x => x.conditionCodeId);
					foreach (var conditionPerType in conditionPerTypes)
					{
						var rateFormula = conditionDutyFormulaCreator.Get(conditionPerType.Key, conditionPerType, measure.reductionIndicator);
						if (!string.IsNullOrEmpty(rateFormula))
						{
							var uoms = rateUOMCreator.Get(conditionPerType);
							foreach (var pref in preferences)
							{
								var rate = Create(measure, rateCode, rateType, rateFormula, pref);
								rate.RefCusRateUOMs = uoms.ToArray();
								yield return rate;
							}
						}
					}
				}
			}
		}

		static RefCusRate Create(measure measure, string rateCode, string rateType, string dutyFormula, string preference)
		{
			return new RefCusRate
			{
				ZZ2_StartDate = measure.dateStartSpecified ? measure.dateStart : new DateTime(1900, 01, 01),
				ZZ2_EndDate = measure.dateEndSpecified ? measure.dateEnd : new DateTime(2079, 06, 06, 23, 59, 0),
				ZZ2_RateFormula = dutyFormula,
				ZZ2_ZY1_NKRateCode = rateCode,
				ZZ2_ZY1_ZZR_NKRateType = rateType,
				ZZ2_ZZS_NKPreference = preference,
				ZZ2_ZZS_ZZZ_NKDataGrouping = !string.IsNullOrEmpty(preference) ? "EUN" : string.Empty
			};
		}
	}
}
