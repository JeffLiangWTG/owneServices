using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.RefDbRepo.SEReferenceData.Services;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.EUNCommonImportTariffPopulator
{
	public class DutyFormulaCreator : IDutyFormulaCreator
	{
		public string Get(IEnumerable<measureConditionComponent> components, string reduceIndicator, bool applyDifference, bool useCIF)
		{
			return Get(components.Select(x => new DutyComponent(x, applyDifference, useCIF)), reduceIndicator);
		}

		public string Get(IEnumerable<measureComponent> components, string reduceIndicator)
		{
			return Get(components.Select(x => new DutyComponent(x)), reduceIndicator);
		}

		static string Get(IEnumerable<DutyComponent> components, string reduceIndicator)
		{
			var result = string.Empty;
			var isFormulaOpen = false;
			foreach (var component in components.OrderBy(x => x.dutyExpressionId))  // assumption: require order by id to build expression
			{
				// TARIC business Codes - Duty Expressions
				switch (component.dutyExpressionId)
				{
					case "01": // % or amount
						result = GetPercentageOrAmount(component);
						break;
					case "02": // minus % or amount
						result = $"{result} - {GetPercentageOrAmount(component)}";
						break;
					case "04": // + % or amount
					case "19": // + % or amount
					case "20": // + % or amount
						result = $"{result} + {GetPercentageOrAmount(component)}";
						break;
					case "12": // + agricultural component
						result = $"{result} + {GetAgricultureComponent("EA", reduceIndicator)}";
						break;
					case "14": // + reduced agricultural component
						result = $"{result} + {GetAgricultureComponent("EAR", reduceIndicator)}";
						break;
					case "15": // Minimum
						if (isFormulaOpen)
						{
							result = $"{result})";
						}
						result = $"MAX({result}, {GetPercentageOrAmount(component)}";
						isFormulaOpen = true;
						break;
					case "17": // Maximum
					case "35": // Maximum
						if (isFormulaOpen)
						{
							result = $"{result})";
						}
						result = $"MIN({result}, {GetPercentageOrAmount(component)}";
						isFormulaOpen = true;
						break;
					case "21": // + additional duty on sugar
						result = $"{result} + {GetAgricultureComponent("ADSZ", reduceIndicator)}";
						break;
					case "25": // + reduced additional duty on sugar
						result = $"{result} + {GetAgricultureComponent("ADSZR", reduceIndicator)}";
						break;
					case "27": // + additional duty on flour
						result = $"{result} + {GetAgricultureComponent("ADFM", reduceIndicator)}";
						break;
					case "29": // + reduced additional duty on flour
						result = $"{result} + {GetAgricultureComponent("ADFMR", reduceIndicator)}";
						break;
					case "36": // minus % CIF
						result = $"{result} - {GetPercentageOrAmount(component)}";
						break;
					case "37": // nothing
						result = "0";
						break;
					case "99": // Supplementary unit
						result = string.Empty;
						break;
					default:
						throw new InvalidOperationException($"Unknown duty expression id :{component.dutyExpressionId}");
				}
			}
			if (isFormulaOpen)
			{
				result = $"{result})";
			}
			return result;
		}

		static string GetPercentageOrAmount(DutyComponent component)
		{
			if (component.dutyAmount == 0)
			{
				return "0";
			}
			var dutyValue = component.useCIF ? "CIF" : "VFD";
			if (string.IsNullOrEmpty(component.monetaryUnitCode) && string.IsNullOrEmpty(component.measurementUnitCode)) // Percentage amount
			{
				return $"{dutyValue} * {component.dutyAmount / 100}";
			}
			if (!string.IsNullOrEmpty(component.measurementUnitCode))
			{
				var unitCode = component.measurementUnitCode + (component.measurementUnitQualifierCode ?? string.Empty);
				return component.applyDifference ? $"({component.dutyAmount} - {dutyValue}/[{unitCode}]) * [{unitCode}]" :
					$"{component.dutyAmount} * [{unitCode}]";
			}
			throw new InvalidOperationException($"Unexpected measure component:{component}");
		}

		static string GetAgricultureComponent(string agriCom, string reduceIndication)
		{
			return string.IsNullOrEmpty(reduceIndication) ? $"#{agriCom}#" : $"#{agriCom}({reduceIndication})#";
		}

		class DutyComponent
		{
			public DutyComponent(measureConditionComponent component, bool applyDifference, bool useCIF)
			{
				dutyAmount = component.dutyAmount;
				dutyExpressionId = component.dutyExpressionId;
				monetaryUnitCode = component.monetaryUnitCode;
				measurementUnitCode = component.measurementUnitCode;
				measurementUnitQualifierCode = component.measurementUnitQualifierCode;
				this.applyDifference = applyDifference;
				this.useCIF = useCIF;
			}

			public DutyComponent(measureComponent component)
			{
				dutyAmount = component.dutyAmount;
				dutyExpressionId = component.dutyExpressionId;
				monetaryUnitCode = component.monetaryUnitCode;
				measurementUnitCode = component.measurementUnitCode;
				measurementUnitQualifierCode = component.measurementUnitQualifierCode;
			}

			public string dutyExpressionId;
			public decimal dutyAmount;
			public string monetaryUnitCode;
			public string measurementUnitCode;
			public string measurementUnitQualifierCode;
			public bool applyDifference; // Apply the difference between the amount of the action and the price before duty
			public bool useCIF; // use CIF instead of VFD in the formula
		}
	}
}
