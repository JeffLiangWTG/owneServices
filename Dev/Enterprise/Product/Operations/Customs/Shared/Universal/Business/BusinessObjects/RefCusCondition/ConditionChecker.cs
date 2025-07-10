using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using static Enterprise.Core.Constants.Customs.Universal;

namespace Enterprise.Customs.Universal
{
	public static class ConditionChecker
	{
		public enum ConditionDirection
		{
			Either = 0,
			Import = 1,
			Export = 2,
		}

		public delegate ZBool EvaluateConditionValue(ZString conditionType, ZString conditionValueType, ZString conditionValue);

		public delegate ZString GetFriendlyConditionValue(ZString conditionType, ZString conditionValueType, ZString conditionValue);

		public static IEnumerable<RefCusCondition> GetApplicableConditions(BusinessObjectFactory factory, TariffView tariff, IZZConditionSelectionCriteria criteria)
		{
			return GetApplicableConditions(factory, tariff, new[] { criteria });
		}

		public static IEnumerable<RefCusCondition> GetApplicableConditions(BusinessObjectFactory factory, TariffView tariff, IEnumerable<IZZConditionSelectionCriteria> criterias)
		{
			if (tariff != null)
			{
				var conditionLoader = new ApplicableConditionLoader(factory);
				var tariffCriteriaSets = criterias.Select(x => new ConditionLoadTariffCriteriaSet(tariff, x));
				return conditionLoader.LoadDataForMultipleCriteriaSets(tariffCriteriaSets);
			}

			return Enumerable.Empty<RefCusCondition>();
		}

		public static (ZString NotMetMessageForNonInformation, ZString NotMetMessageForInformation) CheckConditionsAreMet(
			this TariffView tariff,
			IEnumerable<IZZConditionSelectionCriteria> criterias,
			EvaluateConditionValue evaluationDelegate,
			GetFriendlyConditionValue getFriendlyConditionValue,
			IUniversalRateCalcData calcDataForConditionFormula = null)
		{
			var notMetMessageForNonInformation = new ZStringBuilder();
			var notMetMessageForInformation = new ZStringBuilder();
			var conditions = GetApplicableConditions(tariff.Factory, tariff, criterias);
			conditions.ForEach(condition => condition.FetchForLoadChildEditableObjectsIfNeeded());

			var splitConditions = conditions.Split(x => !x.IsInformationCondition);
			CheckConditionsAreMet(splitConditions.MatchingSet, notMetMessageForNonInformation, evaluationDelegate, getFriendlyConditionValue, calcDataForConditionFormula);
			CheckConditionsAreMet(splitConditions.NonMatchingSet, notMetMessageForInformation, evaluationDelegate, getFriendlyConditionValue, calcDataForConditionFormula);
			return (notMetMessageForNonInformation.ToString().TrimEnd(), notMetMessageForInformation.ToString().TrimEnd());
		}

		public static (ZString NotMetMessageForNonInformation, ZString NotMetMessageForInformation) CheckConditionsAreMetForConditionClass(
			this TariffView tariff,
			ZString conditionClass,
			EvaluateConditionValue evaluationDelegate,
			GetFriendlyConditionValue getFriendlyConditionValue,
			IUniversalRateCalcData calcDataForConditionFormula = null)
		{
			var notMetMessageForNonInformation = new ZStringBuilder();
			var notMetMessageForInformation = new ZStringBuilder();

			var conditions =
				tariff.FilteredConditions.Where(condition => condition.ConditionClass.Equals(conditionClass));

			var splitConditions = conditions.Split(x => !x.IsInformationCondition);
			CheckConditionsAreMet(splitConditions.MatchingSet, notMetMessageForNonInformation, evaluationDelegate, getFriendlyConditionValue, calcDataForConditionFormula);
			CheckConditionsAreMet(splitConditions.NonMatchingSet, notMetMessageForInformation, evaluationDelegate, getFriendlyConditionValue, calcDataForConditionFormula);
			return (notMetMessageForNonInformation.ToString().TrimEnd(), notMetMessageForInformation.ToString().TrimEnd());
		}

		public static CargoWise.ComponentModel.INotificationType CheckConditionSeverity(RefCusCondition condition)
		{
			return condition.ZX1_Severity == Constants.ConditionSeverity.MSG ? NotificationType.MessageError : NotificationType.Warning;
		}

		public static bool CheckConditionApplicabilitiesExcludeCountry(RefCusCondition condition, ZString tradeGroupCountry, ZDateTime assessmentDate)
		{
			return condition.Applicabilities.Any(x => !x.IsApplicable(tradeGroupCountry, assessmentDate));
		}

