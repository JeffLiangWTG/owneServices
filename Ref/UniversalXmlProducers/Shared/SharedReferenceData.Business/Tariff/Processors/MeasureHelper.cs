using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;
using CargoWise.RefDbRepo.SharedReferenceData.Services.Tariff.Models;

namespace CargoWise.RefDbRepo.SharedReferenceData.Business.Tariff.Processors
{
	public class MeasureHelper
	{
		#region Generate Formula from Components and Conditions
		public string GenerateFormula(Measure m, StringBuilder errorCollector)
		{
			PreprocessMeasure(m);
			var shouldExist = ((m.Components?.Any() ?? false) || (m.Conditions?.Any() ?? false));

			var units = GetAllUnits(m);

			var componentFormula = BuildFormulaFromComponents(m.Components);
			var conditionFormula = string.Empty;

			if (m.Conditions?.Any() ?? false)
			{
				foreach (var con in m.Conditions.OrderBy(x => x.ConditionCode).ThenBy(x => x.SequenceNumber))
				{
					var conditionOp = GetConditionOperation(con);

					if (con.IsRateFormulaCondition)
					{
						var conComFormula = BuildFormulaFromComponents(con.Components, con.MeasureAction);

						if (string.IsNullOrEmpty(conditionFormula))
						{
							if (!string.IsNullOrEmpty(conditionOp))
							{
								conditionFormula = conditionOp.Replace(ThenPart, AddMissingUnits(conComFormula, units));
							}
							else if (conComFormula != "0")
							{
								conditionFormula = conComFormula;
							}
						}
						else
						{
							if (m.Conditions.Any(x => x.SequenceNumber > con.SequenceNumber))
							{
								conditionFormula = conditionFormula.Replace(ElsePart, $"{conditionOp.Replace(ThenPart, AddMissingUnits(conComFormula, units))}");
							}
							else
							{
								conditionFormula = conditionFormula.Replace(ElsePart, AddMissingUnits(conComFormula, units));
							}
						}
					}
					else if (m.ConditionClass == ConditionClass.Class)
					{
						if (!string.IsNullOrEmpty(conditionOp))
						{
							conditionFormula = string.IsNullOrEmpty(conditionFormula) ? conditionOp : $"{conditionFormula} & {conditionOp}";
						}
					}
				}
				if (conditionFormula.Contains(ElsePart))
				{
					errorCollector.Append(CultureInfo.InvariantCulture, $"{{InvalidSourceData:Condition for measure HJID={m.HJID} has no else part: {conditionFormula}}}");
					conditionFormula = conditionFormula.Replace(ElsePart, AddMissingUnits("0", units));
				}
			}

			var formula = $"{componentFormula} {conditionFormula}".Trim();
			if (shouldExist && string.IsNullOrEmpty(formula) && m.ConditionClass == ConditionClass.Rate)
			{
				formula = "0";
			}

			return formula;
		}

		protected virtual void PreprocessMeasure(Measure m) { }

		static List<string> GetAllUnits(Measure m)
		{
			var units = new List<string>();

			if (m.Components?.Any() ?? false)
			{
				units.AddRange(m.Components.Select(x => $"{x.MeasurementUnit}{x.MeasurementUnitQualifier}"));
			}

			if (m.Conditions?.Any() ?? false)
			{
				units.AddRange(m.Conditions.Select(x => $"{x.MeasurementUnit}{x.MeasurementUnitQualifier}"));
				units.AddRange(m.Conditions.SelectMany(x => x.Components.Select(c => $"{c.MeasurementUnit}{c.MeasurementUnitQualifier}")));
			}

			return units.Distinct().Where(x => !string.IsNullOrEmpty(x)).ToList();
		}

		static string AddMissingUnits(string formula, List<string> units)
		{
			if (units.Any())
			{
				foreach (var unit in units)
				{
					var bracketedUnit = $"[{unit}]";
					if (!formula.Contains(bracketedUnit))
					{
						if (formula == "0" || string.IsNullOrEmpty(formula))
						{
							formula = $"0 * {bracketedUnit}";
						}
						else
						{
							formula = $"{formula} + 0 * {bracketedUnit}";
						}
					}
				}
			}

			return formula;
		}

		public static string GenerateConditionFormula(Measure m, MeasureCondition c)
		{
			var formula = string.Empty;

			if (m.ConditionClass == ConditionClass.Control && string.IsNullOrEmpty(c.CertificateCode) && c.DutyAmount.HasValue)
			{
				formula = $"[{c.MeasurementUnit}{c.MeasurementUnitQualifier}] <= {c.DutyAmount:0.000####}";
			}

			return formula;
		}

