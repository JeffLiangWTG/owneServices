using System.Collections.Generic;
using System.Linq;
using CargoWise.RefDbRepo.Common.Argument;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using CargoWise.RefDbRepo.SEReferenceData.Services;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.EUNCommonImportTariffPopulator
{
	public class ConditionValueCreator : IConditionValueCreator
	{
		public ConditionValueCreator(IConditionValueTypeCreator valueTypeCreator)
		{
			this.valueTypeCreator = valueTypeCreator;
		}

		IConditionValueTypeCreator valueTypeCreator;

		public IEnumerable<RefCusConditionValue> Get(IGrouping<string, measureCondition> conditions, string supplementaryUnit)
		{
			if (conditions.Key == "R")
			{
				var zx3_value = string.Empty;
				foreach (var condition in conditions.OrderBy(x => x.sequenceNumber))
				{
					if (condition.ShouldAllow())
					{
						Argument.NotNullOrEmpty(supplementaryUnit, nameof(supplementaryUnit));
						zx3_value += string.IsNullOrEmpty(zx3_value) ? "" : " & ";
						zx3_value += $"[{condition.measurementUnitCode}]/[{supplementaryUnit}] >= {condition.dutyAmount}";
					}
					else if (condition.dutyAmountSpecified && condition.dutyAmount > 0)
					{
						Argument.NotNullOrEmpty(supplementaryUnit, nameof(supplementaryUnit));
						zx3_value += string.IsNullOrEmpty(zx3_value) ? "" : " & ";
						zx3_value += $"[{condition.measurementUnitCode}]/[{supplementaryUnit}] < {condition.dutyAmount}";
					}
				}
				if (!string.IsNullOrEmpty(zx3_value))
				{
					yield return new RefCusConditionValue
					{
						ZX3_Value = zx3_value,
						ZX3_ZX4_NKValueType = "FRM",
						ZX3_LogicalORWithinGroup = 0
					};
				}
			}
			else if (conditions.Key == "U")
			{
				var zx3_value = string.Empty;
				foreach (var condition in conditions.OrderBy(x => x.sequenceNumber))
				{
					if (condition.ShouldAllow())
					{
						zx3_value += string.IsNullOrEmpty(zx3_value) ? "" : " & ";
						zx3_value += $"VFD/[{condition.measurementUnitCode}] > {condition.dutyAmount}";
					}
					else if (condition.dutyAmountSpecified && condition.dutyAmount > 0)
					{
						zx3_value += string.IsNullOrEmpty(zx3_value) ? "" : " & ";
						zx3_value += $"VFD/[{condition.measurementUnitCode}] <= {condition.dutyAmount}";
					}
				}
				if (!string.IsNullOrEmpty(zx3_value))
				{
					yield return new RefCusConditionValue
					{
						ZX3_Value = zx3_value,
						ZX3_ZX4_NKValueType = "FRM",
						ZX3_LogicalORWithinGroup = 0
					};
				}
			}
			else
			{
				foreach (var condition in conditions.OrderBy(x => x.sequenceNumber))
				{
					var zx3_value = GetValue(condition);
					var valueType = valueTypeCreator.Get(condition);
					if (!string.IsNullOrEmpty(zx3_value) && !string.IsNullOrEmpty(valueType))
					{
						yield return new RefCusConditionValue
						{
							ZX3_Value = zx3_value,
							ZX3_ZX4_NKValueType = valueType,
							ZX3_LogicalORWithinGroup = 0
						};
					}
				}
			}
		}

		static string GetValue(measureCondition condition)
		{
			switch (condition.conditionCodeId)
			{
				case "A":
					if (condition.WithCertificate())
					{
						return condition.certificateType + condition.certificateCode;
					}
					else
					{
						return "Apply the mentioned duty";
					}
				case "E":
				case "I":
					if (condition.WithCertificate())
					{
						return condition.certificateType + condition.certificateCode;
					}
					else if (condition.WithDutyAmount())
					{
						return $"VFD/[{condition.measurementUnitCode}] <= {condition.dutyAmount}";
					}
					else
					{
						return string.Empty;
					}
				case "B":
				case "C":
				case "H":
				case "Q":
				case "Y":
				case "YA":
				case "YB":
				case "YC":
				case "YD":
					if (condition.WithCertificate())
					{
						return condition.certificateType + condition.certificateCode;
					}
					else
					{
						return string.Empty;
					}
				default:
					return string.Empty;
			}
		}
	}
}