		static void CheckConditionsAreMet(IEnumerable<RefCusCondition> conditions, ZStringBuilder messageAboutConditionNotMet,
			EvaluateConditionValue evaluationDelegate,
			GetFriendlyConditionValue getFriendlyConditionValue,
			IUniversalRateCalcData calcDataForConditionFormula = null)
		{
			foreach (var groupByConditionClass in conditions
				.GroupBy(x => new { x.ZX1_ZZZ_NKDataGrouping, x.ConditionClass })
				.OrderBy(x => x.Key.ConditionClass))
			{
				var resultForConditionType = new ZStringBuilder();
				foreach (var groupByConditionType in groupByConditionClass
					.GroupBy(x => x.ConditionType)
					.OrderBy(x => x.Key))
				{
					var groupByLogicalAND = groupByConditionType.GroupBy(x => x.ZX1_LogicalANDWithinGroup).ToArray();
					if (groupByLogicalAND.All(andGroup => andGroup.Any(x => x.ShouldStop(evaluationDelegate, calcDataForConditionFormula))))
					{
						resultForConditionType.AppendLine(ZString.Format("    {0}:", groupByConditionType.First().ConditionTypeDescription));
						var orResult = new ZStringBuilder();
						foreach (var stoppingConditionGroup in groupByLogicalAND.OrderBy(x => x.Key))
						{
							var innerResult = new ZStringBuilder();
							foreach (var condition in stoppingConditionGroup.Where(x => x.ShouldStop(evaluationDelegate, calcDataForConditionFormula))
								.OrderBy(x => x.ConditionTypeDescription).ThenBy(x => x.ZX1_Comment))
							{
								var conditionValues = condition.GetLogicalRequirements(getFriendlyConditionValue);
								var conditionValuesResult =
									conditionValues.IsEmpty ? string.Empty : ": " + conditionValues;

								var source = condition.ZX1_Source;
								var sourceResult = source.IsEmpty
									? string.Empty
									: Res.GetString("99F8EDC7-7CA7-4867-B6F4-AC85212BE6CE",
										"\r\n            (Please refer to: {0})", source);

								innerResult.Append(ZString.Format("        {0}{1}{2}", condition.ZX1_Comment,
									conditionValuesResult, sourceResult));
							}

							orResult.Append(innerResult.ToStringWithDelimiterBetweenAppends(Res.GetString("AE1E2B33-7C10-4886-AC43-F6B8AB718D85", " AND\r\n")));
						}
						resultForConditionType.AppendLine(orResult.ToStringWithDelimiterBetweenAppends(Res.GetString("7C1FCF37-8E86-4FB0-B046-5886AFBB6ED0", " OR\r\n")));
					}
				}

				AppendNotMetConditionsMessage(messageAboutConditionNotMet, resultForConditionType, groupByConditionClass.Key.ConditionClass);
			}
		}

		static void AppendNotMetConditionsMessage(ZStringBuilder messageForConditionClass, ZStringBuilder messageForConditionType, ZString conditionClass)
		{
			if (!messageForConditionType.IsEmpty)
			{
				messageForConditionClass.AppendLine(Res.GetString("668FC091-E22A-449F-9866-42772B2492F7",
					"The {0} condition is not satisfied",
					GetConditionClassDescription(conditionClass)));
				messageForConditionClass.AppendLine(messageForConditionType.ToString());
			}
		}

		static ZString GetConditionClassDescription(ZString conditionClass)
		{
			var result = ZString.Empty;
			switch (conditionClass)
			{
				case RefCusConditionTypes.ConditionClass.Control:
					result = Constants.ConditionClassDescription.Control;
					break;
				case RefCusConditionTypes.ConditionClass.Rate:
					result = Constants.ConditionClassDescription.Rate;
					break;
				case RefCusConditionTypes.ConditionClass.Class:
					result = Constants.ConditionClassDescription.Class;
					break;
				case RefCusConditionTypes.ConditionClass.VAT:
					result = Constants.ConditionClassDescription.VAT;
					break;
			}
			return result;
		}

		static ZString GetLogicalRequirements(this RefCusCondition condition, GetFriendlyConditionValue getFriendlyConditionValue)
		{
			var groups = condition.ConditionValues.GroupBy(x => x.ZX3_LogicalORWithinGroup);
			var hasGroups = groups.Count() > 1;

			var result = JoinConditionValues(Res.GetString("58a90a4a-8d69-4ce5-8b94-e2e8a55251cd", " and "),
									groups.OrderBy(x => x.Key)
										.Select(g => JoinConditionValues(Res.GetString("06ee887e-7935-4787-b8d0-8ec7de83456f", " or "),
											g.OrderBy(i => i.IsInformationConditionValue)
												.ThenBy(x => x.ZX3_Value)
												.Select(v => v.GetLogicalRequirement(getFriendlyConditionValue)),
											hasGroups)),
									false);

			if (!result.IsEmpty && condition.ZX1_ConditionValueTrueMeansStop)
			{
				result = Res.GetString("cae521cf-7d85-4bc1-a14e-c2c6536ab354", "not ({0})", result);
			}

			return result;
		}

		static ZString GetLogicalRequirement(this RefCusConditionValue conditionValue, GetFriendlyConditionValue getFriendlyConditionValue)
		{
			var value = conditionValue.ZX3_Value;
			return conditionValue.ConditionValueType.ZX4_IsFormula ? ZString.Format("({0})", value) : (getFriendlyConditionValue?.Invoke(conditionValue.Condition.ConditionType, conditionValue.ConditionValueType.ZX4_ValueType, value) ?? value);
		}

		static ZString JoinConditionValues(ZString separator, IEnumerable<ZString> conditionalValues, bool groupingWithParenthesis)
		{
			var values = conditionalValues.Where(x => !x.IsEmpty).ToArray();
			return values.Length > 1 ? ZString.Format(groupingWithParenthesis ? "({0})" : "{0}", ZString.Join(separator, values)) : values.FirstOrDefault();
		}
	}
}