		public static bool IsRateFormula(IEnumerable<MeasureCondition> conditions, string conditionCode)
		{
			var rateActions = new[] { "01", "02", "03", "11", "12", "13", "15" }; // Other potentials - Requires further investigation 27, 34

			return conditions.Any(x => x.ConditionCode == conditionCode && rateActions.Contains(x.MeasureAction));
		}

		public static bool IsCertificateCondition(string conditionCode)
		{
			var certificateCodes = new[] { "A", "B", "C", "H", "Q", "Z" };

			return certificateCodes.Contains(conditionCode);
		}

		static string BuildFormulaFromComponents(IEnumerable<MeasureComponent> components, string actionCode = "")
		{
			var componentFormula = string.Empty;

			if (components?.Any() ?? false)
			{
				var parts = new List<string>();
				foreach (var comp in components.OrderBy(x => x.DutyExpression))
				{
					var func = GetFunction(comp.DutyExpression);

					if (string.IsNullOrEmpty(actionCode))
					{
						parts.Add(GetDutyExpression(comp));
					}
					else
					{
						parts.Add(GetActionExpression(comp, actionCode));
					}

					if (!string.IsNullOrEmpty(func))
					{
						var part = $"{func}({placeholder1}, {placeholder2})";
						parts.Insert(0, part);
					}
				}

				componentFormula = ComposeParts(parts);
			}

			return componentFormula;
		}

		static string ComposeParts(List<string> parts)
		{
			parts.RemoveAll(x => string.IsNullOrEmpty(x));
			var complete = parts.Where(x => !x.Contains(placeholderPrefix)).ToList();
			parts.RemoveAll(x => !x.Contains(placeholderPrefix));
			var funcs = parts.Where(x => x.Contains(placeholder2)).ToList();
			parts.RemoveAll(x => x.Contains(placeholder2));

			bool subFound = true;
			int i = 0;
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

			return string.Join("; ", complete).Trim(); // Should only be 1 but kept like this for debugging & testing
		}

		static string GetDutyExpression(MeasureComponent mc)
		{
			switch (mc.DutyExpression)
			{
				case "01": //% or amount
					return DutyExpression(mc, isRequired: true, hasPlaceholder: false);
				case "02": //minus % or amount
					return DutyExpression(mc, opSubtract);
				case "04": //+ % or amount
				case "19": //+ % or amount
				case "20": //+ % or amount
					return DutyExpression(mc, opAdd);

				case DutyExpressionIds.AgricultureComponent: //+ agricultural component
					return DutyExpression(mc, opAdd, unitOverride: AgricultureComponent);
				case DutyExpressionIds.ReducedAgricultureComponent: //+ reduced agricultural component
					return DutyExpression(mc, opAdd, unitOverride: ReducedAgricultureComponent);
				case DutyExpressionIds.AdditionalDutyOnSugar: //+ additional duty on sugar
					return DutyExpression(mc, opAdd, unitOverride: AdditionalDutyOnSugar);
				case DutyExpressionIds.ReducedAdditionalDutyOnSugar: //+ reduced additional duty on sugar
					return DutyExpression(mc, opAdd, unitOverride: ReducedAdditionalDutyOnSugar);
				case DutyExpressionIds.AdditionalDutyOnFlour: //+ additional duty on flour
					return DutyExpression(mc, opAdd, unitOverride: AdditionalDutyOnFlour);
				case DutyExpressionIds.ReducedAdditionalDutyOnFlour: //+ reduced additional duty on flour
					return DutyExpression(mc, opAdd, unitOverride: ReducedAdditionalDutyOnFlour);

				case "15": //Minimum
				case "17": //Maximum
				case "35": //Maximum
					return DutyExpression(mc, hasPlaceholder: false);
				case "36": //minus % CIF
					return DutyExpression(mc, opSubtract, priceCode: CIF);

				case "37": //(nothing)
				case "99": //Supplementary unit

				default:
					return string.Empty;
			}
		}

