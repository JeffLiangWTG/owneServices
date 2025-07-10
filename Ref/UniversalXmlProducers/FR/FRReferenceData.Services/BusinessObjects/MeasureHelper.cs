using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;

namespace CargoWise.RefDbRepo.FRReferenceData.Services
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Maintainability", "CA1502:Avoid excessive complexity", Justification = "Legacy.")]
	public static class MeasureHelper
	{
		#region Generate Formula from Components and Conditions

		public static string GenerateFormula(Measure measure, bool makeitReadable)
		{
			var result = makeitReadable ? measure.PlainTextFormula : measure.Formula;

			var conditionGroupResults = new List<string>();
			var conditionGroupLogics = new List<string>();
			var conditionGroupLogicMemberCounts = new List<int>();
			var measureConditions = measure.Conditions;
			if (measureConditions?.Any() ?? false)
			{
				if (measureConditions.Count == 1)
				{
					if (string.IsNullOrEmpty(result))
					{
						result = makeitReadable ? measureConditions.First().PlainTextFormula : measureConditions.First().Formula;
					}
				}
				else
				{
					var conditionsOfInterest = measureConditions.Where(x => x.IsRateFormula || string.IsNullOrEmpty(x.ActionCode));
					var groupedConditions = conditionsOfInterest.OrderBy(x => x.SequenceNumber).GroupBy(x => x.Formula + x.ActionCode + x.Code).ToList();

					foreach (var conditionGroup in groupedConditions)
					{
						//Conditions logic
						var conditionGroupLogic = string.Empty;
						var conditionGroupLogicMemberCount = 0;
						foreach (var condition in conditionGroup)
						{
							var currentConditionLogic = GetConditionLogic(condition, makeitReadable);
							if (condition.SequenceNumber > 1 && !string.IsNullOrEmpty(currentConditionLogic) && !IsFallbackAction(condition.ActionCode))
							{
								conditionGroupLogic += makeitReadable ? " ou " : " || ";
							}
							conditionGroupLogic += currentConditionLogic;
							conditionGroupLogicMemberCount += 1;
						}

						//Conditions result
						var conditionGroupResult = result;

						if (string.IsNullOrEmpty(result))
						{
							conditionGroupResult = makeitReadable ? conditionGroup.First().PlainTextFormula : conditionGroup.First().Formula;
						}
						if (makeitReadable)
						{
							conditionGroupResult = !string.IsNullOrEmpty(conditionGroupResult) && !conditionGroupLogic.Contains("percevoir") ? $"le montant à percevoir est {conditionGroupResult}" : conditionGroupResult;
							conditionGroupLogic = conditionGroupLogic.Contains("percevoir") && string.IsNullOrEmpty(conditionGroupResult) ? string.Empty : conditionGroupLogic; //Case when we're supposed to calculate an amount, but there is no condition component to do so!
						}

						//Array population
						conditionGroupLogics.Add(conditionGroupLogic);
						conditionGroupResults.Add(conditionGroupResult);
						conditionGroupLogicMemberCounts.Add(conditionGroupLogicMemberCount);
					}

					switch (conditionGroupLogics.Count)
					{
						case 1:
							result = makeitReadable ? $"Si {conditionGroupLogics.First()} alors {conditionGroupResults.First()}" : $"{funcIf}({conditionGroupLogics.First()}, {conditionGroupResults.First()})";

							break;
						case 2:
							if (string.IsNullOrEmpty(conditionGroupResults.Last()))//case when fallback is pure regulatory
							{
								result = makeitReadable ? $"Si {conditionGroupLogics.First()} alors {conditionGroupResults.First()} sinon {conditionGroupLogics.Last()}" : $"{funcIf}({conditionGroupLogics.First()}, {conditionGroupResults.First()})";
							}
							else //case when fallback is a calculation
							{
								result = makeitReadable ? $"Si {conditionGroupLogics.First()} alors {conditionGroupResults.First()} sinon {conditionGroupResults.Last()}" : $"{funcIf}({conditionGroupLogics.First()}, {conditionGroupResults.First()}, {conditionGroupResults.Last()})";
							}

							break;
						case 3:
							var firstConditionGroupLogic = conditionGroupLogicMemberCounts.First() > 1 ? $"({conditionGroupLogics.First()})" : conditionGroupLogics.First();
							var secondConditionGroupLogic = conditionGroupLogicMemberCounts[1] > 1 ? $"({conditionGroupLogics[1]})" : conditionGroupLogics[1];

							if (string.IsNullOrEmpty(conditionGroupResults.Last()) || conditionGroupResults.Last() == conditionGroupResults[1])//case when fallback is pure regulatory
							{
								result = makeitReadable ? $"Si {firstConditionGroupLogic} et {secondConditionGroupLogic} alors {conditionGroupResults[1]} sinon {conditionGroupLogics.Last()}" : $"{funcIf}({firstConditionGroupLogic} && {secondConditionGroupLogic}, {conditionGroupResults.First()})";
							}
							else //case when fallback is a calculation
							{
								result = makeitReadable ? $"Si {firstConditionGroupLogic} et {secondConditionGroupLogic} alors {conditionGroupResults[1]} sinon {conditionGroupResults.Last()}" : $"{funcIf}({firstConditionGroupLogic} && {secondConditionGroupLogic}, {conditionGroupResults[1]}, {conditionGroupResults.Last()})";
							}

							break;
					}
				}
			}

			var shouldExist = (measure.Components?.Any() ?? false) || (measure.Conditions?.Any() ?? false);
			if (shouldExist && string.IsNullOrEmpty(result))
			{
				result = "0";
			}

			bool hasOperation = result.Contains("*") || result.Contains("/");

			if (!hasOperation && !makeitReadable && result != "0" && result != "{\"Precalcule\"}" && !result.EndsWith(")", System.StringComparison.InvariantCultureIgnoreCase))
			{
				result = $"{result} * [FLAT]";
			}

			if (result == "précalculé")
			{
				result = "Précalculée";
			}

			result = result.Replace("( ", "(").Replace("  ", " ").Replace("sinon Si", "sinon si");

			return result;
		}

		static string GetConditionLogic(Condition condition, bool makeItReadable)
		{
			var result = string.Empty;

			var actionCode = condition.ActionCode;
			var isFallbackCondition = IsFallbackAction(actionCode);
			var op = string.Empty;
			var amountText = string.Empty;
			var additionalText = GetConditionActionCodeDescription(actionCode);
			switch (condition.Code)
			{
				case "2": //Comparison on supp units
					op = RevertOperationIfNecessary(actionCode, ">");
					amountText = GetAmountText(condition.Amount, makeItReadable, false);
					if ((!string.IsNullOrEmpty(op) && op.Contains("<") && amountText == "0"))
					{
						result = string.Empty;
					}
					else
					{
						if (makeItReadable)
						{
							if (isFallbackCondition || IsFallbackValue(amountText))
							{
								result = string.Empty;
							}
							else
							{
								result = $"la quantité est {(op.Contains(">") ? "supérieure" : "inférieure")} {(op.Contains("=") ? "ou égale" : string.Empty)} à {amountText}";
							}
						}
						else
						{
							if (isFallbackCondition || IsFallbackValue(amountText))
							{
								result = string.Empty;
							}
							else
							{
								result = $"[{condition.MeasurementCode}{condition.Qualifier}] {op} {amountText}";
							}
						}
					}
					break;
				case "L":   //CIF price must be higher than the minimum price (see components) //kept for record, not used in FR tariff
					amountText = GetAmountText(condition.Amount, makeItReadable, true);
					result = !isFallbackCondition ? $"{funcIf}({CIF}/[{condition.MeasurementCode}{condition.Qualifier}] >= {amountText})" : string.Empty;
					break;
				case "M":   //Declared price must be equal to or greater than the minimum price/reference price (see components) //kept for record, not used in FR tariff
				case "F":   //The net free at frontier price before duty must be equal to or greater than the minimum price (see components) //kept for record, not used in FR tariff
				case "V":   //Import price must be equal to or greater than the entry price (see components) //kept for record, not used in FR tariff
					amountText = GetAmountText(condition.Amount, makeItReadable, true);
					result = !isFallbackCondition ? $"{funcIf}({VFD}/[{condition.MeasurementCode}{condition.Qualifier}] >= {amountText})" : string.Empty;
					break;
				case "1":
				case "3":
				case "4":
				case "A":
				case "B":
				case "C":
				case "H":
					if (makeItReadable)
					{
						if (isFallbackCondition)
						{
							result = $"{additionalText}";
						}
						else
						{
							result = string.IsNullOrEmpty(condition.DocumentCode) ? string.Empty : $"présentation du document {condition.DocumentCode}";
						}
					}
					else
					{
						if (isFallbackCondition)
						{
							result = $"{amountText}";
						}
						else
						{
							result = string.IsNullOrEmpty(condition.DocumentCode) ? string.Empty : $"{funcHas}(\"CERT\", \"{condition.DocumentCode}\")";
						}
					}

					break;

				// Not used as yet, may need to be included after further investigation
				//case "E":   //The quantity or the price per unit declared, as appropriate, is equal or less than the specified maximum, or presentation of the required document
				//case "G":   //The CIF price plus the duty to be paid/ton must be equal to or greater than the minimum price (see components)
				//case "I":   //The quantity or the price per unit declared, as appropriate, is equal or less than the specified maximum, or presentation of the required document
				//case "N":   //The CIF price before duty must be equal to or greater than the minimum price(see components)
				case "R":   //Ratio "net weight/supplementary unit" is equal to or higher than the condition amount //kept for record, not used in FR tariff
				case "U":   //Ratio "declared value/supplementary unit" should be higher than the condition amount  //kept for record, not used in FR tariff
					op = RevertOperationIfNecessary(condition.ActionCode, ">=");
					amountText = GetAmountText(condition.Amount, makeItReadable, true);
					result = !string.IsNullOrEmpty(op) && op.Contains("<") && amountText == "0" ? string.Empty : $"[{condition.MeasurementCode}{condition.Qualifier}]/[VFD] {op} {amountText}";
					break;
			}

			return result;
		}

		public static bool IsRateFormula(IEnumerable<Condition> conditions, string conditionCode)
		{
			return conditions.Any(x => x.Code == conditionCode && actionCodesForRates.Contains(x.ActionCode));
		}

		static string[] actionCodesForRates = new[] { "01", "02", "03", "11", "12", "13", "15", "27" };

		public static string BuildFormulaFromComponents(IEnumerable<Component> components, bool makeItReadable, string actionCode = "")
		{
			var componentFormula = string.Empty;

			if (components?.Any() ?? false)
			{
				var parts = new List<string>();
				foreach (var component in components.OrderBy(x => x.Code))
				{
					if (string.IsNullOrEmpty(actionCode))
					{
						parts.Add(GetDutyExpression(component, makeItReadable));
					}
					else
					{
						parts.Add(GetActionExpression(component, actionCode, makeItReadable));
					}

					if (!makeItReadable)
					{
						var func = GetFunction(component.Code); //Max or Min

						if (!string.IsNullOrEmpty(func))
						{
							var part = $"{func}({placeholder1}, {placeholder2})";
							parts.Insert(0, part);
						}
					}
				}

				componentFormula = ComposeParts(parts);
			}

			return componentFormula;
		}

		static string ComposeParts(List<string> parts)
		{
			int i = parts.Count - 1;
			while (parts.Any() && i > 0)
			{
				var partIsNotComplete = parts[i].StartsWith(opAdd, System.StringComparison.InvariantCultureIgnoreCase) || parts[i].StartsWith(opSubtract, System.StringComparison.InvariantCultureIgnoreCase) || parts[i].StartsWith(opMultiply, System.StringComparison.InvariantCultureIgnoreCase);
				if (partIsNotComplete)
				{
					parts[i - 1] = $"{parts[i - 1]} {parts[i]}";
					parts[i] = string.Empty;
				}

				i--;
			}

			parts.RemoveAll(x => string.IsNullOrEmpty(x));
			var complete = parts.Where(x => !x.Contains(placeholderPrefix)).ToList();
			parts.RemoveAll(x => !x.Contains(placeholderPrefix));
			var funcs = parts.Where(x => x.Contains(placeholder2)).ToList();
			parts.RemoveAll(x => x.Contains(placeholder2));

			bool subFound = true;
			i = 0;
			while (parts.Any() && subFound)
			{
				subFound = false;
				i = 0;
				if (complete.Any() && parts.Any() && parts[i].Contains(placeholder1))
				{
					parts[i] = parts[i].Replace(placeholder1, complete[0]);
					complete.RemoveAt(0);
					subFound = true;

					if (!parts[i].Contains(placeholderPrefix))
					{
						complete.Add(parts[i]);
						parts.RemoveAt(i);
					}
				}
			}

			while (funcs.Any() && subFound)
			{
				subFound = false;
				i = funcs.Count - 1;

				if (complete.Any() && funcs.Any() && funcs[i].Contains(placeholder1))
				{
					funcs[i] = funcs[i].Replace(placeholder1, complete[0]);
					complete.RemoveAt(0);
					subFound = true;

					if (!funcs[i].Contains(placeholderPrefix))
					{
						complete.Add(funcs[i]);
						funcs.RemoveAt(i);
					}
				}

				if (!subFound && complete.Any() && funcs.Any() && funcs[i].Contains(placeholder2))
				{
					funcs[i] = funcs[i].Replace(placeholder2, complete[0]);
					complete.RemoveAt(0);
					subFound = true;

					if (!funcs[i].Contains(placeholderPrefix))
					{
						complete.Add(funcs[i]);
						funcs.RemoveAt(i);
					}
				}
			}

			return string.Join(" - ", complete).Trim();
		}

		static string GetDutyExpression(Component component, bool makeItReadable)
		{
			switch (component.Code)
			{
				case "01": //% or amount
					return DutyExpression(component, makeItReadable, isRequired: true, hasPlaceholder: false);
				case "02": //minus % or amount
					return DutyExpression(component, makeItReadable, opSubtract);
				case "04": //+ % or amount
				case "19": //+ % or amount
				case "20": //+ % or amount
					return DutyExpression(component, makeItReadable, opAdd);

				case DutyExpressionIds.AgricultureComponent: //+ agricultural component
					return DutyExpression(component, makeItReadable, opAdd, unitOverride: AgricultureComponent);
				case DutyExpressionIds.ReducedAgricultureComponent: //+ reduced agricultural component
					return DutyExpression(component, makeItReadable, opAdd, unitOverride: ReducedAgricultureComponent);
				case DutyExpressionIds.AdditionalDutyOnSugar: //+ additional duty on sugar
					return DutyExpression(component, makeItReadable, opAdd, unitOverride: AdditionalDutyOnSugar);
				case DutyExpressionIds.ReducedAdditionalDutyOnSugar: //+ reduced additional duty on sugar
					return DutyExpression(component, makeItReadable, opAdd, unitOverride: ReducedAdditionalDutyOnSugar);
				case DutyExpressionIds.AdditionalDutyOnFlour: //+ additional duty on flour
					return DutyExpression(component, makeItReadable, opAdd, unitOverride: AdditionalDutyOnFlour);
				case DutyExpressionIds.ReducedAdditionalDutyOnFlour: //+ reduced additional duty on flour
					return DutyExpression(component, makeItReadable, opAdd, unitOverride: ReducedAdditionalDutyOnFlour);

				case "15": //Minimum
					return makeItReadable ? DutyExpression(component, true, plainTextMinimum, hasPlaceholder: false) : DutyExpression(component, false, hasPlaceholder: false);
				case "17": //Maximum
				case "35": //Maximum
					return makeItReadable ? DutyExpression(component, true, plainTextMaximum, hasPlaceholder: false) : DutyExpression(component, false, hasPlaceholder: false);
				case "36": //minus % CIF
					return DutyExpression(component, makeItReadable, opSubtract, priceCode: CIF);

				case "37": //(nothing)
				case "99": //Supplementary unit
				case "PC": //Precalculated
					return makeItReadable ? "précalculé" : @"{""Precalcule""}";
				case "DS":
				case "NP"://Suspension of duty
					return "0";
				default:
					return string.Empty;
			}
		}

		static string GetActionExpression(Component component, string actionCode, bool makeItReadable)
		{
			switch (actionCode)
			{
				case "01":  //Apply the amount of the action (see components)
					return GetDutyExpression(component, makeItReadable);
				case "02":  //Apply the difference between the amount of the action (see components) and the price at import
				case "11":  //Apply the difference between the amount of the action (see components) and the free at frontier price before duty
				case "15": //Apply the difference between the amount of the action (see components) and the price augmented with the countervailing duty (3,8%)
					return ApplyDifference(component, makeItReadable, VFD); //kept for record, not used in FR tariff
				case "03":  //Apply the difference between the amount of the action (see components) and CIF price
				case "12":  //Apply the difference between the amount of the action (see components) and the CIF price before duty
				case "13":  //Apply the difference between the amount of the action (see components) and the CIF price augmented with the duty to be paid per tonne
					return ApplyDifference(component, makeItReadable, CIF); //kept for record, not used in FR tariff
				case "04": //Free circulation is forbidden
					return "La mise en libre pratique n’est pas admise";
				case "05":
					return "Exportation non autorisée";
				case "06":
					return "Importation non autorisée";
				case "07":
					return "Mesure non applicable";
				case "08":
					return "La position déclarée n’est pas permise";
				case "09":
					return "Importation/exportation non autorisée  après controle";
				case "14":
					return "L’exemption/réduction du droit antidumping ne s’applique pas";
				case "16":
					return "Restitution à l’exportation non applicable";
				case "24":
					return "Mise en libre pratique autorisée";
				case "25": //
					return "Exportation autorisée";
				case "26": //
					return "Importation autorisée";
				case "27": //
					return "Appliquer le droit mentionné";
				case "28": //
					return "Sous-position déclarée autorisée";
				case "29": //
					return "Importation/exportation autorisée après controle";
				case "34": //
					return "Appliquer l’exemption/la réduction du droit antidumping";
				case "36": //
					return "Appliquer la restitution à l’exportation";
				case "MI":
					return "Mise à la consommation interdite";
				case "PH":
					return "Prohibition";
				default:
					return string.Empty;
			}
		}

		static string ApplyDifference(Component component, bool makeItReadable, string priceCode)
		{
			var unit = makeItReadable ? $"[{component.MeasurementCodeDescription}{component.QualifierDescription}]" : $"[{component.MeasurementCode}{component.Qualifier}]";
			return $"({component.Amount:0.000####} - {priceCode}/{unit}) * {unit}";
		}

		static string DutyExpression(Component component, bool makeItReadable, string operation = "", bool isRequired = false, string unitOverride = "", bool hasPlaceholder = true, string priceCode = VFD)
		{
			var result = string.Empty;

			bool hasOverrideUnit = !string.IsNullOrEmpty(unitOverride);
			bool hasCurrency = !string.IsNullOrEmpty(component.Currency);
			bool hasDutyAmount = component.Amount.HasValue && component.Amount > 0;
			if (!hasDutyAmount && !hasOverrideUnit)
			{
				return isRequired ? "0" : string.Empty;
			}

			var ph = hasPlaceholder ? $"{placeholder1} " : string.Empty;
			var op = string.IsNullOrEmpty(operation) ? string.Empty : $"{operation} ";

			if (string.IsNullOrEmpty(component.MeasurementCode) && !hasOverrideUnit)
			{
				if (hasCurrency) //Case when maximum or min amount must be fixed value or Flat rate
				{
					result = $"{operation} {GetAmountText(component.Amount, makeItReadable, true)}";
					result += makeItReadable ? string.Empty : " * [FLAT]";
				}
				else //Case of VFD
				{
					if (makeItReadable)
					{
						priceCode = priceCode == CIF ? plainTextCifPrice : string.Empty;
					}
					result = makeItReadable ? $"{GetAmountText(component.Amount, true, false)} {priceCode} %" : $"{ph}{op}{priceCode} {opMultiply} {GetAmountText(component.Amount / 100.0m, false, false)}";
				}
			}
			else //Case of calculation based on supp units
			{
				var amountText = GetAmountText(component.Amount, makeItReadable, true);

				var dutyValueExpression = string.Empty;
				if (hasDutyAmount)
				{
					dutyValueExpression = makeItReadable ? $"{amountText}/" : $"{amountText} {opMultiply} ";
				}

				var unit = unitOverride;
				if (!hasOverrideUnit)
				{
					unit = makeItReadable ? $"{component.MeasurementCodeDescription} {component.QualifierDescription}" : $"[{component.MeasurementCode}{component.Qualifier}]";
				}

				result = $"{ph}{op}{dutyValueExpression}{unit}";
			}

			return result;
		}

		static string GetFunction(string dutyExpressionId)
		{
			switch (dutyExpressionId)
			{
				case "15": //Minimum
					return funcMax;
				case "17": //Maximum
				case "35": //Maximum
					return funcMin;

				default:
					return string.Empty;
			}
		}

		static string GetConditionActionCodeDescription(string actionCode)
		{
			switch (actionCode)
			{
				case "01":
				case "02":
				case "03":
				case "11":
				case "12":
				case "13":
				case "15":
				case "27":
					return "le montant à percevoir est ";
				case "04":
					return "la mise en libre pratique n’est pas admise";
				case "05":
					return "l'exportation n'est pas autorisée";
				case "06":
					return "l'importation n'est pas autorisée";
				case "07":
					return "la mesure n'est pas applicable";
				case "08":
					return "la position déclarée n’est pas permise";
				case "09":
					return "l'importation/exportation est non autorisée après controle";
				case "14":
					return "l’exemption/réduction du droit antidumping ne s’applique pas";
				case "16":
					return "la restitution à l’exportation n'est pas applicable";
				case "24":
					return "la mise en libre pratique est autorisée";
				case "25":
					return "l'exportation autorisée";
				case "26":
					return "l'importation autorisée";
				case "28":
					return "la sous-position déclarée autorisée";
				case "29":
					return "l'mportation/exportation autorisée après controle";
				case "34":
					return "la réduction du droit antidumping est";
				case "36":
					return "la restitution à l’exportation";
				case "MI":
					return "la mise à la consommation est interdite";
				case "PH":
					return "prohibition";
				default:
					return string.Empty;
			}
		}

		static bool IsFallbackAction(string actionCode) => FallbackActions.Contains(actionCode);

		static bool IsFallbackValue(string value) => value == "0";

		static string[] FallbackActions = new string[] { "04", "05", "06", "07", "08", "09", "14", "16", "24", "25", "26", "28", "29", "MI", "PH" };

		static string GetAmountText(decimal? amount, bool makeItReadable, bool useCurrency)
		{
			var amountText = amount?.ToString("0.#######", CultureInfo.InvariantCulture) ?? "0";
			if (makeItReadable)
			{
				amountText = amountText.Replace(".", ",");
				if (useCurrency)
				{
					amountText = amountText + " EUR";
				}
			}
			return amountText;
		}

		static string RevertOperationIfNecessary(string actionCode, string op)
		{
			if (IsNegativeAction(actionCode))
			{
				op = ReverseOperation(op);
			}

			return op;
		}

		static bool IsNegativeAction(string actionCode)
		{
			switch (actionCode)
			{
				case "04":  //The entry into free circulation is not allowed
				case "05":  //Export is not allowed
				case "06":  //Import is not allowed
				case "07":  //Measure not applicable
				case "08":  //Declared subheading not allowed
				case "09":  //Import/export not allowed after control
				case "10":  //Declaration to be corrected  - box 33, 37, 38, 41 or 46 incorrect
				case "14":  //The exemption/reduction of the anti-dumping duty is not applicable
				case "16":  //Export refund not applicable
					return true;

				default:
					return false;
			}
		}

		static string ReverseOperation(string op)
		{
			switch (op)
			{
				case ">":
					return "<=";
				case ">=":
					return "<";
				case "<":
					return ">=";
				case "<=":
					return ">";

				default:
					return string.Empty;
			}
		}

		const string opMultiply = "*";
		const string opAdd = "+";
		const string opSubtract = "-";
		const string funcMax = "MAX";
		const string funcMin = "MIN";
		const string plainTextMaximum = "Maximum :";
		const string plainTextMinimum = "Minimum :";
		const string funcIf = "If";
		const string funcHas = "has";
		const string VFD = "VFD";
		const string CIF = "CIF";
		const string plainTextCifPrice = "du prix CIF";
		public const string DefaultSupplementaryUnit = "NAR";
		public const string SupplementaryUnitPlaceHolder = "SUPU";
		const string AgricultureComponent = "#EA(1)#";
		const string ReducedAgricultureComponent = "#EAR(1)#";
		const string AdditionalDutyOnSugar = "#ADSZ(1)#";
		const string ReducedAdditionalDutyOnSugar = "#ADSZR(1)#";
		const string AdditionalDutyOnFlour = "#ADFM(1)#";
		const string ReducedAdditionalDutyOnFlour = "#ADFMR(1)#";

		const string placeholderPrefix = "PH:";
		const string placeholder1 = placeholderPrefix + "1";
		const string placeholder2 = placeholderPrefix + "2";

		static class DutyExpressionIds
		{
			public const string AgricultureComponent = "12";
			public const string ReducedAgricultureComponent = "14";
			public const string AdditionalDutyOnSugar = "21";
			public const string ReducedAdditionalDutyOnSugar = "25";
			public const string AdditionalDutyOnFlour = "27";
			public const string ReducedAdditionalDutyOnFlour = "29";
		}

		public static bool IsLookingIncorrect(string formula)
		{
			var result = false;

			if (formula != "0" && !formula.Contains("{\"Precalcule\"}") && formula.Split(new char[] { ' ' }).Length < 3)
			{
				return true;
			}

			if (formula.Contains(",)") || formula.Contains("(,"))
			{
				return true;
			}

			if (formula.Count(x => x == '[') != formula.Count(x => x == ']')
				|| formula.Count(x => x == '{') != formula.Count(x => x == '}')
				|| formula.Count(x => x == '(') != formula.Count(x => x == ')')
				|| formula.Count(x => x == '"') % 2 != 0)
			{
				return true;
			}

			return result;
		}
		#endregion

		#region Measure Class

		public static class MeasureClass
		{
			public const string Other = "OTHER";
			public const string Special = "SPECIAL";
			public const string Rate = "RATE";
			public const string Control = "CTRL";
			public const string Vat = "VAT";
			public const string RedevanceAndFees = "FEES";
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Maintainability", "CA1502:Avoid excessive complexity", Justification = "Just switch Case with lot of case.")]
		public static string GetMeasureClass(string measureType)
		{
			switch (measureType)
			{
				case "277":
				case "278":
				case "410":
				case "420":
				case "464":
				case "465":
				case "467":
				case "473":
				case "474":
				case "475":
				case "476":
				case "477":
				case "478":
				case "479":
				case "481":
				case "485":
				case "705":
				case "706":
				case "707":
				case "708":
				case "709":
				case "710":
				case "711":
				case "712":
				case "713":
				case "714":
				case "715":
				case "716":
				case "717":
				case "718":
				case "719":
				case "722":
				case "724":
				case "725":
				case "728":
				case "730":
				case "735":
				case "740":
				case "745":
				case "746":
				case "747":
				case "748":
				case "749":
				case "750":
				case "751":
				case "755":
				case "760":
				case "761":
				case "766":
				case "770":
				case "771":
				case "772":
				case "773":
				case "774":
				case "AAN":
				case "ADE":
				case "ADI":
				case "AEX":
				case "AMB":
				case "AQE":
				case "AQI":
				case "ARD":
				case "BDU":
				case "BIO":
				case "CEA":
				case "CIA":
				case "COV":
				case "CWE":
				case "CWI":
				case "CZO":
				case "DIS":
				case "LAC":
				case "PAC":
				case "PBP":
				case "PCA":
				case "PCB":
				case "PCC":
				case "PCD":
				case "PCE":
				case "PCF":
				case "PCG":
				case "PCI":
				case "PCJ":
				case "PCK":
				case "PCL":
				case "PCM":
				case "PCN":
				case "PCR":
				case "PCS":
				case "PCT":
				case "PCU":
				case "PCV":
				case "PCW":
				case "PCX":
				case "PCY":
				case "PCZ":
				case "PDM":
				case "PHY":
				case "PPH":
				case "PRE":
				case "PRI":
				case "STE":
				case "STI":
					return MeasureClass.Control;
				case "109":
				case "110":
				case "111":
				case "440":
				case "445":
				case "450":
				case "460":
				case "461":
				case "462":
				case "463":
				case "470":
				case "471":
				case "SEP":
				case "SIP":
				case "USU":
				case "USI":
					return MeasureClass.Other;
				case "103":
				case "105":
				case "106":
				case "112":
				case "115":
				case "117":
				case "119":
				case "122":
				case "123":
				case "140":
				case "141":
				case "142":
				case "143":
				case "144":
				case "145":
				case "146":
				case "147":
				case "488":
				case "489":
				case "490":
				case "491":
				case "492":
				case "493":
				case "494":
				case "495":
				case "496":
				case "551":
				case "552":
				case "553":
				case "554":
				case "555":
				case "561":
				case "562":
				case "564":
				case "565":
				case "566":
				case "570":
				case "651":
				case "652":
				case "653":
				case "654":
				case "655":
				case "656":
				case "657":
				case "658":
				case "672":
				case "673":
				case "674":
				case "680":
				case "681":
				case "683":
				case "684":
				case "685":
				case "686":
				case "687":
				case "688":
				case "690":
				case "695":
				case "696":
				case "ALP":
				case "AMC":
				case "BNA":
				case "CBE":
				case "CBS":
				case "CMP":
				case "CSS":
				case "OEA":
				case "OEB":
				case "OFI":
				case "ORA":
				case "ORB":
				case "PMX":
				case "RCA":
				case "RMA":
				case "ROC":
				case "RPH":
				case "SOU":
				case "TBO":
				case "TCG":
				case "TDA":
				case "TDB":
				case "TDC":
				case "TDF":
				case "TDH":
				case "TEX":
				case "TIC":
				case "TIP":
				case "TLP":
				case "TMC":
				case "TMP":
				case "TPC":
				case "TPP":
				case "TSC":
				case "TVF":
					return MeasureClass.Rate;
				case "RCP":
				case "RCR":
				case "RSD":
				case "RVT":
				case "TIM":
					return MeasureClass.RedevanceAndFees;
				case "482":
				case "483":
				case "484":
					return MeasureClass.Special;
				case "TVA":
				case "TVB":
					return MeasureClass.Vat;
				default:
					return string.Empty;
			}
		}

		#endregion

		#region Misc

		public static void SplitRegionWhereNecessary(string originalRegion, out string region1, out string region2)
		{
			region1 = "";
			region2 = "";
			switch (originalRegion) //Application territory of type "METRO" should be splitted into CONTI and CORSE
			{
				case "METRO":
					region1 = "CONTI";
					region2 = "CORSE";
					break;
				default:
					region1 = originalRegion;
					break;
			}
		}

		#endregion
	}
}
