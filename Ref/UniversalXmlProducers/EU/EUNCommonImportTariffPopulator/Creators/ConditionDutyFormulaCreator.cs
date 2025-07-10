using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.RefDbRepo.SEReferenceData.Services;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.EUNCommonImportTariffPopulator
{
	public class ConditionDutyFormulaCreator : IConditionDutyFormulaCreator
	{
		public ConditionDutyFormulaCreator()
		{
			dutyFormulaCreator = new DutyFormulaCreator();
		}
		readonly DutyFormulaCreator dutyFormulaCreator;

		public string Get(string conditionTypeId, IEnumerable<measureCondition> conditions, string reduceIndicator)
		{
			var result = string.Empty;
			switch (conditionTypeId)
			{
				case "F": // The net free at frontier price before duty must be equal to or greater than the minimum price (see components)
				case "L": // CIF price must be higher than the minimum price (see components)
				case "M": // Declared price must be equal to or greater than the minimum price/reference price (see components)
				case "V": // Import price must be equal to or greater than the entry price (see components)
					var useCIF = conditionTypeId == "L";
					foreach (var condition in conditions.OrderByDescending(x => x.sequenceNumber))
					{
						var dutyAmount = dutyFormulaCreator.Get(condition.measureConditionComponent, reduceIndicator, ShouldApplyTheDifference(condition), useCIF);
						var conditionExp = GetCondition(condition);
						result = string.IsNullOrEmpty(conditionExp) ? dutyAmount : $"If({conditionExp}, {dutyAmount}, {result})";
					}
					break;
				case "A": // Presentation of an anti-dumping/countervailing document
					foreach(var condition in conditions.OrderByDescending(x => x.sequenceNumber))
					{
						var dutyAmount = dutyFormulaCreator.Get(condition.measureConditionComponent, reduceIndicator, false, false);
						if(string.IsNullOrEmpty(dutyAmount))
						{
							break;
						}
						var conditionExp = GetConditionForA(condition);
						result = string.IsNullOrEmpty(conditionExp) ? dutyAmount : $"If({conditionExp}, {dutyAmount}, {result})";
					}
					break;
			}
			return result;
		}

		static bool ShouldApplyTheDifference(measureCondition condition)
		{
			switch (condition.actionCode)
			{
				case "01": // Apply the amount of the action (see components)
					return false;
				case "11": // Apply the difference between the amount of the action (see components) and the free at frontier price before duty
					return true;
				default:
					throw new NotImplementedException($"Unknown action code: {condition.actionCode}");
			}
		}

		static string GetCondition(measureCondition condition)
		{
			if (condition.dutyAmountSpecified && condition.dutyAmount == 0)
			{
				return string.Empty;
			}
			else if (condition.monetaryUnitCode == "EUR" && !string.IsNullOrEmpty(condition.measurementUnitCode) && condition.dutyAmountSpecified)
			{
				var unitCode = condition.measurementUnitCode + (condition.measurementUnitQualifierCode ?? string.Empty);
				switch (condition.conditionCodeId)
				{
					case "F":
					case "V":
					case "M":
						return $"VFD/[{unitCode}] >= {condition.dutyAmount}";
					case "L":
						return $"CIF/[{unitCode}] > {condition.dutyAmount}";
					default:
						throw new NotImplementedException($"Unknown condition code id: {condition.conditionCodeId}");
				}
			}
			throw new NotImplementedException($"Unknow condition {condition}");
		}

		static string GetConditionForA(measureCondition condition)
		{
			if (string.IsNullOrEmpty(condition.certificateType) || string.IsNullOrEmpty(condition.certificateCode))
			{
				return string.Empty;
			}
			else
			{
				return $"HAS(\"CERT\",\"{condition.certificateType}{condition.certificateCode}\")";
			}
		}
	}
}