		static string GetActionExpression(MeasureComponent mc, string actionCode)
		{
			switch (actionCode)
			{
				case "01":  //Apply the amount of the action (see components)
					return GetDutyExpression(mc);
				case "02":  //Apply the difference between the amount of the action (see components) and the price at import
				case "11":  //Apply the difference between the amount of the action (see components) and the free at frontier price before duty
				case "15": //Apply the difference between the amount of the action (see components) and the price augmented with the countervailing duty (3,8%)
					return ApplyDifference(mc, VFD);
				case "03":  //Apply the difference between the amount of the action (see components) and CIF price
				case "12":  //Apply the difference between the amount of the action (see components) and the CIF price before duty
				case "13":  //Apply the difference between the amount of the action (see components) and the CIF price augmented with the duty to be paid per tonne
					return ApplyDifference(mc, CIF);

				default:
					return string.Empty;
			}
		}

		static string ApplyDifference(MeasureComponent mc, string priceCode)
		{
			var unit = $"[{mc.MeasurementUnit}{mc.MeasurementUnitQualifier}]";
			return $"({mc.DutyAmount:0.000####} - {priceCode}/{unit}) * {unit}";
		}

		static string DutyExpression(MeasureComponent mc, string operation = "", bool isRequired = false, string unitOverride = "", bool hasPlaceholder = true, string priceCode = VFD)
		{
			var hasOverrideUnit = !string.IsNullOrEmpty(unitOverride);
			var noUnit = string.IsNullOrEmpty(mc.MeasurementUnit) && !hasOverrideUnit;
			var hasDutyAmount = mc.DutyAmount.HasValue && (mc.DutyAmount > 0 || !noUnit);

			if (!hasDutyAmount && !hasOverrideUnit)
			{
				return isRequired ? "0" : string.Empty;
			}

			var ph = hasPlaceholder ? $"{placeholder1} " : string.Empty;
			var op = string.IsNullOrEmpty(operation) ? string.Empty : $"{operation} ";

			if (noUnit)
			{
				return $"{ph}{op}{priceCode} {opMulti} {mc.DutyAmount / 100.0m:0.000####}";
			}
			else
			{
				var dutyValueExpression = hasDutyAmount ? $"{mc.DutyAmount:0.000####} {opMulti} " : string.Empty;
				var unit = hasOverrideUnit ? unitOverride : $"[{mc.MeasurementUnit}{mc.MeasurementUnitQualifier}]";

				return $"{ph}{op}{dutyValueExpression}{unit}";
			}
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

		static string GetConditionOperation(MeasureCondition mc)
		{
			var result = string.Empty;
			var op = string.Empty;
			var valType = VFD;
			bool useCert = false;
			bool isConditionExpression = false;

			switch (mc.ConditionCode)
			{
				case "L":   //CIF price must be higher than the minimum price (see components)
					valType = CIF;
					op = ">=";
					break;
				case "M":   //Declared price must be equal to or greater than the minimum price/reference price (see components)
				case "F":   //The net free at frontier price before duty must be equal to or greater than the minimum price (see components)
				case "V":   //Import price must be equal to or greater than the entry price (see components)
					op = ">=";
					break;

				case "A": //Presentation of an anti-dumping/countervailing document
				case "B": //Presentation of a certificate/licence/document
				case "C": //Presentation of a certificate/licence/document
				case "H": //Presentation of a certificate/licence/document
				case "Q": //Presentation of an endorsed certificate/licence
				case "Z": //Presentation of more than one certificate
					useCert = true;
					op = $"{mc.CertificateTypeCode}{mc.CertificateCode}";
					break;

				case "E":   //The quantity or the price per unit declared, as appropriate, is equal or less than the specified maximum, or presentation of the required document
				case "I":   //The quantity or the price per unit declared, as appropriate, is equal or less than the specified maximum, or presentation of the required document
					op = "<=";
					valType = string.Empty;
					break;

				// Not used as yet, may need to be included after further investigation
				//case "G":   //The CIF price plus the duty to be paid/ton must be equal to or greater than the minimum price (see components)
				//case "N":   //The CIF price before duty must be equal to or greater than the minimum price(see components)
				case "R":   //Ratio "net weight/supplementary unit" is equal to or higher than the condition amount
				case "U":   //Ratio "declared value/supplementary unit" should be higher than the condition amount
					isConditionExpression = true;
					op = ">=";
					valType = SupplementaryUnitPlaceHolder;
					break;
			}

			if (!string.IsNullOrEmpty(op))
			{
				if (!isConditionExpression)
				{
					string condition;
					if (useCert)
					{
						condition = $"{funcHas}(\"CERT\", \"{op}\")";
					}
					else
					{
						var valExp = string.Empty;
						if (!string.IsNullOrEmpty(valType))
						{
							valExp = $"{valType}/";
						}
						var valueExpr = $"{valExp}[{mc.MeasurementUnit}{mc.MeasurementUnitQualifier}]";

						condition = $"{valueExpr} {op} {mc.DutyAmount:0.000####}";
					}

					result = $"{funcIf}({condition}, {ThenPart}, {ElsePart})";
				}
				else
				{
					if (IsNegativeAction(mc.MeasureAction))
					{
						op = ReverseOperation(op);
					}

					if (!(op.Contains("<") && mc.DutyAmount == 0))
					{
						result = $"[{mc.MeasurementUnit}{mc.MeasurementUnitQualifier}]/[{valType}] {op} {mc.DutyAmount:0.000####}";
					}
				}
			}

			return result;
		}

		public static bool IsNegativeAction(string actionCode)
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

		const string opMulti = "*";
		const string opAdd = "+";
		const string opSubtract = "-";
		const string funcMax = "MAX";
		const string funcMin = "MIN";
		const string funcIf = "If";
		const string funcHas = "has";

		const string VFD = "VFD";
		const string CIF = "CIF";
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
		const string ThenPart = "THENPART";
		const string ElsePart = "ELSEPART";

		public static class ConditionClass
		{
			public const string Class = "CLASS";
			public const string Rate = "RATE";
			public const string Control = "CTRL";
			public const string Vat = "VAT";
		}
		#endregion

		public static IEnumerable<string> GetTariffTypes(string tradeMovementCode)
		{
			var result = new List<string>();

			switch (tradeMovementCode)
			{
				case "0": // Import
					result.Add(TariffTypeImport);
					break;

				case "1": // Export
					result.Add(TariffTypeExport);
					break;

				case "2": // Import & Export
					result.Add(TariffTypeImport);
					result.Add(TariffTypeExport);
					break;
			}

			return result;
		}

		public const string TariffTypeImport = "IMP";
		public const string TariffTypeExport = "EXP";

		public static string GetDutyRateCode(Measure measure)
		{
			var rateCode = RateCodes.Default;

			var dtyExpressions = measure.Components?.Select(x => x.DutyExpression)?.ToList() ?? new List<string>();
			dtyExpressions.AddRange(measure.Conditions?.SelectMany(x => x.Components?.Select(y => y.DutyExpression) ?? new List<string>()) ?? new List<string>());

			if (dtyExpressions.Any(x => !string.IsNullOrEmpty(x) && int.Parse(x, CultureInfo.CurrentCulture) >= 12))
			{
				if (dtyExpressions.Contains(DutyExpressionIds.AgricultureComponent))
				{
					rateCode = RateCodes.AgriculturalComponent;
				}
				else if (dtyExpressions.Contains(DutyExpressionIds.ReducedAgricultureComponent))
				{
					rateCode = RateCodes.ReducedAgricultureComponent;
				}
				else if (dtyExpressions.Contains(DutyExpressionIds.AdditionalDutyOnSugar))
				{
					rateCode = RateCodes.AdditionalDutyOnSugar;
				}
				else if (dtyExpressions.Contains(DutyExpressionIds.ReducedAdditionalDutyOnSugar))
				{
					rateCode = RateCodes.ReducedAdditionalDutyOnSugar;
				}
				else if (dtyExpressions.Contains(DutyExpressionIds.AdditionalDutyOnFlour))
				{
					rateCode = RateCodes.AdditionalDutyOnFlour;
				}
				else if (dtyExpressions.Contains(DutyExpressionIds.ReducedAdditionalDutyOnFlour))
				{
					rateCode = RateCodes.ReducedAdditionalDutyOnFlour;
				}
			}

			return rateCode;
		}

		public static class RateCodes
		{
			public const string Default = "A00";
			public const string AgriculturalComponent = "EA";
			public const string ReducedAgricultureComponent = "EAR";
			public const string AdditionalDutyOnSugar = "ADSZ";
			public const string ReducedAdditionalDutyOnSugar = "ADSZR";
			public const string AdditionalDutyOnFlour = "ADFM";
			public const string ReducedAdditionalDutyOnFlour = "ADFMR";
			public const string ExciseDefault = "306";
		}

		static class DutyExpressionIds
		{
			public const string AgricultureComponent = "12";
			public const string ReducedAgricultureComponent = "14";
			public const string AdditionalDutyOnSugar = "21";
			public const string ReducedAdditionalDutyOnSugar = "25";
			public const string AdditionalDutyOnFlour = "27";
			public const string ReducedAdditionalDutyOnFlour = "29";
		}
	}
}
